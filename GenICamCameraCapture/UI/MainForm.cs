using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using GenICamCameraCapture.Camera;
using GenICamCameraCapture.Models;
using GenICamCameraCapture.Providers;

namespace GenICamCameraCapture.UI;

public partial class MainForm : Form
{
    // ── 相机 ──────────────────────────────────────────────────
    private CameraManager?   _manager;
    private CameraDevice?    _camera;
    private ICameraProvider? _currentProvider;
    private readonly List<DeviceItem> _deviceItems = new();

    private int      _frameCount;
    private int      _savedCount;
    private DateTime _lastFpsTime       = DateTime.Now;
    private int      _lastFpsFrameCount;
    private readonly System.Windows.Forms.Timer _uiTimer;
    private readonly object _frameLock = new();

    // 线扫拼接缓冲
    private const int LineScanAccumulateRows = 512;
    private byte[]? _lineScanBuffer;
    private int     _lineScanWidth;
    private int     _lineScanBytesPerPixel;
    private int     _lineScanCurrentRow;
    private bool    _lineScanDirty;

    private FrameData?      _latestAreaFrame;
    private volatile bool   _autoSaveEnabled;

    // ── 图像显示 ──────────────────────────────────────────────
    private Bitmap?      _cameraBitmap;
    private readonly object _bitmapLock = new();

    // ── 焊点 ──────────────────────────────────────────────────
    private readonly List<SolderJoint> _solderJoints = new();
    private int  _nextJointId   = 1;
    private int? _focusedJointId;          // 当前放大显示的焊点

    // ── 模式 & 焊接状态 ───────────────────────────────────────
    private bool _isAutoMode = true;
    private enum WeldingState { Idle, Welding, Paused }
    private WeldingState _weldingState = WeldingState.Idle;

    // ─────────────────────────────────────────────────────────
    public MainForm()
    {
        InitializeComponent();

        txtCtiPath.Text = Path.Combine(AppContext.BaseDirectory, @"GenTL\Runtime\MVProducerCXP.cti");

        _uiTimer = new System.Windows.Forms.Timer { Interval = 100 };
        _uiTimer.Tick += UiTimer_Tick;
        _uiTimer.Start();

        // 相机控制事件
        cboProvider.SelectedIndexChanged += CboProvider_SelectedIndexChanged;
        btnBrowseCti.Click    += BtnBrowseCti_Click;
        btnInitialize.Click   += BtnInitialize_Click;
        btnEnumerate.Click    += BtnEnumerate_Click;
        btnConnect.Click      += BtnConnect_Click;
        btnDisconnect.Click   += BtnDisconnect_Click;
        btnStartGrab.Click    += BtnStartGrab_Click;
        btnStopGrab.Click     += BtnStopGrab_Click;
        btnGetExposure.Click  += BtnGetExposure_Click;
        btnSetExposure.Click  += BtnSetExposure_Click;

        // 工作流事件
        btnCapturePhoto.Click  += BtnCapturePhoto_Click;
        btnDetectJoints.Click  += BtnDetectJoints_Click;
        btnAutoMode.Click      += (_, _) => SetMode(true);
        btnManualMode.Click    += (_, _) => SetMode(false);
        btnSelectAll.Click     += (_, _) => { jointCirclePanel.SelectAll();   SyncJointsFromPanel(); UpdateCircleCount(); picCamera.Invalidate(); };
        btnDeselectAll.Click   += (_, _) => { jointCirclePanel.SelectNone();  SyncJointsFromPanel(); UpdateCircleCount(); picCamera.Invalidate(); };

        // 圈圈选择事件
        jointCirclePanel.JointToggled += JointCircle_JointToggled;

        // 焊接按钮
        btnStartWelding.Click += BtnStartWelding_Click;
        btnPauseWelding.Click += BtnPauseWelding_Click;
        btnStopWelding.Click  += BtnStopWelding_Click;

        // 日志
        btnClearLog.Click += (_, _) => txtLog.Clear();

        // 相机画面叠层
        picCamera.Paint      += PicCamera_Paint;
        picCamera.MouseClick += PicCamera_MouseClick;

        FormClosing += MainForm_FormClosing;

        // 初始模式状态
        SetMode(true);
        UpdateCircleCount();

        AppendLog("GenICam/GenTL 焊点检测系统已启动");
        AppendLog("流程: ① 拍照 → ② 识别焊点 → ③ 右侧圈圈选择 → ④ 选择模式 → ⑤ 开始焊接");
    }

