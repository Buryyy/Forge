using System.Collections.Generic;

namespace Forge.DiscordBot.Models
{
    public class Help
    {
        public List<Command> Commands { get; set; } = new List<Command>();
    }
}
