//------------------------------------------------------------------------------
/*
	@brief		DDS File Type Plugin for Paint.NET

	@note		Copyright (c) 2006 Dean Ashton         http://www.dmashton.co.uk

	Permission is hereby granted, free of charge, to any person obtaining
	a copy of this software and associated documentation files (the 
	"Software"), to	deal in the Software without restriction, including
	without limitation the rights to use, copy, modify, merge, publish,
	distribute, sublicense, and/or sell copies of the Software, and to 
	permit persons to whom the Software is furnished to do so, subject to 
	the following conditions:

	The above copyright notice and this permission notice shall be included
	in all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
	OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
	MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
	IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY 
	CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
	TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
	SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/
/***************************************************************************
 *  This is pretty much a complete rewrite of the DDS File Type Plugin     *
 *  for Paint.Net distributed as part of sims3tools.  All the bugs are     *
 *  mine.                                                                  *
 *                                                                         *
 *  Copyright (C) 2012 by Peter L Jones                                    *
 *  pljones@users.sf.net                                                   *
 ***************************************************************************/
/* Minor changes by Camille Marinetti to support programmatic control of 
 * compression and generation of mipmaps. March 2014
 **/
//------------------------------------------------------------------------------

// If we want to do the alignment as per the (broken) DDS documentation, then we
// uncomment this define.. 
//#define	APPLY_PITCH_ALIGNMENT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using PaintDotNet;
using System.Drawing;

namespace System.Drawing
{
    /// <summary>
    /// Represents an image encoded using one of the supported DDS mechanisms.
    /// </summary>
    /// <remarks>
    /// A &quot;DirectX Draw Surface&quot; stores compressed pixel data that is used when
    /// rendering scenes.  The pixel data may be used for purposes other than purely display,
    /// such as being used for masked operations on another DDS image.
    /// <para>Note that this assembly depends on two unmanaged libraries:
    /// <br/>squishinterface_Win32.dll - code for 32bit Windows systems.
    /// <br/>squishinterface_x64.dll - code for 64bit Windows systems.
    /// </para>
    /// </remarks>
    public class DdsFile : IDisposable
    {
        // Used to identify the resource as a DDS
        const uint fourccDDS_ = 0x20534444;//'DDS '

        /// <summary>
        /// Header of the DDS image.
        /// </summary>
        public DdsHeader ddsHeader { get; private set; }

        // ARGB data as extracted from the DDS resource
        uint[] baseImage = null;
        internal uint[] currentImage = null;

        // HSVa data
        ColorHSVA[] hsvData = null;

        // The hsvShift applied to create hsvData
        HSVShift hsvShift;


        #region DDS header
        /// <summary>
        /// DXT1 tag
        /// </summary>
        public const uint fourccDXT1 = 0x31545844;
        /// <summary>
        /// DXT3 tag
        /// </summary>
        public const uint fourccDXT3 = 0x33545844;
        /// <summary>
        /// DXT5 tag
        /// </summary>
        public const uint fourccDXT5 = 0x35545844;

        /// <summary>
        /// Describes the DDS file format
        /// </summary>
        public enum DdsFileFormat
        {
            /// <summary>
            /// DXT1
            /// </summary>
            DDS_FORMAT_DXT1,
            /// <summary>
            /// DXT3
            /// </summary>
            DDS_FORMAT_DXT3,
            /// <summary>
            /// DXT5
            /// </summary>
            DDS_FORMAT_DXT5,
            /// <summary>
            /// 32bit A8R8G8B8
            /// </summary>
            DDS_FORMAT_A8R8G8B8,
            /// <summary>
            /// 32bit, top 8 ignored X8R8G8B8
            /// </summary>
            DDS_FORMAT_X8R8G8B8,
            /// <summary>
            /// 32bit A8B8G8R8
            /// </summary>
            DDS_FORMAT_A8B8G8R8,
            /// <summary>
            /// 32bit, top 8 ignored X8B8G8R8
            /// </summary>
            DDS_FORMAT_X8B8G8R8,
            /// <summary>
            /// 16bit A1R5G5B5
            /// </summary>
            DDS_FORMAT_A1R5G5B5,
            /// <summary>
            /// 16bit A4R4G4B4
            /// </summary>
            DDS_FORMAT_A4R4G4B4,
            /// <summary>
            /// 24bit R8G8B8
            /// </summary>
            DDS_FORMAT_R8G8B8,
            /// <summary>
            /// 16bit R5G6B5
            /// </summary>
            DDS_FORMAT_R5G6B5,

            /// <summary>
            /// 16bit Alpha/Luminance
            /// </summary>
            DDS_FORMAT_A8L8,
        }

        /// <summary>
        /// Pixel format
        /// </summary>
        public class DdsPixelFormat
        {
            /// <summary>
            /// Pixel format flags
            /// </summary>
            public enum PixelFormatFlags : uint
            {
                /// <summary>
                /// Format supports alpha bit(s)
                /// </summary>
                DDS_ALPHAPIXELS = 0x00000001,
                /// <summary>
                /// Format supports alpha value
                /// </summary>
                DDS_ALPHA = 0x00000002,
                /// <summary>
                /// Format supports FOURCC DXT identifier
                /// </summary>
                DDS_FOURCC = 0x00000004,
                /// <summary>
                /// Format supports RGB values
                /// </summary>
                DDS_RGB = 0x00000040,
                /// <summary>
                /// Format supports YUV values
                /// </summary>
                DDS_YUV = 0x00000200,
                /// <summary>
                /// Format supports luminance value
                /// </summary>
                DDS_LUMINANCE = 0x00020000,
            }

            internal uint m_size;
            /// <summary>
            /// Flags
            /// </summary>
            public PixelFormatFlags m_flags { get; private set; }
            /// <summary>
            /// Four CC
            /// </summary>
            public uint m_fourCC { get; private set; }
            /// <summary>
            /// Number of bits
            /// </summary>
            public int m_rgbBitCount { get; private set; }
            /// <summary>
            /// Location in the pixel of the red channel bits
            /// </summary>
            public uint m_rBitMask { get; private set; }
            /// <summary>
            /// Location in the pixel of the green channel bits
            /// </summary>
            public uint m_gBitMask { get; private set; }
            /// <summary>
            /// Location in the pixel of the blue channel bits
            /// </summary>
            public uint m_bBitMask { get; private set; }
            /// <summary>
            /// Location in the pixel of the alpha channel bits
            /// </summary>
            public uint m_aBitMask { get; private set; }

            uint Size()
            {
                return 8 * 4;
            }

            internal DdsPixelFormat(DdsFileFormat fileFormat)
            {
                m_size = Size();
                switch (fileFormat)
                {
                    case DdsFileFormat.DDS_FORMAT_DXT1:
                    case DdsFileFormat.DDS_FORMAT_DXT3:
                    case DdsFileFormat.DDS_FORMAT_DXT5:
                        // DXT1/DXT3/DXT5
                        m_flags = PixelFormatFlags.DDS_FOURCC;
                        switch (fileFormat)
                        {
                            case DdsFileFormat.DDS_FORMAT_DXT1: m_fourCC = fourccDXT1; break;
                            case DdsFileFormat.DDS_FORMAT_DXT3: m_fourCC = fourccDXT3; break;
                            case DdsFileFormat.DDS_FORMAT_DXT5: m_fourCC = fourccDXT5; break;
                        }
                        m_rgbBitCount = 0;
                        m_rBitMask = 0;
                        m_gBitMask = 0;
                        m_bBitMask = 0;
                        m_aBitMask = 0;
                        break;

                    case DdsFileFormat.DDS_FORMAT_A8R8G8B8:
                        m_flags = PixelFormatFlags.DDS_RGB | PixelFormatFlags.DDS_ALPHAPIXELS;      //CGM
                        m_fourCC = 0;
                        m_rgbBitCount = 32;
                        m_rBitMask = 0x00ff0000;
                        m_gBitMask = 0x0000ff00;
                        m_bBitMask = 0x000000ff;
                        m_aBitMask = 0xff000000;
                        break;

                    case DdsFileFormat.DDS_FORMAT_X8R8G8B8:
                        m_flags = PixelFormatFlags.DDS_RGB;
                        m_fourCC = 0;
                        m_rgbBitCount = 32;
                        m_rBitMask = 0x00ff0000;
                        m_gBitMask = 0x0000ff00;
                        m_bBitMask = 0x000000ff;
                        m_aBitMask = 0x00000000;
                        break;

                    case DdsFileFormat.DDS_FORMAT_A8B8G8R8:
                        m_flags = PixelFormatFlags.DDS_RGB | PixelFormatFlags.DDS_ALPHAPIXELS;      //CGM
                        m_fourCC = 0;
                        m_rgbBitCount = 32;
                        m_rBitMask = 0x000000ff;
                        m_gBitMask = 0x0000ff00;
                        m_bBitMask = 0x00ff0000;
                        m_aBitMask = 0xff000000;
                        break;
 
                    case DdsFileFormat.DDS_FORMAT_X8B8G8R8:
                        m_flags = PixelFormatFlags.DDS_RGB;
                        m_fourCC = 0;
                        m_rgbBitCount = 32;
                        m_rBitMask = 0x000000ff;
                        m_gBitMask = 0x0000ff00;
                        m_bBitMask = 0x00ff0000;
                        m_aBitMask = 0x00000000;
                        break;

                    case DdsFileFormat.DDS_FORMAT_R8G8B8:
                        m_flags = PixelFormatFlags.DDS_RGB;
                        m_fourCC = 0;
                        m_rgbBitCount = 24;
                        m_rBitMask = 0x00ff0000;
                        m_gBitMask = 0x0000ff00;
                        m_bBitMask = 0x000000ff;
                        m_aBitMask = 0x00000000;
                        break;

                    case DdsFileFormat.DDS_FORMAT_A1R5G5B5:
                        m_flags = PixelFormatFlags.DDS_RGB | PixelFormatFlags.DDS_ALPHAPIXELS;  //CGM
                        m_fourCC = 0;
                        m_rgbBitCount = 16;
                        m_rBitMask = 0x00007c00;
                        m_gBitMask = 0x000003e0;
                        m_bBitMask = 0x0000001f;
                        m_aBitMask = 0x00008000;
                        break;

                    case DdsFileFormat.DDS_FORMAT_A4R4G4B4:
                        m_flags = PixelFormatFlags.DDS_RGB | PixelFormatFlags.DDS_ALPHAPIXELS;  //CGM
                        m_fourCC = 0;
                        m_rgbBitCount = 16;
                        m_rBitMask = 0x00000f00;
                        m_gBitMask = 0x000000f0;
                        m_bBitMask = 0x0000000f;
                        m_aBitMask = 0x0000f000;
                        break;

                    case DdsFileFormat.DDS_FORMAT_R5G6B5:
                        m_flags = PixelFormatFlags.DDS_RGB;
                        m_fourCC = 0;
                        m_rgbBitCount = 16;
                        m_rBitMask = 0x0000f800;
                        m_gBitMask = 0x000007e0;
                        m_bBitMask = 0x0000001f;
                        m_aBitMask = 0x00000000;
                        break;

                    case DdsFileFormat.DDS_FORMAT_A8L8:
                        m_flags = PixelFormatFlags.DDS_LUMINANCE | PixelFormatFlags.DDS_ALPHAPIXELS;
                        m_fourCC = 0;
                        m_rgbBitCount = 16;
                        m_rBitMask = 0x000000ff;
                        m_gBitMask = 0x00000000;
                        m_bBitMask = 0x00000000;
                        m_aBitMask = 0x0000ff00;
                        break;

                    default:
                        break;
                }
            }

