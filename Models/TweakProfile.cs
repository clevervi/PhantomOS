using System.Collections.Generic;

namespace PhantomOS.Models
{
    public class TweakProfile
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "🚀";
        public List<string> TweakIds { get; set; } = new List<string>();
    }
}
