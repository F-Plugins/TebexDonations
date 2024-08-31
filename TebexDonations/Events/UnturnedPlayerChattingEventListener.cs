using Microsoft.Extensions.Configuration;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Players.Chat.Events;
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

        public Task HandleEventAsync(object sender, UnturnedPlayerChattingEvent @event)
        {
            if (@event.Message.StartsWith(_configuration["StoreCommand"]))
            {
                @event.Player.Player.sendBrowserRequest("", _configuration["StoreURL"]);
                @event.IsCancelled = true;
            }

            return Task.CompletedTask;
        }
    }
}