            internal DdsPixelFormat(DdsPixelFormat basis)
            {
                m_size = basis.m_size;
                m_flags = basis.m_flags;
                m_fourCC = basis.m_fourCC;
                m_rgbBitCount = basis.m_rgbBitCount;
                m_rBitMask = basis.m_rBitMask;
                m_gBitMask = basis.m_gBitMask;
                m_bBitMask = basis.m_bBitMask;
                m_aBitMask = basis.m_aBitMask;
            }

            internal DdsPixelFormat(BinaryReader input)
            {
                this.m_size = input.ReadUInt32();
                this.m_flags = (PixelFormatFlags)input.ReadUInt32();
                this.m_fourCC = input.ReadUInt32();
                this.m_rgbBitCount = input.ReadInt32();
                this.m_rBitMask = input.ReadUInt32();
                this.m_gBitMask = input.ReadUInt32();
                this.m_bBitMask = input.ReadUInt32();
                this.m_aBitMask = input.ReadUInt32();

                if ((m_rgbBitCount & 0xFF) / 8 > 4)
                    throw new FormatException("Pixel size must be four bytes or less.");
            }

            internal void Write(BinaryWriter output)
            {
                output.Write(this.m_size);
                output.Write((uint)this.m_flags);
                output.Write(this.m_fourCC);
                output.Write(this.m_rgbBitCount);
                output.Write(this.m_rBitMask);
                output.Write(this.m_gBitMask);
                output.Write(this.m_bBitMask);
                output.Write(this.m_aBitMask);
            }

            [Obsolete]
            internal bool IsFourChannel
            {
                get
                {
                    if ((m_flags & DdsPixelFormat.PixelFormatFlags.DDS_FOURCC) != 0) return true;
                    if ((m_flags & (DdsPixelFormat.PixelFormatFlags.DDS_RGB | DdsPixelFormat.PixelFormatFlags.DDS_ALPHAPIXELS | DdsPixelFormat.PixelFormatFlags.DDS_ALPHA)) != 0
                        && m_aBitMask != 0) return true;
                    return false;
                }
            }

            internal int AlphaDepth
            {
                get
                {
                    if ((m_flags & DdsPixelFormat.PixelFormatFlags.DDS_FOURCC) != 0) return m_fourCC == fourccDXT1 ? 1 : m_fourCC == fourccDXT3 ? 3 : 5;

                    int i = 0;
                    for (uint j = m_aBitMask; j != 0; j >>= 1) if ((j & 1) != 0) i++;
                    return i;
                }

                set
                {
                    // Did anything change?
                    if (value == AlphaDepth) return;

                    // Are we DXTn format?
                    if ((m_flags & DdsPixelFormat.PixelFormatFlags.DDS_FOURCC) != 0)
                    {
                        // "AlphaDepth" is abused for selecting format
                        if (value == 1) this.m_fourCC = fourccDXT1;
                        else if (value == 3) this.m_fourCC = fourccDXT3;
                        else if (value == 5) this.m_fourCC = fourccDXT5;
                        else
                            throw new ArgumentException(String.Format("Invalid value {0} for AlphaDepth for DXTn image.  Please use '1', '3' or '5'.", value));
                        return;
                    }

                    // Setting to zero is pretty easy...
                    if (value == 0)
                    {
                        this.m_aBitMask = 0;
                        this.m_flags &= ~PixelFormatFlags.DDS_ALPHA;
                        this.m_flags &= ~PixelFormatFlags.DDS_ALPHAPIXELS;

                        if ((this.m_flags & PixelFormatFlags.DDS_RGB) != 0 && (this.m_flags & PixelFormatFlags.DDS_LUMINANCE) == 0)
                        {
                            if (this.m_rgbBitCount == 32 && this.m_bBitMask == 0x000000ff)
                            {
                                // We can drop 32bit to 24bit?
                                this.m_rgbBitCount = 24;
                            }
                            else if (this.m_rgbBitCount == 16)
                            {
                                // We can expand bits per pixel
                                this.m_rBitMask = 0x0000f800;
                                this.m_gBitMask = 0x000007e0;
                                this.m_bBitMask = 0x0000001f;
                            }
                        }
                        else
                        {
                            // If it is DDS_LUMINANCE, we're slightly worried by value == 0 as it's not currently supported...
                            // (If it's something else, we're really, really worried...)
                            throw new ArgumentException("AlphaDepth of zero not supported.");
                        }
                    }

                    else // non-zero case
                    {
                        // RGB, not Luminance
                        if ((this.m_flags & PixelFormatFlags.DDS_RGB) != 0 && (this.m_flags & PixelFormatFlags.DDS_LUMINANCE) == 0)
                        {
                            this.m_flags |= PixelFormatFlags.DDS_ALPHAPIXELS;       //CGM
                            // Select an appropriate bitmask
                            switch (value)
                            {
                                case 1:
                                    this.m_rgbBitCount = 16;
                                    m_rBitMask = 0x00007c00;
                                    m_gBitMask = 0x000003e0;
                                    m_bBitMask = 0x0000001f;
                                    m_aBitMask = 0x00008000;
                                    break;
                                case 4:
                                    this.m_rgbBitCount = 16;
                                    m_rBitMask = 0x00000f00;
                                    m_gBitMask = 0x000000f0;
                                    m_bBitMask = 0x0000000f;
                                    m_aBitMask = 0x0000f000;
                                    break;
                                case 8:
                                    // Only change the encoding order for ex-16bpp
                                    if (m_rgbBitCount == 16)
                                    {
                                        m_rBitMask = 0x000000ff;
                                        m_gBitMask = 0x0000ff00;
                                        m_bBitMask = 0x00ff0000;
                                    }
                                    this.m_rgbBitCount = 32;
                                    m_aBitMask = 0xff000000;
                                    break;
                                default:
                                    throw new ArgumentException(String.Format("Invalid value {0} for AlphaDepth.  Only 0, 1, 4 or 8 allowed.", value));
                            }
                        }
                        else // Luminance...  This should never actually happen, as we only support this format.
                        {
                            if (value == 8 && m_rgbBitCount == 16)
                            {
                                this.m_flags |= PixelFormatFlags.DDS_ALPHAPIXELS;
                                this.m_rgbBitCount = 16;
                                m_rBitMask = 0x000000ff;
                                m_gBitMask = 0x00000000;
                                m_bBitMask = 0x00000000;
                                m_aBitMask = 0x0000ff00;
                            }
                            else
                            {
                                throw new ArgumentException(String.Format("Invalid value {0} for AlphaDepth.  Only 8 allowed.", value));
                            }
                        }
                    }

                }
            }

            internal int PixelSize { get { return m_rgbBitCount / 8; } }

            internal DdsFileFormat fileFormatFromPixelFormat
            {
                get
                {
                    // Is it DXTn?
                    if ((m_flags & DdsPixelFormat.PixelFormatFlags.DDS_FOURCC) != 0)
                    {
                        switch (m_fourCC)
                        {
                            case fourccDXT1: return DdsFileFormat.DDS_FORMAT_DXT1;
                            case fourccDXT3: return DdsFileFormat.DDS_FORMAT_DXT3;
                            case fourccDXT5: return DdsFileFormat.DDS_FORMAT_DXT5;
                            default:
                                throw new InvalidOperationException(String.Format("Unsupported DDS FOURCC value 0x{0:X8}.", m_fourCC));
                        }
                    }

                    // Is it a Luminance map?
                    if ((m_flags & PixelFormatFlags.DDS_LUMINANCE) != 0)
                    {
                        if (m_rgbBitCount == 16)
                        {
                            if ((m_flags & PixelFormatFlags.DDS_ALPHAPIXELS) != 0 &&
                                (m_rBitMask == 0x000000ff) && (m_gBitMask == 0x00000000) && (m_bBitMask == 0x00000000) && (m_aBitMask == 0x0000ff00))
                            {
                                return DdsFileFormat.DDS_FORMAT_A8L8;
                            }
                        }
                    }

                    // No... so use the bit depths and masks
                    else
                    {
                        if (m_rgbBitCount == 32)
                        {
                            if ((m_rBitMask == 0x00ff0000) && (m_gBitMask == 0x0000ff00) && (m_bBitMask == 0x000000ff))
                            {
                                return m_aBitMask == 0xff000000 ? DdsFileFormat.DDS_FORMAT_A8R8G8B8 : DdsFileFormat.DDS_FORMAT_X8R8G8B8;
                            }
                            else if ((m_rBitMask == 0x000000ff) && (m_gBitMask == 0x0000ff00) && (m_bBitMask == 0x00ff0000))
                            {
                                return m_aBitMask == 0xff000000 ? DdsFileFormat.DDS_FORMAT_A8B8G8R8 : DdsFileFormat.DDS_FORMAT_X8B8G8R8;
                            }
                        }
                        else if (m_rgbBitCount == 24)
                        {
                            if ((m_rBitMask == 0x00ff0000) && (m_gBitMask == 0x0000ff00) && (m_bBitMask == 0x000000ff) && (m_aBitMask == 0x00000000))
                            {
                                return DdsFileFormat.DDS_FORMAT_R8G8B8;
                            }
                        }
                        else if (m_rgbBitCount == 16)
                        {
                            if ((m_rBitMask == 0x00000f00) && (m_gBitMask == 0x000000f0) && (m_bBitMask == 0x0000000f) && (m_aBitMask == 0x0000f000))
                            {
                                return DdsFileFormat.DDS_FORMAT_A4R4G4B4;
                            }
                            else if ((m_rBitMask == 0x00007c00) && (m_gBitMask == 0x000003e0) && (m_bBitMask == 0x0000001f) && (m_aBitMask == 0x00008000))
                            {
                                return DdsFileFormat.DDS_FORMAT_A1R5G5B5;
                            }
                            else if (
                                (m_rBitMask == 0x0000f800) && (m_gBitMask == 0x000007e0) && (m_bBitMask == 0x0000001f) && (m_aBitMask == 0x00000000))
                            {
                                return DdsFileFormat.DDS_FORMAT_R5G6B5;
                            }
                        }
                    }

                    // Oh dear...
                    throw new NotImplementedException("Unsupported DDS format (this should never happen).");
                }
            }

            //Mipmap public 

            internal void codecFromPixelFormat(out ToARGB toARGB, out ToPixel toPixel)
            {
                toARGB = null;
                toPixel = null;

                // If we should use DXTn, throw back no codec
                if ((m_flags & DdsPixelFormat.PixelFormatFlags.DDS_FOURCC) != 0) return;

                // No support for alpha-only images
                if ((m_flags & (DdsPixelFormat.PixelFormatFlags.DDS_RGB | DdsPixelFormat.PixelFormatFlags.DDS_LUMINANCE)) == 0) return;

                try
                {
                    switch (fileFormatFromPixelFormat)
                    {
                        case DdsFileFormat.DDS_FORMAT_A8R8G8B8: toARGB = fromDDS_A8R8G8B8; toPixel = toDDS_A8R8G8B8; break;
                        case DdsFileFormat.DDS_FORMAT_A8B8G8R8: toARGB = fromDDS_A8B8G8R8; toPixel = toDDS_A8B8G8R8; break;
                        case DdsFileFormat.DDS_FORMAT_A1R5G5B5: toARGB = fromDDS_A1R5G5B5; toPixel = toDDS_A1R5G5B5; break;
                        case DdsFileFormat.DDS_FORMAT_A4R4G4B4: toARGB = fromDDS_A4R4G4B4; toPixel = toDDS_A4R4G4B4; break;
                        case DdsFileFormat.DDS_FORMAT_X8R8G8B8: toARGB = fromDDS_X8R8G8B8; toPixel = toDDS_X8R8G8B8; break;
                        case DdsFileFormat.DDS_FORMAT_X8B8G8R8: toARGB = fromDDS_X8B8G8R8; toPixel = toDDS_X8B8G8R8; break;
                        case DdsFileFormat.DDS_FORMAT_R8G8B8: toARGB = fromDDS_R8G8B8; toPixel = toDDS_R8G8B8; break;
                        case DdsFileFormat.DDS_FORMAT_R5G6B5: toARGB = fromDDS_R5G6B5; toPixel = toDDS_R5G6B5; break;
                        case DdsFileFormat.DDS_FORMAT_A8L8: toARGB = fromDDS_A8L8; toPixel = toDDS_A8L8; break;
                    }
                }
                catch (NotImplementedException) { return; }// Any other unsupported types, just let the caller deal with the problem
            }

