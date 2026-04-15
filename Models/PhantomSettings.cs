using System;
using System.Collections.Generic;

namespace PhantomOS.Models
{
    public class PhantomSettings
    {
        public string LastProfile { get; set; } = "Safe";
        public List<string> AppliedTweakIds { get; set; } = new List<string>();
        public bool AutoApplyOnStartup { get; set; } = false;
        public DateTime LastScanDate { get; set; } = DateTime.MinValue;
        public string PreferredTheme { get; set; } = "Dark";
        public bool IsFirstRun { get; set; } = true;
    }
}
