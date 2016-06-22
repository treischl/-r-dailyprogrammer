using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

// Adapted a lot of code from https://github.com/cyotek/Dithering
namespace _272_dither_that_image
{
    class Program
    {
        static void Main(string[] args)
        {
            string pwd = Directory.GetCurrentDirectory();
            string fileName = args[0];

            using (var input = (Bitmap)Image.FromFile($"{pwd}\\{fileName}").Clone())
            {
                var size = input.Size;
                var pixels = GetPixels(input);
                Diffuse(pixels, size);
                SetPixels(input, pixels);
                input.Save($"{pwd}\\dithered_{fileName}");
            }
        }

        static Offset[] offsets = new Offset[]
        {
            new Offset(1, 0, 7),
            new Offset(-1, 1, 3),
            new Offset(0, 1, 5),
            new Offset(1, 1, 1)
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

            public ArgbColor(int a, int r, int g, int b) : this()
            {
                A = (byte)a;
                R = (byte)r;
                G = (byte)g;
                B = (byte)b;
            }
            public ArgbColor(Color color)
            {
                A = color.A;
                R = color.R;
                G = color.G;
                B = color.B;
            }
        }

        struct Offset
        {
            public int X;
            public int Y;
            public int C;

            public Offset(int x, int y, int c) : this()
            {
                X = x;
                Y = y;
                C = c;
            }
        }

        static ArgbColor[,] GetPixels(Bitmap bitmap)
        {
            var pixels = new ArgbColor[bitmap.Width, bitmap.Height];
            for (int w = 0; w < bitmap.Width; w++)
            {
                for (int h = 0; h < bitmap.Height; h++)
                {
                    pixels[w, h] = new ArgbColor(bitmap.GetPixel(w, h));
                }
            }
            return pixels;
        }

        static void Diffuse(ArgbColor[,] pixels, Size size)
        {
            for (int row = 0; row < size.Height; row++)
            {
                for (int col = 0; col < size.Width; col++)
                {
                    ArgbColor current, transformed;

                    current = pixels[col, row];
                    bool isBlack = (byte)(0.299 * current.R + 0.587 * current.G + 0.114 * current.B) < 127;
                    transformed = isBlack ? new ArgbColor(current.A, 0, 0, 0) : new ArgbColor(current.A, 255, 255, 255);
                    
                    pixels[col, row] = transformed;
                    
                    var errors = new
                    {
                        R = current.R - transformed.R,
                        G = current.G - transformed.G,
                        B = current.B - transformed.B
                    };

                    foreach (var offset in offsets)
                    {
                        int offsetX = col + offset.X;
                        int offsetY = row + offset.Y;

                        if (0 <= offsetX && offsetX < size.Width &&
                            0 <= offsetY && offsetY < size.Height)
                        {
                            var offsetPixel = pixels[col + offset.X, row + offset.Y];

                            int r = offsetPixel.R + (errors.R * offset.C / 16);
                            int g = offsetPixel.G + (errors.G * offset.C / 16);
                            int b = offsetPixel.B + (errors.B * offset.C / 16);

                            pixels[col + offset.X, row + offset.Y] = new ArgbColor(offsetPixel.A, r, g, b);
                        }
                    }
                }
            }
        }

        static void SetPixels(Bitmap bitmap, ArgbColor[,] pixels)
        {
            for (int w = 0; w < bitmap.Width; w++)
            {
                for (int h = 0; h < bitmap.Height; h++)
                {
                    var px = pixels[w, h];
                    bitmap.SetPixel(w, h, Color.FromArgb(px.A, px.R, px.G, px.B));
                }
            }
        }
    }
}
