﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VykazyPrace.Dialogs;

namespace VykazyPrace.UserControls.Calendar
{
    public partial class DayUC : UserControl
    {
        public DayUC()
        {
            InitializeComponent();
        }

        private void labelDay_DoubleClick(object sender, EventArgs e)
        {
            this.OnDoubleClick(EventArgs.Empty);
        }

        private void labelHours_DoubleClick(object sender, EventArgs e)
        {
            this.OnDoubleClick(EventArgs.Empty);
        }
    }
}
