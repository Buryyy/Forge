using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Forge.DiscordBot.Configs;
using Forge.DiscordBot.Interfaces;
using Forge.DiscordBot.Models;
using Forge.DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CommandService = Forge.DiscordBot.Services.CommandService;

namespace Forge.DiscordBot
{
    public class App
    {

        private static App _instance;
        public static App Current => _instance ??= new App();

        
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
            .ConfigureHostConfiguration(config =>
            {
                config.AddEnvironmentVariables("BOT_")
                .AddCommandLine(args, new Dictionary<string, string>
                {
                    ["--environment"] = "Environment",
                    ["-e"] = "Environment"
                });
            })
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", false)
                .AddJsonFile($"config.{context.HostingEnvironment.EnvironmentName}.json", true);

                if (context.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets("TrishaBot");
                }
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.AddConsole();
                if (context.HostingEnvironment.IsDevelopment())
                {
                    logging.AddDebug();
                }
            })
            .ConfigureServices((context, services) =>
            {
                RegisterConfigInstances(services);

                var config = new BotConfig();
                context.Configuration.Bind(config);
                services.AddSingleton(config);

                RegisterDiscordClient(services, config);

                services.AddSingleton<IBehaviorService, BehaviorService>();
                services.AddSingleton(provider => BuildCommandHandler(provider, config));

                services.AddHostedService<BotClient>();
            })
            .UseConsoleLifetime();

            await host.RunConsoleAsync();
        }

        private static void RegisterDiscordClient(IServiceCollection services, BotConfig config)
        {
            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = config.LogLevel,
                AlwaysDownloadUsers = config.AlwaysDownloadUsers,
                MessageCacheSize = config.MessageCacheSize,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });
         
            services.AddSingleton<IDiscordClient>(client)
                .AddSingleton(client);
        }

        private static ICommandService BuildCommandHandler(IServiceProvider provider, BotConfig config)
        {
            var logger = provider.GetService<ILogger<ICommandService>>();
            var commandService = new Discord.Commands.CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Sync,
                SeparatorChar = ' ',
                LogLevel = config.LogLevel
            });

            var discordLogger = new DiscordLogger<ICommandService>(logger);
            commandService.Log += discordLogger.OnLogAsync;

            var handler = ActivatorUtilities.CreateInstance(provider, typeof(CommandService), commandService) as ICommandService;
            return handler;
        }

        private static void RegisterConfigInstances(IServiceCollection services)
        {
            var assembly = Assembly.GetEntryAssembly();
            var interfaceType = typeof(CommandConfig);

            var exportedTypes = assembly.ExportedTypes;

            foreach (var type in exportedTypes)
            {
                try
                {
                    var typeInfo = type.GetTypeInfo();

                    if (!interfaceType.IsAssignableFrom(type) || typeInfo.IsInterface || typeInfo.IsAbstract)
                    {
                        continue;
                    }

                    services.AddTransient(type, (sp) =>
                    {
                        var instance = ActivatorUtilities.CreateInstance(sp, type) as CommandConfig;
                        instance.Reload();
                        return instance;
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }
    }
}
