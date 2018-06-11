using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Xmods.ImageFileHandler
{
    internal partial class JPGoptions : Form
    {
        internal int jpgQualitySelected { get { return Quality_trackBar.Value; } }
        internal JPGoptions(int jpgQualityPercent)
        {
            InitializeComponent();
            Quality_trackBar.Value = jpgQualityPercent;
            Quality_display.Text = Quality_trackBar.Value.ToString();
        }

        private void Quality_trackBar_Scroll(object sender, EventArgs e)
        {
            Quality_display.Text = Quality_trackBar.Value.ToString();
        }

        private void Save_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancel_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