            #region Supported non-DXT1/3/5 conversion methods - decompress
            static uint fromDDS_A8R8G8B8(uint pixelColour) { return pixelColour; }
            static uint fromDDS_X8R8G8B8(uint pixelColour) { return 0xFF000000 | pixelColour; }
            internal static uint fromDDS_A8B8G8R8(uint pixelColour) { return (pixelColour & 0xFF00FF00) | (pixelColour & 0x00FF0000) >> 16 | (pixelColour & 0x000000FF) << 16; }
            static uint fromDDS_X8B8G8R8(uint pixelColour) { return 0xFF000000 | (pixelColour & 0x00FF0000) >> 16 | (pixelColour & 0x000000FF) << 16; }
            static uint fromDDS_A1R5G5B5(uint pixelColour)
            {
                uint A = (pixelColour >> 15) & 0xff;
                uint R = (pixelColour >> 10) & 0x1f;
                uint G = (pixelColour >> 5) & 0x1f;
                uint B = (pixelColour >> 0) & 0x1f;

                return (A << 24) | ((R << 3) | (R >> 2) << 16) | ((G << 3) | (G >> 2) << 8) | ((B << 3) | (B >> 2));
            }
            static uint fromDDS_A4R4G4B4(uint pixelColour)
            {
                uint A = (pixelColour >> 12) & 0x0f;
                uint R = (pixelColour >> 8) & 0x0f;
                uint G = (pixelColour >> 4) & 0x0f;
                uint B = (pixelColour >> 0) & 0x0f;

                return ((A << 4) | A) << 24 | ((R << 4) | R) << 16 | ((G << 4) | G) << 8 | ((B << 4) | B);
            }
            static uint fromDDS_R8G8B8(uint pixelColour) { return 0xFF000000 | pixelColour; }
            static uint fromDDS_R5G6B5(uint pixelColour)
            {
                uint R = (pixelColour >> 11) & 0x1f;
                uint G = (pixelColour >> 5) & 0x3f;
                uint B = (pixelColour >> 0) & 0x1f;

                return ((uint)0xff << 24) | ((R << 3) | (R >> 2)) << 16 | ((G << 2) | (G >> 4)) << 8 | ((B << 3) | (B >> 2));
            }
            static uint fromDDS_A8L8(uint pixelColour)
            {
                uint A = (pixelColour & 0x0000FF00) >> 8;
                uint L = pixelColour & 0x000000FF;

                return A << 24 | L << 16 | L << 8 | L;
            }
            #endregion

            #region Supported non-DXT1/3/5 conversion methods - compress
            static uint toDDS_A8R8G8B8(uint argb) { return argb; }
            static uint toDDS_X8R8G8B8(uint argb) { return argb & 0x00FFFFFF; }
            internal static uint toDDS_A8B8G8R8(uint argb) { return (argb & 0xFF00FF00) | (argb & 0x00FF0000) >> 16 | (argb & 0x000000FF) << 16; }
            static uint toDDS_X8B8G8R8(uint argb) { return (argb & 0x0000FF00) | (argb & 0x00FF0000) >> 16 | (argb & 0x000000FF) << 16; }
            static uint toDDS_A1R5G5B5(uint argb)
            {
                uint A = (uint)((argb & 0xFF000000) == 0 ? 0 : 1) << 15;
                uint R = ((argb & 0x00F80000) >> 3) >> 6;
                uint G = ((argb & 0x0000F800) >> 3) >> 3;
                uint B = ((argb & 0x000000F8) >> 3) >> 0;
                return A | R | G | B;
            }
            static uint toDDS_A4R4G4B4(uint argb)
            {
                uint A = ((argb & 0xF0000000) >> 4) >> 12;
                uint R = ((argb & 0x00F00000) >> 4) >> 8;
                uint G = ((argb & 0x0000F000) >> 4) >> 4;
                uint B = ((argb & 0x000000F0) >> 4);
                return A | R | G | B;
            }
            static uint toDDS_R8G8B8(uint argb) { return argb & 0x00FFFFFF; }
            static uint toDDS_R5G6B5(uint argb)
            {
                uint R = ((argb & 0x00F80000) >> 3) >> 5;
                uint G = ((argb & 0x0000FC00) >> 2) >> 3;
                uint B = (argb & 0x000000F8) >> 3;
                return (uint)0x00000000 | R | G | B;
            }
            static uint toDDS_A8L8(uint argb)
            {
                uint L = (argb.R() + argb.G() + argb.B()) / 3;
                return argb.A() << 8 | (L & 0xFF);
            }
            #endregion
        }

        /// <summary>
        /// Header of a DDS image
        /// </summary>
        public class DdsHeader
        {
            /// <summary>
            /// About the image header
            /// </summary>
            public enum HeaderFlags
            {
                /// <summary>
                /// Texture
                /// </summary>
                DDS_HEADER_FLAGS_TEXTURE = 0x00001007,	// DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT 
                /// <summary>
                /// MIP map
                /// </summary>
                DDS_HEADER_FLAGS_MIPMAP = 0x00020000,	// DDSD_MIPMAPCOUNT
                /// <summary>
                /// Volume
                /// </summary>
                DDS_HEADER_FLAGS_VOLUME = 0x00800000,	// DDSD_DEPTH
                /// <summary>
                /// Uses pitch
                /// </summary>
                DDS_HEADER_FLAGS_PITCH = 0x00000008,	// DDSD_PITCH
                /// <summary>
                /// Uses linear size
                /// </summary>
                DDS_HEADER_FLAGS_LINEARSIZE = 0x00080000,	// DDSD_LINEARSIZE
            }
            /// <summary>
            /// About the image
            /// </summary>
            public enum SurfaceFlags
            {
                /// <summary>
                /// Is a texture
                /// </summary>
                DDS_SURFACE_FLAGS_TEXTURE = 0x00001000,	// DDSCAPS_TEXTURE
                /// <summary>
                /// Is a MIP map
                /// </summary>
                DDS_SURFACE_FLAGS_MIPMAP = 0x00400008,	// DDSCAPS_COMPLEX | DDSCAPS_MIPMAP
                /// <summary>
                /// Is a cube map
                /// </summary>
                DDS_SURFACE_FLAGS_COMPLEX = 0x00000008,	// DDSCAPS_COMPLEX      //CGM
            }
            /// <summary>
            /// About a cubemap image
            /// </summary>
            public enum CubemapFlags
            {
                /// <summary>
                /// Has +X face
                /// </summary>
                DDS_CUBEMAP_POSITIVEX = 0x00000600, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEX
                /// <summary>
                /// Has -X face
                /// </summary>
                DDS_CUBEMAP_NEGATIVEX = 0x00000a00, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEX
                /// <summary>
                /// Has +Y face
                /// </summary>
                DDS_CUBEMAP_POSITIVEY = 0x00001200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEY
                /// <summary>
                /// Has -Y face
                /// </summary>
                DDS_CUBEMAP_NEGATIVEY = 0x00002200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEY
                /// <summary>
                /// Has +Z face
                /// </summary>
                DDS_CUBEMAP_POSITIVEZ = 0x00004200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEZ
                /// <summary>
                /// Has -Z face
                /// </summary>
                DDS_CUBEMAP_NEGATIVEZ = 0x00008200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEZ
                /// <summary>
                /// Has all faces
                /// </summary>
                DDS_CUBEMAP_ALLFACES = (DDS_CUBEMAP_POSITIVEX | DDS_CUBEMAP_NEGATIVEX |
                                                    DDS_CUBEMAP_POSITIVEY | DDS_CUBEMAP_NEGATIVEY |
                                                    DDS_CUBEMAP_POSITIVEZ | DDS_CUBEMAP_NEGATIVEZ)
            }
            /// <summary>
            /// About a volume image
            /// </summary>
            public enum VolumeFlags
            {
                /// <summary>
                /// Is a volume
                /// </summary>
                DDS_FLAGS_VOLUME = 0x00200000,	// DDSCAPS2_VOLUME
            }

            uint m_size;
            /// <summary>
            /// About the image header
            /// </summary>
            public HeaderFlags m_headerFlags { get; internal set; }
            /// <summary>
            /// Stores the height read
            /// </summary>
            public int m_height { get; private set; }
            /// <summary>
            /// Stores the width read
            /// </summary>
            public int m_width { get; private set; }
            /// <summary>
            /// Stores the pitch or linear size read
            /// </summary>
            public int m_pitchOrLinearSize { get; internal set; }           //CGM
            /// <summary>
            /// Stores the pixel depth read
            /// </summary>
            public uint m_depth { get; private set; }
            /// <summary>
            /// Stores the MIP map count read
            /// </summary>
            public uint m_mipMapCount { get; internal set; }                //CGM
            uint m_reserved1_0;
            uint m_reserved1_1;
            uint m_reserved1_2;
            uint m_reserved1_3;
            uint m_reserved1_4;
            uint m_reserved1_5;
            uint m_reserved1_6;
            uint m_reserved1_7;
            uint m_reserved1_8;
            uint m_reserved1_9;
            uint m_reserved1_10;
            /// <summary>
            /// Pixel format
            /// </summary>
            public DdsPixelFormat m_pixelFormat { get; private set; }
            /// <summary>
            /// About the image
            /// </summary>
            public SurfaceFlags m_surfaceFlags { get; internal set; }        //dwCAPS - CGM
            /// <summary>
            /// About a cubemap image
            /// </summary>
            public CubemapFlags m_cubemapFlags { get; private set; }        //dwCAPS2
            uint m_reserved2_0;
            uint m_reserved2_1;
            uint m_reserved2_2;

            internal DdsHeader(DdsFileFormat ddsFileFormat, int width, int height)
            {
                m_size = 124;
                m_headerFlags = HeaderFlags.DDS_HEADER_FLAGS_TEXTURE;//minimum
                m_width = width;
                m_height = height;
                m_surfaceFlags = SurfaceFlags.DDS_SURFACE_FLAGS_TEXTURE;
                SetDdsFileFormat(ddsFileFormat);
            }

