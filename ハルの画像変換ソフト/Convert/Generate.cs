using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilverNBTLibrary;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ハルの画像変換ソフト.Convert
{
    class Generate
    {
        public NBTTagCompound ImageConvert(Bitmap bmp, bool IsSchematic, ref BackgroundWorker bw)
        {
            var schema = new SilverNBTLibrary.Structure.Schematic(bmp.Width, 1, bmp.Height);
            var mapbyte = new byte[16384];
            var compound = new NBTTagCompound();
            int i = 0;
            using (BitmapAccessor accessor = new BitmapAccessor(bmp))
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color bitmapRGB = accessor.GetPixel(x, y);

                        var block = IsSchematic ? RGBToBlockColor(bitmapRGB) : RGBToMapColor(bitmapRGB);
                        if (IsSchematic)
                        {
                            schema.SetBlock(x, 0, y, block.ID, block.Meta);
                        }
                        else
                        {
                            mapbyte[i] = (byte)block.ID;
                            i++;
                        }
                    }

                    bw.ReportProgress(y);
                }
            }
            if (IsSchematic)
            {
                compound = schema.SaveToNBT();
            }
            else
            {
                var map = new NBTTagByteArray("colors", mapbyte);
                compound = NBTFile.LoadFromFile(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\data\MapTemplate.dat");
                var data = (NBTTagCompound)compound.GetTag("data");
                data.Add(map);
            }
            bmp.Dispose();

            return compound;
        }

        #region FloydSteinberg

        public Bitmap FloydSteinberg(Bitmap bmp, ref BackgroundWorker bw, bool IsSchematic)
        {
            using (BitmapAccessor accessor = new BitmapAccessor(bmp))
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {

                        Color bitmapRGB = accessor.GetPixel(x, y);
                        var block = IsSchematic ? RGBToBlockColor(bitmapRGB) : RGBToMapColor(bitmapRGB);

                        var blockRGB = Color.FromArgb(block.R, block.G, block.B);

                        if (x < accessor.Width - 1)
                        {
                            var nextPixel = accessor.GetPixel(x + 1, y);
                            var d = (decimal)7 / (decimal)16;
                            accessor.SetPixel(x + 1, y, GetDitheringColor(d, nextPixel, bitmapRGB, blockRGB));
                        }
                        /*
                        if (x < accessor.Width - 2)
                        {
                            var nextPixel = accessor.GetPixel(x + 2, y);
                            var d = (decimal)4 / (decimal)42;
                            accessor.SetPixel(x + 2, y, GetDitheringColor(d, nextPixel, pc, bc));
                        }
                        */
                        /*
                        if (x > 1 && y < accessor.Height - 1)
                        {
                            var nextPixel = accessor.GetPixel(x - 2, y + 1);
                            var d = (decimal)2 / (decimal)42;
                            accessor.SetPixel(x - 2, y + 1, GetDitheringColor(d, nextPixel, pc, bc));
                        }
                        */

                        if (x > 0 && y < accessor.Height - 1)
                        {
                            var nextPixel = accessor.GetPixel(x - 1, y + 1);
                            var d = (decimal)3 / (decimal)16;
                            accessor.SetPixel(x - 1, y + 1, GetDitheringColor(d, nextPixel, bitmapRGB, blockRGB));
                        }

                        if (y < accessor.Height - 1)
                        {
                            var nextPixel = accessor.GetPixel(x, y + 1);
                            var d = (decimal)5 / (decimal)42;
                            accessor.SetPixel(x, y + 1, GetDitheringColor(d, nextPixel, bitmapRGB, blockRGB));
                        }

                        if (x < accessor.Width - 1 && y < accessor.Height - 1)
                        {
                            var nextPixel = accessor.GetPixel(x + 1, y + 1);
                            var d = (decimal)1 / (decimal)42;
                            accessor.SetPixel(x + 1, y + 1, GetDitheringColor(d, nextPixel, bitmapRGB, blockRGB));
                        }

                        /*
                        if (x < accessor.Width - 2 && y < accessor.Height - 1)
                        {
                            var nextPixel = accessor.GetPixel(x + 2, y + 1);
                            var d = (decimal)2 / (decimal)42;
                            accessor.SetPixel(x + 2, y + 1, GetDitheringColor(d, nextPixel, pc, bc));
                        }
                        */
                        /*
                        if (x > 1 && y < accessor.Height - 2)
                        {
                            var nextPixel = accessor.GetPixel(x - 2, y + 2);
                            var d = (decimal)1 / (decimal)42;
                            accessor.SetPixel(x - 2, y + 2, GetDitheringColor(d, nextPixel, pc, bc));
                        }

                        //
                        if (x > 0 && y < accessor.Height - 2)
                        {
                            var nextPixel = accessor.GetPixel(x - 1, y + 1);
                            var d = (decimal)2 / (decimal)42;
                            accessor.SetPixel(x - 1, y + 2, GetDitheringColor(d, nextPixel, pc, bc));
                        }

                        //
                        if (y < accessor.Height - 2)
                        {
                            var nextPixel = accessor.GetPixel(x, y + 2);
                            var d = (decimal)4 / (decimal)42;
                            accessor.SetPixel(x, y + 2, GetDitheringColor(d, nextPixel, pc, bc));
                        }

                        //
                        if (x < accessor.Width - 2 && y < accessor.Height - 2)
                        {
                            var nextPixel = accessor.GetPixel(x + 1, y + 2);
                            var d = (decimal)2 / (decimal)42;
                            accessor.SetPixel(x + 1, y + 2, GetDitheringColor(d, nextPixel, pc, bc));
                        }

                        if (x < accessor.Width - 2 && y < accessor.Height - 2)
                        {
                            var nextPixel = accessor.GetPixel(x + 2, y + 2);
                            var d = (decimal)1 / (decimal)42;
                            accessor.SetPixel(x + 2, y + 2, GetDitheringColor(d, nextPixel, pc, bc));
                        }
                        */
                    }
                    bw.ReportProgress(0);
                }
            }
            return bmp;
        }
        #endregion FloydSteinberg

        private Color GetDitheringColor(decimal d, Color nextPixel, Color pc, Color bc)
        {
            int gosaR = pc.R - bc.R;
            int gosaG = pc.G - bc.G;
            int gosaB = pc.B - bc.B;
            int r = Math.Max(0, Math.Min(255, (int)Math.Floor(nextPixel.R + (gosaR * d))));
            int g = Math.Max(0, Math.Min(255, (int)Math.Floor(nextPixel.G + (gosaG * d))));
            int b = Math.Max(0, Math.Min(255, (int)Math.Floor(nextPixel.B + (gosaB * d))));

            return Color.FromArgb(r, g, b);
        }

        private Block RGBToBlockColor(Color color)
        {
            double distance = double.MaxValue;
            var ret = new Block();

            foreach (var block in Colors.blockpalette)
            {
                double n = GetApproximate(color, block);

                if (distance >= n)
                {
                    distance = n;
                    ret = block;
                }

            }
            return ret;
        }

        private Block RGBToMapColor(Color color)
        {
            double distance = double.MaxValue;
            var ret = new Block();
            double n;
            foreach (var block in Colors.mappalette)
            {
                n = GetApproximate(color, block);

                if (distance >= n)
                {
                    distance = n;
                    ret = block;
                }

            }
            return ret;
        }

        private double GetApproximate(Color color, Block block)
        {
            double r = color.R - block.R;
            double g = color.G - block.G;
            double b = color.B - block.B;
            double n = Math.Sqrt(r * r + g * g + b * b);

            if (n < 0)
            {
                n = (-n);
            }
            return n;
        }
    }
}
