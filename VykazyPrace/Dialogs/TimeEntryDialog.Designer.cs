namespace VykazyPrace.Dialogs
{
    partial class TimeEntryDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            label3 = new Label();
            button1 = new Button();
            button2 = new Button();
            comboBox1 = new ComboBox();
            listBox1 = new ListBox();
            groupBox1 = new GroupBox();
            label4 = new Label();
            button7 = new Button();
            button8 = new Button();
            button6 = new Button();
            button5 = new Button();
            panel1 = new Panel();
            label2 = new Label();
            label5 = new Label();
            button4 = new Button();
            button3 = new Button();
            panel2 = new Panel();
            label6 = new Label();
            groupBox1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Font = new Font("Reddit Sans", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label1.Location = new Point(188, 9);
            label1.Name = "label1";
            label1.Size = new Size(150, 35);
            label1.TabIndex = 0;
            label1.Text = "25.02.2025";
            label1.TextAlign = ContentAlignment.TopCenter;
            // 
            // label3
            // 
            label3.Font = new Font("Reddit Sans", 12F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label3.ForeColor = SystemColors.ControlDarkDark;
            label3.Location = new Point(344, 12);
            label3.Name = "label3";
            label3.Size = new Size(125, 25);
            label3.TabIndex = 2;
            label3.Text = "26.02.2025";
            label3.TextAlign = ContentAlignment.TopCenter;
            // 
            // button1
            // 
            button1.BackColor = Color.White;
            button1.Dock = DockStyle.Left;
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Reddit Sans", 10F, FontStyle.Bold);
            button1.Location = new Point(0, 0);
            button1.Name = "button1";
            button1.Size = new Size(234, 31);
            button1.TabIndex = 3;
            button1.Text = "PROJEKT";
            button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            button2.BackColor = SystemColors.AppWorkspace;
            button2.Dock = DockStyle.Right;
            button2.FlatAppearance.BorderSize = 0;
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("Reddit Sans", 10F);
            button2.Location = new Point(232, 0);
            button2.Name = "button2";
            button2.Size = new Size(234, 31);
            button2.TabIndex = 4;
            button2.Text = "ZAKÁZKA";
            button2.UseVisualStyleBackColor = false;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.IntegralHeight = false;
            comboBox1.ItemHeight = 25;
            comboBox1.Location = new Point(11, 71);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(467, 33);
            comboBox1.TabIndex = 5;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            comboBox1.SelectionChangeCommitted += comboBox1_SelectionChangeCommitted;
            comboBox1.TextChanged += comboBox1_TextChanged;
            // 
            // listBox1
            // 
            listBox1.BorderStyle = BorderStyle.FixedSingle;
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 25;
            listBox1.Items.AddRange(new object[] { "7,5h\t0230I24 - Automatické balení po pecích a UV", "tes", "test", "test", "test", "tes" });
            listBox1.Location = new Point(12, 83);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(485, 77);
            listBox1.TabIndex = 9;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(button7);
            groupBox1.Controls.Add(button8);
            groupBox1.Controls.Add(button6);
            groupBox1.Controls.Add(button5);
            groupBox1.Controls.Add(panel1);
            groupBox1.Controls.Add(comboBox1);
            groupBox1.Location = new Point(12, 204);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(485, 163);
            groupBox1.TabIndex = 10;
            groupBox1.TabStop = false;
            groupBox1.Text = "Zápis hodin";
            // 
            // label4
            // 
            label4.Location = new Point(203, 110);
            label4.Name = "label4";
            label4.Size = new Size(84, 43);
            label4.TabIndex = 14;
            label4.Text = "4 h";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // button7
            // 
            button7.Location = new Point(293, 110);
            button7.Name = "button7";
            button7.Size = new Size(90, 43);
            button7.TabIndex = 13;
            button7.Text = "+ 0,5 h";
            button7.UseVisualStyleBackColor = true;
            // 
            // button8
            // 
            button8.Location = new Point(389, 110);
            button8.Name = "button8";
            button8.Size = new Size(90, 43);
            button8.TabIndex = 12;
            button8.Text = "+ 1 h";
            button8.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            button6.Location = new Point(107, 110);
            button6.Name = "button6";
            button6.Size = new Size(90, 43);
            button6.TabIndex = 11;
            button6.Text = "- 0,5 h";
            button6.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            button5.Location = new Point(11, 110);
            button5.Name = "button5";
            button5.Size = new Size(90, 43);
            button5.TabIndex = 10;
            button5.Text = "- 1 h";
            button5.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(button1);
            panel1.Controls.Add(button2);
            panel1.Location = new Point(11, 32);
            panel1.Name = "panel1";
            panel1.Size = new Size(468, 33);
            panel1.TabIndex = 9;
            // 
            // label2
            // 
            label2.Font = new Font("Reddit Sans", 12F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label2.ForeColor = SystemColors.ControlDarkDark;
            label2.Location = new Point(57, 12);
            label2.Name = "label2";
            label2.Size = new Size(125, 25);
            label2.TabIndex = 11;
            label2.Text = "24.02.2025";
            label2.TextAlign = ContentAlignment.TopCenter;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(9, 168);
            label5.Name = "label5";
            label5.Size = new Size(73, 25);
            label5.TabIndex = 12;
            label5.Text = "7,5 / 15 h";
            // 
            // button4
            // 
            button4.Location = new Point(344, 373);
            button4.Name = "button4";
            button4.Size = new Size(153, 43);
            button4.TabIndex = 14;
            button4.Text = "Zrušit";
            button4.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(12, 373);
            button3.Name = "button3";
            button3.Size = new Size(326, 43);
            button3.TabIndex = 13;
            button3.Text = "Uložit";
            button3.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ControlLight;
            panel2.Location = new Point(115, 48);
            panel2.Name = "panel2";
            panel2.Size = new Size(300, 1);
            panel2.TabIndex = 15;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Reddit Sans", 8F);
            label6.Location = new Point(9, 63);
            label6.Name = "label6";
            label6.Size = new Size(89, 17);
            label6.TabIndex = 16;
            label6.Text = "Zapsané hodiny:";
            // 
            // TimeEntryDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(509, 428);
            Controls.Add(label6);
            Controls.Add(panel2);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(label5);
            Controls.Add(label2);
            Controls.Add(groupBox1);
            Controls.Add(listBox1);
            Controls.Add(label3);
            Controls.Add(label1);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "TimeEntryDialog";
            Text = "TimeEntryDialog";
            Load += TimeEntryDialog_Load;
            groupBox1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label3;
        private Button button1;
        private Button button2;
        private ComboBox comboBox1;
        private ListBox listBox1;
        private GroupBox groupBox1;
        private Panel panel1;
        private Label label2;
        private Button button7;
        private Button button8;
        private Button button6;
        private Button button5;
        private Label label5;
        private Label label4;
        private Button button4;
        private Button button3;
        private Panel panel2;
        private Label label6;
    }
}