            internal DdsHeader(DdsHeader basis)
            {
                m_size = basis.m_size;
                m_headerFlags = basis.m_headerFlags;
                m_height = basis.m_height;
                m_width = basis.m_width;
                m_pitchOrLinearSize = basis.m_pitchOrLinearSize;
                m_depth = basis.m_depth;
                m_mipMapCount = basis.m_mipMapCount;
                m_reserved1_0 = basis.m_reserved1_0;
                m_reserved1_1 = basis.m_reserved1_1;
                m_reserved1_2 = basis.m_reserved1_2;
                m_reserved1_3 = basis.m_reserved1_3;
                m_reserved1_4 = basis.m_reserved1_4;
                m_reserved1_5 = basis.m_reserved1_5;
                m_reserved1_6 = basis.m_reserved1_6;
                m_reserved1_7 = basis.m_reserved1_7;
                m_reserved1_8 = basis.m_reserved1_8;
                m_reserved1_9 = basis.m_reserved1_9;
                m_reserved1_10 = basis.m_reserved1_10;
                m_pixelFormat = new DdsPixelFormat(basis.m_pixelFormat);
                m_surfaceFlags = basis.m_surfaceFlags;
                m_cubemapFlags = basis.m_cubemapFlags;
                m_reserved2_0 = basis.m_reserved2_0;
                m_reserved2_1 = basis.m_reserved2_1;
                m_reserved2_2 = basis.m_reserved2_2;
            }

            internal DdsHeader(System.IO.Stream input)
            {
                BinaryReader br = new BinaryReader(input);

                this.m_size = br.ReadUInt32();
                this.m_headerFlags = (HeaderFlags)br.ReadUInt32();
                this.m_height = br.ReadInt32();
                this.m_width = br.ReadInt32();
                this.m_pitchOrLinearSize = br.ReadInt32();
                this.m_depth = br.ReadUInt32();
                this.m_mipMapCount = br.ReadUInt32();
                this.m_reserved1_0 = br.ReadUInt32();
                this.m_reserved1_1 = br.ReadUInt32();
                this.m_reserved1_2 = br.ReadUInt32();
                this.m_reserved1_3 = br.ReadUInt32();
                this.m_reserved1_4 = br.ReadUInt32();
                this.m_reserved1_5 = br.ReadUInt32();
                this.m_reserved1_6 = br.ReadUInt32();
                this.m_reserved1_7 = br.ReadUInt32();
                this.m_reserved1_8 = br.ReadUInt32();
                this.m_reserved1_9 = br.ReadUInt32();
                this.m_reserved1_10 = br.ReadUInt32();
                this.m_pixelFormat = new DdsPixelFormat(br);
                this.m_surfaceFlags = (SurfaceFlags)br.ReadUInt32();
                this.m_cubemapFlags = (CubemapFlags)br.ReadUInt32();
                this.m_reserved2_0 = br.ReadUInt32();
                this.m_reserved2_1 = br.ReadUInt32();
                this.m_reserved2_2 = br.ReadUInt32();
            }

            internal void Write(Stream output)
            {
                BinaryWriter writer = new BinaryWriter(output);
                writer.Write(this.m_size);
                writer.Write((uint)this.m_headerFlags);
                writer.Write(this.m_height);
                writer.Write(this.m_width);
                writer.Write(this.m_pitchOrLinearSize);
                writer.Write(this.m_depth);
                writer.Write(this.m_mipMapCount);
                writer.Write(this.m_reserved1_0);
                writer.Write(this.m_reserved1_1);
                writer.Write(this.m_reserved1_2);
                writer.Write(this.m_reserved1_3);
                writer.Write(this.m_reserved1_4);
                writer.Write(this.m_reserved1_5);
                writer.Write(this.m_reserved1_6);
                writer.Write(this.m_reserved1_7);
                writer.Write(this.m_reserved1_8);
                writer.Write(this.m_reserved1_9);
                writer.Write(this.m_reserved1_10);
                this.m_pixelFormat.Write(writer);
                writer.Write((uint)this.m_surfaceFlags);
                writer.Write((uint)this.m_cubemapFlags);
                writer.Write(this.m_reserved2_0);
                writer.Write(this.m_reserved2_1);
                writer.Write(this.m_reserved2_2);
            }

            internal void SetDdsFileFormat(DdsFileFormat ddsFileFormat)
            {
                switch (ddsFileFormat)
                {
                    case DdsFileFormat.DDS_FORMAT_DXT1:
                    case DdsFileFormat.DDS_FORMAT_DXT3:
                    case DdsFileFormat.DDS_FORMAT_DXT5:
                        m_headerFlags |= HeaderFlags.DDS_HEADER_FLAGS_LINEARSIZE;
                        m_headerFlags &= ~HeaderFlags.DDS_HEADER_FLAGS_PITCH;               //CGM
                        break;
                    default:
                        m_headerFlags |= HeaderFlags.DDS_HEADER_FLAGS_PITCH;
                        m_headerFlags &= ~HeaderFlags.DDS_HEADER_FLAGS_LINEARSIZE;          //CGM
                        break;
                }
                m_pixelFormat = new DdsPixelFormat(ddsFileFormat);
                m_pitchOrLinearSize = internalPitch();
            }

            internal int internalPitch()
            {
                if ((m_pixelFormat.m_flags & DdsPixelFormat.PixelFormatFlags.DDS_FOURCC) != 0)
                {
                    //if (m_pixelFormat.m_fourCC == fourccDXT1)
                    //{
                    //    return Math.Max(1, (m_width + 3) / 4) * 8;
                    //}
                    //return Math.Max(1, (m_width + 3) / 4) * 16;
                    return m_width * m_height;                                              //CGM
                }
                return (m_width * m_pixelFormat.m_rgbBitCount + 7) / 8;
            }

            internal int getRowPitch()
            {
                if ((m_headerFlags & DdsHeader.HeaderFlags.DDS_HEADER_FLAGS_PITCH) != 0)
                {
                    // Pitch specified.. so we can use directly
                    return m_pitchOrLinearSize;
                }
                else if ((m_headerFlags & DdsHeader.HeaderFlags.DDS_HEADER_FLAGS_LINEARSIZE) != 0)
                {
                    // Linear size specified.. compute row pitch. Of course, this should never happen
                    // as linear size is *supposed* to be for compressed textures. But Microsoft don't 
                    // always play by the rules when it comes to DDS output. 
                    return m_pitchOrLinearSize / m_height;
                }
                else
                {
                    // Another case of Microsoft not obeying their standard is the 'Convert to..' shell extension
                    // that ships in the DirectX SDK. Seems to always leave flags empty..so no indication of pitch
                    // or linear size. And - to cap it all off - they leave pitchOrLinearSize as *zero*. Zero??? If
                    // we get this bizarre set of inputs, we just go 'screw it' and compute row pitch ourselves, 
                    // making sure we DWORD align it (if that code path is enabled).
                    return m_width * m_pixelFormat.PixelSize;

#if	APPLY_PITCH_ALIGNMENT
					rowPitch = ( ( ( int )rowPitch + 3 ) & ( ~3 ) );
#endif	// APPLY_PITCH_ALIGNMENT
                }
            }
        }
        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            baseImage = null;
            currentImage = null;
            maskInEffect = false;
            hsvData = null;
            hsvShift = HSVShift.Empty;
        }

        #region File I/O

        /// <summary>
        /// Loads the header from a DDS image.
        /// </summary>
        /// <param name="input">A <see cref="Stream"/> containing a DDS-encoded image.</param>
        /// <exception cref="FormatException"><paramref name="input"/> does not contain a DDS-encoded image.</exception>
        /// <remarks>
        /// Once loaded, the following methods will return valid information about the image:
        /// <list type="bullet">
        /// <item><description>HasAlphaChannel</description></item>
        /// <item><description>Size</description></item>
        /// </list>
        /// </remarks>
        public void GetInfo(System.IO.Stream input)
        {
            long posn = input.Position;
            uint ddsTag = new BinaryReader(input).ReadUInt32();
            if (ddsTag != fourccDDS_)
                throw new FormatException("Stream does not appear to contain a DDS image");

            ddsHeader = new DdsHeader(input);

            input.Position = posn;
        }


        /// <summary>
        /// Loads the data from an image encoded using one of the supported DDS mechanisms.
        /// If <paramref name="supportHSV"/> is true, also creates an HSVa-encoded version of the image.
        /// </summary>
        /// <param name="input">A <see cref="System.IO.Stream"/> containing the DDS-encoded image.</param>
        /// <param name="supportHSV">When true, create an HSVa-encoded version of the image.</param>
        public void Load(System.IO.Stream input, bool supportHSV)
        {
            // Read the DDS tag. If it's not right, then bail.. 
            uint ddsTag = new BinaryReader(input).ReadUInt32();
            if (ddsTag != fourccDDS_)
                throw new FormatException("File does not appear to be a DDS image");

            // Read everything in.. for now assume it worked like a charm..
            ddsHeader = new DdsHeader(input);

            if ((ddsHeader.m_pixelFormat.m_flags & DdsPixelFormat.PixelFormatFlags.DDS_FOURCC) != 0)
            {
                #region We can use squish
                DdsSquish.SquishFlags squishFlags = 0;

                switch (ddsHeader.m_pixelFormat.m_fourCC)
                {
                    case fourccDXT1:
                        squishFlags = DdsSquish.SquishFlags.kDxt1;
                        break;

                    case fourccDXT3:
                        squishFlags = DdsSquish.SquishFlags.kDxt3;
                        break;

                    case fourccDXT5:
                        squishFlags = DdsSquish.SquishFlags.kDxt5;
                        break;

                    default:
                        throw new FormatException("File does not use a squish-supported compression format");
                }

                // Compute size of compressed block area
                int blockCount = ((ddsHeader.m_width + 3) / 4) * ((ddsHeader.m_height + 3) / 4);
                int blockSize = ((squishFlags & DdsSquish.SquishFlags.kDxt1) != 0) ? 8 : 16;

                // Allocate room for compressed blocks, and read data into it.
                byte[] compressedBlocks = new byte[blockCount * blockSize];
                input.Read(compressedBlocks, 0, compressedBlocks.Length);

                // Now decompress..
                byte[] pixelData = DdsSquish.DecompressImage(compressedBlocks, ddsHeader.m_width, ddsHeader.m_height, squishFlags);

                // Convert R, G, B, A byte array to ARGB uint array...
                baseImage = new uint[pixelData.Length / sizeof(uint)];
                for (int i = 0; i < baseImage.Length; i++)
                {
                    uint value = BitConverter.ToUInt32(pixelData, i * sizeof(uint));
                    baseImage[i] = DdsPixelFormat.fromDDS_A8B8G8R8(value);// >> 8 | value << 24;
                }
                #endregion
            }
            else
            {
                #region It's a non-squishable one, so try manual methods...

                ToARGB toARGB = null;
                ToPixel unused;
                ddsHeader.m_pixelFormat.codecFromPixelFormat(out toARGB, out unused);

                // If toARGB is still null, then it's an unsupported format.
                if (toARGB == null)
                    throw new FormatException("File is not a supported non-DXT DDS format");

                // We need the pitch for a row, so we can allocate enough memory for the load.
                int rowPitch = ddsHeader.getRowPitch();

                // Ok.. now, we need to allocate room for the bytes to read in from.. it's rowPitch bytes * height
                byte[] readPixelData = new byte[rowPitch * ddsHeader.m_height];
                input.Read(readPixelData, 0, readPixelData.Length);

                // ...and for the image
                baseImage = new uint[ddsHeader.m_width * ddsHeader.m_height];

                // And now we have the arduous task of filling that up with stuff..
                for (int destY = 0; destY < ddsHeader.m_height; destY++)
                {
                    for (int destX = 0; destX < (int)ddsHeader.m_width; destX++)
                    {
                        // Compute pixel offsets
                        int srcPixelOffset = (destY * rowPitch) + (destX * ddsHeader.m_pixelFormat.PixelSize);
                        int destPixelOffset = (destY * (int)ddsHeader.m_width) + destX;

                        // Build our pixel colour as a DWORD
                        uint pixelColour = 0;
                        for (int loop = 0; loop < ddsHeader.m_pixelFormat.PixelSize; loop++)
                            pixelColour |= (uint)(readPixelData[srcPixelOffset + loop] << (8 * loop));

                        // delegate takes care of calculation, set when determining format
                        baseImage[destPixelOffset] = toARGB(pixelColour);
                    }
                }
                #endregion
            }

            currentImage = (uint[])baseImage.Clone();
            if (supportHSV) UpdateHSVData();
        }

