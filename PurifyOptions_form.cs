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
    public partial class PurifyOptions_form : Form
    {
        DdsFile originalDDS;
        byte[] rgb = new byte[3] { 210, 180, 140 };
        Color preR = new Color(255, 0, 0);
        Color preG = new Color(0, 255, 0);
        Color preB = new Color(0, 0, 255);
        Color preA = new Color(0, 0, 0);
        int contrastTrackbarValue = 0;
        int brightTrackbarValue = 0;
        bool[] updateChannels;

        public Purify_options PurifyOptions
        {
            get
            {
                return new Purify_options(PurifyAmount_trackBar.Value, Brightness_trackBar.Value, PurifyRed_checkBox.Checked,
                    PurifyGreen_checkBox.Checked, PurifyBlue_checkBox.Checked, PurifyAlpha_checkBox.Checked,
                    Preview_checkBox.Checked);
            }
        }

        public PurifyOptions_form(DdsFile dds, Purify_options options)
        {
            InitializeComponent();
            updateChannels = new bool[] { options.Red, options.Green, options.Blue, options.Alpha };
            PurifyAmount_trackBar.Value = options.Contrast;
            PurifyRed_checkBox.Checked = options.Red;
            PurifyGreen_checkBox.Checked = options.Green;
            PurifyBlue_checkBox.Checked = options.Blue;
            PurifyAlpha_checkBox.Checked = options.Alpha;
            Preview_checkBox.Checked = options.Preview;
            originalDDS = new DdsFile();
            originalDDS.CreateImage(dds.Resize(new Size(350, 350)), false);
            contrastTrackbarValue = options.Contrast;
            brightTrackbarValue = options.Brightness;
            UpdatePreview();
        }

        private void Brightness_trackBar_Changed(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void PurifyAmount_trackBar_Changed(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (!Preview_checkBox.Checked) return;
            DdsFile previewDDS = new DdsFile();
            previewDDS.CreateImage(originalDDS, false);
            contrastTrackbarValue = PurifyAmount_trackBar.Value;
            brightTrackbarValue = Brightness_trackBar.Value;
            previewDDS.SetPixels(AdjustColors);
            previewDDS.SetPixels(TatPreview);
            Preview_pictureBox.Image = previewDDS.Image;
            previewDDS.Dispose();
        }

        internal uint TatPreview(int x, int y, uint pixel)
        {
            return PixelBangers.TatPreview(pixel, rgb, preR, preG, preB, preA);
        }

        internal uint AdjustColors(int x, int y, uint pixel)
        {
            return PixelBangers.BrightnessContrast(pixel, brightTrackbarValue, contrastTrackbarValue, updateChannels);
        }

        private void Accept_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void Cancel_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void BGcolor_panel_Click(object sender, EventArgs e)
        {
            ColorDialog bg = new ColorDialog();
            bg.AllowFullOpen = true;
            bg.AnyColor = true;
            bg.Color = BGcolor_panel.BackColor;
            bg.FullOpen = true;
            if (bg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BGcolor_panel.BackColor = bg.Color;
                rgb = new byte[] { bg.Color.R, bg.Color.G, bg.Color.B };
            }
            UpdatePreview();
        }

        private void PurifyRed_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            updateChannels[0] = PurifyRed_checkBox.Checked;
            UpdatePreview();
        }

        private void PurifyGreen_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            updateChannels[1] = PurifyGreen_checkBox.Checked;
            UpdatePreview();
        }

        private void PurifyBlue_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            updateChannels[2] = PurifyBlue_checkBox.Checked;
            UpdatePreview();
        }

        private void PurifyAlpha_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            updateChannels[3] = PurifyAlpha_checkBox.Checked;
            UpdatePreview();
        }
    }

    public class Purify_options
    {
        int contrast, bright;
        bool[] channels;
        bool preview;
        public int Contrast { get { return contrast; } set { contrast = value; } }
        public int Brightness { get { return bright; } set { bright = value; } }
        public bool Red { get { return channels[0]; } set { channels[0] = value; } }
        public bool Green { get { return channels[1]; } set { channels[1] = value; } }
        public bool Blue { get { return channels[2]; } set { channels[2] = value; } }
        public bool Alpha { get { return channels[3]; } set { channels[3] = value; } }
        public bool[] ActiveChannels { get { return channels; } }
        public bool Preview { get { return preview; } set { preview = value; } }

        public Purify_options()
        {
            contrast = 0;
            bright = 0;
            channels = new bool[] { true, true, true, false };
            preview = true;
        }
        public Purify_options(int ContrastChange, int BrightnessChange, bool IncludeRed, bool IncludeGreen, bool IncludeBlue, bool IncludeAlpha, bool ShowPreview)
        {
            contrast = ContrastChange;
            bright = BrightnessChange;
            channels = new bool[] { IncludeRed, IncludeGreen, IncludeBlue, IncludeAlpha };
            preview = ShowPreview;
        }
    }
}
