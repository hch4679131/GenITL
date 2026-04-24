namespace GenICamCameraCapture.UI;

partial class MainForm
{
    // ── 左侧相机控制栏 ────────────────────────────────────────
    private Panel        pnlCameraControls = null!;
    private GroupBox     grpCameraSettings = null!;
    private Label        lblProvider       = null!;
    private ComboBox     cboProvider       = null!;
    private Label        lblCtiPath        = null!;
    private TextBox      txtCtiPath        = null!;
    private Button       btnBrowseCti      = null!;
    private Button       btnInitialize     = null!;
    private Button       btnEnumerate      = null!;

    private GroupBox     grpDeviceList     = null!;
    private ListBox      lstDevices        = null!;
    private Button       btnConnect        = null!;
    private Button       btnDisconnect     = null!;

    private GroupBox     grpAcquisition    = null!;
    private Label        lblBufferCount    = null!;
    private NumericUpDown nudBufferCount   = null!;
    private CheckBox     chkAutoSave       = null!;
    private Button       btnStartGrab      = null!;
    private Button       btnStopGrab       = null!;

    private GroupBox     grpExposure       = null!;
    private Label        lblExposureUs     = null!;
    private TextBox      txtExposureTime   = null!;
    private Button       btnGetExposure    = null!;
    private Button       btnSetExposure    = null!;

    private GroupBox     grpStatus         = null!;
    private Label        lblStateLbl       = null!;
    private Label        lblState          = null!;
    private Label        lblFrameCountLbl  = null!;
    private Label        lblFrameCount     = null!;
    private Label        lblFpsLbl         = null!;
    private Label        lblFps            = null!;
    private Label        lblResolutionLbl  = null!;
    private Label        lblResolution     = null!;
    private Label        lblPixelFmtLbl    = null!;
    private Label        lblPixelFmt       = null!;

    // ── 中央区域 ──────────────────────────────────────────────
    private Panel        pnlCenter         = null!;
    private PictureBox   picCamera         = null!;
    private Panel        pnlBottom         = null!;   // 下方条带

    private GroupBox     grpZoom           = null!;
    private PictureBox   picZoom           = null!;

    private GroupBox     grpWorkflow       = null!;
    private Button       btnCapturePhoto   = null!;
    private Button       btnDetectJoints   = null!;
    private Button       btnAutoMode       = null!;
    private Button       btnManualMode     = null!;
    private Button       btnStartWelding   = null!;
    private Button       btnPauseWelding   = null!;
    private Button       btnStopWelding    = null!;
    private Label        lblWeldStatus     = null!;

    // ── 右侧焊点圈圈面板 ─────────────────────────────────────
    private Panel            pnlRight          = null!;
    private Label            lblCircleTitle    = null!;
    private JointCirclePanel jointCirclePanel  = null!;
    private Panel            pnlCircleActions  = null!;
    private Button           btnSelectAll      = null!;
    private Button           btnDeselectAll    = null!;
    private Label            lblCircleCount    = null!;

    // ── 日志区 ───────────────────────────────────────────────
    private Panel    pnlLog    = null!;
    private TextBox  txtLog    = null!;
    private Button   btnClearLog = null!;

    // ─────────────────────────────────────────────────────────
    private void InitializeComponent()
    {
        SuspendLayout();

        BuildCameraControlsPanel();
        BuildRightCirclePanel();
        BuildCenterPanel();
        BuildLogPanel();

        Text            = "GenICam 焊点检测系统";
        Size            = new Size(1460, 920);
        MinimumSize     = new Size(1140, 740);
        StartPosition   = FormStartPosition.CenterScreen;
        Font            = new Font("微软雅黑", 9f);
        BackColor       = Color.FromArgb(30, 32, 40);

        // 添加顺序决定 Dock 叠放：Bottom → Left → Right → Fill
        Controls.Add(pnlCenter);
        Controls.Add(pnlCameraControls);
        Controls.Add(pnlRight);
        Controls.Add(pnlLog);

        ResumeLayout(false);
    }