        ///  <summary>
        ///  Saves the current image using one of the supported DDS mechanisms.
        ///  </summary>
        ///  <param name="output">A <see cref="T:System.IO.Stream"/> to receive the DDS-encoded image.</param>
        public void Save(Stream output)
        {
            uint[] sourcePixels;
            if (SupportsHSV && !hsvShift.IsEmpty)
            {
                sourcePixels = ColorHSVA.ToArrayARGB(hsvData, hsvShift);
            }
            else
            {
                sourcePixels = currentImage;
            }

            // Write the DDS tag.
            new BinaryWriter(output).Write(fourccDDS_);

            // Set up for mipmaps  -  start CGM code
            if ((ddsHeader.m_headerFlags & DdsHeader.HeaderFlags.DDS_HEADER_FLAGS_MIPMAP) != 0)
            {
                ddsHeader.m_mipMapCount = DdsMipmap.numberMipmaps(ddsHeader);
                ddsHeader.m_surfaceFlags |= DdsHeader.SurfaceFlags.DDS_SURFACE_FLAGS_MIPMAP;
                ddsHeader.m_surfaceFlags |= DdsHeader.SurfaceFlags.DDS_SURFACE_FLAGS_COMPLEX;
            }
            else
            {
                ddsHeader.m_mipMapCount = 1;
                ddsHeader.m_surfaceFlags &= ~DdsHeader.SurfaceFlags.DDS_SURFACE_FLAGS_MIPMAP;
                ddsHeader.m_surfaceFlags &= ~DdsHeader.SurfaceFlags.DDS_SURFACE_FLAGS_COMPLEX;
            }       //end Mipmap header section

            // Write the header.
            ddsHeader.Write(output);

            if ((ddsHeader.m_pixelFormat.m_flags & DdsPixelFormat.PixelFormatFlags.DDS_FOURCC) != 0)
            {
                #region We can use squish
                DdsSquish.SquishFlags flags = 0;
                switch (ddsHeader.m_pixelFormat.m_fourCC)
                {
                    case fourccDXT1:
                        flags = DdsSquish.SquishFlags.kDxt1;
                        break;

                    case fourccDXT3:
                        flags = DdsSquish.SquishFlags.kDxt3;
                        break;

                    case fourccDXT5:
                        flags = DdsSquish.SquishFlags.kDxt5;
                        break;

                    default:
                        throw new FormatException("File is not a squish-supported DDS format");
                }

                DdsFile currentMip = new DdsFile();                  //CGM
                currentMip.CreateImage(this, SupportsHSV);           //CGM

                for (int mip = 0; mip < ddsHeader.m_mipMapCount; mip++) //CGM - use copy of DdsFile (currentMip) to iterate through mipmaps, including main texture
                {
                    // Convert ARGB uint array to R, G, B, A byte array...
                    byte[] pixelData = new byte[sourcePixels.Length * sizeof(uint)];
                    for (int i = 0; i < sourcePixels.Length; i++)
                    {
                        uint value = DdsPixelFormat.toDDS_A8B8G8R8(sourcePixels[i]);
                        Array.Copy(BitConverter.GetBytes(value), 0, pixelData, i * sizeof(uint), sizeof(uint));
                    }

                    byte[] compressedBlocks = DdsSquish.CompressImage(pixelData, currentMip.ddsHeader.m_width, currentMip.ddsHeader.m_height, flags);

                    // Write out compressed data.
                    output.Write(compressedBlocks, 0, compressedBlocks.Length);

                    //Mipmap code - CGM
                    if (currentMip.ddsHeader.m_width > 1 | currentMip.ddsHeader.m_height > 1)
                    {
                        currentMip = DdsMipmap.nextMipmap(this, currentMip.ddsHeader.m_width, currentMip.ddsHeader.m_height);
                        if (currentMip.SupportsHSV && !currentMip.hsvShift.IsEmpty)
                        {
                            sourcePixels = ColorHSVA.ToArrayARGB(currentMip.hsvData, currentMip.hsvShift);
                        }
                        else
                        {
                            sourcePixels = currentMip.currentImage;
                        }
                    }   //end Mipmap code
                }

                #endregion
            }
            else
            {
                #region It's a non-squishable one, so try manual methods...

                ToARGB unused = null;
                ToPixel toPixel;
                ddsHeader.m_pixelFormat.codecFromPixelFormat(out unused, out toPixel);

                // If toPixel is still null, then it's an unsupported format.
                if (toPixel == null)
                    throw new FormatException("Image is not a supported non-DXT DDS format");

                DdsFile currentMip = new DdsFile();                  //CGM
                currentMip.CreateImage(this, SupportsHSV);           //CGM

                for (int mip = 0; mip < ddsHeader.m_mipMapCount; mip++) //CGM - use copy of DdsFile (currentMip) to iterate through mipmaps, including main texture
                {
                    // We need the pitch for a row, so we can allocate enough memory for the output.
                    int rowPitch = currentMip.ddsHeader.getRowPitch();

                    // Ok.. now, we need to allocate room for the bytes to write out... it's rowPitch bytes * height
                    byte[] pixelData = new byte[rowPitch * currentMip.ddsHeader.m_height];

                    // Convert ARGB uint array to DDS pixel format
                    for (int srcY = 0; srcY < currentMip.ddsHeader.m_height; srcY++)
                    {
                        for (int srcX = 0; srcX < currentMip.ddsHeader.m_width; srcX++)
                        {
                            // Compute pixel offsets
                            int destPixelOffset = (srcY * rowPitch) + (srcX * ddsHeader.m_pixelFormat.PixelSize);
                            int srcPixelOffset = (srcY * currentMip.ddsHeader.m_width) + srcX;

                            // delegate takes care of calculation, set when determining format
                            uint pixelColour = toPixel(sourcePixels[srcPixelOffset]);

                            // Store each computed byte
                            for (int loop = 0; loop < ddsHeader.m_pixelFormat.PixelSize; loop++)
                                pixelData[destPixelOffset + loop] = (byte)((pixelColour >> (8 * loop)) & 0xff);
                        }
                    }

                    // And write it...
                    output.Write(pixelData, 0, pixelData.Length);

                    //Mipmap code - CGM
                    if (currentMip.ddsHeader.m_width > 1 | currentMip.ddsHeader.m_height > 1)
                    {
                        currentMip = DdsMipmap.nextMipmap(this, currentMip.ddsHeader.m_width, currentMip.ddsHeader.m_height);
                        if (currentMip.SupportsHSV && !currentMip.hsvShift.IsEmpty)
                        {
                            sourcePixels = ColorHSVA.ToArrayARGB(currentMip.hsvData, currentMip.hsvShift);
                        }
                        else
                        {
                            sourcePixels = currentMip.currentImage;
                        }
                    }   //end Mipmap code
                }
                #endregion
            }

            output.Flush();
        }

        internal delegate uint ToARGB(uint pixelColour);
        internal delegate uint ToPixel(uint argb);
        #endregion

        #region "Constructors", really
        /// <summary>
        /// Creates an image of a single colour, specified by the byte parameters,
        /// with the size given by the int parameters.
        /// If <paramref name="supportHSV"/> is true, also creates an HSVa-encoded version of the image.
        /// </summary>
        /// <param name="r">Amount of red per pixel.</param>
        /// <param name="g">Amount of green per pixel.</param>
        /// <param name="b">Amount of blue per pixel.</param>
        /// <param name="a">Amount of alpha per pixel.</param>
        /// <param name="width">Width of image.</param>
        /// <param name="height">Height of image.</param>
        /// <param name="supportHSV">When true, create an HSVa-encoded version of the image.</param>
        public void CreateImage(byte r, byte g, byte b, byte a, int width, int height, bool supportHSV)
        {
            CreateImage((uint)(a << 24 | r << 16 | g << 8 | b), width, height, supportHSV);
        }

        /// <summary>
        /// Creates an image of a single colour, specified by the <see cref="Color"/> parameter,
        /// with the size given by the int parameters.
        /// If <paramref name="supportHSV"/> is true, also creates an HSVa-encoded version of the image.
        /// </summary>
        /// <param name="color"><see cref="Color"/> of image.</param>
        /// <param name="width">Width of image.</param>
        /// <param name="height">Height of image.</param>
        /// <param name="supportHSV">When true, create an HSVa-encoded version of the image.</param>
        public void CreateImage(Color color, int width, int height, bool supportHSV)
        {
            CreateImage((uint)color.ToArgb(), width, height, supportHSV);
        }

        /// <summary>
        /// Creates an image of a single colour, specified by the uint parameter
        /// (low byte is "blue", then "green", then "red" then high byte is "alpha"),
        /// with the size given by the int parameters.
        /// If <paramref name="supportHSV"/> is true, also creates an HSVa-encoded version of the image.
        /// </summary>
        /// <param name="argb">Colour of image (low byte is "blue", then "green", then "red" then high byte is "alpha").</param>
        /// <param name="width">Width of image.</param>
        /// <param name="height">Height of image.</param>
        /// <param name="supportHSV">When true, create an HSVa-encoded version of the image.</param>
        public void CreateImage(uint argb, int width, int height, bool supportHSV)
        {
            ddsHeader = new DdsHeader(DdsFileFormat.DDS_FORMAT_DXT5, width, height);

            // SetPixels operates on currentImage, so create this...
            currentImage = new uint[width * height];
            SetPixels((x, y, unused) => argb);

            // ...and set the baseImage from the current image
            baseImage = (uint[])currentImage.Clone();

            if (supportHSV) UpdateHSVData();
        }

        /// <summary>
        /// Creates an image from a given <see cref="T:DdsFile"/>.
        /// If <paramref name="supportHSV"/> is true, also creates an HSVa-encoded version of the image.
        /// </summary>
        /// <param name="image"><see cref="T:DdsFile"/> to clone.</param>
        /// <param name="supportHSV">When true, create an HSVa-encoded version of the image.</param>
        public void CreateImage(DdsFile image, bool supportHSV)
        {
            ddsHeader = new DdsHeader(image.ddsHeader);

            baseImage = (uint[])image.currentImage.Clone();

            currentImage = (uint[])baseImage.Clone();
            if (supportHSV) UpdateHSVData();
        }

