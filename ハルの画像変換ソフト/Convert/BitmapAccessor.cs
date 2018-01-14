using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ハルの画像変換ソフト.Convert
{
    class BitmapAccessor : IDisposable
    {
        /* ----------------------------------------------------------------- */
        ///
        /// IsSupported
        /// 
        /// <summary>
        /// 指定した PixelFormat が BimatpAccessor で対応しているかどうか．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public static bool IsSupported(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Canonical:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppArgb:
                    return true;
                // 以下，未実装の PixelFormat
                // case PixelFormat.Alpha:
                // case PixelFormat.DontCare:
                // case PixelFormat.Extended:
                // case PixelFormat.Format16bppArgb1555:
                // case PixelFormat.Format16bppGrayScale:
                // case PixelFormat.Format16bppRgb555:
                // case PixelFormat.Format16bppRgb565:
                // case PixelFormat.Format1bppIndexed:
                // case PixelFormat.Format32bppPArgb:
                // case PixelFormat.Format48bppRgb:
                // case PixelFormat.Format4bppIndexed:
                // case PixelFormat.Format64bppArgb:
                // case PixelFormat.Format64bppPArgb:
                // case PixelFormat.Format8bppIndexed:
                // case PixelFormat.Gdi:
                // case PixelFormat.Indexed:
                // case PixelFormat.Max:
                // case PixelFormat.PAlpha:
                default:
                    break;
            }
            return false;
        }

        /* ----------------------------------------------------------------- */
        /// Constructor
        /* ----------------------------------------------------------------- */
        public BitmapAccessor(Bitmap img)
        {
            Debug.Assert(IsSupported(img.PixelFormat));

            original_ = img;
            data_ = original_.LockBits(new Rectangle(0, 0, original_.Width, original_.Height), ImageLockMode.ReadWrite, img.PixelFormat);
        }

        /* ----------------------------------------------------------------- */
        /// Dispose
        /* ----------------------------------------------------------------- */
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /* ----------------------------------------------------------------- */
        /// Dispose
        /* ----------------------------------------------------------------- */
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed_)
            {
                if (disposing)
                {
                    if (original_ != null && data_ != null)
                    {
                        original_.UnlockBits(data_);
                        data_ = null;
                    }
                }
            }
            disposed_ = true;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetPixel
        ///
        /// <summary>
        /// Bitmap の指定したピクセルの色を取得する．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Color GetPixel(int x, int y)
        {
            Debug.Assert(this.original_ != null);
            Debug.Assert(this.data_ != null);

            int offset = this.GetOffset(x, y);
            return Color.FromArgb(this.GetAlpha(offset), this.GetRed(offset), this.GetGreen(offset), this.GetBlue(offset));
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SetPixel
        ///
        /// <summary>
        /// Bitmap の指定したピクセルの色を設定する．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void SetPixel(int x, int y, Color c)
        {
            Debug.Assert(this.original_ != null);
            Debug.Assert(this.data_ != null);

            int offset = this.GetOffset(x, y);
            this.SetAlpha(offset, c.A);
            this.SetRed(offset, c.R);
            this.SetGreen(offset, c.G);
            this.SetBlue(offset, c.B);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Width
        /// 
        /// <summary>
        /// Bitmap オブジェクトのピクセルの幅を取得する．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public int Width
        {
            get { return data_.Width; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Height
        ///
        /// <summary>
        /// Bitmap オブジェクトの高さ（ピクセル単位）を取得する．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int Height
        {
            get { return data_.Height; }
        }

        /* ----------------------------------------------------------------- */
        /// 内部処理のためのメソッド群
        /* ----------------------------------------------------------------- */
        #region Private methods
        /* ----------------------------------------------------------------- */
        ///
        /// GetOffset
        ///
        /// <summary>
        /// Bitmap の (x, y) ピクセルの色情報に対応する最初のバイト位置を
        /// 取得する．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private int GetOffset(int x, int y)
        {
            switch (data_.PixelFormat)
            {
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return x * 8 + data_.Stride * y;
                case PixelFormat.Format48bppRgb:
                    return x * 6 + data_.Stride * y;
                case PixelFormat.Canonical:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return x * 4 + data_.Stride * y;
                case PixelFormat.Format24bppRgb:
                    return x * 3 + data_.Stride * y;
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                    return x * 2 + data_.Stride * y;
                case PixelFormat.Format8bppIndexed:
                    return x + data_.Stride * y;
                case PixelFormat.Format4bppIndexed:
                    return 2 / x + data_.Stride * y;
                case PixelFormat.Format1bppIndexed:
                    return 8 / x + data_.Stride * y;
                // 以下，計算方法が不明な PixelFormat
                // case PixelFormat.Alpha:
                // case PixelFormat.DontCare:
                // case PixelFormat.Extended:
                // case PixelFormat.Gdi:
                // case PixelFormat.Indexed:
                // case PixelFormat.Max:
                // case PixelFormat.PAlpha:
                default:
                    break;
            }
            return -1;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetAlpha
        ///
        /// <summary>
        /// Bitmap の特定ピクセルのアルファ値を取得する．引数 offset は，
        /// 取得したいピクセルの最初のバイト位置を示す．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private int GetAlpha(int offset)
        {
            if (data_.PixelFormat == PixelFormat.Format32bppArgb)
            {
                return Marshal.ReadByte(data_.Scan0, offset + 3);
            }
            else return 0;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetRed
        ///
        /// <summary>
        /// Bitmap の特定ピクセルの赤色値を取得する．引数 offset は，取得
        /// したいピクセルの最初のバイト位置を示す．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private byte GetRed(int offset)
        {
            return Marshal.ReadByte(data_.Scan0, offset + 2);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetGreen
        ///
        /// <summary>
        /// Bitmap の特定ピクセルの緑色値を取得する．引数 offset は，取得
        /// したいピクセルの最初のバイト位置を示す．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private byte GetGreen(int offset)
        {
            return Marshal.ReadByte(data_.Scan0, offset + 1);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetBlue
        ///
        /// <summary>
        /// Bitmap の特定ピクセルの青色値を取得する．引数 offset は，取得
        /// したいピクセルの最初のバイト位置を示す．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private byte GetBlue(int offset)
        {
            return Marshal.ReadByte(data_.Scan0, offset);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SetAlpha
        ///
        /// <summary>
        /// Bitmap の特定ピクセルのアルファ値を設定する．引数 offset は，
        /// 設定したいピクセルの最初のバイト位置を示す．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetAlpha(int offset, byte value)
        {
            if (data_.PixelFormat == PixelFormat.Format32bppArgb)
            {
                Marshal.WriteByte(data_.Scan0, offset + 3, value);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SetRed
        ///
        /// <summary>
        /// Bitmap の特定ピクセルの赤色値を設定する．引数 offset は，設定
        /// したいピクセルの最初のバイト位置を示す．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetRed(int offset, byte value)
        {
            Marshal.WriteByte(data_.Scan0, offset + 2, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SetGreen
        ///
        /// <summary>
        /// Bitmap の特定ピクセルの緑色値を設定する．引数 offset は，設定
        /// したいピクセルの最初のバイト位置を示す．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetGreen(int offset, byte value)
        {
            Marshal.WriteByte(data_.Scan0, offset + 1, value);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SetBlue
        ///
        /// <summary>
        /// Bitmap の特定ピクセルの青色値を設定する．引数 offset は，設定
        /// したいピクセルの最初のバイト位置を示す．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void SetBlue(int offset, byte value)
        {
            Marshal.WriteByte(data_.Scan0, offset, value);
        }
        #endregion

        /* ----------------------------------------------------------------- */
        /// 変数定義
        /* ----------------------------------------------------------------- */
        #region Member variables
        private Bitmap original_ = null;
        private BitmapData data_ = null;
        private bool disposed_ = false;
        #endregion
    }
}
