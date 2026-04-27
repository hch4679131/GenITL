namespace GenICamCameraCapture.UI;

partial class MainForm
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
        this.pnlCameraControls = new System.Windows.Forms.Panel();
        this.grpCameraSettings = new System.Windows.Forms.GroupBox();
        this.lblProvider = new System.Windows.Forms.Label();
        this.cboProvider = new System.Windows.Forms.ComboBox();
        this.lblCtiPath = new System.Windows.Forms.Label();
        this.txtCtiPath = new System.Windows.Forms.TextBox();
        this.btnBrowseCti = new System.Windows.Forms.Button();
        this.btnInitialize = new System.Windows.Forms.Button();
        this.btnEnumerate = new System.Windows.Forms.Button();
        this.grpDeviceList = new System.Windows.Forms.GroupBox();
        this.lstDevices = new System.Windows.Forms.ListBox();
        this.btnConnect = new System.Windows.Forms.Button();
        this.btnDisconnect = new System.Windows.Forms.Button();
        this.grpAcquisition = new System.Windows.Forms.GroupBox();
        this.lblBufferCount = new System.Windows.Forms.Label();
        this.nudBufferCount = new System.Windows.Forms.NumericUpDown();
        this.chkAutoSave = new System.Windows.Forms.CheckBox();
        this.btnStartGrab = new System.Windows.Forms.Button();
        this.btnStopGrab = new System.Windows.Forms.Button();
        this.grpExposure = new System.Windows.Forms.GroupBox();
        this.lblExposureUs = new System.Windows.Forms.Label();
        this.txtExposureTime = new System.Windows.Forms.TextBox();
        this.btnGetExposure = new System.Windows.Forms.Button();
        this.btnSetExposure = new System.Windows.Forms.Button();
        this.grpStatus = new System.Windows.Forms.GroupBox();
        this.lblStateLbl = new System.Windows.Forms.Label();
        this.lblState = new System.Windows.Forms.Label();
        this.lblFrameCountLbl = new System.Windows.Forms.Label();
        this.lblFrameCount = new System.Windows.Forms.Label();
        this.lblFpsLbl = new System.Windows.Forms.Label();
        this.lblFps = new System.Windows.Forms.Label();
        this.lblResolutionLbl = new System.Windows.Forms.Label();
        this.lblResolution = new System.Windows.Forms.Label();
        this.lblPixelFmtLbl = new System.Windows.Forms.Label();
        this.lblPixelFmt = new System.Windows.Forms.Label();
        this.btnOpenCalib = new System.Windows.Forms.Button();
        this.lblCalibStatus = new System.Windows.Forms.Label();
        this.pnlRight = new System.Windows.Forms.Panel();
        this.lblCircleTitle = new System.Windows.Forms.Label();
        this.jointCirclePanel = new JointCirclePanel();
        this.pnlCircleActions = new System.Windows.Forms.Panel();
        this.btnSelectAll = new System.Windows.Forms.Button();
        this.btnDeselectAll = new System.Windows.Forms.Button();
        this.lblCircleCount = new System.Windows.Forms.Label();
        this.pnlCenter = new System.Windows.Forms.Panel();
        this.picCamera = new System.Windows.Forms.PictureBox();
        this.pnlBottom = new System.Windows.Forms.Panel();
        this.grpZoom = new System.Windows.Forms.GroupBox();
        this.picZoom = new System.Windows.Forms.PictureBox();
        this.grpWorkflow = new System.Windows.Forms.GroupBox();
        this.lblStep1 = new System.Windows.Forms.Label();
        this.btnCapturePhoto = new System.Windows.Forms.Button();
        this.lblStep2 = new System.Windows.Forms.Label();
        this.btnDetectJoints = new System.Windows.Forms.Button();
        this.lblStep3 = new System.Windows.Forms.Label();
        this.lblHint3 = new System.Windows.Forms.Label();
        this.lblStep4 = new System.Windows.Forms.Label();
        this.btnAutoMode = new System.Windows.Forms.Button();
        this.btnManualMode = new System.Windows.Forms.Button();
        this.lblStep5 = new System.Windows.Forms.Label();
        this.btnStartWelding = new System.Windows.Forms.Button();
        this.btnPauseWelding = new System.Windows.Forms.Button();
        this.btnStopWelding = new System.Windows.Forms.Button();
        this.lblWeldStatus = new System.Windows.Forms.Label();
        this.pnlLog = new System.Windows.Forms.Panel();
        this.txtLog = new System.Windows.Forms.TextBox();
        this.btnClearLog = new System.Windows.Forms.Button();
        this.pnlCameraControls.SuspendLayout();
        this.grpCameraSettings.SuspendLayout();
        this.grpDeviceList.SuspendLayout();
        this.grpAcquisition.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudBufferCount)).BeginInit();
        this.grpExposure.SuspendLayout();
        this.grpStatus.SuspendLayout();
        this.pnlRight.SuspendLayout();
        this.pnlCircleActions.SuspendLayout();
        this.pnlCenter.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.picCamera)).BeginInit();
        this.pnlBottom.SuspendLayout();
        this.grpZoom.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.picZoom)).BeginInit();
        this.grpWorkflow.SuspendLayout();
        this.pnlLog.SuspendLayout();
        this.SuspendLayout();
        //
        // lblProvider
        //
        this.lblProvider.Text = "厂商:";
        this.lblProvider.Location = new Point(8, 22);
        this.lblProvider.Size = new Size(40, 18);
        this.lblProvider.TextAlign = ContentAlignment.MiddleRight;
        //
        // cboProvider
        //
        this.cboProvider.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboProvider.Items.AddRange(new object[] { "华睿 (CXP)", "海康 (GigE)" });
        this.cboProvider.Location = new Point(52, 20);
        this.cboProvider.Size = new Size(168, 23);
        this.cboProvider.SelectedIndex = 0;
        //
        // lblCtiPath
        //
        this.lblCtiPath.Text = "CTI:";
        this.lblCtiPath.Location = new Point(8, 52);
        this.lblCtiPath.Size = new Size(30, 18);
        this.lblCtiPath.TextAlign = ContentAlignment.MiddleRight;
        //
        // txtCtiPath
        //
        this.txtCtiPath.Font = new Font("微软雅黑", 7.5f);
        this.txtCtiPath.Location = new Point(8, 70);
        this.txtCtiPath.Size = new Size(174, 23);
        //
        // btnBrowseCti
        //
        this.btnBrowseCti.Text = "…";
        this.btnBrowseCti.Location = new Point(185, 69);
        this.btnBrowseCti.Size = new Size(35, 24);
        //
        // btnInitialize
        //
        this.btnInitialize.Text = "初始化 CTI";
        this.btnInitialize.Location = new Point(8, 100);
        this.btnInitialize.Size = new Size(104, 26);
        //
        // btnEnumerate
        //
        this.btnEnumerate.Text = "枚举设备";
        this.btnEnumerate.Enabled = false;
        this.btnEnumerate.Location = new Point(116, 100);
        this.btnEnumerate.Size = new Size(103, 26);
        //
        // grpCameraSettings
        //
        this.grpCameraSettings.Text = "相机设置";
        this.grpCameraSettings.Location = new Point(4, 4);
        this.grpCameraSettings.Size = new Size(233, 138);
        this.grpCameraSettings.Controls.AddRange(new Control[] {
            this.lblProvider, this.cboProvider, this.lblCtiPath, this.txtCtiPath,
            this.btnBrowseCti, this.btnInitialize, this.btnEnumerate });
        //
        // lstDevices
        //
        this.lstDevices.Location = new Point(8, 22);
        this.lstDevices.Size = new Size(217, 58);
        //
        // btnConnect
        //
        this.btnConnect.Text = "连接";
        this.btnConnect.Enabled = false;
        this.btnConnect.Location = new Point(8, 86);
        this.btnConnect.Size = new Size(103, 26);
        //
        // btnDisconnect
        //
        this.btnDisconnect.Text = "断开";
        this.btnDisconnect.Enabled = false;
        this.btnDisconnect.Location = new Point(115, 86);
        this.btnDisconnect.Size = new Size(102, 26);
        //
        // grpDeviceList
        //
        this.grpDeviceList.Text = "设备列表";
        this.grpDeviceList.Location = new Point(4, 146);
        this.grpDeviceList.Size = new Size(233, 122);
        this.grpDeviceList.Controls.AddRange(new Control[] {
            this.lstDevices, this.btnConnect, this.btnDisconnect });
        //
        // lblBufferCount
        //
        this.lblBufferCount.Text = "缓冲数:";
        this.lblBufferCount.Location = new Point(8, 22);
        this.lblBufferCount.Size = new Size(52, 18);
        this.lblBufferCount.TextAlign = ContentAlignment.MiddleRight;
        //
        // nudBufferCount
        //
        this.nudBufferCount.Location = new Point(64, 20);
        this.nudBufferCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this.nudBufferCount.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
        this.nudBufferCount.Value = new decimal(new int[] { 16, 0, 0, 0 });
        this.nudBufferCount.Size = new Size(60, 23);
        //
        // chkAutoSave
        //
        this.chkAutoSave.Text = "自动保存";
        this.chkAutoSave.AutoSize = true;
        this.chkAutoSave.Location = new Point(138, 22);
        //
        // btnStartGrab
        //
        this.btnStartGrab.Text = "开始采集";
        this.btnStartGrab.Enabled = false;
        this.btnStartGrab.Location = new Point(8, 50);
        this.btnStartGrab.Size = new Size(103, 26);
        //
        // btnStopGrab
        //
        this.btnStopGrab.Text = "停止采集";
        this.btnStopGrab.Enabled = false;
        this.btnStopGrab.Location = new Point(115, 50);
        this.btnStopGrab.Size = new Size(102, 26);
        //
        // grpAcquisition
        //
        this.grpAcquisition.Text = "采集控制";
        this.grpAcquisition.Location = new Point(4, 272);
        this.grpAcquisition.Size = new Size(233, 90);
        this.grpAcquisition.Controls.AddRange(new Control[] {
            this.lblBufferCount, this.nudBufferCount, this.chkAutoSave,
            this.btnStartGrab, this.btnStopGrab });
        //
        // lblExposureUs
        //
        this.lblExposureUs.Text = "曝光(μs):";
        this.lblExposureUs.Location = new Point(8, 22);
        this.lblExposureUs.Size = new Size(62, 18);
        this.lblExposureUs.TextAlign = ContentAlignment.MiddleRight;
        //
        // txtExposureTime
        //
        this.txtExposureTime.Text = "10000";
        this.txtExposureTime.Enabled = false;
        this.txtExposureTime.Location = new Point(74, 20);
        this.txtExposureTime.Size = new Size(90, 23);
        //
        // btnGetExposure
        //
        this.btnGetExposure.Text = "读取";
        this.btnGetExposure.Enabled = false;
        this.btnGetExposure.Location = new Point(8, 50);
        this.btnGetExposure.Size = new Size(74, 26);
        //
        // btnSetExposure
        //
        this.btnSetExposure.Text = "设置";
        this.btnSetExposure.Enabled = false;
        this.btnSetExposure.Location = new Point(86, 50);
        this.btnSetExposure.Size = new Size(74, 26);
        //
        // grpExposure
        //
        this.grpExposure.Text = "曝光控制";
        this.grpExposure.Location = new Point(4, 366);
        this.grpExposure.Size = new Size(233, 88);
        this.grpExposure.Controls.AddRange(new Control[] {
            this.lblExposureUs, this.txtExposureTime,
            this.btnGetExposure, this.btnSetExposure });
        //
        // Status labels (sr=22, rh=20 → y: 22,42,62,82,102)
        //
        this.lblStateLbl.Text = "状态:";
        this.lblStateLbl.Location = new Point(8, 22);
        this.lblStateLbl.Size = new Size(64, 18);
        this.lblStateLbl.TextAlign = ContentAlignment.MiddleRight;
        this.lblStateLbl.Font = new Font("微软雅黑", 8.5f);
        //
        this.lblState.Text = "就绪";
        this.lblState.Location = new Point(72, 22);
        this.lblState.Size = new Size(158, 18);
        this.lblState.ForeColor = Color.Gray;
        this.lblState.Font = new Font("微软雅黑", 8.5f, FontStyle.Bold);
        //
        this.lblFrameCountLbl.Text = "帧数:";
        this.lblFrameCountLbl.Location = new Point(8, 42);
        this.lblFrameCountLbl.Size = new Size(64, 18);
        this.lblFrameCountLbl.TextAlign = ContentAlignment.MiddleRight;
        this.lblFrameCountLbl.Font = new Font("微软雅黑", 8.5f);
        //
        this.lblFrameCount.Text = "0";
        this.lblFrameCount.Location = new Point(72, 42);
        this.lblFrameCount.Size = new Size(158, 18);
        this.lblFrameCount.ForeColor = Color.Black;
        this.lblFrameCount.Font = new Font("微软雅黑", 8.5f, FontStyle.Bold);
        //
        this.lblFpsLbl.Text = "FPS:";
        this.lblFpsLbl.Location = new Point(8, 62);
        this.lblFpsLbl.Size = new Size(64, 18);
        this.lblFpsLbl.TextAlign = ContentAlignment.MiddleRight;
        this.lblFpsLbl.Font = new Font("微软雅黑", 8.5f);
        //
        this.lblFps.Text = "0.0";
        this.lblFps.Location = new Point(72, 62);
        this.lblFps.Size = new Size(158, 18);
        this.lblFps.ForeColor = Color.Black;
        this.lblFps.Font = new Font("微软雅黑", 8.5f, FontStyle.Bold);
        //
        this.lblResolutionLbl.Text = "分辨率:";
        this.lblResolutionLbl.Location = new Point(8, 82);
        this.lblResolutionLbl.Size = new Size(64, 18);
        this.lblResolutionLbl.TextAlign = ContentAlignment.MiddleRight;
        this.lblResolutionLbl.Font = new Font("微软雅黑", 8.5f);
        //
        this.lblResolution.Text = "--";
        this.lblResolution.Location = new Point(72, 82);
        this.lblResolution.Size = new Size(158, 18);
        this.lblResolution.ForeColor = Color.Black;
        this.lblResolution.Font = new Font("微软雅黑", 8.5f, FontStyle.Bold);
        //
        this.lblPixelFmtLbl.Text = "像素格式:";
        this.lblPixelFmtLbl.Location = new Point(8, 102);
        this.lblPixelFmtLbl.Size = new Size(64, 18);
        this.lblPixelFmtLbl.TextAlign = ContentAlignment.MiddleRight;
        this.lblPixelFmtLbl.Font = new Font("微软雅黑", 8.5f);
        //
        this.lblPixelFmt.Text = "--";
        this.lblPixelFmt.Location = new Point(72, 102);
        this.lblPixelFmt.Size = new Size(158, 18);
        this.lblPixelFmt.ForeColor = Color.Black;
        this.lblPixelFmt.Font = new Font("微软雅黑", 8.5f, FontStyle.Bold);
        //
        // grpStatus
        //
        this.grpStatus.Text = "状态信息";
        this.grpStatus.Location = new Point(4, 458);
        this.grpStatus.Size = new Size(233, 134);
        this.grpStatus.Controls.AddRange(new Control[] {
            this.lblStateLbl, this.lblState,
            this.lblFrameCountLbl, this.lblFrameCount,
            this.lblFpsLbl, this.lblFps,
            this.lblResolutionLbl, this.lblResolution,
            this.lblPixelFmtLbl, this.lblPixelFmt });
        //
        // btnOpenCalib
        //
        this.btnOpenCalib.Text = "9点标定";
        this.btnOpenCalib.BackColor = Color.FromArgb(52, 100, 160);
        this.btnOpenCalib.ForeColor = Color.White;
        this.btnOpenCalib.FlatStyle = FlatStyle.Flat;
        this.btnOpenCalib.Font = new Font("微软雅黑", 9.5f, FontStyle.Bold);
        this.btnOpenCalib.Location = new Point(4, 600);
        this.btnOpenCalib.Size = new Size(233, 30);
        this.btnOpenCalib.FlatAppearance.BorderSize = 0;
        //
        // lblCalibStatus
        //
        this.lblCalibStatus.Text = "标定：未标定";
        this.lblCalibStatus.TextAlign = ContentAlignment.MiddleCenter;
        this.lblCalibStatus.ForeColor = Color.Gray;
        this.lblCalibStatus.Font = new Font("微软雅黑", 8f);
        this.lblCalibStatus.Location = new Point(4, 634);
        this.lblCalibStatus.Size = new Size(233, 18);
        //
        // pnlCameraControls
        //
        this.pnlCameraControls.AutoScroll = true;
        this.pnlCameraControls.BackColor = Color.FromArgb(245, 245, 248);
        this.pnlCameraControls.Dock = DockStyle.Left;
        this.pnlCameraControls.Width = 245;
        this.pnlCameraControls.Controls.AddRange(new Control[] {
            this.grpCameraSettings, this.grpDeviceList, this.grpAcquisition,
            this.grpExposure, this.grpStatus, this.btnOpenCalib, this.lblCalibStatus });
        //
        // lblCircleTitle
        //
        this.lblCircleTitle.Text = "焊点选择";
        this.lblCircleTitle.BackColor = Color.FromArgb(40, 100, 180);
        this.lblCircleTitle.Dock = DockStyle.Top;
        this.lblCircleTitle.Font = new Font("微软雅黑", 10f, FontStyle.Bold);
        this.lblCircleTitle.ForeColor = Color.White;
        this.lblCircleTitle.Height = 32;
        this.lblCircleTitle.TextAlign = ContentAlignment.MiddleCenter;
        //
        // jointCirclePanel
        //
        this.jointCirclePanel.Dock = DockStyle.Fill;
        //
        // btnSelectAll
        //
        this.btnSelectAll.Text = "全选";
        this.btnSelectAll.FlatStyle = FlatStyle.Flat;
        this.btnSelectAll.Location = new Point(8, 10);
        this.btnSelectAll.Size = new Size(80, 28);
        //
        // btnDeselectAll
        //
        this.btnDeselectAll.Text = "全不选";
        this.btnDeselectAll.FlatStyle = FlatStyle.Flat;
        this.btnDeselectAll.Location = new Point(96, 10);
        this.btnDeselectAll.Size = new Size(80, 28);
        //
        // lblCircleCount
        //
        this.lblCircleCount.Text = "已选: 0 / 0 个焊点";
        this.lblCircleCount.ForeColor = Color.FromArgb(100, 100, 100);
        this.lblCircleCount.Location = new Point(8, 44);
        this.lblCircleCount.Size = new Size(370, 18);
        //
        // pnlCircleActions
        //
        this.pnlCircleActions.BackColor = Color.White;
        this.pnlCircleActions.Dock = DockStyle.Bottom;
        this.pnlCircleActions.Height = 68;
        this.pnlCircleActions.Controls.AddRange(new Control[] {
            this.btnSelectAll, this.btnDeselectAll, this.lblCircleCount });
        //
        // pnlRight
        //
        this.pnlRight.BackColor = Color.White;
        this.pnlRight.Dock = DockStyle.Right;
        this.pnlRight.Width = 450;
        this.pnlRight.Controls.Add(this.jointCirclePanel);
        this.pnlRight.Controls.Add(this.pnlCircleActions);
        this.pnlRight.Controls.Add(this.lblCircleTitle);
        //
        // Workflow step number labels (① ② ③ ④ ⑤)
        // row positions: 18, 54, 90, 124, 160
        //
        this.lblStep1.Text = "①";
        this.lblStep1.Font = new Font("微软雅黑", 11f, FontStyle.Bold);
        this.lblStep1.ForeColor = Color.FromArgb(100, 180, 255);
        this.lblStep1.Location = new Point(10, 18);
        this.lblStep1.Size = new Size(24, 24);
        this.lblStep1.TextAlign = ContentAlignment.MiddleCenter;
        //
        this.lblStep2.Text = "②";
        this.lblStep2.Font = new Font("微软雅黑", 11f, FontStyle.Bold);
        this.lblStep2.ForeColor = Color.FromArgb(100, 180, 255);
        this.lblStep2.Location = new Point(10, 54);
        this.lblStep2.Size = new Size(24, 24);
        this.lblStep2.TextAlign = ContentAlignment.MiddleCenter;
        //
        this.lblStep3.Text = "③";
        this.lblStep3.Font = new Font("微软雅黑", 11f, FontStyle.Bold);
        this.lblStep3.ForeColor = Color.FromArgb(100, 180, 255);
        this.lblStep3.Location = new Point(10, 90);
        this.lblStep3.Size = new Size(24, 24);
        this.lblStep3.TextAlign = ContentAlignment.MiddleCenter;
        //
        this.lblHint3.Text = "选择焊点 → 操作右侧圆形界面";
        this.lblHint3.AutoSize = true;
        this.lblHint3.Font = new Font("微软雅黑", 8.5f);
        this.lblHint3.ForeColor = Color.FromArgb(160, 165, 175);
        this.lblHint3.Location = new Point(40, 94);
        //
        this.lblStep4.Text = "④";
        this.lblStep4.Font = new Font("微软雅黑", 11f, FontStyle.Bold);
        this.lblStep4.ForeColor = Color.FromArgb(100, 180, 255);
        this.lblStep4.Location = new Point(10, 124);
        this.lblStep4.Size = new Size(24, 24);
        this.lblStep4.TextAlign = ContentAlignment.MiddleCenter;
        //
        this.lblStep5.Text = "⑤";
        this.lblStep5.Font = new Font("微软雅黑", 11f, FontStyle.Bold);
        this.lblStep5.ForeColor = Color.FromArgb(100, 180, 255);
        this.lblStep5.Location = new Point(10, 160);
        this.lblStep5.Size = new Size(24, 24);
        this.lblStep5.TextAlign = ContentAlignment.MiddleCenter;
        //
        // btnCapturePhoto
        //
        this.btnCapturePhoto.Text = "拍照";
        this.btnCapturePhoto.BackColor = Color.FromArgb(41, 128, 185);
        this.btnCapturePhoto.FlatStyle = FlatStyle.Flat;
        this.btnCapturePhoto.Font = new Font("微软雅黑", 9.5f, FontStyle.Bold);
        this.btnCapturePhoto.ForeColor = Color.White;
        this.btnCapturePhoto.Location = new Point(40, 18);
        this.btnCapturePhoto.Size = new Size(90, 28);
        this.btnCapturePhoto.FlatAppearance.BorderSize = 0;
        //
        // btnDetectJoints
        //
        this.btnDetectJoints.Text = "识别焊点";
        this.btnDetectJoints.BackColor = Color.FromArgb(39, 174, 96);
        this.btnDetectJoints.FlatStyle = FlatStyle.Flat;
        this.btnDetectJoints.Font = new Font("微软雅黑", 9.5f, FontStyle.Bold);
        this.btnDetectJoints.ForeColor = Color.White;
        this.btnDetectJoints.Location = new Point(40, 54);
        this.btnDetectJoints.Size = new Size(110, 28);
        this.btnDetectJoints.FlatAppearance.BorderSize = 0;
        //
        // btnAutoMode
        //
        this.btnAutoMode.Text = "自动模式";
        this.btnAutoMode.BackColor = Color.FromArgb(142, 68, 173);
        this.btnAutoMode.FlatStyle = FlatStyle.Flat;
        this.btnAutoMode.Font = new Font("微软雅黑", 9f, FontStyle.Bold);
        this.btnAutoMode.ForeColor = Color.White;
        this.btnAutoMode.Location = new Point(40, 124);
        this.btnAutoMode.Size = new Size(96, 28);
        this.btnAutoMode.Tag = "auto";
        this.btnAutoMode.FlatAppearance.BorderSize = 0;
        //
        // btnManualMode
        //
        this.btnManualMode.Text = "手动模式";
        this.btnManualMode.BackColor = Color.FromArgb(52, 55, 65);
        this.btnManualMode.FlatStyle = FlatStyle.Flat;
        this.btnManualMode.Font = new Font("微软雅黑", 9f);
        this.btnManualMode.ForeColor = Color.FromArgb(200, 200, 210);
        this.btnManualMode.Location = new Point(142, 124);
        this.btnManualMode.Size = new Size(96, 28);
        this.btnManualMode.Tag = "manual";
        this.btnManualMode.FlatAppearance.BorderSize = 0;
        //
        // btnStartWelding
        //
        this.btnStartWelding.Text = "▶  开始焊接";
        this.btnStartWelding.BackColor = Color.FromArgb(39, 174, 96);
        this.btnStartWelding.FlatStyle = FlatStyle.Flat;
        this.btnStartWelding.Font = new Font("微软雅黑", 9.5f, FontStyle.Bold);
        this.btnStartWelding.ForeColor = Color.White;
        this.btnStartWelding.Location = new Point(40, 160);
        this.btnStartWelding.Size = new Size(118, 30);
        this.btnStartWelding.FlatAppearance.BorderSize = 0;
        //
        // btnPauseWelding
        //
        this.btnPauseWelding.Text = "⏸  暂停";
        this.btnPauseWelding.BackColor = Color.FromArgb(243, 156, 18);
        this.btnPauseWelding.Enabled = false;
        this.btnPauseWelding.FlatStyle = FlatStyle.Flat;
        this.btnPauseWelding.Font = new Font("微软雅黑", 9f, FontStyle.Bold);
        this.btnPauseWelding.ForeColor = Color.White;
        this.btnPauseWelding.Location = new Point(164, 160);
        this.btnPauseWelding.Size = new Size(90, 30);
        this.btnPauseWelding.FlatAppearance.BorderSize = 0;
        //
        // btnStopWelding
        //
        this.btnStopWelding.Text = "⏹  停止";
        this.btnStopWelding.BackColor = Color.FromArgb(231, 76, 60);
        this.btnStopWelding.Enabled = false;
        this.btnStopWelding.FlatStyle = FlatStyle.Flat;
        this.btnStopWelding.Font = new Font("微软雅黑", 9f, FontStyle.Bold);
        this.btnStopWelding.ForeColor = Color.White;
        this.btnStopWelding.Location = new Point(260, 160);
        this.btnStopWelding.Size = new Size(90, 30);
        this.btnStopWelding.FlatAppearance.BorderSize = 0;
        //
        // lblWeldStatus
        //
        this.lblWeldStatus.Text = "就绪";
        this.lblWeldStatus.Font = new Font("微软雅黑", 9f, FontStyle.Bold);
        this.lblWeldStatus.ForeColor = Color.FromArgb(130, 130, 140);
        this.lblWeldStatus.Location = new Point(356, 165);
        this.lblWeldStatus.Size = new Size(110, 22);
        //
        // grpWorkflow
        //
        this.grpWorkflow.Text = "操作流程";
        this.grpWorkflow.BackColor = Color.FromArgb(30, 32, 40);
        this.grpWorkflow.Dock = DockStyle.Fill;
        this.grpWorkflow.Font = new Font("微软雅黑", 8.5f);
        this.grpWorkflow.ForeColor = Color.FromArgb(150, 150, 165);
        this.grpWorkflow.Controls.AddRange(new Control[] {
            this.lblStep1, this.btnCapturePhoto,
            this.lblStep2, this.btnDetectJoints,
            this.lblStep3, this.lblHint3,
            this.lblStep4, this.btnAutoMode, this.btnManualMode,
            this.lblStep5, this.btnStartWelding, this.btnPauseWelding,
            this.btnStopWelding, this.lblWeldStatus });
        //
        // picZoom
        //
        this.picZoom.BackColor = Color.FromArgb(15, 15, 20);
        this.picZoom.Dock = DockStyle.Fill;
        this.picZoom.SizeMode = PictureBoxSizeMode.Zoom;
        //
        // grpZoom
        //
        this.grpZoom.Text = "焊点放大";
        this.grpZoom.BackColor = Color.FromArgb(26, 28, 36);
        this.grpZoom.Dock = DockStyle.Left;
        this.grpZoom.Font = new Font("微软雅黑", 8.5f);
        this.grpZoom.ForeColor = Color.FromArgb(150, 150, 150);
        this.grpZoom.Width = 230;
        this.grpZoom.Controls.Add(this.picZoom);
        //
        // pnlBottom
        //
        this.pnlBottom.BackColor = Color.FromArgb(26, 28, 36);
        this.pnlBottom.Dock = DockStyle.Bottom;
        this.pnlBottom.Height = 230;
        this.pnlBottom.Controls.Add(this.grpWorkflow);
        this.pnlBottom.Controls.Add(this.grpZoom);
        //
        // picCamera
        //
        this.picCamera.BackColor = Color.FromArgb(20, 20, 24);
        this.picCamera.Cursor = Cursors.Cross;
        this.picCamera.Dock = DockStyle.Fill;
        this.picCamera.SizeMode = PictureBoxSizeMode.Zoom;
        //
        // pnlCenter
        //
        this.pnlCenter.Dock = DockStyle.Fill;
        this.pnlCenter.Controls.Add(this.picCamera);
        this.pnlCenter.Controls.Add(this.pnlBottom);
        //
        // txtLog
        //
        this.txtLog.BackColor = Color.FromArgb(18, 18, 22);
        this.txtLog.BorderStyle = BorderStyle.None;
        this.txtLog.Dock = DockStyle.Fill;
        this.txtLog.Font = new Font("Consolas", 8.5f);
        this.txtLog.ForeColor = Color.FromArgb(170, 210, 170);
        this.txtLog.Multiline = true;
        this.txtLog.ReadOnly = true;
        this.txtLog.ScrollBars = ScrollBars.Vertical;
        //
        // btnClearLog
        //
        this.btnClearLog.Text = "清除";
        this.btnClearLog.BackColor = Color.FromArgb(35, 35, 40);
        this.btnClearLog.Dock = DockStyle.Right;
        this.btnClearLog.FlatStyle = FlatStyle.Flat;
        this.btnClearLog.ForeColor = Color.FromArgb(180, 180, 180);
        this.btnClearLog.Width = 56;
        //
        // pnlLog
        //
        this.pnlLog.BackColor = Color.FromArgb(18, 18, 22);
        this.pnlLog.Dock = DockStyle.Bottom;
        this.pnlLog.Height = 120;
        this.pnlLog.Controls.Add(this.txtLog);
        this.pnlLog.Controls.Add(this.btnClearLog);
        //
        // MainForm
        //
        this.BackColor = Color.FromArgb(30, 32, 40);
        this.Font = new Font("微软雅黑", 9f);
        this.MinimumSize = new Size(1140, 740);
        this.Size = new Size(1460, 920);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "GenICam 焊点检测系统";
        // Dock order: Fill first, then Left/Right, then Bottom
        this.Controls.Add(this.pnlCenter);
        this.Controls.Add(this.pnlCameraControls);
        this.Controls.Add(this.pnlRight);
        this.Controls.Add(this.pnlLog);
        //
        this.grpCameraSettings.ResumeLayout(false);
        this.grpCameraSettings.PerformLayout();
        this.grpDeviceList.ResumeLayout(false);
        this.grpAcquisition.ResumeLayout(false);
        this.grpAcquisition.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudBufferCount)).EndInit();
        this.grpExposure.ResumeLayout(false);
        this.grpExposure.PerformLayout();
        this.grpStatus.ResumeLayout(false);
        this.pnlCameraControls.ResumeLayout(false);
        this.pnlCircleActions.ResumeLayout(false);
        this.pnlRight.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.picCamera)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.picZoom)).EndInit();
        this.grpZoom.ResumeLayout(false);
        this.grpWorkflow.ResumeLayout(false);
        this.grpWorkflow.PerformLayout();
        this.pnlBottom.ResumeLayout(false);
        this.pnlCenter.ResumeLayout(false);
        this.pnlLog.ResumeLayout(false);
        this.pnlLog.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    // ── 左侧相机控制栏 ────────────────────────────────────────
    private System.Windows.Forms.Button        btnOpenCalib      = null!;
    private System.Windows.Forms.Label         lblCalibStatus    = null!;
    private System.Windows.Forms.Panel         pnlCameraControls = null!;
    private System.Windows.Forms.GroupBox      grpCameraSettings = null!;
    private System.Windows.Forms.Label         lblProvider       = null!;
    private System.Windows.Forms.ComboBox      cboProvider       = null!;
    private System.Windows.Forms.Label         lblCtiPath        = null!;
    private System.Windows.Forms.TextBox       txtCtiPath        = null!;
    private System.Windows.Forms.Button        btnBrowseCti      = null!;
    private System.Windows.Forms.Button        btnInitialize     = null!;
    private System.Windows.Forms.Button        btnEnumerate      = null!;
    private System.Windows.Forms.GroupBox      grpDeviceList     = null!;
    private System.Windows.Forms.ListBox       lstDevices        = null!;
    private System.Windows.Forms.Button        btnConnect        = null!;
    private System.Windows.Forms.Button        btnDisconnect     = null!;
    private System.Windows.Forms.GroupBox      grpAcquisition    = null!;
    private System.Windows.Forms.Label         lblBufferCount    = null!;
    private System.Windows.Forms.NumericUpDown nudBufferCount    = null!;
    private System.Windows.Forms.CheckBox      chkAutoSave       = null!;
    private System.Windows.Forms.Button        btnStartGrab      = null!;
    private System.Windows.Forms.Button        btnStopGrab       = null!;
    private System.Windows.Forms.GroupBox      grpExposure       = null!;
    private System.Windows.Forms.Label         lblExposureUs     = null!;
    private System.Windows.Forms.TextBox       txtExposureTime   = null!;
    private System.Windows.Forms.Button        btnGetExposure    = null!;
    private System.Windows.Forms.Button        btnSetExposure    = null!;
    private System.Windows.Forms.GroupBox      grpStatus         = null!;
    private System.Windows.Forms.Label         lblStateLbl       = null!;
    private System.Windows.Forms.Label         lblState          = null!;
    private System.Windows.Forms.Label         lblFrameCountLbl  = null!;
    private System.Windows.Forms.Label         lblFrameCount     = null!;
    private System.Windows.Forms.Label         lblFpsLbl         = null!;
    private System.Windows.Forms.Label         lblFps            = null!;
    private System.Windows.Forms.Label         lblResolutionLbl  = null!;
    private System.Windows.Forms.Label         lblResolution     = null!;
    private System.Windows.Forms.Label         lblPixelFmtLbl    = null!;
    private System.Windows.Forms.Label         lblPixelFmt       = null!;
    // ── 中央区域 ──────────────────────────────────────────────
    private System.Windows.Forms.Panel         pnlCenter         = null!;
    private System.Windows.Forms.PictureBox    picCamera         = null!;
    private System.Windows.Forms.Panel         pnlBottom         = null!;
    private System.Windows.Forms.GroupBox      grpZoom           = null!;
    private System.Windows.Forms.PictureBox    picZoom           = null!;
    private System.Windows.Forms.GroupBox      grpWorkflow       = null!;
    private System.Windows.Forms.Label         lblStep1          = null!;
    private System.Windows.Forms.Button        btnCapturePhoto   = null!;
    private System.Windows.Forms.Label         lblStep2          = null!;
    private System.Windows.Forms.Button        btnDetectJoints   = null!;
    private System.Windows.Forms.Label         lblStep3          = null!;
    private System.Windows.Forms.Label         lblHint3          = null!;
    private System.Windows.Forms.Label         lblStep4          = null!;
    private System.Windows.Forms.Button        btnAutoMode       = null!;
    private System.Windows.Forms.Button        btnManualMode     = null!;
    private System.Windows.Forms.Label         lblStep5          = null!;
    private System.Windows.Forms.Button        btnStartWelding   = null!;
    private System.Windows.Forms.Button        btnPauseWelding   = null!;
    private System.Windows.Forms.Button        btnStopWelding    = null!;
    private System.Windows.Forms.Label         lblWeldStatus     = null!;
    // ── 右侧焊点圈圈面板 ─────────────────────────────────────
    private System.Windows.Forms.Panel         pnlRight          = null!;
    private System.Windows.Forms.Label         lblCircleTitle    = null!;
    private JointCirclePanel                    jointCirclePanel  = null!;
    private System.Windows.Forms.Panel         pnlCircleActions  = null!;
    private System.Windows.Forms.Button        btnSelectAll      = null!;
    private System.Windows.Forms.Button        btnDeselectAll    = null!;
    private System.Windows.Forms.Label         lblCircleCount    = null!;
    // ── 日志区 ───────────────────────────────────────────────
    private System.Windows.Forms.Panel         pnlLog            = null!;
    private System.Windows.Forms.TextBox       txtLog            = null!;
    private System.Windows.Forms.Button        btnClearLog       = null!;
}
