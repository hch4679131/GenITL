namespace GenICamCameraCapture.UI;

partial class CalibrationForm
{
    // ── 左侧预览 ──────────────────────────────────────────────
    private Panel      pnlPreview      = null!;
    private Label      lblPreviewTitle = null!;
    private PictureBox picCalib        = null!;
    private Panel      pnlPreviewBot   = null!;
    private Button     btnGrabFrame    = null!;
    private Label      lblClickHint    = null!;

    // ── 右侧控制 ──────────────────────────────────────────────
    private Panel    pnlControls     = null!;

    // TCP 组
    private GroupBox grpTcp          = null!;
    private RadioButton rdoClient    = null!;
    private RadioButton rdoServer    = null!;
    private Label    lblIp           = null!;
    private TextBox  txtIp           = null!;
    private Label    lblPort         = null!;
    private TextBox  txtPort         = null!;
    private Button   btnTcpConnect   = null!;
    private Button   btnTcpDisconnect = null!;
    private Label    lblConnStatus   = null!;

    // 标定点表格
    private GroupBox     grpPoints   = null!;
    private DataGridView dgvPoints   = null!;
    private Panel        pnlPointBtns = null!;
    private Button       btnMoveToPoint = null!;
    private Button       btnMarkPoint   = null!;
    private Button       btnClearPoint  = null!;

    // 结果与日志
    private GroupBox grpResult      = null!;
    private Button   btnCompute     = null!;
    private Button   btnSaveApply   = null!;
    private Label    lblRms         = null!;
    private TextBox  txtTcpLog      = null!;

    private void InitializeComponent()
    {
        SuspendLayout();

        BuildPreviewPanel();
        BuildControlsPanel();

        Text          = "9点标定";
        Size          = new Size(1020, 700);
        MinimumSize   = new Size(860, 580);
        StartPosition = FormStartPosition.CenterParent;
        Font          = new Font("微软雅黑", 9f);
        BackColor     = Color.FromArgb(36, 38, 46);

        Controls.Add(pnlPreview);    // DockLeft
        Controls.Add(pnlControls);  // DockFill

        ResumeLayout(false);
    }

    // ─────────────────────────────────────────────────────────
    // 左侧相机预览（440px）
    // ─────────────────────────────────────────────────────────
    private void BuildPreviewPanel()
    {
        lblPreviewTitle = new Label
        {
            Text      = "相机预览  （标记模式下点击图像记录像素坐标）",
            Dock      = DockStyle.Top,
            Height    = 28,
            TextAlign = ContentAlignment.MiddleCenter,
            Font      = new Font("微软雅黑", 9f, FontStyle.Bold),
            BackColor = Color.FromArgb(50, 80, 140),
            ForeColor = Color.White
        };

        picCalib = new PictureBox
        {
            Dock      = DockStyle.Fill,
            SizeMode  = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(18, 18, 24),
            Cursor    = Cursors.Default
        };

        lblClickHint = new Label
        {
            Text      = "请先抓取图像",
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(140, 140, 150),
            Font      = new Font("微软雅黑", 8.5f)
        };

        btnGrabFrame = new Button
        {
            Text      = "抓取当前帧",
            Dock      = DockStyle.Right,
            Width     = 110,
            BackColor = Color.FromArgb(41, 128, 185),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnGrabFrame.FlatAppearance.BorderSize = 0;

        pnlPreviewBot = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 36,
            BackColor = Color.FromArgb(28, 30, 38),
            Padding   = new Padding(4, 4, 4, 4)
        };
        pnlPreviewBot.Controls.Add(lblClickHint);
        pnlPreviewBot.Controls.Add(btnGrabFrame);

        pnlPreview = new Panel
        {
            Width     = 440,
            Dock      = DockStyle.Left,
            BackColor = Color.FromArgb(18, 18, 24)
        };
        pnlPreview.Controls.Add(picCalib);
        pnlPreview.Controls.Add(pnlPreviewBot);
        pnlPreview.Controls.Add(lblPreviewTitle);
    }

