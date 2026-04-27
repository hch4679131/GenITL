using System.Drawing.Drawing2D;
using GenICamCameraCapture.Calibration;
using GenICamCameraCapture.Models;

namespace GenICamCameraCapture.UI;

/// <summary>
/// 9点标定窗口。
/// 流程：① TCP连接 → ② 逐点发送移动命令 → ③ 抓帧标记像素 → ④ 计算矩阵 → ⑤ 保存应用。
/// </summary>
public partial class CalibrationForm : Form
{
    // ── 依赖注入 ──────────────────────────────────────────────
    private readonly Func<Bitmap?> _grabFrame; // 从主窗口借用最新帧

    // ── 状态 ─────────────────────────────────────────────────
    private readonly TcpRobotLink    _tcp    = new();
    private readonly CalibrationPoint[] _pts = new CalibrationPoint[9];

    private Bitmap? _previewBitmap;
    private bool    _markingMode;       // 等待用户在图上点击
    private int     _markingIndex = -1; // 当前标记的行 (0-8)

    // 计算结果，对外只读
    public float[,]? CalibMatrix { get; private set; }

    // 默认 3×3 机器人坐标格（mm），可在表格中修改
    private static readonly (float x, float y)[] DefaultGrid =
    {
        (150f, 150f), (200f, 150f), (250f, 150f),
        (150f, 200f), (200f, 200f), (250f, 200f),
        (150f, 250f), (200f, 250f), (250f, 250f)
    };

    // ─────────────────────────────────────────────────────────
    public CalibrationForm(Func<Bitmap?> grabFrame)
    {
        _grabFrame = grabFrame;
        InitializeComponent();
        InitPoints();
        BindEvents();
        PopulateGrid();
        Log("9点标定就绪。流程：连接TCP → 逐点移动机器人 → 抓帧标记像素 → 计算矩阵 → 保存应用");
    }

    // ── 初始化 ────────────────────────────────────────────────

    private void InitPoints()
    {
        for (int i = 0; i < 9; i++)
            _pts[i] = new CalibrationPoint
            {
                Index  = i + 1,
                RobotX = DefaultGrid[i].x,
                RobotY = DefaultGrid[i].y
            };
    }

    private void BindEvents()
    {
        // TCP
        btnTcpConnect.Click    += BtnTcpConnect_Click;
        btnTcpDisconnect.Click += BtnTcpDisconnect_Click;
        rdoClient.CheckedChanged += (_, _) => txtIp.Enabled = rdoClient.Checked;

        // 标定点操作
        btnGrabFrame.Click    += BtnGrabFrame_Click;
        btnMoveToPoint.Click  += BtnMoveToPoint_Click;
        btnMarkPoint.Click    += BtnMarkPoint_Click;
        btnClearPoint.Click   += BtnClearPoint_Click;
        picCalib.Paint        += PicCalib_Paint;
        picCalib.MouseClick   += PicCalib_MouseClick;

        // 表格机器人坐标编辑回写
        dgvPoints.CellEndEdit += DgvPoints_CellEndEdit;

        // 计算与保存
        btnCompute.Click   += BtnCompute_Click;
        btnSaveApply.Click += BtnSaveApply_Click;

        // 结果区自适应日志文本框
        grpResult.Resize += GrpResult_Resize;

        // TCP 事件（跨线程）
        _tcp.LogOutput    += (_, msg) => SafeLog(msg);
        _tcp.DataReceived += (_, msg) => SafeLog($"[机器人] {msg}");

        FormClosing += (_, _) => _tcp.Dispose();
    }

    private void GrpResult_Resize(object? sender, EventArgs e)
    {
        txtTcpLog.Size = new Size(grpResult.Width - 16, grpResult.Height - 62);
    }

    private void PopulateGrid()
    {
        dgvPoints.Rows.Clear();
        foreach (var p in _pts)
        {
            dgvPoints.Rows.Add(
                p.Index,
                p.RobotX.ToString("F2"),
                p.RobotY.ToString("F2"),
                p.IsMarked ? p.PixelX.ToString("F1") : "--",
                p.IsMarked ? p.PixelY.ToString("F1") : "--",
                p.IsMarked ? "✓ 已标记" : "待标记");
        }
    }

    private void RefreshRow(int idx)
    {
        var p   = _pts[idx];
        var row = dgvPoints.Rows[idx];
        row.Cells["ColRobotX"].Value = p.RobotX.ToString("F2");
        row.Cells["ColRobotY"].Value = p.RobotY.ToString("F2");
        row.Cells["ColPixelX"].Value = p.IsMarked ? p.PixelX.ToString("F1") : "--";
        row.Cells["ColPixelY"].Value = p.IsMarked ? p.PixelY.ToString("F1") : "--";
        row.Cells["ColStatus"].Value = p.IsMarked ? "✓ 已标记" : "待标记";

        // 高亮已标记行
        row.DefaultCellStyle.BackColor =
            p.IsMarked ? Color.FromArgb(28, 80, 40) : Color.FromArgb(28, 30, 38);
    }

