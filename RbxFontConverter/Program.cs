using System;
using SharpFont;
using System.Drawing;
using System.IO;

namespace RbxFontConverter
{
    class Program
    {
        public static Library library = new Library();

        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please supply a path and a list of font sizes.");
                return 1;
            }

            string path = args[0];
            Face face = new Face(library, path);
            Console.WriteLine("Font family: {0}, style: {1}", face.FamilyName, face.StyleName);
            
            for (int i = 1; i < args.Length; i++)
            {
                int fontSize = int.Parse(args[i]);
                string saveLocation = face.FamilyName + face.StyleName + "-" + fontSize + "pt.png";
                string dataSaveLocation = face.FamilyName + face.StyleName + "-" + fontSize + "pt-data.lua";

                Console.WriteLine("Font size: {0}", fontSize);

                FontExporter exporter = new FontExporter(face, fontSize);
                exporter.Export(saveLocation, dataSaveLocation);
            }

            return 0;
        }
    }
}
