using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using CASPartResource;
using NameMapResource;
using s3pi.Interfaces;
using s3pi.Package;
using s3pi.GenericRCOLResource;
using TxtcResource;
using Xmods.FNV;
using Xmods.ImageFileHandler;

namespace Tattooinator
{
    public partial class TattForm : Form
    {
        string PngFilter = "PNG image files (*.png)|*.png|All files (*.*)|*.*";
        string ImageFilterOpen = "Compatible Image files (*.bmp;*.dds;*.gif;*.jpg;*.jpeg;*.exif;*.png;*.tif;*.tiff)|*.bmp;*.dds;*.gif;*.jpg;*.jpeg;*.exif;*.png;*.tif;*.tiff|All files (*.*)|*.*";
        string ImageFilterSave = "BMP Image (*.bmp)|*.bmp|DDS Image (*.dds)|*.dds|GIF Image (*.gif)|*.gif|JPG Image (*.jpg;*.jpeg)|*.jpg;*.jpeg|PNG Image (*.png)|*.png|TIFF Image (*.tif;*.tiff)|*.tif;*.tiff|All files (*.*)|*.*";
        string PackageFilter = "Package files (*.package)|*.package|All files (*.*)|*.*";
        Preset[] presets = new Preset[3] { new Preset(0xFFFF0000, 0xFF00FF00, 0xFF0000FF, 0xFF000000),
                                           new Preset(0xFFCC0000, 0xFF00CC00, 0xFF0000CC, 0xFF000000),
                                           new Preset(0xFF880000, 0xFF008800, 0xFF000088, 0xFF000000)};
        int currentPreset = 0;
        Color bgColor = new Color(0x00D2B48Cu);
        DdsFile dds = null;
        DdsFile ddsOriginal = null;
        DdsSaveOptions saveOptionsDDS = new DdsSaveOptions("DXT5", true, 90);
        uint[] resizePixels;
        Size resizeSize;
        Bitmap thumbNail = null;
        Purify_options purifyOptions = new Purify_options();

        public TattForm()
        {
            InitializeComponent();
            PresetPrep();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Tattooinator V3.0" + System.Environment.NewLine + "by cmar");
        }

        private void fNVHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HashPanel fnv = new HashPanel();
            fnv.Show();
        }

