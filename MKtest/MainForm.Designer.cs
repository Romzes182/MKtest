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
            flowLayoutPanel = new FlowLayoutPanel();
            beelinkCollapsiblePanel = new Panel();
            beelinkContentPanel = new Panel();
            beelinkClearLogButton = new Button();
            timeGroupBox = new GroupBox();
            timeSetButton = new Button();
            manualTimePicker = new DateTimePicker();
            manualDatePicker = new DateTimePicker();
            timeDisableNTPButton = new Button();
            timeEnableNTPButton = new Button();
            timeCheckButton = new Button();
            beelinkTestButton = new Button();
            beelinkLogTextBox = new TextBox();
            beelinkDisconnectButton = new Button();
            beelinkConnectButton = new Button();
            beelinkStatusLabel = new Label();
            beelinkHeaderPanel = new Panel();
            beelinkHeaderLabel = new Label();
            settingsTabPage = new TabPage();
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
            flowLayoutPanel.SuspendLayout();
            beelinkCollapsiblePanel.SuspendLayout();
            beelinkContentPanel.SuspendLayout();
            timeGroupBox.SuspendLayout();
            beelinkHeaderPanel.SuspendLayout();
            settingsTabPage.SuspendLayout();
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
            mainTabPage.Controls.Add(flowLayoutPanel);
            mainTabPage.Location = new Point(4, 24);
            mainTabPage.Name = "mainTabPage";
            mainTabPage.Padding = new Padding(3);
            mainTabPage.Size = new Size(1000, 701);
            mainTabPage.TabIndex = 0;
            mainTabPage.Text = "Главная";
            mainTabPage.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel
            // 
            flowLayoutPanel.AutoScroll = true;
            flowLayoutPanel.Controls.Add(beelinkCollapsiblePanel);
            flowLayoutPanel.Dock = DockStyle.Fill;
            flowLayoutPanel.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel.Location = new Point(3, 3);
            flowLayoutPanel.Name = "flowLayoutPanel";
            flowLayoutPanel.Size = new Size(994, 695);
            flowLayoutPanel.TabIndex = 1;
            flowLayoutPanel.WrapContents = false;
            // 
            // beelinkCollapsiblePanel
            // 
            beelinkCollapsiblePanel.BorderStyle = BorderStyle.FixedSingle;
            beelinkCollapsiblePanel.Controls.Add(beelinkContentPanel);
            beelinkCollapsiblePanel.Controls.Add(beelinkHeaderPanel);
            beelinkCollapsiblePanel.Location = new Point(10, 10);
            beelinkCollapsiblePanel.Margin = new Padding(10);
            beelinkCollapsiblePanel.Name = "beelinkCollapsiblePanel";
            beelinkCollapsiblePanel.Size = new Size(430, 329);
            beelinkCollapsiblePanel.TabIndex = 0;
            // 
            // beelinkContentPanel
            // 
            beelinkContentPanel.Controls.Add(beelinkClearLogButton);
            beelinkContentPanel.Controls.Add(timeGroupBox);
            beelinkContentPanel.Controls.Add(beelinkTestButton);
            beelinkContentPanel.Controls.Add(beelinkLogTextBox);
            beelinkContentPanel.Controls.Add(beelinkDisconnectButton);
            beelinkContentPanel.Controls.Add(beelinkConnectButton);
            beelinkContentPanel.Controls.Add(beelinkStatusLabel);
            beelinkContentPanel.Dock = DockStyle.Fill;
            beelinkContentPanel.Location = new Point(0, 30);
            beelinkContentPanel.Name = "beelinkContentPanel";
            beelinkContentPanel.Size = new Size(428, 297);
            beelinkContentPanel.TabIndex = 1;
            // 
            // beelinkClearLogButton
            // 
            beelinkClearLogButton.Location = new Point(10, 265);
            beelinkClearLogButton.Name = "beelinkClearLogButton";
            beelinkClearLogButton.Size = new Size(100, 25);
            beelinkClearLogButton.TabIndex = 6;
            beelinkClearLogButton.Text = "Очистить лог";
            beelinkClearLogButton.UseVisualStyleBackColor = true;
            beelinkClearLogButton.Click += beelinkClearLogButton_Click;
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
            timeGroupBox.Size = new Size(411, 88);
            timeGroupBox.TabIndex = 4;
            timeGroupBox.TabStop = false;
            timeGroupBox.Text = "Управление временем";
            // 
            // timeSetButton
            // 
            timeSetButton.Location = new Point(240, 55);
            timeSetButton.Name = "timeSetButton";
            timeSetButton.Size = new Size(120, 25);
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
            manualTimePicker.Size = new Size(90, 23);
            manualTimePicker.TabIndex = 4;
            manualTimePicker.Value = new DateTime(2026, 1, 31, 16, 27, 18, 279);
            // 
            // manualDatePicker
            // 
            manualDatePicker.Format = DateTimePickerFormat.Short;
            manualDatePicker.Location = new Point(10, 55);
            manualDatePicker.Name = "manualDatePicker";
            manualDatePicker.Size = new Size(120, 23);
            manualDatePicker.TabIndex = 3;
            manualDatePicker.Value = new DateTime(2026, 1, 31, 16, 27, 18, 280);
            // 
            // timeDisableNTPButton
            // 
            timeDisableNTPButton.Location = new Point(270, 20);
            timeDisableNTPButton.Name = "timeDisableNTPButton";
            timeDisableNTPButton.Size = new Size(120, 25);
            timeDisableNTPButton.TabIndex = 2;
            timeDisableNTPButton.Text = "Выключить NTP";
            timeDisableNTPButton.UseVisualStyleBackColor = true;
            timeDisableNTPButton.Click += timeDisableNTPButton_Click;
            // 
            // timeEnableNTPButton
            // 
            timeEnableNTPButton.Location = new Point(140, 20);
            timeEnableNTPButton.Name = "timeEnableNTPButton";
            timeEnableNTPButton.Size = new Size(120, 25);
            timeEnableNTPButton.TabIndex = 1;
            timeEnableNTPButton.Text = "Включить NTP";
            timeEnableNTPButton.UseVisualStyleBackColor = true;
            timeEnableNTPButton.Click += timeEnableNTPButton_Click;
            // 
            // timeCheckButton
            // 
            timeCheckButton.Location = new Point(10, 20);
            timeCheckButton.Name = "timeCheckButton";
            timeCheckButton.Size = new Size(120, 25);
            timeCheckButton.TabIndex = 0;
            timeCheckButton.Text = "Статус времени";
            timeCheckButton.UseVisualStyleBackColor = true;
            timeCheckButton.Click += timeCheckButton_Click;
            // 
            // beelinkTestButton
            // 
            beelinkTestButton.Location = new Point(211, 38);
            beelinkTestButton.Name = "beelinkTestButton";
            beelinkTestButton.Size = new Size(59, 21);
            beelinkTestButton.TabIndex = 3;
            beelinkTestButton.Text = "Тест ";
            beelinkTestButton.UseVisualStyleBackColor = true;
            beelinkTestButton.Click += beelinkTestButton_Click;
            // 
            // beelinkLogTextBox
            // 
            beelinkLogTextBox.Font = new Font("Consolas", 9F);
            beelinkLogTextBox.Location = new Point(10, 159);
            beelinkLogTextBox.Multiline = true;
            beelinkLogTextBox.Name = "beelinkLogTextBox";
            beelinkLogTextBox.ReadOnly = true;
            beelinkLogTextBox.ScrollBars = ScrollBars.Vertical;
            beelinkLogTextBox.Size = new Size(411, 100);
            beelinkLogTextBox.TabIndex = 5;
            beelinkLogTextBox.TextChanged += beelinkLogTextBox_TextChanged;
            // 
            // beelinkDisconnectButton
            // 
            beelinkDisconnectButton.Location = new Point(106, 38);
            beelinkDisconnectButton.Name = "beelinkDisconnectButton";
            beelinkDisconnectButton.Size = new Size(90, 21);
            beelinkDisconnectButton.TabIndex = 2;
            beelinkDisconnectButton.Text = "Отключить";
            beelinkDisconnectButton.UseVisualStyleBackColor = true;
            beelinkDisconnectButton.Click += beelinkDisconnectButton_Click;
            // 
            // beelinkConnectButton
            // 
            beelinkConnectButton.Location = new Point(10, 38);
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
            beelinkStatusLabel.Location = new Point(10, 20);
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
            beelinkHeaderPanel.Size = new Size(428, 30);
            beelinkHeaderPanel.TabIndex = 0;
            // 
            // beelinkHeaderLabel
            // 
            beelinkHeaderLabel.AutoSize = true;
            beelinkHeaderLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            beelinkHeaderLabel.Location = new Point(10, 8);
            beelinkHeaderLabel.Name = "beelinkHeaderLabel";
            beelinkHeaderLabel.Size = new Size(75, 15);
            beelinkHeaderLabel.TabIndex = 0;
            beelinkHeaderLabel.Text = "SSH Beelink";
            // 
            // settingsTabPage
            // 
            settingsTabPage.Controls.Add(sshBeelinkGroupBox);
            settingsTabPage.Location = new Point(4, 24);
            settingsTabPage.Name = "settingsTabPage";
            settingsTabPage.Padding = new Padding(3);
            settingsTabPage.Size = new Size(809, 610);
            settingsTabPage.TabIndex = 1;
            settingsTabPage.Text = "Настройки";
            settingsTabPage.UseVisualStyleBackColor = true;
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
            sshBeelinkGroupBox.Size = new Size(450, 200);
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
            flowLayoutPanel.ResumeLayout(false);
            beelinkCollapsiblePanel.ResumeLayout(false);
            beelinkContentPanel.ResumeLayout(false);
            beelinkContentPanel.PerformLayout();
            timeGroupBox.ResumeLayout(false);
            beelinkHeaderPanel.ResumeLayout(false);
            beelinkHeaderPanel.PerformLayout();
            settingsTabPage.ResumeLayout(false);
            sshBeelinkGroupBox.ResumeLayout(false);
            sshBeelinkGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)portNumeric).EndInit();
            ResumeLayout(false);
        }
        #endregion

        #region Объявления компонентов (разделено по группам)

        #region Основные элементы управления
        private TabControl mainTabControl;
        private TabPage mainTabPage;
        private TabPage settingsTabPage;
        private FlowLayoutPanel flowLayoutPanel;
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

        #endregion
    }
}