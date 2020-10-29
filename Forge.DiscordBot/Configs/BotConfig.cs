using Discord;

namespace Forge.DiscordBot.Configs
{
    public class BotConfig
    {
        public LogSeverity LogLevel { get; set; }
        public bool AlwaysDownloadUsers { get; set; }
        public int MessageCacheSize { get; set; }
        public string Token { get; set; }
        public char PrefixChar { get; set; }
        public string CleverbotKey { get; set; }
    }
}
