using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace dither_that_image
{
    class Program
    {
        const PixelFormat format = PixelFormat.Format32bppArgb;

        static void Main(string[] args)
        {
            var file = new FileInfo($"{Directory.GetCurrentDirectory()}\\{args[0]}");

            using (var input = (Bitmap)Image.FromFile(file.FullName))
            using (var output = new Bitmap(input.Width, input.Height, format))
            {
                CopyImage(input, output);
                var pixels = GetPixels(output);
                Dither(pixels, output.Size);
                SaveImage(pixels, file.FullName.Replace(file.Extension, $"_dithered{file.Extension}"));
            }
        }

        static Offset[] offsets { get; } = new Offset[]
        {
            new Offset() { X = 1, Y = 0, C = 7 },
            new Offset() { X = -1, Y = 1, C = 3 },
            new Offset() { X = 0, Y = 1, C = 5 },
            new Offset() { X = 1, Y = 1, C = 1 }
        };

        [StructLayout(LayoutKind.Explicit)]
        struct ArgbColor
        {
            [FieldOffset(0)]
            public byte B;
            [FieldOffset(1)]
            public byte G;
            [FieldOffset(2)]
            public byte R;
            [FieldOffset(3)]
            public byte A;

            public ArgbColor(byte a, byte r, byte g, byte b) : this()
            {
                A = a;
                R = r;
                G = g;
                B = b;
            }
        }

        struct Offset
        {
            public int X;
            public int Y;
            public int C;
        }

        static void CopyImage(Bitmap original, Bitmap copy)
        {
            using (var graphics = Graphics.FromImage(copy))
            {
                graphics.Clear(Color.Transparent);
                graphics.PageUnit = GraphicsUnit.Pixel;
                graphics.DrawImage(original, new Rectangle(Point.Empty, original.Size));
            }
        }

        unsafe static ArgbColor[,] GetPixels(Bitmap bitmap)
        {
            var pixels = new ArgbColor[bitmap.Width, bitmap.Height];
            PixelyPixelFace(bitmap, ImageLockMode.WriteOnly, (x, y, ptr) => { pixels[x, y] = *ptr; });
            return pixels;
        }

        unsafe static void SaveImage(ArgbColor[,] pixels, string savePath)
        {
            var bitmap = new Bitmap(pixels.GetLength(0), pixels.GetLength(1), format);
            PixelyPixelFace(bitmap, ImageLockMode.ReadWrite, (x, y, ptr) => { *ptr = pixels[x, y]; });
            bitmap.Save(savePath);
        }

        unsafe delegate void PixelAction(int x, int y, ArgbColor* ptr);
        unsafe static void PixelyPixelFace(Bitmap bitmap, ImageLockMode lockMode, PixelAction pixelAction)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            var bitmapData = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), lockMode, format);
            
            var ptr = (ArgbColor*)bitmapData.Scan0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    pixelAction(x, y, ptr++);

            bitmap.UnlockBits(bitmapData);
        }

        static void Dither(ArgbColor[,] pixels, Size size)
        {
            Func<int, int, int, byte> floydSteinberg = (o, e, c) => (byte)(Math.Min(255, Math.Max(0, o + ((e * c) >> 4))));

            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    var current = pixels[x, y];
                    byte gray = (byte)(0.2126 * current.R + 0.7152 * current.G + 0.0722 * current.B); //https://en.wikipedia.org/wiki/Grayscale#Luma_coding_in_video_systems
                    byte bw = (byte)(gray < 128 ? 0 : 255);
                    pixels[x, y] = new ArgbColor(current.A, bw, bw, bw);

                    int error = gray - bw;

                    foreach (var offset in offsets)
                    {
                        int offsetX = x + offset.X;
                        int offsetY = y + offset.Y;
                        
                        if (0 < offsetX && offsetX < size.Width && 0 <= offsetY && offsetY < size.Height)
                        {
                            var offPx = pixels[offsetX, offsetY];

                            byte r = floydSteinberg(offPx.R, error, offset.C);
                            byte g = floydSteinberg(offPx.G, error, offset.C);
                            byte b = floydSteinberg(offPx.B, error, offset.C);

                            pixels[offsetX, offsetY] = new ArgbColor(offPx.A, r, g, b);
                        }
                    }
                }
            }
        }
    }
}
