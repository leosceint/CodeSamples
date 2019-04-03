//Code sample: Using Windows low-level keyboard hook to intercept key events and print them into console, 
// taking into account shift/caps state and keyboard layout
//
//Author: MSDN-WhiteKnight (https://github.com/MSDN-WhiteKnight)

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows.Forms;

namespace KeyboardHookSample
{
    class Program
    {
        static KeyLogger k;

        static void Main(string[] args)
        {            
            k = new KeyLogger();
            k.Start();

            Console.CancelKeyPress += Console_CancelKeyPress;
            Console.WriteLine("Started listening for keyboard events...");
            Application.Run();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            k.Stop();            
            Application.Exit();
        }
    }

    sealed class KeyLogger : IDisposable //отслеживает события клавиатуры с помощью низкоуровневого хука Windows
    {
        //объявления типов, констант и функций Windows API
        delegate IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        const int WH_KEYBOARD_LL = 13;
        const int HC_ACTION = 0;
        const int WM_KEYDOWN = 0x0100;
        const uint VK_CAPITAL = 0x14;        

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr SetWindowsHookEx(int idHook, KeyboardProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("USER32.dll")]
        public static extern short GetKeyState(int vKey);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        public static extern int ToUnicodeEx(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags,
            IntPtr dwhkl);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(
            uint uCode,
            uint uMapType);

        [DllImport("user32.dll")]
        public static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        //преобразование виртуального кода клавиши в его Юникод-представление
        public static string VKCodeToUnicode(uint vkCode)
        {
            StringBuilder buf = new StringBuilder();

            byte[] keyboardState = new byte[255];

            short x;
            byte y;

            for (int i = 0; i < 255; i++)
            {
                if (i == VK_CAPITAL)
                {
                    x = GetKeyState(i);
                }
                else
                {
                    x = GetAsyncKeyState(i);
                }
                y = 0;
                if ((x & 0x8000) != 0) y = (byte)(y | 0x80);
                if ((x & 0x0001) != 0) y = (byte)(y | 0x01);
                keyboardState[i] = y;
            }

            ToUnicodeEx(vkCode, MapVirtualKey(vkCode, 0), keyboardState, buf, 5, 0,
                GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero)));
            return buf.ToString();
        }

        IntPtr _hookID = IntPtr.Zero;                

        //начинает отслеживание событий клавиатуры
        public void Start()
        {
            if (disposed) throw new ObjectDisposedException("KeyLogger");
            if (_hookID != IntPtr.Zero) return;

            _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookCallback, IntPtr.Zero, 0);

            if (_hookID == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                throw new Win32Exception(error, "Failed to install hook! Error: "+ error.ToString());
            }            
        }

        //останавливает отслеживание событий клавиатуры
        public void Stop()
        {
            if (disposed) return;
            if (_hookID == IntPtr.Zero) return;

            UnhookWindowsHookEx(_hookID);
            _hookID = IntPtr.Zero;              
        }               

        //вызывается Windows при нажатии клавиши
        IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                string s = VKCodeToUnicode((uint)vkCode);

                Console.Out.WriteLine("Key: " + ((Keys)vkCode).ToString() + "; Character: " + s);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        //так как KeyLogger использует неуправляемый ресурс, реализуем Dispose и Finalize...
        bool disposed = false;                  

        public void Dispose()
        {
            if (disposed) return;

            Stop();
            disposed = true;
            GC.SuppressFinalize(this);
        }

        ~KeyLogger()
        {            
            Dispose();
        }  
    }
}
