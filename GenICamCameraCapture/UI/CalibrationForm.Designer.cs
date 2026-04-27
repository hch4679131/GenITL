namespace GenICamCameraCapture.UI;

partial class CalibrationForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this.pnlPreview = new System.Windows.Forms.Panel();
        this.lblPreviewTitle = new System.Windows.Forms.Label();
        this.picCalib = new System.Windows.Forms.PictureBox();
        this.pnlPreviewBot = new System.Windows.Forms.Panel();
        this.btnGrabFrame = new System.Windows.Forms.Button();
        this.lblClickHint = new System.Windows.Forms.Label();
        this.pnlControls = new System.Windows.Forms.Panel();
        this.grpTcp = new System.Windows.Forms.GroupBox();
        this.rdoClient = new System.Windows.Forms.RadioButton();
        this.rdoServer = new System.Windows.Forms.RadioButton();
        this.lblIp = new System.Windows.Forms.Label();
        this.txtIp = new System.Windows.Forms.TextBox();
        this.lblPort = new System.Windows.Forms.Label();
        this.txtPort = new System.Windows.Forms.TextBox();
        this.btnTcpConnect = new System.Windows.Forms.Button();
        this.btnTcpDisconnect = new System.Windows.Forms.Button();
        this.lblConnStatus = new System.Windows.Forms.Label();
        this.grpPoints = new System.Windows.Forms.GroupBox();
        this.dgvPoints = new System.Windows.Forms.DataGridView();
        this.ColIdx = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.ColRobotX = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.ColRobotY = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.ColPixelX = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.ColPixelY = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.ColStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.pnlPointBtns = new System.Windows.Forms.Panel();
        this.btnMoveToPoint = new System.Windows.Forms.Button();
        this.btnMarkPoint = new System.Windows.Forms.Button();
        this.btnClearPoint = new System.Windows.Forms.Button();
        this.grpResult = new System.Windows.Forms.GroupBox();
        this.btnCompute = new System.Windows.Forms.Button();
        this.btnSaveApply = new System.Windows.Forms.Button();
        this.lblRms = new System.Windows.Forms.Label();
        this.txtTcpLog = new System.Windows.Forms.TextBox();
        this.pnlPreview.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.picCalib)).BeginInit();
        this.pnlPreviewBot.SuspendLayout();
        this.pnlControls.SuspendLayout();
        this.grpTcp.SuspendLayout();
        this.grpPoints.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvPoints)).BeginInit();
        this.pnlPointBtns.SuspendLayout();
        this.grpResult.SuspendLayout();
        this.SuspendLayout();
        //
        // lblPreviewTitle
        //
        this.lblPreviewTitle.BackColor = Color.FromArgb(50, 80, 140);
        this.lblPreviewTitle.Dock = DockStyle.Top;
        this.lblPreviewTitle.Font = new Font("微软雅黑", 9f, FontStyle.Bold);
        this.lblPreviewTitle.ForeColor = Color.White;
        this.lblPreviewTitle.Height = 28;
        this.lblPreviewTitle.Text = "相机预览  （标记模式下点击图像记录像素坐标）";
        this.lblPreviewTitle.TextAlign = ContentAlignment.MiddleCenter;
        //
        // picCalib
        //
        this.picCalib.BackColor = Color.FromArgb(18, 18, 24);
        this.picCalib.Cursor = Cursors.Default;
        this.picCalib.Dock = DockStyle.Fill;
        this.picCalib.SizeMode = PictureBoxSizeMode.Zoom;
        //
        // lblClickHint
        //
        this.lblClickHint.Dock = DockStyle.Fill;
        this.lblClickHint.Font = new Font("微软雅黑", 8.5f);
        this.lblClickHint.ForeColor = Color.FromArgb(140, 140, 150);
        this.lblClickHint.Text = "请先抓取图像";
        this.lblClickHint.TextAlign = ContentAlignment.MiddleLeft;
        //
        // btnGrabFrame
        //
        this.btnGrabFrame.BackColor = Color.FromArgb(41, 128, 185);
        this.btnGrabFrame.Dock = DockStyle.Right;
        this.btnGrabFrame.FlatStyle = FlatStyle.Flat;
        this.btnGrabFrame.ForeColor = Color.White;
        this.btnGrabFrame.Text = "抓取当前帧";
        this.btnGrabFrame.Width = 110;
        this.btnGrabFrame.FlatAppearance.BorderSize = 0;
        //
        // pnlPreviewBot
        //
        this.pnlPreviewBot.BackColor = Color.FromArgb(28, 30, 38);
        this.pnlPreviewBot.Dock = DockStyle.Bottom;
        this.pnlPreviewBot.Height = 36;
        this.pnlPreviewBot.Padding = new Padding(4);
        this.pnlPreviewBot.Controls.Add(this.lblClickHint);
        this.pnlPreviewBot.Controls.Add(this.btnGrabFrame);
        //
        // pnlPreview
        //
        this.pnlPreview.BackColor = Color.FromArgb(18, 18, 24);
        this.pnlPreview.Dock = DockStyle.Left;
        this.pnlPreview.Width = 440;
        this.pnlPreview.Controls.Add(this.picCalib);
        this.pnlPreview.Controls.Add(this.pnlPreviewBot);
        this.pnlPreview.Controls.Add(this.lblPreviewTitle);
        //
        // rdoClient
        //
        this.rdoClient.AutoSize = true;
        this.rdoClient.Checked = true;
        this.rdoClient.ForeColor = Color.White;
        this.rdoClient.Location = new Point(8, 18);
        this.rdoClient.TabStop = true;
        this.rdoClient.Text = "客户端（连接机器人）";
        //
        // rdoServer
        //
        this.rdoServer.AutoSize = true;
        this.rdoServer.ForeColor = Color.White;
        this.rdoServer.Location = new Point(8, 38);
        this.rdoServer.Text = "服务端（等待机器人）";
        //
        // lblIp
        //
        this.lblIp.ForeColor = Color.Silver;
        this.lblIp.Location = new Point(8, 62);
        this.lblIp.Size = new Size(66, 22);
        this.lblIp.Text = "IP / 主机:";
        this.lblIp.TextAlign = ContentAlignment.MiddleRight;
        //
        // txtIp
        //
        this.txtIp.Location = new Point(78, 61);
        this.txtIp.Size = new Size(130, 22);
        this.txtIp.Text = "192.168.1.100";
        //
        // lblPort
        //
        this.lblPort.ForeColor = Color.Silver;
        this.lblPort.Location = new Point(215, 62);
        this.lblPort.Size = new Size(40, 22);
        this.lblPort.Text = "端口:";
        this.lblPort.TextAlign = ContentAlignment.MiddleRight;
        //
        // txtPort
        //
        this.txtPort.Location = new Point(258, 61);
        this.txtPort.Size = new Size(62, 22);
        this.txtPort.Text = "9999";
        //
        // btnTcpConnect
        //
        this.btnTcpConnect.BackColor = Color.FromArgb(39, 174, 96);
        this.btnTcpConnect.FlatStyle = FlatStyle.Flat;
        this.btnTcpConnect.Font = new Font("微软雅黑", 9f);
        this.btnTcpConnect.ForeColor = Color.White;
        this.btnTcpConnect.Location = new Point(8, 90);
        this.btnTcpConnect.Size = new Size(110, 26);
        this.btnTcpConnect.Text = "连接 / 监听";
        this.btnTcpConnect.FlatAppearance.BorderSize = 0;
        //
        // btnTcpDisconnect
        //
        this.btnTcpDisconnect.BackColor = Color.FromArgb(231, 76, 60);
        this.btnTcpDisconnect.Enabled = false;
        this.btnTcpDisconnect.FlatStyle = FlatStyle.Flat;
        this.btnTcpDisconnect.Font = new Font("微软雅黑", 9f);
        this.btnTcpDisconnect.ForeColor = Color.White;
        this.btnTcpDisconnect.Location = new Point(124, 90);
        this.btnTcpDisconnect.Size = new Size(80, 26);
        this.btnTcpDisconnect.Text = "断开";
        this.btnTcpDisconnect.FlatAppearance.BorderSize = 0;
        //
        // lblConnStatus
        //
        this.lblConnStatus.Font = new Font("微软雅黑", 9f, FontStyle.Bold);
        this.lblConnStatus.ForeColor = Color.Gray;
        this.lblConnStatus.Location = new Point(212, 94);
        this.lblConnStatus.Size = new Size(120, 22);
        this.lblConnStatus.Text = "● 未连接";
        //
        // grpTcp
        //
        this.grpTcp.BackColor = Color.FromArgb(36, 38, 46);
        this.grpTcp.Dock = DockStyle.Top;
        this.grpTcp.ForeColor = Color.Silver;
        this.grpTcp.Height = 126;
        this.grpTcp.Text = "TCP 连接（机器人通讯）";
        this.grpTcp.Controls.AddRange(new Control[] {
            this.rdoClient, this.rdoServer,
            this.lblIp, this.txtIp, this.lblPort, this.txtPort,
            this.btnTcpConnect, this.btnTcpDisconnect, this.lblConnStatus });
        //
        // ColIdx
        //
        this.ColIdx.FillWeight = 25f;
        this.ColIdx.HeaderText = "#";
        this.ColIdx.Name = "ColIdx";
        this.ColIdx.ReadOnly = true;
        //
        // ColRobotX
        //
        this.ColRobotX.FillWeight = 120f;
        this.ColRobotX.HeaderText = "机器人X(mm)";
        this.ColRobotX.Name = "ColRobotX";
        //
        // ColRobotY
        //
        this.ColRobotY.FillWeight = 120f;
        this.ColRobotY.HeaderText = "机器人Y(mm)";
        this.ColRobotY.Name = "ColRobotY";
        //
        // ColPixelX
        //
        this.ColPixelX.FillWeight = 90f;
        this.ColPixelX.HeaderText = "像素X";
        this.ColPixelX.Name = "ColPixelX";
        this.ColPixelX.ReadOnly = true;
        //
        // ColPixelY
        //
        this.ColPixelY.FillWeight = 90f;
        this.ColPixelY.HeaderText = "像素Y";
        this.ColPixelY.Name = "ColPixelY";
        this.ColPixelY.ReadOnly = true;
        //
        // ColStatus
        //
        this.ColStatus.FillWeight = 70f;
        this.ColStatus.HeaderText = "状态";
        this.ColStatus.Name = "ColStatus";
        this.ColStatus.ReadOnly = true;
        //
        // dgvPoints
        //
        this.dgvPoints.AllowUserToAddRows = false;
        this.dgvPoints.AllowUserToDeleteRows = false;
        this.dgvPoints.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvPoints.BackgroundColor = Color.FromArgb(28, 30, 38);
        this.dgvPoints.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 44, 56);
        this.dgvPoints.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 9f, FontStyle.Bold);
        this.dgvPoints.ColumnHeadersDefaultCellStyle.ForeColor = Color.Silver;
        this.dgvPoints.Columns.AddRange(new DataGridViewColumn[] {
            this.ColIdx, this.ColRobotX, this.ColRobotY,
            this.ColPixelX, this.ColPixelY, this.ColStatus });
        this.dgvPoints.DefaultCellStyle.BackColor = Color.FromArgb(28, 30, 38);
        this.dgvPoints.DefaultCellStyle.ForeColor = Color.White;
        this.dgvPoints.DefaultCellStyle.SelectionBackColor = Color.FromArgb(50, 110, 180);
        this.dgvPoints.DefaultCellStyle.SelectionForeColor = Color.White;
        this.dgvPoints.Dock = DockStyle.Fill;
        this.dgvPoints.EnableHeadersVisualStyles = false;
        this.dgvPoints.Font = new Font("微软雅黑", 9f);
        this.dgvPoints.ForeColor = Color.White;
        this.dgvPoints.GridColor = Color.FromArgb(55, 58, 70);
        this.dgvPoints.MultiSelect = false;
        this.dgvPoints.RowHeadersVisible = false;
        this.dgvPoints.RowTemplate.Height = 26;
        this.dgvPoints.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        //
        // btnMoveToPoint
        //
        this.btnMoveToPoint.BackColor = Color.FromArgb(41, 128, 185);
        this.btnMoveToPoint.FlatStyle = FlatStyle.Flat;
        this.btnMoveToPoint.Font = new Font("微软雅黑", 9f);
        this.btnMoveToPoint.ForeColor = Color.White;
        this.btnMoveToPoint.Location = new Point(4, 6);
        this.btnMoveToPoint.Size = new Size(160, 28);
        this.btnMoveToPoint.Text = "① 移动机器人到此点";
        this.btnMoveToPoint.FlatAppearance.BorderSize = 0;
        //
        // btnMarkPoint
        //
        this.btnMarkPoint.BackColor = Color.FromArgb(142, 68, 173);
        this.btnMarkPoint.FlatStyle = FlatStyle.Flat;
        this.btnMarkPoint.Font = new Font("微软雅黑", 9f);
        this.btnMarkPoint.ForeColor = Color.White;
        this.btnMarkPoint.Location = new Point(170, 6);
        this.btnMarkPoint.Size = new Size(160, 28);
        this.btnMarkPoint.Text = "② 标记此点像素坐标";
        this.btnMarkPoint.FlatAppearance.BorderSize = 0;
        //
        // btnClearPoint
        //
        this.btnClearPoint.BackColor = Color.FromArgb(80, 80, 90);
        this.btnClearPoint.FlatStyle = FlatStyle.Flat;
        this.btnClearPoint.Font = new Font("微软雅黑", 9f);
        this.btnClearPoint.ForeColor = Color.White;
        this.btnClearPoint.Location = new Point(336, 6);
        this.btnClearPoint.Size = new Size(80, 28);
        this.btnClearPoint.Text = "清除此点";
        this.btnClearPoint.FlatAppearance.BorderSize = 0;
        //
        // pnlPointBtns
        //
        this.pnlPointBtns.BackColor = Color.FromArgb(30, 32, 40);
        this.pnlPointBtns.Dock = DockStyle.Bottom;
        this.pnlPointBtns.Height = 42;
        this.pnlPointBtns.Controls.AddRange(new Control[] {
            this.btnMoveToPoint, this.btnMarkPoint, this.btnClearPoint });
        //
        // grpPoints
        //
        this.grpPoints.BackColor = Color.FromArgb(36, 38, 46);
        this.grpPoints.Dock = DockStyle.Fill;
        this.grpPoints.ForeColor = Color.Silver;
        this.grpPoints.Text = "标定点（双击机器人坐标列可直接编辑）";
        this.grpPoints.Controls.Add(this.dgvPoints);
        this.grpPoints.Controls.Add(this.pnlPointBtns);
        //
        // btnCompute
        //
        this.btnCompute.BackColor = Color.FromArgb(39, 174, 96);
        this.btnCompute.FlatStyle = FlatStyle.Flat;
        this.btnCompute.Font = new Font("微软雅黑", 9f);
        this.btnCompute.ForeColor = Color.White;
        this.btnCompute.Location = new Point(8, 18);
        this.btnCompute.Size = new Size(130, 30);
        this.btnCompute.Text = "计算标定矩阵";
        this.btnCompute.FlatAppearance.BorderSize = 0;
        //
        // btnSaveApply
        //
        this.btnSaveApply.BackColor = Color.FromArgb(243, 156, 18);
        this.btnSaveApply.Enabled = false;
        this.btnSaveApply.FlatStyle = FlatStyle.Flat;
        this.btnSaveApply.Font = new Font("微软雅黑", 9f);
        this.btnSaveApply.ForeColor = Color.White;
        this.btnSaveApply.Location = new Point(146, 18);
        this.btnSaveApply.Size = new Size(110, 30);
        this.btnSaveApply.Text = "保存并应用";
        this.btnSaveApply.FlatAppearance.BorderSize = 0;
        //
        // lblRms
        //
        this.lblRms.Font = new Font("微软雅黑", 9f, FontStyle.Bold);
        this.lblRms.ForeColor = Color.FromArgb(100, 210, 150);
        this.lblRms.Location = new Point(268, 22);
        this.lblRms.Size = new Size(180, 24);
        this.lblRms.Text = "RMS误差: --";
        //
        // txtTcpLog
        //
        this.txtTcpLog.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
        this.txtTcpLog.BackColor = Color.FromArgb(18, 18, 22);
        this.txtTcpLog.BorderStyle = BorderStyle.None;
        this.txtTcpLog.Font = new Font("Consolas", 8f);
        this.txtTcpLog.ForeColor = Color.FromArgb(160, 210, 160);
        this.txtTcpLog.Location = new Point(8, 54);
        this.txtTcpLog.Multiline = true;
        this.txtTcpLog.ReadOnly = true;
        this.txtTcpLog.ScrollBars = ScrollBars.Vertical;
        this.txtTcpLog.Size = new Size(560, 62);
        //
        // grpResult
        //
        this.grpResult.BackColor = Color.FromArgb(36, 38, 46);
        this.grpResult.Dock = DockStyle.Bottom;
        this.grpResult.ForeColor = Color.Silver;
        this.grpResult.Height = 128;
        this.grpResult.Text = "标定结果 / 通讯日志";
        this.grpResult.Controls.AddRange(new Control[] {
            this.btnCompute, this.btnSaveApply, this.lblRms, this.txtTcpLog });
        //
        // pnlControls
        //
        this.pnlControls.BackColor = Color.FromArgb(36, 38, 46);
        this.pnlControls.Dock = DockStyle.Fill;
        this.pnlControls.Padding = new Padding(4);
        this.pnlControls.Controls.Add(this.grpPoints);
        this.pnlControls.Controls.Add(this.grpResult);
        this.pnlControls.Controls.Add(this.grpTcp);
        //
        // CalibrationForm
        //
        this.BackColor = Color.FromArgb(36, 38, 46);
        this.Font = new Font("微软雅黑", 9f);
        this.MinimumSize = new Size(860, 580);
        this.Size = new Size(1020, 700);
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "9点标定";
        // DockFill added first, DockLeft added last
        this.Controls.Add(this.pnlControls);
        this.Controls.Add(this.pnlPreview);
        //
        this.pnlPreviewBot.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.picCalib)).EndInit();
        this.pnlPreview.ResumeLayout(false);
        this.grpTcp.ResumeLayout(false);
        this.grpTcp.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvPoints)).EndInit();
        this.pnlPointBtns.ResumeLayout(false);
        this.grpPoints.ResumeLayout(false);
        this.grpResult.ResumeLayout(false);
        this.grpResult.PerformLayout();
        this.pnlControls.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    // ── 左侧预览 ──────────────────────────────────────────────
    private System.Windows.Forms.Panel      pnlPreview      = null!;
    private System.Windows.Forms.Label      lblPreviewTitle = null!;
    private System.Windows.Forms.PictureBox picCalib        = null!;
    private System.Windows.Forms.Panel      pnlPreviewBot   = null!;
    private System.Windows.Forms.Button     btnGrabFrame    = null!;
    private System.Windows.Forms.Label      lblClickHint    = null!;
    // ── 右侧控制 ──────────────────────────────────────────────
    private System.Windows.Forms.Panel      pnlControls     = null!;
    // TCP 组
    private System.Windows.Forms.GroupBox    grpTcp          = null!;
    private System.Windows.Forms.RadioButton rdoClient       = null!;
    private System.Windows.Forms.RadioButton rdoServer       = null!;
    private System.Windows.Forms.Label       lblIp           = null!;
    private System.Windows.Forms.TextBox     txtIp           = null!;
    private System.Windows.Forms.Label       lblPort         = null!;
    private System.Windows.Forms.TextBox     txtPort         = null!;
    private System.Windows.Forms.Button      btnTcpConnect   = null!;
    private System.Windows.Forms.Button      btnTcpDisconnect = null!;
    private System.Windows.Forms.Label       lblConnStatus   = null!;
    // 标定点表格
    private System.Windows.Forms.GroupBox                  grpPoints    = null!;
    private System.Windows.Forms.DataGridView              dgvPoints    = null!;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColIdx       = null!;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColRobotX    = null!;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColRobotY    = null!;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColPixelX    = null!;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColPixelY    = null!;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColStatus    = null!;
    private System.Windows.Forms.Panel                     pnlPointBtns = null!;
    private System.Windows.Forms.Button                    btnMoveToPoint = null!;
    private System.Windows.Forms.Button                    btnMarkPoint   = null!;
    private System.Windows.Forms.Button                    btnClearPoint  = null!;
    // 结果与日志
    private System.Windows.Forms.GroupBox grpResult    = null!;
    private System.Windows.Forms.Button   btnCompute   = null!;
    private System.Windows.Forms.Button   btnSaveApply = null!;
    private System.Windows.Forms.Label    lblRms       = null!;
    private System.Windows.Forms.TextBox  txtTcpLog    = null!;
}
