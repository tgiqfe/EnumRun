using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Security.Principal;
using System.Security.AccessControl;
using Microsoft.Win32;

namespace EnumRun.Lib
{
    internal class RegistryControl
    {
        /// <summary>
        /// レジストリパスからルート部分を取得
        /// </summary>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        public static RegistryKey GetRootKey(string rootPath)
        {
            if (rootPath.Contains("\\"))
            {
                rootPath = rootPath.Substring(0, rootPath.IndexOf("\\"));
            }
            switch (rootPath)
            {
                case "HKCR":
                case "HKCR:":
                case "HKEY_CLASSES_ROOT":
                    return Microsoft.Win32.Registry.ClassesRoot;
                case "HKCU":
                case "HKCU:":
                case "HKEY_CURRENT_USER":
                    return Microsoft.Win32.Registry.CurrentUser;
                case "HKLM":
                case "HKLM:":
                case "HKEY_LOCAL_MACHINE":
                    return Microsoft.Win32.Registry.LocalMachine;
                case "HKU":
                case "HKU:":
                case "HKEY_USERS":
                    return Microsoft.Win32.Registry.Users;
                case "HKCC":
                case "HKCC:":
                case "HKEY_CURRENT_CONFIG:":
                    return Microsoft.Win32.Registry.CurrentConfig;
            }
            return null;
        }

        /// <summary>
        /// RegistryKeyインスタンスを生成
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isCreate"></param>
        /// <param name="writable"></param>
        /// <returns></returns>
        public static RegistryKey GetRegistryKey(string path, bool isCreate, bool writable)
        {
            if (path.Contains("\\"))
            {
                string keyName = path.Substring(path.IndexOf("\\") + 1);
                return isCreate ?
                    GetRootKey(path).CreateSubKey(keyName, writable) :
                    GetRootKey(path).OpenSubKey(keyName, writable);
            }
            return isCreate ?
                GetRootKey(path).CreateSubKey("", writable) :
                GetRootKey(path).OpenSubKey("", writable);
        }

