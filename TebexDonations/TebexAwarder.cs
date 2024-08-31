using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenMod.API.Commands;
using OpenMod.API.Ioc;
using OpenMod.API.Users;
using OpenMod.Core.Helpers;
using OpenMod.Core.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TebexDonations.API.Commands;
using TebexDonations.API;
using TebexDonations.Models;
using OpenMod.API.Prioritization;
using SDG.Unturned;

namespace TebexDonations
{
    [PluginServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.High)]
    internal class TebexAwarder : ITebexAwarder, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger<TebexAwarder> _logger;
        private readonly ICommandExecutor _commandExecutor;
        private readonly TebexCommandActor _tebexCommandActor;
        private readonly IUserManager _userManager;
        private bool _disposed;
        private double _queueCheckInterval = 0;

        public TebexAwarder(
            IConfiguration configuration,
            ILogger<TebexAwarder> logger,
            IUserManager userManager,
            IServiceProvider serviceProvider,
            ICommandExecutor commandExecutor)
        {
            _logger = logger;
            _configuration = configuration;
            _userManager = userManager;
            _commandExecutor = commandExecutor;
            _tebexCommandActor = ActivatorUtilities.CreateInstance<TebexCommandActor>(serviceProvider);
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(Worker, _cancellationTokenSource.Token);
        }

        public async Task ForceCheckAsync()
        {
            await PerformQueueCheckAsync();
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _cancellationTokenSource.Cancel();
        }

        private async Task Worker()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (!await PerformQueueCheckAsync())
                    {
                        return;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(_queueCheckInterval), _cancellationTokenSource.Token);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "There was an error while processing the queue check");
                }
            }
        }

        private async Task<bool> PerformQueueCheckAsync()
        {
            if (Level.isLoaded)
                return true;

            var secret = _configuration["Secret"];
            var endpoint = _configuration["Endpoint"];

            if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(endpoint))
            {
                _logger.LogError("TebexDonations secret or endpoint have not been set up correctly");
                return false;
            }

            using var client = new HttpClient();
            client.BaseAddress = new Uri(endpoint);
            client.DefaultRequestHeaders.Add("X-TebexDonations-Secret", secret);

            var queueResponse = await client.GetAsync("queue", _cancellationTokenSource.Token);

            if (!queueResponse.IsSuccessStatusCode)
            {
                _logger.LogError("HTTP Error: {0}, {1}.", (int)queueResponse.StatusCode, queueResponse.StatusCode);

                if (queueResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogError("Invalid Secret please set it correctly on the configuration");
                }

                return false;
            }

            var body = await queueResponse.Content.ReadAsStringAsync();

            var queue = JsonConvert.DeserializeObject<TebexQueue>(body);

            if (queue is null)
            {
                _logger.LogError("Failed to parse the response from the website");
                return false;
            }

            _queueCheckInterval = queue.Meta.NextCheck;

            if (queue.Meta.ExecuteOffline)
            {
                if (!await ExecuteOfflineCommandsAsync(client))
                {
                    return false;
                }
            }

            foreach (var player in queue.Players)
            {
                var user = await _userManager.FindUserAsync(KnownActorTypes.Player, player.UId, UserSearchMode.FindById);
                if (user?.Session is not null)
                {
                    if (!await ExecuteOnlineCommandsAsync(client, player.Id, user))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private async Task<bool> ExecuteOfflineCommandsAsync(HttpClient client)
        {
            var commandsResponse = await client.GetAsync("queue/offline-commands", _cancellationTokenSource.Token);

            if (!commandsResponse.IsSuccessStatusCode)
            {
                _logger.LogError("HTTP Error: {0}, {1}.", (int)commandsResponse.StatusCode, commandsResponse.StatusCode);

                if (commandsResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogError("Invalid Secret please set it correctly on the configuration");
                }

                return false;
            }

            var body = await commandsResponse.Content.ReadAsStringAsync();
            var offlineCommands = JsonConvert.DeserializeObject<TebexCommands>(body);

            if (offlineCommands is null)
            {
                _logger.LogError("Failed to parse the offline commands response from the website");
                return false;
            }

            var commands = offlineCommands.Commands;

            foreach (var command in commands)
            {
                await _commandExecutor.ExecuteAsync(_tebexCommandActor, GetCommandArgs(command.Player.UId, command.Player.Name, command.Command), string.Empty);
            }

            if (!await DeleteCommandsAsync(client, commands.Select(c => c.Id)))
            {
                return false;
            }

            return true;
        }

        private async Task<bool> ExecuteOnlineCommandsAsync(HttpClient client, string playerId, IUser user)
        {
            var response = await client.GetAsync($"queue/online-commands/{playerId}", _cancellationTokenSource.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("HTTP Error: {0}, {1}.", (int)response.StatusCode, response.StatusCode);

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogError("Invalid Secret please set it correctly on the configuration");
                }

                return false;
            }

            var body = await response.Content.ReadAsStringAsync();
            var offlineCommands = JsonConvert.DeserializeObject<TebexCommands>(body);

            if (offlineCommands is null)
            {
                _logger.LogError("Failed to parse the online commands response from the website");
                return false;
            }

            var commands = offlineCommands.Commands;

            foreach (var command in commands)
            {
                await _commandExecutor.ExecuteAsync(_tebexCommandActor, GetCommandArgs(user.Id, user.DisplayName, command.Command), string.Empty);
            }

            if (!await DeleteCommandsAsync(client, commands.Select(c => c.Id)))
            {
                return false;
            }

            return true;
        }

        private async Task<bool> DeleteCommandsAsync(HttpClient client, IEnumerable<int> commandIds)
        {
            string url = string.Concat("queue?", string.Join("&", commandIds.Select(id => string.Concat("ids[]=", id))));

            var response = await client.DeleteAsync(url, _cancellationTokenSource.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("HTTP Error: {0}, {1}.", (int)response.StatusCode, response.StatusCode);

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogError("Invalid Secret please set it correctly on the configuration");
                }

                return false;
            }

            return true;
        }

        private string[] GetCommandArgs(string id, string name, string command)
        {
            return ArgumentsParser.ParseArguments(command.Replace("{id}", id).Replace("{username}", name));
        }
    }
}
