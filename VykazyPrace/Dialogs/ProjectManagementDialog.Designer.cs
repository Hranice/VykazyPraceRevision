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
            panel1 = new Panel();
            buttonProject = new Button();
            buttonContract = new Button();
            groupBox1 = new GroupBox();
            label6 = new Label();
            listBoxProjectContract = new ListBox();
            label1 = new Label();
            textBoxProjectContractTitle = new TextBox();
            textBoxProjectContractNote = new TextBox();
            label5 = new Label();
            buttonRemove = new Button();
            panel1.SuspendLayout();
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
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(buttonProject);
            panel1.Controls.Add(buttonContract);
            panel1.Location = new Point(12, 12);
            panel1.Name = "panel1";
            panel1.Size = new Size(400, 33);
            panel1.TabIndex = 21;
            // 
            // buttonProject
            // 
            buttonProject.BackColor = Color.White;
            buttonProject.Dock = DockStyle.Left;
            buttonProject.FlatAppearance.BorderSize = 0;
            buttonProject.FlatStyle = FlatStyle.Flat;
            buttonProject.Font = new Font("Reddit Sans", 10F, FontStyle.Bold);
            buttonProject.Location = new Point(0, 0);
            buttonProject.Name = "buttonProject";
            buttonProject.Size = new Size(200, 31);
            buttonProject.TabIndex = 3;
            buttonProject.Text = "PROJEKT";
            buttonProject.UseVisualStyleBackColor = false;
            // 
            // buttonContract
            // 
            buttonContract.BackColor = SystemColors.AppWorkspace;
            buttonContract.Dock = DockStyle.Right;
            buttonContract.FlatAppearance.BorderSize = 0;
            buttonContract.FlatStyle = FlatStyle.Flat;
            buttonContract.Font = new Font("Reddit Sans", 10F);
            buttonContract.Location = new Point(198, 0);
            buttonContract.Name = "buttonContract";
            buttonContract.Size = new Size(200, 31);
            buttonContract.TabIndex = 4;
            buttonContract.Text = "ZAKÁZKA";
            buttonContract.UseVisualStyleBackColor = false;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(buttonRemove);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(textBoxProjectContractNote);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(textBoxProjectContractTitle);
            groupBox1.Controls.Add(buttonAdd);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(textBoxProjectContractDescription);
            groupBox1.Location = new Point(12, 167);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(400, 265);
            groupBox1.TabIndex = 22;
            groupBox1.TabStop = false;
            groupBox1.Text = "Přidání projektu";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Reddit Sans", 8F);
            label6.Location = new Point(10, 55);
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
            listBoxProjectContract.Items.AddRange(new object[] { "0230I24 - Automatické balení po pecích a UV", "tes", "test", "test", "test", "tes" });
            listBoxProjectContract.Location = new Point(12, 75);
            listBoxProjectContract.Name = "listBoxProjectContract";
            listBoxProjectContract.Size = new Size(399, 77);
            listBoxProjectContract.TabIndex = 23;
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
            // textBoxProjectContractNote
            // 
            textBoxProjectContractNote.Location = new Point(12, 122);
            textBoxProjectContractNote.Margin = new Padding(5, 8, 5, 8);
            textBoxProjectContractNote.Multiline = true;
            textBoxProjectContractNote.Name = "textBoxProjectContractNote";
            textBoxProjectContractNote.Size = new Size(371, 84);
            textBoxProjectContractNote.TabIndex = 17;
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
            // buttonRemove
            // 
            buttonRemove.Location = new Point(255, 219);
            buttonRemove.Margin = new Padding(4, 5, 4, 5);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new Size(128, 36);
            buttonRemove.TabIndex = 20;
            buttonRemove.Text = "Odstranit";
            buttonRemove.UseVisualStyleBackColor = true;
            // 
            // ProjectManagementDialog
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(424, 442);
            Controls.Add(label6);
            Controls.Add(listBoxProjectContract);
            Controls.Add(panel1);
            Controls.Add(groupBox1);
            Font = new Font("Reddit Sans", 12F);
            Margin = new Padding(4, 5, 4, 5);
            Name = "ProjectManagementDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Správa projektů";
            panel1.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button buttonAdd;
        private Label label2;
        private TextBox textBoxProjectContractDescription;
        private Panel panel1;
        private Button buttonProject;
        private Button buttonContract;
        private GroupBox groupBox1;
        private Label label6;
        private ListBox listBoxProjectContract;
        private Label label1;
        private TextBox textBoxProjectContractTitle;
        private Label label5;
        private TextBox textBoxProjectContractNote;
        private Button buttonRemove;
    }
}