        /// <summary>
        /// キーを再帰的にコピー。
        /// </summary>
        /// <param name="sourceKey"></param>
        /// <param name="destinationKey"></param>
        /// <param name="excludeKey"></param>
        public static void CopyRegistryKey(RegistryKey sourceKey, RegistryKey destinationKey, string[] excludeKey)
        {
            foreach (string paramName in sourceKey.GetValueNames())
            {
                RegistryValueKind valueKind = sourceKey.GetValueKind(paramName);
                destinationKey.SetValue(
                    paramName,
                    valueKind == RegistryValueKind.ExpandString ?
                        sourceKey.GetValue(paramName, null, RegistryValueOptions.DoNotExpandEnvironmentNames) :
                        sourceKey.GetValue(paramName),
                    valueKind);
            }
            foreach (string childKeyName in sourceKey.GetSubKeyNames())
            {
                //  除外キー
                string childKeyPath = sourceKey.Name + "\\" + childKeyName;
                if (excludeKey?.Any(x => childKeyPath.StartsWith(x, StringComparison.OrdinalIgnoreCase)) ?? false)
                {
                    continue;
                }

                using (RegistryKey subSrcKey = sourceKey.OpenSubKey(childKeyName, false))
                using (RegistryKey subDstKey = destinationKey.CreateSubKey(childKeyName, true))
                {
                    try
                    {
                        CopyRegistryKey(subSrcKey, subDstKey, excludeKey);
                    }
                    catch (System.Security.SecurityException)
                    {
                        //  アクセス権無しの為にアクセス拒否
                    }
                    catch (UnauthorizedAccessException)
                    {
                        //  読み取り専用の為に書き込み拒否
                    }
                    catch (ArgumentException)
                    {
                        //  Registry.SetValueで書き込みできないレジストリパス/値への対策
                        //  reg copyコマンドでコピー実行
                        using (Process proc = new Process())
                        {
                            proc.StartInfo.FileName = "reg.exe";
                            proc.StartInfo.Arguments = $@"copy ""{subSrcKey.ToString()}"" ""{subDstKey.ToString()}"" /s /f";
                            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            proc.Start();
                            proc.WaitForExit();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// AccessControlRuleから文字列に変換
        /// </summary>
        /// <param name="rules"></param>
        /// <returns></returns>
        public static string AccessRulesToString(AuthorizationRuleCollection rules)
        {
            var accessRuleList = new List<string>();
            foreach (RegistryAccessRule rule in rules)
            {
                accessRuleList.Add(string.Format("{0};{1};{2};{3};{4}",
                    rule.IdentityReference.Value,
                    rule.RegistryRights.ToString(),
                    rule.InheritanceFlags.ToString(),
                    rule.PropagationFlags.ToString(),
                    rule.AccessControlType.ToString()));
            }
            return string.Join("/", accessRuleList);
        }

        /// <summary>
        /// 文字列からAccessControlに変換
        /// </summary>
        /// <param name="accessString"></param>
        /// <returns></returns>
        public static List<RegistryAccessRule> StringToAccessRules(string accessString)
        {
            var ruleList = new List<RegistryAccessRule>();
            foreach (string ruleStr in accessString.Split('/'))
            {
                string[] fields = ruleStr.Split(';');
                ruleList.Add(new RegistryAccessRule(
                    new NTAccount(fields[0]),
                    Enum.TryParse(fields[1], out RegistryRights tempRights) ? tempRights : RegistryRights.ReadKey,
                    Enum.TryParse(fields[2], out InheritanceFlags tempInheritance) ? tempInheritance : InheritanceFlags.ContainerInherit,
                    Enum.TryParse(fields[3], out PropagationFlags tempPropagation) ? tempPropagation : PropagationFlags.None,
                    Enum.TryParse(fields[4], out AccessControlType tempAccessControlType) ? tempAccessControlType : AccessControlType.Allow));
            }
            return ruleList;
        }

        /// <summary>
        /// 文字列からbyte配列に変換 (REG_BINARY用。解凍とテキストブロックにも対応)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] StringToRegBinary(string text, bool expand)
        {
            byte[] bytes = new byte[0] { };
            if (text.Contains("\n"))
            {
                text = Regex.Replace(text, "\r?\n", "");
            }
            if (Regex.IsMatch(text, @"^[0-9a-fA-F]+$"))
            {
                List<byte> tempBytes = new List<byte>();
                for (int i = 0; i < text.Length / 2; i++)
                {
                    tempBytes.Add(Convert.ToByte(text.Substring(i * 2, 2), 16));
                }
                bytes = tempBytes.ToArray();
            }

            if (expand)
            {
                int BUFF_SIZE = 4096;

                using (var msS = new System.IO.MemoryStream(bytes))
                using (var msD = new System.IO.MemoryStream())
                {
                    using (var gs = new System.IO.Compression.GZipStream(msS, System.IO.Compression.CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[BUFF_SIZE];
                        int readed = 0;
                        while ((readed = gs.Read(buffer, 0, BUFF_SIZE)) > 0)
                        {
                            msD.Write(buffer, 0, readed);
                        }
                    }
                    return msD.ToArray();
                }
            }

            return bytes;
        }

        /// <summary>
        /// レジストリ値の種類を変換 (string ⇒ RegistryValueKind)
        /// </summary>
        /// <param name="valueKindString"></param>
        /// <returns></returns>
        public static RegistryValueKind StringToValueKind(string valueKindString)
        {
            if (!string.IsNullOrEmpty(valueKindString))
            {
                switch (valueKindString.ToUpper())
                {
                    case "REG_SZ": return RegistryValueKind.String;
                    case "REG_BINARY": return RegistryValueKind.Binary;
                    case "REG_DWORD": return RegistryValueKind.DWord;
                    case "REG_QWORD": return RegistryValueKind.QWord;
                    case "REG_MULTI_SZ": return RegistryValueKind.MultiString;
                    case "REG_EXPAND_SZ": return RegistryValueKind.ExpandString;
                    case "REG_NONE": return RegistryValueKind.None;
                }
            }
            return RegistryValueKind.String;
        }

        /// <summary>
        /// レジストリ値の種類を返還 (RegistryValueKind ⇒ string)
        /// </summary>
        /// <param name="valueKind"></param>
        /// <returns></returns>
        public static string ValueKindToString(RegistryValueKind valueKind)
        {
            switch (valueKind)
            {
                case RegistryValueKind.String: return "REG_SZ";
                case RegistryValueKind.Binary: return "REG_BINARY";
                case RegistryValueKind.DWord: return "REG_DWORD";
                case RegistryValueKind.QWord: return "REG_QWORD";
                case RegistryValueKind.MultiString: return "REG_MULTI_SZ";
                case RegistryValueKind.ExpandString: return "REG_EXPAND_SZ";
                case RegistryValueKind.None: return "REG_NONE";
            }
            return "REG_SZ";
        }

        /// <summary>
        /// レジストリ値を文字列に変換
        /// </summary>
        /// <param name="regKey"></param>
        /// <param name="name"></param>
        /// <param name="valuekind"></param>
        /// <param name="noResolv"></param>
        /// <returns></returns>
        public static string RegistryValueToString(RegistryKey regKey, string name, RegistryValueKind valueKind, bool noResolv)
        {
            switch (valueKind)
            {
                case RegistryValueKind.String:
                    return regKey.GetValue(name) as string;
                case RegistryValueKind.DWord:
                case RegistryValueKind.QWord:
                    return regKey.GetValue(name).ToString();
                case RegistryValueKind.ExpandString:
                    return noResolv ?
                        regKey.GetValue(name, "", RegistryValueOptions.DoNotExpandEnvironmentNames) as string :
                        regKey.GetValue(name) as string;
                case RegistryValueKind.Binary:
                    return BitConverter.ToString(regKey.GetValue(name) as byte[]).Replace("-", "").ToUpper();
                case RegistryValueKind.MultiString:
                    return string.Join("\\0", regKey.GetValue(name) as string[]);
                case RegistryValueKind.None:
                default:
                    return null;
            }
        }

        /// <summary>
        /// レジストリ値を文字列に変換
        /// (REG_BINNARYの圧縮とテキストブロック化に対応)
        /// </summary>
        /// <param name="regKey"></param>
        /// <param name="name"></param>
        /// <param name="valueKind"></param>
        /// <param name="noResolv"></param>
        /// <param name="compression"></param>
        /// <param name="textBlock"></param>
        /// <returns></returns>
        public static string RegistryValueToString(RegistryKey regKey, string name, RegistryValueKind valueKind, bool noResolv, bool compress, int textBlock)
        {
            switch (valueKind)
            {
                case RegistryValueKind.String:
                    return regKey.GetValue(name) as string;
                case RegistryValueKind.DWord:
                case RegistryValueKind.QWord:
                    return regKey.GetValue(name).ToString();
                case RegistryValueKind.ExpandString:
                    return noResolv ?
                        regKey.GetValue(name, "", RegistryValueOptions.DoNotExpandEnvironmentNames) as string :
                        regKey.GetValue(name) as string;
                case RegistryValueKind.Binary:
                    string retText = "";
                    if (compress)
                    {
                        int BUFF_SIZE = 4096;


                        using (var msS = new System.IO.MemoryStream(regKey.GetValue(name) as byte[]))
                        using (var msD = new System.IO.MemoryStream())
                        {
                            using (var gs = new System.IO.Compression.GZipStream(msD, System.IO.Compression.CompressionMode.Compress))
                            {
                                byte[] buffer = new byte[BUFF_SIZE];
                                int readed = 0;
                                while ((readed = msS.Read(buffer, 0, BUFF_SIZE)) > 0)
                                {
                                    gs.Write(buffer, 0, readed);
                                }
                            }
                            retText = BitConverter.ToString(msD.ToArray()).Replace("-", "");
                        }
                    }
                    else
                    {
                        retText = BitConverter.ToString(regKey.GetValue(name) as byte[]).Replace("-", "").ToUpper();
                    }

                    if (textBlock > 0)
                    {
                        int count = textBlock;
                        StringBuilder sb = new StringBuilder();
                        using (var sr = new System.IO.StringReader(retText))
                        {
                            int readed = 0;
                            char[] buffer = new char[count];
                            while ((readed = sr.Read(buffer, 0, count)) > 0)
                            {
                                sb.AppendLine(new string(buffer, 0, readed));
                            }
                        }
                        retText = sb.ToString().TrimEnd('\n').TrimEnd('\r');
                    }
                    return retText;
                case RegistryValueKind.MultiString:
                    return string.Join("\\0", regKey.GetValue(name) as string[]);
                case RegistryValueKind.None:
                default:
                    return null;
            }
        }

        /// <summary>
        /// レジストリ値をバイト配列に変換。こちらはREG_BINARYでも圧縮対応する予定は無し
        /// </summary>
        /// <param name="regKey"></param>
        /// <param name="name"></param>
        /// <param name="valueKind"></param>
        /// <param name="noResolv"></param>
        /// <returns></returns>
        public static byte[] RegistryValueToBytes(RegistryKey regKey, string name, RegistryValueKind? valueKind, bool noResolv)
        {
            if (valueKind == null)
            {
                valueKind = regKey.GetValueKind(name);
            }
            switch (valueKind)
            {
                case RegistryValueKind.String:
                    return Encoding.UTF8.GetBytes((regKey.GetValue(name) as string) ?? "");
                case RegistryValueKind.DWord:
                    return BitConverter.GetBytes((regKey.GetValue(name) as int?) ?? 0);
                case RegistryValueKind.QWord:
                    return BitConverter.GetBytes((regKey.GetValue(name) as long?) ?? 0);
                case RegistryValueKind.ExpandString:
                    return Encoding.UTF8.GetBytes(noResolv ?
                        ((regKey.GetValue(name, "", RegistryValueOptions.DoNotExpandEnvironmentNames) as string) ?? "") :
                        ((regKey.GetValue(name) as string) ?? ""));
                case RegistryValueKind.Binary:
                    return (regKey.GetValue(name) as byte[]) ?? new byte[0] { };
                case RegistryValueKind.MultiString:
                    return Encoding.UTF8.GetBytes((string.Join("\\0", regKey.GetValue(name) as string[])) ?? "");
                case RegistryValueKind.None:
                default:
                    return new byte[0] { };
            }
        }

        /// <summary>
        /// 対象のレジストリキー/値の有無チェック
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Exists(string path, string name = null)
        {
            using (RegistryKey regKey = GetRegistryKey(path, false, false))
            {
                if (regKey == null) { return false; }

                return name == null ?
                    true :
                    regKey.GetValueNames().Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
