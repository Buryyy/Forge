using System.Collections.Generic;

namespace Forge.DiscordBot.Models
{
    public class Command
    {
        public string Alias
        {
            get; set;

        }
        public List<Parameter> Parameters { get; set; }
        public string Summary { get; set; }
    }
}
