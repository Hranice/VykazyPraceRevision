namespace VykazyPrace.Dialogs
{
    partial class LoadProjectsFromFolderDialog
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
            button1 = new Button();
            label1 = new Label();
            buttonLoadProjectsFromFolder = new Button();
            button3 = new Button();
            comboBox1 = new ComboBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 12);
            button1.Name = "button1";
            button1.Size = new Size(75, 41);
            button1.TabIndex = 1;
            button1.Text = "Složka";
            button1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(93, 20);
            label1.Name = "label1";
            label1.Size = new Size(347, 25);
            label1.TabIndex = 2;
            label1.Text = "Z:\\Zakazkova dokumentace\\_HGD\\00_Projekty";
            // 
            // buttonLoadProjectsFromFolder
            // 
            buttonLoadProjectsFromFolder.Location = new Point(334, 94);
            buttonLoadProjectsFromFolder.Name = "buttonLoadProjectsFromFolder";
            buttonLoadProjectsFromFolder.Size = new Size(106, 41);
            buttonLoadProjectsFromFolder.TabIndex = 3;
            buttonLoadProjectsFromFolder.Text = "Provést";
            buttonLoadProjectsFromFolder.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(253, 94);
            button3.Name = "button3";
            button3.Size = new Size(75, 41);
            button3.TabIndex = 4;
            button3.Text = "Zavřít";
            button3.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            comboBox1.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "Smazat dosavadní záznamy", "Ponechat záznamy a ignorovat duplikáty (přidat jen nové)" });
            comboBox1.Location = new Point(12, 59);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(428, 29);
            comboBox1.TabIndex = 5;
            comboBox1.Text = "Ponechat záznamy a ignorovat duplikáty (přidat jen nové)";
            // 
            // LoadProjectsFromFolderDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(450, 145);
            Controls.Add(comboBox1);
            Controls.Add(button3);
            Controls.Add(buttonLoadProjectsFromFolder);
            Controls.Add(label1);
            Controls.Add(button1);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "LoadProjectsFromFolderDialog";
            Text = "Načíst ze složky";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Label label1;
        private Button buttonLoadProjectsFromFolder;
        private Button button3;
        private ComboBox comboBox1;
    }
}