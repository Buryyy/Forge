using System.Collections.Generic;

namespace Forge.DiscordBot.Models
{
   public class Module
    {
        public string Name { get; set; }
        public string Summary { get; set; }
        public List<Command> Commands = new List<Command>();
    }
}
