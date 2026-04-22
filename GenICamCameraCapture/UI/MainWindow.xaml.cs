using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GenICamCameraCapture.Camera;
using GenICamCameraCapture.Providers;
using Microsoft.Win32;

namespace GenICamCameraCapture.UI;

public partial class MainWindow : Window
{
    private CameraManager? _manager;
    private CameraDevice? _camera;
    private ICameraProvider? _currentProvider;
    private List<DeviceItem> _deviceItems = new();

    private int _frameCount;
    private int _savedCount;
    private DateTime _lastFpsTime = DateTime.Now;
    private int _lastFpsFrameCount;
    private readonly DispatcherTimer _uiTimer;
    private WriteableBitmap? _bitmap;
    private readonly object _frameLock = new();

    // 线扫拼接缓冲
    private const int LineScanAccumulateRows = 512;
    private byte[]? _lineScanBuffer;
    private int _lineScanWidth;
    private int _lineScanBytesPerPixel;
    private int _lineScanCurrentRow;
    private bool _lineScanDirty;

    // 面阵模式帧
    private FrameData? _latestAreaFrame;
    // 避免在采集回调中 Dispatcher.Invoke 读 checkbox（会死锁）
    private volatile bool _autoSaveEnabled;

    public MainWindow()
    {
        InitializeComponent();

        _uiTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _uiTimer.Tick += UiTimer_Tick;
        _uiTimer.Start();

        cmbProvider.SelectionChanged += CmbProvider_SelectionChanged;

        AppendLog("GenICam/GenTL 统一相机采集系统 已启动");
    }

    private void CmbProvider_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        string tag = (cmbProvider.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? "huaray";
        txtCtiPath.Text = tag switch
        {
            "hikvision" => Path.Combine(AppContext.BaseDirectory, @"GenTL\Runtime\MVProducerGEV.cti"),
            _ => Path.Combine(AppContext.BaseDirectory, @"GenTL\Runtime\MVProducerCXP.cti")
        };

        // 切换厂商后重置状态，必须重新初始化 CTI
        _currentProvider = null;
        _camera = null;
        _deviceItems.Clear();
        lstDevices.Items.Clear();
        btnInitialize.IsEnabled = true;
        btnEnumerate.IsEnabled = false;
        btnConnect.IsEnabled = false;
        btnDisconnect.IsEnabled = false;
        btnStartGrab.IsEnabled = false;
        btnStopGrab.IsEnabled = false;
        btnSetExposure.IsEnabled = false;
        btnGetExposure.IsEnabled = false;
        AppendLog($"已切换厂商: {(tag == "hikvision" ? "海康" : "华睿")}，请重新初始化 CTI");
    }

    #region 日志

