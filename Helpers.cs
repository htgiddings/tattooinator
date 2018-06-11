using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TxtcResource;
using s3pi.Interfaces;
using Xmods.FNV;

namespace Tattooinator
{
    public class Preset
    {
        Color rchannel, gchannel, bchannel, achannel;
        public uint rChannelHue { get { return rchannel.Hue; } set { rchannel.Hue = value; } }
        public uint gChannelHue { get { return gchannel.Hue; } set { gchannel.Hue = value; } }
        public uint bChannelHue { get { return bchannel.Hue; } set { bchannel.Hue = value; } }
        public uint aChannelHue { get { return achannel.Hue; } set { achannel.Hue = value; } }
        public uint[] HuesRGBA { get { return new uint[4] { rchannel.Hue, gchannel.Hue, bchannel.Hue, achannel.Hue }; } }
        public Color rChannelColor { get { return rchannel; } set { rchannel = value; } }
        public Color gChannelColor { get { return gchannel; } set { gchannel = value; } }
        public Color bChannelColor { get { return bchannel; } set { bchannel = value; } }
        public Color aChannelColor { get { return achannel; } set { achannel = value; } }
        public Color[] ColorsRGB { get { return new Color[4] { rchannel, gchannel, bchannel, achannel }; } }
        public float[] rChannelPercents { get { return new float[] { rchannel.Red / 255f, rchannel.Green / 255f, rchannel.Blue / 255f, rchannel.Alpha / 255f }; } }
        public float[] gChannelPercents { get { return new float[] { gchannel.Red / 255f, gchannel.Green / 255f, gchannel.Blue / 255f, gchannel.Alpha / 255f }; } }
        public float[] bChannelPercents { get { return new float[] { bchannel.Red / 255f, bchannel.Green / 255f, bchannel.Blue / 255f, bchannel.Alpha / 255f }; } }
        public float[] aChannelPercents { get { return new float[] { achannel.Red / 255f, achannel.Green / 255f, achannel.Blue / 255f, achannel.Alpha / 255f }; } }
        public string rChannelPercentString { get { return StringMe(rChannelPercents); } }
        public string gChannelPercentString { get { return StringMe(gChannelPercents); } }
        public string bChannelPercentString { get { return StringMe(bChannelPercents); } }
        public string aChannelPercentString { get { return StringMe(aChannelPercents); } }
        private string StringMe(float[] percents)
        {
            return percents[0].ToString("F4", System.Globalization.CultureInfo.InvariantCulture) + ", " +
                   percents[1].ToString("F4", System.Globalization.CultureInfo.InvariantCulture) + ", " +
                   percents[2].ToString("F4", System.Globalization.CultureInfo.InvariantCulture) + ", " +
                   percents[3].ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
        }
        public Preset()
        {
            rchannel = new Color();
            gchannel = new Color();
            bchannel = new Color();
            achannel = new Color();
        }
        public Preset(uint R, uint G, uint B, uint A)
        {
            rchannel = new Color(R);
            gchannel = new Color(G);
            bchannel = new Color(B);
            achannel = new Color(A);
        }
    }
    public class Color
    {
        byte r, g, b, a;
        public byte Red { get { return r; } set { r = value; } }
        public byte Green { get { return g; } set { g = value; } }
        public byte Blue { get { return b; } set { b = value; } }
        public byte Alpha { get { return a; } set { a = value; } }
        public byte[] RGB { get { return new byte[3] { r, g, b }; } }
        public byte[] RGBA { get { return new byte[4] { r, g, b, a }; } }
        public uint Hue
        {
            get
            { return (uint)((a << 24) + (r << 16) + (g << 8) + b); }
            set
            {
                a = (byte)((value & 0xFF000000) >> 24);
                r = (byte)((value & 0x00FF0000) >> 16);
                g = (byte)((value & 0x0000FF00) >> 8);
                b = (byte)(value & 0x000000FF);
            }
        }
        public System.Drawing.Color SystemColor { get { return System.Drawing.Color.FromArgb((255 << 24) + (r << 16) + (g << 8) + b); } }

