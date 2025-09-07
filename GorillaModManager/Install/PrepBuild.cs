using System.Diagnostics;
using System.Threading.Tasks;
using GorillaModManager.Models.Mods;
using GorillaModManager.Services;
using Microsoft.CodeAnalysis.Scripting;

namespace GorillaModManager.Install;

public class PrepBuild : IStep
{
    public async Task<bool> Run(string directory, BrowserMod mod)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = directory,
        };

        string[] commands = [
            "new tool-manifest --force",
            "tool install Cake.Tool --version 5.0.0",
        ];

        foreach (var command in commands)
        {
            processStartInfo.Arguments = command;
            var process = Process.Start(processStartInfo);
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                await ItemInstaller.Notify("Failed Prep", "Failed to prepare project for building on command " + commands);
                return false;
            }
        }

        return true;
    }
}
