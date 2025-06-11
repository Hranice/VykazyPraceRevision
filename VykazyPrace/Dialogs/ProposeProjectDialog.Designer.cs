namespace VykazyPrace.Dialogs
{
    partial class ProposeProjectDialog
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
            label9 = new Label();
            textBoxProjectTitle = new TextBox();
            textBoxProjectDescription = new TextBox();
            label7 = new Label();
            buttonSend = new Button();
            SuspendLayout();
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Reddit Sans", 9.75F);
            label9.Location = new Point(13, 11);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(121, 21);
            label9.TabIndex = 42;
            label9.Text = "Označení projektu*";
            // 
            // textBoxProjectTitle
            // 
            textBoxProjectTitle.Location = new Point(158, 35);
            textBoxProjectTitle.Margin = new Padding(5, 8, 5, 8);
            textBoxProjectTitle.Name = "textBoxProjectTitle";
            textBoxProjectTitle.PlaceholderText = "Projekt vývoje software na výkaz hodin";
            textBoxProjectTitle.Size = new Size(297, 28);
            textBoxProjectTitle.TabIndex = 41;
            // 
            // textBoxProjectDescription
            // 
            textBoxProjectDescription.Location = new Point(14, 35);
            textBoxProjectDescription.Margin = new Padding(5, 8, 5, 8);
            textBoxProjectDescription.Name = "textBoxProjectDescription";
            textBoxProjectDescription.PlaceholderText = "000E00";
            textBoxProjectDescription.ReadOnly = true;
            textBoxProjectDescription.Size = new Size(135, 28);
            textBoxProjectDescription.TabIndex = 43;
            textBoxProjectDescription.Text = "000N01";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Reddit Sans", 9.75F);
            label7.Location = new Point(158, 11);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(104, 21);
            label7.TabIndex = 44;
            label7.Text = "Název projektu*";
            // 
            // buttonSend
            // 
            buttonSend.Location = new Point(361, 74);
            buttonSend.Name = "buttonSend";
            buttonSend.Size = new Size(94, 40);
            buttonSend.TabIndex = 45;
            buttonSend.Text = "Odeslat";
            buttonSend.UseVisualStyleBackColor = true;
            buttonSend.Click += buttonSend_Click;
            // 
            // ProposeProjectDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(467, 130);
            Controls.Add(buttonSend);
            Controls.Add(label9);
            Controls.Add(textBoxProjectTitle);
            Controls.Add(textBoxProjectDescription);
            Controls.Add(label7);
            Font = new Font("Reddit Sans", 12F);
            Name = "ProposeProjectDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Návrh projektu";
            Load += ProposeProjectDialog_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label9;
        private TextBox textBoxProjectTitle;
        private TextBox textBoxProjectDescription;
        private Label label7;
        private Button buttonSend;
    }
}