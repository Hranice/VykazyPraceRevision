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
            customComboBox1 = new VykazyPrace.UserControls.CustomComboBox();
            buttonSave = new Button();
            button1 = new Button();
            listBoxEvents = new ListBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(0, 0);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(384, 641);
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
            // customComboBox1
            // 
            customComboBox1.Location = new Point(939, 151);
            customComboBox1.Name = "customComboBox1";
            customComboBox1.SelectedIndex = -1;
            customComboBox1.Size = new Size(150, 23);
            customComboBox1.TabIndex = 4;
            // 
            // buttonSave
            // 
            buttonSave.Location = new Point(1057, 114);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(55, 28);
            buttonSave.TabIndex = 5;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += buttonSave_Click;
            // 
            // button1
            // 
            button1.Location = new Point(963, 267);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 6;
            button1.Text = "Kalendář";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // listBoxEvents
            // 
            listBoxEvents.FormattingEnabled = true;
            listBoxEvents.ItemHeight = 15;
            listBoxEvents.Location = new Point(594, 330);
            listBoxEvents.Name = "listBoxEvents";
            listBoxEvents.Size = new Size(459, 94);
            listBoxEvents.TabIndex = 7;
            // 
            // TestDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1124, 641);
            Controls.Add(listBoxEvents);
            Controls.Add(button1);
            Controls.Add(buttonSave);
            Controls.Add(customComboBox1);
            Controls.Add(buttonReload);
            Controls.Add(buttonCreate);
            Controls.Add(buttonDrop);
            Controls.Add(dataGridView1);
            MinimumSize = new Size(1140, 590);
            Name = "TestDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TestDialog";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridView1;
        private Button buttonDrop;
        private Button buttonCreate;
        private Button buttonReload;
        private UserControls.CustomComboBox customComboBox1;
        private Button buttonSave;
        private Button button1;
        private ListBox listBoxEvents;
    }
}