    // ─────────────────────────────────────────────────────────
    // 左侧相机控制栏（245px 宽）
    // ─────────────────────────────────────────────────────────
    private void BuildCameraControlsPanel()
    {
        // GroupBox: 相机设置
        lblProvider = new Label { Text = "厂商:", Location = new Point(8, 22), Size = new Size(40, 18), TextAlign = ContentAlignment.MiddleRight };
        cboProvider = new ComboBox { Location = new Point(52, 20), Size = new Size(168, 23), DropDownStyle = ComboBoxStyle.DropDownList };
        cboProvider.Items.AddRange(new object[] { "华睿 (CXP)", "海康 (GigE)" });
        cboProvider.SelectedIndex = 0;

        lblCtiPath  = new Label { Text = "CTI:", Location = new Point(8, 52), Size = new Size(30, 18), TextAlign = ContentAlignment.MiddleRight };
        txtCtiPath  = new TextBox { Location = new Point(8, 70), Size = new Size(174, 23), Font = new Font("微软雅黑", 7.5f) };
        btnBrowseCti = new Button { Text = "…", Location = new Point(185, 69), Size = new Size(35, 24) };
        btnInitialize = new Button { Text = "初始化 CTI", Location = new Point(8, 100), Size = new Size(104, 26) };
        btnEnumerate  = new Button { Text = "枚举设备", Location = new Point(116, 100), Size = new Size(103, 26), Enabled = false };

        grpCameraSettings = new GroupBox { Text = "相机设置", Location = new Point(4, 4), Size = new Size(233, 138) };
        grpCameraSettings.Controls.AddRange(new Control[] { lblProvider, cboProvider, lblCtiPath, txtCtiPath, btnBrowseCti, btnInitialize, btnEnumerate });

        // GroupBox: 设备列表
        lstDevices   = new ListBox { Location = new Point(8, 22), Size = new Size(217, 58) };
        btnConnect   = new Button  { Text = "连接", Location = new Point(8,   86), Size = new Size(103, 26), Enabled = false };
        btnDisconnect = new Button { Text = "断开", Location = new Point(115, 86), Size = new Size(102, 26), Enabled = false };

        grpDeviceList = new GroupBox { Text = "设备列表", Location = new Point(4, 146), Size = new Size(233, 122) };
        grpDeviceList.Controls.AddRange(new Control[] { lstDevices, btnConnect, btnDisconnect });

        // GroupBox: 采集控制
        lblBufferCount = new Label { Text = "缓冲数:", Location = new Point(8, 22), Size = new Size(52, 18), TextAlign = ContentAlignment.MiddleRight };
        nudBufferCount = new NumericUpDown { Location = new Point(64, 20), Size = new Size(60, 23), Minimum = 1, Maximum = 100, Value = 16 };
        chkAutoSave    = new CheckBox { Text = "自动保存", Location = new Point(138, 22), AutoSize = true };
        btnStartGrab   = new Button { Text = "开始采集", Location = new Point(8,   50), Size = new Size(103, 26), Enabled = false };
        btnStopGrab    = new Button { Text = "停止采集", Location = new Point(115, 50), Size = new Size(102, 26), Enabled = false };

        grpAcquisition = new GroupBox { Text = "采集控制", Location = new Point(4, 272), Size = new Size(233, 90) };
        grpAcquisition.Controls.AddRange(new Control[] { lblBufferCount, nudBufferCount, chkAutoSave, btnStartGrab, btnStopGrab });

        // GroupBox: 曝光控制
        lblExposureUs  = new Label  { Text = "曝光(μs):", Location = new Point(8, 22), Size = new Size(62, 18), TextAlign = ContentAlignment.MiddleRight };
        txtExposureTime = new TextBox { Location = new Point(74, 20), Size = new Size(90, 23), Text = "10000", Enabled = false };
        btnGetExposure = new Button { Text = "读取", Location = new Point(8,  50), Size = new Size(74, 26), Enabled = false };
        btnSetExposure = new Button { Text = "设置", Location = new Point(86, 50), Size = new Size(74, 26), Enabled = false };

        grpExposure = new GroupBox { Text = "曝光控制", Location = new Point(4, 366), Size = new Size(233, 88) };
        grpExposure.Controls.AddRange(new Control[] { lblExposureUs, txtExposureTime, btnGetExposure, btnSetExposure });

        // GroupBox: 状态信息
        int sr = 22, rh = 20;
        lblStateLbl      = MkL("状态:",     8, sr);       lblState      = MkV("就绪", 72, sr,        Color.Gray);
        lblFrameCountLbl = MkL("帧数:",     8, sr + rh);  lblFrameCount = MkV("0",    72, sr + rh,   Color.Black);
        lblFpsLbl        = MkL("FPS:",      8, sr+rh*2);  lblFps        = MkV("0.0",  72, sr+rh*2,  Color.Black);
        lblResolutionLbl = MkL("分辨率:",   8, sr+rh*3);  lblResolution = MkV("--",   72, sr+rh*3,  Color.Black);
        lblPixelFmtLbl   = MkL("像素格式:", 8, sr+rh*4);  lblPixelFmt   = MkV("--",   72, sr+rh*4,  Color.Black);

        grpStatus = new GroupBox { Text = "状态信息", Location = new Point(4, 458), Size = new Size(233, 134) };
        grpStatus.Controls.AddRange(new Control[] {
            lblStateLbl, lblState, lblFrameCountLbl, lblFrameCount,
            lblFpsLbl, lblFps, lblResolutionLbl, lblResolution, lblPixelFmtLbl, lblPixelFmt });

        pnlCameraControls = new Panel
        {
            Width      = 245,
            Dock       = DockStyle.Left,
            AutoScroll = true,
            BackColor  = Color.FromArgb(245, 245, 248),
            Padding    = Padding.Empty
        };
        pnlCameraControls.Controls.AddRange(new Control[] {
            grpCameraSettings, grpDeviceList, grpAcquisition, grpExposure, grpStatus });
    }

