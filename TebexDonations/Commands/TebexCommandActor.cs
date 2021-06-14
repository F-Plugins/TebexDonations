using Microsoft.Extensions.Logging;
using OpenMod.API.Commands;
using OpenMod.Core.Users;
using System.Drawing;
using System.Threading.Tasks;

namespace TebexDonations.Commands
{
    public class TebexCommandActor : ICommandActor
    {
        private readonly ILogger _logger;

        public TebexCommandActor(ILogger logger)
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
