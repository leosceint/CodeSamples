// *** Example: decompiling method's source code from assembly at runtime ***
// Requires ICSharpCode.Decompiler (https://www.nuget.org/packages/ICSharpCode.Decompiler)
//
// Author: MSDN-WhiteKnight (https://github.com/MSDN-WhiteKnight)

using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace ConsoleTest1
{
    class Program
    {
        public static AstNode FindNode(AstNode root,string match)
        {            
            //find method in syntax tree
            var children = root.Children;

            foreach (AstNode x in children)
            {
                var s = x.ToString();
                if (s == match && x.Parent.NodeType == NodeType.Member)
                {
                    return x.Parent;
                }

                AstNode res = FindNode(x, match);
                if (res != null) return res;
            }
            return null;
        }

        public static string GetSourceDecompiled<T>(Predicate<T> match)
        {        
            string module_path = match.Method.Module.FullyQualifiedName;            

            var settings = new DecompilerSettings();
            settings.AnonymousMethods = false; //disable anonymous methods inlining         

            var decompiler = new CSharpDecompiler( module_path, settings );            

            //decompile type that contains method
            SyntaxTree tree = decompiler.DecompileType(
                new ICSharpCode.Decompiler.TypeSystem.FullTypeName(match.Method.DeclaringType.FullName)
                );

            //find method in syntax tree
            var children = tree.Children.ToList();
            AstNode res = null;
            foreach (var x in children)
            {
                res = FindNode(x, match.Method.Name);
                if (res != null) break;
            }

            string s = "";
            if (res != null) s = res.ToString();
            return s;
        }        

        static void Main(string[] args)
        {
            string source;            
            source = GetSourceDecompiled<string>((s) => s == "Test" || s.Length==0);            
            Console.WriteLine(source);            

            Console.ReadKey();
        }
    }    
}