        internal Color()
        {
            r = 0;
            g = 0;
            b = 0;
            a = 0;
        }
        internal Color(byte R, byte G, byte B, byte A)
        {
            r = R;
            g = G;
            b = B;
            a = A;
        }
        internal Color(byte R, byte G, byte B) : this(R, G, B, 255) { }
        internal Color(byte[] rgba)
        {
            r = rgba[0];
            g = rgba[1];
            b = rgba[2];
            a = rgba[3];
        }
        internal Color(uint hue)
        {
            a = (byte)((hue & 0xFF000000) >> 24);
            r = (byte)((hue & 0x00FF0000) >> 16);
            g = (byte)((hue & 0x0000FF00) >> 8);
            b = (byte)(hue & 0x000000FF);
        }
        internal Color(string percents)
        {
            string[] str = percents.Split(new char[] { ',' });
            byte[] tmp = new byte[str.Length];
            try
            {
                for (int i = 0; i < str.Length; i++)
                {
                    tmp[i] = (byte)(float.Parse(str[i]) * 255);
                }
                r = tmp[0];
                g = tmp[1];
                b = tmp[2];
                a = tmp[3];
            }
            catch
            {
                throw new ApplicationException("Invalid preset color definition!");
            }
        }
    }

    public enum ResourceTypes : uint
    {
        BBLN = 0x062C8204U,
        CASP = 0x034AEECBU,
        DDS = 0x00B2D882U,
        KEY = 0x0166038CU,
        THUM = 0x626F60CEU,
        TXTC = 0x033A1435U,
        VPXY = 0x736884F1U,
        XML = 0x0333406CU
    }

    public static class PixelBangers
    {
        public static uint TatPreview(uint pixel, byte[] backgroundRGB, Color presetR, Color presetG, Color presetB, Color presetA)
        {
            Color pixelColor = new Color(pixel);
            uint[] rgbOut = new uint[3];

            for (int i = 0; i < 3; i++)
            {
                double rtemp = backgroundRGB[i] * ((255.0 - pixelColor.Red) / 255.0) + presetR.RGBA[i] * (pixelColor.Red / 255.0);
                double rtemp1 = rtemp * ((255.0 - pixelColor.Green) / 255.0) + presetG.RGBA[i] * (pixelColor.Green / 255.0);
                double rtemp2 = rtemp1 * ((255.0 - pixelColor.Blue) / 255.0) + presetB.RGBA[i] * (pixelColor.Blue / 255.0);
                rgbOut[i] = (uint)Math.Round(rtemp2 * ((255.0 - pixelColor.Alpha) / 255.0) + presetA.RGBA[i] * (pixelColor.Alpha / 255.0));
            }

            return (uint)((255 << 24) + (rgbOut[0] << 16) + (rgbOut[1] << 8) + (rgbOut[2]));
        }

        public static uint BrightnessContrast(uint pixel, int brightAmount, int contrastAmount, bool[] updateChannels)
        {
            Color pixelColor = new Color(pixel);
            byte[] p = pixelColor.RGBA;
            for (int i = 0; i < p.Length; i++)
            {
                if (updateChannels[i])
                {
                    //contrast
                    float d = ((float)p[i] - 128f) * (1f + (contrastAmount / 100f));
                    p[i] = (byte)Math.Max(0, Math.Min(255, 128 + d));
                    //brightness
                    if (p[i] > 0) p[i] = (byte)Math.Max(0, Math.Min(255, p[i] + brightAmount));
                }
            }
            Color newColor = new Color(p[0], p[1], p[2], p[3]);
            return (newColor.Hue);
        }
    }

