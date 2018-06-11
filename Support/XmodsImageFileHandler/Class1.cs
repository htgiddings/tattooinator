using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Xmods.ImageFileHandler
{
    ///<summary>
    ///Handler class for DdsFile and Image
    ///</summary>
    public class ImageHandler
    {
        ///<summary>
        ///Opens an image file (dds, jpeg/jpg, gif, png, bmp, tif) and returns a DdsFile
        ///</summary>
        public static DdsFile OpenDDS(string fileString)
        {
            if (String.CompareOrdinal(fileString, " ") <= 0 || !File.Exists(fileString))
            {
                throw new ApplicationException("File '" + fileString + "'is not a valid filename");
            }

            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream();
                using (FileStream fs = File.OpenRead(fileString))
                {
                    ms.SetLength(fs.Length);
                    fs.Read(ms.GetBuffer(), 0, (int)fs.Length);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error: Could not read file " + fileString + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
            ms.Position = 0;

            DdsFile dds = new DdsFile();
            if (String.CompareOrdinal(Path.GetExtension(fileString).ToUpperInvariant(), ".DDS") == 0)
            {
                dds.Load(ms, false);
            }
            else
            {
                Bitmap img = (Bitmap)Bitmap.FromStream(ms, true);
                dds.CreateImage(img, true);
                if (!Image.IsAlphaPixelFormat(img.PixelFormat))
                {
                    dds.UseDXT = false;
                    dds.AlphaDepth = 0;
                }
            }

            return dds;
        }

        ///<summary>
        ///Opens an image file ((dds, jpeg/jpg, gif, png, bmp, tif) and returns a Bitmap image
        ///</summary>
        public static Bitmap OpenImage(string fileString)
        {
            if (String.CompareOrdinal(fileString, " ") <= 0 || !File.Exists(fileString))
            {
                throw new ApplicationException("File '" + fileString + "'is not a valid filename");
            }

            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream();
                using (FileStream fs = File.OpenRead(fileString))
                {
                    ms.SetLength(fs.Length);
                    fs.Read(ms.GetBuffer(), 0, (int)fs.Length);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error: Could not read file " + fileString + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
            }
            ms.Position = 0;

            Bitmap img = (Bitmap)Bitmap.FromStream(ms, true);
            return img;
        }

        ///<summary>
        ///Saves a DdsFile to dds, jpeg/jpg, gif, png, bmp, or tif, depanding on the filename extension
        ///</summary>
        public static DdsSaveOptions SaveImage(string fileString, DdsFile dds, DdsSaveOptions saveOptions, int jpgQualityPercent = 90)
        {
            if (String.CompareOrdinal(Path.GetExtension(fileString).ToUpperInvariant(), ".DDS") == 0 ||
                     !Path.HasExtension(fileString))
            {
                DDSoptions f = new DDSoptions(saveOptions);
                DialogResult r = f.ShowDialog();
                if (r == DialogResult.Cancel) return saveOptions;
                DdsFile ddsSave = new DdsFile();
                ddsSave.CreateImage(dds, false);

                if (String.CompareOrdinal(f.DDScompression, "None") == 0)
                {
                    ddsSave.UseDXT = false;
                }
                else if (String.CompareOrdinal(f.DDScompression, "DXT1") == 0)
                {
                    ddsSave.UseDXT = true;
                    ddsSave.AlphaDepth = 1;
                }
                else if (String.CompareOrdinal(f.DDScompression, "DXT3") == 0)
                {
                    ddsSave.UseDXT = true;
                    ddsSave.AlphaDepth = 3;
                }
                else
                {
                    ddsSave.UseDXT = true;
                    ddsSave.AlphaDepth = 5;
                }
                ddsSave.GenerateMipmaps = f.DDSmipmaps;

                using (FileStream myStream = new FileStream(fileString, FileMode.Create, FileAccess.Write))
                {
                    ddsSave.Save(myStream);
                    myStream.Close();
                }
                return new DdsSaveOptions(f.DDScompression, f.DDSmipmaps, saveOptions.jpgQualityPercent);
            }
            else
            {
                int jpgQuality = 
                    SaveImage(fileString, dds.GetImage(true, true, true, true, false), saveOptions.jpgQualityPercent);
                return new DdsSaveOptions(saveOptions.ddsCompressFormat, saveOptions.generateMipmaps, jpgQuality);
            }
        }

        ///<summary>
        ///Saves a Bitmap image to jpeg/jpg, gif, png, bmp, or tif, depanding on the filename extension
        ///</summary>
        public static int SaveImage(string fileString, Bitmap img, int jpgQualityPercent)
        {
            if (String.CompareOrdinal(Path.GetExtension(fileString).ToUpperInvariant(), ".JPG") == 0 ||
                     String.CompareOrdinal(Path.GetExtension(fileString).ToUpperInvariant(), ".JPEG") == 0)
            {
                JPGoptions f = new JPGoptions(jpgQualityPercent);
                DialogResult r = f.ShowDialog();
                if (r == DialogResult.Cancel) return jpgQualityPercent;

                ImageCodecInfo inf = GetEncoderInfo(ImageFormat.Jpeg);
                System.Drawing.Imaging.Encoder encode = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters encodeParams = new EncoderParameters(1);
                EncoderParameter encodeParam = new EncoderParameter(encode, f.jpgQualitySelected);
                encodeParams.Param[0] = encodeParam;

                using (FileStream myStream = new FileStream(fileString, FileMode.Create, FileAccess.Write))
                {
                    img.Save(myStream, inf, encodeParams);
                    myStream.Close();
                }
                return f.jpgQualitySelected;
            }
            else
            {
                ImageFormat imgFormat;
                if (String.CompareOrdinal(Path.GetExtension(fileString).ToUpperInvariant(), ".PNG") == 0 |
                                !Path.HasExtension(fileString))
                {
                    imgFormat = ImageFormat.Png;
                }
                else if (String.CompareOrdinal(Path.GetExtension(fileString).ToUpperInvariant(), ".GIF") == 0)
                {
                    imgFormat = ImageFormat.Gif;
                }
                else if (String.CompareOrdinal(Path.GetExtension(fileString).ToUpperInvariant(), ".BMP") == 0)
                {
                    imgFormat = ImageFormat.Bmp;
                }
                else if ((String.CompareOrdinal(Path.GetExtension(fileString).ToUpperInvariant(), ".TIF") == 0) |
                         (String.CompareOrdinal(Path.GetExtension(fileString).ToUpperInvariant(), ".TIFF") == 0))
                {
                    imgFormat = ImageFormat.Tiff;
                }
                else
                {
                    throw new ApplicationException("Not a recognized image file extension!");
                }

                using (FileStream myStream = new FileStream(fileString, FileMode.Create, FileAccess.Write))
                {
                    img.Save(myStream, imgFormat);
                    myStream.Close();
                }
                return jpgQualityPercent;
            }
        }

        private static ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in encoders)
            {
                if (codec.FormatID == format.Guid) return codec;
            }
            return null;
        }
    }

    ///<summary>
    ///Default save options for a dds image
    ///</summary>
    public class DdsSaveOptions
    {
        private string ddsCompression;
        private bool mipmaps;
        private int jpgCompression;
        ///<summary>
        ///Compression format for dds: None, DXT1, DXT3, or DXT5
        ///</summary>
        public string ddsCompressFormat { get { return ddsCompression; } }
        ///<summary>
        ///Whether to generate mipmaps when saving to file
        ///</summary>
        public bool generateMipmaps { get { return mipmaps; } }
        ///<summary>
        ///Whether to generate mipmaps when saving to file
        ///</summary>
        public int jpgQualityPercent { get { return jpgCompression; } }
        ///<summary>
        ///Class defining save options for dds images
        ///</summary>
        ///<param name="ddsCompressFormat">Compression format string must be either: None, DXT1, DXT3, or DXT5.</param>
        ///<param name="generateMipmaps">Generate mipmaps on save: true or false.</param>
        public DdsSaveOptions(string ddsCompressFormat, bool generateMipmaps)
        {
            ddsCompression = ddsCompressFormat;
            mipmaps = generateMipmaps;
            jpgCompression = 90;
        }
        ///<summary>
        ///Class defining save options for dds images
        ///</summary>
        ///<param name="ddsCompressFormat">Compression format string must be either: None, DXT1, DXT3, or DXT5.</param>
        ///<param name="generateMipmaps">Generate mipmaps on save: true or false.</param>
        ///<param name="jpgQualityPercent">jpg quality percent must be 0 - 100.</param>
        public DdsSaveOptions(string ddsCompressFormat, bool generateMipmaps, int jpgQualityPercent)
        {
            ddsCompression = ddsCompressFormat;
            mipmaps = generateMipmaps;
            jpgCompression = jpgQualityPercent;
        }

    }

}
