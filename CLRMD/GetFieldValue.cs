// *** Example: Attach to managed process and read object's field value ***
// Requires Microsoft.Diagnostics.Runtime (https://www.nuget.org/packages/Microsoft.Diagnostics.Runtime/)
//
// Author: MSDN-WhiteKnight (https://github.com/MSDN-WhiteKnight)

using System;
using Microsoft.Diagnostics.Runtime;

namespace ConsoleApplication1
{
    class Program
    { 
        static void Main(string[] args)
        {
            int pid = 1234;           
            
            DataTarget dt=DataTarget.AttachToProcess(pid,5000,AttachFlag.Passive); 

            using (dt)
            {
                //pick first CLR version
                ClrInfo runtimeInfo = dt.ClrVersions[0];
                ClrRuntime runtime = runtimeInfo.CreateRuntime();

                ClrType type;  
                
                //enumerate objects on managed heap                
                foreach (ulong obj in runtime.Heap.EnumerateObjectAddresses())
                {
                    type = runtime.Heap.GetObjectType(obj);

                    if (type == null) continue;

                    if (type.Name == "MyProject.MyClass") 
                    {
                        Console.WriteLine("Address 0x{0:X}: {1}", obj, type.Name);
                        ClrInstanceField f = type.GetFieldByName("Foo");
                        object val = f.GetValue(obj);
                        if (val != null) Console.WriteLine(val.ToString());
                    }
                }
            }

            Console.ReadKey();
        }
    } 
}
