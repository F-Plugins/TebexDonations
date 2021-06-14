using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Players.Chat.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TebexDonations.Events
{
    public class UnturnedPlayerChattingEventListener : IEventListener<UnturnedPlayerChattingEvent>
    {
        private readonly IConfiguration _configuration;

        public UnturnedPlayerChattingEventListener(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task HandleEventAsync(object sender, UnturnedPlayerChattingEvent @event)
        {
            var name = _configuration.GetSection("TebexConfiguration:StoreCommand").Get<string>();

            if (@event.Message.StartsWith(name))
            {
                await UniTask.SwitchToMainThread();
                @event.Player.Player.sendBrowserRequest(_configuration.GetSection("TebexConfiguration:StoreURL").Get<string>(), "Check out the donations page");
                await UniTask.SwitchToThreadPool();
                @event.IsCancelled = true;
            }
        }
    }
}
