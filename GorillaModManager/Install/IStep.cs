using System.Threading.Tasks;
using GorillaModManager.Models.Mods;

namespace GorillaModManager.Install;

public interface IStep
{
    public Task<bool> Run(string directory, BrowserMod mod);
}
