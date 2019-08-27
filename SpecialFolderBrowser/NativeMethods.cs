//Sample: displaying contents of Windows Shell special folders (such as My Computer, Control Panel etc.) in WPF WebBrowser
// *** P/Invoke & COM Interop decarations ***
//Copyright (c) 2019, MSDN.WhiteKnight

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WpfApplication1
{
    public static class NativeMethods
    {
        public const uint SHGFI_DISPLAYNAME = 0x000000200;
        public const uint SHGFI_ICON = 0x000000100;
        public const uint SHGFI_PIDL = 0x000000008;
        public const uint GIL_FORSHELL = 0x0002;
        public const int SHGDN_NORMAL = 0x0000;
        public const int SHGDN_FORPARSING = 0x8000;
        public const int SHCONTF_FOLDERS = 0x0020;
        public const int SHCONTF_NONFOLDERS = 0x0040;

        [DllImport("shell32.dll")]
        public static extern int SHGetKnownFolderIDList(ref Guid rfid, int dwFlags, IntPtr hToken, out IntPtr ppidl);

        [DllImport("ole32.dll")]
        public static extern void CoTaskMemFree(IntPtr pv);

        [DllImport("shell32.dll")]
        public static extern int SHBindToObject(
            IShellFolder psf,
            IntPtr pidl,
            [MarshalAs(UnmanagedType.IUnknown)] object pbc,
            ref Guid riid,
            out IShellFolder ppv);

        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrRetToStr(ref STRRET pstr, IntPtr pidl, out IntPtr ppsz);

        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr hIcon);
    }

    /* COM Interfaces */

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214E6-0000-0000-C000-000000000046")]
    public interface IShellFolder
    {
        void ParseDisplayName(IntPtr hwnd, IntPtr pbc, String pszDisplayName, UInt32 pchEaten, out IntPtr ppidl, UInt32 pdwAttributes);
        void EnumObjects(IntPtr hwnd, int grfFlags, out IEnumIDList ppenumIDList);
        void BindToObject(IntPtr pidl, IntPtr pbc, [In]ref Guid riid, out IntPtr ppv);
        void BindToStorage(IntPtr pidl, IntPtr pbc, [In]ref Guid riid, out IntPtr ppv);
        [PreserveSig]
        Int32 CompareIDs(Int32 lParam, IntPtr pidl1, IntPtr pidl2);
        void CreateViewObject(IntPtr hwndOwner, [In] ref Guid riid, out IntPtr ppv);
        void GetAttributesOf(UInt32 cidl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]IntPtr[] apidl, ref uint rgfInOut);
        void GetUIObjectOf(IntPtr hwndOwner, UInt32 cidl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]IntPtr[] apidl,
            [In] ref Guid riid, UInt32 rgfReserved, out IExtractIcon ppv);
        void GetDisplayNameOf(IntPtr pidl, int uFlags, out STRRET pName);
        void SetNameOf(IntPtr hwnd, IntPtr pidl, String pszName, int uFlags, out IntPtr ppidlOut);
    }

    [StructLayout(LayoutKind.Explicit, Size = 520)]
    public struct STRRETinternal
    {
        [FieldOffset(0)]
        public IntPtr pOleStr;

        [FieldOffset(0)]
        public IntPtr pStr;

        [FieldOffset(0)]
        public uint uOffset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STRRET
    {
        public uint uType;
        public STRRETinternal data;
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F2-0000-0000-C000-000000000046")]
    public interface IEnumIDList
    {
        [PreserveSig()]
        uint Next(uint celt, out IntPtr rgelt, IntPtr pceltFetched);
        [PreserveSig()]
        uint Skip(uint celt);
        [PreserveSig()]
        uint Reset();
        [PreserveSig()]
        uint Clone(out IEnumIDList ppenum);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    [ComImport()]
    [Guid("000214fa-0000-0000-c000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IExtractIcon
    {
        [PreserveSig]
        int GetIconLocation(
            uint uFlags,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 2)] StringBuilder szIconFile,
            int cchMax,
            out int piIndex,
            out uint pwFlags);


        [PreserveSig]
        int Extract(
            [MarshalAs(UnmanagedType.LPWStr)] string pszFile, uint nIconIndex, out IntPtr phiconLarge,
            out IntPtr phiconSmall, uint nIconSize);
    }
}
