using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Trader
{
    public class ScriptConverter : IConverter
    {
        public ScriptConverter(string arguments, string interpreter = "py")
        {
            Script = new Process
            {
                StartInfo = new ProcessStartInfo(interpreter)
                {
                    Arguments = arguments,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            Script.Start();
        }

        public Process Script { get; }
        private readonly object _key = new object();

        public string Convert(string json)
        {
            lock (_key)
            {
                Script.StandardInput.WriteLine(json);
                //Task.Factory.StartNew(DumpError);
                //var line = Script.StandardOutput.ReadLine();
                ////Console.WriteLine(line);
                //return line;
                return Script.StandardOutput.ReadLine();
            }
        }
        private void DumpError()
        {
            Console.Error.WriteLine(Script.StandardError.ReadToEnd());
        }
    }
}
