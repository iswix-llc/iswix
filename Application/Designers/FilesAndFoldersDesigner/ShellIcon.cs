//Class adapted from example found at:
//http://www.pinvoke.net/default.aspx/shell32.SHGetFileInfo

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Designers.FilesAndFolders
{
    public static class ShellIcon
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        class Win32
        {
            public const uint SHGFI_ICON = 0x100;
            public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
            public const uint SHGFI_SMALLICON = 0x1; // 'Small icon

            [DllImport("shell32.dll")]
            public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

            [DllImport("User32.dll")]
            public static extern int DestroyIcon(IntPtr hIcon);

        }

        static ShellIcon()
        {

        }

        public static Bitmap GetSmallBitmap(string fileName)
        {
            return GetBitmap(fileName, Win32.SHGFI_SMALLICON);
        }

        public static Bitmap GetLargeBitmap(string fileName)
        {
            return GetBitmap(fileName, Win32.SHGFI_LARGEICON);
        }

        private static Bitmap GetBitmap(string fileName, uint flags)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr hImgSmall = Win32.SHGetFileInfo(fileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | flags);

            Icon icon = (Icon)System.Drawing.Icon.FromHandle(shinfo.hIcon).Clone();
            Win32.DestroyIcon(shinfo.hIcon);

            Bitmap bitmap = icon.ToBitmap();
            bitmap.MakeTransparent();
            return bitmap;
        }
    }
}