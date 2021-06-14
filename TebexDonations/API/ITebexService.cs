using Cysharp.Threading.Tasks;
using OpenMod.API.Ioc;
using System.Collections.Generic;
using TebexDonations.Models;

namespace TebexDonations.API
{
    [Service]
    public interface ITebexService
    {
        UniTask StartServiceAsync();
        UniTask Queue();
        UniTask ExecuteOnlineCommands(TebexPlayer player, string name, string steamId);
        UniTask ExecuteOfflineCommands();
        UniTask DeleteCommands(List<int> commandIds);
    }
}
