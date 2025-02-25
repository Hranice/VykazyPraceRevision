namespace VykazyPrace
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            dataToolStripMenuItem = new ToolStripMenuItem();
            vizualizaceToolStripMenuItem = new ToolStripMenuItem();
            exportToolStripMenuItem = new ToolStripMenuItem();
            uživateléToolStripMenuItem = new ToolStripMenuItem();
            správaUživatelůToolStripMenuItem = new ToolStripMenuItem();
            projektyToolStripMenuItem = new ToolStripMenuItem();
            archivToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { dataToolStripMenuItem, uživateléToolStripMenuItem, projektyToolStripMenuItem, archivToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1029, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // dataToolStripMenuItem
            // 
            dataToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { vizualizaceToolStripMenuItem, exportToolStripMenuItem });
            dataToolStripMenuItem.Name = "dataToolStripMenuItem";
            dataToolStripMenuItem.Size = new Size(43, 20);
            dataToolStripMenuItem.Text = "Data";
            // 
            // vizualizaceToolStripMenuItem
            // 
            vizualizaceToolStripMenuItem.Name = "vizualizaceToolStripMenuItem";
            vizualizaceToolStripMenuItem.Size = new Size(131, 22);
            vizualizaceToolStripMenuItem.Text = "Vizualizace";
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new Size(131, 22);
            exportToolStripMenuItem.Text = "Export";
            // 
            // uživateléToolStripMenuItem
            // 
            uživateléToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { správaUživatelůToolStripMenuItem });
            uživateléToolStripMenuItem.Name = "uživateléToolStripMenuItem";
            uživateléToolStripMenuItem.Size = new Size(66, 20);
            uživateléToolStripMenuItem.Text = "Uživatelé";
            // 
            // správaUživatelůToolStripMenuItem
            // 
            správaUživatelůToolStripMenuItem.Name = "správaUživatelůToolStripMenuItem";
            správaUživatelůToolStripMenuItem.Size = new Size(180, 22);
            správaUživatelůToolStripMenuItem.Text = "Správa uživatelů";
            správaUživatelůToolStripMenuItem.Click += správaUživatelůToolStripMenuItem_Click;
            // 
            // projektyToolStripMenuItem
            // 
            projektyToolStripMenuItem.Name = "projektyToolStripMenuItem";
            projektyToolStripMenuItem.Size = new Size(62, 20);
            projektyToolStripMenuItem.Text = "Projekty";
            // 
            // archivToolStripMenuItem
            // 
            archivToolStripMenuItem.Name = "archivToolStripMenuItem";
            archivToolStripMenuItem.Size = new Size(53, 20);
            archivToolStripMenuItem.Text = "Archiv";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(9F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1029, 750);
            Controls.Add(menuStrip1);
            Font = new Font("Reddit Sans", 12F);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(4, 5, 4, 5);
            Name = "MainForm";
            Text = "Výkazy práce";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem dataToolStripMenuItem;
        private ToolStripMenuItem vizualizaceToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem uživateléToolStripMenuItem;
        private ToolStripMenuItem projektyToolStripMenuItem;
        private ToolStripMenuItem archivToolStripMenuItem;
        private ToolStripMenuItem správaUživatelůToolStripMenuItem;
    }
}
