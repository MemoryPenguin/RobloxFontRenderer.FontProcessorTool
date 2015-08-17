using System.Collections.Generic;
using System.Text;

namespace RbxFontConverter
{
    /// <summary>
    /// Represents data that can be used with the ROBLOX script to render a font at a given size.
    /// </summary>
    class FontInfo
    {
        public Dictionary<char, CharInfo> charInfos;
        public readonly string fontName;
        public readonly int fontSize;

        public FontInfo(string fontName, int fontSize)
        {
            this.fontName = fontName;
            this.fontSize = fontSize;
            this.charInfos = new Dictionary<char, CharInfo>();
        }

        public void AddCharacterInfo(char theChar, CharInfo info)
        {
            charInfos.Add(theChar, info);
        }

        public string GetExportedInfo()
        {
            StringBuilder builder = new StringBuilder("return {");

            builder.AppendLine().AppendFormat("\tName = \"{0}\";", this.fontName).AppendLine();
            builder.AppendFormat("\tSize = {0};", this.fontSize).AppendLine();
            builder.Append("\tCharacters = {").AppendLine();
            
            foreach (KeyValuePair<char, CharInfo> keyValue in this.charInfos)
            {
                string escapedStr = "";
                if (keyValue.Key == '\'')
                    escapedStr = "'\\''";
                else if (keyValue.Key == '\\')
                    escapedStr = "'\\\\'";
                else
                    escapedStr = "'" + keyValue.Key + "'";

                builder.Append("\t\t{ Char = " + escapedStr + "; ");
                builder.AppendFormat("AdvanceWidth = {0}; " , keyValue.Value.advanceWidth);
                builder.AppendFormat("ImageX = {0}; ", keyValue.Value.imageX);
                builder.AppendFormat("ImageY = {0}; ", keyValue.Value.imageY);
                builder.AppendFormat("ImageWidth = {0}; ", keyValue.Value.imageWidth);
                builder.AppendFormat("ImageHeight = {0}; ", keyValue.Value.imageHeight);
                builder.Append("};").AppendLine();
            }

            builder.Append("\t}").AppendLine();
            builder.Append("}").AppendLine();

            return builder.ToString();
        }

        public sealed class CharInfo
        {
            public readonly char theChar;
            public readonly int advanceWidth, imageX, imageY, imageWidth, imageHeight;
            
            public CharInfo(char theChar, int advanceWidth = 0, int imageX = 0, int imageY = 0, int imageWidth = 0, int imageHeight = 0)
            {
                this.theChar = theChar;
                this.advanceWidth = advanceWidth;
                this.imageX = imageX;
                this.imageY = imageY;
                this.imageWidth = imageWidth;
                this.imageHeight = imageHeight;
            }
        }
    }
}
