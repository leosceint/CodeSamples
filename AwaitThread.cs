// Code sample: Creating Task from System.Threading.Thread to use with await
// Author: MSDN.WhiteKnight
// Idea: VladD

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

namespace ConsoleApp1
{
    public class MyWaitHandle : WaitHandle
    {
        public MyWaitHandle(Microsoft.Win32.SafeHandles.SafeWaitHandle swh) : base()
        {            
            this.SafeWaitHandle = swh;            
        }
    }

    class Program
    {
        static void ThreadProc()
        {
            Console.WriteLine("Hello");
            Thread.Sleep(2500);
            Console.WriteLine("Goodbye");
        }        

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200),
            SYNCHRONIZE = 0x00100000
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle,uint dwThreadId);
                

        public static Task<object> GetTaskFromThread(Thread th)
        {
            MethodInfo mi = typeof(Thread).GetMethod("GetNativeHandle", BindingFlags.NonPublic | 
                BindingFlags.Instance);
            object thh = mi.Invoke(th, new object[0]);
            FieldInfo fi = thh.GetType().GetField("m_ptr", BindingFlags.NonPublic | BindingFlags.Instance);
            IntPtr clrhandle = (IntPtr)fi.GetValue(thh);

            uint nativeId = (uint)Marshal.ReadInt32(clrhandle, (IntPtr.Size == 8) ? 0x022C : 0x0160); //hack (Source: https://stackoverflow.com/a/46791587/8674428)           
            IntPtr handle = OpenThread(ThreadAccess.SYNCHRONIZE, false, nativeId);            

            var swh = new Microsoft.Win32.SafeHandles.SafeWaitHandle(handle, true);
            MyWaitHandle wh = new MyWaitHandle(swh);

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            WaitOrTimerCallback cb = (a, b) =>
            {
                tcs.TrySetResult(null); 
            };

            ThreadPool.RegisterWaitForSingleObject(wh, cb, null, 999999, true);
            return tcs.Task;
        }
        
        static void Main(string[] args)
        {
            AsyncMain().Wait();
            Console.ReadKey();
        }

        static async Task AsyncMain()
        {
            Thread th = new Thread(ThreadProc);
            th.Start();
            Console.WriteLine("Thread started");

            await GetTaskFromThread(th);
            Console.WriteLine("Thread finished");
            
        }      
    }
}
