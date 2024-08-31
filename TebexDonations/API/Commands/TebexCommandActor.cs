using Microsoft.Extensions.Logging;
using OpenMod.API.Commands;
using OpenMod.Core.Users;
using System.Drawing;
using System.Threading.Tasks;

namespace TebexDonations.API.Commands
{
    public class TebexCommandActor : ICommandActor
    {
        private readonly ILogger<TebexCommandActor> _logger;

        public TebexCommandActor(ILogger<TebexCommandActor> logger)
        {
            _logger = logger;
        }

        public string Id => "TebexCommandActor";

        public string Type => KnownActorTypes.Console;

        public string DisplayName => "TebexCommandActor";

        public string FullActorName => "TebexCommandActor";

        public Task PrintMessageAsync(string message)
        {
            _logger.LogInformation(message);
            return Task.CompletedTask;
        }

        public Task PrintMessageAsync(string message, Color color)
        {
            return PrintMessageAsync(message);
        }
    }
}