        /// <summary>
        /// Creates an image from a given <see cref="T:DdsFile"/>, resized as requested.
        /// If <paramref name="supportHSV"/> is true, also creates an HSVa-encoded version of the image.
        /// </summary>
        /// <param name="image"><see cref="T:DdsFile"/> to clone.</param>
        /// <param name="width">Width of image.</param>
        /// <param name="height">Height of image.</param>
        /// <param name="supportHSV">When true, create an HSVa-encoded version of the image.</param>
        public void CreateImage(DdsFile image, int width, int height, bool supportHSV)
        {
            ddsHeader = new DdsHeader(image.ddsHeader.m_pixelFormat.fileFormatFromPixelFormat, width, height);

            // Regardless of the pixel format, using Bitmap to resize the image pre-multiplies the alpha
            // so we need this mess.

            // First, get the image, strip the alpha, then resize
            using (DdsFile ddsFileBase = new DdsFile())
            {
                ddsFileBase.CreateImage(image, false);
                ddsFileBase.DeleteAlphaChannel();
                baseImage = new Bitmap(ddsFileBase.Image, width, height).ToARGBData();
            }

            currentImage = (uint[])baseImage.Clone();

            // Now reapply the original alpha
            if (image.AlphaDepth > 0)
            {
                Bitmap alpha = new Bitmap(image.GetGreyscaleFromAlpha(), width, height);
                SetAlphaFromGreyscale(alpha);
            }

            if (supportHSV) UpdateHSVData();
        }

        /// <summary>
        /// Creates an image from a given <see cref="T:Image"/>.
        /// If <paramref name="supportHSV"/> is true, also creates an HSVa-encoded version of the image.
        /// </summary>
        /// <param name="image"><see cref="T:Image"/> from which to extract image pixels.</param>
        /// <param name="supportHSV">When true, create an HSVa-encoded version of the image.</param>
        public void CreateImage(Image image, bool supportHSV) { CreateImage(new Bitmap(image), supportHSV); }

        /// <summary>
        /// Creates an image from a given <see cref="T:Bitmap"/>.
        /// If <paramref name="supportHSV"/> is true, also creates an HSVa-encoded version of the image.
        /// </summary>
        /// <param name="image"><see cref="T:Bitmap"/> from which to extract image pixels.</param>
        /// <param name="supportHSV">When true, create an HSVa-encoded version of the image.</param>
        public void CreateImage(Bitmap image, bool supportHSV)
        {
            ddsHeader = new DdsHeader(DdsFileFormat.DDS_FORMAT_DXT5, image.Width, image.Height);

            baseImage = image.ToARGBData();

            currentImage = (uint[])baseImage.Clone();
            if (supportHSV) UpdateHSVData();
        }

        /// <summary>
        /// Creates an image from a given <see cref="T:Bitmap"/>, resized as requested.
        /// If <paramref name="supportHSV"/> is true, also creates an HSVa-encoded version of the image.
        /// </summary>
        /// <param name="image"><see cref="T:Bitmap"/> from which to extract image pixels.</param>
        /// <param name="width">Width of image.</param>
        /// <param name="height">Height of image.</param>
        /// <param name="supportHSV">When true, create an HSVa-encoded version of the image.</param>
        public void CreateImage(Bitmap image, int width, int height, bool supportHSV)
        {
            using (DdsFile ddsFileBase = new DdsFile())
            {
                ddsFileBase.CreateImage(image, false);
                CreateImage(ddsFileBase.Resize(new Size(width, height)), supportHSV);
            }
        }
        #endregion

        #region Set Alpha channel
        /// <summary>
        /// Converts the R, G and B channels of the supplied <paramref name="image"/> to greyscale
        /// and loads this into the Alpha channel of the current image.
        /// The image format will be changed to one supporting an 8-bit Alpha channel, if required.
        /// </summary>
        /// <param name="image"><see cref="DdsFile"/> to extract greyscale data from for alpha channel.</param>
        public void SetAlphaFromGreyscale(DdsFile image)
        {
            AlphaDepth = UseDXT ? 5 : 8;

            SetPixels((x, y, value) =>
            {
                uint alpha;
                lock (image)
                {
                    uint srcValue = image.GetPixel(x % image.ddsHeader.m_width, y % image.ddsHeader.m_height);
                    alpha = ((srcValue.R() + srcValue.G() + srcValue.B()) / 3) & 0xff;
                }

                return (value & 0x00FFFFFF) | (alpha << 24);
            });

            if (SupportsHSV) UpdateHSVData();
        }

        /// <summary>
        /// Converts the R, G and B channels of the supplied <paramref name="image"/> to greyscale
        /// and loads this into the Alpha channel of the current image.
        /// </summary>
        /// <param name="image"><see cref="T:Image"/> to extract greyscale data from for alpha channel.</param>
        public void SetAlphaFromGreyscale(Image image) { SetAlphaFromGreyscale(new Bitmap(image)); }

        // (0 + 0 + 0) / 3 = 0
        // (255 + 255 + 255) / 3 = 255
        /// <summary>
        /// Converts the R, G and B channels of the supplied <paramref name="image"/> to greyscale
        /// and loads this into the Alpha channel of the current image.
        /// The image format will be changed to one supporting an Alpha channel, if required.
        /// </summary>
        /// <param name="image"><see cref="Bitmap"/> to extract greyscale data from for alpha channel.</param>
        public void SetAlphaFromGreyscale(Bitmap image)
        {
            AlphaDepth = UseDXT ? 5 : 8;

            SetPixels((x, y, value) =>
            {
                uint alpha;
                lock (image)
                {
                    Color srcValue = image.GetPixel(x % image.Width, y % image.Height);
                    alpha = ((uint)(srcValue.R + srcValue.G + srcValue.B) / 3) & 0xff;
                }

                return (value & 0x00FFFFFF) | (alpha << 24);
            });

            if (SupportsHSV) UpdateHSVData();
        }

        /// <summary>
        /// Set the image format to one without an alpha channel, clearing the alpha data.
        /// </summary>
        public void DeleteAlphaChannel()
        {
            AlphaDepth = UseDXT ? 1 : 0;
            SetPixels((x, y, value) => value |= 0xFF000000);

            if (SupportsHSV) UpdateHSVData();
        }

        /// <summary>
        /// Get a greyscale image representing the alpha channel of the current image.
        /// </summary>
        /// <returns>A greyscale image representing the alpha channel of the current image.</returns>
        public Bitmap GetGreyscaleFromAlpha()
        {
            uint[] greyscale = new uint[currentImage.Length];

            DoAction((x, y, value) => greyscale[x + y * ddsHeader.m_width] = ((uint)0xFF << 24) | value.A() << 16 | value.A() << 8 | value.A() << 0);

            return greyscale.ToBitmap(this.Size);
        }

        /// <summary>
        /// When true, indicates the DDS image is encoded with an alpha channel.
        /// </summary>
        [Obsolete("AlphaDepth should be used in preference")]
        public bool HasAlphaChannel { get { return ddsHeader.m_pixelFormat.IsFourChannel; } }

        /// <summary>
        /// When UseDXT is true, the DXT compression mode.
        /// Otherwise the number of bits per alpha pixel.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown if an unsupported alpha depth is set.
        /// </exception>
        public int AlphaDepth
        {
            get { return ddsHeader.m_pixelFormat.AlphaDepth; }
            set 
            { 
                ddsHeader.m_pixelFormat.AlphaDepth = value;
                ddsHeader.m_pitchOrLinearSize = ddsHeader.internalPitch();          //CGM
            }
        }
        #endregion

        /// <summary>
        /// If true, use DXT-type image compression for storage.
        /// The exact format will depend on the <see cref="AlphaDepth"/>.
        /// Setting to false (from true) will default to A8B8G8R8 format.
        /// </summary>
        public bool UseDXT
        {
            get { return (ddsHeader.m_pixelFormat.m_flags & DdsPixelFormat.PixelFormatFlags.DDS_FOURCC) != 0; }
            set
            {
                if (value == UseDXT) return;

                if (value)
                {
                    int a = ddsHeader.m_pixelFormat.AlphaDepth;
                    // Turn on DXT
                    if (a <= 1) ddsHeader.SetDdsFileFormat(DdsFileFormat.DDS_FORMAT_DXT1);
                    else if (a <= 4) ddsHeader.SetDdsFileFormat(DdsFileFormat.DDS_FORMAT_DXT3);
                    else if (a <= 8) ddsHeader.SetDdsFileFormat(DdsFileFormat.DDS_FORMAT_DXT5);
                    else
                        throw new InvalidOperationException("Cannot set UseDXT with unsupported alpha depth.");
                }
                else
                {
                    // Turn off DXT
                    ddsHeader.SetDdsFileFormat(DdsFileFormat.DDS_FORMAT_A8R8G8B8);          //For game compatibility - CGM
                }
            }
        }

        /// <summary>
        /// If true, mipmaps will be created and written when the image is saved.
        /// </summary>
        public bool GenerateMipmaps             //Mipmap code - CGM
        {
            get { return ((ddsHeader.m_headerFlags & DdsHeader.HeaderFlags.DDS_HEADER_FLAGS_MIPMAP) != 0); }
            set
            {
                if (value)
                {
                    ddsHeader.m_headerFlags |= DdsHeader.HeaderFlags.DDS_HEADER_FLAGS_MIPMAP;
                }
                else
                {
                    ddsHeader.m_headerFlags &= ~DdsHeader.HeaderFlags.DDS_HEADER_FLAGS_MIPMAP;
                }
            }
        }       //end mipmap code

        /// <summary>
        /// If true, treat the image as a luminance (plus alpha) map for storage.
        /// Currently only A8L8, 16bit coding is supported.
        /// Setting to false (from true) will default to A8B8G8R8 (non-DXT) format.
        /// </summary>
        /// <remarks>
        /// Note that setting to true does not turn the image into a greyscale.
        /// This only happens on saving the image (and will not affect the displayed values until they are read back in).
        /// </remarks>
        public bool UseLuminance
        {
            get { return (ddsHeader.m_pixelFormat.m_flags & DdsPixelFormat.PixelFormatFlags.DDS_LUMINANCE) != 0; }
            set
            {
                if (value == UseLuminance) return;

                if (value)
                {
                    ddsHeader.SetDdsFileFormat(DdsFileFormat.DDS_FORMAT_A8L8);
                }
                else
                {
                    ddsHeader.SetDdsFileFormat(DdsFileFormat.DDS_FORMAT_A8B8G8R8);
                }
            }
        }

