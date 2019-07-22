// *** Example: Reading PDB file and obtaining method's source line numbers via CLRMD ***
// Requires Microsoft.Diagnostics.Runtime (https://www.nuget.org/packages/Microsoft.Diagnostics.Runtime/)
//
// Author: MSDN-WhiteKnight (https://github.com/MSDN-WhiteKnight)

using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Utilities.Pdb;

namespace ConsoleTest1
{
    class Program
    {
        public static string GetSourceFromPdb<T>(Predicate<T> match)
        {            
            int token = match.Method.MetadataToken;

            //construct symbols file path
            string module_path = match.Method.Module.FullyQualifiedName;
            string pdb_path = Path.Combine(
                Path.GetDirectoryName(module_path),
                Path.GetFileNameWithoutExtension(module_path) + ".pdb"
                );            

            StringBuilder sb = new StringBuilder();
            PdbReader reader = new PdbReader(pdb_path);

            using (reader)
            {
                //find method in symbols
                var func = reader.GetFunctionFromToken((uint)token);                

                foreach (PdbSequencePointCollection coll in func.SequencePoints)
                {
                    //read source file
                    string[] lines = File.ReadAllLines(coll.File.Name, System.Text.Encoding.UTF8);                    

                    //find method beginning & end source lines
                    var points_sorted = coll.Lines.
                        Where<PdbSequencePoint>((x)=> x.LineBegin <= lines.Length && x.LineEnd<=lines.Length).
                        OrderBy<PdbSequencePoint, uint>((x) => x.Offset);
                    PdbSequencePoint start = points_sorted.First();
                    PdbSequencePoint end = points_sorted.Last();   

                    bool reading = false;
                    int index_start;                    
                    int index_end;

                    //read method's code from source
                    for(int i=1; i<=lines.Length;i++)
                    {
                        string line = lines[i-1];
                        index_start = 0;
                        index_end = line.Length;

                        if (!reading)
                        {
                            if (i >= start.LineBegin)
                            {
                                //first line
                                reading = true;
                                index_start = start.ColBegin - 1;
                                if (index_start < 0) index_start = 0;                    
                            }
                        }                        

                        if (reading)
                        {
                            if (i >= end.LineEnd)
                            {
                                //last line
                                index_end = end.ColEnd - 1;
                                if (index_end > line.Length) index_end = line.Length;

                                sb.AppendLine(line.Substring(index_start, index_end - index_start));
                                break;
                            }

                            //read current line
                            sb.AppendLine(line.Substring(index_start, index_end - index_start));
                        }  
                    }      
                }

            }

            return sb.ToString();
        }
        
        static void Main(string[] args)
        {
            string source;            
            source = GetSourceFromPdb<string>( (s) =>  s == "Test"  ||  s.Length==0);            
            Console.WriteLine(source); 
            Console.ReadKey();
        }
    }    
}
