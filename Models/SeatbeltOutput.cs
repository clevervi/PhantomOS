using System.Collections.Generic;

namespace PhantomOS.Models
{
    public class SeatbeltOutput
    {
        public Header Header { get; set; } = new Header();
        public List<CommandResult> Commands { get; set; } = new List<CommandResult>();
    }

    public class Header
    {
        public string Version { get; set; } = string.Empty;
        public string DateTime { get; set; } = string.Empty;
    }

    public class CommandResult
    {
        public string Command { get; set; } = string.Empty;
        public List<dynamic> Output { get; set; } = new List<dynamic>();
    }
}
