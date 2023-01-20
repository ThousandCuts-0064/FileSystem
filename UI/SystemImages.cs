using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using FileSystemNS;

namespace UI
{
    public static class SystemImages
    {
        public static ImageList List { get; }
        public static Image FileExplorer { get; }
        public static Image Directory { get; }
        public static Image Txt { get; }
        public static Image Rtf { get; }
        public static Image Img { get; }
        public static Image Wav { get; }

        static SystemImages()
        {
            List = new ImageList();
            int width = 512;
            int height = 512;

            using (SolidBrush brush = new SolidBrush(Color.Black))
            using (Pen pen = new Pen(Color.Black, 64))
            {
                FileExplorer = CreateImage(Color.Orange);
                Directory = CreateImage(Color.Yellow);
                Txt = CreateImage(Color.Gray);
                Rtf = CreateImage(Color.Red);
                Img = CreateImage(Color.Green);
                Wav = CreateImage(Color.Blue);

                Image CreateImage(Color color)
                {
                    brush.Color = color;
                    Bitmap bitmap = new Bitmap(width, height);
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.FillEllipse(brush, 0, 0, width, height);
                        graphics.DrawEllipse(pen, 0, 0, width, height);
                    }
                    return bitmap;
                }
            }

            List.Images.Add(nameof(FileExplorer), FileExplorer);
            List.Images.Add(nameof(FileSystemNS.Directory), Directory);
            List.Images.Add(nameof(Txt), Txt);
            List.Images.Add(nameof(Rtf), Rtf);
            List.Images.Add(nameof(Img), Img);
            List.Images.Add(nameof(Wav), Wav);
        }

        public static string FormatToKey(FileFormat format)
        {
            switch (format)
            {
                case FileFormat.Txt: return nameof(Txt);

                case FileFormat.Rtf: return nameof(Rtf);

                case FileFormat.Bmp:
                case FileFormat.Emf:
                case FileFormat.Wmf:
                case FileFormat.Gif:
                case FileFormat.Jpeg:
                case FileFormat.Png:
                case FileFormat.Tiff:
                case FileFormat.Exif:
                case FileFormat.Icon:
                    return nameof(Img);

                case FileFormat.Wav: return nameof(Wav);

                default: throw new InvalidOperationException();
            }
        }
    }
}
