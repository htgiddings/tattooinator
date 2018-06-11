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
    internal partial class DDSoptions : Form
    {
        internal string DDScompression { get { return Compression_comboBox.SelectedItem as string; } }
        internal bool DDSmipmaps { get { return Mipmap_checkBox.Checked; } }

        internal DDSoptions(DdsSaveOptions saveOptions)
        {
            InitializeComponent();
            Compression_comboBox.SelectedItem = saveOptions.ddsCompressFormat;
            Mipmap_checkBox.Checked = saveOptions.generateMipmaps;
        }

        private void DDSsaveGo_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void DDScancel_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private void DDSoptions_Load(object sender, EventArgs e)
        {

        }
    }
}