        /// <summary>
        /// Extract a <see cref="T:Image"/> representing the current image, subject to the filtering requested.
        /// </summary>
        /// <param name="red">When true, the red channel of the DDS contributes to the red pixels of the returned image.</param>
        /// <param name="green">When true, the green channel of the DDS contributes to the green pixels of the returned image.</param>
        /// <param name="blue">When true, the blue channel of the DDS contributes to the blue pixels of the returned image.</param>
        /// <param name="alpha">When true, the alpha channel of the DDS contributes to the pixel transparency in the returned image.</param>
        /// <param name="invert">When true, the alpha channel of the DDS is inverted.</param>
        /// <returns>A <see cref="System.Drawing.Image"/> representation of the DDS encoded image in the loaded <see cref="System.IO.Stream"/>.</returns>
        /// <seealso cref="Load"/>
        public Bitmap GetImage(bool red, bool green, bool blue, bool alpha, bool invert)
        {
            uint mask = (alpha ? (uint)0xFF000000 : 0) | (red ? (uint)0x00FF0000 : 0) | (green ? (uint)0x0000FF00 : 0) | (blue ? (uint)0x000000FF : 0);
            Bitmap bitmap = new Bitmap(ddsHeader.m_width, ddsHeader.m_height,
                alpha ? Imaging.PixelFormat.Format32bppArgb : Imaging.PixelFormat.Format32bppRgb);

            uint[] sourcePixels;
            if (SupportsHSV && !hsvShift.IsEmpty)
            {
                sourcePixels = ColorHSVA.ToArrayARGB(hsvData, hsvShift);
            }
            else
            {
                sourcePixels = currentImage;
            }
            for (int y = 0; y < bitmap.Height; y++)
            {
                int offset = y * bitmap.Width;
                for (int x = 0; x < bitmap.Width; x++)
                {
                    uint pixel = sourcePixels[offset + x] & mask;
                    if (alpha) { if (invert) pixel = (pixel & 0x00FFFFFF) | ((255 - pixel.A()) << 24); }
                    else pixel |= 0xFF000000;

                    bitmap.SetPixel(x, y, Color.FromArgb((int)pixel));
                }
            }

            return bitmap;

          //  uint mask = (alpha ? (uint)0xFF000000 : 0) | (red ? (uint)0x00FF0000 : 0) | (green ? (uint)0x0000FF00 : 0) | (blue ? (uint)0x000000FF : 0);
          //  Bitmap bitmap = new Bitmap(ddsHeader.m_width, ddsHeader.m_height,
          //      alpha ? Imaging.PixelFormat.Format32bppArgb : Imaging.PixelFormat.Format32bppRgb);
          //  Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
          //  Imaging.BitmapData bmpData = bitmap.LockBits(rect, Imaging.ImageLockMode.ReadWrite,
          //      bitmap.PixelFormat);
          //  IntPtr ptr;
          //  if (bmpData.Stride > 0) ptr = bmpData.Scan0;
          //  else ptr = bmpData.Scan0 + bmpData.Stride * (bitmap.Height - 1);
          //  int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
          //  byte[] argbValues = new byte[bytes];
          //  System.Runtime.InteropServices.Marshal.Copy(ptr, argbValues, 0, bytes);

          //  uint[] sourcePixels;
          //  if (SupportsHSV && !hsvShift.IsEmpty)
          //  {
          //      sourcePixels = ColorHSVA.ToArrayARGB(hsvData, hsvShift);
          //  }
          //  else
          //  {
          //      sourcePixels = currentImage;
          //  }
          //  for (int y = 0; y < bitmap.Height; y++)
          //  {
          //      int offset = y * bitmap.Width;
          //      for (int x = 0; x < bitmap.Width; x++)
          //      {
          //          uint pixel = sourcePixels[offset + x] & mask;
          //          if (alpha) { if (invert) pixel = (pixel & 0x00FFFFFF) | ((255 - pixel.A()) << 24); }
          //          else pixel |= 0xFF000000;

                    // bitmap.SetPixel(x, y, Color.FromArgb((int)pixel));
          //          argbValues[x + offset] = (byte)(pixel >> 24);
          //          argbValues[x + offset + 1] = (byte)(pixel >> 16);
          //          argbValues[x + offset + 2] = (byte)(pixel >> 8);
          //          argbValues[x + offset + 3] = (byte)(0xFF);
          //      }
          //  }

          //  System.Runtime.InteropServices.Marshal.Copy(argbValues, 0, ptr, bytes);
          //  bitmap.UnlockBits(bmpData);
          //  return bitmap;

        }

        /// <summary>
        /// Apply a transformation function to the current image.
        /// </summary>
        /// <param name="transform">
        /// A transformation function taking a current pixel value and returning a new value.
        /// </param>
        public void SetPixels(Func<uint, uint> transform) { SetPixels((x, y, value) => transform(value)); }

        /// <summary>
        /// Apply a transformation function to the current image.
        /// </summary>
        /// <param name="transform">A transformation function taking
        /// <c>x</c>, <c>y</c> and <c>pixel</c> parameters and
        /// returning a new <c>pixel</c> value.</param>
        public void SetPixels(Func<int, int, uint, uint> transform)
        {
            DoAction((x, y, value) => SetPixel(x, y, transform(x, y, value)));

            if (SupportsHSV) UpdateHSVData();
        }

        /// <summary>
        /// Perform an action on each pixel in the current image.
        /// </summary>
        /// <param name="action">An action taking <c>x</c>, <c>y</c> and <c>pixel</c> parameters.</param>
        public void DoAction(Action<int, int, uint> action)
        {
            Enumerable.Range(0, ddsHeader.m_width).AsParallel()
                .ForAll(x => Enumerable.Range(0, ddsHeader.m_height).AsParallel()
                    .ForAll(y => action(x, y, GetPixel(x, y))));
        }

        private uint GetPixel(int x, int y) { return currentImage[y * ddsHeader.m_width + x]; }
        private void SetPixel(int x, int y, uint value) { currentImage[y * ddsHeader.m_width + x] = value; }

        /// <summary>
        /// The current image.
        /// </summary>
        public Bitmap Image { get { return GetImage(true, true, true, true, false); } }

        /// <summary>
        /// Get a new DdsFile of the given size based on the current image.
        /// </summary>
        /// <param name="size">The new size.</param>
        /// <returns>A new DdsFile of the given size based on the current image.</returns>
        public DdsFile Resize(Size size)
        {
            DdsFile ddsFile = new DdsFile();
            ddsFile.CreateImage(this, size.Width, size.Height, SupportsHSV);
            return ddsFile;
        }

        /// <summary>
        /// The image size.
        /// </summary>
        public Size Size { get { return new Size(ddsHeader.m_width, ddsHeader.m_height); } }

        /// <summary>
        /// The HSVShift applied to the current image, when supported.
        /// </summary>
        /// <seealso cref="SupportsHSV"/>
        public HSVShift HSVShift { get { return hsvShift; } set { hsvShift = value; } }

        /// <summary>
        /// True if the image is prepared to process HSV requests.
        /// </summary>
        public bool SupportsHSV
        {
            get { return hsvData != null; }
            set { if (value != SupportsHSV) { if (value) UpdateHSVData(); else hsvData = null; } }
        }

        void UpdateHSVData() { hsvData = ColorHSVA.ToArrayColorHSVA(currentImage); }

        //----------------------------------------------------------------

        /// <summary>
        /// Set the colour of the image based on the channels in the <paramref name="mask"/>.
        /// </summary>
        /// <param name="mask">The <see cref="System.IO.Stream"/> containing the DDS image to use as a mask.</param>
        /// <param name="ch1Colour">(Nullable) ARGB colour to the image when the first channel of the mask is active.</param>
        /// <param name="ch2Colour">(Nullable) ARGB colour to the image when the second channel of the mask is active.</param>
        /// <param name="ch3Colour">(Nullable) ARGB colour to the image when the third channel of the mask is active.</param>
        /// <param name="ch4Colour">(Nullable) ARGB colour to the image when the fourth channel of the mask is active.</param>
        public void MaskedSetColour(DdsFile mask, uint? ch1Colour, uint? ch2Colour, uint? ch3Colour, uint? ch4Colour)
        {

            maskInEffect = maskInEffect || ch1Colour.HasValue || ch2Colour.HasValue || ch3Colour.HasValue || ch4Colour.HasValue;

            if (!maskInEffect) return;

            if (ch1Colour.HasValue) MaskedSetColour(mask, x => x.R() > 0, ch1Colour.Value);
            if (ch2Colour.HasValue) MaskedSetColour(mask, x => x.G() > 0, ch2Colour.Value);
            if (ch3Colour.HasValue) MaskedSetColour(mask, x => x.B() > 0, ch3Colour.Value);
            if (ch4Colour.HasValue) MaskedSetColour(mask, x => x.A() > 0, ch4Colour.Value);

            if (SupportsHSV) UpdateHSVData();
        }
        void MaskedSetColour(DdsFile mask, Channel channel, uint argb)
        {
            MaskedApply(this.currentImage, this.Size, mask.currentImage, mask.Size, channel, (x, y) => argb);
        }

        /// <summary>
        /// Use the <paramref name="mask"/> to apply the DDS image supplied.
        /// </summary>
        /// <param name="mask">Determines to which areas each DDS image is applied.</param>
        /// <param name="ch1DdsFile">DDS image applied to <paramref name="mask"/> channel 1 area.</param>
        /// <param name="ch2DdsFile">DDS image applied to <paramref name="mask"/> channel 2 area.</param>
        /// <param name="ch3DdsFile">DDS image applied to <paramref name="mask"/> channel 3 area.</param>
        /// <param name="ch4DdsFile">DDS image applied to <paramref name="mask"/> channel 4 area.</param>
        public void MaskedApplyImage(DdsFile mask, DdsFile ch1DdsFile, DdsFile ch2DdsFile, DdsFile ch3DdsFile, DdsFile ch4DdsFile)
        {
            this.MaskedApplyImage(mask,
                (ch1DdsFile == null) ? null : ch1DdsFile.currentImage, (ch1DdsFile == null) ? Size.Empty : ch1DdsFile.Size,
                (ch2DdsFile == null) ? null : ch2DdsFile.currentImage, (ch2DdsFile == null) ? Size.Empty : ch2DdsFile.Size,
                (ch3DdsFile == null) ? null : ch3DdsFile.currentImage, (ch3DdsFile == null) ? Size.Empty : ch3DdsFile.Size,
                (ch4DdsFile == null) ? null : ch4DdsFile.currentImage, (ch4DdsFile == null) ? Size.Empty : ch4DdsFile.Size);
        }

        /// <summary>
        /// Use the <paramref name="mask"/> to apply the supplied images.
        /// </summary>
        /// <param name="mask">Determines to which areas each image is applied.</param>
        /// <param name="ch1Image">Image applied to <paramref name="mask"/> channel 1 area.</param>
        /// <param name="ch2Image">Image applied to <paramref name="mask"/> channel 2 area.</param>
        /// <param name="ch3Image">Image applied to <paramref name="mask"/> channel 3 area.</param>
        /// <param name="ch4Image">Image applied to <paramref name="mask"/> channel 4 area.</param>
        public void MaskedApplyImage(DdsFile mask, Image ch1Image, Image ch2Image, Image ch3Image, Image ch4Image)
		{
            this.MaskedApplyImage(mask,
                (ch1Image == null) ? null : new Bitmap(ch1Image),
                (ch2Image == null) ? null : new Bitmap(ch2Image),
                (ch3Image == null) ? null : new Bitmap(ch3Image),
                (ch4Image == null) ? null : new Bitmap(ch4Image));
        }

        /// <summary>
        /// Use the <paramref name="mask"/> to apply the supplied bitmaps.
        /// </summary>
        /// <param name="mask">Determines to which areas each image is applied.</param>
        /// <param name="ch1Bitmap">Bitmap applied to <paramref name="mask"/> channel 1 area.</param>
        /// <param name="ch2Bitmap">Bitmap applied to <paramref name="mask"/> channel 2 area.</param>
        /// <param name="ch3Bitmap">Bitmap applied to <paramref name="mask"/> channel 3 area.</param>
        /// <param name="ch4Bitmap">Bitmap applied to <paramref name="mask"/> channel 4 area.</param>
        public void MaskedApplyImage(DdsFile mask, Bitmap ch1Bitmap, Bitmap ch2Bitmap, Bitmap ch3Bitmap, Bitmap ch4Bitmap)
		{
            this.MaskedApplyImage(mask,
                (ch1Bitmap == null) ? null : ch1Bitmap.ToARGBData(), (ch1Bitmap == null) ? Size.Empty : ch1Bitmap.Size,
                (ch2Bitmap == null) ? null : ch2Bitmap.ToARGBData(), (ch2Bitmap == null) ? Size.Empty : ch2Bitmap.Size,
                (ch3Bitmap == null) ? null : ch3Bitmap.ToARGBData(), (ch3Bitmap == null) ? Size.Empty : ch3Bitmap.Size,
                (ch4Bitmap == null) ? null : ch4Bitmap.ToARGBData(), (ch4Bitmap == null) ? Size.Empty : ch4Bitmap.Size);
        }