    private void AppendLog(string msg)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(() => AppendLog(msg));
            return;
        }
        string time = DateTime.Now.ToString("HH:mm:ss.fff");
        txtLog.AppendText($"[{time}] {msg}\n");
        txtLog.ScrollToEnd();
    }

    private void BtnClearLog_Click(object sender, RoutedEventArgs e)
    {
        txtLog.Clear();
    }

    #endregion

    #region CTI 文件

    private void BtnBrowseCti_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "选择 CTI 文件",
            Filter = "GenTL Producer (*.cti)|*.cti|所有文件 (*.*)|*.*",
            InitialDirectory = System.IO.Path.GetDirectoryName(txtCtiPath.Text) ?? @"C:\"
        };
        if (dlg.ShowDialog() == true)
        {
            txtCtiPath.Text = dlg.FileName;
        }
    }

    #endregion

    #region 初始化与枚举

    private void BtnInitialize_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string ctiPath = txtCtiPath.Text.Trim();
            if (!System.IO.File.Exists(ctiPath))
            {
                MessageBox.Show($"CTI 文件不存在:\n{ctiPath}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _manager?.Dispose();
            _manager = new CameraManager();

            // 根据选择的厂商创建 Provider
            string providerTag = (cmbProvider.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? "huaray";
            if (providerTag == "hikvision")
                _currentProvider = new HikvisionProvider(ctiPath);
            else
                _currentProvider = new HuarayProvider(ctiPath);

            _manager.RegisterProvider(_currentProvider);
            _manager.InitializeAll();

            AppendLog("CTI 初始化成功");
            txtStatus.Text = "已初始化";
            txtStatus.Foreground = new SolidColorBrush(Colors.DarkOrange);

            btnEnumerate.IsEnabled = true;
            btnInitialize.IsEnabled = false;
        }
        catch (Exception ex)
        {
            AppendLog($"初始化失败: {ex.Message}");
            MessageBox.Show(ex.Message, "初始化失败", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnEnumerate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_manager == null) return;

            var allDevices = _manager.EnumerateAllDevices();
            _deviceItems.Clear();
            lstDevices.Items.Clear();

            foreach (var (provider, info) in allDevices)
            {
                var item = new DeviceItem
                {
                    Provider = provider,
                    Info = info,
                    DisplayText = $"{info.Model} (SN:{info.SerialNumber})"
                };
                _deviceItems.Add(item);
                lstDevices.Items.Add(item);
            }

            AppendLog($"枚举完成，找到 {allDevices.Count} 个设备");

            if (allDevices.Count > 0)
            {
                lstDevices.SelectedIndex = 0;
                btnConnect.IsEnabled = true;
            }
            else
            {
                btnConnect.IsEnabled = false;
                AppendLog("未找到设备，请检查相机连接");
            }
        }
        catch (Exception ex)
        {
            AppendLog($"枚举失败: {ex.Message}");
        }
    }

    #endregion

    #region 连接/断开

    private void BtnConnect_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_manager == null || lstDevices.SelectedItem is not DeviceItem selected) return;

            _camera = _manager.OpenDevice(selected.Provider, selected.Info);
            _camera.OnLog += AppendLog;
            _camera.OnFrameReceived += OnFrameReceived;

            AppendLog($"已连接: {selected.Info.Model} (SN:{selected.Info.SerialNumber})");
            txtStatus.Text = "已连接";
            txtStatus.Foreground = new SolidColorBrush(Colors.Green);

            btnConnect.IsEnabled = false;
            btnDisconnect.IsEnabled = true;
            btnStartGrab.IsEnabled = true;
            btnEnumerate.IsEnabled = false;

            // 解析 XML 后启用曝光控制
            try
            {
                _camera.ParseXmlRegisters();
                btnSetExposure.IsEnabled = _camera.HasExposureControl;
                btnGetExposure.IsEnabled = _camera.HasExposureControl;
                if (_camera.HasExposureControl)
                {
                    double cur = _camera.GetExposureTime();
                    if (cur > 0) txtExposureTime.Text = cur.ToString("F0");
                    AppendLog($"当前曝光时间: {cur} μs");
                }
            }
            catch (Exception ex2)
            {
                AppendLog($"读取曝光参数失败: {ex2.Message}");
            }
        }
        catch (Exception ex)
        {
            AppendLog($"连接失败: {ex.Message}");
            MessageBox.Show(ex.Message, "连接失败", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_camera != null)
            {
                if (_camera.IsGrabbing)
                    StopGrabbing();

                _camera.OnFrameReceived -= OnFrameReceived;
                _camera.OnLog -= AppendLog;
                _camera.Dispose();
                _camera = null;
            }

            AppendLog("已断开设备");
            txtStatus.Text = "已断开";
            txtStatus.Foreground = new SolidColorBrush(Colors.Gray);

            btnConnect.IsEnabled = true;
            btnDisconnect.IsEnabled = false;
            btnStartGrab.IsEnabled = false;
            btnStopGrab.IsEnabled = false;
            btnEnumerate.IsEnabled = true;
            btnSetExposure.IsEnabled = false;
            btnGetExposure.IsEnabled = false;
        }
        catch (Exception ex)
        {
            AppendLog($"断开失败: {ex.Message}");
        }
    }

    #endregion

    #region 采集控制

    private void BtnStartGrab_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_camera == null) return;

            _frameCount = 0;
            _savedCount = 0;
            _lastFpsTime = DateTime.Now;
            _lastFpsFrameCount = 0;

            // 清空线扫拼接缓冲
            lock (_frameLock)
            {
                _lineScanBuffer = null;
                _lineScanCurrentRow = 0;
                _lineScanDirty = false;
            }

            int bufferCount = 16;
            if (int.TryParse(txtBufferCount.Text, out int bc) && bc > 0 && bc <= 100)
                bufferCount = bc;

            _camera.StartGrab(bufferCount);

            txtStatus.Text = "采集中...";
            txtStatus.Foreground = new SolidColorBrush(Colors.Green);
            btnStartGrab.IsEnabled = false;
            btnStopGrab.IsEnabled = true;
            btnDisconnect.IsEnabled = false;
        }
        catch (Exception ex)
        {
            AppendLog($"启动采集失败: {ex.Message}");
            MessageBox.Show(ex.Message, "采集失败", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnStopGrab_Click(object sender, RoutedEventArgs e)
    {
        StopGrabbing();
    }

    private void StopGrabbing()
    {
        try
        {
            _camera?.StopGrab();

            txtStatus.Text = "已停止";
            txtStatus.Foreground = new SolidColorBrush(Colors.DarkOrange);
            btnStartGrab.IsEnabled = true;
            btnStopGrab.IsEnabled = false;
            btnDisconnect.IsEnabled = true;

            AppendLog($"采集完成，共 {_frameCount} 帧");
        }
        catch (Exception ex)
        {
            AppendLog($"停止采集失败: {ex.Message}");
        }
    }

    #endregion

    #region 曝光控制

    private void BtnSetExposure_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_camera == null) return;
            if (!double.TryParse(txtExposureTime.Text, out double us) || us <= 0)
            {
                MessageBox.Show("请输入有效的曝光时间（正数，单位微秒）", "提示");
                return;
            }
            _camera.SetExposureTime(us);
        }
        catch (Exception ex)
        {
            AppendLog($"设置曝光失败: {ex.Message}");
        }
    }

    private void BtnGetExposure_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_camera == null) return;
            double us = _camera.GetExposureTime();
            txtExposureTime.Text = us.ToString("F0");
            AppendLog($"当前曝光时间: {us} μs");
        }
        catch (Exception ex)
        {
            AppendLog($"读取曝光失败: {ex.Message}");
        }
    }

    #endregion

    #region 帧处理

    private void OnFrameReceived(FrameData frame)
    {
        Interlocked.Increment(ref _frameCount);

        bool isLineScan = frame.Height <= 16;

        if (isLineScan)
        {
            // 线扫模式：将行数据追加到拼接缓冲
            lock (_frameLock)
            {
                int bpp = (frame.Width > 0 && frame.Height > 0)
                    ? frame.Data.Length / (frame.Width * frame.Height) : 1;
                if (bpp <= 0) bpp = 1;
                int rowBytes = frame.Width * bpp;

                // 宽度或 bpp 变化时重新分配
                if (_lineScanBuffer == null || _lineScanWidth != frame.Width || _lineScanBytesPerPixel != bpp)
                {
                    _lineScanWidth = frame.Width;
                    _lineScanBytesPerPixel = bpp;
                    _lineScanBuffer = new byte[rowBytes * LineScanAccumulateRows];
                    _lineScanCurrentRow = 0;
                }

                int rowsToCopy = Math.Min(frame.Height, LineScanAccumulateRows);
                for (int r = 0; r < rowsToCopy; r++)
                {
                    int srcOffset = r * rowBytes;
                    int dstRow = _lineScanCurrentRow % LineScanAccumulateRows;
                    int dstOffset = dstRow * rowBytes;

                    if (srcOffset + rowBytes <= frame.Data.Length && dstOffset + rowBytes <= _lineScanBuffer.Length)
                        Buffer.BlockCopy(frame.Data, srcOffset, _lineScanBuffer, dstOffset, rowBytes);

                    _lineScanCurrentRow++;
                }

                _lineScanDirty = true;
            }
        }
        else
        {
            // 面阵模式：直接存整帧
            lock (_frameLock)
            {
                _lineScanBuffer = null;
            }
            Volatile.Write(ref _latestAreaFrame, frame);
        }

        // 自动保存（用 volatile field 而非 Dispatcher.Invoke，避免死锁）
        if (_autoSaveEnabled)
        {
            if (_frameCount % 10 == 0 && _savedCount < 50)
            {
                string dir = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location) ?? ".",
                    "captures");
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
                string path = System.IO.Path.Combine(dir,
                    $"frame_{frame.FrameId}_{frame.Width}x{frame.Height}.raw");
                frame.SaveRaw(path);
                Interlocked.Increment(ref _savedCount);
                AppendLog($"已保存: {path}");
            }
        }
    }

    private void UiTimer_Tick(object? sender, EventArgs e)
    {
        // 在 UI 线程安全地同步 checkbox 状态
        _autoSaveEnabled = chkAutoSave.IsChecked == true;

        bool updated = false;

        // 线扫模式：从拼接缓冲刷新图像
        lock (_frameLock)
        {
            if (_lineScanDirty && _lineScanBuffer != null && _lineScanWidth > 0)
            {
                _lineScanDirty = false;
                int totalRows = Math.Min(_lineScanCurrentRow, LineScanAccumulateRows);
                if (totalRows > 0)
                {
                    try
                    {
                        UpdateLineScanImage(_lineScanBuffer, _lineScanWidth, totalRows, _lineScanBytesPerPixel);
                        txtResolution.Text = $"{_lineScanWidth} x {totalRows} (线扫拼接)";
                        txtImagePlaceholder.Visibility = Visibility.Collapsed;
                        updated = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"线扫显示异常: {ex.Message}");
                    }
                }
            }
        }

        // 面阵模式
        if (!updated)
        {
            var frame = Interlocked.Exchange(ref _latestAreaFrame, null);
            if (frame != null && frame.Width > 0 && frame.Height > 0)
            {
                try
                {
                    UpdateAreaImage(frame);
                    txtResolution.Text = $"{frame.Width} x {frame.Height}";
                    txtPixelFormat.Text = $"0x{frame.PixelFormat:X8}";
                    txtImagePlaceholder.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"显示图像异常: {ex.Message}");
                }
            }
        }

        txtFrameCount.Text = _frameCount.ToString();

        var now = DateTime.Now;
        double elapsed = (now - _lastFpsTime).TotalSeconds;
        if (elapsed >= 1.0)
        {
            double fps = (_frameCount - _lastFpsFrameCount) / elapsed;
            txtFps.Text = $"{fps:F1} fps";
            _lastFpsTime = now;
            _lastFpsFrameCount = _frameCount;
        }
    }

    private void UpdateLineScanImage(byte[] buffer, int width, int totalRows, int bpp)
    {
        int rowBytes = width * bpp;
        bool wrapped = _lineScanCurrentRow > LineScanAccumulateRows;
        int startRow = wrapped ? (_lineScanCurrentRow % LineScanAccumulateRows) : 0;

        // 组装连续显示数据（循环缓冲 → 线性）
        byte[] displayBuf = new byte[rowBytes * totalRows];
        if (!wrapped)
        {
            Buffer.BlockCopy(buffer, 0, displayBuf, 0, rowBytes * totalRows);
        }
        else
        {
            int oldRows = totalRows - startRow;
            Buffer.BlockCopy(buffer, startRow * rowBytes, displayBuf, 0, oldRows * rowBytes);
            Buffer.BlockCopy(buffer, 0, displayBuf, oldRows * rowBytes, startRow * rowBytes);
        }

        if (bpp == 1)
        {
            EnsureBitmap(width, totalRows, PixelFormats.Gray8);
            _bitmap!.WritePixels(new Int32Rect(0, 0, width, totalRows), displayBuf, rowBytes, 0);
        }
        else if (bpp == 3)
        {
            EnsureBitmap(width, totalRows, PixelFormats.Rgb24);
            _bitmap!.WritePixels(new Int32Rect(0, 0, width, totalRows), displayBuf, rowBytes, 0);
        }
        else
        {
            byte[] gray = new byte[width * totalRows];
            for (int i = 0; i < gray.Length && i * bpp < displayBuf.Length; i++)
                gray[i] = displayBuf[i * bpp];
            EnsureBitmap(width, totalRows, PixelFormats.Gray8);
            _bitmap!.WritePixels(new Int32Rect(0, 0, width, totalRows), gray, width, 0);
        }
    }

    private void UpdateAreaImage(FrameData frame)
    {
        int w = frame.Width;
        int h = frame.Height;
        int bpp = frame.Data.Length / (w * h);
        if (bpp <= 0) bpp = 1;

        if (bpp == 1)
        {
            EnsureBitmap(w, h, PixelFormats.Gray8);
            _bitmap!.WritePixels(new Int32Rect(0, 0, w, h), frame.Data, w, 0);
        }
        else if (bpp == 3)
        {
            EnsureBitmap(w, h, PixelFormats.Rgb24);
            _bitmap!.WritePixels(new Int32Rect(0, 0, w, h), frame.Data, w * 3, 0);
        }
        else
        {
            byte[] gray = new byte[w * h];
            for (int i = 0; i < gray.Length && i * bpp < frame.Data.Length; i++)
                gray[i] = frame.Data[i * bpp];
            EnsureBitmap(w, h, PixelFormats.Gray8);
            _bitmap!.WritePixels(new Int32Rect(0, 0, w, h), gray, w, 0);
        }
    }

    private void EnsureBitmap(int w, int h, PixelFormat fmt)
    {
        if (_bitmap == null || _bitmap.PixelWidth != w || _bitmap.PixelHeight != h || _bitmap.Format != fmt)
        {
            _bitmap = new WriteableBitmap(w, h, 96, 96, fmt, null);
            imgDisplay.Source = _bitmap;
        }
    }

    #endregion

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _uiTimer.Stop();

        try
        {
            if (_camera != null)
            {
                _camera.OnFrameReceived -= OnFrameReceived;
                _camera.OnLog -= AppendLog;
                _camera.Dispose();
            }
        }
        catch { }

        try { _manager?.Dispose(); } catch { }
    }
}

public class DeviceItem
{
    public ICameraProvider Provider { get; set; } = null!;
    public CameraDeviceInfo Info { get; set; } = null!;
    public string DisplayText { get; set; } = "";
    public override string ToString() => DisplayText;
}
