using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Forge.DiscordBot.Configs;
using Forge.DiscordBot.Interfaces;
using Forge.DiscordBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Forge.DiscordBot
{
    public class BotClient : IDiscordBot, IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly ICommandService _commandHandler;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly Dictionary<string, IBotBehavior> _behaviors;
        private readonly IBehaviorService _behaviorService;
        private readonly ILogger<BotClient> _logger;
        private readonly BotConfig _configuration;

        public BotClient(BotConfig config, DiscordSocketClient client, IBehaviorService behaviorService, ICommandService commandHandler, ILogger<BotClient> logger, IConfiguration configroot, IHostApplicationLifetime appLifetime)
        {
            _configuration = config;
            _behaviorService = behaviorService;
            _logger = logger;
            _behaviors = new Dictionary<string, IBotBehavior>();
            _client = client;
            _commandHandler = commandHandler;
            _appLifetime = appLifetime;
        }

        
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.SetStatusAsync(UserStatus.Offline);
            await _client.LogoutAsync();
            await _client.StopAsync();
        }

        public async Task RunBotAsync()
        {
            var logger = new DiscordLogger<BotClient>(_logger);
            _client.Ready += OnReadyAsync;
            _client.Log += logger.OnLogAsync;
            _client.LoggedOut += OnLoggedOutAsync;
            _client.Disconnected += OnDisconnected;

            await _behaviorService.InstallAsync();

            try
            {
                await ConnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        private Task OnDisconnected(Exception arg)
        {
            _logger.LogError(arg.Message);
            _appLifetime.StopApplication();
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken) =>
            RunBotAsync();

        private async Task OnLoggedOutAsync() =>
            await _behaviorService.StopAsync();

        private async Task OnReadyAsync()
        {
            await _behaviorService.RunAsync();
            await _commandHandler.InstallAsync();
        }

        private async Task ConnectAsync()
        {
            const int maxAttempts = 10;
            var currentAttempt = 0;
            do
            {
                currentAttempt++;
                try
                {
                    await _client.LoginAsync(TokenType.Bot, _configuration.Token);
                    await _client.StartAsync();
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to connect: {ex.Message}");
                    await Task.Delay(currentAttempt * 1000);
                }
            }
            while (currentAttempt < maxAttempts);
        }
    }
}