        private void TattooImageFile_button_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = ImageFilterOpen;
            openFileDialog1.Title = "Select Image File";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.CheckFileExists = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                TattooImageFile.Text = openFileDialog1.FileName;
            }
            else
            {
                return;
            }
            if (String.CompareOrdinal(TattooImageFile.Text, " ") <= 0 || !File.Exists(TattooImageFile.Text))
            {
                MessageBox.Show("You must select a compatible image file!");
                return;
            }

            dds = ImageHandler.OpenDDS(TattooImageFile.Text);
            if (dds == null) return;

            Wait_label.Visible = true;
            Wait_label.Refresh();

            ddsOriginal = new DdsFile();
            ddsOriginal.CreateImage(dds, false);

            if (dds.AlphaDepth == 0)
            {
                dds.UseDXT = false;
                dds.AlphaDepth = 8;
                dds.SetPixels(ZeroAlpha);
            }

            Update_DDSdisplay();
        }

        private void ImagePaste_button_Click(object sender, EventArgs e)
        {
            Bitmap clip = (Bitmap)Clipboard.GetImage();
            if (clip == null) return;
            dds = new DdsFile();
            dds.CreateImage(clip, false);
            if (!Image.IsAlphaPixelFormat(clip.PixelFormat))
            {
                dds.UseDXT = false;
                dds.AlphaDepth = 8;
                dds.SetPixels(ZeroAlpha);
            }
            ddsOriginal = new DdsFile();
            ddsOriginal.CreateImage(dds, false);
            Update_DDSdisplay();
        }

        private void ThumbnailFile_button_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = PngFilter;
            openFileDialog1.Title = "Select PNG thumbnail image file";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.CheckFileExists = true;
            if (!(openFileDialog1.ShowDialog() == DialogResult.OK))
            {
                return;
            }
            Bitmap tmp = ImageHandler.OpenImage(openFileDialog1.FileName);
            thumbNail = new Bitmap(tmp, 256, 256);
        }

        private void ThumbnailView_button_Click(object sender, EventArgs e)
        {
            if (thumbNail == null)
            {
                MessageBox.Show("No thumbnail image is loaded!");
                return;
            }
            ThumbnailView_form view = new ThumbnailView_form(thumbNail);
            view.ShowDialog();
        }

        private void ThumbnailRemove_button_Click(object sender, EventArgs e)
        {
            thumbNail = null;
        }

        internal void Update_DDSdisplay()
        {
            if (dds == null) return;
            Wait_label.Visible = true;
            Wait_label.Refresh();
            DdsFile preview = new DdsFile();
            preview.CreateImage(dds, false);
            preview.SetPixels(TatPreview);
            if (!preview.Size.Equals(new System.Drawing.Size(512, 512))) preview = preview.Resize(new System.Drawing.Size(512, 512));
            Preview_pictureBox.Image = preview.Image;
            preview.Dispose();
            Wait_label.Visible = false;
            Preset1_button.Font = (currentPreset == 0) ? new Font(Preset1_button.Font, FontStyle.Bold) : new Font(Preset1_button.Font, FontStyle.Regular);
            Preset2_button.Font = (currentPreset == 1) ? new Font(Preset2_button.Font, FontStyle.Bold) : new Font(Preset2_button.Font, FontStyle.Regular);
            Preset3_button.Font = (currentPreset == 2) ? new Font(Preset3_button.Font, FontStyle.Bold) : new Font(Preset3_button.Font, FontStyle.Regular);
            Resize_checkBox.Checked = !dds.Size.Equals(new System.Drawing.Size(512, 512));
        }

        internal uint TatPreview(int x, int y, uint pixel)
        {
            return PixelBangers.TatPreview(pixel, new byte[3] { bgColor.Red, bgColor.Green, bgColor.Blue },
                presets[currentPreset].rChannelColor, presets[currentPreset].gChannelColor,
                presets[currentPreset].bChannelColor, presets[currentPreset].aChannelColor);
        }

        internal uint TatTransform(int x, int y, uint pixel)
        {
            Color pixelColor = new Color(pixel);
            Color newColor = new Color(pixel);

            if (PhotoConvert_radioButton.Checked)
            {
                double rtemp = pixelColor.Blue*((255.0-(.4*pixelColor.Green))/255.0);
                newColor.Blue = (byte) (rtemp*((255.0-(.4*pixelColor.Red))/255.0));
                newColor.Green = (byte) (pixelColor.Green*((255.0-(.4*pixelColor.Red))/255.0));
                newColor.Red = pixelColor.Red;
            }

            if (ReplaceAlpha_checkBox.Checked)
            {
                if (Black2Alpha_radioButton.Checked) 
                {
                    if (!White2Back_checkBox.Checked & (pixelColor.Red == 0 & pixelColor.Green == 0 & pixelColor.Blue == 0))
                    {
                        newColor.Alpha = 0;
                    }
                    else
                    {
                        float rscale = (Math.Abs(pixelColor.Red-pixelColor.Green) +
                                        Math.Abs(pixelColor.Green-pixelColor.Blue) +
                                        Math.Abs(pixelColor.Red-pixelColor.Blue)) / 2;
                        newColor.Alpha = (byte) ((255 - MaxByte(pixelColor.Red, pixelColor.Green, pixelColor.Blue)) * ((255 - rscale) / 255)); 
                    }
                }
                else if (White2Alpha_radioButton.Checked)
                {
                    if (White2Back_checkBox.Checked & (pixelColor.Red == 255 & pixelColor.Green == 255 & pixelColor.Blue == 255))
                    {
                        newColor.Alpha = 0;
                    }
                    else
                    {
                        float rscale = (Math.Abs(pixelColor.Red-pixelColor.Green) +
                                        Math.Abs(pixelColor.Green-pixelColor.Blue) +
                                        Math.Abs(pixelColor.Red-pixelColor.Blue)) / 2;
                        newColor.Alpha = (byte) (((pixelColor.Red + pixelColor.Green + pixelColor.Blue) / 3) * ((255 - rscale) / 255));
                    }
                }
                else if (BlankAlpha_radioButton.Checked)
                {
                    newColor.Alpha = 0;
                }
                else if (invertAlpha_radioButton.Checked)
                {
                    newColor.Alpha = (byte)(255 - pixelColor.Alpha);
                }
            }

            if (White2Back_checkBox.Checked & pixelColor.Red == 255 & pixelColor.Green == 255 & pixelColor.Blue == 255)
            {
                newColor.Red = 0;
                newColor.Green = 0;
                newColor.Blue = 0;
            }

            if (Black2Red_radioButton.Checked)
            {
                float rscale = (Math.Abs(pixelColor.Red-pixelColor.Green) +
                                Math.Abs(pixelColor.Green-pixelColor.Blue) +
                                Math.Abs(pixelColor.Red-pixelColor.Blue)) / 2;
                newColor.Red = (byte) ((255 - MaxByte(pixelColor.Red, pixelColor.Green, pixelColor.Blue)) * ((255 - rscale) / 255));
                newColor.Green = 0;
                newColor.Blue = 0;
            }

            return newColor.Hue;
        }

        public static byte MaxByte(byte value1, byte value2, byte value3)
        {
            byte tmp = Byte.MinValue;
            if (value1 > tmp) tmp = value1;
            if (value2 > tmp) tmp = value2;
            if (value3 > tmp) tmp = value3;
            return tmp;
        }

        internal uint ZeroAlpha(int x, int y, uint pixel)
        {
            return (pixel & 0x00FFFFFFu);
        }

        internal uint AdjustColors(int x, int y, uint pixel)
        {
            return PixelBangers.BrightnessContrast(pixel, purifyOptions.Brightness, purifyOptions.Contrast, purifyOptions.ActiveChannels);
        }

        internal uint Resizer(int x, int y, uint pixel)
        {
            int originX = 255 - (resizeSize.Width / 2);
            int originY = 255 - (resizeSize.Height / 2);
            if ((x < originX) || (x >= (255 + (resizeSize.Width / 2))) ||
                (y < originY) || (y >= (255 + (resizeSize.Height / 2))))
            {
                return 0U;
            }
            else
            {
                return resizePixels[((y - originY) * resizeSize.Width) + (x - originX)];
            }
        }

        private void ChannelR_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!ChannelR_checkBox.Checked)
            {
                ChannelG_checkBox.Checked = false;
                ChannelB_checkBox.Checked = false;
                ChannelA_checkBox.Checked = false;
            }

        }

        private void ChannelG_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ChannelG_checkBox.Checked)
            {
                ChannelR_checkBox.Checked = true;
            }
            else
            {
                ChannelB_checkBox.Checked = false;
                ChannelA_checkBox.Checked = false;
            }
        }

        private void ChannelB_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ChannelB_checkBox.Checked)
            {
                ChannelR_checkBox.Checked = true;
                ChannelG_checkBox.Checked = true;
            }
            else
            {
                ChannelA_checkBox.Checked = false;
            }

        }

        private void ChannelA_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ChannelA_checkBox.Checked)
            {
                ChannelR_checkBox.Checked = true;
                ChannelG_checkBox.Checked = true;
                ChannelB_checkBox.Checked = true;
            }
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
                bgColor = new Color(bg.Color.R, bg.Color.G, bg.Color.B, bg.Color.A);
            }
            Update_DDSdisplay();
        }

        private void PresetPrep()
        {
            if (Presets_dataGridView.Rows.Count > 0) Presets_dataGridView.Rows.Clear();
            Presets_dataGridView.Rows.Add(presets.Length);
            for (int i = 0; i < presets.Length; i++)
            {
                //Presets_dataGridView.Rows[i].Height = 35;
                Presets_dataGridView.Rows[i].Cells["PresetNumber"].Value = "Preset " + (i + 1).ToString();
                Presets_dataGridView.Rows[i].Cells["ChannelR"].Style.BackColor = presets[i].rChannelColor.SystemColor;
                Presets_dataGridView.Rows[i].Cells["ChannelG"].Style.BackColor = presets[i].gChannelColor.SystemColor;
                Presets_dataGridView.Rows[i].Cells["ChannelB"].Style.BackColor = presets[i].bChannelColor.SystemColor;
                Presets_dataGridView.Rows[i].Cells["ChannelA"].Style.BackColor = presets[i].aChannelColor.SystemColor;
                foreach (DataGridViewCell c in Presets_dataGridView.Rows[i].Cells)
                {
                    c.ToolTipText = "Set up the preset colors";
                }
            }
        }

        private void Presets_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex >= Presets_dataGridView.Columns["ChannelR"].Index & 
                e.ColumnIndex <= Presets_dataGridView.Columns["ChannelA"].Index)
            {
                ColorDialog pre = new ColorDialog();
                pre.AllowFullOpen = true;
                pre.AnyColor = true;
                pre.Color = Presets_dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor;
                pre.FullOpen = true;
                if (pre.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Presets_dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = pre.Color;
                    switch (e.ColumnIndex)
                    {
                        case 1:
                            presets[e.RowIndex].rChannelHue = (uint)pre.Color.ToArgb();
                            break;
                        case 2:
                            presets[e.RowIndex].gChannelHue = (uint)pre.Color.ToArgb();
                            break;
                        case 3:
                            presets[e.RowIndex].bChannelHue = (uint)pre.Color.ToArgb();
                            break;
                        case 4:
                            presets[e.RowIndex].aChannelHue = (uint)pre.Color.ToArgb();
                            break;
                    }
                    Update_DDSdisplay();
                }
            }
            else if (e.ColumnIndex == Presets_dataGridView.Columns["MoveUp"].Index)
            {
                if (e.RowIndex <= 0) return;
                presets[e.RowIndex - 1].rChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex].Cells["ChannelR"].Style.BackColor.ToArgb();
                presets[e.RowIndex - 1].gChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex].Cells["ChannelG"].Style.BackColor.ToArgb();
                presets[e.RowIndex - 1].bChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex].Cells["ChannelB"].Style.BackColor.ToArgb();
                presets[e.RowIndex - 1].aChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex].Cells["ChannelA"].Style.BackColor.ToArgb();
                presets[e.RowIndex].rChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex - 1].Cells["ChannelR"].Style.BackColor.ToArgb();
                presets[e.RowIndex].gChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex - 1].Cells["ChannelG"].Style.BackColor.ToArgb();
                presets[e.RowIndex].bChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex - 1].Cells["ChannelB"].Style.BackColor.ToArgb();
                presets[e.RowIndex].aChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex - 1].Cells["ChannelA"].Style.BackColor.ToArgb();
                PresetPrep();
                Update_DDSdisplay();
            }
            else if (e.ColumnIndex == Presets_dataGridView.Columns["MoveDown"].Index)
            {
                if (e.RowIndex >= Presets_dataGridView.RowCount - 1) return;
                presets[e.RowIndex + 1].rChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex].Cells["ChannelR"].Style.BackColor.ToArgb();
                presets[e.RowIndex + 1].gChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex].Cells["ChannelG"].Style.BackColor.ToArgb();
                presets[e.RowIndex + 1].bChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex].Cells["ChannelB"].Style.BackColor.ToArgb();
                presets[e.RowIndex + 1].aChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex].Cells["ChannelA"].Style.BackColor.ToArgb();
                presets[e.RowIndex].rChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex + 1].Cells["ChannelR"].Style.BackColor.ToArgb();
                presets[e.RowIndex].gChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex + 1].Cells["ChannelG"].Style.BackColor.ToArgb();
                presets[e.RowIndex].bChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex + 1].Cells["ChannelB"].Style.BackColor.ToArgb();
                presets[e.RowIndex].aChannelHue = (uint)Presets_dataGridView.Rows[e.RowIndex + 1].Cells["ChannelA"].Style.BackColor.ToArgb();
                PresetPrep();
                Update_DDSdisplay();
            }
        }

        private void Presets_dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            Presets_dataGridView.ClearSelection();
        }


        private void Black2Red_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (Black2Red_radioButton.Checked)
            {
                White2Back_checkBox.Checked = true;
                ReplaceAlpha_checkBox.Checked = true;
                BlankAlpha_radioButton.Checked = true;
            }
        }

        private void PhotoConvert_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (PhotoConvert_radioButton.Checked)
            {
                White2Back_checkBox.Checked = true;
                ReplaceAlpha_checkBox.Checked = true;
                Black2Alpha_radioButton.Checked = true;
            }
        }

        private void ReplaceAlpha_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            ReplaceAlpha_panel.Enabled = ReplaceAlpha_checkBox.Checked;
        }

        private void Black2Alpha_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (Black2Alpha_radioButton.Checked) White2Back_checkBox.Checked = true;
        }

        private void White2Alpha_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (Black2Alpha_radioButton.Checked) White2Back_checkBox.Checked = false;
        }

        private void TransformGo_button_Click(object sender, EventArgs e)
        {
            Wait_label.Visible = true;
            Wait_label.Refresh();

            dds.SetPixels(TatTransform);
            if (Black2Red_radioButton.Checked)
            {
                ChannelG_checkBox.Checked = false;
                ChannelB_checkBox.Checked = false;
                ChannelA_checkBox.Checked = false;
            }
            if (Black2Alpha_radioButton.Checked)
            {
                for (int i = 0; i < 3; i++)
                {
                    presets[i].aChannelHue = 0x00000000;
                    Presets_dataGridView.Rows[i].Cells["ChannelA"].Style.BackColor = presets[i].aChannelColor.SystemColor;
                }
            }
            else if (White2Alpha_radioButton.Checked)
            {
                for (int i = 0; i < 3; i++)
                {
                    presets[i].aChannelHue = 0x00FFFFFF;
                    Presets_dataGridView.Rows[i].Cells["ChannelA"].Style.BackColor = presets[i].aChannelColor.SystemColor;
                }
            }

            if (Resize_checkBox.Checked & !dds.Size.Equals(new Size(512, 512)))
            {
                Bitmap bits = dds.GetImage(true, true, true, true, false);
                int width = bits.Width;
                int height = bits.Height;

                float factorH = height / 512f;
                if (width == height)
                {
                    dds = dds.Resize(new Size(512, 512));
                }
                else if (width > height)
                {
                    float factor = 512f / width;
                    resizeSize = new Size(512, (int)(height * factor));
                    DdsFile tmp = new DdsFile();
                    tmp.CreateImage(dds, resizeSize.Width, resizeSize.Height, false);
                    resizePixels = tmp.GetImage(true, true, true, true, false).ToARGBData();
                    dds = dds.Resize(new Size(512, 512));
                    dds.SetPixels(Resizer);
                }
                else if (width < height)
                {
                    float factor = 512f / height;
                    resizeSize = new Size((int)(width * factor), 512);
                    DdsFile tmp = new DdsFile();
                    tmp.CreateImage(dds, resizeSize.Width, resizeSize.Height, false);
                    resizePixels = tmp.GetImage(true, true, true, true, false).ToARGBData();
                    dds = dds.Resize(new Size(512, 512));
                    dds.SetPixels(Resizer);
                }
            }

            Update_DDSdisplay();
        }

        private void TransformReset_button_Click(object sender, EventArgs e)
        {
            Wait_label.Visible = true;
            Wait_label.Refresh();

            dds.CreateImage(ddsOriginal, false);
            if (dds.AlphaDepth == 0)
            {
                dds.UseDXT = false;
                dds.AlphaDepth = 8;
                dds.SetPixels(ZeroAlpha);
            }

            Update_DDSdisplay();
            Black2Red_radioButton.Checked = false;
            PhotoConvert_radioButton.Checked = false;
            ReplaceAlpha_checkBox.Checked = false;
            Black2Alpha_radioButton.Checked = false;
            White2Alpha_radioButton.Checked = false;
            BlankAlpha_radioButton.Checked = false;
            White2Back_checkBox.Checked = false;
        }

        private void ViewSourcebutton_Click(object sender, EventArgs e)
        {
            if (ddsOriginal == null) return;
            DdsFile tmp = new DdsFile();
            tmp.CreateImage(ddsOriginal, false);
            DDSPanel dp = new DDSPanel();
            dp.DDSLoad(tmp);
            SourceImage_Form disp = new SourceImage_Form(dp);
            disp.Show();
        }

        private void Preset1_button_Click(object sender, EventArgs e)
        {
            currentPreset = 0;
            Update_DDSdisplay();
        }

        private void Preset2_button_Click(object sender, EventArgs e)
        {
            currentPreset = 1;
            Update_DDSdisplay();
        }

        private void Preset3_button_Click(object sender, EventArgs e)
        {
            currentPreset = 2;
            Update_DDSdisplay();
        }

        private void ImageSave_button_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = ImageFilterSave;
            saveFileDialog1.Title = "Save Tattoo Image";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.DefaultExt = "dds";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                saveOptionsDDS = ImageHandler.SaveImage(saveFileDialog1.FileName, dds, saveOptionsDDS);
            }
        }

        private void TattooGo_button_Click(object sender, EventArgs e)
        {
            float sortOrder;
            try
            {
                sortOrder = float.Parse(CASPsortOrder.Text);
            }
            catch
            {
                MessageBox.Show("Please enter a valid number for Sort Order in CAS");
                return;
            }
            if (String.CompareOrdinal(TattooName.Text, " ") <= 0)
            {
                MessageBox.Show("Please enter a name for the tattoo");
                return;
            }
            if (dds == null)
            {
                MessageBox.Show("Please select an image for the tattoo");
                return;
            }
            ulong tatInstance;
            string tatName;
            if (TattooName.Text.IndexOf("uutattoo") < 0)
            {
                tatName = "uutattoo" + TattooName.Text;
            }
            else
            {
                tatName = TattooName.Text;
            }
            if (String.CompareOrdinal(TattooInstance.Text, " ") > 0)
            {
                if (!UInt64.TryParse(TattooInstance.Text, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out tatInstance))
                {
                    MessageBox.Show("Please enter a valid Tattoo Instance number in hexidecimal, or leave it blank");
                    return;
                }
            }
            else
            {
                tatInstance = FNVhash.FNV64(tatName);
            }

            PleaseWait_label.Visible = true;
            this.Refresh();

            uint tatGroup = FNVhash.FNV24(tatName);
            ulong txtcInstance = FNVhash.FNV64(tatName + "TattooTemplate_Top_diffuse");

            Package myPack;
            myPack = (Package)Package.NewPackage(0);

            NameMapResource.NameMapResource myMap = new NameMapResource.NameMapResource(0, null);
            myMap.Add(tatInstance, tatName);
            myMap.Add(tatGroup, tatName);
            myMap.Add(txtcInstance, tatName + "TattooTemplate_Top_diffuse");

            XmlDocument doc1 = PresetBuilder.presetXML(tatName, tatInstance, presets[0],
                ChannelR_checkBox.Checked, ChannelG_checkBox.Checked, ChannelB_checkBox.Checked, ChannelA_checkBox.Checked);
            XmlDocument doc2 = PresetBuilder.presetXML(tatName, tatInstance, presets[1],
                ChannelR_checkBox.Checked, ChannelG_checkBox.Checked, ChannelB_checkBox.Checked, ChannelA_checkBox.Checked);
            XmlDocument doc3 = PresetBuilder.presetXML(tatName, tatInstance, presets[2],
                ChannelR_checkBox.Checked, ChannelG_checkBox.Checked, ChannelB_checkBox.Checked, ChannelA_checkBox.Checked);

            CASPartResource.CASPartResource myCasp;
            myCasp = new CASPartResource.CASPartResource(0, null);
            CASPartResource.CASPartResource.Preset pre1 = new CASPartResource.CASPartResource.Preset(0, null, doc1.OuterXml, 1);
            CASPartResource.CASPartResource.Preset pre2 = new CASPartResource.CASPartResource.Preset(0, null, doc2.OuterXml, 2);
            CASPartResource.CASPartResource.Preset pre3 = new CASPartResource.CASPartResource.Preset(0, null, doc3.OuterXml, 3);
            myCasp.Presets = new CASPartResource.CASPartResource.PresetList(null);
            myCasp.Presets.Add(pre1);
            myCasp.Presets.Add(pre2);
            myCasp.Presets.Add(pre3);
            myCasp.Unknown1 = tatName;
            myCasp.SortPriority = sortOrder;
            myCasp.Clothing = ClothingType.TattooTemplate;
            myCasp.DataType = DataTypeFlags.Body;
            myCasp.AgeGender = new AgeGenderFlags(0, null, (CASPtodder_checkBox.Checked ? AgeFlags.Toddler : 0) |
                (CASPchild_checkBox.Checked ? AgeFlags.Child : 0) | (CASPteen_checkBox.Checked ? AgeFlags.Teen : 0) |
                (CASPadult_checkBox.Checked ? AgeFlags.YoungAdult | AgeFlags.Adult : 0) |
                (CASPelder_checkBox.Checked ? AgeFlags.Elder : 0),
                (CASPfemale_checkBox.Checked ? GenderFlags.Female : 0) | (CASPmale_checkBox.Checked ? GenderFlags.Male : 0),
                0, 0);
            myCasp.ClothingCategory = ClothingCategoryFlags.Athletic | ClothingCategoryFlags.Career | ClothingCategoryFlags.Everyday |
                ClothingCategoryFlags.FireFighting | ClothingCategoryFlags.Formalwear | ClothingCategoryFlags.Makeover |
                ClothingCategoryFlags.MartialArts | ClothingCategoryFlags.Naked | ClothingCategoryFlags.Outerwear |
                ClothingCategoryFlags.SkinnyDippingTowel | ClothingCategoryFlags.Sleepwear | ClothingCategoryFlags.Swimwear |
                ClothingCategoryFlags.Singed | ClothingCategoryFlags.ValidForMaternity;
            myCasp.CasPart1Index = 2;
            myCasp.CasPart2Index = 2;
            myCasp.BlendInfoFatIndex = 3;
            myCasp.BlendInfoFitIndex = 4;
            myCasp.BlendInfoThinIndex = 5;
            myCasp.BlendInfoSpecialIndex = 6;
            myCasp.OverlayPriority = 2u;
            myCasp.VPXYIndexes = new ByteIndexList(null);
            myCasp.VPXYIndexes.Add(7);
            myCasp.Diffuse1Indexes = new ByteIndexList(null);
            myCasp.Diffuse1Indexes.Add(8);
            myCasp.Specular1Indexes = new ByteIndexList(null);
            myCasp.Specular1Indexes.Add(9);
            myCasp.BONDIndexes = new ByteIndexList(null);
            myCasp.BONDIndexes.AddRange(new List<byte> { 2, 2, 2, 2, 2 });
            myCasp.Unknown4 = "bare";
            myCasp.TGIBlocks = new CountedTGIBlockList(null);
            myCasp.TGIBlocks.Add(new TGIBlock(0, null, (uint)ResourceTypes.DDS, 0, tatInstance));
            myCasp.TGIBlocks.Add(new TGIBlock(0, null, (uint)ResourceTypes.XML, 0, 0xF7FC14B9EA85B390));
            myCasp.TGIBlocks.Add(new TGIBlock(0, null, 0, 0, 0));
            myCasp.TGIBlocks.Add(new TGIBlock(0, null, (uint)ResourceTypes.BBLN, 0, 0xCBE03A305F80FF50));
            myCasp.TGIBlocks.Add(new TGIBlock(0, null, (uint)ResourceTypes.BBLN, 0, 0xCBE032305F80F1F8));
            myCasp.TGIBlocks.Add(new TGIBlock(0, null, (uint)ResourceTypes.BBLN, 0, 0x540F4B31F0B42342));
            myCasp.TGIBlocks.Add(new TGIBlock(0, null, (uint)ResourceTypes.BBLN, 0, 0x82F02E48897E22B4));
            myCasp.TGIBlocks.Add(new TGIBlock(0, null, (uint)ResourceTypes.VPXY, 1, tatGroup));
            myCasp.TGIBlocks.Add(new TGIBlock(0, null, (uint)ResourceTypes.TXTC, tatGroup, txtcInstance));
            myCasp.TGIBlocks.Add(new TGIBlock(0, null, (uint)ResourceTypes.TXTC, tatGroup, 0xCBF29CE484222325));

            uint fakeType = (uint)tatInstance;
            uint fakeGroup = (uint)(tatInstance >> 32);
            ulong fakeInstance = (ulong)ResourceTypes.DDS << 32;
            TxtcResource.TxtcResource txtc = TattooTextureCompositor.TattooTxtc(new TGIBlock(0, null, fakeType, fakeGroup, fakeInstance));

            VPXY vpxy = new VPXY(0, null, 4, null, 2,
                new BoundingBox(0, null, new Vertex(0, null, -.0060f, 1.7157f, -.0060f), new Vertex(0, null, .0060f, 1.7277f, .0060f)),
                new byte[] { 0, 0, 0, 0 }, 0, 0, new TGIBlockList(null));
            GenericRCOLResource vpxyRcol = new GenericRCOLResource(0, null);
            vpxyRcol.Version = 3;
            vpxyRcol.PublicChunks = 1;
            vpxyRcol.ChunkEntries = new GenericRCOLResource.ChunkEntryList(null);
            GenericRCOLResource.ChunkEntry vpxyChunk = new GenericRCOLResource.ChunkEntry(0, null, new TGIBlock(0, null, "ITG", (uint)ResourceTypes.VPXY, 1, tatGroup), vpxy);
            vpxyRcol.ChunkEntries.Add(vpxyChunk);
            IResourceIndexEntry rs = myPack.AddResource(new TGIBlock(0, null, (uint)ResourceTypes.KEY, 0, tatInstance), myMap.Stream, true);
            rs.Compressed = (ushort)0xFFFF;
            rs = myPack.AddResource(new TGIBlock(0, null, (uint)ResourceTypes.CASP, 0, tatInstance), myCasp.Stream, true);
            rs.Compressed = (ushort)0xFFFF;
            rs = myPack.AddResource(new TGIBlock(0, null, (uint)ResourceTypes.TXTC, tatGroup, txtcInstance), txtc.Stream, true);
            rs.Compressed = (ushort)0xFFFF;
            rs = myPack.AddResource(new TGIBlock(0, null, (uint)ResourceTypes.VPXY, 1, tatGroup), vpxyRcol.Stream, true);
            rs.Compressed = (ushort)0xFFFF;
            MemoryStream ms = new MemoryStream();
            doc1.Save(ms);
            rs = myPack.AddResource(new TGIBlock(0, null, (uint)ResourceTypes.XML, 0, tatInstance), ms, true);
            rs.Compressed = (ushort)0xFFFF;
            MemoryStream ms2 = new MemoryStream();
            dds.UseDXT = false;
            dds.AlphaDepth = 8;
            dds.GenerateMipmaps = true;
            dds.Save(ms2);
            rs = myPack.AddResource(new TGIBlock(0, null, (uint)ResourceTypes.DDS, 0, tatInstance), ms2, true);
            rs.Compressed = (ushort)0xFFFF;
            if (thumbNail != null)
            {
                MemoryStream ms3 = new MemoryStream();
                thumbNail.Save(ms3, System.Drawing.Imaging.ImageFormat.Png);
                rs = myPack.AddResource(new TGIBlock(0, null, (uint)ResourceTypes.THUM, 1, tatInstance), ms3, true);
                rs.Compressed = (ushort)0xFFFF;
            }

            PleaseWait_label.Visible = false;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = PackageFilter;
            saveFileDialog1.Title = "Save as a new package";
            saveFileDialog1.FileName = TattooName.Text + ".package";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.OverwritePrompt = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                myPack.SaveAs(saveFileDialog1.FileName);
            }
            ms.Dispose();
            ms2.Dispose();
        }

        private void openTattooPackageToolStripMenuItem_Click(object sender, EventArgs e)
        { 
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = PackageFilter;
            openFileDialog1.Title = "Select Tattoo Package File";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.CheckFileExists = true;
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Package myPack;
            try
            {
                myPack = (Package)Package.OpenPackage(0, openFileDialog1.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot open package: " + openFileDialog1.FileName + " - Error is:" + System.Environment.NewLine +
                    ex.Message + System.Environment.NewLine + ex.InnerException);
                return;
            }

            Predicate<IResourceIndexEntry> isCASP = r => r.ResourceType == (uint)ResourceTypes.CASP;
            List<IResourceIndexEntry> CASPlist = myPack.FindAll(isCASP);
            if (CASPlist.Count == 0)
            {
                MessageBox.Show("No CASP files found in package!");
                Package.ClosePackage(0, (IPackage)myPack);
                return;
            }

            Predicate<IResourceIndexEntry> isKEY = r => r.ResourceType == (uint)ResourceTypes.KEY;
            IResourceIndexEntry irKey = myPack.Find(isKEY);
            NameMapResource.NameMapResource nMap = null;
            if (irKey != null)
            {
                Stream n = myPack.GetResource(irKey);
                nMap = new NameMapResource.NameMapResource(0, n);
            }

            int numTattoos = 0;
            foreach (IResourceIndexEntry r in CASPlist)
            {
                Stream s = myPack.GetResource(r);
                s.Position = 0;
                CASPartResource.CASPartResource casp = new CASPartResource.CASPartResource(0, s);
                if (casp.Clothing == ClothingType.TattooTemplate)
                {
                    numTattoos++;
                    string tatName = "";
                    if (casp.Presets.Count > 0)
                    {
                        XmlDocument imp = new XmlDocument();
                        imp.LoadXml(casp.Presets[0].XmlFile.ReadToEnd());
                        XmlNodeList nodes = imp.GetElementsByTagName("value");
                        foreach (XmlNode n in nodes)
                        {
                            if (n.Attributes["key"].InnerXml.Contains("daeFileName"))
                            {
                                tatName = n.Attributes["value"].InnerXml;
                                break;
                            }
                        }
                    }
                    string tmp;
                    if (nMap != null && nMap.TryGetValue(r.Instance, out tmp)) tatName = tmp;
                    DialogResult res = MessageBox.Show("Open tattoo: " + tatName + "?", "Select Tattoo", MessageBoxButtons.YesNoCancel);
                    if (res == DialogResult.Yes)
                    {
                        bool gotImage = false;
                        for (int i = 0; i < Math.Min(casp.Presets.Count, 3); i++)
                        {
                            XmlDocument imp = new XmlDocument();
                            imp.LoadXml(casp.Presets[i].XmlFile.ReadToEnd());
                            XmlNodeList nodes = imp.GetElementsByTagName("value");
                            foreach (XmlNode n in nodes)
                            {
                                if (n.Attributes["key"].InnerXml.Contains("Layer1ColorR"))
                                {
                                    presets[i].rChannelColor = new Color(n.Attributes["value"].InnerXml);
                                }
                                else if (n.Attributes["key"].InnerXml.Contains("Layer1ColorG"))
                                {
                                    presets[i].gChannelColor = new Color(n.Attributes["value"].InnerXml);
                                }
                                else if (n.Attributes["key"].InnerXml.Contains("Layer1ColorB"))
                                {
                                    presets[i].bChannelColor = new Color(n.Attributes["value"].InnerXml);
                                }
                                else if (n.Attributes["key"].InnerXml.Contains("Layer1ColorA"))
                                {
                                    presets[i].aChannelColor = new Color(n.Attributes["value"].InnerXml);
                                }
                                else if (n.Attributes["key"].InnerXml.Contains("Layer1Mask") && !gotImage)
                                {
                                    string[] imgTgi = n.Attributes["value"].InnerXml.Split(':');
                                    Predicate<IResourceIndexEntry> isIMG;
                                    try
                                    {
                                        isIMG = ri => ri.ResourceType == UInt32.Parse(imgTgi[1], System.Globalization.NumberStyles.HexNumber) &
                                            ri.ResourceGroup == UInt32.Parse(imgTgi[2], System.Globalization.NumberStyles.HexNumber) &
                                            ri.Instance == UInt64.Parse(imgTgi[3], System.Globalization.NumberStyles.HexNumber);
                                    }
                                    catch
                                    {
                                        MessageBox.Show("Could not parse TGI of tattoo image!");
                                        return;
                                    }
                                    IResourceIndexEntry img = myPack.Find(isIMG);
                                    if (img != null)
                                    {
                                        Stream d = myPack.GetResource(img);
                                        d.Position = 0;
                                        dds = new DdsFile();
                                        dds.Load(d, false);
                                        ddsOriginal = new DdsFile();
                                        ddsOriginal.CreateImage(dds, false);
                                        gotImage = true;
                                    }
                                }
                            }
                        }
                        if (!gotImage) MessageBox.Show("Could not find tattoo image!");
                        bgColor = new Color(0x00D2B48Cu);
                        BGcolor_panel.BackColor = System.Drawing.Color.FromArgb(bgColor.Red, bgColor.Green, bgColor.Blue);
                        currentPreset = 0;
                        PresetPrep();
                        Update_DDSdisplay();
                        TattooImageFile.Text = "";
                        Predicate<IResourceIndexEntry> isTHUM = ri => ri.ResourceType == (uint)ResourceTypes.THUM &
                                                        ri.ResourceGroup == 1U & ri.Instance == r.Instance;
                        IResourceIndexEntry irThum = myPack.Find(isTHUM);
                        if (irThum != null)
                        {
                            Stream t = myPack.GetResource(irThum);
                            thumbNail = new Bitmap(t);
                        }
                        else
                        {
                            thumbNail = null;
                        }
                        TattooName.Text = tatName;
                        TattooInstance.Text = r.Instance.ToString("X16");
                        AgeGenderFlags ag = casp.AgeGender;
                        CASPtodder_checkBox.Checked = ((ag.Age & AgeFlags.Toddler) > 0);
                        CASPchild_checkBox.Checked = ((ag.Age & AgeFlags.Child) > 0);
                        CASPteen_checkBox.Checked = ((ag.Age & AgeFlags.Teen) > 0);
                        CASPadult_checkBox.Checked = ((ag.Age & AgeFlags.Adult) > 0);
                        CASPelder_checkBox.Checked = ((ag.Age & AgeFlags.Elder) > 0);
                        CASPmale_checkBox.Checked = ((ag.Gender & GenderFlags.Male) > 0);
                        CASPfemale_checkBox.Checked = ((ag.Gender & GenderFlags.Female) > 0);
                        CASPsortOrder.Text = casp.SortPriority.ToString();
                        saveOptionsDDS = new DdsSaveOptions("DXT5", true, 90);
                        PhotoConvert_radioButton.Checked = false;
                        Black2Red_radioButton.Checked = false;
                        Resize_checkBox.Checked = false;
                        ReplaceAlpha_checkBox.Checked = false;
                        Black2Alpha_radioButton.Checked = true;
                        White2Alpha_radioButton.Checked = false;
                        BlankAlpha_radioButton.Checked = false;
                        invertAlpha_radioButton.Checked = false;
                        White2Back_checkBox.Checked = false;
                        break;
                    }
                    else if (res == DialogResult.No)
                    {
                        continue;
                    }
                    else
                    {
                        Package.ClosePackage(0, (IPackage)myPack);
                        return;
                    }

                }
            }

            if (numTattoos == 0)
            {
                MessageBox.Show("No tattoos found in package!");
            }
            Package.ClosePackage(0, (IPackage)myPack);
           // CASPartResource.CASPartResource casp = 
        }

        private void newTattooToolStripMenuItem_Click(object sender, EventArgs e)
        {
            presets = new Preset[3] { new Preset(0xFFFF0000, 0xFF00FF00, 0xFF0000FF, 0xFF000000),
                                           new Preset(0xFFCC0000, 0xFF00CC00, 0xFF0000CC, 0xFF000000),
                                           new Preset(0xFF880000, 0xFF008800, 0xFF000088, 0xFF000000)};
            currentPreset = 0;
            PresetPrep();
            CASPtodder_checkBox.Checked = false;
            CASPchild_checkBox.Checked = false;
            CASPteen_checkBox.Checked = true;
            CASPadult_checkBox.Checked = true;
            CASPelder_checkBox.Checked = true;
            CASPmale_checkBox.Checked = true;
            CASPfemale_checkBox.Checked = true;
            ChannelA_checkBox.Checked = true;
            ChannelG_checkBox.Checked = true;
            ChannelB_checkBox.Checked = true;
            ChannelA_checkBox.Checked = false;
            TattooName.Text = "";
            TattooInstance.Text = "";
            TattooImageFile.Text = "";
            CASPsortOrder.Text = "100";
            bgColor = new Color(0x00D2B48Cu);
            BGcolor_panel.BackColor = System.Drawing.Color.FromArgb(bgColor.Red, bgColor.Green, bgColor.Blue);
            dds = null;
            ddsOriginal = null;
            saveOptionsDDS = new DdsSaveOptions("DXT5", true, 90);
            thumbNail = null;
            Preview_pictureBox.Image = null;
            PhotoConvert_radioButton.Checked = false;
            Black2Red_radioButton.Checked = false;
            Resize_checkBox.Checked = false;
            ReplaceAlpha_checkBox.Checked = false;
            Black2Alpha_radioButton.Checked = true;
            White2Alpha_radioButton.Checked = false;
            BlankAlpha_radioButton.Checked = false;
            invertAlpha_radioButton.Checked = false;
            White2Back_checkBox.Checked = false;
        }

        private void ClosetoolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("Close Tattooinator?", "Close", MessageBoxButtons.OKCancel);
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                this.Close();
            }
        }

        private void Purify_button_Click(object sender, EventArgs e)
        {
            PurifyOptions_form p = new PurifyOptions_form(dds, purifyOptions);
            DialogResult res =  p.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.Cancel) return;
            purifyOptions = p.PurifyOptions;
            dds.SetPixels(AdjustColors);
            Update_DDSdisplay();
        }
    }
}
