using OpenMod.API.Ioc;
using System.Threading.Tasks;

namespace TebexDonations.API
{
    [Service]
    public interface ITebexAwarder
    {
        Task ForceCheckAsync();
    }
}
