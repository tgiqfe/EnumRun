using System.Diagnostics;

namespace EnumRun.Lib
{
    public class Language
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Extensions { get; set; }
        public string Command { get; set; }
        public string Command_x86 { get; set; }
        public string ArgsPrefix { get; set; }
        public string ArgsMidWithoutArgs { get; set; }
        public string ArgsMidWithArgs { get; set; }
        public string ArgsSuffix { get; set; }

        public Language() { }

        /// <summary>
        /// ToStringをオーバーライド。cmd[.bat .cmd] のように表示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}[{1}]", this.Name, string.Join(" ", Extensions));
        }

        public Process GetProcess(string scriptFile, string arguments)
        {
            if (Command == null)
            {
                return new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = scriptFile,
                        Arguments = arguments,
                    },
                };
            }
            else
            {
                return new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = Environment.Is64BitProcess || string.IsNullOrEmpty(Command_x86) ?
                            Command : Command_x86,
                        Arguments = string.IsNullOrEmpty(arguments) ?
                            string.Format("{0}{1}{2}",
                                ArgsPrefix, scriptFile, ArgsMidWithoutArgs) :
                            string.Format("{0}{1}{2}{3}{4}",
                                ArgsPrefix, scriptFile, ArgsMidWithArgs, arguments, ArgsSuffix),
                    }
                };
            }
        }
    }
}
