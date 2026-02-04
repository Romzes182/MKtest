namespace MKtest
{
    partial class MainForm
    {
        #region Поля компонентов
        private System.ComponentModel.IContainer components = null;
        #endregion

        #region Метод Dispose
        /// <summary>
        /// Clean up any resources being used.
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
        #endregion

        #region Код, сгенерированный конструктором форм
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            mainTabControl = new TabControl();
            mainTabPage = new TabPage();
            splitContainerMain = new SplitContainer();
            panelLeft = new Panel();
            leftFlowLayout = new FlowLayoutPanel();
            beelinkCollapsiblePanel = new Panel();
            beelinkContentPanel = new Panel();
            timeGroupBox = new GroupBox();
            timeSetButton = new Button();
            manualTimePicker = new DateTimePicker();
            manualDatePicker = new DateTimePicker();
            timeDisableNTPButton = new Button();
            timeEnableNTPButton = new Button();
            timeCheckButton = new Button();
            beelinkTestButton = new Button();
            beelinkDisconnectButton = new Button();
            beelinkConnectButton = new Button();
            beelinkStatusLabel = new Label();
            beelinkHeaderPanel = new Panel();
            beelinkHeaderLabel = new Label();
            webServerCollapsiblePanel = new Panel();
            webServerContentPanel = new Panel();
            webServerStatusLabel = new Label();
            webServerStopButton = new Button();
            webServerStartButton = new Button();
            webServerHeaderPanel = new Panel();
            webServerHeaderLabel = new Label();
            panelRight = new Panel();
            demoCollapsiblePanel = new Panel();
            demoContentPanel = new Panel();
            lblDemoStatus = new Label();
            btnStopDemo = new Button();
            btnStartDemo = new Button();
            cmbDemoScenarios = new ComboBox();
            demoHeaderPanel = new Panel();
            demoHeaderLabel = new Label();
            logPanel = new Panel();
            beelinkLogTextBox = new TextBox();
            beelinkClearLogButton = new Button();
            settingsTabPage = new TabPage();
            webServerGroupBox = new GroupBox();
            webServerResetButton = new Button();
            webServerSaveButton = new Button();
            webServerPortNumeric = new NumericUpDown();
            webServerPortLabel = new Label();
            webServerIpTextBox = new TextBox();
            webServerIpLabel = new Label();
            sshBeelinkGroupBox = new GroupBox();
            resetButton = new Button();
            saveButton = new Button();
            passwordRootTextBox = new TextBox();
            passwordRootLabel = new Label();
            passwordUserTextBox = new TextBox();
            passwordUserLabel = new Label();
            userTextBox = new TextBox();
            userLabel = new Label();
            portNumeric = new NumericUpDown();
            portLabel = new Label();
            ipTextBox = new TextBox();
            ipLabel = new Label();
            mainTabControl.SuspendLayout();
            mainTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            panelLeft.SuspendLayout();
            leftFlowLayout.SuspendLayout();
            beelinkCollapsiblePanel.SuspendLayout();
            beelinkContentPanel.SuspendLayout();
            timeGroupBox.SuspendLayout();
            beelinkHeaderPanel.SuspendLayout();
            webServerCollapsiblePanel.SuspendLayout();
            webServerContentPanel.SuspendLayout();
            webServerHeaderPanel.SuspendLayout();
            panelRight.SuspendLayout();
            demoCollapsiblePanel.SuspendLayout();
            demoContentPanel.SuspendLayout();
            demoHeaderPanel.SuspendLayout();
            logPanel.SuspendLayout();
            settingsTabPage.SuspendLayout();
            webServerGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webServerPortNumeric).BeginInit();
            sshBeelinkGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)portNumeric).BeginInit();
            SuspendLayout();
            // 
            // mainTabControl
            // 
            mainTabControl.Controls.Add(mainTabPage);
            mainTabControl.Controls.Add(settingsTabPage);
            mainTabControl.Dock = DockStyle.Fill;
            mainTabControl.Location = new Point(0, 0);
            mainTabControl.Name = "mainTabControl";
            mainTabControl.SelectedIndex = 0;
            mainTabControl.Size = new Size(1008, 729);
            mainTabControl.TabIndex = 0;
            // 
            // mainTabPage
            // 
            mainTabPage.Controls.Add(splitContainerMain);
            mainTabPage.Controls.Add(logPanel);
            mainTabPage.Location = new Point(4, 24);
            mainTabPage.Name = "mainTabPage";
            mainTabPage.Padding = new Padding(3);
            mainTabPage.Size = new Size(1000, 701);
            mainTabPage.TabIndex = 0;
            mainTabPage.Text = "Главная";
            mainTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.Location = new Point(3, 3);
            splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(panelLeft);
            splitContainerMain.Panel1MinSize = 300;
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(panelRight);
            splitContainerMain.Panel2.Paint += splitContainerMain_Panel2_Paint;
            splitContainerMain.Panel2MinSize = 300;
            splitContainerMain.Size = new Size(994, 576);
            splitContainerMain.SplitterDistance = 401;
            splitContainerMain.TabIndex = 0;
            // 
            // panelLeft
            // 
            panelLeft.Controls.Add(leftFlowLayout);
            panelLeft.Location = new Point(0, 0);
            panelLeft.Name = "panelLeft";
            panelLeft.Padding = new Padding(10);
            panelLeft.Size = new Size(395, 320);
            panelLeft.TabIndex = 0;
            // 
            // leftFlowLayout
            // 
            leftFlowLayout.AutoScroll = true;
            leftFlowLayout.Controls.Add(beelinkCollapsiblePanel);
            leftFlowLayout.Controls.Add(webServerCollapsiblePanel);
            leftFlowLayout.FlowDirection = FlowDirection.TopDown;
            leftFlowLayout.Location = new Point(10, 10);
            leftFlowLayout.Name = "leftFlowLayout";
            leftFlowLayout.Size = new Size(376, 297);
            leftFlowLayout.TabIndex = 0;
            leftFlowLayout.WrapContents = false;
            // 
            // beelinkCollapsiblePanel
            // 
            beelinkCollapsiblePanel.BorderStyle = BorderStyle.FixedSingle;
            beelinkCollapsiblePanel.Controls.Add(beelinkContentPanel);
            beelinkCollapsiblePanel.Controls.Add(beelinkHeaderPanel);
            beelinkCollapsiblePanel.Location = new Point(3, 3);
            beelinkCollapsiblePanel.Margin = new Padding(3, 3, 3, 10);
            beelinkCollapsiblePanel.Name = "beelinkCollapsiblePanel";
            beelinkCollapsiblePanel.Size = new Size(367, 190);
            beelinkCollapsiblePanel.TabIndex = 0;
            // 
            // beelinkContentPanel
            // 
            beelinkContentPanel.Controls.Add(timeGroupBox);
            beelinkContentPanel.Controls.Add(beelinkTestButton);
            beelinkContentPanel.Controls.Add(beelinkDisconnectButton);
            beelinkContentPanel.Controls.Add(beelinkConnectButton);
            beelinkContentPanel.Controls.Add(beelinkStatusLabel);
            beelinkContentPanel.Dock = DockStyle.Fill;
            beelinkContentPanel.Location = new Point(0, 24);
            beelinkContentPanel.Name = "beelinkContentPanel";
            beelinkContentPanel.Size = new Size(365, 164);
            beelinkContentPanel.TabIndex = 1;
            // 
            // timeGroupBox
            // 
            timeGroupBox.Controls.Add(timeSetButton);
            timeGroupBox.Controls.Add(manualTimePicker);
            timeGroupBox.Controls.Add(manualDatePicker);
            timeGroupBox.Controls.Add(timeDisableNTPButton);
            timeGroupBox.Controls.Add(timeEnableNTPButton);
            timeGroupBox.Controls.Add(timeCheckButton);
            timeGroupBox.Location = new Point(10, 65);
            timeGroupBox.Name = "timeGroupBox";
            timeGroupBox.Size = new Size(352, 88);
            timeGroupBox.TabIndex = 4;
            timeGroupBox.TabStop = false;
            timeGroupBox.Text = "Управление временем";
            // 
            // timeSetButton
            // 
            timeSetButton.Location = new Point(229, 51);
            timeSetButton.Name = "timeSetButton";
            timeSetButton.Size = new Size(118, 25);
            timeSetButton.TabIndex = 5;
            timeSetButton.Text = "Установить время";
            timeSetButton.UseVisualStyleBackColor = true;
            timeSetButton.Click += timeSetButton_Click;
            // 
            // manualTimePicker
            // 
            manualTimePicker.Format = DateTimePickerFormat.Time;
            manualTimePicker.Location = new Point(140, 55);
            manualTimePicker.Name = "manualTimePicker";
            manualTimePicker.ShowUpDown = true;
            manualTimePicker.Size = new Size(68, 23);
            manualTimePicker.TabIndex = 4;
            manualTimePicker.Value = new DateTime(2026, 1, 31, 16, 27, 18, 279);
            // 
            // manualDatePicker
            // 
            manualDatePicker.Format = DateTimePickerFormat.Short;
            manualDatePicker.Location = new Point(10, 55);
            manualDatePicker.Name = "manualDatePicker";
            manualDatePicker.Size = new Size(93, 23);
            manualDatePicker.TabIndex = 3;
            manualDatePicker.Value = new DateTime(2026, 1, 31, 16, 27, 18, 280);
            // 
            // timeDisableNTPButton
            // 
            timeDisableNTPButton.Location = new Point(240, 20);
            timeDisableNTPButton.Name = "timeDisableNTPButton";
            timeDisableNTPButton.Size = new Size(107, 25);
            timeDisableNTPButton.TabIndex = 2;
            timeDisableNTPButton.Text = "Выключить NTP";
            timeDisableNTPButton.UseVisualStyleBackColor = true;
            timeDisableNTPButton.Click += timeDisableNTPButton_Click;
            // 
            // timeEnableNTPButton
            // 
            timeEnableNTPButton.Location = new Point(121, 20);
            timeEnableNTPButton.Name = "timeEnableNTPButton";
            timeEnableNTPButton.Size = new Size(100, 25);
            timeEnableNTPButton.TabIndex = 1;
            timeEnableNTPButton.Text = "Включить NTP";
            timeEnableNTPButton.UseVisualStyleBackColor = true;
            timeEnableNTPButton.Click += timeEnableNTPButton_Click;
            // 
            // timeCheckButton
            // 
            timeCheckButton.Location = new Point(10, 20);
            timeCheckButton.Name = "timeCheckButton";
            timeCheckButton.Size = new Size(105, 25);
            timeCheckButton.TabIndex = 0;
            timeCheckButton.Text = "Статус времени";
            timeCheckButton.UseVisualStyleBackColor = true;
            timeCheckButton.Click += timeCheckButton_Click;
            // 
            // beelinkTestButton
            // 
            beelinkTestButton.Location = new Point(3, 33);
            beelinkTestButton.Name = "beelinkTestButton";
            beelinkTestButton.Size = new Size(90, 21);
            beelinkTestButton.TabIndex = 3;
            beelinkTestButton.Text = "Тест ";
            beelinkTestButton.UseVisualStyleBackColor = true;
            beelinkTestButton.Click += beelinkTestButton_Click;
            // 
            // beelinkDisconnectButton
            // 
            beelinkDisconnectButton.Location = new Point(99, 6);
            beelinkDisconnectButton.Name = "beelinkDisconnectButton";
            beelinkDisconnectButton.Size = new Size(90, 21);
            beelinkDisconnectButton.TabIndex = 2;
            beelinkDisconnectButton.Text = "Отключить";
            beelinkDisconnectButton.UseVisualStyleBackColor = true;
            beelinkDisconnectButton.Click += beelinkDisconnectButton_Click;
            // 
            // beelinkConnectButton
            // 
            beelinkConnectButton.Location = new Point(3, 6);
            beelinkConnectButton.Name = "beelinkConnectButton";
            beelinkConnectButton.Size = new Size(90, 21);
            beelinkConnectButton.TabIndex = 1;
            beelinkConnectButton.Text = "Подключить";
            beelinkConnectButton.UseVisualStyleBackColor = true;
            beelinkConnectButton.Click += beelinkConnectButton_Click;
            // 
            // beelinkStatusLabel
            // 
            beelinkStatusLabel.AutoSize = true;
            beelinkStatusLabel.Location = new Point(195, 9);
            beelinkStatusLabel.Name = "beelinkStatusLabel";
            beelinkStatusLabel.Size = new Size(137, 15);
            beelinkStatusLabel.TabIndex = 0;
            beelinkStatusLabel.Text = "Статус: Не подключено";
            // 
            // beelinkHeaderPanel
            // 
            beelinkHeaderPanel.BackColor = SystemColors.ActiveCaption;
            beelinkHeaderPanel.Controls.Add(beelinkHeaderLabel);
            beelinkHeaderPanel.Cursor = Cursors.Hand;
            beelinkHeaderPanel.Dock = DockStyle.Top;
            beelinkHeaderPanel.Location = new Point(0, 0);
            beelinkHeaderPanel.Name = "beelinkHeaderPanel";
            beelinkHeaderPanel.Size = new Size(365, 24);
            beelinkHeaderPanel.TabIndex = 0;
            beelinkHeaderPanel.Click += CollapsiblePanelHeader_Click;
            // 
            // beelinkHeaderLabel
            // 
            beelinkHeaderLabel.Dock = DockStyle.Fill;
            beelinkHeaderLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            beelinkHeaderLabel.Location = new Point(0, 0);
            beelinkHeaderLabel.Name = "beelinkHeaderLabel";
            beelinkHeaderLabel.Size = new Size(365, 24);
            beelinkHeaderLabel.TabIndex = 0;
            beelinkHeaderLabel.Text = "SSH Beelink";
            // 
            // webServerCollapsiblePanel
            // 
            webServerCollapsiblePanel.BorderStyle = BorderStyle.FixedSingle;
            webServerCollapsiblePanel.Controls.Add(webServerContentPanel);
            webServerCollapsiblePanel.Controls.Add(webServerHeaderPanel);
            webServerCollapsiblePanel.Location = new Point(3, 206);
            webServerCollapsiblePanel.Margin = new Padding(3, 3, 3, 10);
            webServerCollapsiblePanel.Name = "webServerCollapsiblePanel";
            webServerCollapsiblePanel.Size = new Size(366, 81);
            webServerCollapsiblePanel.TabIndex = 2;
            // 
            // webServerContentPanel
            // 
            webServerContentPanel.Controls.Add(webServerStatusLabel);
            webServerContentPanel.Controls.Add(webServerStopButton);
            webServerContentPanel.Controls.Add(webServerStartButton);
            webServerContentPanel.Dock = DockStyle.Fill;
            webServerContentPanel.Location = new Point(0, 24);
            webServerContentPanel.Name = "webServerContentPanel";
            webServerContentPanel.Size = new Size(364, 55);
            webServerContentPanel.TabIndex = 1;
            // 
            // webServerStatusLabel
            // 
            webServerStatusLabel.AutoSize = true;
            webServerStatusLabel.Location = new Point(208, 15);
            webServerStatusLabel.Name = "webServerStatusLabel";
            webServerStatusLabel.Size = new Size(115, 15);
            webServerStatusLabel.TabIndex = 2;
            webServerStatusLabel.Text = "Статус: Остановлен";
            // 
            // webServerStopButton
            // 
            webServerStopButton.Enabled = false;
            webServerStopButton.Location = new Point(110, 10);
            webServerStopButton.Name = "webServerStopButton";
            webServerStopButton.Size = new Size(79, 25);
            webServerStopButton.TabIndex = 1;
            webServerStopButton.Text = "Остановить";
            webServerStopButton.UseVisualStyleBackColor = true;
            // 
            // webServerStartButton
            // 
            webServerStartButton.Location = new Point(10, 10);
            webServerStartButton.Name = "webServerStartButton";
            webServerStartButton.Size = new Size(73, 25);
            webServerStartButton.TabIndex = 0;
            webServerStartButton.Text = "Запустить";
            webServerStartButton.UseVisualStyleBackColor = true;
            // 
            // webServerHeaderPanel
            // 
            webServerHeaderPanel.BackColor = SystemColors.ActiveCaption;
            webServerHeaderPanel.Controls.Add(webServerHeaderLabel);
            webServerHeaderPanel.Cursor = Cursors.Hand;
            webServerHeaderPanel.Dock = DockStyle.Top;
            webServerHeaderPanel.Location = new Point(0, 0);
            webServerHeaderPanel.Name = "webServerHeaderPanel";
            webServerHeaderPanel.Size = new Size(364, 24);
            webServerHeaderPanel.TabIndex = 0;
            webServerHeaderPanel.Click += CollapsiblePanelHeader_Click;
            // 
            // webServerHeaderLabel
            // 
            webServerHeaderLabel.Dock = DockStyle.Fill;
            webServerHeaderLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            webServerHeaderLabel.Location = new Point(0, 0);
            webServerHeaderLabel.Name = "webServerHeaderLabel";
            webServerHeaderLabel.Size = new Size(364, 24);
            webServerHeaderLabel.TabIndex = 0;
            webServerHeaderLabel.Text = "Веб-сервер";
            // 
            // panelRight
            // 
            panelRight.Controls.Add(demoCollapsiblePanel);
            panelRight.Location = new Point(3, 10);
            panelRight.Name = "panelRight";
            panelRight.Padding = new Padding(10);
            panelRight.Size = new Size(581, 187);
            panelRight.TabIndex = 0;
            // 
            // demoCollapsiblePanel
            // 
            demoCollapsiblePanel.BorderStyle = BorderStyle.FixedSingle;
            demoCollapsiblePanel.Controls.Add(demoContentPanel);
            demoCollapsiblePanel.Controls.Add(demoHeaderPanel);
            demoCollapsiblePanel.Location = new Point(10, 4);
            demoCollapsiblePanel.Margin = new Padding(0, 0, 0, 10);
            demoCollapsiblePanel.Name = "demoCollapsiblePanel";
            demoCollapsiblePanel.Size = new Size(244, 102);
            demoCollapsiblePanel.TabIndex = 8;
            // 
            // demoContentPanel
            // 
            demoContentPanel.Controls.Add(lblDemoStatus);
            demoContentPanel.Controls.Add(btnStopDemo);
            demoContentPanel.Controls.Add(btnStartDemo);
            demoContentPanel.Controls.Add(cmbDemoScenarios);
            demoContentPanel.Dock = DockStyle.Fill;
            demoContentPanel.Location = new Point(0, 23);
            demoContentPanel.Name = "demoContentPanel";
            demoContentPanel.Size = new Size(242, 77);
            demoContentPanel.TabIndex = 1;
            demoContentPanel.Paint += demoContentPanel_Paint;
            // 
            // lblDemoStatus
            // 
            lblDemoStatus.AutoSize = true;
            lblDemoStatus.Location = new Point(19, 47);
            lblDemoStatus.Name = "lblDemoStatus";
            lblDemoStatus.Size = new Size(115, 15);
            lblDemoStatus.TabIndex = 3;
            lblDemoStatus.Text = "Статус: Остановлен";
            // 
            // btnStopDemo
            // 
            btnStopDemo.Enabled = false;
            btnStopDemo.Location = new Point(162, 41);
            btnStopDemo.Name = "btnStopDemo";
            btnStopDemo.Size = new Size(71, 25);
            btnStopDemo.TabIndex = 2;
            btnStopDemo.Text = "Остановить";
            btnStopDemo.UseVisualStyleBackColor = true;
            // 
            // btnStartDemo
            // 
            btnStartDemo.Location = new Point(162, 6);
            btnStartDemo.Name = "btnStartDemo";
            btnStartDemo.Size = new Size(71, 25);
            btnStartDemo.TabIndex = 1;
            btnStartDemo.Text = "Запустить";
            btnStartDemo.UseVisualStyleBackColor = true;
            // 
            // cmbDemoScenarios
            // 
            cmbDemoScenarios.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDemoScenarios.FormattingEnabled = true;
            cmbDemoScenarios.Location = new Point(10, 15);
            cmbDemoScenarios.Name = "cmbDemoScenarios";
            cmbDemoScenarios.Size = new Size(135, 23);
            cmbDemoScenarios.TabIndex = 0;
            // 
            // demoHeaderPanel
            // 
            demoHeaderPanel.BackColor = SystemColors.ActiveCaption;
            demoHeaderPanel.Controls.Add(demoHeaderLabel);
            demoHeaderPanel.Cursor = Cursors.Hand;
            demoHeaderPanel.Dock = DockStyle.Top;
            demoHeaderPanel.Location = new Point(0, 0);
            demoHeaderPanel.Name = "demoHeaderPanel";
            demoHeaderPanel.Size = new Size(242, 23);
            demoHeaderPanel.TabIndex = 0;
            demoHeaderPanel.Click += CollapsiblePanelHeader_Click;
            // 
            // demoHeaderLabel
            // 
            demoHeaderLabel.Dock = DockStyle.Fill;
            demoHeaderLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            demoHeaderLabel.Location = new Point(0, 0);
            demoHeaderLabel.Name = "demoHeaderLabel";
            demoHeaderLabel.Size = new Size(242, 23);
            demoHeaderLabel.TabIndex = 0;
            demoHeaderLabel.Text = "Демо-сценарии";
            // 
            // logPanel
            // 
            logPanel.BorderStyle = BorderStyle.FixedSingle;
            logPanel.Controls.Add(beelinkLogTextBox);
            logPanel.Controls.Add(beelinkClearLogButton);
            logPanel.Dock = DockStyle.Bottom;
            logPanel.Location = new Point(3, 579);
            logPanel.Name = "logPanel";
            logPanel.Size = new Size(994, 119);
            logPanel.TabIndex = 7;
            // 
            // beelinkLogTextBox
            // 
            beelinkLogTextBox.Dock = DockStyle.Fill;
            beelinkLogTextBox.Font = new Font("Consolas", 9F);
            beelinkLogTextBox.Location = new Point(0, 0);
            beelinkLogTextBox.Multiline = true;
            beelinkLogTextBox.Name = "beelinkLogTextBox";
            beelinkLogTextBox.ReadOnly = true;
            beelinkLogTextBox.ScrollBars = ScrollBars.Vertical;
            beelinkLogTextBox.Size = new Size(992, 86);
            beelinkLogTextBox.TabIndex = 5;
            // 
            // beelinkClearLogButton
            // 
            beelinkClearLogButton.Dock = DockStyle.Bottom;
            beelinkClearLogButton.Location = new Point(0, 86);
            beelinkClearLogButton.Name = "beelinkClearLogButton";
            beelinkClearLogButton.Size = new Size(992, 31);
            beelinkClearLogButton.TabIndex = 6;
            beelinkClearLogButton.Text = "Очистить лог";
            beelinkClearLogButton.UseVisualStyleBackColor = true;
            beelinkClearLogButton.Click += beelinkClearLogButton_Click;
            // 
            // settingsTabPage
            // 
            settingsTabPage.Controls.Add(webServerGroupBox);
            settingsTabPage.Controls.Add(sshBeelinkGroupBox);
            settingsTabPage.Location = new Point(4, 24);
            settingsTabPage.Name = "settingsTabPage";
            settingsTabPage.Padding = new Padding(3);
            settingsTabPage.Size = new Size(1000, 701);
            settingsTabPage.TabIndex = 1;
            settingsTabPage.Text = "Настройки";
            settingsTabPage.UseVisualStyleBackColor = true;
            // 
            // webServerGroupBox
            // 
            webServerGroupBox.Controls.Add(webServerResetButton);
            webServerGroupBox.Controls.Add(webServerSaveButton);
            webServerGroupBox.Controls.Add(webServerPortNumeric);
            webServerGroupBox.Controls.Add(webServerPortLabel);
            webServerGroupBox.Controls.Add(webServerIpTextBox);
            webServerGroupBox.Controls.Add(webServerIpLabel);
            webServerGroupBox.Location = new Point(10, 190);
            webServerGroupBox.Name = "webServerGroupBox";
            webServerGroupBox.Size = new Size(319, 80);
            webServerGroupBox.TabIndex = 1;
            webServerGroupBox.TabStop = false;
            webServerGroupBox.Text = "Веб-сервер";
            // 
            // webServerResetButton
            // 
            webServerResetButton.Location = new Point(228, 49);
            webServerResetButton.Name = "webServerResetButton";
            webServerResetButton.Size = new Size(80, 25);
            webServerResetButton.TabIndex = 5;
            webServerResetButton.Text = "Сбросить";
            webServerResetButton.UseVisualStyleBackColor = true;
            webServerResetButton.Click += webServerResetButton_Click;
            // 
            // webServerSaveButton
            // 
            webServerSaveButton.Location = new Point(228, 18);
            webServerSaveButton.Name = "webServerSaveButton";
            webServerSaveButton.Size = new Size(80, 25);
            webServerSaveButton.TabIndex = 4;
            webServerSaveButton.Text = "Сохранить";
            webServerSaveButton.UseVisualStyleBackColor = true;
            webServerSaveButton.Click += webServerSaveButton_Click;
            // 
            // webServerPortNumeric
            // 
            webServerPortNumeric.Location = new Point(121, 49);
            webServerPortNumeric.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            webServerPortNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            webServerPortNumeric.Name = "webServerPortNumeric";
            webServerPortNumeric.Size = new Size(80, 23);
            webServerPortNumeric.TabIndex = 3;
            webServerPortNumeric.Value = new decimal(new int[] { 8080, 0, 0, 0 });
            // 
            // webServerPortLabel
            // 
            webServerPortLabel.AutoSize = true;
            webServerPortLabel.Location = new Point(10, 53);
            webServerPortLabel.Name = "webServerPortLabel";
            webServerPortLabel.Size = new Size(38, 15);
            webServerPortLabel.TabIndex = 2;
            webServerPortLabel.Text = "Порт:";
            // 
            // webServerIpTextBox
            // 
            webServerIpTextBox.Location = new Point(121, 20);
            webServerIpTextBox.Name = "webServerIpTextBox";
            webServerIpTextBox.Size = new Size(80, 23);
            webServerIpTextBox.TabIndex = 1;
            webServerIpTextBox.Text = "*";
            // 
            // webServerIpLabel
            // 
            webServerIpLabel.AutoSize = true;
            webServerIpLabel.Location = new Point(10, 28);
            webServerIpLabel.Name = "webServerIpLabel";
            webServerIpLabel.Size = new Size(54, 15);
            webServerIpLabel.TabIndex = 0;
            webServerIpLabel.Text = "IP адрес:";
            // 
            // sshBeelinkGroupBox
            // 
            sshBeelinkGroupBox.Controls.Add(resetButton);
            sshBeelinkGroupBox.Controls.Add(saveButton);
            sshBeelinkGroupBox.Controls.Add(passwordRootTextBox);
            sshBeelinkGroupBox.Controls.Add(passwordRootLabel);
            sshBeelinkGroupBox.Controls.Add(passwordUserTextBox);
            sshBeelinkGroupBox.Controls.Add(passwordUserLabel);
            sshBeelinkGroupBox.Controls.Add(userTextBox);
            sshBeelinkGroupBox.Controls.Add(userLabel);
            sshBeelinkGroupBox.Controls.Add(portNumeric);
            sshBeelinkGroupBox.Controls.Add(portLabel);
            sshBeelinkGroupBox.Controls.Add(ipTextBox);
            sshBeelinkGroupBox.Controls.Add(ipLabel);
            sshBeelinkGroupBox.Location = new Point(10, 10);
            sshBeelinkGroupBox.Name = "sshBeelinkGroupBox";
            sshBeelinkGroupBox.Size = new Size(369, 174);
            sshBeelinkGroupBox.TabIndex = 0;
            sshBeelinkGroupBox.TabStop = false;
            sshBeelinkGroupBox.Text = "SSH Beelink";
            // 
            // resetButton
            // 
            resetButton.Location = new Point(280, 52);
            resetButton.Name = "resetButton";
            resetButton.Size = new Size(80, 25);
            resetButton.TabIndex = 11;
            resetButton.Text = "Сбросить";
            resetButton.UseVisualStyleBackColor = true;
            resetButton.Click += resetButton_Click;
            // 
            // saveButton
            // 
            saveButton.Location = new Point(280, 22);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(80, 25);
            saveButton.TabIndex = 10;
            saveButton.Text = "Сохранить";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += saveButton_Click;
            // 
            // passwordRootTextBox
            // 
            passwordRootTextBox.Location = new Point(121, 141);
            passwordRootTextBox.Name = "passwordRootTextBox";
            passwordRootTextBox.PasswordChar = '*';
            passwordRootTextBox.Size = new Size(150, 23);
            passwordRootTextBox.TabIndex = 9;
            // 
            // passwordRootLabel
            // 
            passwordRootLabel.AutoSize = true;
            passwordRootLabel.Location = new Point(10, 145);
            passwordRootLabel.Name = "passwordRootLabel";
            passwordRootLabel.Size = new Size(88, 15);
            passwordRootLabel.TabIndex = 8;
            passwordRootLabel.Text = "Password Root:";
            // 
            // passwordUserTextBox
            // 
            passwordUserTextBox.Location = new Point(121, 112);
            passwordUserTextBox.Name = "passwordUserTextBox";
            passwordUserTextBox.PasswordChar = '*';
            passwordUserTextBox.Size = new Size(150, 23);
            passwordUserTextBox.TabIndex = 7;
            // 
            // passwordUserLabel
            // 
            passwordUserLabel.AutoSize = true;
            passwordUserLabel.Location = new Point(10, 115);
            passwordUserLabel.Name = "passwordUserLabel";
            passwordUserLabel.Size = new Size(86, 15);
            passwordUserLabel.TabIndex = 6;
            passwordUserLabel.Text = "Password User:";
            // 
            // userTextBox
            // 
            userTextBox.Location = new Point(121, 82);
            userTextBox.Name = "userTextBox";
            userTextBox.Size = new Size(150, 23);
            userTextBox.TabIndex = 5;
            // 
            // userLabel
            // 
            userLabel.AutoSize = true;
            userLabel.Location = new Point(10, 85);
            userLabel.Name = "userLabel";
            userLabel.Size = new Size(33, 15);
            userLabel.TabIndex = 4;
            userLabel.Text = "User:";
            // 
            // portNumeric
            // 
            portNumeric.Location = new Point(121, 55);
            portNumeric.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            portNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            portNumeric.Name = "portNumeric";
            portNumeric.Size = new Size(150, 23);
            portNumeric.TabIndex = 3;
            portNumeric.Value = new decimal(new int[] { 2323, 0, 0, 0 });
            // 
            // portLabel
            // 
            portLabel.AutoSize = true;
            portLabel.Location = new Point(10, 55);
            portLabel.Name = "portLabel";
            portLabel.Size = new Size(32, 15);
            portLabel.TabIndex = 2;
            portLabel.Text = "Port:";
            // 
            // ipTextBox
            // 
            ipTextBox.Location = new Point(121, 25);
            ipTextBox.Name = "ipTextBox";
            ipTextBox.Size = new Size(150, 23);
            ipTextBox.TabIndex = 1;
            // 
            // ipLabel
            // 
            ipLabel.AutoSize = true;
            ipLabel.Location = new Point(10, 25);
            ipLabel.Name = "ipLabel";
            ipLabel.Size = new Size(20, 15);
            ipLabel.TabIndex = 0;
            ipLabel.Text = "IP:";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1008, 729);
            Controls.Add(mainTabControl);
            Name = "MainForm";
            Text = "MKtest";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            mainTabControl.ResumeLayout(false);
            mainTabPage.ResumeLayout(false);
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            panelLeft.ResumeLayout(false);
            leftFlowLayout.ResumeLayout(false);
            beelinkCollapsiblePanel.ResumeLayout(false);
            beelinkContentPanel.ResumeLayout(false);
            beelinkContentPanel.PerformLayout();
            timeGroupBox.ResumeLayout(false);
            beelinkHeaderPanel.ResumeLayout(false);
            webServerCollapsiblePanel.ResumeLayout(false);
            webServerContentPanel.ResumeLayout(false);
            webServerContentPanel.PerformLayout();
            webServerHeaderPanel.ResumeLayout(false);
            panelRight.ResumeLayout(false);
            demoCollapsiblePanel.ResumeLayout(false);
            demoContentPanel.ResumeLayout(false);
            demoContentPanel.PerformLayout();
            demoHeaderPanel.ResumeLayout(false);
            logPanel.ResumeLayout(false);
            logPanel.PerformLayout();
            settingsTabPage.ResumeLayout(false);
            webServerGroupBox.ResumeLayout(false);
            webServerGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)webServerPortNumeric).EndInit();
            sshBeelinkGroupBox.ResumeLayout(false);
            sshBeelinkGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)portNumeric).EndInit();
            ResumeLayout(false);
        }
        #endregion

        #region Объявления компонентов

        #region Основные элементы управления
        private TabControl mainTabControl;
        private TabPage mainTabPage;
        private TabPage settingsTabPage;
        private SplitContainer splitContainerMain;
        private Panel panelLeft;
        private FlowLayoutPanel leftFlowLayout;
        private Panel panelRight;
        private Panel logPanel;
        #endregion

        #region Сворачиваемая панель SSH Beelink
        private Panel beelinkCollapsiblePanel;
        private Panel beelinkHeaderPanel;
        private Label beelinkHeaderLabel;
        private Panel beelinkContentPanel;
        private Label beelinkStatusLabel;
        private Button beelinkConnectButton;
        private Button beelinkDisconnectButton;
        private Button beelinkTestButton;
        private GroupBox timeGroupBox;
        private Button timeCheckButton;
        private Button timeEnableNTPButton;
        private Button timeDisableNTPButton;
        private DateTimePicker manualDatePicker;
        private DateTimePicker manualTimePicker;
        private Button timeSetButton;
        private TextBox beelinkLogTextBox;
        private Button beelinkClearLogButton;
        #endregion

        #region Элементы настроек SSH Beelink
        private GroupBox sshBeelinkGroupBox;
        private Label ipLabel;
        private TextBox ipTextBox;
        private Label portLabel;
        private NumericUpDown portNumeric;
        private Label userLabel;
        private TextBox userTextBox;
        private Label passwordUserLabel;
        private TextBox passwordUserTextBox;
        private Label passwordRootLabel;
        private TextBox passwordRootTextBox;
        private Button saveButton;
        private Button resetButton;
        #endregion

        #region Веб-сервер элементы
        private GroupBox webServerGroupBox;
        private Label webServerIpLabel;
        private NumericUpDown webServerPortNumeric;
        private Label webServerPortLabel;
        private TextBox webServerIpTextBox;
        private Button webServerResetButton;
        private Button webServerSaveButton;
        private Panel webServerCollapsiblePanel;
        private Panel webServerHeaderPanel;
        private Label webServerHeaderLabel;
        private Panel webServerContentPanel;
        private Label webServerStatusLabel;
        private Button webServerStopButton;
        private Button webServerStartButton;
        #endregion

        #region Демо-сценарии элементы
        private Panel demoCollapsiblePanel;
        private Panel demoHeaderPanel;
        private Label demoHeaderLabel;
        private Panel demoContentPanel;
        private Button btnStopDemo;
        private Button btnStartDemo;
        private ComboBox cmbDemoScenarios;
        private Label lblDemoStatus;
        #endregion

        #endregion
    }
}