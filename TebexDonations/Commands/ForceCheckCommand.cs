using Cysharp.Threading.Tasks;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TebexDonations.API;

namespace TebexDonations.Commands
{
    [Command("tebex:forcecheck")]
    [CommandDescription("A command to force check the queue of packages")]
    public class ForceCheckCommand : UnturnedCommand
    {
        private readonly ITebexService _tebexService;

        public ForceCheckCommand(IServiceProvider serviceProvider, ITebexService service) : base(serviceProvider)
        {
            _tebexService = service;
        }

        protected async override UniTask OnExecuteAsync()
        {
            await Context.Actor.PrintMessageAsync("Checking for the queue to execute commands...");
            await _tebexService.Queue();
        }
    }
}
