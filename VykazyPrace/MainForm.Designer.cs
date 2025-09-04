namespace VykazyPrace
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            menuStrip1 = new MenuStrip();
            dataToolStripMenuItem = new ToolStripMenuItem();
            exportToolStripMenuItem = new ToolStripMenuItem();
            uživateléToolStripMenuItem = new ToolStripMenuItem();
            správaUživatelůToolStripMenuItem = new ToolStripMenuItem();
            projektyToolStripMenuItem = new ToolStripMenuItem();
            správaProjektůToolStripMenuItem = new ToolStripMenuItem();
            správaIndexůToolStripMenuItem = new ToolStripMenuItem();
            návrhProjektuToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripMenuItem();
            nastaveníToolStripMenuItem = new ToolStripMenuItem();
            testToolStripMenuItem = new ToolStripMenuItem();
            oProgramuToolStripMenuItem = new ToolStripMenuItem();
            správceToolStripMenuItem = new ToolStripMenuItem();
            přehledToolStripMenuItem = new ToolStripMenuItem();
            radioButton1 = new RadioButton();
            radioButton2 = new RadioButton();
            comboBoxUsers = new ComboBox();
            panel1 = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            buttonReloadPowerKey = new Button();
            label2 = new Label();
            tableLayoutPanel2 = new TableLayoutPanel();
            buttonReloadNetworkDisks = new Button();
            label1 = new Label();
            panel2 = new Panel();
            buttonNow = new Button();
            labelSelectedDate = new Label();
            buttonNext = new Button();
            buttonPrevious = new Button();
            panelContainer = new Panel();
            panelCalendarContainer = new Panel();
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            zobrazitToolStripMenuItem = new ToolStripMenuItem();
            ukoncitToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            panel1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            panel2.SuspendLayout();
            panelContainer.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { dataToolStripMenuItem, uživateléToolStripMenuItem, projektyToolStripMenuItem, toolStripMenuItem1 });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1269, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // dataToolStripMenuItem
            // 
            dataToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { exportToolStripMenuItem });
            dataToolStripMenuItem.Name = "dataToolStripMenuItem";
            dataToolStripMenuItem.Size = new Size(43, 20);
            dataToolStripMenuItem.Text = "Data";
            dataToolStripMenuItem.Visible = false;
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new Size(108, 22);
            exportToolStripMenuItem.Text = "Export";
            exportToolStripMenuItem.Click += exportToolStripMenuItem_Click;
            // 
            // uživateléToolStripMenuItem
            // 
            uživateléToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { správaUživatelůToolStripMenuItem });
            uživateléToolStripMenuItem.Name = "uživateléToolStripMenuItem";
            uživateléToolStripMenuItem.Size = new Size(66, 20);
            uživateléToolStripMenuItem.Text = "Uživatelé";
            uživateléToolStripMenuItem.Visible = false;
            // 
            // správaUživatelůToolStripMenuItem
            // 
            správaUživatelůToolStripMenuItem.Name = "správaUživatelůToolStripMenuItem";
            správaUživatelůToolStripMenuItem.Size = new Size(159, 22);
            správaUživatelůToolStripMenuItem.Text = "Správa uživatelů";
            správaUživatelůToolStripMenuItem.Click += správaUživatelůToolStripMenuItem_Click;
            // 
            // projektyToolStripMenuItem
            // 
            projektyToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { správaProjektůToolStripMenuItem, správaIndexůToolStripMenuItem, návrhProjektuToolStripMenuItem });
            projektyToolStripMenuItem.Name = "projektyToolStripMenuItem";
            projektyToolStripMenuItem.Size = new Size(62, 20);
            projektyToolStripMenuItem.Text = "Projekty";
            // 
            // správaProjektůToolStripMenuItem
            // 
            správaProjektůToolStripMenuItem.Name = "správaProjektůToolStripMenuItem";
            správaProjektůToolStripMenuItem.Size = new Size(156, 22);
            správaProjektůToolStripMenuItem.Text = "Správa projektů";
            správaProjektůToolStripMenuItem.Visible = false;
            správaProjektůToolStripMenuItem.Click += správaProjektůToolStripMenuItem_Click;
            // 
            // správaIndexůToolStripMenuItem
            // 
            správaIndexůToolStripMenuItem.Name = "správaIndexůToolStripMenuItem";
            správaIndexůToolStripMenuItem.Size = new Size(156, 22);
            správaIndexůToolStripMenuItem.Text = "Správa indexů";
            správaIndexůToolStripMenuItem.Click += správaIndexůToolStripMenuItem_Click;
            // 
            // návrhProjektuToolStripMenuItem
            // 
            návrhProjektuToolStripMenuItem.Name = "návrhProjektuToolStripMenuItem";
            návrhProjektuToolStripMenuItem.Size = new Size(156, 22);
            návrhProjektuToolStripMenuItem.Text = "Návrh projektu";
            návrhProjektuToolStripMenuItem.Click += návrhProjektuToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { nastaveníToolStripMenuItem, testToolStripMenuItem, oProgramuToolStripMenuItem, správceToolStripMenuItem, přehledToolStripMenuItem });
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(24, 20);
            toolStripMenuItem1.Text = "?";
            // 
            // nastaveníToolStripMenuItem
            // 
            nastaveníToolStripMenuItem.Name = "nastaveníToolStripMenuItem";
            nastaveníToolStripMenuItem.Size = new Size(139, 22);
            nastaveníToolStripMenuItem.Text = "Nastavení";
            nastaveníToolStripMenuItem.Click += nastaveníToolStripMenuItem_Click;
            // 
            // testToolStripMenuItem
            // 
            testToolStripMenuItem.Name = "testToolStripMenuItem";
            testToolStripMenuItem.Size = new Size(139, 22);
            testToolStripMenuItem.Text = "Test";
            testToolStripMenuItem.Click += testToolStripMenuItem_Click_1;
            // 
            // oProgramuToolStripMenuItem
            // 
            oProgramuToolStripMenuItem.Name = "oProgramuToolStripMenuItem";
            oProgramuToolStripMenuItem.Size = new Size(139, 22);
            oProgramuToolStripMenuItem.Text = "O programu";
            oProgramuToolStripMenuItem.Click += oProgramuToolStripMenuItem_Click;
            // 
            // správceToolStripMenuItem
            // 
            správceToolStripMenuItem.Name = "správceToolStripMenuItem";
            správceToolStripMenuItem.Size = new Size(139, 22);
            správceToolStripMenuItem.Text = "Správce";
            správceToolStripMenuItem.Visible = false;
            správceToolStripMenuItem.Click += správceToolStripMenuItem_Click;
            // 
            // přehledToolStripMenuItem
            // 
            přehledToolStripMenuItem.Name = "přehledToolStripMenuItem";
            přehledToolStripMenuItem.Size = new Size(139, 22);
            přehledToolStripMenuItem.Text = "Přehled";
            přehledToolStripMenuItem.Click += přehledToolStripMenuItem_Click;
            // 
            // radioButton1
            // 
            radioButton1.Appearance = Appearance.Button;
            radioButton1.Checked = true;
            radioButton1.Dock = DockStyle.Fill;
            radioButton1.Location = new Point(10, 3);
            radioButton1.Margin = new Padding(10, 3, 10, 3);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(107, 32);
            radioButton1.TabIndex = 2;
            radioButton1.TabStop = true;
            radioButton1.Text = "TÝDEN";
            radioButton1.TextAlign = ContentAlignment.MiddleCenter;
            radioButton1.UseVisualStyleBackColor = true;
            radioButton1.CheckedChanged += radioButton1_CheckedChanged;
            // 
            // radioButton2
            // 
            radioButton2.Appearance = Appearance.Button;
            radioButton2.Dock = DockStyle.Fill;
            radioButton2.Location = new Point(10, 41);
            radioButton2.Margin = new Padding(10, 3, 10, 3);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new Size(107, 32);
            radioButton2.TabIndex = 3;
            radioButton2.Text = "MĚSÍC";
            radioButton2.TextAlign = ContentAlignment.MiddleCenter;
            radioButton2.UseVisualStyleBackColor = true;
            radioButton2.CheckedChanged += radioButton1_CheckedChanged;
            // 
            // comboBoxUsers
            // 
            comboBoxUsers.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            comboBoxUsers.Enabled = false;
            comboBoxUsers.FormattingEnabled = true;
            comboBoxUsers.Location = new Point(961, 7);
            comboBoxUsers.Name = "comboBoxUsers";
            comboBoxUsers.Size = new Size(308, 33);
            comboBoxUsers.TabIndex = 0;
            comboBoxUsers.Visible = false;
            comboBoxUsers.SelectedIndexChanged += comboBoxUsers_SelectedIndexChanged;
            // 
            // panel1
            // 
            panel1.Controls.Add(tableLayoutPanel1);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 69);
            panel1.Name = "panel1";
            panel1.Size = new Size(127, 592);
            panel1.TabIndex = 6;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 4);
            tableLayoutPanel1.Controls.Add(radioButton1, 0, 0);
            tableLayoutPanel1.Controls.Add(radioButton2, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 3);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 75F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 75F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(127, 592);
            tableLayoutPanel1.TabIndex = 8;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(buttonReloadPowerKey, 1, 0);
            tableLayoutPanel3.Controls.Add(label2, 0, 0);
            tableLayoutPanel3.Dock = DockStyle.Bottom;
            tableLayoutPanel3.Location = new Point(3, 520);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.RowStyles.Add(new RowStyle());
            tableLayoutPanel3.Size = new Size(121, 69);
            tableLayoutPanel3.TabIndex = 12;
            // 
            // buttonReloadPowerKey
            // 
            buttonReloadPowerKey.BackColor = Color.Tomato;
            buttonReloadPowerKey.Dock = DockStyle.Top;
            buttonReloadPowerKey.Location = new Point(3, 28);
            buttonReloadPowerKey.Name = "buttonReloadPowerKey";
            buttonReloadPowerKey.Size = new Size(115, 41);
            buttonReloadPowerKey.TabIndex = 13;
            buttonReloadPowerKey.Text = "⟳";
            buttonReloadPowerKey.UseVisualStyleBackColor = false;
            buttonReloadPowerKey.Click += buttonReloadPowerKey_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 0);
            label2.Name = "label2";
            label2.Size = new Size(83, 25);
            label2.TabIndex = 1;
            label2.Text = "PowerKey";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(buttonReloadNetworkDisks, 1, 0);
            tableLayoutPanel2.Controls.Add(label1, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Bottom;
            tableLayoutPanel2.Location = new Point(3, 445);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(121, 69);
            tableLayoutPanel2.TabIndex = 11;
            // 
            // buttonReloadNetworkDisks
            // 
            buttonReloadNetworkDisks.BackColor = Color.Tomato;
            buttonReloadNetworkDisks.Dock = DockStyle.Top;
            buttonReloadNetworkDisks.Location = new Point(3, 28);
            buttonReloadNetworkDisks.Name = "buttonReloadNetworkDisks";
            buttonReloadNetworkDisks.Size = new Size(115, 41);
            buttonReloadNetworkDisks.TabIndex = 13;
            buttonReloadNetworkDisks.Text = "⟳";
            buttonReloadNetworkDisks.UseVisualStyleBackColor = false;
            buttonReloadNetworkDisks.Click += buttonReloadNetworkDisks_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(99, 25);
            label1.TabIndex = 1;
            label1.Text = "Síťové disky";
            // 
            // panel2
            // 
            panel2.Controls.Add(buttonNow);
            panel2.Controls.Add(labelSelectedDate);
            panel2.Controls.Add(buttonNext);
            panel2.Controls.Add(buttonPrevious);
            panel2.Controls.Add(comboBoxUsers);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 24);
            panel2.Name = "panel2";
            panel2.Size = new Size(1269, 45);
            panel2.TabIndex = 6;
            // 
            // buttonNow
            // 
            buttonNow.Location = new Point(10, 6);
            buttonNow.Name = "buttonNow";
            buttonNow.Size = new Size(106, 33);
            buttonNow.TabIndex = 4;
            buttonNow.Text = "Dnes";
            buttonNow.UseVisualStyleBackColor = true;
            buttonNow.Click += buttonNow_Click;
            // 
            // labelSelectedDate
            // 
            labelSelectedDate.Font = new Font("Reddit Sans", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
            labelSelectedDate.Location = new Point(209, 7);
            labelSelectedDate.Name = "labelSelectedDate";
            labelSelectedDate.Size = new Size(274, 32);
            labelSelectedDate.TabIndex = 3;
            labelSelectedDate.Text = "24.3. - 30.3.2025";
            labelSelectedDate.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // buttonNext
            // 
            buttonNext.Location = new Point(489, 6);
            buttonNext.Name = "buttonNext";
            buttonNext.Size = new Size(75, 33);
            buttonNext.TabIndex = 2;
            buttonNext.Text = ">";
            buttonNext.UseVisualStyleBackColor = true;
            buttonNext.Click += buttonNext_Click;
            // 
            // buttonPrevious
            // 
            buttonPrevious.Location = new Point(128, 6);
            buttonPrevious.Name = "buttonPrevious";
            buttonPrevious.Size = new Size(75, 33);
            buttonPrevious.TabIndex = 1;
            buttonPrevious.Text = "<";
            buttonPrevious.UseVisualStyleBackColor = true;
            buttonPrevious.Click += buttonPrevious_Click;
            // 
            // panelContainer
            // 
            panelContainer.BorderStyle = BorderStyle.FixedSingle;
            panelContainer.Controls.Add(panelCalendarContainer);
            panelContainer.Dock = DockStyle.Fill;
            panelContainer.Location = new Point(127, 69);
            panelContainer.Margin = new Padding(0);
            panelContainer.Name = "panelContainer";
            panelContainer.Size = new Size(1142, 592);
            panelContainer.TabIndex = 7;
            // 
            // panelCalendarContainer
            // 
            panelCalendarContainer.Dock = DockStyle.Fill;
            panelCalendarContainer.Location = new Point(0, 0);
            panelCalendarContainer.Name = "panelCalendarContainer";
            panelCalendarContainer.Size = new Size(1140, 590);
            panelCalendarContainer.TabIndex = 1;
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "WorkLog";
            notifyIcon1.Visible = true;
            notifyIcon1.MouseDoubleClick += notifyIcon1_MouseDoubleClick;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { zobrazitToolStripMenuItem, ukoncitToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(118, 48);
            // 
            // zobrazitToolStripMenuItem
            // 
            zobrazitToolStripMenuItem.Name = "zobrazitToolStripMenuItem";
            zobrazitToolStripMenuItem.Size = new Size(117, 22);
            zobrazitToolStripMenuItem.Text = "Zobrazit";
            // 
            // ukoncitToolStripMenuItem
            // 
            ukoncitToolStripMenuItem.Name = "ukoncitToolStripMenuItem";
            ukoncitToolStripMenuItem.Size = new Size(117, 22);
            ukoncitToolStripMenuItem.Text = "Ukončit";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1279, 671);
            Controls.Add(panelContainer);
            Controls.Add(panel1);
            Controls.Add(panel2);
            Controls.Add(menuStrip1);
            Font = new Font("Reddit Sans", 12F);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Margin = new Padding(4, 5, 4, 5);
            MinimumSize = new Size(1295, 710);
            Name = "MainForm";
            Padding = new Padding(0, 0, 10, 10);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Výkazy práce";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            panel2.ResumeLayout(false);
            panelContainer.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem dataToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem uživateléToolStripMenuItem;
        private ToolStripMenuItem projektyToolStripMenuItem;
        private ToolStripMenuItem správaUživatelůToolStripMenuItem;
        private ToolStripMenuItem správaProjektůToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem oProgramuToolStripMenuItem;
        private ToolStripMenuItem nastaveníToolStripMenuItem;
        private RadioButton radioButton1;
        private RadioButton radioButton2;
        private ComboBox comboBoxUsers;
        private Panel panel1;
        private Panel panel2;
        private Panel panelContainer;
        private Panel panelCalendarContainer;
        private Label labelSelectedDate;
        private Button buttonNext;
        private Button buttonPrevious;
        private ToolStripMenuItem testToolStripMenuItem;
        private ToolStripMenuItem správaIndexůToolStripMenuItem;
        private Button buttonNow;
        private ToolStripMenuItem správceToolStripMenuItem;
        private ToolStripMenuItem návrhProjektuToolStripMenuItem;
        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem zobrazitToolStripMenuItem;
        private ToolStripMenuItem ukoncitToolStripMenuItem;
        private ToolStripMenuItem přehledToolStripMenuItem;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Label label1;
        private Button buttonReloadNetworkDisks;
        private TableLayoutPanel tableLayoutPanel3;
        private Button buttonReloadPowerKey;
        private Label label2;
    }
}
