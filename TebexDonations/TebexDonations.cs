using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using System;
using TebexDonations.API;

[assembly: PluginMetadata("Feli.TebexDonations", DisplayName = "TebexDonations")]

namespace TebexDonations
{
    public class TebexDonations : OpenModUnturnedPlugin
    {
        private readonly ILogger<TebexDonations> _logger;
        private readonly ITebexService _tebexService;
        private readonly IConfiguration _configuration;

        public TebexDonations(
            ILogger<TebexDonations> logger,
            IConfiguration configuration,
            ITebexService tebexService,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _tebexService = tebexService;
        }

        protected override async UniTask OnLoadAsync()
        {
            _logger.LogInformation("Tebex Donations plugin made by Feli#8563");
            _logger.LogInformation("Get more plugins at: https://discord.gg/aDdDuXcDPn");
            _logger.LogInformation("Starting plugin..");

            if (_configuration.GetSection("TebexConfiguration:TebexSecret").Get<string>() == "")
            {
                _logger.LogWarning("Set up your Tebex Secret in the configuration file to make this plugin work");
            }

            await _tebexService.StartServiceAsync();
        }
    }
}
