using System.Collections.Generic;
using Forge.DiscordBot.Interfaces;
using Newtonsoft.Json;

namespace Forge.DiscordBot.Configs
{
    public class PermissionsConfig : CommandConfig
    {
        [JsonIgnore]
        public override string ConfigKey => nameof(PermissionsConfig);

        //command, [role ids]
        public Dictionary<string, List<ulong>> Permissions { get; set; } = new Dictionary<string, List<ulong>>();
    }
}
