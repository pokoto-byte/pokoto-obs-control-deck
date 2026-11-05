namespace OBSControlDeck
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblObsStatus;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabControl;
        private System.Windows.Forms.GroupBox grpScenes;
        private System.Windows.Forms.Button btnPrePartita;
        private System.Windows.Forms.Button btnDiretta;
        private System.Windows.Forms.Button btnFormazCasa;
        private System.Windows.Forms.Button btnFormazOspiti;
        private System.Windows.Forms.Button btnIntervallo;
        private System.Windows.Forms.Button btnFinePartita;
        private System.Windows.Forms.GroupBox grpReplay;
        private System.Windows.Forms.Button btnSaveReplay;
        private System.Windows.Forms.Button btnReplayMain;
        private System.Windows.Forms.Button btnReplaySX;
        private System.Windows.Forms.Button btnReplayDX;
        private System.Windows.Forms.GroupBox grpFunctionTexts;
        private System.Windows.Forms.Button btnF1Visit;
        private System.Windows.Forms.Button btnF2Home;
        private System.Windows.Forms.Button btnF2Visit;
        private System.Windows.Forms.Button btnF3Home;
        private System.Windows.Forms.Button btnF3Visit;
        private System.Windows.Forms.Button btnF4Home;
        private System.Windows.Forms.Button btnF4Visit;
        private System.Windows.Forms.Button btnF5Home;
        private System.Windows.Forms.Button btnF5Visit;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            lblObsStatus = new Label();
            tabMain = new TabControl();
            tabControl = new TabPage();
            grpFunctionTexts = new GroupBox();
            btnF1Home = new Button();
            cmsButtonRename = new ContextMenuStrip(components);
            mnuRenameButton = new ToolStripMenuItem();
            mnuResetButtonName = new ToolStripMenuItem();
            btnF1Visit = new Button();
            btnF5Visit = new Button();
            btnF5Home = new Button();
            btnF4Visit = new Button();
            btnF4Home = new Button();
            btnF3Visit = new Button();
            btnF3Home = new Button();
            btnF2Visit = new Button();
            btnF2Home = new Button();
            grpAudio = new GroupBox();
            btnWireless = new Button();
            btnMic2 = new Button();
            btnMic1 = new Button();
            grpScenes = new GroupBox();
            btnPrePartita = new Button();
            btnDiretta = new Button();
            btnFormazCasa = new Button();
            btnFormazOspiti = new Button();
            btnIntervallo = new Button();
            btnFinePartita = new Button();
            grpReplay = new GroupBox();
            btnSaveReplay = new Button();
            btnReplayMain = new Button();
            btnReplaySX = new Button();
            btnReplayDX = new Button();
            tabPage2 = new TabPage();
            grpSources = new GroupBox();
            grpAudioConfig = new GroupBox();
            tblAudioConfig = new TableLayoutPanel();
            lblCfgMic1 = new Label();
            cmbMic1 = new ComboBox();
            lblCfgMic2 = new Label();
            cmbMic2 = new ComboBox();
            lblCfgWireless = new Label();
            cmbWireless = new ComboBox();
            lstScenes = new ListBox();
            tvObsItems = new TreeView();
            chkUnlockFunctionLayout = new CheckBox();
            tabPage1 = new TabPage();
            chkAlwaysOnTop = new CheckBox();
            chkAutoConnect = new CheckBox();
            grpProfiles = new GroupBox();
            btnDeleteProfile = new Button();
            btnRenameProfile = new Button();
            btnNewProfile = new Button();
            cmbProfile = new ComboBox();
            lblActiveProfile = new Label();
            grpConnection = new GroupBox();
            txtHost = new TextBox();
            lblHost = new Label();
            lblPorta = new Label();
            txtPort = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            btnConnect = new Button();
            grpAbout = new GroupBox();
            lblAboutProduct = new Label();
            lblAboutVersion = new Label();
            lblAboutAuthor = new Label();
            lnkGitHub = new LinkLabel();
            lnkObsForum = new LinkLabel();
            lnkSupportProject = new LinkLabel();
            lblAboutOpenSource = new Label();
            lblAboutDisclaimer = new Label();
            lblAboutDevelopment = new Label();
            tabMain.SuspendLayout();
            tabControl.SuspendLayout();
            grpFunctionTexts.SuspendLayout();
            cmsButtonRename.SuspendLayout();
            grpAudio.SuspendLayout();
            grpScenes.SuspendLayout();
            grpReplay.SuspendLayout();
            tabPage2.SuspendLayout();
            grpSources.SuspendLayout();
            grpAudioConfig.SuspendLayout();
            tblAudioConfig.SuspendLayout();
            tabPage1.SuspendLayout();
            grpProfiles.SuspendLayout();
            grpConnection.SuspendLayout();
            grpAbout.SuspendLayout();
            SuspendLayout();
            // 
            // lblObsStatus
            // 
            lblObsStatus.AutoSize = true;
            lblObsStatus.ForeColor = Color.Red;
            lblObsStatus.Location = new Point(9, 5);
            lblObsStatus.Name = "lblObsStatus";
            lblObsStatus.Size = new Size(136, 20);
            lblObsStatus.TabIndex = 1;
            lblObsStatus.Text = "● OBS Disconnected";
            // 
            // tabMain
            // 
            tabMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabMain.Controls.Add(tabControl);
            tabMain.Controls.Add(tabPage2);
            tabMain.Controls.Add(tabPage1);
            tabMain.Location = new Point(-5, 29);
            tabMain.Margin = new Padding(3, 4, 3, 4);
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.Size = new Size(248, 831);
            tabMain.TabIndex = 2;
            // 
            // tabControl
            // 
            tabControl.BackColor = SystemColors.ControlDark;
            tabControl.Controls.Add(grpFunctionTexts);
            tabControl.Controls.Add(grpAudio);
            tabControl.Controls.Add(grpScenes);
            tabControl.Controls.Add(grpReplay);
            tabControl.Location = new Point(4, 29);
            tabControl.Margin = new Padding(3, 4, 3, 4);
            tabControl.Name = "tabControl";
            tabControl.Padding = new Padding(3, 4, 3, 4);
            tabControl.Size = new Size(240, 798);
            tabControl.TabIndex = 0;
            tabControl.Text = "MAIN";
            // 
            // grpFunctionTexts
            // 
            grpFunctionTexts.Controls.Add(btnF1Home);
            grpFunctionTexts.Controls.Add(btnF1Visit);
            grpFunctionTexts.Controls.Add(btnF5Visit);
            grpFunctionTexts.Controls.Add(btnF5Home);
            grpFunctionTexts.Controls.Add(btnF4Visit);
            grpFunctionTexts.Controls.Add(btnF4Home);
            grpFunctionTexts.Controls.Add(btnF3Visit);
            grpFunctionTexts.Controls.Add(btnF3Home);
            grpFunctionTexts.Controls.Add(btnF2Visit);
            grpFunctionTexts.Controls.Add(btnF2Home);
            grpFunctionTexts.Location = new Point(6, 383);
            grpFunctionTexts.Margin = new Padding(3, 4, 3, 4);
            grpFunctionTexts.Name = "grpFunctionTexts";
            grpFunctionTexts.Padding = new Padding(3, 4, 3, 4);
            grpFunctionTexts.Size = new Size(235, 335);
            grpFunctionTexts.TabIndex = 4;
            grpFunctionTexts.TabStop = false;
            grpFunctionTexts.Text = "SOURCES";
            // 
            // btnF1Home
            // 
            btnF1Home.ContextMenuStrip = cmsButtonRename;
            btnF1Home.Location = new Point(0, 29);
            btnF1Home.Margin = new Padding(3, 4, 3, 4);
            btnF1Home.Name = "btnF1Home";
            btnF1Home.Size = new Size(112, 60);
            btnF1Home.TabIndex = 2;
            btnF1Home.Text = "HOME";
            // 
            // cmsButtonRename
            // 
            cmsButtonRename.ImageScalingSize = new Size(20, 20);
            cmsButtonRename.Items.AddRange(new ToolStripItem[] { mnuRenameButton, mnuResetButtonName });
            cmsButtonRename.Name = "cmsButtonRename";
            cmsButtonRename.Size = new Size(190, 52);
            cmsButtonRename.Opening += cmsButtonRename_Opening;
            // 
            // mnuRenameButton
            // 
            mnuRenameButton.Name = "mnuRenameButton";
            mnuRenameButton.Size = new Size(189, 24);
            mnuRenameButton.Text = "Rename Button...";
            // 
            // mnuResetButtonName
            // 
            mnuResetButtonName.Name = "mnuResetButtonName";
            mnuResetButtonName.Size = new Size(189, 24);
            mnuResetButtonName.Text = "Reset name";
            // 
            // btnF1Visit
            // 
            btnF1Visit.ContextMenuStrip = cmsButtonRename;
            btnF1Visit.Location = new Point(123, 29);
            btnF1Visit.Margin = new Padding(3, 4, 3, 4);
            btnF1Visit.Name = "btnF1Visit";
            btnF1Visit.Size = new Size(112, 60);
            btnF1Visit.TabIndex = 2;
            btnF1Visit.Text = "VISIT";
            // 
            // btnF5Visit
            // 
            btnF5Visit.ContextMenuStrip = cmsButtonRename;
            btnF5Visit.Location = new Point(123, 269);
            btnF5Visit.Margin = new Padding(3, 4, 3, 4);
            btnF5Visit.Name = "btnF5Visit";
            btnF5Visit.Size = new Size(111, 60);
            btnF5Visit.TabIndex = 14;
            btnF5Visit.Text = "VISIT";
            // 
            // btnF5Home
            // 
            btnF5Home.ContextMenuStrip = cmsButtonRename;
            btnF5Home.Location = new Point(0, 269);
            btnF5Home.Margin = new Padding(3, 4, 3, 4);
            btnF5Home.Name = "btnF5Home";
            btnF5Home.Size = new Size(112, 60);
            btnF5Home.TabIndex = 13;
            btnF5Home.Text = "HOME";
            // 
            // btnF4Visit
            // 
            btnF4Visit.ContextMenuStrip = cmsButtonRename;
            btnF4Visit.Location = new Point(123, 209);
            btnF4Visit.Margin = new Padding(3, 4, 3, 4);
            btnF4Visit.Name = "btnF4Visit";
            btnF4Visit.Size = new Size(111, 60);
            btnF4Visit.TabIndex = 11;
            btnF4Visit.Text = "VISIT";
            // 
            // btnF4Home
            // 
            btnF4Home.ContextMenuStrip = cmsButtonRename;
            btnF4Home.Location = new Point(0, 209);
            btnF4Home.Margin = new Padding(3, 4, 3, 4);
            btnF4Home.Name = "btnF4Home";
            btnF4Home.Size = new Size(112, 60);
            btnF4Home.TabIndex = 10;
            btnF4Home.Text = "HOME";
            // 
            // btnF3Visit
            // 
            btnF3Visit.ContextMenuStrip = cmsButtonRename;
            btnF3Visit.Location = new Point(123, 149);
            btnF3Visit.Margin = new Padding(3, 4, 3, 4);
            btnF3Visit.Name = "btnF3Visit";
            btnF3Visit.Size = new Size(112, 60);
            btnF3Visit.TabIndex = 8;
            btnF3Visit.Text = "VISIT";
            // 
            // btnF3Home
            // 
            btnF3Home.ContextMenuStrip = cmsButtonRename;
            btnF3Home.Location = new Point(0, 149);
            btnF3Home.Margin = new Padding(3, 4, 3, 4);
            btnF3Home.Name = "btnF3Home";
            btnF3Home.Size = new Size(112, 60);
            btnF3Home.TabIndex = 7;
            btnF3Home.Text = "HOME";
            // 
            // btnF2Visit
            // 
            btnF2Visit.ContextMenuStrip = cmsButtonRename;
            btnF2Visit.Location = new Point(123, 89);
            btnF2Visit.Margin = new Padding(3, 4, 3, 4);
            btnF2Visit.Name = "btnF2Visit";
            btnF2Visit.Size = new Size(111, 60);
            btnF2Visit.TabIndex = 5;
            btnF2Visit.Text = "VISIT";
            // 
            // btnF2Home
            // 
            btnF2Home.ContextMenuStrip = cmsButtonRename;
            btnF2Home.Location = new Point(0, 89);
            btnF2Home.Margin = new Padding(3, 4, 3, 4);
            btnF2Home.Name = "btnF2Home";
            btnF2Home.Size = new Size(112, 60);
            btnF2Home.TabIndex = 4;
            btnF2Home.Text = "HOME";
            // 
            // grpAudio
            // 
            grpAudio.Controls.Add(btnWireless);
            grpAudio.Controls.Add(btnMic2);
            grpAudio.Controls.Add(btnMic1);
            grpAudio.Location = new Point(6, 717);
            grpAudio.Margin = new Padding(3, 4, 3, 4);
            grpAudio.Name = "grpAudio";
            grpAudio.Padding = new Padding(3, 4, 3, 4);
            grpAudio.Size = new Size(235, 77);
            grpAudio.TabIndex = 3;
            grpAudio.TabStop = false;
            grpAudio.Text = "AUDIO";
            // 
            // btnWireless
            // 
            btnWireless.Location = new Point(155, 21);
            btnWireless.Margin = new Padding(3, 4, 3, 4);
            btnWireless.Name = "btnWireless";
            btnWireless.Size = new Size(66, 45);
            btnWireless.TabIndex = 2;
            btnWireless.Text = "WIRL";
            btnWireless.UseVisualStyleBackColor = true;
            // 
            // btnMic2
            // 
            btnMic2.Location = new Point(82, 21);
            btnMic2.Margin = new Padding(3, 4, 3, 4);
            btnMic2.Name = "btnMic2";
            btnMic2.Size = new Size(66, 45);
            btnMic2.TabIndex = 1;
            btnMic2.Text = "MIC 2";
            btnMic2.UseVisualStyleBackColor = true;
            btnMic2.Click += btnMic2_Click;
            // 
            // btnMic1
            // 
            btnMic1.Location = new Point(6, 21);
            btnMic1.Margin = new Padding(3, 4, 3, 4);
            btnMic1.Name = "btnMic1";
            btnMic1.Size = new Size(67, 45);
            btnMic1.TabIndex = 0;
            btnMic1.Text = "MIC 1";
            btnMic1.UseVisualStyleBackColor = true;
            // 
            // grpScenes
            // 
            grpScenes.Controls.Add(btnPrePartita);
            grpScenes.Controls.Add(btnDiretta);
            grpScenes.Controls.Add(btnFormazCasa);
            grpScenes.Controls.Add(btnFormazOspiti);
            grpScenes.Controls.Add(btnIntervallo);
            grpScenes.Controls.Add(btnFinePartita);
            grpScenes.Location = new Point(6, 3);
            grpScenes.Margin = new Padding(3, 4, 3, 4);
            grpScenes.Name = "grpScenes";
            grpScenes.Padding = new Padding(3, 4, 3, 4);
            grpScenes.Size = new Size(235, 232);
            grpScenes.TabIndex = 0;
            grpScenes.TabStop = false;
            grpScenes.Text = "SCENE";
            // 
            // btnPrePartita
            // 
            btnPrePartita.ContextMenuStrip = cmsButtonRename;
            btnPrePartita.Location = new Point(0, 20);
            btnPrePartita.Margin = new Padding(3, 4, 3, 4);
            btnPrePartita.Name = "btnPrePartita";
            btnPrePartita.Size = new Size(112, 76);
            btnPrePartita.TabIndex = 0;
            btnPrePartita.Text = "PRE PARTITA";
            btnPrePartita.UseVisualStyleBackColor = true;
            // 
            // btnDiretta
            // 
            btnDiretta.ContextMenuStrip = cmsButtonRename;
            btnDiretta.Location = new Point(123, 20);
            btnDiretta.Margin = new Padding(3, 4, 3, 4);
            btnDiretta.Name = "btnDiretta";
            btnDiretta.Size = new Size(112, 76);
            btnDiretta.TabIndex = 1;
            btnDiretta.Text = "DIRETTA";
            btnDiretta.UseVisualStyleBackColor = true;
            // 
            // btnFormazCasa
            // 
            btnFormazCasa.ContextMenuStrip = cmsButtonRename;
            btnFormazCasa.Location = new Point(0, 97);
            btnFormazCasa.Margin = new Padding(3, 4, 3, 4);
            btnFormazCasa.Name = "btnFormazCasa";
            btnFormazCasa.Size = new Size(112, 65);
            btnFormazCasa.TabIndex = 2;
            btnFormazCasa.Text = "FORMAZ. CASA";
            btnFormazCasa.UseVisualStyleBackColor = true;
            // 
            // btnFormazOspiti
            // 
            btnFormazOspiti.ContextMenuStrip = cmsButtonRename;
            btnFormazOspiti.Location = new Point(123, 97);
            btnFormazOspiti.Margin = new Padding(3, 4, 3, 4);
            btnFormazOspiti.Name = "btnFormazOspiti";
            btnFormazOspiti.Size = new Size(111, 65);
            btnFormazOspiti.TabIndex = 3;
            btnFormazOspiti.Text = "FORMAZ. OSPITI";
            btnFormazOspiti.UseVisualStyleBackColor = true;
            // 
            // btnIntervallo
            // 
            btnIntervallo.ContextMenuStrip = cmsButtonRename;
            btnIntervallo.Location = new Point(0, 164);
            btnIntervallo.Margin = new Padding(3, 4, 3, 4);
            btnIntervallo.Name = "btnIntervallo";
            btnIntervallo.Size = new Size(112, 64);
            btnIntervallo.TabIndex = 4;
            btnIntervallo.Text = "INTERVALLO";
            btnIntervallo.UseVisualStyleBackColor = true;
            // 
            // btnFinePartita
            // 
            btnFinePartita.ContextMenuStrip = cmsButtonRename;
            btnFinePartita.Location = new Point(123, 164);
            btnFinePartita.Margin = new Padding(3, 4, 3, 4);
            btnFinePartita.Name = "btnFinePartita";
            btnFinePartita.Size = new Size(111, 64);
            btnFinePartita.TabIndex = 5;
            btnFinePartita.Text = "FINE PARTITA";
            btnFinePartita.UseVisualStyleBackColor = true;
            // 
            // grpReplay
            // 
            grpReplay.Controls.Add(btnSaveReplay);
            grpReplay.Controls.Add(btnReplayMain);
            grpReplay.Controls.Add(btnReplaySX);
            grpReplay.Controls.Add(btnReplayDX);
            grpReplay.Location = new Point(6, 236);
            grpReplay.Margin = new Padding(3, 4, 3, 4);
            grpReplay.Name = "grpReplay";
            grpReplay.Padding = new Padding(3, 4, 3, 4);
            grpReplay.Size = new Size(235, 160);
            grpReplay.TabIndex = 1;
            grpReplay.TabStop = false;
            grpReplay.Text = "REPLAY";
            // 
            // btnSaveReplay
            // 
            btnSaveReplay.Location = new Point(0, 32);
            btnSaveReplay.Margin = new Padding(3, 4, 3, 4);
            btnSaveReplay.Name = "btnSaveReplay";
            btnSaveReplay.Size = new Size(234, 48);
            btnSaveReplay.TabIndex = 0;
            btnSaveReplay.Text = "SAVE REPLAY";
            btnSaveReplay.UseVisualStyleBackColor = true;
            // 
            // btnReplayMain
            // 
            btnReplayMain.ContextMenuStrip = cmsButtonRename;
            btnReplayMain.Location = new Point(0, 85);
            btnReplayMain.Margin = new Padding(3, 4, 3, 4);
            btnReplayMain.Name = "btnReplayMain";
            btnReplayMain.Size = new Size(97, 48);
            btnReplayMain.TabIndex = 1;
            btnReplayMain.Text = "MAIN";
            btnReplayMain.UseVisualStyleBackColor = true;
            // 
            // btnReplaySX
            // 
            btnReplaySX.ContextMenuStrip = cmsButtonRename;
            btnReplaySX.Location = new Point(105, 85);
            btnReplaySX.Margin = new Padding(3, 4, 3, 4);
            btnReplaySX.Name = "btnReplaySX";
            btnReplaySX.Size = new Size(65, 48);
            btnReplaySX.TabIndex = 2;
            btnReplaySX.Text = "SX";
            btnReplaySX.UseVisualStyleBackColor = true;
            // 
            // btnReplayDX
            // 
            btnReplayDX.ContextMenuStrip = cmsButtonRename;
            btnReplayDX.Location = new Point(174, 85);
            btnReplayDX.Margin = new Padding(3, 4, 3, 4);
            btnReplayDX.Name = "btnReplayDX";
            btnReplayDX.Size = new Size(61, 48);
            btnReplayDX.TabIndex = 3;
            btnReplayDX.Text = "DX";
            btnReplayDX.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(grpSources);
            tabPage2.Location = new Point(4, 29);
            tabPage2.Margin = new Padding(3, 4, 3, 4);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3, 4, 3, 4);
            tabPage2.Size = new Size(240, 798);
            tabPage2.TabIndex = 3;
            tabPage2.Text = "SETTINGS";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // grpSources
            // 
            grpSources.Controls.Add(grpAudioConfig);
            grpSources.Controls.Add(tvObsItems);
            grpSources.Controls.Add(chkUnlockFunctionLayout);
            grpSources.Location = new Point(3, 8);
            grpSources.Margin = new Padding(3, 4, 3, 4);
            grpSources.Name = "grpSources";
            grpSources.Padding = new Padding(3, 4, 3, 4);
            grpSources.Size = new Size(237, 752);
            grpSources.TabIndex = 0;
            grpSources.TabStop = false;
            grpSources.Text = "SCENES / SOURCES";
            // 
            // grpAudioConfig
            // 
            grpAudioConfig.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpAudioConfig.Controls.Add(tblAudioConfig);
            grpAudioConfig.Controls.Add(lstScenes);
            grpAudioConfig.Location = new Point(-2, 608);
            grpAudioConfig.Margin = new Padding(3, 4, 3, 4);
            grpAudioConfig.Name = "grpAudioConfig";
            grpAudioConfig.Padding = new Padding(3, 4, 3, 4);
            grpAudioConfig.Size = new Size(239, 148);
            grpAudioConfig.TabIndex = 5;
            grpAudioConfig.TabStop = false;
            grpAudioConfig.Text = "AUDIO";
            // 
            // tblAudioConfig
            // 
            tblAudioConfig.ColumnCount = 2;
            tblAudioConfig.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22.5641022F));
            tblAudioConfig.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 77.4359F));
            tblAudioConfig.Controls.Add(lblCfgMic1, 0, 0);
            tblAudioConfig.Controls.Add(cmbMic1, 1, 0);
            tblAudioConfig.Controls.Add(lblCfgMic2, 0, 1);
            tblAudioConfig.Controls.Add(cmbMic2, 1, 1);
            tblAudioConfig.Controls.Add(lblCfgWireless, 0, 2);
            tblAudioConfig.Controls.Add(cmbWireless, 1, 2);
            tblAudioConfig.Dock = DockStyle.Fill;
            tblAudioConfig.Location = new Point(3, 24);
            tblAudioConfig.Margin = new Padding(3, 4, 3, 4);
            tblAudioConfig.Name = "tblAudioConfig";
            tblAudioConfig.Padding = new Padding(3, 4, 3, 4);
            tblAudioConfig.RowCount = 3;
            tblAudioConfig.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            tblAudioConfig.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            tblAudioConfig.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            tblAudioConfig.Size = new Size(233, 120);
            tblAudioConfig.TabIndex = 0;
            // 
            // lblCfgMic1
            // 
            lblCfgMic1.Anchor = AnchorStyles.Left;
            lblCfgMic1.AutoSize = true;
            lblCfgMic1.Location = new Point(6, 4);
            lblCfgMic1.Name = "lblCfgMic1";
            lblCfgMic1.Size = new Size(39, 37);
            lblCfgMic1.TabIndex = 0;
            lblCfgMic1.Text = "MIC 1";
            // 
            // cmbMic1
            // 
            cmbMic1.Dock = DockStyle.Fill;
            cmbMic1.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMic1.Location = new Point(57, 8);
            cmbMic1.Margin = new Padding(3, 4, 3, 4);
            cmbMic1.Name = "cmbMic1";
            cmbMic1.Size = new Size(170, 28);
            cmbMic1.TabIndex = 1;
            // 
            // lblCfgMic2
            // 
            lblCfgMic2.Anchor = AnchorStyles.Left;
            lblCfgMic2.AutoSize = true;
            lblCfgMic2.Location = new Point(6, 41);
            lblCfgMic2.Name = "lblCfgMic2";
            lblCfgMic2.Size = new Size(39, 37);
            lblCfgMic2.TabIndex = 2;
            lblCfgMic2.Text = "MIC 2";
            // 
            // cmbMic2
            // 
            cmbMic2.Dock = DockStyle.Fill;
            cmbMic2.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMic2.Location = new Point(57, 45);
            cmbMic2.Margin = new Padding(3, 4, 3, 4);
            cmbMic2.Name = "cmbMic2";
            cmbMic2.Size = new Size(170, 28);
            cmbMic2.TabIndex = 3;
            // 
            // lblCfgWireless
            // 
            lblCfgWireless.Anchor = AnchorStyles.Left;
            lblCfgWireless.AutoSize = true;
            lblCfgWireless.Location = new Point(6, 87);
            lblCfgWireless.Name = "lblCfgWireless";
            lblCfgWireless.Size = new Size(43, 20);
            lblCfgWireless.TabIndex = 4;
            lblCfgWireless.Text = "WIRL";
            // 
            // cmbWireless
            // 
            cmbWireless.Dock = DockStyle.Fill;
            cmbWireless.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbWireless.Location = new Point(57, 82);
            cmbWireless.Margin = new Padding(3, 4, 3, 4);
            cmbWireless.Name = "cmbWireless";
            cmbWireless.Size = new Size(170, 28);
            cmbWireless.TabIndex = 5;
            // 
            // lstScenes
            // 
            lstScenes.FormattingEnabled = true;
            lstScenes.Location = new Point(-7, -221);
            lstScenes.Margin = new Padding(3, 4, 3, 4);
            lstScenes.Name = "lstScenes";
            lstScenes.Size = new Size(83, 24);
            lstScenes.TabIndex = 5;
            lstScenes.Visible = false;
            // 
            // tvObsItems
            // 
            tvObsItems.CheckBoxes = true;
            tvObsItems.Location = new Point(0, 29);
            tvObsItems.Margin = new Padding(3, 4, 3, 4);
            tvObsItems.Name = "tvObsItems";
            tvObsItems.Size = new Size(233, 547);
            tvObsItems.TabIndex = 0;
            // 
            // chkUnlockFunctionLayout
            // 
            chkUnlockFunctionLayout.AutoSize = true;
            chkUnlockFunctionLayout.Location = new Point(0, 580);
            chkUnlockFunctionLayout.Margin = new Padding(3, 4, 3, 4);
            chkUnlockFunctionLayout.Name = "chkUnlockFunctionLayout";
            chkUnlockFunctionLayout.Size = new Size(178, 24);
            chkUnlockFunctionLayout.TabIndex = 1;
            chkUnlockFunctionLayout.Text = "Unlock Buttons Layout";
            chkUnlockFunctionLayout.UseVisualStyleBackColor = true;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(chkAlwaysOnTop);
            tabPage1.Controls.Add(chkAutoConnect);
            tabPage1.Controls.Add(grpProfiles);
            tabPage1.Controls.Add(grpConnection);
            tabPage1.Controls.Add(grpAbout);
            tabPage1.Location = new Point(4, 29);
            tabPage1.Margin = new Padding(3, 4, 3, 4);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(240, 798);
            tabPage1.TabIndex = 2;
            tabPage1.Text = "CONNECT";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // chkAlwaysOnTop
            // 
            chkAlwaysOnTop.AutoSize = true;
            chkAlwaysOnTop.Checked = true;
            chkAlwaysOnTop.CheckState = CheckState.Checked;
            chkAlwaysOnTop.Location = new Point(6, 339);
            chkAlwaysOnTop.Margin = new Padding(3, 4, 3, 4);
            chkAlwaysOnTop.Name = "chkAlwaysOnTop";
            chkAlwaysOnTop.Size = new Size(125, 24);
            chkAlwaysOnTop.TabIndex = 8;
            chkAlwaysOnTop.Text = "Always on top";
            chkAlwaysOnTop.UseVisualStyleBackColor = true;
            // 
            // chkAutoConnect
            // 
            chkAutoConnect.AutoSize = true;
            chkAutoConnect.Location = new Point(6, 171);
            chkAutoConnect.Margin = new Padding(3, 4, 3, 4);
            chkAutoConnect.Name = "chkAutoConnect";
            chkAutoConnect.Size = new Size(246, 24);
            chkAutoConnect.TabIndex = 7;
            chkAutoConnect.Text = "Connect automatically at startup";
            chkAutoConnect.UseVisualStyleBackColor = true;
            // 
            // grpProfiles
            // 
            grpProfiles.Controls.Add(btnDeleteProfile);
            grpProfiles.Controls.Add(btnRenameProfile);
            grpProfiles.Controls.Add(btnNewProfile);
            grpProfiles.Controls.Add(cmbProfile);
            grpProfiles.Controls.Add(lblActiveProfile);
            grpProfiles.Location = new Point(3, 204);
            grpProfiles.Margin = new Padding(3, 4, 3, 4);
            grpProfiles.Name = "grpProfiles";
            grpProfiles.Padding = new Padding(3, 4, 3, 4);
            grpProfiles.Size = new Size(245, 127);
            grpProfiles.TabIndex = 6;
            grpProfiles.TabStop = false;
            grpProfiles.Text = "PROFILES";
            // 
            // btnDeleteProfile
            // 
            btnDeleteProfile.Location = new Point(130, 87);
            btnDeleteProfile.Margin = new Padding(3, 4, 3, 4);
            btnDeleteProfile.Name = "btnDeleteProfile";
            btnDeleteProfile.Size = new Size(86, 31);
            btnDeleteProfile.TabIndex = 4;
            btnDeleteProfile.Text = "DELETE";
            btnDeleteProfile.UseVisualStyleBackColor = true;
            // 
            // btnRenameProfile
            // 
            btnRenameProfile.Location = new Point(24, 87);
            btnRenameProfile.Margin = new Padding(3, 4, 3, 4);
            btnRenameProfile.Name = "btnRenameProfile";
            btnRenameProfile.Size = new Size(86, 31);
            btnRenameProfile.TabIndex = 3;
            btnRenameProfile.Text = "RENAME";
            btnRenameProfile.UseVisualStyleBackColor = true;
            // 
            // btnNewProfile
            // 
            btnNewProfile.Location = new Point(173, 48);
            btnNewProfile.Margin = new Padding(3, 4, 3, 4);
            btnNewProfile.Name = "btnNewProfile";
            btnNewProfile.Size = new Size(55, 31);
            btnNewProfile.TabIndex = 2;
            btnNewProfile.Text = "NEW";
            btnNewProfile.UseVisualStyleBackColor = true;
            // 
            // cmbProfile
            // 
            cmbProfile.FormattingEnabled = true;
            cmbProfile.Location = new Point(3, 48);
            cmbProfile.Margin = new Padding(3, 4, 3, 4);
            cmbProfile.Name = "cmbProfile";
            cmbProfile.Size = new Size(159, 28);
            cmbProfile.TabIndex = 1;
            // 
            // lblActiveProfile
            // 
            lblActiveProfile.AutoSize = true;
            lblActiveProfile.Location = new Point(3, 24);
            lblActiveProfile.Name = "lblActiveProfile";
            lblActiveProfile.Size = new Size(98, 20);
            lblActiveProfile.TabIndex = 0;
            lblActiveProfile.Text = "Active profile";
            // 
            // grpConnection
            // 
            grpConnection.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpConnection.Controls.Add(txtHost);
            grpConnection.Controls.Add(lblHost);
            grpConnection.Controls.Add(lblPorta);
            grpConnection.Controls.Add(txtPort);
            grpConnection.Controls.Add(lblPassword);
            grpConnection.Controls.Add(txtPassword);
            grpConnection.Controls.Add(btnConnect);
            grpConnection.Location = new Point(3, 4);
            grpConnection.Margin = new Padding(3, 4, 3, 4);
            grpConnection.Name = "grpConnection";
            grpConnection.Padding = new Padding(3, 4, 3, 4);
            grpConnection.Size = new Size(240, 161);
            grpConnection.TabIndex = 5;
            grpConnection.TabStop = false;
            grpConnection.Text = "OBS CONNECTION";
            // 
            // txtHost
            // 
            txtHost.BackColor = SystemColors.Window;
            txtHost.Location = new Point(47, 28);
            txtHost.Margin = new Padding(3, 4, 3, 4);
            txtHost.Name = "txtHost";
            txtHost.Size = new Size(180, 27);
            txtHost.TabIndex = 1;
            txtHost.Text = "localhost";
            // 
            // lblHost
            // 
            lblHost.AutoSize = true;
            lblHost.Location = new Point(9, 32);
            lblHost.Name = "lblHost";
            lblHost.Size = new Size(40, 20);
            lblHost.TabIndex = 0;
            lblHost.Text = "Host";
            // 
            // lblPorta
            // 
            lblPorta.AutoSize = true;
            lblPorta.Location = new Point(7, 63);
            lblPorta.Name = "lblPorta";
            lblPorta.Size = new Size(43, 20);
            lblPorta.TabIndex = 2;
            lblPorta.Text = "Port";
            // 
            // txtPort
            // 
            txtPort.Location = new Point(3, 87);
            txtPort.Margin = new Padding(3, 4, 3, 4);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(77, 27);
            txtPort.TabIndex = 3;
            txtPort.Text = "4455";
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(130, 63);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(70, 20);
            lblPassword.TabIndex = 4;
            lblPassword.Text = "Password";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(102, 87);
            txtPassword.Margin = new Padding(3, 4, 3, 4);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(125, 27);
            txtPassword.TabIndex = 5;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(7, 121);
            btnConnect.Margin = new Padding(3, 4, 3, 4);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(215, 32);
            btnConnect.TabIndex = 6;
            btnConnect.Text = "CONNECT";
            btnConnect.UseVisualStyleBackColor = true;
            // 
            // grpAbout
            // 
            grpAbout.Controls.Add(lblAboutDevelopment);
            grpAbout.Controls.Add(lblAboutDisclaimer);
            grpAbout.Controls.Add(lblAboutOpenSource);
            grpAbout.Controls.Add(lnkObsForum);
            grpAbout.Controls.Add(lnkGitHub);
            grpAbout.Controls.Add(lnkSupportProject);
            grpAbout.Controls.Add(lblAboutAuthor);
            grpAbout.Controls.Add(lblAboutVersion);
            grpAbout.Controls.Add(lblAboutProduct);
            grpAbout.Location = new Point(3, 372);
            grpAbout.Margin = new Padding(3, 4, 3, 4);
            grpAbout.Name = "grpAbout";
            grpAbout.Padding = new Padding(3, 4, 3, 4);
            grpAbout.Size = new Size(240, 275);
            grpAbout.TabIndex = 9;
            grpAbout.TabStop = false;
            grpAbout.Text = "ABOUT";
            // 
            // lblAboutProduct
            // 
            lblAboutProduct.AutoSize = true;
            lblAboutProduct.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblAboutProduct.Location = new Point(9, 26);
            lblAboutProduct.Name = "lblAboutProduct";
            lblAboutProduct.Size = new Size(165, 20);
            lblAboutProduct.TabIndex = 0;
            lblAboutProduct.Text = "Pokoto OBS Control Deck";
            // 
            // lblAboutVersion
            // 
            lblAboutVersion.AutoSize = true;
            lblAboutVersion.Location = new Point(9, 50);
            lblAboutVersion.Name = "lblAboutVersion";
            lblAboutVersion.Size = new Size(60, 20);
            lblAboutVersion.TabIndex = 1;
            lblAboutVersion.Text = "Version";
            // 
            // lblAboutAuthor
            // 
            lblAboutAuthor.AutoSize = true;
            lblAboutAuthor.Location = new Point(9, 74);
            lblAboutAuthor.Name = "lblAboutAuthor";
            lblAboutAuthor.Size = new Size(120, 20);
            lblAboutAuthor.TabIndex = 2;
            lblAboutAuthor.Text = "Created by Pokoto";
            // 
            // lnkGitHub
            // 
            lnkGitHub.AutoSize = true;
            lnkGitHub.Location = new Point(9, 102);
            lnkGitHub.Name = "lnkGitHub";
            lnkGitHub.Size = new Size(60, 20);
            lnkGitHub.TabIndex = 3;
            lnkGitHub.TabStop = true;
            lnkGitHub.Text = "GitHub";
            lnkGitHub.LinkClicked += lnkGitHub_LinkClicked;
            // 
            // lnkObsForum
            // 
            lnkObsForum.AutoSize = true;
            lnkObsForum.Location = new Point(90, 102);
            lnkObsForum.Name = "lnkObsForum";
            lnkObsForum.Size = new Size(80, 20);
            lnkObsForum.TabIndex = 4;
            lnkObsForum.TabStop = true;
            lnkObsForum.Text = "OBS Forum";
            lnkObsForum.LinkClicked += lnkObsForum_LinkClicked;
            // 
            // lnkSupportProject
            // 
            lnkSupportProject.AutoSize = true;
            lnkSupportProject.Location = new Point(9, 130);
            lnkSupportProject.Name = "lnkSupportProject";
            lnkSupportProject.Size = new Size(120, 20);
            lnkSupportProject.TabIndex = 5;
            lnkSupportProject.TabStop = true;
            lnkSupportProject.Text = "Support this project";
            lnkSupportProject.LinkClicked += lnkSupportProject_LinkClicked;
            // 
            // lblAboutOpenSource
            // 
            lblAboutOpenSource.AutoSize = true;
            lblAboutOpenSource.Location = new Point(9, 154);
            lblAboutOpenSource.Name = "lblAboutOpenSource";
            lblAboutOpenSource.Size = new Size(130, 20);
            lblAboutOpenSource.TabIndex = 5;
            lblAboutOpenSource.Text = "Free & Open Source";
            // 
            // lblAboutDisclaimer
            // 
            lblAboutDisclaimer.Font = new Font("Segoe UI", 8F);
            lblAboutDisclaimer.ForeColor = SystemColors.GrayText;
            lblAboutDisclaimer.Location = new Point(9, 186);
            lblAboutDisclaimer.Name = "lblAboutDisclaimer";
            lblAboutDisclaimer.Size = new Size(225, 36);
            lblAboutDisclaimer.TabIndex = 6;
            lblAboutDisclaimer.Text = "Independent community project." + Environment.NewLine + "Not affiliated with the OBS Project.";
            // 
            // lblAboutDevelopment
            // 
            lblAboutDevelopment.Font = new Font("Segoe UI", 8F);
            lblAboutDevelopment.ForeColor = SystemColors.GrayText;
            lblAboutDevelopment.Location = new Point(9, 226);
            lblAboutDevelopment.Name = "lblAboutDevelopment";
            lblAboutDevelopment.Size = new Size(225, 36);
            lblAboutDevelopment.TabIndex = 7;
            lblAboutDevelopment.Text = "Developed with assistance from" + Environment.NewLine + "Microsoft Copilot and GitHub Copilot";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(233, 852);
            Controls.Add(tabMain);
            Controls.Add(lblObsStatus);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimumSize = new Size(249, 678);
            Name = "MainForm";
            Text = "Pokoto OBS Control Deck";
            TopMost = true;
            tabMain.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            grpFunctionTexts.ResumeLayout(false);
            cmsButtonRename.ResumeLayout(false);
            grpAudio.ResumeLayout(false);
            grpScenes.ResumeLayout(false);
            grpReplay.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            grpSources.ResumeLayout(false);
            grpSources.PerformLayout();
            grpAudioConfig.ResumeLayout(false);
            tblAudioConfig.ResumeLayout(false);
            tblAudioConfig.PerformLayout();
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            grpProfiles.ResumeLayout(false);
            grpProfiles.PerformLayout();
            grpConnection.ResumeLayout(false);
            grpConnection.PerformLayout();
            grpAbout.ResumeLayout(false);
            grpAbout.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TabPage tabPage1;
        private GroupBox grpAbout;
        private Label lblAboutProduct;
        private Label lblAboutVersion;
        private Label lblAboutAuthor;
        private LinkLabel lnkGitHub;
        private LinkLabel lnkObsForum;
        private LinkLabel lnkSupportProject;
        private Label lblAboutOpenSource;
        private Label lblAboutDisclaimer;
        private Label lblAboutDevelopment;
        private GroupBox grpConnection;
        private TextBox txtHost;
        private Label lblHost;
        private Label lblPorta;
        private TextBox txtPort;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnConnect;
        private GroupBox grpProfiles;
        private Button btnDeleteProfile;
        private Button btnRenameProfile;
        private Button btnNewProfile;
        private ComboBox cmbProfile;
        private Label lblActiveProfile;
        private CheckBox chkAutoConnect;
        private TabPage tabPage2;
        private GroupBox grpSources;
        private TreeView tvObsItems;
        private GroupBox grpAudio;
        private Button btnWireless;
        private Button btnMic2;
        private Button btnMic1;
        private ContextMenuStrip cmsButtonRename;
        private ToolStripMenuItem mnuRenameButton;
        private ToolStripMenuItem mnuResetButtonName;
        private Button btnF1Home;
        private CheckBox chkAlwaysOnTop;
        private GroupBox grpAudioConfig;
        private TableLayoutPanel tblAudioConfig;
        private Label lblCfgMic1;
        private ComboBox cmbMic1;
        private Label lblCfgMic2;
        private ComboBox cmbMic2;
        private Label lblCfgWireless;
        private ComboBox cmbWireless;
        private ListBox lstScenes;
        private CheckBox chkUnlockFunctionLayout;
    }
}
