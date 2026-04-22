namespace GenICamCameraCapture.UI;

partial class MainForm
{
    // ── 左侧相机控制栏 ────────────────────────────────────────
    private Panel pnlCameraControls = null!;
    private GroupBox grpCameraSettings = null!;
    private Label lblProvider = null!;
    private ComboBox cboProvider = null!;
    private Label lblCtiPath = null!;
    private TextBox txtCtiPath = null!;
    private Button btnBrowseCti = null!;
    private Button btnInitialize = null!;
    private Button btnEnumerate = null!;

    private GroupBox grpDeviceList = null!;
    private ListBox lstDevices = null!;
    private Button btnConnect = null!;
    private Button btnDisconnect = null!;

    private GroupBox grpAcquisition = null!;
    private Label lblBufferCount = null!;
    private NumericUpDown nudBufferCount = null!;
    private CheckBox chkAutoSave = null!;
    private Button btnStartGrab = null!;
    private Button btnStopGrab = null!;

    private GroupBox grpExposure = null!;
    private Label lblExposureUs = null!;
    private TextBox txtExposureTime = null!;
    private Button btnGetExposure = null!;
    private Button btnSetExposure = null!;

    private GroupBox grpStatus = null!;
    private Label lblStateLbl = null!;
    private Label lblState = null!;
    private Label lblFrameCountLbl = null!;
    private Label lblFrameCount = null!;
    private Label lblFpsLbl = null!;
    private Label lblFps = null!;
    private Label lblResolutionLbl = null!;
    private Label lblResolution = null!;
    private Label lblPixelFmtLbl = null!;
    private Label lblPixelFmt = null!;

    // ── 中央图像区 ────────────────────────────────────────────
    private Panel pnlImageArea = null!;
    private Label lblCameraTitle = null!;
    private PictureBox picCamera = null!;

    // ── 右侧焊点选择栏 ───────────────────────────────────────
    private Panel pnlJointControls = null!;
    private Label lblJointsTitle = null!;
    private Button btnCapturePhoto = null!;
    private Button btnDetectJoints = null!;
    private ListView listViewJoints = null!;
    private ColumnHeader colJointId = null!;
    private ColumnHeader colJointX = null!;
    private ColumnHeader colJointY = null!;
    private Panel pnlJointButtons = null!;
    private Button btnSelectAll = null!;
    private Button btnDeselectAll = null!;
    private Label lblSelectedCount = null!;

    // ── 底部焊接控制栏 ────────────────────────────────────────
    private Panel pnlWeldBar = null!;
    private Button btnStartWelding = null!;
    private Button btnPauseWelding = null!;
    private Button btnStopWelding = null!;
    private Label lblWeldStatusLbl = null!;
    private Label lblWeldStatus = null!;

    // ── 日志区 ───────────────────────────────────────────────
    private Panel pnlLog = null!;
    private TextBox txtLog = null!;
    private Button btnClearLog = null!;

