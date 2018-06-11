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
    public partial class ThumbnailView_form : Form
    {
        public ThumbnailView_form(Bitmap img)
        {
            InitializeComponent();
            Thumb_pictureBox.Image = (Image)img;
        }

        private void Okay_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
