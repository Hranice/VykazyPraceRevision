namespace VykazyPrace.Dialogs
{
    partial class TimeEntryV2Dialog
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
            comboBoxStart = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            comboBoxEnd = new ComboBox();
            groupBox1 = new GroupBox();
            label5 = new Label();
            comboBoxIndex = new ComboBox();
            label4 = new Label();
            comboBoxEntryType = new ComboBox();
            comboBoxProjects = new ComboBox();
            label8 = new Label();
            label3 = new Label();
            textBoxDescription = new TextBox();
            buttonConfirm = new Button();
            buttonRemove = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // comboBoxStart
            // 
            comboBoxStart.FormattingEnabled = true;
            comboBoxStart.Items.AddRange(new object[] { "0:00", "0:30", "1:00", "1:30", "2:00", "2:30", "3:00", "3:30", "4:00", "4:30", "5:00", "5:30", "6:00", "6:30", "7:00", "7:30", "8:00", "8:30", "9:00", "9:30", "10:00", "10:30", "11:00", "11:30", "12:00", "12:30", "13:00", "13:30", "14:00", "14:30", "15:00", "15:30", "16:00", "16:30", "17:00", "17:30", "18:00", "18:30", "19:00", "19:30", "20:00", "20:30", "21:00", "21:30", "22:00", "22:30", "23:00", "23:30" });
            comboBoxStart.Location = new Point(97, 51);
            comboBoxStart.Name = "comboBoxStart";
            comboBoxStart.Size = new Size(69, 33);
            comboBoxStart.TabIndex = 0;
            comboBoxStart.SelectedIndexChanged += comboBoxStart_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(23, 54);
            label1.Name = "label1";
            label1.Size = new Size(75, 25);
            label1.TabIndex = 1;
            label1.Text = "Počátek*";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(13, 93);
            label2.Name = "label2";
            label2.Size = new Size(85, 25);
            label2.TabIndex = 3;
            label2.Text = "Ukončení*";
            // 
            // comboBoxEnd
            // 
            comboBoxEnd.FormattingEnabled = true;
            comboBoxEnd.Items.AddRange(new object[] { "0:00", "0:30", "1:00", "1:30", "2:00", "2:30", "3:00", "3:30", "4:00", "4:30", "5:00", "5:30", "6:00", "6:30", "7:00", "7:30", "8:00", "8:30", "9:00", "9:30", "10:00", "10:30", "11:00", "11:30", "12:00", "12:30", "13:00", "13:30", "14:00", "14:30", "15:00", "15:30", "16:00", "16:30", "17:00", "17:30", "18:00", "18:30", "19:00", "19:30", "20:00", "20:30", "21:00", "21:30", "22:00", "22:30", "23:00", "23:30" });
            comboBoxEnd.Location = new Point(97, 90);
            comboBoxEnd.Name = "comboBoxEnd";
            comboBoxEnd.Size = new Size(69, 33);
            comboBoxEnd.TabIndex = 2;
            comboBoxEnd.SelectionChangeCommitted += comboBoxEnd_SelectionChangeCommitted;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(comboBoxIndex);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(comboBoxEntryType);
            groupBox1.Controls.Add(comboBoxProjects);
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(textBoxDescription);
            groupBox1.Controls.Add(comboBoxStart);
            groupBox1.Controls.Add(comboBoxEnd);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(label2);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(447, 294);
            groupBox1.TabIndex = 5;
            groupBox1.TabStop = false;
            groupBox1.Text = "Čtvrtek 06.03.2025";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label5.Location = new Point(149, 150);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(43, 21);
            label5.TabIndex = 32;
            label5.Text = "Index";
            // 
            // comboBoxIndex
            // 
            comboBoxIndex.FormattingEnabled = true;
            comboBoxIndex.IntegralHeight = false;
            comboBoxIndex.ItemHeight = 25;
            comboBoxIndex.Location = new Point(149, 174);
            comboBoxIndex.Name = "comboBoxIndex";
            comboBoxIndex.Size = new Size(272, 33);
            comboBoxIndex.TabIndex = 31;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label4.Location = new Point(23, 150);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(35, 21);
            label4.TabIndex = 30;
            label4.Text = "Typ*";
            // 
            // comboBoxEntryType
            // 
            comboBoxEntryType.FormattingEnabled = true;
            comboBoxEntryType.IntegralHeight = false;
            comboBoxEntryType.ItemHeight = 25;
            comboBoxEntryType.Location = new Point(23, 174);
            comboBoxEntryType.Name = "comboBoxEntryType";
            comboBoxEntryType.Size = new Size(120, 33);
            comboBoxEntryType.TabIndex = 29;
            // 
            // comboBoxProjects
            // 
            comboBoxProjects.FormattingEnabled = true;
            comboBoxProjects.IntegralHeight = false;
            comboBoxProjects.ItemHeight = 25;
            comboBoxProjects.Location = new Point(23, 240);
            comboBoxProjects.Name = "comboBoxProjects";
            comboBoxProjects.Size = new Size(398, 33);
            comboBoxProjects.TabIndex = 27;
            comboBoxProjects.SelectionChangeCommitted += comboBoxProjects_SelectionChangeCommitted;
            comboBoxProjects.TextChanged += comboBoxProjects_TextChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label8.Location = new Point(23, 216);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(57, 21);
            label8.TabIndex = 28;
            label8.Text = "Projekt*";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label3.Location = new Point(181, 27);
            label3.Name = "label3";
            label3.Size = new Size(94, 21);
            label3.TabIndex = 5;
            label3.Text = "Popis činnosti*";
            // 
            // textBoxDescription
            // 
            textBoxDescription.Location = new Point(181, 51);
            textBoxDescription.Multiline = true;
            textBoxDescription.Name = "textBoxDescription";
            textBoxDescription.Size = new Size(240, 72);
            textBoxDescription.TabIndex = 4;
            textBoxDescription.Text = "Zapínání kávovaru";
            // 
            // buttonConfirm
            // 
            buttonConfirm.DialogResult = DialogResult.OK;
            buttonConfirm.Location = new Point(360, 312);
            buttonConfirm.Name = "buttonConfirm";
            buttonConfirm.Size = new Size(99, 41);
            buttonConfirm.TabIndex = 6;
            buttonConfirm.Text = "Potvrdit";
            buttonConfirm.UseVisualStyleBackColor = true;
            buttonConfirm.Click += buttonConfirm_Click;
            // 
            // buttonRemove
            // 
            buttonRemove.DialogResult = DialogResult.Abort;
            buttonRemove.Location = new Point(255, 312);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new Size(99, 41);
            buttonRemove.TabIndex = 7;
            buttonRemove.Text = "Odstranit";
            buttonRemove.UseVisualStyleBackColor = true;
            buttonRemove.Click += buttonRemove_Click;
            // 
            // TimeEntryV2Dialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(469, 364);
            Controls.Add(buttonRemove);
            Controls.Add(buttonConfirm);
            Controls.Add(groupBox1);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "TimeEntryV2Dialog";
            Text = "Záznam činnosti";
            Load += TimeEntryV2Dialog_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private ComboBox comboBoxStart;
        private Label label1;
        private Label label2;
        private ComboBox comboBoxEnd;
        private GroupBox groupBox1;
        private TextBox textBoxDescription;
        private Button buttonConfirm;
        private Label label3;
        private Button buttonRemove;
        private Label label4;
        private ComboBox comboBoxEntryType;
        private ComboBox comboBoxProjects;
        private Label label8;
        private Label label5;
        private ComboBox comboBoxIndex;
    }
}