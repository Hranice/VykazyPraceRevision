namespace VykazyPrace.Dialogs
{
    partial class ProjectManagementDialog
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
            buttonAdd = new Button();
            label2 = new Label();
            textBoxProjectContractDescription = new TextBox();
            groupBox1 = new GroupBox();
            buttonArchive = new Button();
            label5 = new Label();
            textBoxProjectContractNote = new TextBox();
            label1 = new Label();
            textBoxProjectContractTitle = new TextBox();
            label6 = new Label();
            listBoxProjectContract = new ListBox();
            buttonLoadFromFolder = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // buttonAdd
            // 
            buttonAdd.Location = new Point(12, 219);
            buttonAdd.Margin = new Padding(4, 5, 4, 5);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(235, 36);
            buttonAdd.TabIndex = 19;
            buttonAdd.Text = "Přidat";
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += buttonAdd_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label2.Location = new Point(12, 36);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(121, 21);
            label2.TabIndex = 14;
            label2.Text = "Označení projektu*";
            // 
            // textBoxProjectContractDescription
            // 
            textBoxProjectContractDescription.Location = new Point(12, 61);
            textBoxProjectContractDescription.Margin = new Padding(5, 8, 5, 8);
            textBoxProjectContractDescription.Name = "textBoxProjectContractDescription";
            textBoxProjectContractDescription.Size = new Size(121, 28);
            textBoxProjectContractDescription.TabIndex = 13;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(buttonArchive);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(textBoxProjectContractNote);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(textBoxProjectContractTitle);
            groupBox1.Controls.Add(buttonAdd);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(textBoxProjectContractDescription);
            groupBox1.Location = new Point(12, 198);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(400, 265);
            groupBox1.TabIndex = 22;
            groupBox1.TabStop = false;
            groupBox1.Text = "Přidání projektu";
            // 
            // buttonArchive
            // 
            buttonArchive.Location = new Point(255, 219);
            buttonArchive.Margin = new Padding(4, 5, 4, 5);
            buttonArchive.Name = "buttonArchive";
            buttonArchive.Size = new Size(128, 36);
            buttonArchive.TabIndex = 20;
            buttonArchive.Text = "Archivovat";
            buttonArchive.UseVisualStyleBackColor = true;
            buttonArchive.Click += buttonArchive_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label5.Location = new Point(12, 97);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(69, 21);
            label5.TabIndex = 18;
            label5.Text = "Poznámka";
            // 
            // textBoxProjectContractNote
            // 
            textBoxProjectContractNote.Location = new Point(12, 122);
            textBoxProjectContractNote.Margin = new Padding(5, 8, 5, 8);
            textBoxProjectContractNote.Multiline = true;
            textBoxProjectContractNote.Name = "textBoxProjectContractNote";
            textBoxProjectContractNote.Size = new Size(371, 84);
            textBoxProjectContractNote.TabIndex = 17;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            label1.Location = new Point(143, 36);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(104, 21);
            label1.TabIndex = 16;
            label1.Text = "Název projektu*";
            // 
            // textBoxProjectContractTitle
            // 
            textBoxProjectContractTitle.Location = new Point(143, 61);
            textBoxProjectContractTitle.Margin = new Padding(5, 8, 5, 8);
            textBoxProjectContractTitle.Name = "textBoxProjectContractTitle";
            textBoxProjectContractTitle.Size = new Size(240, 28);
            textBoxProjectContractTitle.TabIndex = 15;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Reddit Sans", 8F);
            label6.Location = new Point(12, 10);
            label6.Name = "label6";
            label6.Size = new Size(93, 17);
            label6.TabIndex = 24;
            label6.Text = "Seznam projektů:";
            // 
            // listBoxProjectContract
            // 
            listBoxProjectContract.BorderStyle = BorderStyle.FixedSingle;
            listBoxProjectContract.FormattingEnabled = true;
            listBoxProjectContract.ItemHeight = 25;
            listBoxProjectContract.Location = new Point(12, 30);
            listBoxProjectContract.Name = "listBoxProjectContract";
            listBoxProjectContract.Size = new Size(399, 127);
            listBoxProjectContract.TabIndex = 23;
            listBoxProjectContract.SelectedIndexChanged += listBoxProjectContract_SelectedIndexChanged;
            // 
            // buttonLoadFromFolder
            // 
            buttonLoadFromFolder.Location = new Point(239, 163);
            buttonLoadFromFolder.Name = "buttonLoadFromFolder";
            buttonLoadFromFolder.Size = new Size(173, 34);
            buttonLoadFromFolder.TabIndex = 21;
            buttonLoadFromFolder.Text = "Načíst ze složky..";
            buttonLoadFromFolder.UseVisualStyleBackColor = true;
            // 
            // ProjectManagementDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(424, 471);
            Controls.Add(buttonLoadFromFolder);
            Controls.Add(label6);
            Controls.Add(listBoxProjectContract);
            Controls.Add(groupBox1);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "ProjectManagementDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Správa projektů";
            Load += ProjectManagementDialog_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button buttonAdd;
        private Label label2;
        private TextBox textBoxProjectContractDescription;
        private GroupBox groupBox1;
        private Label label6;
        private ListBox listBoxProjectContract;
        private Label label1;
        private TextBox textBoxProjectContractTitle;
        private Label label5;
        private TextBox textBoxProjectContractNote;
        private Button buttonArchive;
        private Button buttonLoadFromFolder;
    }
}