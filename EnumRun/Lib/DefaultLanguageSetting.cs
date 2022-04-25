using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EnumRun.Lib
{
    /// <summary>
    /// DefaultのLanguageの設定値を取得
    /// </summary>
    internal class DefaultLanguageSetting
    {
        public static List<Language> Create()
        {
            List<Language> list = new List<Language>();
            list.Add(new Language()
            {
                Name = "exe",
                Extensions = new string[] { ".exe" },
                Command = null,
            });
            list.Add(new Language()
            {
                Name = "cmd",
                Extensions = new string[] { ".bat", ".cmd" },
                Command = "cmd",
                ArgsPrefix = "/c \"",
                ArgsMidWithoutArgs = "\"",
                ArgsMidWithArgs = "\" ",
                ArgsSuffix = "",
            });
            list.Add(new Language()
            {
                Name = "PowerShell",
                Extensions = new string[] { ".ps1" },
                Command = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                ArgsPrefix = "-ExecutionPolicy Unrestricted -File \"",
                ArgsMidWithoutArgs = "\"",
                ArgsMidWithArgs = "\" ",
                ArgsSuffix = "",
            });
            list.Add(new Language()
            {
                Name = "Pwsh7",
                Description = "PowerShell v7",
                Extensions = new string[] { ".ps1" },
                Command = WhereCommand("pwsh") ?? @"C:\Program Files\PowerShell\7\pwsh.exe",
                ArgsPrefix = "-ExecutionPolicy Unrestricted -File \"",
                ArgsMidWithoutArgs = "\"",
                ArgsMidWithArgs = "\" ",
                ArgsSuffix = "",
            });
            list.Add(new Language()
            {
                Name = "WScript",
                Extensions = new string[] { ".vbs", ".vbe", ".js", ".jse", ".wsf", ".wsh" },
                Command = @"C:\Windows\System32\wscript.exe",
                ArgsPrefix = "//nologo \"",
                ArgsMidWithoutArgs = "\"",
                ArgsMidWithArgs = "\" ",
                ArgsSuffix = "",
            });
            list.Add(new Language()
            {
                Name = "Go",
                Extensions = new string[] { ".go" },
                Command = WhereCommand("go") ?? @"C:\Program Files\Go\bin\go.exe",
                ArgsPrefix = "\"",
                ArgsMidWithoutArgs = "\"",
                ArgsMidWithArgs = "\" ",
                ArgsSuffix = "",
            });
            list.Add(new Language()
            {
                Name = "Node.js",
                Extensions = new string[] { ".js" },
                Command = WhereCommand("node") ?? @"C:\Program Files\nodejs\node.exe",
                ArgsPrefix = "\"",
                ArgsMidWithoutArgs = "\"",
                ArgsMidWithArgs = "\" ",
                ArgsSuffix = "",
            });
            return list;
        }

        private static string WhereCommand(params string[] commands)
        {
            foreach (string command in commands)
            {
                using (var proc = new Process())
                {
                    proc.StartInfo.FileName = "where.exe";
                    proc.StartInfo.Arguments = command;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.Start();
                    string output_psCommandPath = proc.StandardOutput.ReadLine();     //最初の1行のみ取得
                    proc.WaitForExit();

                    return proc.ExitCode == 0 ? output_psCommandPath.Trim() : null;
                }
            }
            return null;
        }
    }
}
