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
            comboBox1 = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            comboBox2 = new ComboBox();
            groupBox1 = new GroupBox();
            buttonConfirm = new Button();
            textBox1 = new TextBox();
            label3 = new Label();
            buttonRemove = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "9:00", "9:30", "10:00", "10:30" });
            comboBox1.Location = new Point(97, 51);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(69, 33);
            comboBox1.TabIndex = 0;
            comboBox1.Text = "9:00";
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
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Items.AddRange(new object[] { "10:30", "11:00", "11:30", "12:00" });
            comboBox2.Location = new Point(97, 90);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(69, 33);
            comboBox2.TabIndex = 2;
            comboBox2.Text = "10:30";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(comboBox1);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(comboBox2);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(447, 151);
            groupBox1.TabIndex = 5;
            groupBox1.TabStop = false;
            groupBox1.Text = "Čtvrtek 06.03.2025";
            // 
            // buttonConfirm
            // 
            buttonConfirm.Location = new Point(360, 169);
            buttonConfirm.Name = "buttonConfirm";
            buttonConfirm.Size = new Size(99, 41);
            buttonConfirm.TabIndex = 6;
            buttonConfirm.Text = "Potvrdit";
            buttonConfirm.UseVisualStyleBackColor = true;
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
            // buttonRemove
            // 
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
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private ComboBox comboBox1;
        private Label label1;
        private Label label2;
        private ComboBox comboBox2;
        private GroupBox groupBox1;
        private TextBox textBox1;
        private Button buttonConfirm;
        private Label label3;
        private Button buttonRemove;
    }
}