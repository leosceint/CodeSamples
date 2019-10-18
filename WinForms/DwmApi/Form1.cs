//Sample: Extending form's client area to display menu in caption using DWM API
// *** Main Form ***
//Copyright (c) 2019, MSDN.WhiteKnight
//Based on: https://docs.microsoft.com/en-us/windows/win32/dwm/customframe
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {      
        //client size margins
        const int TOPEXTENDWIDTH = 1;
        const int BOTTOMEXTENDWIDTH = 30;
        const int LEFTEXTENDWIDTH = 1;
        const int RIGHTEXTENDWIDTH = 1;

        //WinAPI
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyBottomHeight;
            public int cyTopHeight;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left, top, right, bottom;
        }
        
        [DllImport("dwmapi.dll")]
        static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool AdjustWindowRectEx(ref RECT lpRect, uint dwStyle,
        bool bMenu, uint dwExStyle);

        [DllImport("dwmapi.dll")]
        static extern bool DwmDefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref IntPtr plResult);

        static int GET_X_LPARAM(IntPtr lp)
        {
            short loword = (short)((ulong)lp & 0xffff);
            return loword;
        }

        static int GET_Y_LPARAM(IntPtr lp)
        {
            short hiword = (short)((((ulong)lp)>>16) & 0xffff);
            return hiword;
        }

        const uint WM_NCCALCSIZE = 0x0083;
        const uint WM_NCHITTEST = 0x0084;

        const uint WS_OVERLAPPED = 0x00000000;
        const uint WS_CAPTION = 0x00C00000;
        const uint WS_SYSMENU = 0x00080000;
        const uint WS_THICKFRAME = 0x00040000;
        const uint WS_MINIMIZEBOX = 0x00020000;
        const uint WS_MAXIMIZEBOX = 0x00010000;
        const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU |
              WS_THICKFRAME |  WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
               
        const uint HTTOPLEFT = 13;
        const uint HTTOPRIGHT = 14;
        const uint HTTOP = 12;
        const uint HTCAPTION = 2;
        const uint HTLEFT = 10;
        const uint HTNOWHERE = 0;
        const uint HTRIGHT = 11;
        const uint HTBOTTOM = 15;
        const uint HTBOTTOMLEFT = 16;
        const uint HTBOTTOMRIGHT = 17;

        //handle mouse clicks
        static IntPtr HitTestNCA(IntPtr hWnd, IntPtr wParam, IntPtr lParam)
        {
            // Get the point coordinates for the hit test.
            var ptMouse = new Point(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));

            // Get the window rectangle.
            RECT rcWindow;
            GetWindowRect(hWnd, out rcWindow);

            // Get the frame rectangle, adjusted for the style without a caption.
            RECT rcFrame = new RECT();
            AdjustWindowRectEx(ref rcFrame, WS_OVERLAPPEDWINDOW & ~WS_CAPTION, false, 0);

            // Determine if the hit test is for resizing. Default middle (1,1).
            ushort uRow = 1;
            ushort uCol = 1;
            bool fOnResizeBorder = false;

            // Determine if the point is at the top or bottom of the window.
            if (ptMouse.Y >= rcWindow.top && ptMouse.Y < rcWindow.top + BOTTOMEXTENDWIDTH)
            {
                fOnResizeBorder = (ptMouse.Y < (rcWindow.top - rcFrame.top));
                uRow = 0;
            }
            else if (ptMouse.Y < rcWindow.bottom && ptMouse.Y >= rcWindow.bottom - TOPEXTENDWIDTH)
            {
                uRow = 2;
            }

            // Determine if the point is at the left or right of the window.
            if (ptMouse.X >= rcWindow.left && ptMouse.X < rcWindow.left + LEFTEXTENDWIDTH)
            {
                uCol = 0; // left side
            }
            else if (ptMouse.X < rcWindow.right && ptMouse.X >= rcWindow.right - RIGHTEXTENDWIDTH)
            {
                uCol = 2; // right side
            }

            // Hit test (HTTOPLEFT, ... HTBOTTOMRIGHT)
            IntPtr[,] hitTests = new IntPtr[,]
            {
                { (IntPtr)HTTOPLEFT, fOnResizeBorder? (IntPtr)HTTOP : (IntPtr)HTCAPTION, (IntPtr)HTTOPRIGHT },
                { (IntPtr)HTLEFT,  (IntPtr)HTNOWHERE, (IntPtr)HTRIGHT},
                { (IntPtr)HTBOTTOMLEFT, (IntPtr)HTBOTTOM, (IntPtr)HTBOTTOMRIGHT },
            };

            return hitTests[uRow, uCol];
        }

        public Form1()
        {
            InitializeComponent();
            
            foreach (ToolStripMenuItem item in menuStrip1.Items)
            {
                item.Paint += Item_Paint;
            }
        }

        private void Item_Paint(object sender, PaintEventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item == null) return;
            
            //elements on the former Non-client are need to be drawn manually
            e.Graphics.FillRectangle(SystemBrushes.Control, 2, 2, item.Width - 4,item.Height - 4);
            e.Graphics.DrawString(item.Text, SystemFonts.DefaultFont, SystemBrushes.ControlText, 2, 2);
        }
        
        bool dwminit = false;
        private void Form1_Activated(object sender, EventArgs e)
        {
            if (dwminit == false)
            {
                // Extend the frame into the client area.
                MARGINS margins = new MARGINS();

                margins.cxLeftWidth = LEFTEXTENDWIDTH;
                margins.cxRightWidth = RIGHTEXTENDWIDTH;
                margins.cyBottomHeight = BOTTOMEXTENDWIDTH;
                margins.cyTopHeight = TOPEXTENDWIDTH;

                int hr = DwmExtendFrameIntoClientArea(this.Handle, ref margins);
                dwminit = true;

                if (hr != 0)
                {
                    throw Marshal.GetExceptionForHR(hr);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            bool fCallDWP = true;
            IntPtr lRet = IntPtr.Zero;
            fCallDWP = !DwmDefWindowProc(m.HWnd, m.Msg, m.WParam, m.LParam, ref lRet);

            if (m.Msg == WM_NCCALCSIZE)
            {
                if (m.WParam != (IntPtr)0)
                {
                    //remove standard frame
                    lRet = IntPtr.Zero;                    
                    fCallDWP = false;
                }
            }

            if (m.Msg == WM_NCHITTEST && lRet==IntPtr.Zero)
            {
                //handle mouse clicks
                lRet = HitTestNCA(m.HWnd, m.WParam, m.LParam);

                if (lRet != (IntPtr)HTNOWHERE)
                {
                    fCallDWP = false;
                }
            }

            m.Result = lRet;

            //pass to base class if unhandled
            if (fCallDWP) base.WndProc(ref m);
        }             

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //workaround to display correctly in Windows 10, because it prematurely whitens all non-client area            

            e.Graphics.FillRectangle(Brushes.Black, 0, 0, this.Width, BOTTOMEXTENDWIDTH);
            e.Graphics.FillRectangle(Brushes.Black, this.Width - RIGHTEXTENDWIDTH, 0, RIGHTEXTENDWIDTH, this.Height);
            e.Graphics.FillRectangle(Brushes.Black, 0, 0, LEFTEXTENDWIDTH, this.Height);
            e.Graphics.FillRectangle(Brushes.Black, 0, this.Height- TOPEXTENDWIDTH, this.Width, TOPEXTENDWIDTH);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //workaround for correct repaint on window resize
            this.Invalidate();
        }

        //other handlers...

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("hi");
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("new");
        }
    }
}