    // ─────────────────────────────────────────────────────────
    // 右侧焊点圈圈面板（390px 宽）
    // ─────────────────────────────────────────────────────────
    private void BuildRightCirclePanel()
    {
        lblCircleTitle = new Label
        {
            Text      = "焊点选择",
            Dock      = DockStyle.Top,
            Height    = 32,
            Font      = new Font("微软雅黑", 10f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.FromArgb(40, 100, 180),
            ForeColor = Color.White
        };

        jointCirclePanel = new JointCirclePanel { Dock = DockStyle.Fill };

        // 底部按钮行
        btnSelectAll   = MkBtn("全选",   new Point(8,   10), new Size(80, 28));
        btnDeselectAll = MkBtn("全不选", new Point(96,  10), new Size(80, 28));
        lblCircleCount = new Label
        {
            Text      = "已选: 0 / 0 个焊点",
            Location  = new Point(8, 44),
            Size      = new Size(370, 18),
            ForeColor = Color.FromArgb(100, 100, 100)
        };

        pnlCircleActions = new Panel { Dock = DockStyle.Bottom, Height = 68, BackColor = Color.White, Padding = Padding.Empty };
        pnlCircleActions.Controls.AddRange(new Control[] { btnSelectAll, btnDeselectAll, lblCircleCount });

        pnlRight = new Panel { Width = 390, Dock = DockStyle.Right, BackColor = Color.White };
        pnlRight.Controls.Add(jointCirclePanel);
        pnlRight.Controls.Add(pnlCircleActions);
        pnlRight.Controls.Add(lblCircleTitle);
    }

    // ─────────────────────────────────────────────────────────
    // 中央区域（填充剩余空间）
    // ─────────────────────────────────────────────────────────
    private void BuildCenterPanel()
    {
        // ── 主相机预览 ──
        picCamera = new PictureBox
        {
            Dock      = DockStyle.Fill,
            SizeMode  = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(20, 20, 24),
            Cursor    = Cursors.Cross
        };

        // ── 下方条带 ──
        // 左: 焊点放大图
        picZoom = new PictureBox
        {
            Dock      = DockStyle.Fill,
            SizeMode  = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(15, 15, 20)
        };
        grpZoom = new GroupBox
        {
            Text      = "焊点放大",
            Dock      = DockStyle.Left,
            Width     = 230,
            ForeColor = Color.FromArgb(150, 150, 150),
            BackColor = Color.FromArgb(26, 28, 36),
            Font      = new Font("微软雅黑", 8.5f)
        };
        grpZoom.Controls.Add(picZoom);

        // 右: 操作流程
        grpWorkflow = BuildWorkflowGroup();

        pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 230, BackColor = Color.FromArgb(26, 28, 36) };
        pnlBottom.Controls.Add(grpWorkflow);
        pnlBottom.Controls.Add(grpZoom);

        pnlCenter = new Panel { Dock = DockStyle.Fill };
        pnlCenter.Controls.Add(picCamera);
        pnlCenter.Controls.Add(pnlBottom);
    }

