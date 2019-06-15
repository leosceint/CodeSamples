// Example: Parsing JSON in C# using Internet Explorer COM object
// Requires Internet Explorer 9+
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ConsoleApplication1
{
    class Program
    {
        static Dictionary<string,object> GetObjectProperties(object o)
        {
            var res = new Dictionary<string, object>();
            
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(o))
            {
               res.Add(prop.Name,prop.GetValue(o));
            }
            
            return res;
        }     

        [STAThread]
        static void Main(string[] args)
        {            
            dynamic html = Activator.CreateInstance(Type.GetTypeFromProgID("htmlfile"));
            try
            {
                html.open();
                html.write("<html><head><meta http-equiv=\"x-ua-compatible\" content=\"IE=9\" /></head><body></body></html>");

                dynamic JSON = html.defaultView.JSON;
                dynamic obj = JSON.parse(
                    "{\"a\":true, \"b\":10, \"c\": \"Hello\"}"
                    );

                var dict = GetObjectProperties(obj);
                foreach (string prop in dict.Keys)
                {
                    Console.WriteLine(prop + ": " + dict[prop].ToString());
                }
            }
            finally
            {
                html.close();
            }
            
            Console.ReadKey();
        }
    }    
}