    // ── TCP 连接 ──────────────────────────────────────────────

    private async void BtnTcpConnect_Click(object? sender, EventArgs e)
    {
        btnTcpConnect.Enabled = false;
        try
        {
            if (!int.TryParse(txtPort.Text.Trim(), out int port) || port <= 0)
            { MessageBox.Show("端口号无效", "提示"); return; }

            if (rdoClient.Checked)
                await _tcp.ConnectAsClientAsync(txtIp.Text.Trim(), port);
            else
                await _tcp.StartServerAsync(port);

            SetConnStatus(true);
        }
        catch (Exception ex)
        {
            Log($"[错误] 连接失败: {ex.Message}");
            SetConnStatus(false);
            btnTcpConnect.Enabled = true;
        }
    }

    private void BtnTcpDisconnect_Click(object? sender, EventArgs e)
    {
        _tcp.Disconnect();
        SetConnStatus(false);
    }

    private void SetConnStatus(bool connected)
    {
        if (InvokeRequired) { BeginInvoke(() => SetConnStatus(connected)); return; }
        lblConnStatus.Text      = connected ? "● 已连接" : "● 未连接";
        lblConnStatus.ForeColor = connected ? Color.LimeGreen : Color.Gray;
        btnTcpConnect.Enabled   = !connected;
        btnTcpDisconnect.Enabled = connected;
    }

    // ── 移动机器人 ────────────────────────────────────────────

    private async void BtnMoveToPoint_Click(object? sender, EventArgs e)
    {
        int idx = SelectedRowIndex();
        if (idx < 0) { MessageBox.Show("请先选中一行", "提示"); return; }
        var p = _pts[idx];

        // 命令格式：MOVE_TO x y（mm）
        string cmd = $"MOVE_TO {p.RobotX:F2} {p.RobotY:F2}";

        if (_tcp.IsConnected)
        {
            btnMoveToPoint.Enabled = false;
            Log($"[发送] {cmd}");
            string? resp = await _tcp.SendAndWaitAsync(cmd, "OK", 10000);
            btnMoveToPoint.Enabled = true;
            if (resp == null)
                Log("[警告] 等待机器人响应超时");
            else
                Log($"[响应] {resp}");
        }
        else
        {
            // 未连接时 Mock
            Log($"[Mock] {cmd}  →  OK {p.RobotX:F2} {p.RobotY:F2}（未连接，模拟响应）");
        }
    }

    // ── 标记像素坐标 ──────────────────────────────────────────

    private void BtnGrabFrame_Click(object? sender, EventArgs e)
    {
        var bmp = _grabFrame();
        if (bmp == null) { Log("主窗口尚无可用帧，请先开始采集"); return; }

        _previewBitmap?.Dispose();
        _previewBitmap = (Bitmap)bmp.Clone();
        picCalib.Image = _previewBitmap;
        picCalib.Invalidate();
        lblClickHint.Text = "已抓取图像。选中标定点后点击「标记」，再点击图像上的靶标位置";
        Log("已抓取当前帧");
    }

    private void BtnMarkPoint_Click(object? sender, EventArgs e)
    {
        if (_previewBitmap == null) { Log("请先抓取图像"); return; }
        int idx = SelectedRowIndex();
        if (idx < 0) { MessageBox.Show("请先在表格中选中要标记的行", "提示"); return; }

        _markingMode  = true;
        _markingIndex = idx;
        picCalib.Cursor = Cursors.Cross;
        lblClickHint.Text = $"▶ 标记模式：点击图像中第 {idx + 1} 个靶标的中心";
        Log($"进入标记模式，请点击标定点 {idx + 1} 的位置");
    }

    private void PicCalib_MouseClick(object? sender, MouseEventArgs e)
    {
        if (!_markingMode || _previewBitmap == null) return;

        var rect = GetZoomRect(_previewBitmap, picCalib.ClientSize);
        if (rect.IsEmpty) return;

        float scale = rect.Width / _previewBitmap.Width;
        float imgX  = (e.X - rect.X) / scale;
        float imgY  = (e.Y - rect.Y) / scale;

        if (imgX < 0 || imgY < 0 || imgX > _previewBitmap.Width || imgY > _previewBitmap.Height) return;

        var p = _pts[_markingIndex];
        p.PixelX   = imgX;
        p.PixelY   = imgY;
        p.IsMarked = true;
        RefreshRow(_markingIndex);

        _markingMode  = false;
        _markingIndex = -1;
        picCalib.Cursor = Cursors.Default;
        lblClickHint.Text = $"✓ 标定点 {p.Index} 已记录像素坐标 ({imgX:F1}, {imgY:F1})";
        Log($"标定点 {p.Index}: 像素 ({imgX:F1}, {imgY:F1}) ← 机器人 ({p.RobotX:F2}, {p.RobotY:F2})");
        picCalib.Invalidate();
    }

