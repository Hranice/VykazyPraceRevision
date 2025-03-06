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
            label3 = new Label();
            textBox1 = new TextBox();
            buttonConfirm = new Button();
            buttonRemove = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // comboBoxStart
            // 
            comboBoxStart.FormattingEnabled = true;
            comboBoxStart.Items.AddRange(new object[] { "0:00", "0:30", "1:00", "1:30", "2:00", "2:30", "3:30", "4:00", "4:30", "5:00", "5:30", "6:00", "6:30", "7:00", "7:30", "8:00", "8:30", "9:00", "9:30", "10:00", "10:30", "11:00", "11:30", "12:00", "12:30", "13:00", "13:30", "14:00", "14:30", "15:00", "15:30", "16:00", "16:30", "17:00", "17:30", "18:00", "18:30", "19:00", "19:30", "20:00", "20:30", "21:00", "21:30", "22:00", "22:30", "23:00", "23:30" });
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
            label1.Size = new Size(68, 25);
            label1.TabIndex = 1;
            label1.Text = "Počátek";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(13, 93);
            label2.Name = "label2";
            label2.Size = new Size(78, 25);
            label2.TabIndex = 3;
            label2.Text = "Ukončení";
            // 
            // comboBoxEnd
            // 
            comboBoxEnd.FormattingEnabled = true;
            comboBoxEnd.Items.AddRange(new object[] { "0:00", "0:30", "1:00", "1:30", "2:00", "2:30", "3:30", "4:00", "4:30", "5:00", "5:30", "6:00", "6:30", "7:00", "7:30", "8:00", "8:30", "9:00", "9:30", "10:00", "10:30", "11:00", "11:30", "12:00", "12:30", "13:00", "13:30", "14:00", "14:30", "15:00", "15:30", "16:00", "16:30", "17:00", "17:30", "18:00", "18:30", "19:00", "19:30", "20:00", "20:30", "21:00", "21:30", "22:00", "22:30", "23:00", "23:30" });
            comboBoxEnd.Location = new Point(97, 90);
            comboBoxEnd.Name = "comboBoxEnd";
            comboBoxEnd.Size = new Size(69, 33);
            comboBoxEnd.TabIndex = 2;
            comboBoxEnd.SelectionChangeCommitted += comboBoxEnd_SelectionChangeCommitted;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(comboBoxStart);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(comboBoxEnd);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(447, 151);
            groupBox1.TabIndex = 5;
            groupBox1.TabStop = false;
            groupBox1.Text = "Čtvrtek 06.03.2025";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label3.Location = new Point(181, 27);
            label3.Name = "label3";
            label3.Size = new Size(89, 21);
            label3.TabIndex = 5;
            label3.Text = "Popis činnosti";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(181, 51);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(240, 72);
            textBox1.TabIndex = 4;
            textBox1.Text = "Zapínání kávovaru";
            // 
            // buttonConfirm
            // 
            buttonConfirm.DialogResult = DialogResult.OK;
            buttonConfirm.Location = new Point(360, 169);
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
            buttonRemove.Location = new Point(255, 169);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new Size(99, 41);
            buttonRemove.TabIndex = 7;
            buttonRemove.Text = "Odstranit";
            buttonRemove.UseVisualStyleBackColor = true;
            // 
            // TimeEntryV2Dialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(469, 219);
            Controls.Add(buttonRemove);
            Controls.Add(buttonConfirm);
            Controls.Add(groupBox1);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "TimeEntryV2Dialog";
            Text = "Provést záznam";
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
        private TextBox textBox1;
        private Button buttonConfirm;
        private Label label3;
        private Button buttonRemove;
    }
}