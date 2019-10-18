//Sample: Extending form's client area to display menu in caption using DWM API
// *** Entry point ***
//Copyright (c) 2019, MSDN.WhiteKnight
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WindowsFormsApp1
{
	static class Program
	{      
        
        [STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
            var f1 = new Form1();            
            Application.Run(f1);           


            

        }
	}
}
