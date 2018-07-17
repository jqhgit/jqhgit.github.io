using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace dynamiccompile
{
    public class DynamicAssemblyDef
    {
        public string m_do_namespace;
        public string m_do_class;
        public string m_do_method;
        public bool m_do_method_static;
        public object[] m_do_params = null;

        public string[] m_code;
    }
    public class Program : MarshalByRefObject
    {
        public static int sss = 111;
        public static int Plus(int a, int b)
        {
            Console.WriteLine("Program Original static fun(Plus): done!" + sss.ToString());
            return a + b;
        }
        public static void print()
        {
            Console.WriteLine("Program Original static fun(print): done!");
        }

        private static List<DynamicAssemblyDef> m_dynamic_codes = new List<DynamicAssemblyDef>();
        private static CompilerParameters m_cpParameters;

        public static void LoadDynamicCodeConfig()
        {
            m_dynamic_codes.Clear();
            string configdir = Path.Combine(System.Environment.CurrentDirectory,"config");
            string[] files = Directory.GetFiles(configdir);
            
            foreach (string file in files)
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                StreamReader read = new StreamReader(fs);
                bool head_session = false;
                bool head_read = false;

                DynamicAssemblyDef assembly = new DynamicAssemblyDef();
                List<string> code = new List<string>();
                while (!read.EndOfStream)
                {
                    string line = read.ReadLine().Trim();
                    if (line.Length <= 0)
                        continue;

                    if (!head_read && null != line && line.Length > 2 && line.Substring(0,2) == "//")
                    {
                        line = line.Substring(2);
                    }

                    if (line.ToLower().Equals("[head]"))
                    {
                        head_session = true;
                        continue;
                    }
                    else if (line.ToLower().Equals("[end]"))
                    {
                        head_session = false;
                        head_read = true;
                        continue;
                    }

                    if (head_session)
                    {
                        string[] res = line.Split(':');
                        if (res.Length < 2)
                        {
                            continue;
                        }

                        if (res[0].Trim().ToLower().Equals("namespace"))
                        {
                            assembly.m_do_namespace = res[1].Trim();
                        }
                        else if (res[0].Trim().ToLower().Equals("class"))
                        {
                            assembly.m_do_class = res[1].Trim();
                        }
                        else if (res[0].Trim().ToLower().Equals("method"))
                        {
                            assembly.m_do_method = res[1].Trim();

                            if(res.Length == 3 && res[2].Trim().ToLower().Equals("s"))
                                assembly.m_do_method_static = true;
                            else
                                assembly.m_do_method_static = false;
                        }
                    }
                    else if(head_read)
                    {
                        code.Add(line);
                    }
                }

                if (head_read && code.Count > 0)
                {
                    assembly.m_code = code.ToArray();
                    m_dynamic_codes.Add(assembly);
                }
                read.Close();
                fs.Close();
            }
        }


        public static void mainLoop()
        {
            Assembly curAsb = Assembly.GetExecutingAssembly();
            string[] assembly_ref = new string[1];
            assembly_ref[0] = curAsb.Location;

            m_cpParameters = new CompilerParameters(assembly_ref);
            m_cpParameters.GenerateExecutable = false;
            m_cpParameters.GenerateInMemory = true;
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            foreach (DynamicAssemblyDef dad in m_dynamic_codes)
            {
                string code = string.Join("\n",dad.m_code);
                m_cpParameters.OutputAssembly = dad.m_do_class+".dll";
                CompilerResults cpres = provider.CompileAssemblyFromSource(m_cpParameters, code);
                if (cpres.Errors.HasErrors)                            //如果有错误   
                {
                    Console.WriteLine("编译错误: ");
                    foreach (CompilerError error in cpres.Errors)
                    {
                        Console.WriteLine("error:\nfile:{0} error_code:{1} line:{2} \n{3}\n",error.FileName, error.ErrorNumber, error.Line, error.ErrorText);
                    }
                    continue;
                }
                Assembly asb = cpres.CompiledAssembly;

                MethodInfo printMethodInfo = null;
                if (dad.m_do_method_static)
                {
                    Type cls = asb.GetType($"{dad.m_do_namespace}.{dad.m_do_class}");
                    printMethodInfo = cls.GetMethod($"{dad.m_do_method}");
                    printMethodInfo.Invoke(null, dad.m_do_params);
                }
                else
                {
                    object clsins = asb.CreateInstance($"{dad.m_do_namespace}.{dad.m_do_class}");
                    Type testType = clsins.GetType();
                    printMethodInfo = testType.GetMethod($"{dad.m_do_method}");
                    printMethodInfo.Invoke(clsins, dad.m_do_params);
                }
                asb = null;
                cpres = null;
            }
            provider = null;
        }


        static void Main(string[] args)
        {
            sss = 2;
            Console.Write("Enter any key to test(exit to end)>:");
            while (Console.ReadLine() != "exit")
            {
                Console.Write("Enter any key to test(exit to end)>:");

                //AppDomain domain = AppDomain.CreateDomain("CompileDoMain");
                //Program myprogram = (Program)domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().GetName().FullName, "dynamiccompile.Program");
                //myprogram.LoadDynamicCodeConfig();
                //myprogram.mainLoop();
                //AppDomain.Unload(domain);

                
                // assembly不能卸载，多次执行compile内存会增长
                LoadDynamicCodeConfig();
                mainLoop();
            }
        }
    }
}