    private void BtnClearPoint_Click(object? sender, EventArgs e)
    {
        int idx = SelectedRowIndex();
        if (idx < 0) return;
        _pts[idx].IsMarked = false;
        _pts[idx].PixelX   = 0;
        _pts[idx].PixelY   = 0;
        RefreshRow(idx);
        picCalib.Invalidate();
        Log($"已清除标定点 {idx + 1}");
    }

    private void DgvPoints_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= 9) return;
        var cell = dgvPoints.Rows[e.RowIndex].Cells[e.ColumnIndex];
        if (!float.TryParse(cell.Value?.ToString(), out float val)) return;

        if (dgvPoints.Columns[e.ColumnIndex].Name == "ColRobotX")
            _pts[e.RowIndex].RobotX = val;
        else if (dgvPoints.Columns[e.ColumnIndex].Name == "ColRobotY")
            _pts[e.RowIndex].RobotY = val;
    }

    // ── 计算标定矩阵 ──────────────────────────────────────────

    private void BtnCompute_Click(object? sender, EventArgs e)
    {
        int markedCount = _pts.Count(p => p.IsMarked);
        if (markedCount < 3)
        {
            MessageBox.Show($"至少需要 3 个已标记的点（当前 {markedCount} 个）", "提示");
            return;
        }

        var m = CalibrationEngine.Compute(_pts);
        if (m == null)
        {
            Log("[错误] 矩阵奇异，请检查标定点是否共线或重复");
            return;
        }

        CalibMatrix = m;
        float rms   = CalibrationEngine.ComputeRms(m, _pts);
        lblRms.Text = $"RMS误差: {rms:F3} mm  ({markedCount} 点)";
        lblRms.ForeColor = rms < 1f ? Color.LimeGreen : rms < 3f ? Color.Yellow : Color.OrangeRed;

        Log($"标定完成，RMS = {rms:F3} mm");
        Log($"矩阵: [{m[0,0]:F4}  {m[0,1]:F4}  {m[0,2]:F4}]");
        Log($"      [{m[1,0]:F4}  {m[1,1]:F4}  {m[1,2]:F4}]");

        btnSaveApply.Enabled = true;
    }

    private void BtnSaveApply_Click(object? sender, EventArgs e)
    {
        if (CalibMatrix == null) return;

        // 保存到文件
        string path = Path.Combine(AppContext.BaseDirectory, "calib_matrix.json");
        File.WriteAllText(path, CalibrationEngine.MatrixToJson(CalibMatrix));
        Log($"已保存标定矩阵到 {path}");

        DialogResult = DialogResult.OK;
        Close();
    }

    // ── 图像叠层绘制 ──────────────────────────────────────────

    private void PicCalib_Paint(object? sender, PaintEventArgs e)
    {
        if (_previewBitmap == null) return;
        var g    = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = GetZoomRect(_previewBitmap, picCalib.ClientSize);
        if (rect.IsEmpty) return;

        float scale = rect.Width / _previewBitmap.Width;

        // 已标记的点
        using var selPen  = new Pen(Color.LimeGreen, 1.5f);
        using var selFill = new SolidBrush(Color.FromArgb(80, Color.LimeGreen));
        using var fnt     = new Font("Arial", 8f, FontStyle.Bold);
        const float R = 7f;

        foreach (var p in _pts.Where(pt => pt.IsMarked))
        {
            float dx = rect.X + p.PixelX * scale;
            float dy = rect.Y + p.PixelY * scale;

            g.FillEllipse(selFill, dx - R, dy - R, R * 2, R * 2);
            g.DrawEllipse(selPen, dx - R, dy - R, R * 2, R * 2);
            g.DrawLine(selPen, dx - R * 1.5f, dy, dx + R * 1.5f, dy);
            g.DrawLine(selPen, dx, dy - R * 1.5f, dx, dy + R * 1.5f);
            using var lb = new SolidBrush(Color.LimeGreen);
            g.DrawString(p.Index.ToString(), fnt, lb, dx + R + 2, dy - 8);
        }

        // 当前标记中的行高亮
        if (_markingMode && _markingIndex >= 0)
        {
            using var hintBrush = new SolidBrush(Color.FromArgb(60, Color.Yellow));
            g.FillRectangle(hintBrush, rect);
        }
    }

    // ── 辅助 ─────────────────────────────────────────────────

    private int SelectedRowIndex() =>
        dgvPoints.SelectedRows.Count > 0 ? dgvPoints.SelectedRows[0].Index : -1;

    private void Log(string msg)
    {
        if (InvokeRequired) { BeginInvoke(() => Log(msg)); return; }
        txtTcpLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\r\n");
        txtTcpLog.SelectionStart = txtTcpLog.Text.Length;
        txtTcpLog.ScrollToCaret();
    }

    private void SafeLog(string msg) => Log(msg);

    private static RectangleF GetZoomRect(Bitmap bmp, Size client)
    {
        float sx = (float)client.Width  / bmp.Width;
        float sy = (float)client.Height / bmp.Height;
        float s  = Math.Min(sx, sy);
        float w  = bmp.Width * s, h = bmp.Height * s;
        return new RectangleF((client.Width - w) / 2f, (client.Height - h) / 2f, w, h);
    }
}