    // ─────────────────────────────────────────────────────────
    // 右侧控制区
    // ─────────────────────────────────────────────────────────
    private void BuildControlsPanel()
    {
        BuildTcpGroup();
        BuildPointsGroup();
        BuildResultGroup();

        pnlControls = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.FromArgb(36, 38, 46),
            Padding   = new Padding(4)
        };
        pnlControls.Controls.Add(grpPoints);   // Fill
        pnlControls.Controls.Add(grpResult);   // Bottom
        pnlControls.Controls.Add(grpTcp);      // Top
    }

    private void BuildTcpGroup()
    {
        rdoClient = new RadioButton { Text = "客户端（连接机器人）", Location = new Point(8,  18), AutoSize = true, Checked = true, ForeColor = Color.White };
        rdoServer = new RadioButton { Text = "服务端（等待机器人）", Location = new Point(8,  38), AutoSize = true, ForeColor = Color.White };

        lblIp   = new Label  { Text = "IP / 主机:", Location = new Point(8,  62), Size = new Size(66, 22), TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.Silver };
        txtIp   = new TextBox { Text = "192.168.1.100", Location = new Point(78, 61), Size = new Size(130, 22) };
        lblPort = new Label  { Text = "端口:",    Location = new Point(215, 62), Size = new Size(40, 22), TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.Silver };
        txtPort = new TextBox { Text = "9999",     Location = new Point(258, 61), Size = new Size(62, 22) };

        btnTcpConnect    = MkBtn("连接 / 监听", new Point(8,   90), new Size(110, 26), Color.FromArgb(39, 174, 96));
        btnTcpDisconnect = MkBtn("断开",        new Point(124, 90), new Size(80,  26), Color.FromArgb(231, 76, 60));
        btnTcpDisconnect.Enabled = false;

        lblConnStatus = new Label
        {
            Text      = "● 未连接",
            Location  = new Point(212, 94),
            Size      = new Size(120, 22),
            ForeColor = Color.Gray,
            Font      = new Font("微软雅黑", 9f, FontStyle.Bold)
        };

        grpTcp = new GroupBox
        {
            Text      = "TCP 连接（机器人通讯）",
            Dock      = DockStyle.Top,
            Height    = 126,
            ForeColor = Color.Silver,
            BackColor = Color.FromArgb(36, 38, 46)
        };
        grpTcp.Controls.AddRange(new Control[] {
            rdoClient, rdoServer, lblIp, txtIp, lblPort, txtPort,
            btnTcpConnect, btnTcpDisconnect, lblConnStatus });
    }

    private void BuildPointsGroup()
    {
        // DataGridView
        dgvPoints = new DataGridView
        {
            Dock                  = DockStyle.Fill,
            AllowUserToAddRows    = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible     = false,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect           = false,
            BackgroundColor       = Color.FromArgb(28, 30, 38),
            ForeColor             = Color.White,
            GridColor             = Color.FromArgb(55, 58, 70),
            Font                  = new Font("微软雅黑", 9f),
            RowTemplate           = { Height = 26 }
        };
        dgvPoints.DefaultCellStyle.BackColor         = Color.FromArgb(28, 30, 38);
        dgvPoints.DefaultCellStyle.ForeColor         = Color.White;
        dgvPoints.DefaultCellStyle.SelectionBackColor = Color.FromArgb(50, 110, 180);
        dgvPoints.DefaultCellStyle.SelectionForeColor = Color.White;
        dgvPoints.ColumnHeadersDefaultCellStyle.BackColor  = Color.FromArgb(40, 44, 56);
        dgvPoints.ColumnHeadersDefaultCellStyle.ForeColor  = Color.Silver;
        dgvPoints.ColumnHeadersDefaultCellStyle.Font       = new Font("微软雅黑", 9f, FontStyle.Bold);
        dgvPoints.EnableHeadersVisualStyles = false;

        dgvPoints.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColIdx",    HeaderText = "#",        ReadOnly = true,  FillWeight = 25 });
        dgvPoints.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColRobotX", HeaderText = "机器人X(mm)", ReadOnly = false, FillWeight = 120 });
        dgvPoints.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColRobotY", HeaderText = "机器人Y(mm)", ReadOnly = false, FillWeight = 120 });
        dgvPoints.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColPixelX", HeaderText = "像素X",      ReadOnly = true,  FillWeight = 90 });
        dgvPoints.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColPixelY", HeaderText = "像素Y",      ReadOnly = true,  FillWeight = 90 });
        dgvPoints.Columns.Add(new DataGridViewTextBoxColumn { Name = "ColStatus", HeaderText = "状态",       ReadOnly = true,  FillWeight = 70 });

        // 操作按钮行
        btnMoveToPoint = MkBtn("① 移动机器人到此点", new Point(4,   6), new Size(160, 28), Color.FromArgb(41, 128, 185));
        btnMarkPoint   = MkBtn("② 标记此点像素坐标", new Point(170, 6), new Size(160, 28), Color.FromArgb(142, 68, 173));
        btnClearPoint  = MkBtn("清除此点",            new Point(336, 6), new Size(80,  28), Color.FromArgb(80, 80, 90));

        pnlPointBtns = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 42,
            BackColor = Color.FromArgb(30, 32, 40),
            Padding   = new Padding(0)
        };
        pnlPointBtns.Controls.AddRange(new Control[] { btnMoveToPoint, btnMarkPoint, btnClearPoint });

        grpPoints = new GroupBox
        {
            Text      = "标定点（双击机器人坐标列可直接编辑）",
            Dock      = DockStyle.Fill,
            ForeColor = Color.Silver,
            BackColor = Color.FromArgb(36, 38, 46)
        };
        grpPoints.Controls.Add(dgvPoints);
        grpPoints.Controls.Add(pnlPointBtns);
    }

    private void BuildResultGroup()
    {
        btnCompute   = MkBtn("计算标定矩阵",   new Point(8,   18), new Size(130, 30), Color.FromArgb(39, 174, 96));
        btnSaveApply = MkBtn("保存并应用",      new Point(146, 18), new Size(110, 30), Color.FromArgb(243, 156, 18));
        btnSaveApply.Enabled = false;

        lblRms = new Label
        {
            Text      = "RMS误差: --",
            Location  = new Point(268, 22),
            Size      = new Size(180, 24),
            ForeColor = Color.FromArgb(100, 210, 150),
            Font      = new Font("微软雅黑", 9f, FontStyle.Bold)
        };

        txtTcpLog = new TextBox
        {
            Location    = new Point(8, 54),
            Size        = new Size(560, 62),
            Multiline   = true,
            ReadOnly    = true,
            ScrollBars  = ScrollBars.Vertical,
            BackColor   = Color.FromArgb(18, 18, 22),
            ForeColor   = Color.FromArgb(160, 210, 160),
            Font        = new Font("Consolas", 8f),
            BorderStyle = BorderStyle.None,
            Anchor      = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
        };

        grpResult = new GroupBox
        {
            Text      = "标定结果 / 通讯日志",
            Dock      = DockStyle.Bottom,
            Height    = 128,
            ForeColor = Color.Silver,
            BackColor = Color.FromArgb(36, 38, 46)
        };
        grpResult.Controls.AddRange(new Control[] { btnCompute, btnSaveApply, lblRms, txtTcpLog });
        grpResult.Resize += (_, _) =>
            txtTcpLog.Size = new Size(grpResult.Width - 16, grpResult.Height - 62);
    }

    private static Button MkBtn(string text, Point loc, Size sz, Color bg)
    {
        var b = new Button
        {
            Text      = text,
            Location  = loc,
            Size      = sz,
            BackColor = bg,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("微软雅黑", 9f)
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }
}
