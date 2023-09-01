using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ADVNow.Utils
{
    class WindowUtil
    {
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static uint WM_GETICON = 0x007F;

        public static IntPtr ICON_BIG = new IntPtr(1);

        public static IntPtr IDI_APPLICATION = new IntPtr(0x7F00);

        public static int GCL_HICON = -14;

        public static Bitmap? CaptureWindow(IntPtr hwnd)
        {
            RECT windowRect;
            GetWindowRect(hwnd, out windowRect);

            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;

            if (width == 0 || height == 0)
            {
                return null;
            }

            Bitmap bmp = new Bitmap(width, height);
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                IntPtr hdcBitmap = gfx.GetHdc();
                PrintWindow(hwnd, hdcBitmap, 1);
                gfx.ReleaseHdc(hdcBitmap);
            }

            Rectangle rect = GetRect(bmp);
            bmp = Crop(bmp, rect);

            return bmp;
        }

        public static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
                return new IntPtr((long)GetClassLong32(hWnd, nIndex));
            else
                return GetClassLong64(hWnd, nIndex);
        }

        public static Bitmap? GetSmallWindowIcon(IntPtr hWnd)
        {
            try
            {
                IntPtr hIcon = default(IntPtr);

                hIcon = SendMessage(hWnd, WM_GETICON, ICON_BIG, 0);

                if (hIcon == IntPtr.Zero) hIcon = GetClassLongPtr(hWnd, GCL_HICON);

                if (hIcon != IntPtr.Zero) return new Bitmap(Icon.FromHandle(hIcon).ToBitmap(), 64, 64);
                else return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Rectangle GetRect(Bitmap bmp)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            var bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            var rgbValues = new byte[bytes];
            Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytes);
            bmp.UnlockBits(bmpData);

            int x0 = bmp.Width;
            int y0 = bmp.Height;
            int x1 = 0;
            int y1 = 0;

            for (int i = 3; i < rgbValues.Length; i += 4)
            {
                if (rgbValues[i] != 0)
                {
                    int x = i / 4 % bmp.Width;
                    int y = i / 4 / bmp.Width;

                    if (x0 > x) x0 = x;
                    if (y0 > y) y0 = y;
                    if (x1 < x) x1 = x;
                    if (y1 < y) y1 = y;
                }
            }

            return new Rectangle(x0, y0, x1 - x0, y1 - y0);
        }

        public static Bitmap Crop(Bitmap bmp, Rectangle rect)
        {
            var newbmp = new Bitmap(rect.Width, rect.Height);
            using (var g = Graphics.FromImage(newbmp))
            {
                g.DrawImage(bmp, 0, 0, rect, GraphicsUnit.Pixel);
            }
            return newbmp;
        }
    }
}