    // ─────────────────────────────────────────────────────────
    // 日志
    // ─────────────────────────────────────────────────────────
    private void AppendLog(string msg)
    {
        if (InvokeRequired) { BeginInvoke(() => AppendLog(msg)); return; }
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss.fff}] {msg}\n");
        txtLog.SelectionStart = txtLog.Text.Length;
        txtLog.ScrollToCaret();
    }

    // ─────────────────────────────────────────────────────────
    // 厂商 / CTI
    // ─────────────────────────────────────────────────────────
    private void CboProvider_SelectedIndexChanged(object? sender, EventArgs e)
    {
        txtCtiPath.Text = cboProvider.SelectedIndex == 1
            ? Path.Combine(AppContext.BaseDirectory, @"GenTL\Runtime\MVProducerGEV.cti")
            : Path.Combine(AppContext.BaseDirectory, @"GenTL\Runtime\MVProducerCXP.cti");
        _currentProvider = null;
        _camera = null;
        _deviceItems.Clear();
        lstDevices.Items.Clear();
        ResetButtonStates();
        AppendLog($"已切换厂商: {cboProvider.Text}，请重新初始化 CTI");
    }

    private void BtnBrowseCti_Click(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title = "选择 CTI 文件",
            Filter = "GenTL Producer (*.cti)|*.cti|所有文件 (*.*)|*.*",
            InitialDirectory = Path.GetDirectoryName(txtCtiPath.Text) ?? @"C:\"
        };
        if (dlg.ShowDialog() == DialogResult.OK) txtCtiPath.Text = dlg.FileName;
    }

    // ─────────────────────────────────────────────────────────
    // 初始化与枚举
    // ─────────────────────────────────────────────────────────
    private void BtnInitialize_Click(object? sender, EventArgs e)
    {
        try
        {
            string ctiPath = txtCtiPath.Text.Trim();
            if (!File.Exists(ctiPath))
            {
                MessageBox.Show($"CTI 文件不存在:\n{ctiPath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _manager?.Dispose();
            _manager = new CameraManager();
            _currentProvider = cboProvider.SelectedIndex == 1
                ? (ICameraProvider)new HikvisionProvider(ctiPath)
                : new HuarayProvider(ctiPath);
            _manager.RegisterProvider(_currentProvider);
            _manager.InitializeAll();
            AppendLog("CTI 初始化成功");
            SetStatus("已初始化", Color.DarkOrange);
            btnEnumerate.Enabled = true;
            btnInitialize.Enabled = false;
        }
        catch (Exception ex)
        {
            AppendLog($"初始化失败: {ex.Message}");
            MessageBox.Show(ex.Message, "初始化失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnEnumerate_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_manager == null) return;
            var allDevices = _manager.EnumerateAllDevices();
            _deviceItems.Clear();
            lstDevices.Items.Clear();
            foreach (var (provider, info) in allDevices)
            {
                var item = new DeviceItem { Provider = provider, Info = info };
                _deviceItems.Add(item);
                lstDevices.Items.Add(item);
            }
            AppendLog($"枚举完成，找到 {allDevices.Count} 个设备");
            if (allDevices.Count > 0) { lstDevices.SelectedIndex = 0; btnConnect.Enabled = true; }
            else AppendLog("未找到设备，请检查相机连接");
        }
        catch (Exception ex) { AppendLog($"枚举失败: {ex.Message}"); }
    }

    // ─────────────────────────────────────────────────────────
    // 连接 / 断开
    // ─────────────────────────────────────────────────────────
    private void BtnConnect_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_manager == null || lstDevices.SelectedItem is not DeviceItem selected) return;
            _camera = _manager.OpenDevice(selected.Provider, selected.Info);
            _camera.OnLog          += AppendLog;
            _camera.OnFrameReceived += OnFrameReceived;
            AppendLog($"已连接: {selected.Info.Model} (SN:{selected.Info.SerialNumber})");
            SetStatus("已连接", Color.Green);
            btnConnect.Enabled    = false;
            btnDisconnect.Enabled = true;
            btnStartGrab.Enabled  = true;
            btnEnumerate.Enabled  = false;
            try
            {
                _camera.ParseXmlRegisters();
                bool hasExp = _camera.HasExposureControl;
                txtExposureTime.Enabled = hasExp;
                btnGetExposure.Enabled  = hasExp;
                btnSetExposure.Enabled  = hasExp;
                if (hasExp)
                {
                    double cur = _camera.GetExposureTime();
                    if (cur > 0) txtExposureTime.Text = cur.ToString("F0");
                    AppendLog($"当前曝光时间: {cur} μs");
                }
            }
            catch (Exception ex2) { AppendLog($"读取曝光参数失败: {ex2.Message}"); }
        }
        catch (Exception ex)
        {
            AppendLog($"连接失败: {ex.Message}");
            MessageBox.Show(ex.Message, "连接失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnDisconnect_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_camera != null)
            {
                if (_camera.IsGrabbing) StopGrabbing();
                _camera.OnFrameReceived -= OnFrameReceived;
                _camera.OnLog           -= AppendLog;
                _camera.Dispose();
                _camera = null;
            }
            AppendLog("已断开设备");
            SetStatus("已断开", Color.Gray);
            btnConnect.Enabled     = true;
            btnDisconnect.Enabled  = false;
            btnStartGrab.Enabled   = false;
            btnStopGrab.Enabled    = false;
            btnEnumerate.Enabled   = true;
            txtExposureTime.Enabled = false;
            btnGetExposure.Enabled  = false;
            btnSetExposure.Enabled  = false;
        }
        catch (Exception ex) { AppendLog($"断开失败: {ex.Message}"); }
    }

    // ─────────────────────────────────────────────────────────
    // 采集控制
    // ─────────────────────────────────────────────────────────
    private void BtnStartGrab_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_camera == null) return;
            _frameCount = 0; _savedCount = 0;
            _lastFpsTime = DateTime.Now; _lastFpsFrameCount = 0;
            lock (_frameLock) { _lineScanBuffer = null; _lineScanCurrentRow = 0; _lineScanDirty = false; }
            _camera.StartGrab((int)nudBufferCount.Value);
            SetStatus("采集中...", Color.Green);
            btnStartGrab.Enabled  = false;
            btnStopGrab.Enabled   = true;
            btnDisconnect.Enabled = false;
        }
        catch (Exception ex)
        {
            AppendLog($"启动采集失败: {ex.Message}");
            MessageBox.Show(ex.Message, "采集失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnStopGrab_Click(object? sender, EventArgs e) => StopGrabbing();

    private void StopGrabbing()
    {
        try
        {
            _camera?.StopGrab();
            SetStatus("已停止", Color.DarkOrange);
            btnStartGrab.Enabled  = true;
            btnStopGrab.Enabled   = false;
            btnDisconnect.Enabled = true;
            AppendLog($"采集完成，共 {_frameCount} 帧");
        }
        catch (Exception ex) { AppendLog($"停止采集失败: {ex.Message}"); }
    }

    // ─────────────────────────────────────────────────────────
    // 曝光控制
    // ─────────────────────────────────────────────────────────
    private void BtnSetExposure_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_camera == null) return;
            if (!double.TryParse(txtExposureTime.Text, out double us) || us <= 0)
            { MessageBox.Show("请输入有效的曝光时间（正数，单位微秒）", "提示"); return; }
            _camera.SetExposureTime(us);
            AppendLog($"曝光时间已设置: {us} μs");
        }
        catch (Exception ex) { AppendLog($"设置曝光失败: {ex.Message}"); }
    }

    private void BtnGetExposure_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_camera == null) return;
            double us = _camera.GetExposureTime();
            txtExposureTime.Text = us.ToString("F0");
            AppendLog($"当前曝光时间: {us} μs");
        }
        catch (Exception ex) { AppendLog($"读取曝光失败: {ex.Message}"); }
    }

    // ─────────────────────────────────────────────────────────
    // 帧处理
    // ─────────────────────────────────────────────────────────
    private void OnFrameReceived(FrameData frame)
    {
        Interlocked.Increment(ref _frameCount);
        bool isLineScan = frame.Height <= 16;

        if (isLineScan)
        {
            lock (_frameLock)
            {
                int bpp = (frame.Width > 0 && frame.Height > 0) ? frame.Data.Length / (frame.Width * frame.Height) : 1;
                if (bpp <= 0) bpp = 1;
                int rowBytes = frame.Width * bpp;

                if (_lineScanBuffer == null || _lineScanWidth != frame.Width || _lineScanBytesPerPixel != bpp)
                {
                    _lineScanWidth = frame.Width; _lineScanBytesPerPixel = bpp;
                    _lineScanBuffer = new byte[rowBytes * LineScanAccumulateRows];
                    _lineScanCurrentRow = 0;
                }
                int rowsToCopy = Math.Min(frame.Height, LineScanAccumulateRows);
                for (int r = 0; r < rowsToCopy; r++)
                {
                    int srcOff = r * rowBytes;
                    int dstRow = _lineScanCurrentRow % LineScanAccumulateRows;
                    int dstOff = dstRow * rowBytes;
                    if (srcOff + rowBytes <= frame.Data.Length && dstOff + rowBytes <= _lineScanBuffer.Length)
                        Buffer.BlockCopy(frame.Data, srcOff, _lineScanBuffer, dstOff, rowBytes);
                    _lineScanCurrentRow++;
                }
                _lineScanDirty = true;
            }
        }
        else
        {
            lock (_frameLock) { _lineScanBuffer = null; }
            Volatile.Write(ref _latestAreaFrame, frame);
        }

        if (_autoSaveEnabled && _frameCount % 10 == 0 && _savedCount < 50)
        {
            string dir  = Path.Combine(Path.GetDirectoryName(typeof(MainForm).Assembly.Location) ?? ".", "captures");
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, $"frame_{frame.FrameId}_{frame.Width}x{frame.Height}.raw");
            frame.SaveRaw(path);
            Interlocked.Increment(ref _savedCount);
            AppendLog($"已保存: {path}");
        }
    }

    private void UiTimer_Tick(object? sender, EventArgs e)
    {
        _autoSaveEnabled = chkAutoSave.Checked;
        bool updated = false;

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
                        var bmp = BuildLineScanBitmap(_lineScanBuffer, _lineScanWidth, totalRows, _lineScanBytesPerPixel);
                        SetDisplayBitmap(bmp);
                        lblResolution.Text = $"{_lineScanWidth} x {totalRows} (线扫)";
                        updated = true;
                    }
                    catch { }
                }
            }
        }

        if (!updated)
        {
            var frame = Interlocked.Exchange(ref _latestAreaFrame, null);
            if (frame != null && frame.Width > 0 && frame.Height > 0)
            {
                try
                {
                    var bmp = BuildAreaBitmap(frame);
                    SetDisplayBitmap(bmp);
                    lblResolution.Text = $"{frame.Width} x {frame.Height}";
                    lblPixelFmt.Text   = $"0x{frame.PixelFormat:X8}";
                }
                catch { }
            }
        }

        lblFrameCount.Text = _frameCount.ToString();
        double elapsed = (DateTime.Now - _lastFpsTime).TotalSeconds;
        if (elapsed >= 1.0)
        {
            lblFps.Text        = $"{(_frameCount - _lastFpsFrameCount) / elapsed:F1}";
            _lastFpsTime       = DateTime.Now;
            _lastFpsFrameCount = _frameCount;
        }

        UpdateZoomView();
    }

    private void SetDisplayBitmap(Bitmap bmp)
    {
        lock (_bitmapLock)
        {
            var old = _cameraBitmap;
            _cameraBitmap  = bmp;
            picCamera.Image = bmp;
            picCamera.Invalidate();
            old?.Dispose();
        }
    }

    // ─────────────────────────────────────────────────────────
    // 位图构建
    // ─────────────────────────────────────────────────────────
    private static Bitmap BuildAreaBitmap(FrameData frame)
    {
        int w = frame.Width, h = frame.Height;
        int bpp = frame.Data.Length / (w * h);
        if (bpp <= 0) bpp = 1;
        return bpp == 1 ? BuildMono8(frame.Data, w, h)
             : bpp == 3 ? BuildRgb24(frame.Data, w, h)
             : BuildMono8(ExtractFirstChannel(frame.Data, w * h, bpp), w, h);
    }

    private static Bitmap BuildLineScanBitmap(byte[] buf, int width, int totalRows, int bpp)
    {
        int rowBytes = width * bpp;
        byte[] display = new byte[rowBytes * totalRows];
        Buffer.BlockCopy(buf, 0, display, 0, display.Length);
        return bpp == 1 ? BuildMono8(display, width, totalRows) : BuildRgb24(display, width, totalRows);
    }

    private static Bitmap BuildMono8(byte[] data, int w, int h)
    {
        var bmp = new Bitmap(w, h, PixelFormat.Format8bppIndexed);
        var pal = bmp.Palette;
        for (int i = 0; i < 256; i++) pal.Entries[i] = Color.FromArgb(i, i, i);
        bmp.Palette = pal;
        var bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
        int stride = bd.Stride;
        if (stride == w)
            Marshal.Copy(data, 0, bd.Scan0, Math.Min(data.Length, w * h));
        else
            for (int y = 0; y < h && y * w < data.Length; y++)
                Marshal.Copy(data, y * w, bd.Scan0 + y * stride, Math.Min(w, data.Length - y * w));
        bmp.UnlockBits(bd);
        return bmp;
    }

    private static Bitmap BuildRgb24(byte[] data, int w, int h)
    {
        var bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
        var bd  = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        int rowBytes = w * 3;
        for (int y = 0; y < h && y * rowBytes < data.Length; y++)
            Marshal.Copy(data, y * rowBytes, bd.Scan0 + y * bd.Stride, Math.Min(rowBytes, data.Length - y * rowBytes));
        bmp.UnlockBits(bd);
        return bmp;
    }

    private static byte[] ExtractFirstChannel(byte[] data, int pixels, int bpp)
    {
        var gray = new byte[pixels];
        for (int i = 0; i < pixels && i * bpp < data.Length; i++) gray[i] = data[i * bpp];
        return gray;
    }

    // ─────────────────────────────────────────────────────────
    // 拍照 & 焊点检测
    // ─────────────────────────────────────────────────────────
    private void BtnCapturePhoto_Click(object? sender, EventArgs e)
    {
        var frame = Volatile.Read(ref _latestAreaFrame);
        if (frame != null && frame.Width > 0)
        {
            SetDisplayBitmap(BuildAreaBitmap(frame));
            AppendLog($"已截取当前帧: {frame.Width}x{frame.Height}");
        }
        else
        {
            AppendLog("尚无可用帧，请先开始采集");
        }
    }

    private void BtnDetectJoints_Click(object? sender, EventArgs e)
    {
        lock (_bitmapLock)
        {
            if (_cameraBitmap == null) { AppendLog("请先拍照或开始采集"); return; }
        }

        _solderJoints.Clear();
        _nextJointId  = 1;
        _focusedJointId = null;

        int imgW, imgH;
        lock (_bitmapLock) { imgW = _cameraBitmap!.Width; imgH = _cameraBitmap.Height; }

        // Mock 检测：模拟 48 / 54 / 72 三种料型，可替换为真实算法
        var rng        = new Random();
        int[] counts   = { 48, 54, 72 };
        int detectedN  = counts[rng.Next(counts.Length)];

        for (int i = 0; i < detectedN; i++)
        {
            _solderJoints.Add(new SolderJoint
            {
                Id         = _nextJointId++,
                X          = rng.Next(40, imgW - 40),
                Y          = rng.Next(40, imgH - 40),
                IsSelected = true
            });
        }

        // 将检测到的焊点 ID 传给圈圈面板
        jointCirclePanel.SetPresentJoints(_solderJoints.Select(j => j.Id));
        // 确保 _solderJoints 的 IsSelected 与圈圈初始状态（全选）同步
        SyncJointsFromPanel();
        UpdateCircleCount();
        picCamera.Invalidate();
        AppendLog($"检测到 {detectedN} 个焊点（模拟数据），已显示在右侧选择界面");
    }

    // ─────────────────────────────────────────────────────────
    // 圈圈面板事件
    // ─────────────────────────────────────────────────────────
    private void JointCircle_JointToggled(object? sender, int slotIndex)
    {
        int id = slotIndex + 1;
        var joint = _solderJoints.FirstOrDefault(j => j.Id == id);
        if (joint != null)
            joint.IsSelected = jointCirclePanel.GetState(slotIndex) == JointCirclePanel.JointState.Selected;

        _focusedJointId = id;
        UpdateCircleCount();
        picCamera.Invalidate();
    }

    // 将圈圈面板的当前状态同步回 _solderJoints
    private void SyncJointsFromPanel()
    {
        foreach (var j in _solderJoints)
            j.IsSelected = jointCirclePanel.GetState(j.Id - 1) == JointCirclePanel.JointState.Selected;
    }

    private void UpdateCircleCount()
    {
        int sel   = jointCirclePanel.SelectedCount;
        int total = jointCirclePanel.PresentCount;
        lblCircleCount.Text = $"已选: {sel} / {total} 个焊点";
    }

    // ─────────────────────────────────────────────────────────
    // 焊点放大图
    // ─────────────────────────────────────────────────────────
    private void UpdateZoomView()
    {
        if (_focusedJointId == null)
        {
            if (picZoom.Image != null) { picZoom.Image = null; }
            return;
        }

        var joint = _solderJoints.FirstOrDefault(j => j.Id == _focusedJointId.Value);
        if (joint == null) return;

        Bitmap? src;
        lock (_bitmapLock) { src = _cameraBitmap; }
        if (src == null) return;

        const int half = 70;
        int cx = (int)joint.X, cy = (int)joint.Y;
        int x  = Math.Clamp(cx - half, 0, src.Width  - 1);
        int y  = Math.Clamp(cy - half, 0, src.Height - 1);
        int w  = Math.Min(half * 2, src.Width  - x);
        int h  = Math.Min(half * 2, src.Height - y);
        if (w <= 0 || h <= 0) return;

        var crop = new Bitmap(w, h);
        try
        {
            using var g = Graphics.FromImage(crop);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src, new Rectangle(0, 0, w, h), new Rectangle(x, y, w, h), GraphicsUnit.Pixel);

            // 画中心十字
            using var pen = new Pen(Color.LimeGreen, 1f);
            int mx = cx - x, my = cy - y;
            g.DrawLine(pen, mx - 8, my, mx + 8, my);
            g.DrawLine(pen, mx, my - 8, mx, my + 8);
        }
        catch { crop.Dispose(); return; }

        var old = picZoom.Image;
        picZoom.Image = crop;
        old?.Dispose();
    }

    // ─────────────────────────────────────────────────────────
    // 图像叠层绘制
    // ─────────────────────────────────────────────────────────
    private void PicCamera_Paint(object? sender, PaintEventArgs e)
    {
        if (_solderJoints.Count == 0) return;
        Bitmap? bmp;
        lock (_bitmapLock) { bmp = _cameraBitmap; }
        if (bmp == null) return;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var imgRect = GetZoomRect(bmp, picCamera.ClientSize);
        if (imgRect.IsEmpty) return;

        float scale = imgRect.Width / bmp.Width;
        float r     = Math.Clamp(scale * 18f, 7f, 20f);

        using var selPen     = new Pen(Color.LimeGreen, 2f);
        using var unselPen   = new Pen(Color.OrangeRed, 2f);
        using var selFill    = new SolidBrush(Color.FromArgb(65, Color.LimeGreen));
        using var unselFill  = new SolidBrush(Color.FromArgb(55, Color.OrangeRed));
        using var labelFont  = new Font("Arial", 7.5f, FontStyle.Bold);
        using var selBrush   = new SolidBrush(Color.LimeGreen);
        using var unselBrush = new SolidBrush(Color.OrangeRed);

        foreach (var joint in _solderJoints)
        {
            bool sel    = joint.IsSelected;
            float dx    = imgRect.X + joint.X * scale;
            float dy    = imgRect.Y + joint.Y * scale;
            bool focused = joint.Id == _focusedJointId;

            var pen   = sel ? selPen   : unselPen;
            var fill  = sel ? selFill  : unselFill;
            var lbr   = sel ? selBrush : unselBrush;

            // 聚焦焊点画双圈
            if (focused)
            {
                using var focusPen = new Pen(Color.Yellow, 1.5f);
                e.Graphics.DrawEllipse(focusPen, dx - r - 4, dy - r - 4, (r + 4) * 2, (r + 4) * 2);
            }

            e.Graphics.FillEllipse(fill, dx - r, dy - r, r * 2, r * 2);
            e.Graphics.DrawEllipse(pen,  dx - r, dy - r, r * 2, r * 2);
            e.Graphics.DrawLine(pen, dx - r * 0.35f, dy, dx + r * 0.35f, dy);
            e.Graphics.DrawLine(pen, dx, dy - r * 0.35f, dx, dy + r * 0.35f);
            e.Graphics.DrawString($"J{joint.Id}", labelFont, lbr, dx + r + 2, dy - 8);
        }
    }

    private void PicCamera_MouseClick(object? sender, MouseEventArgs e)
    {
        Bitmap? bmp;
        lock (_bitmapLock) { bmp = _cameraBitmap; }
        if (bmp == null || _solderJoints.Count == 0) return;

        var imgRect = GetZoomRect(bmp, picCamera.ClientSize);
        if (imgRect.IsEmpty) return;

        float scale = imgRect.Width / bmp.Width;
        float imgX  = (e.X - imgRect.X) / scale;
        float imgY  = (e.Y - imgRect.Y) / scale;
        if (imgX < 0 || imgY < 0 || imgX > bmp.Width || imgY > bmp.Height) return;

        float hitRadius = 22f / scale;
        SolderJoint? nearest = null;
        float minDist = float.MaxValue;
        foreach (var j in _solderJoints)
        {
            float dist = MathF.Sqrt((j.X - imgX) * (j.X - imgX) + (j.Y - imgY) * (j.Y - imgY));
            if (dist < hitRadius && dist < minDist) { minDist = dist; nearest = j; }
        }

        if (nearest != null)
        {
            // 同步圈圈面板
            int slot = nearest.Id - 1;
            if (slot >= 0 && slot < JointCirclePanel.TotalSlots)
            {
                nearest.IsSelected = !nearest.IsSelected;
                if (nearest.IsSelected)
                    jointCirclePanel.SelectAll(); // 临时 hack，实际应直接操作单个槽位
                // 圈圈面板通过 JointToggled 反向同步，此处直接更新状态
                var newState = nearest.IsSelected
                    ? JointCirclePanel.JointState.Selected
                    : JointCirclePanel.JointState.Unselected;
                // JointCirclePanel 不对外暴露直接设置单个槽位 API，通过 SetPresentJoints 重置
                // 简化处理：重新设置 panel，保留当前选中状态
                ApplyJointsToPanel();
            }

            _focusedJointId = nearest.Id;
            UpdateCircleCount();
            picCamera.Invalidate();
            AppendLog($"焊点 J{nearest.Id} 已{(nearest.IsSelected ? "选中" : "取消选中")}");
        }
    }

    // 将 _solderJoints 的选中状态写回圈圈面板
    private void ApplyJointsToPanel()
    {
        jointCirclePanel.SetPresentJoints(_solderJoints.Select(j => j.Id));
        foreach (var j in _solderJoints)
            jointCirclePanel.SetSlotSelected(j.Id - 1, j.IsSelected);
    }

    private static RectangleF GetZoomRect(Bitmap bmp, Size clientSize)
    {
        float sx = (float)clientSize.Width  / bmp.Width;
        float sy = (float)clientSize.Height / bmp.Height;
        float s  = Math.Min(sx, sy);
        float w  = bmp.Width * s, h = bmp.Height * s;
        return new RectangleF((clientSize.Width - w) / 2f, (clientSize.Height - h) / 2f, w, h);
    }

    // ─────────────────────────────────────────────────────────
    // 模式切换（自动 / 手动）
    // ─────────────────────────────────────────────────────────
    private void SetMode(bool autoMode)
    {
        _isAutoMode = autoMode;

        // 激活态按钮高亮
        btnAutoMode.BackColor   = autoMode ? Color.FromArgb(142, 68, 173) : Color.FromArgb(52, 55, 65);
        btnAutoMode.ForeColor   = Color.White;
        btnManualMode.BackColor = autoMode ? Color.FromArgb(52, 55, 65)   : Color.FromArgb(142, 68, 173);
        btnManualMode.ForeColor = Color.White;

        AppendLog($"焊接模式: {(autoMode ? "自动（全部选中焊点依次焊接）" : "手动（点击右侧圈圈选定单个焊点）")}");
    }

    // ─────────────────────────────────────────────────────────
    // 焊接控制按钮（Start/Pause/Stop 仅存根）
    // ─────────────────────────────────────────────────────────
    private void BtnStartWelding_Click(object? sender, EventArgs e)
    {
        var selected = _solderJoints.Where(j => j.IsSelected).ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show("请至少选择一个焊点", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        SendJointCoordinates(selected);
        _weldingState = WeldingState.Welding;
        UpdateWeldingButtons();
        SetWeldStatus("焊接中...", Color.LimeGreen);
        AppendLog($"[存根] 开始焊接，模式={(_isAutoMode ? "自动" : "手动")}，共 {selected.Count} 个焊点");
    }

    private void BtnPauseWelding_Click(object? sender, EventArgs e)
    {
        if (_weldingState == WeldingState.Welding)
        {
            _weldingState = WeldingState.Paused;
            UpdateWeldingButtons();
            SetWeldStatus("已暂停", Color.Orange);
            AppendLog("[存根] 焊接已暂停");
        }
        else if (_weldingState == WeldingState.Paused)
        {
            _weldingState = WeldingState.Welding;
            UpdateWeldingButtons();
            SetWeldStatus("焊接中...", Color.LimeGreen);
            AppendLog("[存根] 焊接已恢复");
        }
    }

    private void BtnStopWelding_Click(object? sender, EventArgs e)
    {
        _weldingState = WeldingState.Idle;
        UpdateWeldingButtons();
        SetWeldStatus("就绪", Color.FromArgb(130, 130, 140));
        AppendLog("[存根] 焊接已停止");
    }

    private void UpdateWeldingButtons()
    {
        btnStartWelding.Enabled  = _weldingState == WeldingState.Idle;
        btnPauseWelding.Enabled  = _weldingState is WeldingState.Welding or WeldingState.Paused;
        btnStopWelding.Enabled   = _weldingState != WeldingState.Idle;
        btnPauseWelding.Text     = _weldingState == WeldingState.Paused ? "▶  恢复" : "⏸  暂停";
    }

    // ─────────────────────────────────────────────────────────
    // IPC：向焊接进程发送坐标
    // ─────────────────────────────────────────────────────────
    private void SendJointCoordinates(List<SolderJoint> joints)
    {
        var sb = new StringBuilder("[");
        for (int i = 0; i < joints.Count; i++)
        {
            var j = joints[i];
            sb.Append($"{{\"id\":{j.Id},\"x\":{j.X:F2},\"y\":{j.Y:F2}}}");
            if (i < joints.Count - 1) sb.Append(',');
        }
        sb.Append(']');
        string json = sb.ToString();

        bool sent = false;
        try
        {
            using var pipe = new NamedPipeClientStream(".", "weld-joints", PipeDirection.Out);
            pipe.Connect(300);
            using var writer = new StreamWriter(pipe);
            writer.WriteLine(json);
            sent = true;
            AppendLog($"已通过命名管道发送 {joints.Count} 个焊点坐标");
        }
        catch { }

        if (!sent)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "weld_joints.json");
            File.WriteAllText(path, json, Encoding.UTF8);
            AppendLog($"已写入焊点坐标到文件: weld_joints.json（共 {joints.Count} 个）");
        }
    }

    // ─────────────────────────────────────────────────────────
    // 辅助
    // ─────────────────────────────────────────────────────────
    private void SetStatus(string text, Color color)
    {
        lblState.Text     = text;
        lblState.ForeColor = color;
    }

    private void SetWeldStatus(string text, Color color)
    {
        lblWeldStatus.Text      = text;
        lblWeldStatus.ForeColor = color;
    }

    private void ResetButtonStates()
    {
        btnInitialize.Enabled   = true;
        btnEnumerate.Enabled    = false;
        btnConnect.Enabled      = false;
        btnDisconnect.Enabled   = false;
        btnStartGrab.Enabled    = false;
        btnStopGrab.Enabled     = false;
        btnSetExposure.Enabled  = false;
        btnGetExposure.Enabled  = false;
        txtExposureTime.Enabled = false;
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _uiTimer.Stop();
        try
        {
            if (_camera != null)
            {
                _camera.OnFrameReceived -= OnFrameReceived;
                _camera.OnLog           -= AppendLog;
                _camera.Dispose();
            }
        }
        catch { }
        try { _manager?.Dispose(); } catch { }
        picZoom.Image?.Dispose();
    }
}

public class DeviceItem
{
    public ICameraProvider Provider { get; set; } = null!;
    public CameraDeviceInfo Info    { get; set; } = null!;
    public override string ToString() => $"{Info.Model} (SN:{Info.SerialNumber})";
}
