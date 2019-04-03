//Code sample: dynamically compiling C# code using Roslyn API
//
//Copyright MSDN-WhiteKnight (https://github.com/MSDN-WhiteKnight), 2019

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RoslynTest
{
    class Program
    {
        static void RunScript()
        {
            var script = @"using System;
                public static class Program
                {
                    public static int Main(string[] args)
                    {
                        var x = 7 * 8;
                        Console.WriteLine(x.ToString());
                        return x;
                    }
                }";

            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var refs = new List<PortableExecutableReference>
            {
                 MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                 MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                 MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Assembly.GetEntryAssembly().Location)
            };


            // Parse the script to a SyntaxTree
            var syntaxTree = CSharpSyntaxTree.ParseText(script);
            var options = new CSharpCompilationOptions(OutputKind.ConsoleApplication);
            // Compile the SyntaxTree to a CSharpCompilation
            var compilation = CSharpCompilation.Create("Script",
                new[] { syntaxTree },
                refs,
                new CSharpCompilationOptions(
                    OutputKind.ConsoleApplication,
                    optimizationLevel: OptimizationLevel.Release,                    
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default)
                    );

            var result = compilation.Emit("script.exe");
            if (!result.Success)
            {
                throw new ApplicationException("Cannot compile script");
            }            

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "script.exe";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true; 
            psi.UserName = "Vasya";
            psi.Password = "123";

            var process = new Process();
            using (process)
            {
                process.StartInfo = psi;                
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    string res = process.StandardOutput.ReadLine();                    
                    Console.WriteLine(res);
                }
            }
        }
    }
}
