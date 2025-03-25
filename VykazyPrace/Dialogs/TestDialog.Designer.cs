namespace VykazyPrace.Dialogs
{
    partial class TestDialog
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
            dataGridView1 = new DataGridView();
            buttonDrop = new Button();
            buttonCreate = new Button();
            buttonReload = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(0, 0);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(1045, 641);
            dataGridView1.TabIndex = 0;
            // 
            // buttonDrop
            // 
            buttonDrop.Location = new Point(1057, 12);
            buttonDrop.Name = "buttonDrop";
            buttonDrop.Size = new Size(55, 28);
            buttonDrop.TabIndex = 1;
            buttonDrop.Text = "Drop";
            buttonDrop.UseVisualStyleBackColor = true;
            buttonDrop.Click += buttonDrop_Click;
            // 
            // buttonCreate
            // 
            buttonCreate.Location = new Point(1057, 46);
            buttonCreate.Name = "buttonCreate";
            buttonCreate.Size = new Size(55, 28);
            buttonCreate.TabIndex = 2;
            buttonCreate.Text = "Create";
            buttonCreate.UseVisualStyleBackColor = true;
            buttonCreate.Click += buttonCreate_Click;
            // 
            // buttonReload
            // 
            buttonReload.Location = new Point(1057, 80);
            buttonReload.Name = "buttonReload";
            buttonReload.Size = new Size(55, 28);
            buttonReload.TabIndex = 3;
            buttonReload.Text = "Reload";
            buttonReload.UseVisualStyleBackColor = true;
            buttonReload.Click += buttonReload_Click;
            // 
            // TestDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1124, 641);
            Controls.Add(buttonReload);
            Controls.Add(buttonCreate);
            Controls.Add(buttonDrop);
            Controls.Add(dataGridView1);
            MinimumSize = new Size(1140, 590);
            Name = "TestDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TestDialog";
            Load += TestDialog_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridView1;
        private Button buttonDrop;
        private Button buttonCreate;
        private Button buttonReload;
    }
}