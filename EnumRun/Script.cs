﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using EnumRun.Lib;

namespace EnumRun
{
    internal class Script
    {
        public string FilePath { get; set; }
        public int FileNumber { get; set; }
        public Language Language { get; set; }
        public bool Enabled { get; set; }
        public EnumRunOption Option { get; set; }

        private static readonly Regex pattern_fileNum = new Regex(@"^\d+(?=_)");

        public Script() { }
        public Script(string filePath, EnumRunSetting setting, LanguageCollection collection)
        {
            this.FilePath = filePath;

            Match match;
            this.FileNumber = (match = pattern_fileNum.Match(filePath)).Success ?
                int.Parse(match.Value) : -1;

            if (setting.Ranges.Within(this.FileNumber))
            {
                this.Enabled = true;
                this.Language = collection.GetLanguage(this.FilePath);
                this.Option = new EnumRunOption(this.FilePath);
            }
        }




    }
}
