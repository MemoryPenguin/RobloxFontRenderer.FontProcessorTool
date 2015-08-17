using SharpFont;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace RbxFontConverter
{
    /// <summary>
    /// Exports a Face object with a given size in points.
    /// </summary>
    class FontExporter
    {
        private Face face;
        private int pointsSize;
        private FontInfo fontInfo;

        public string exportedCharacters;

        public static readonly int MAX_WIDTH = 512;

        public FontExporter(Face face, int size)
        {
            this.face = face;
            this.pointsSize = size;
            this.fontInfo = new FontInfo(face.FamilyName + "." + face.StyleName, size);

            this.face.SetCharSize(size * 96 / 72, 0, 96, 96);

            exportedCharacters = "";
            for (int i = 32; i < 126; i++)
            {
                exportedCharacters += (char)i;
            }
        }

        public FontExporter(string path, int size) : this(new Face(Program.library, path), size) { }

        public void Export(string imageOutputPath, string dataOutputPath)
        {
            float penX = 0;
            float penY = 0;
            float width = 0;
            float height = 0;
            float cellHeight = 0;

            // get cell bounds
            for (int i = 0; i < exportedCharacters.Length; i++)
            {
                char current = exportedCharacters[i];
                uint glyphIndex = face.GetCharIndex(current);
                face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);

                width += (float)face.Glyph.Metrics.HorizontalAdvance;

                if (face.Glyph.Metrics.Height > cellHeight)
                    cellHeight = (float)face.Glyph.Metrics.Height;
            }

            int numCells = exportedCharacters.Length;

            if (width > MAX_WIDTH)
            {
                int numOverlaps = (int)Math.Truncate(width / MAX_WIDTH);
                width = MAX_WIDTH;
                height = (numOverlaps + 1) * cellHeight;
            }
            else
                height = cellHeight;

            Bitmap bmp = new Bitmap((int)Math.Ceiling(width), (int)Math.Ceiling(height), PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            face.SetUnpatentedHinting(true);

            for (int i = 0; i < exportedCharacters.Length; i++)
            {
                char current = exportedCharacters[i];
                uint glyphIndex = face.GetCharIndex(current);
                face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
                face.Glyph.RenderGlyph(RenderMode.Normal);

                if (current == ' ')
                {
                    penX += (float)face.Glyph.Metrics.HorizontalAdvance;
                    fontInfo.AddCharacterInfo(current, new FontInfo.CharInfo(current, face.Glyph.Metrics.HorizontalAdvance.ToInt32()));
                    // it's a space, just go forwards without rendering anything
                    continue;
                }

                if (penX + face.Glyph.Metrics.HorizontalAdvance + face.Glyph.BitmapLeft > width)
                {
                    penX = 0;
                    penY += cellHeight;
                }


                int advanceAmount = face.Glyph.Metrics.HorizontalAdvance.ToInt32();
                int imageX = (int)penX;
                int imageY = (int)penY;
                int imageWidth = face.Glyph.Metrics.Width.ToInt32();
                int imageHeight = face.Glyph.Metrics.Height.ToInt32();

                FTBitmap ftbmp = face.Glyph.Bitmap;
                Bitmap copy = ftbmp.ToGdipBitmap(Color.White);
                g.DrawImageUnscaled(copy, imageX, imageY);
                fontInfo.AddCharacterInfo(current, new FontInfo.CharInfo(current, advanceAmount, imageX, imageY, imageWidth, imageHeight));
                penX += (float)face.Glyph.Metrics.HorizontalAdvance;
            }

            g.Dispose();
            bmp = FixBitmap(bmp);
            bmp.Save(imageOutputPath);
            File.WriteAllText(dataOutputPath, fontInfo.GetExportedInfo());
        }

        /// <summary>
        /// 'Fixes' the bitmap so that it's a pure-white image, while leaving alpha the same.  There's probably a better way to do this.
        /// </summary>
        /// <param name="source">The source bitmap</param>
        /// <returns>A copy of <code>source</code> that only contains white, with the same alpha values.</returns>
        private Bitmap FixBitmap(Bitmap source)
        {
            Bitmap newBmp = new Bitmap(source);
            Rectangle bounds = new Rectangle(0, 0, newBmp.Width, newBmp.Height);
            BitmapData data = newBmp.LockBits(bounds, ImageLockMode.ReadWrite, newBmp.PixelFormat);
            IntPtr ptr = data.Scan0;

            int bytes = Math.Abs(data.Stride) * newBmp.Height;
            byte[] pixelData = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, pixelData, 0, bytes);
            
            for (int i = 0; i < pixelData.Length; i++)
            {
                // don't edit the alpha
                if (i % 4 != 3 && pixelData[i] > 0)
                {
                    // max out r/g/b
                    pixelData[i] = 255;
                    
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(pixelData, 0, ptr, bytes);
            newBmp.UnlockBits(data);

            return newBmp;
        }

        public string GetFontData()
        {
            return fontInfo.GetExportedInfo();
        }
    }
}