        void MaskedApplyImage(DdsFile mask,
            uint[] ch1image, Size ch1imageSize, uint[] ch2image, Size ch2imageSize, uint[] ch3image, Size ch3imageSize, uint[] ch4image, Size ch4imageSize)
        {

            maskInEffect = maskInEffect || ch1image != null || ch2image != null || ch3image != null || ch4image != null;

            if (!maskInEffect) return;

            if (ch1image != null) MaskedApplyImage(mask, x => x.R() > 0, ch1image, ch1imageSize);
            if (ch2image != null) MaskedApplyImage(mask, x => x.G() > 0, ch2image, ch2imageSize);
            if (ch3image != null) MaskedApplyImage(mask, x => x.B() > 0, ch3image, ch3imageSize);
            if (ch4image != null) MaskedApplyImage(mask, x => x.A() > 0, ch4image, ch4imageSize);

            if (SupportsHSV) UpdateHSVData();
        }
        void MaskedApplyImage(DdsFile mask, Channel channel, uint[] image, Size imageSize)
        {
            MaskedApply(this.currentImage, this.Size, mask.currentImage, mask.Size, channel, (x, y) => image[((y % imageSize.Height) * imageSize.Width) + (x % imageSize.Width)]);
        }

        /// <summary>
        /// Delegate to determine whether a channel of a given UInt32 ARGB format value is active.
        /// </summary>
        /// <param name="argb">The UInt32 ARGB format value.</param>
        /// <returns>True if the channel is active, otherwise false.</returns>
        delegate bool Channel(uint argb);

        /// <summary>
        /// Return the UInt32 ARGB format pixel value for a given X/Y coordinate.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns>A UInt32 ARGB format pixel value.</returns>
        delegate uint ARGBAt(int x, int y);

        void MaskedApply(uint[] image, Size imageSize, uint[] mask, Size maskSize, Channel channel, ARGBAt argbAt)
        {
            for (int y = 0; y < imageSize.Height; y++)
            {
                int imageOffset = y * imageSize.Width;
                int maskOffset = (y % maskSize.Height) * maskSize.Width;

                for (int x = 0; x < imageSize.Width; x++)
                {
                    uint imagePixel = image[imageOffset + x];
                    uint maskPixel = mask[maskOffset + x % maskSize.Width];
                    if (channel(maskPixel)) image[imageOffset + x] = argbAt(x, y);
                }
            }
        }

        //----------------------------------------------------------------

        bool maskInEffect = false;

        /// <summary>
        /// Clears a previously-applied HSVShift mask.
        /// </summary>
        public void ClearMask()
        {
            if (!maskInEffect) return;
            currentImage = (uint[])baseImage.Clone();
            if (SupportsHSV) UpdateHSVData();
            maskInEffect = false;
        }

        /// <summary>
        /// Apply <see cref="HSVShift"/> values to this DDS image based on the
        /// channels in the <paramref name="mask"/>.
        /// </summary>
        /// <param name="mask">A DDS image file, each colourway acting as a mask channel.</param>
        /// <param name="ch1Shift">A shift to apply to the image when the first channel of the mask is active.</param>
        /// <param name="ch2Shift">A shift to apply to the image when the second channel of the mask is active.</param>
        /// <param name="ch3Shift">A shift to apply to the image when the third channel of the mask is active.</param>
        /// <param name="ch4Shift">A shift to apply to the image when the fourth channel of the mask is active.</param>
        public void MaskedHSVShift(DdsFile mask, HSVShift ch1Shift, HSVShift ch2Shift, HSVShift ch3Shift, HSVShift ch4Shift)
        {
            if (!SupportsHSV) return;

            maskInEffect = maskInEffect || !ch1Shift.IsEmpty || !ch2Shift.IsEmpty || !ch3Shift.IsEmpty || !ch4Shift.IsEmpty;

            if (!maskInEffect) return;

            if (!ch1Shift.IsEmpty) MaskedHSVShift(mask, hsvData, x => x.R() > 0, ch1Shift);
            if (!ch2Shift.IsEmpty) MaskedHSVShift(mask, hsvData, x => x.G() > 0, ch2Shift);
            if (!ch3Shift.IsEmpty) MaskedHSVShift(mask, hsvData, x => x.B() > 0, ch3Shift);
            if (!ch4Shift.IsEmpty) MaskedHSVShift(mask, hsvData, x => x.A() > 0, ch4Shift);

            currentImage = ColorHSVA.ToArrayARGB(hsvData);
        }

        /// <summary>
        /// Apply <see cref="HSVShift"/> values to this DDS image based on the
        /// channels in the <paramref name="mask"/>.
        /// Each channel of the mask acts independently, in order "R", "G", "B", "A".
        /// </summary>
        /// <param name="mask">A DDS image file, each colourway acting as a mask channel.</param>
        /// <param name="ch1Shift">A shift to apply to the image when the first channel of the mask is active.</param>
        /// <param name="ch2Shift">A shift to apply to the image when the second channel of the mask is active.</param>
        /// <param name="ch3Shift">A shift to apply to the image when the third channel of the mask is active.</param>
        /// <param name="ch4Shift">A shift to apply to the image when the fourth channel of the mask is active.</param>
        public void MaskedHSVShiftNoBlend(DdsFile mask, HSVShift ch1Shift, HSVShift ch2Shift, HSVShift ch3Shift, HSVShift ch4Shift)
        {
            if (!SupportsHSV) return;

            maskInEffect = maskInEffect || !ch1Shift.IsEmpty || !ch2Shift.IsEmpty || !ch3Shift.IsEmpty || !ch4Shift.IsEmpty;

            if (!maskInEffect) return;

            ColorHSVA[] result = new ColorHSVA[hsvData.Length];
            Array.Copy(hsvData, 0, result, 0, result.Length);

            if (!ch1Shift.IsEmpty) MaskedHSVShift(mask, result, x => x.R() > 0, ch1Shift);
            if (!ch2Shift.IsEmpty) MaskedHSVShift(mask, result, x => x.G() > 0, ch2Shift);
            if (!ch3Shift.IsEmpty) MaskedHSVShift(mask, result, x => x.B() > 0, ch3Shift);
            if (!ch4Shift.IsEmpty) MaskedHSVShift(mask, result, x => x.A() > 0, ch4Shift);

            hsvData = result;
            currentImage = ColorHSVA.ToArrayARGB(hsvData);
        }

        void MaskedHSVShift(DdsFile mask, ColorHSVA[] result, Channel channel, HSVShift hsvShift)
        {
            for (int y = 0; y < this.Size.Height; y++)
            {
                int imageOffset = y * this.Size.Width;
                int maskOffset = (y % mask.Size.Height) * mask.Size.Width;

                for (int x = 0; x < this.Size.Width; x++)
                {
                    uint maskPixel = mask.currentImage[maskOffset + x % mask.Size.Width];
                    if (channel(maskPixel)) result[imageOffset + x] = hsvData[imageOffset + x].HSVShift(hsvShift);
                }
            }
        }
    }

    /// <summary>
    /// Convert a <see cref="T:Color"/> value into a UInt32 ARGB format pixel value.
    /// </summary>
    /// <param name="color">A <see cref="T:Color"/> value</param>
    /// <returns>A UInt32 ARGB format pixel value.</returns>
    public delegate uint ARGBToPixel(Color color);

    /// <summary>
    /// Extensions for working with bitmap images.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Convert an array of UInt32 ARGB elements into a <see cref="T:Bitmap"/>.
        /// </summary>
        /// <param name="argbData">The array of UInt32 ARGB elements to decode.</param>
        /// <param name="size">The size of the encoded image.</param>
        /// <returns>The decoded image.</returns>
        public static Bitmap ToBitmap(this uint[] argbData, Size size) { return argbData.ToBitmap(size.Width, size.Height); }
        /// <summary>
        /// Convert an array of UInt32 ARGB elements into a <see cref="T:Bitmap"/>.
        /// </summary>
        /// <param name="argbData">The array of UInt32 ARGB elements to decode.</param>
        /// <param name="width">The width of the encoded image.</param>
        /// <param name="height">The height of the encoded image.</param>
        /// <returns>The decoded bitmap image.</returns>
        public static Bitmap ToBitmap(this uint[] argbData, int width, int height)
        {
            Bitmap res = new Bitmap(width, height, Imaging.PixelFormat.Format32bppArgb);
            for (int y = 0; y < height; y++)
            {
                int offset = y * width;
                for (int x = 0; x < width; x++)
                    res.SetPixel(x, y, Color.FromArgb((int)argbData[offset + x]));
            }
            return res;
        }

        /// <summary>
        /// Converts a <see cref="T:Image"/> into an array of UInt32 ARGB elements.
        /// </summary>
        /// <param name="image">The image to encode.</param>
        /// <returns>An array of UInt32 ARGB elements.</returns>
        public static uint[] ToARGBData(this Image image) { return new Bitmap(image).ToARGBData(); }
        /// <summary>
        /// Converts a <see cref="T:Bitmap"/> into an array of UInt32 ARGB elements.
        /// </summary>
        /// <param name="bitmap">The bitmap image to encode.</param>
        /// <returns>An array of UInt32 ARGB elements.</returns>
        public static uint[] ToARGBData(this Bitmap bitmap) { return bitmap.ToPixelData(x => (uint)x.ToArgb()); }

        /// <summary>
        /// Converts a <see cref="T:Bitmap"/> into a pixel data array,
        /// using the provided encoder.
        /// </summary>
        /// <param name="bitmap">The bitmap image to encode.</param>
        /// <param name="encoder">The method to invoke to encode bitmap <see cref="T:Color"/> pixels.</param>
        /// <returns>An array of uint elements containing the encoded pixel data.</returns>
        public static uint[] ToPixelData(this Bitmap bitmap, ARGBToPixel encoder)
        {
            uint[] pixelData = new uint[bitmap.Width * bitmap.Height];
            for (int y = 0; y < bitmap.Height; y++)
            {
                int offset = y * bitmap.Width;
                for (int x = 0; x < bitmap.Width; x++)
                    pixelData[offset + x] = encoder(bitmap.GetPixel(x, y));
            }
            return pixelData;
        }

        /// <summary>
        /// Extract the alpha channel from an ARGB format UInt32 value.
        /// </summary>
        /// <param name="argb">An ARGB format UInt32 value.</param>
        /// <returns>The alpha channel extracted.</returns>
        public static uint A(this uint argb) { return (argb & 0xFF000000) >> 24; }
        /// <summary>
        /// Extract the red channel from an ARGB format UInt32 value.
        /// </summary>
        /// <param name="argb">An ARGB format UInt32 value.</param>
        /// <returns>The red channel extracted.</returns>
        public static uint R(this uint argb) { return (argb & 0x00FF0000) >> 16; }
        /// <summary>
        /// Extract the green channel from an ARGB format UInt32 value.
        /// </summary>
        /// <param name="argb">An ARGB format UInt32 value.</param>
        /// <returns>The green channel extracted.</returns>
        public static uint G(this uint argb) { return (argb & 0x0000FF00) >> 8; }
        /// <summary>
        /// Extract the blue channel from an ARGB format UInt32 value.
        /// </summary>
        /// <param name="argb">An ARGB format UInt32 value.</param>
        /// <returns>The blue channel extracted.</returns>
        public static uint B(this uint argb) { return (argb & 0x000000FF) >> 0; }
    }
}