    private GroupBox BuildWorkflowGroup()
    {
        // 步骤标签字体
        var stepFont  = new Font("微软雅黑", 11f, FontStyle.Bold);
        var hintFont  = new Font("微软雅黑", 8.5f);
        var btnFont   = new Font("微软雅黑", 9.5f, FontStyle.Bold);

        // 步骤标签（序号圆圈风格用 Unicode 圆圈数字）
        Label MkStep(string num, string tip, int y) => new Label
        {
            Text      = num,
            Location  = new Point(10, y),
            Size      = new Size(24, 24),
            Font      = stepFont,
            ForeColor = Color.FromArgb(100, 180, 255),
            TextAlign = ContentAlignment.MiddleCenter
        };
        Label MkHint(string t, int x, int y) => new Label
        {
            Text      = t,
            Location  = new Point(x, y),
            AutoSize  = true,
            Font      = hintFont,
            ForeColor = Color.FromArgb(160, 165, 175)
        };

        // ① 拍照
        int row1y = 18;
        var lbl1  = MkStep("①", "拍照", row1y);
        btnCapturePhoto = new Button
        {
            Text      = "拍照",
            Location  = new Point(40, row1y),
            Size      = new Size(90, 28),
            Font      = btnFont,
            BackColor = Color.FromArgb(41, 128, 185),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnCapturePhoto.FlatAppearance.BorderSize = 0;

        // ② 识别焊点
        int row2y = row1y + 36;
        var lbl2  = MkStep("②", "识别", row2y);
        btnDetectJoints = new Button
        {
            Text      = "识别焊点",
            Location  = new Point(40, row2y),
            Size      = new Size(110, 28),
            Font      = btnFont,
            BackColor = Color.FromArgb(39, 174, 96),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnDetectJoints.FlatAppearance.BorderSize = 0;

        // ③ 选择焊点（提示）
        int row3y = row2y + 36;
        var lbl3  = MkStep("③", "选择", row3y);
        var hint3 = MkHint("选择焊点 → 操作右侧圆形界面", 40, row3y + 4);

        // ④ 焊接模式
        int row4y = row3y + 34;
        var lbl4 = MkStep("④", "模式", row4y);

        btnAutoMode = new Button
        {
            Text      = "自动模式",
            Location  = new Point(40, row4y),
            Size      = new Size(96, 28),
            Font      = new Font("微软雅黑", 9f, FontStyle.Bold),
            BackColor = Color.FromArgb(142, 68, 173),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Tag       = "auto"
        };
        btnAutoMode.FlatAppearance.BorderSize = 0;

        btnManualMode = new Button
        {
            Text      = "手动模式",
            Location  = new Point(142, row4y),
            Size      = new Size(96, 28),
            Font      = new Font("微软雅黑", 9f),
            BackColor = Color.FromArgb(52, 55, 65),
            ForeColor = Color.FromArgb(200, 200, 210),
            FlatStyle = FlatStyle.Flat,
            Tag       = "manual"
        };
        btnManualMode.FlatAppearance.BorderSize = 0;

        // ⑤ 开始焊接
        int row5y = row4y + 36;
        var lbl5  = MkStep("⑤", "焊接", row5y);

        btnStartWelding = new Button
        {
            Text      = "▶  开始焊接",
            Location  = new Point(40, row5y),
            Size      = new Size(118, 30),
            Font      = new Font("微软雅黑", 9.5f, FontStyle.Bold),
            BackColor = Color.FromArgb(39, 174, 96),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnStartWelding.FlatAppearance.BorderSize = 0;

        btnPauseWelding = new Button
        {
            Text      = "⏸  暂停",
            Location  = new Point(164, row5y),
            Size      = new Size(90, 30),
            Font      = new Font("微软雅黑", 9f, FontStyle.Bold),
            BackColor = Color.FromArgb(243, 156, 18),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Enabled   = false
        };
        btnPauseWelding.FlatAppearance.BorderSize = 0;

        btnStopWelding = new Button
        {
            Text      = "⏹  停止",
            Location  = new Point(260, row5y),
            Size      = new Size(90, 30),
            Font      = new Font("微软雅黑", 9f, FontStyle.Bold),
            BackColor = Color.FromArgb(231, 76, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Enabled   = false
        };
        btnStopWelding.FlatAppearance.BorderSize = 0;

        // 焊接状态标签（⑤行右侧）
        lblWeldStatus = new Label
        {
            Text      = "就绪",
            Location  = new Point(356, row5y + 5),
            Size      = new Size(110, 22),
            Font      = new Font("微软雅黑", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(130, 130, 140)
        };

        var grp = new GroupBox
        {
            Text      = "操作流程",
            Dock      = DockStyle.Fill,
            ForeColor = Color.FromArgb(150, 150, 165),
            BackColor = Color.FromArgb(30, 32, 40),
            Font      = new Font("微软雅黑", 8.5f)
        };
        grp.Controls.AddRange(new Control[] {
            lbl1, btnCapturePhoto,
            lbl2, btnDetectJoints,
            lbl3, hint3,
            lbl4, btnAutoMode, btnManualMode,
            lbl5, btnStartWelding, btnPauseWelding, btnStopWelding, lblWeldStatus
        });
        return grp;
    }

    // ─────────────────────────────────────────────────────────
    // 日志区
    // ─────────────────────────────────────────────────────────
    private void BuildLogPanel()
    {
        btnClearLog = new Button
        {
            Text      = "清除",
            Dock      = DockStyle.Right,
            Width     = 56,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.FromArgb(180, 180, 180),
            BackColor = Color.FromArgb(35, 35, 40)
        };
        txtLog = new TextBox
        {
            Dock        = DockStyle.Fill,
            Multiline   = true,
            ReadOnly    = true,
            ScrollBars  = ScrollBars.Vertical,
            BackColor   = Color.FromArgb(18, 18, 22),
            ForeColor   = Color.FromArgb(170, 210, 170),
            Font        = new Font("Consolas", 8.5f),
            BorderStyle = BorderStyle.None
        };
        pnlLog = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 120,
            BackColor = Color.FromArgb(18, 18, 22)
        };
        pnlLog.Controls.Add(txtLog);
        pnlLog.Controls.Add(btnClearLog);
    }

    // ─────────────────────────────────────────────────────────
    // 工厂辅助
    // ─────────────────────────────────────────────────────────
    private static Label MkL(string text, int x, int y) =>
        new Label { Text = text, Location = new Point(x, y), Size = new Size(64, 18), TextAlign = ContentAlignment.MiddleRight, Font = new Font("微软雅黑", 8.5f) };

    private static Label MkV(string text, int x, int y, Color color) =>
        new Label { Text = text, Location = new Point(x, y), Size = new Size(158, 18), ForeColor = color, Font = new Font("微软雅黑", 8.5f, FontStyle.Bold) };

    private static Button MkBtn(string text, Point loc, Size size) =>
        new Button { Text = text, Location = loc, Size = size, FlatStyle = FlatStyle.Flat };
}
