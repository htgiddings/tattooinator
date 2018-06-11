using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tattooinator
{
    public partial class SourceImage_Form : Form
    {
        public SourceImage_Form(DDSPanel dp)
        {
            dp.Location = new Point(5, 5);
            dp.Size = new Size(512, 512);
            dp.Fit = true;
            this.Controls.Add(dp);
            dp.Show();
            InitializeComponent();
        }
    }
}
