using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenMod.API.Commands;
using OpenMod.API.Ioc;
using OpenMod.API.Users;
using OpenMod.Core.Helpers;
using OpenMod.Core.Users;
using System;
using System.Collections.Generic;
using System.Net.Http;
using TebexDonations.API;
using TebexDonations.Commands;
using TebexDonations.Models;

namespace TebexDonations.Services
{
    [PluginServiceImplementation(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class TebexService : ITebexService, IDisposable
    {
        private bool _active;
        private int _nextCheck;

        private ILogger<TebexService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICommandExecutor _commandExecutor;
        private readonly TebexCommandActor _tebexCommandActor;
        private readonly IUserManager _userManager;

        public TebexService(IConfiguration configuration, ILogger<TebexService> logger, ICommandExecutor commandExecutor, IUserManager userManager)
        {
            _configuration = configuration;
            _logger = logger;
            _commandExecutor = commandExecutor;
            _tebexCommandActor = new TebexCommandActor(logger);
            _userManager = userManager;
        }

        public UniTask StartServiceAsync()
        {
            _active = true;
            _nextCheck = _configuration.GetSection("TebexConfiguration:TebexCheckInterval").Get<int>();
            AsyncHelper.Schedule("TebexService:Schedule", () => CheckerSchedule().AsTask());
            return UniTask.CompletedTask;
        }

        private async UniTask CheckerSchedule()
        {
            while (_active)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_nextCheck));

                await Queue();
            }
        }

        public async UniTask Queue()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(_configuration.GetSection("TebexConfiguration:TebexUrl").Get<string>());
            client.DefaultRequestHeaders.Add("X-Tebex-Secret", _configuration.GetSection("TebexConfiguration:TebexSecret").Get<string>());

            HttpResponseMessage response = null;

            try
            {
                _logger.LogDebug("GET - Queue - Start");
                response = await client.GetAsync("queue");
                _logger.LogDebug("GET - Queue - Finish");
            }
            catch (Exception ex)
            {
                _logger.LogError("HTTP Error: {ex}.", new
                {
                    ex = ex.ToString()
                });
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"HTTP Error: {(int)response.StatusCode}, {response.StatusCode.ToString()}.");

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogInformation("Invalid Secret please set it correctly on the configuration");
                }
            }
            else
            {
                var jsonText = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Information from the request: {text}", new { text = jsonText });
                var json = JsonConvert.DeserializeObject<Queue>(jsonText);

                _nextCheck = json.meta.next_check;

                if (json.meta.execute_offline)
                {
                    await ExecuteOfflineCommands();
                }


                foreach (var player in json.players)
                {
                    var user = await _userManager.FindUserAsync(KnownActorTypes.Player, player.uuid, UserSearchMode.FindById);
                    if (user.Session != null)
                    {
                        await ExecuteOnlineCommands(player, user.DisplayName, user.Id);
                    }
                }
            }
        }

        public async UniTask ExecuteOnlineCommands(TebexPlayer player, string name, string steamId)
        {
            _logger.LogInformation("Executing online commands for player: {name}", new
            {
                name = player.name
            });

            var client = new HttpClient();
            client.BaseAddress = new Uri(_configuration.GetSection("TebexConfiguration:TebexUrl").Get<string>());
            client.DefaultRequestHeaders.Add("X-Tebex-Secret", _configuration.GetSection("TebexConfiguration:TebexSecret").Get<string>());

            HttpResponseMessage response = null;

            try
            {
                _logger.LogDebug("GET - Online - Start");
                response = await client.GetAsync("queue/online-commands/" + player.id);
                _logger.LogDebug("GET - Online - Finish");
            }
            catch (Exception ex)
            {
                _logger.LogError("HTTP Error: {ex}.", new
                {
                    ex = ex.ToString()
                });
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"HTTP Error: {(int)response.StatusCode}, {response.StatusCode.ToString()}.");

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogInformation("Invalid Secret please set it correctly on the configuration");
                }
            }
            else
            {
                var jsonText = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Information from the request: {text}", new { text = jsonText });
                var json = JsonConvert.DeserializeObject<Online>(jsonText);
                var commandIds = new List<int>();
                foreach (var command in json.commands)
                {
                    await _commandExecutor.ExecuteAsync(_tebexCommandActor, GetCommand(steamId, name, command.command), String.Empty);
                    commandIds.Add(command.id);
                }
                await DeleteCommands(commandIds);
            }
        }

        public async UniTask ExecuteOfflineCommands()
        {
            _logger.LogInformation("Executing offline commands...");
            var client = new HttpClient();
            client.BaseAddress = new Uri(_configuration.GetSection("TebexConfiguration:TebexUrl").Get<string>());
            client.DefaultRequestHeaders.Add("X-Tebex-Secret", _configuration.GetSection("TebexConfiguration:TebexSecret").Get<string>());

            HttpResponseMessage response = null;

            try
            {
                _logger.LogDebug("GET - Offline - Start");
                response = await client.GetAsync("queue/offline-commands");
                _logger.LogDebug("GET - Offline - Finish");
            }
            catch (Exception ex)
            {
                _logger.LogError("HTTP Error: {ex}.", new
                {
                    ex = ex.ToString()
                });
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"HTTP Error: {(int)response.StatusCode}, {response.StatusCode.ToString()}.");

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogInformation("Invalid Secret please set it correctly on the configuration");
                }
            }
            else
            {
                var commandIds = new List<int>();
                var jsonText = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Information from the request: {text}", new { text = jsonText });

                var json = JsonConvert.DeserializeObject<Offline>(jsonText);

                foreach(var command in json.commands)
                {
                    await _commandExecutor.ExecuteAsync(_tebexCommandActor, GetCommand(command.player, command.command), String.Empty);
                    commandIds.Add(command.id);
                }

                await DeleteCommands(commandIds);
            }
        }

        public async UniTask DeleteCommands(List<int> commandIds)
        {
            _logger.LogInformation("Deleting commands...");
            var client = new HttpClient();
            client.BaseAddress = new Uri(_configuration.GetSection("TebexConfiguration:TebexUrl").Get<string>());
            client.DefaultRequestHeaders.Add("X-Tebex-Secret", _configuration.GetSection("TebexConfiguration:TebexSecret").Get<string>());

            HttpResponseMessage response = null;

            string url = "queue?";
            string add = "";

            foreach(var id in commandIds)
            {
                url += add + "ids[]=" + id;
                add = "&";
            }

            try
            {
                _logger.LogDebug("DELETE - Delete - Start");
                response = await client.DeleteAsync(url);
                _logger.LogDebug("DELETE - Delete - Finish");
            }
            catch (Exception ex)
            {
                _logger.LogError("HTTP Error: {ex}.", new
                {
                    ex = ex.ToString()
                });
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"HTTP Error: {(int)response.StatusCode}, {response.StatusCode.ToString()}.");

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogInformation("Invalid Secret please set it correctly on the configuration");
                }
            }
        }

        private string[] GetCommand(TebexPlayer player, string command)
        {
            return ArgumentsParser.ParseArguments(command.Replace("{id}", player.id.ToString()).Replace("{username}", player.name));
        }

        private string[] GetCommand(string id, string name, string command)
        {
            return ArgumentsParser.ParseArguments(command.Replace("{id}", id).Replace("{username}", name));
        }

        public void Dispose()
        {
            _active = false;
        }
    }
}
