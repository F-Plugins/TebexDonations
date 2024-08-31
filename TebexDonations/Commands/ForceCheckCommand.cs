using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using System;
using TebexDonations.API;

namespace TebexDonations.Commands
{
    [Command("forcecheck")]
    [CommandDescription("A command to force check the queue of packages")]
    public class ForceCheckCommand : UnturnedCommand
    {
        private readonly ITebexAwarder _tebexAwarder;

        public ForceCheckCommand(IServiceProvider serviceProvider, ITebexAwarder tebexAwarder) : base(serviceProvider)
        {
            _tebexAwarder = tebexAwarder;
        }

        protected async override UniTask OnExecuteAsync()
        {
            await Context.Actor.PrintMessageAsync("Checking for the queue to execute commands...");
            await _tebexAwarder.ForceCheckAsync();
        }
    }
}