    public class TattooTextureCompositor
    {
        public static TxtcResource.TxtcResource TattooTxtc(TGIBlock tatTGI)
        {
            TxtcResource.TxtcResource txtc = new TxtcResource.TxtcResource(0, null);
            txtc.Root.Version = 8;
            txtc.Root.DataType = TxtcResource.TxtcResource.DataTypeFlags.Body;

            TxtcResource.TxtcResource.EntryBlockList entryBlockList = new TxtcResource.TxtcResource.EntryBlockList(null);

            TxtcResource.TxtcResource.EntryList entryList0 = new TxtcResource.TxtcResource.EntryList(null);
            entryList0.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.ID, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.StepType.SetTarget));
            entryList0.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.UIVisible, 0, 0));
            entryList0.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.MinShaderModel, 0, 0));
            entryList0.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SkipShaderModel, 0, (int)TxtcResource.TxtcResource.EntryInt32.ShaderModel.SM_Highest));
            entryList0.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.MinDetailLevel, 0, 0xFFFFFFFF));
            entryList0.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.SkipDetailLevel, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.DetailLevel.Lowest));
            entryList0.Add(new TxtcResource.TxtcResource.EntryString(0, null, TxtcResource.TxtcResource.EntryString.StringProperties.Description, 0, "Texture Step"));
            entryList0.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.RenderTarget, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.RenderTarget.RenderTarget_B));
            TxtcResource.TxtcResource.EntryBlock entBlock0 = new TxtcResource.TxtcResource.EntryBlock(0, null);
            entBlock0.Entries = entryList0;
            entryBlockList.Add(entBlock0);

            TxtcResource.TxtcResource.EntryList entryList1 = new TxtcResource.TxtcResource.EntryList(null);
            entryList1.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.ID, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.StepType.ColorFill));
            entryList1.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.UIVisible, 0, 0));
            entryList1.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.MinShaderModel, 0, 0));
            entryList1.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SkipShaderModel, 0, (int)TxtcResource.TxtcResource.EntryInt32.ShaderModel.SM_Highest));
            entryList1.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.MinDetailLevel, 0, 0xFFFFFFFF));
            entryList1.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.SkipDetailLevel, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.DetailLevel.Lowest));
            entryList1.Add(new TxtcResource.TxtcResource.EntryString(0, null, TxtcResource.TxtcResource.EntryString.StringProperties.Description, 0, "Texture Step"));
            entryList1.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.ColorWrite, 0, (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Alpha +
                (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Blue + (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Green + (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Red));
            entryList1.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.DestinationRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryList1.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableBlending, 0, 0));
            entryList1.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.Color, 0, 0));
            TxtcResource.TxtcResource.EntryBlock entBlock1 = new TxtcResource.TxtcResource.EntryBlock(0, null);
            entBlock1.Entries = entryList1;
            entryBlockList.Add(entBlock1);

            TxtcResource.TxtcResource.EntryList entryList2 = new TxtcResource.TxtcResource.EntryList(null);
            entryList2.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.ID, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.StepType.ChannelSelect));
            entryList2.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.UIVisible, 0, 0));
            entryList2.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.MinShaderModel, 0, 0));
            entryList2.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SkipShaderModel, 0, (int)TxtcResource.TxtcResource.EntryInt32.ShaderModel.SM_Highest));
            entryList2.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.MinDetailLevel, 0, 0xFFFFFFFF));
            entryList2.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.SkipDetailLevel, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.DetailLevel.Lowest));
            entryList2.Add(new TxtcResource.TxtcResource.EntryString(0, null, TxtcResource.TxtcResource.EntryString.StringProperties.Description, 0, "Texture Step"));
            entryList2.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.ColorWrite, 0, (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Alpha));
            entryList2.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.DestinationRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryList2.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableBlending, 0, 0));
            entryList2.Add(new TxtcResource.TxtcResource.EntrySingle(0, null, TxtcResource.TxtcResource.EntrySingle.SingleProperties.Rotation, 0, 0.0f));
            entryList2.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.SourceRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryList2.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableFiltering, 0, 1));
            entryList2.Add(new TxtcResource.TxtcResource.EntryTGIIndex(0, null, TxtcResource.TxtcResource.EntryTGIIndex.TGIIndexProperties.ImageKey, 0, 0));
            entryList2.Add(new TxtcResource.TxtcResource.EntryVector(0, null, TxtcResource.TxtcResource.EntryVector.VectorProperties.ChannelSelect, 0, new float[] { 1.0f, 0.0f, 0.0f, 0.0f }));
            TxtcResource.TxtcResource.EntryBlock entBlock2 = new TxtcResource.TxtcResource.EntryBlock(0, null);
            entBlock2.Entries = entryList2;
            entryBlockList.Add(entBlock2);

            TxtcResource.TxtcResource.EntryList entryList3 = new TxtcResource.TxtcResource.EntryList(null);
            entryList3.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.ID, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.StepType.ColorFill));
            entryList3.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.UIVisible, 0, 0));
            entryList3.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.MinShaderModel, 0, 0));
            entryList3.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SkipShaderModel, 0, (int)TxtcResource.TxtcResource.EntryInt32.ShaderModel.SM_Highest));
            entryList3.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.MinDetailLevel, 0, 0xFFFFFFFF));
            entryList3.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.SkipDetailLevel, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.DetailLevel.Lowest));
            entryList3.Add(new TxtcResource.TxtcResource.EntryString(0, null, TxtcResource.TxtcResource.EntryString.StringProperties.Description, 0, "Texture Step"));
            entryList3.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.ColorWrite, 0, 
                (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Blue + (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Green + (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Red));
            entryList3.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.DestinationRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryList3.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableBlending, 0, 1));
            entryList3.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SourceBlend, 0, (int)TxtcResource.TxtcResource.EntryInt32.Blend.DestinationAlpha));
            entryList3.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.DestinationBlend, 0, (int)TxtcResource.TxtcResource.EntryInt32.Blend.InverseDestinationAlpha));
            entryList3.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.Color, 0, 0xFFED88F7));
            TxtcResource.TxtcResource.EntryBlock entBlock3 = new TxtcResource.TxtcResource.EntryBlock(0, null);
            entBlock3.Entries = entryList3;
            entryBlockList.Add(entBlock3);

            TxtcResource.TxtcResource.EntryList entryList4 = new TxtcResource.TxtcResource.EntryList(null);
            entryList4.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.ID, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.StepType.ChannelSelect));
            entryList4.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.UIVisible, 0, 0));
            entryList4.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.MinShaderModel, 0, 0));
            entryList4.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SkipShaderModel, 0, (int)TxtcResource.TxtcResource.EntryInt32.ShaderModel.SM_Highest));
            entryList4.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.MinDetailLevel, 0, 0xFFFFFFFF));
            entryList4.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.SkipDetailLevel, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.DetailLevel.Lowest));
            entryList4.Add(new TxtcResource.TxtcResource.EntryString(0, null, TxtcResource.TxtcResource.EntryString.StringProperties.Description, 0, "Texture Step"));
            entryList4.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.ColorWrite, 0, (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Alpha));
            entryList4.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.DestinationRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryList4.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableBlending, 0, 0));
            entryList4.Add(new TxtcResource.TxtcResource.EntrySingle(0, null, TxtcResource.TxtcResource.EntrySingle.SingleProperties.Rotation, 0, 0.0f));
            entryList4.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.SourceRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryList4.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableFiltering, 0, 1));
            entryList4.Add(new TxtcResource.TxtcResource.EntryTGIIndex(0, null, TxtcResource.TxtcResource.EntryTGIIndex.TGIIndexProperties.ImageKey, 0, 0));
            entryList4.Add(new TxtcResource.TxtcResource.EntryVector(0, null, TxtcResource.TxtcResource.EntryVector.VectorProperties.ChannelSelect, 0, new float[] { 0.0f, 1.0f, 0.0f, 0.0f }));
            TxtcResource.TxtcResource.EntryBlock entBlock4 = new TxtcResource.TxtcResource.EntryBlock(0, null);
            entBlock4.Entries = entryList4;
            entryBlockList.Add(entBlock4);

            TxtcResource.TxtcResource.EntryList entryList5 = new TxtcResource.TxtcResource.EntryList(null);
            entryList5.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.ID, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.StepType.ColorFill));
            entryList5.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.UIVisible, 0, 0));
            entryList5.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.MinShaderModel, 0, 0));
            entryList5.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SkipShaderModel, 0, (int)TxtcResource.TxtcResource.EntryInt32.ShaderModel.SM_Highest));
            entryList5.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.MinDetailLevel, 0, 0xFFFFFFFF));
            entryList5.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.SkipDetailLevel, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.DetailLevel.Lowest));
            entryList5.Add(new TxtcResource.TxtcResource.EntryString(0, null, TxtcResource.TxtcResource.EntryString.StringProperties.Description, 0, "Texture Step"));
            entryList5.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.ColorWrite, 0,
                (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Blue + (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Green + (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Red));
            entryList5.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.DestinationRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryList5.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableBlending, 0, 1));
            entryList5.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SourceBlend, 0, (int)TxtcResource.TxtcResource.EntryInt32.Blend.DestinationAlpha));
            entryList5.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.DestinationBlend, 0, (int)TxtcResource.TxtcResource.EntryInt32.Blend.InverseDestinationAlpha));
            entryList5.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.Color, 0, 0xFF845DB7));
            TxtcResource.TxtcResource.EntryBlock entBlock5 = new TxtcResource.TxtcResource.EntryBlock(0, null);
            entBlock5.Entries = entryList5;
            entryBlockList.Add(entBlock5);

            TxtcResource.TxtcResource.EntryList entryList6 = new TxtcResource.TxtcResource.EntryList(null);
            entryList6.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.ID, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.StepType.ChannelSelect));
            entryList6.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.UIVisible, 0, 0));
            entryList6.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.MinShaderModel, 0, 0));
            entryList6.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SkipShaderModel, 0, (int)TxtcResource.TxtcResource.EntryInt32.ShaderModel.SM_Highest));
            entryList6.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.MinDetailLevel, 0, 0xFFFFFFFF));
            entryList6.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.SkipDetailLevel, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.DetailLevel.Lowest));
            entryList6.Add(new TxtcResource.TxtcResource.EntryString(0, null, TxtcResource.TxtcResource.EntryString.StringProperties.Description, 0, "Texture Step"));
            entryList6.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.ColorWrite, 0, (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Alpha));
            entryList6.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.DestinationRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryList6.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableBlending, 0, 0));
            entryList6.Add(new TxtcResource.TxtcResource.EntrySingle(0, null, TxtcResource.TxtcResource.EntrySingle.SingleProperties.Rotation, 0, 0.0f));
            entryList6.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.SourceRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryList6.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableFiltering, 0, 1));
            entryList6.Add(new TxtcResource.TxtcResource.EntryTGIIndex(0, null, TxtcResource.TxtcResource.EntryTGIIndex.TGIIndexProperties.ImageKey, 0, 0));
            entryList6.Add(new TxtcResource.TxtcResource.EntryVector(0, null, TxtcResource.TxtcResource.EntryVector.VectorProperties.ChannelSelect, 0, new float[] { 0.0f, 0.0f, 1.0f, 0.0f }));
            TxtcResource.TxtcResource.EntryBlock entBlock6 = new TxtcResource.TxtcResource.EntryBlock(0, null);
            entBlock6.Entries = entryList6;
            entryBlockList.Add(entBlock6);

            TxtcResource.TxtcResource.EntryList entryList7 = new TxtcResource.TxtcResource.EntryList(null);
            entryList7.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.ID, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.StepType.ColorFill));
            entryList7.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.UIVisible, 0, 0));
            entryList7.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.MinShaderModel, 0, 0));
            entryList7.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SkipShaderModel, 0, (int)TxtcResource.TxtcResource.EntryInt32.ShaderModel.SM_Highest));
            entryList7.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.MinDetailLevel, 0, 0xFFFFFFFF));
            entryList7.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.SkipDetailLevel, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.DetailLevel.Lowest));
            entryList7.Add(new TxtcResource.TxtcResource.EntryString(0, null, TxtcResource.TxtcResource.EntryString.StringProperties.Description, 0, "Texture Step"));
            entryList7.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.ColorWrite, 0,
                (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Blue + (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Green + (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Red));
            entryList7.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.DestinationRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryList7.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableBlending, 0, 1));
            entryList7.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SourceBlend, 0, (int)TxtcResource.TxtcResource.EntryInt32.Blend.DestinationAlpha));
            entryList7.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.DestinationBlend, 0, (int)TxtcResource.TxtcResource.EntryInt32.Blend.InverseDestinationAlpha));
            entryList7.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.Color, 0, 0xFFE6EA0E));
            TxtcResource.TxtcResource.EntryBlock entBlock7 = new TxtcResource.TxtcResource.EntryBlock(0, null);
            entBlock7.Entries = entryList7;
            entryBlockList.Add(entBlock7);

            TxtcResource.TxtcResource.EntryList entryList8 = new TxtcResource.TxtcResource.EntryList(null);
            entryList8.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.ID, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.StepType.ChannelSelect));
            entryList8.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.UIVisible, 0, 0));
            entryList8.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.MinShaderModel, 0, 0));
            entryList8.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SkipShaderModel, 0, (int)TxtcResource.TxtcResource.EntryInt32.ShaderModel.SM_Highest));
            entryList8.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.MinDetailLevel, 0, 0xFFFFFFFF));
            entryList8.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.SkipDetailLevel, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.DetailLevel.Lowest));
            entryList8.Add(new TxtcResource.TxtcResource.EntryString(0, null, TxtcResource.TxtcResource.EntryString.StringProperties.Description, 0, "Texture Step"));
            entryList8.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.ColorWrite, 0, (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Alpha));
            entryList8.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.DestinationRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryList8.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableBlending, 0, 0));
            entryList8.Add(new TxtcResource.TxtcResource.EntrySingle(0, null, TxtcResource.TxtcResource.EntrySingle.SingleProperties.Rotation, 0, 0.0f));
            entryList8.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.SourceRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryList8.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableFiltering, 0, 1));
            entryList8.Add(new TxtcResource.TxtcResource.EntryTGIIndex(0, null, TxtcResource.TxtcResource.EntryTGIIndex.TGIIndexProperties.ImageKey, 0, 0));
            entryList8.Add(new TxtcResource.TxtcResource.EntryVector(0, null, TxtcResource.TxtcResource.EntryVector.VectorProperties.ChannelSelect, 0, new float[] { 0.0f, 0.0f, 0.0f, 1.0f }));
            TxtcResource.TxtcResource.EntryBlock entBlock8 = new TxtcResource.TxtcResource.EntryBlock(0, null);
            entBlock8.Entries = entryList8;
            entryBlockList.Add(entBlock8);

            TxtcResource.TxtcResource.EntryList entryList9 = new TxtcResource.TxtcResource.EntryList(null);
            entryList9.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.ID, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.StepType.ColorFill));
            entryList9.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.UIVisible, 0, 0));
            entryList9.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.MinShaderModel, 0, 0));
            entryList9.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SkipShaderModel, 0, (int)TxtcResource.TxtcResource.EntryInt32.ShaderModel.SM_Highest));
            entryList9.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.MinDetailLevel, 0, 0xFFFFFFFF));
            entryList9.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.SkipDetailLevel, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.DetailLevel.Lowest));
            entryList9.Add(new TxtcResource.TxtcResource.EntryString(0, null, TxtcResource.TxtcResource.EntryString.StringProperties.Description, 0, "Texture Step"));
            entryList9.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.ColorWrite, 0,
                (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Blue + (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Green + (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Red));
            entryList9.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.DestinationRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryList9.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableBlending, 0, 1));
            entryList9.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SourceBlend, 0, (int)TxtcResource.TxtcResource.EntryInt32.Blend.DestinationAlpha));
            entryList9.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.DestinationBlend, 0, (int)TxtcResource.TxtcResource.EntryInt32.Blend.InverseDestinationAlpha));
            entryList9.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.Color, 0, 0xFF000000));
            TxtcResource.TxtcResource.EntryBlock entBlock9 = new TxtcResource.TxtcResource.EntryBlock(0, null);
            entBlock9.Entries = entryList9;
            entryBlockList.Add(entBlock9);

            TxtcResource.TxtcResource.EntryList entryListA = new TxtcResource.TxtcResource.EntryList(null);
            entryListA.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.ID, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.StepType.ChannelSelect));
            entryListA.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.UIVisible, 0, 0));
            entryListA.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.MinShaderModel, 0, 0));
            entryListA.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SkipShaderModel, 0, (int)TxtcResource.TxtcResource.EntryInt32.ShaderModel.SM_Highest));
            entryListA.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.MinDetailLevel, 0, 0xFFFFFFFF));
            entryListA.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.SkipDetailLevel, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.DetailLevel.Lowest));
            entryListA.Add(new TxtcResource.TxtcResource.EntryString(0, null, TxtcResource.TxtcResource.EntryString.StringProperties.Description, 0, "Texture Step"));
            entryListA.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.ColorWrite, 0, (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Alpha));
            entryListA.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.DestinationRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryListA.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableBlending, 0, 1));
            entryListA.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SourceBlend, 0, (int)TxtcResource.TxtcResource.EntryInt32.Blend.One));
            entryListA.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.DestinationBlend, 0, (int)TxtcResource.TxtcResource.EntryInt32.Blend.One));
            entryListA.Add(new TxtcResource.TxtcResource.EntrySingle(0, null, TxtcResource.TxtcResource.EntrySingle.SingleProperties.Rotation, 0, 0.0f));
            entryListA.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.SourceRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryListA.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableFiltering, 0, 1));
            entryListA.Add(new TxtcResource.TxtcResource.EntryTGIIndex(0, null, TxtcResource.TxtcResource.EntryTGIIndex.TGIIndexProperties.ImageKey, 0, 0));
            entryListA.Add(new TxtcResource.TxtcResource.EntryVector(0, null, TxtcResource.TxtcResource.EntryVector.VectorProperties.ChannelSelect, 0, new float[] { 1.0f, 1.0f, 1.0f, 1.0f }));
            TxtcResource.TxtcResource.EntryBlock entBlockA = new TxtcResource.TxtcResource.EntryBlock(0, null);
            entBlockA.Entries = entryListA;
            entryBlockList.Add(entBlockA);

            TxtcResource.TxtcResource.EntryList entryListB = new TxtcResource.TxtcResource.EntryList(null);
            entryListB.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.ID, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.StepType.SetTarget));
            entryListB.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.UIVisible, 0, 0));
            entryListB.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.MinShaderModel, 0, 0));
            entryListB.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SkipShaderModel, 0, (int)TxtcResource.TxtcResource.EntryInt32.ShaderModel.SM_Highest));
            entryListB.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.MinDetailLevel, 0, 0xFFFFFFFF));
            entryListB.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.SkipDetailLevel, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.DetailLevel.Lowest));
            entryListB.Add(new TxtcResource.TxtcResource.EntryString(0, null, TxtcResource.TxtcResource.EntryString.StringProperties.Description, 0, "Texture Step"));
            entryListB.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.RenderTarget, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.RenderTarget.RenderTarget_A));
            TxtcResource.TxtcResource.EntryBlock entBlockB = new TxtcResource.TxtcResource.EntryBlock(0, null);
            entBlockB.Entries = entryListB;
            entryBlockList.Add(entBlockB);

            TxtcResource.TxtcResource.EntryList entryListC = new TxtcResource.TxtcResource.EntryList(null);
            entryListC.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.ID, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.StepType.DrawImage));
            entryListC.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.UIVisible, 0, 0));
            entryListC.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.MinShaderModel, 0, 0));
            entryListC.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SkipShaderModel, 0, (int)TxtcResource.TxtcResource.EntryInt32.ShaderModel.SM_Highest));
            entryListC.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.MinDetailLevel, 0, 0xFFFFFFFF));
            entryListC.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.SkipDetailLevel, 0, (uint)TxtcResource.TxtcResource.EntryUInt32.DetailLevel.Lowest));
            entryListC.Add(new TxtcResource.TxtcResource.EntryString(0, null, TxtcResource.TxtcResource.EntryString.StringProperties.Description, 0, "Texture Step"));
            entryListC.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.ColorWrite, 0,
                (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Blue + (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Green + (int)TxtcResource.TxtcResource.EntryInt32.ColorWriteChannels.Red));
            entryListC.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.DestinationRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryListC.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableBlending, 0, 1));
            entryListC.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.SourceBlend, 0, (int)TxtcResource.TxtcResource.EntryInt32.Blend.SourceAlpha));
            entryListC.Add(new TxtcResource.TxtcResource.EntryInt32(0, null, TxtcResource.TxtcResource.EntryInt32.Int32Properties.DestinationBlend, 0, (int)TxtcResource.TxtcResource.EntryInt32.Blend.InverseSourceAlpha));
            entryListC.Add(new TxtcResource.TxtcResource.EntrySingle(0, null, TxtcResource.TxtcResource.EntrySingle.SingleProperties.Rotation, 0, 0.0f));
            entryListC.Add(new TxtcResource.TxtcResource.EntryRectangle(0, null, TxtcResource.TxtcResource.EntryRectangle.RectangleProperties.SourceRectangle, 0, new float[] { 0.0f, 0.0f, 1.0f, 1.0f }));
            entryListC.Add(new TxtcResource.TxtcResource.EntryBoolean(0, null, TxtcResource.TxtcResource.EntryBoolean.BooleanProperties.EnableFiltering, 0, 0));
            entryListC.Add(new TxtcResource.TxtcResource.EntryUInt32(0, null, TxtcResource.TxtcResource.EntryUInt32.UInt32Properties.ImageSource, 0, 0x021E9CD5));
            TxtcResource.TxtcResource.EntryBlock entBlockC = new TxtcResource.TxtcResource.EntryBlock(0, null);
            entBlockC.Entries = entryListC;
            entryBlockList.Add(entBlockC);

            txtc.Root.Entries = entryBlockList;

            CountedTGIBlockList tgi = new CountedTGIBlockList(null);
            tgi.Add(tatTGI);
            txtc.Root.TGIBlocks = tgi;

            return txtc;
        }
    }

    public class PresetBuilder
    {
        public static XmlDocument presetXML(string tattooName, ulong tattooInstance, Preset pre, bool channelRenabled, bool channelGenabled, bool channelBenabled, bool channelAenabled)
        {
            XmlDocument doc = new XmlDocument();

            XmlElement preset = doc.CreateElement("preset");
            doc.AppendChild(preset);

            XmlElement complate = doc.CreateElement("complate");
            XmlAttribute att1 = doc.CreateAttribute("name");
            att1.Value = "TattooTemplate";
            complate.Attributes.Append(att1);
            XmlAttribute att2 = doc.CreateAttribute("reskey");
            att2.Value = "key:0333406C:00000000:F7FC14B9EA85B390";
            complate.Attributes.Append(att2);

            XmlNode node1 = valueNode(doc, "assetRoot", "X:");
            complate.AppendChild(node1);
            XmlNode node2 = valueNode(doc, "daeFileName", tattooName);
            complate.AppendChild(node2);
            XmlNode node3 = valueNode(doc, "age", "U");
            complate.AppendChild(node3);
            XmlNode node4 = valueNode(doc, "gender", "U");
            complate.AppendChild(node4);
            XmlNode node5 = valueNode(doc, "species", "U");
            complate.AppendChild(node5);
            XmlNode node6 = valueNode(doc, "bodyType", "Body");
            complate.AppendChild(node6);
            XmlNode node7 = valueNode(doc, "partType", "Body");
            complate.AppendChild(node7);
            XmlNode node8 = valueNode(doc, "filename", "TattooTemplate");
            complate.AppendChild(node8);
            XmlNode node9 = valueNode(doc, "Layer1Mask", "key:00B2D882:00000000:" + FNVhash.FormatFNV(tattooInstance));
            complate.AppendChild(node9);
            XmlNode node10 = valueNode(doc, "Layer1ColorR", pre.rChannelPercentString);
            complate.AppendChild(node10);
            XmlNode node11 = valueNode(doc, "Layer1ColorG", pre.gChannelPercentString);
            complate.AppendChild(node11);
            XmlNode node12 = valueNode(doc, "Layer1ColorB", pre.bChannelPercentString);
            complate.AppendChild(node12);
            XmlNode node13 = valueNode(doc, "Layer1ColorA", pre.aChannelPercentString);
            complate.AppendChild(node13);
            XmlNode node14 = valueNode(doc, "ColorREnabled", channelRenabled.ToString().ToLower());
            complate.AppendChild(node14);
            XmlNode node15 = valueNode(doc, "ColorGEnabled", channelGenabled.ToString().ToLower());
            complate.AppendChild(node15);
            XmlNode node16 = valueNode(doc, "ColorBEnabled", channelBenabled.ToString().ToLower());
            complate.AppendChild(node16);
            XmlNode node17 = valueNode(doc, "ColorAEnabled", channelAenabled.ToString().ToLower());
            complate.AppendChild(node17);
            XmlNode node18 = valueNode(doc, "ThumbnailMode", "false");
            complate.AppendChild(node18);
            XmlNode node19 = valueNode(doc, "SimBuildMode", "true");
            complate.AppendChild(node19);
            XmlComment credit1 = doc.CreateComment("Tattooinator by cmar");
            complate.AppendChild(credit1);
            XmlComment credit2 = doc.CreateComment("In memory of Salem and Mini");
            complate.AppendChild(credit2);

            preset.AppendChild(complate);
            return doc;
        }

        private static XmlNode valueNode(XmlDocument doc, string keyString, string valueString)
        {
            XmlNode node = doc.CreateElement("value");
            XmlAttribute att1 = doc.CreateAttribute("key");
            att1.Value = keyString;
            node.Attributes.Append(att1);
            XmlAttribute att2 = doc.CreateAttribute("value");
            att2.Value = valueString;
            node.Attributes.Append(att2);
            return node;
        }
    }
}
