// *** Example: Waiting for event without blocking UI in .NET 3.5 WinForms ***
// https://github.com/MSDN-WhiteKnight/CodeSamples

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WindowsFormsTest
{
    public partial class Form1 : Form
    {      
        public Form1()
        {
            InitializeComponent();            
        }  

        [DllImport("user32.dll")]
        static extern int MsgWaitForMultipleObjectsEx(uint nCount, IntPtr[] pHandles,
           uint dwMilliseconds, uint dwWakeMask, uint dwFlags);

        const uint QS_ALLEVENTS = 1215;
        const int WAIT_OBJECT_0 = 0;
        const int WAIT_FAILED = -1;
        const uint INFINITE = 0xFFFFFFFF;

        public static void AwaitEvent(object o, string evt)
        {
            if (o == null || evt == null) throw new ArgumentNullException("Arguments cannot be null");

            EventInfo einfo = o.GetType().GetEvent(evt);
            if (einfo == null)
            {
                throw new ArgumentException(String.Format("*{0}* has no *{1}* event", o, evt));
            }

            ManualResetEvent wh = new ManualResetEvent(false);
            MethodInfo mi = null;
            Delegate deleg = null;
            EventHandler handler = null;            

            //код обработчика события
            handler = (s, e) =>
            {
                mi = handler.Method;
                deleg = Delegate.CreateDelegate(einfo.EventHandlerType, handler.Target, mi);
                einfo.RemoveEventHandler(s, deleg); //отцепляем обработчик события
                wh.Set();
            };

            mi = handler.Method;
            deleg = Delegate.CreateDelegate(einfo.EventHandlerType, handler.Target, mi); //получаем делегат нужного типа
            einfo.AddEventHandler(o, deleg); //присоединяем обработчик события            

            var swh = wh.SafeWaitHandle;

            using (swh)
            {
                IntPtr h = swh.DangerousGetHandle();

                while (true)
                {
                    int res = MsgWaitForMultipleObjectsEx(1, new IntPtr[] { h }, INFINITE, QS_ALLEVENTS, 0);
                    switch (res)
                    {
                        case WAIT_OBJECT_0:
                            return;
                        case WAIT_OBJECT_0 + 1:
                            Application.DoEvents();
                            break;
                        default:
                            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 f = new Form2();
            f.Show();
            
            AwaitEvent(f, "FormClosed");
            MessageBox.Show("Finished!");
         }        
    }   
}