    private void InitializeComponent()
    {
        SuspendLayout();

        // ─────────────────────────────────────────────────────
        // 左侧相机控制栏
        // ─────────────────────────────────────────────────────
        const int leftW = 245;

        // GroupBox: 相机设置
        lblProvider = new Label { Text = "厂商:", Location = new Point(8, 22), Size = new Size(40, 18), TextAlign = ContentAlignment.MiddleRight };
        cboProvider = new ComboBox { Location = new Point(52, 20), Size = new Size(168, 23), DropDownStyle = ComboBoxStyle.DropDownList };
        cboProvider.Items.AddRange(new object[] { "华睿 (CXP)", "海康 (GigE)" });
        cboProvider.SelectedIndex = 0;

        lblCtiPath = new Label { Text = "CTI:", Location = new Point(8, 52), Size = new Size(30, 18), TextAlign = ContentAlignment.MiddleRight };
        txtCtiPath = new TextBox { Location = new Point(8, 70), Size = new Size(174, 23), Font = new Font("微软雅黑", 7.5f) };
        btnBrowseCti = new Button { Text = "…", Location = new Point(185, 69), Size = new Size(35, 24) };

        btnInitialize = new Button { Text = "初始化 CTI", Location = new Point(8, 100), Size = new Size(104, 26) };
        btnEnumerate = new Button { Text = "枚举设备", Location = new Point(116, 100), Size = new Size(103, 26), Enabled = false };

        grpCameraSettings = new GroupBox { Text = "相机设置", Location = new Point(4, 4), Size = new Size(233, 138) };
        grpCameraSettings.Controls.AddRange(new Control[] { lblProvider, cboProvider, lblCtiPath, txtCtiPath, btnBrowseCti, btnInitialize, btnEnumerate });

        // GroupBox: 设备列表
        lstDevices = new ListBox { Location = new Point(8, 22), Size = new Size(217, 58) };
        btnConnect = new Button { Text = "连接", Location = new Point(8, 86), Size = new Size(103, 26), Enabled = false };
        btnDisconnect = new Button { Text = "断开", Location = new Point(115, 86), Size = new Size(102, 26), Enabled = false };

        grpDeviceList = new GroupBox { Text = "设备列表", Location = new Point(4, 146), Size = new Size(233, 122) };
        grpDeviceList.Controls.AddRange(new Control[] { lstDevices, btnConnect, btnDisconnect });

        // GroupBox: 采集控制
        lblBufferCount = new Label { Text = "缓冲数:", Location = new Point(8, 22), Size = new Size(52, 18), TextAlign = ContentAlignment.MiddleRight };
        nudBufferCount = new NumericUpDown { Location = new Point(64, 20), Size = new Size(60, 23), Minimum = 1, Maximum = 100, Value = 16 };
        chkAutoSave = new CheckBox { Text = "自动保存", Location = new Point(138, 22), AutoSize = true };
        btnStartGrab = new Button { Text = "开始采集", Location = new Point(8, 50), Size = new Size(103, 26), Enabled = false };
        btnStopGrab = new Button { Text = "停止采集", Location = new Point(115, 50), Size = new Size(102, 26), Enabled = false };

        grpAcquisition = new GroupBox { Text = "采集控制", Location = new Point(4, 272), Size = new Size(233, 90) };
        grpAcquisition.Controls.AddRange(new Control[] { lblBufferCount, nudBufferCount, chkAutoSave, btnStartGrab, btnStopGrab });

        // GroupBox: 曝光控制
        lblExposureUs = new Label { Text = "曝光(μs):", Location = new Point(8, 22), Size = new Size(62, 18), TextAlign = ContentAlignment.MiddleRight };
        txtExposureTime = new TextBox { Location = new Point(74, 20), Size = new Size(90, 23), Text = "10000", Enabled = false };
        btnGetExposure = new Button { Text = "读取", Location = new Point(8, 50), Size = new Size(74, 26), Enabled = false };
        btnSetExposure = new Button { Text = "设置", Location = new Point(86, 50), Size = new Size(74, 26), Enabled = false };

        grpExposure = new GroupBox { Text = "曝光控制", Location = new Point(4, 366), Size = new Size(233, 88) };
        grpExposure.Controls.AddRange(new Control[] { lblExposureUs, txtExposureTime, btnGetExposure, btnSetExposure });

        // GroupBox: 状态信息
        int sr = 22, rh = 20;
        lblStateLbl = MakeStatusLabel("状态:", 8, sr); lblState = MakeStatusValue("就绪", 72, sr, Color.Gray);
        lblFrameCountLbl = MakeStatusLabel("帧数:", 8, sr + rh); lblFrameCount = MakeStatusValue("0", 72, sr + rh, Color.Black);
        lblFpsLbl = MakeStatusLabel("FPS:", 8, sr + rh * 2); lblFps = MakeStatusValue("0.0", 72, sr + rh * 2, Color.Black);
        lblResolutionLbl = MakeStatusLabel("分辨率:", 8, sr + rh * 3); lblResolution = MakeStatusValue("--", 72, sr + rh * 3, Color.Black);
        lblPixelFmtLbl = MakeStatusLabel("像素格式:", 8, sr + rh * 4); lblPixelFmt = MakeStatusValue("--", 72, sr + rh * 4, Color.Black);

        grpStatus = new GroupBox { Text = "状态信息", Location = new Point(4, 458), Size = new Size(233, 134) };
        grpStatus.Controls.AddRange(new Control[] {
            lblStateLbl, lblState, lblFrameCountLbl, lblFrameCount,
            lblFpsLbl, lblFps, lblResolutionLbl, lblResolution,
            lblPixelFmtLbl, lblPixelFmt
        });

        pnlCameraControls = new Panel
        {
            Width = leftW,
            Dock = DockStyle.Left,
            AutoScroll = true,
            BackColor = Color.FromArgb(245, 245, 248),
            Padding = new Padding(0)
        };
        pnlCameraControls.Controls.AddRange(new Control[] {
            grpCameraSettings, grpDeviceList, grpAcquisition, grpExposure, grpStatus
        });

        // ─────────────────────────────────────────────────────
        // 右侧焊点选择栏
        // ─────────────────────────────────────────────────────
        const int rightW = 285;

        lblJointsTitle = new Label
        {
            Text = "焊点选择",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("微软雅黑", 10f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.FromArgb(60, 120, 200),
            ForeColor = Color.White
        };

        var pnlJointTopBtns = new Panel { Dock = DockStyle.Top, Height = 38, Padding = new Padding(6, 4, 6, 4) };
        btnCapturePhoto = new Button { Text = "📷 拍照", Location = new Point(6, 5), Size = new Size(88, 28), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnCapturePhoto.FlatAppearance.BorderSize = 0;
        btnDetectJoints = new Button { Text = "🔍 检测焊点", Location = new Point(100, 5), Size = new Size(106, 28), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnDetectJoints.FlatAppearance.BorderSize = 0;
        pnlJointTopBtns.Controls.AddRange(new Control[] { btnCapturePhoto, btnDetectJoints });

        colJointId = new ColumnHeader { Text = "编号", Width = 55 };
        colJointX = new ColumnHeader { Text = "X (px)", Width = 82 };
        colJointY = new ColumnHeader { Text = "Y (px)", Width = 82 };
        listViewJoints = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            CheckBoxes = true,
            FullRowSelect = true,
            GridLines = true,
            Font = new Font("微软雅黑", 9f)
        };
        listViewJoints.Columns.AddRange(new[] { colJointId, colJointX, colJointY });

        pnlJointButtons = new Panel { Dock = DockStyle.Bottom, Height = 58, Padding = new Padding(6, 6, 6, 6) };
        btnSelectAll = new Button { Text = "全选", Location = new Point(6, 6), Size = new Size(80, 26), FlatStyle = FlatStyle.Flat };
        btnDeselectAll = new Button { Text = "全不选", Location = new Point(90, 6), Size = new Size(80, 26), FlatStyle = FlatStyle.Flat };
        lblSelectedCount = new Label { Text = "已选: 0 / 0 个焊点", Location = new Point(6, 36), Size = new Size(260, 18), ForeColor = Color.FromArgb(80, 80, 80) };
        pnlJointButtons.Controls.AddRange(new Control[] { btnSelectAll, btnDeselectAll, lblSelectedCount });

        pnlJointControls = new Panel
        {
            Width = rightW,
            Dock = DockStyle.Right,
            BackColor = Color.White
        };
        pnlJointControls.Controls.Add(listViewJoints);
        pnlJointControls.Controls.Add(pnlJointButtons);
        pnlJointControls.Controls.Add(pnlJointTopBtns);
        pnlJointControls.Controls.Add(lblJointsTitle);

        // ─────────────────────────────────────────────────────
        // 中央相机预览区
        // ─────────────────────────────────────────────────────
        lblCameraTitle = new Label
        {
            Text = "相机预览",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("微软雅黑", 10f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.FromArgb(45, 45, 48),
            ForeColor = Color.White
        };

        picCamera = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(30, 30, 30),
            Cursor = Cursors.Cross
        };

        pnlImageArea = new Panel { Dock = DockStyle.Fill };
        pnlImageArea.Controls.Add(picCamera);
        pnlImageArea.Controls.Add(lblCameraTitle);

        // ─────────────────────────────────────────────────────
        // 底部焊接控制栏
        // ─────────────────────────────────────────────────────
        btnStartWelding = new Button
        {
            Text = "▶  开始焊接",
            Size = new Size(130, 36),
            Location = new Point(8, 8),
            BackColor = Color.FromArgb(39, 174, 96),
            ForeColor = Color.White,
            Font = new Font("微软雅黑", 10f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat
        };
        btnStartWelding.FlatAppearance.BorderSize = 0;

        btnPauseWelding = new Button
        {
            Text = "⏸  暂停",
            Size = new Size(110, 36),
            Location = new Point(146, 8),
            BackColor = Color.FromArgb(243, 156, 18),
            ForeColor = Color.White,
            Font = new Font("微软雅黑", 10f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        btnPauseWelding.FlatAppearance.BorderSize = 0;

        btnStopWelding = new Button
        {
            Text = "⏹  停止",
            Size = new Size(110, 36),
            Location = new Point(264, 8),
            BackColor = Color.FromArgb(231, 76, 60),
            ForeColor = Color.White,
            Font = new Font("微软雅黑", 10f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        btnStopWelding.FlatAppearance.BorderSize = 0;

        lblWeldStatusLbl = new Label { Text = "焊接状态:", Location = new Point(392, 14), Size = new Size(70, 22), Font = new Font("微软雅黑", 9f) };
        lblWeldStatus = new Label
        {
            Text = "就绪",
            Location = new Point(466, 14),
            Size = new Size(200, 22),
            ForeColor = Color.Gray,
            Font = new Font("微软雅黑", 9f, FontStyle.Bold)
        };

        pnlWeldBar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            BackColor = Color.FromArgb(235, 237, 240),
            Padding = new Padding(0)
        };
        pnlWeldBar.Controls.AddRange(new Control[] { btnStartWelding, btnPauseWelding, btnStopWelding, lblWeldStatusLbl, lblWeldStatus });

        // ─────────────────────────────────────────────────────
        // 日志区
        // ─────────────────────────────────────────────────────
        btnClearLog = new Button
        {
            Text = "清除",
            Dock = DockStyle.Right,
            Width = 56,
            FlatStyle = FlatStyle.Flat
        };
        txtLog = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(20, 20, 20),
            ForeColor = Color.FromArgb(180, 220, 180),
            Font = new Font("Consolas", 8.5f),
            BorderStyle = BorderStyle.None
        };

        pnlLog = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 140,
            BackColor = Color.FromArgb(20, 20, 20)
        };
        pnlLog.Controls.Add(txtLog);
        pnlLog.Controls.Add(btnClearLog);

        // ─────────────────────────────────────────────────────
        // Form
        // ─────────────────────────────────────────────────────
        Text = "GenICam 焊点检测与焊接系统";
        Size = new Size(1400, 900);
        MinimumSize = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("微软雅黑", 9f);

        // 添加顺序决定 Dock 叠放顺序（先 Bottom 后 Fill）
        Controls.Add(pnlImageArea);       // DockFill (最后加，先布局)
        Controls.Add(pnlCameraControls);  // DockLeft
        Controls.Add(pnlJointControls);   // DockRight
        Controls.Add(pnlWeldBar);         // DockBottom
        Controls.Add(pnlLog);             // DockBottom（在 weld bar 之上）

        ResumeLayout(false);
    }

    private static Label MakeStatusLabel(string text, int x, int y) =>
        new Label { Text = text, Location = new Point(x, y), Size = new Size(64, 18), TextAlign = ContentAlignment.MiddleRight, Font = new Font("微软雅黑", 8.5f) };

    private static Label MakeStatusValue(string text, int x, int y, Color color) =>
        new Label { Text = text, Location = new Point(x, y), Size = new Size(158, 18), ForeColor = color, Font = new Font("微软雅黑", 8.5f, FontStyle.Bold) };
}
