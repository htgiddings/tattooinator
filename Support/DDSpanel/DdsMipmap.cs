//------------------------------------------------------------------------------
/*
 * 
 *  Copyright (C) 2013 by Camille Marinetti for s3pi by Peter L Jones
 *  pljones@users.sf.net
 * 
 * This is a derived work from:
 * 
 */

/*
 DDS GIMP plugin

 Copyright (C) 2004-2012 Shawn Kirst <skirst@gmail.com>,
with parts (C) 2003 Arne Reuter <homepage@arnereuter.de> where specified.

 This program is free software; you can redistribute it and/or
 modify it under the terms of the GNU General Public
 License as published by the Free Software Foundation; either
 version 2 of the License, or (at your option) any later version.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with this program; see the file COPYING.  If not, write to
 the Free Software Foundation, 51 Franklin Street, Fifth Floor
 Boston, MA 02110-1301, USA.
*/


using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Drawing
{
    internal sealed class DdsMipmap
    {
        public static uint numberMipmaps(DdsFile.DdsHeader ddsHeader)
        {
            int w = ddsHeader.m_width << 1;             //MipMap code from GIMP DDS plugin source by Shawn Kirst
            int h = ddsHeader.m_height << 1;
            int n = 0;

            while (w > 1 || h > 1)
            {
                if (w > 1) w >>= 1;
                if (h > 1) h >>= 1;
                ++n;
            }
            return (uint)n;
        }

        public static Size nextMipmapSize(DdsFile currentMipmap) 
        {
            return nextMipmapSize(currentMipmap.ddsHeader.m_width, currentMipmap.ddsHeader.m_height);
        }

        public static Size nextMipmapSize(int currentWidth, int currentHeight)
        {
            int w = Math.Max(currentWidth >> 1, 1);
            int h = Math.Max(currentHeight >> 1, 1);
            return new Size(w, h);
        }

        public static DdsFile nextMipmap(DdsFile mainTexture, int width, int height)
        {
            Size nextSize = nextMipmapSize(width, height);
            DdsFile mip = mainTexture.Resize(nextSize);
            return mip;
        }

     /*   public static DdsFile nextMipmap(DdsFile mainTexture, int width, int height)
        {
            Size nextSize = nextMipmapSize(width, height);

            Bitmap main = new Bitmap(mainTexture.Image);
            Rectangle rect1 = new Rectangle(0, 0, main.Width, main.Height);
            System.Drawing.Imaging.BitmapData bmpData1 = main.LockBits(rect1, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                main.PixelFormat);
            IntPtr ptr1;
            if (bmpData1.Stride > 0) ptr1 = bmpData1.Scan0;
            else ptr1 = bmpData1.Scan0 + bmpData1.Stride * (main.Height - 1);
            int bytes1 = Math.Abs(bmpData1.Stride) * main.Height;
            byte[] argbValues1 = new byte[bytes1];
            System.Runtime.InteropServices.Marshal.Copy(ptr1, argbValues1, 0, bytes1);

            Bitmap mip = new Bitmap(nextSize.Width, nextSize.Height);
            Rectangle rect2 = new Rectangle(0, 0, mip.Width, mip.Height);
            System.Drawing.Imaging.BitmapData bmpData2 = mip.LockBits(rect2, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                main.PixelFormat);
            IntPtr ptr2;
            if (bmpData2.Stride > 0) ptr2 = bmpData2.Scan0;
            else ptr2 = bmpData2.Scan0 + bmpData2.Stride * (mip.Height - 1);
            int bytes2 = Math.Abs(bmpData2.Stride) * mip.Height;
            byte[] argbValues2 = new byte[bytes2];
            System.Runtime.InteropServices.Marshal.Copy(ptr2, argbValues2, 0, bytes2);

            for (int l = 0; l < main.Height; l += 2)
            {
                for (int c = 0; c < main.Width; c += 2)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int ind = (l * bmpData1.Stride) + (c * 8) + i;      //index of upper left pixel in block of four
                        byte pixel = (byte)(((argbValues1[ind] + argbValues1[ind + 4] + argbValues1[ind + bmpData1.Stride] + argbValues1[ind + bmpData1.Stride + 4]) / 4f) + 0.5f);
                        int ind2 = ((l / 2) * bmpData2.Stride) + (c / 2);
                        argbValues2[ind2] = pixel;
                    }
                }
            }
            main.UnlockBits(bmpData1);
            System.Runtime.InteropServices.Marshal.Copy(argbValues2, 0, ptr2, bytes2);
            main.UnlockBits(bmpData2);

            DdsFile dds = new DdsFile();
            dds.CreateImage(mip, false);
            return dds;
        } */
    }
}
