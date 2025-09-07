using GorillaModManager.Services;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System;
using GorillaModManager.Views;
using GorillaModManager.ViewModels;
using System.Threading.Tasks;

namespace GorillaModManager.Models.Mods
{
    public class BrowserMod
    {
        public required string ModName { get; set; }
        public required string ModShortDescription { get; set; }
        public required string ModAuthor { get; set; }
        public required string Url { get; set; }
        public required string CommitHash { get; set; }
        public required string ThumbnailImageUrl { get; set; }
        public string Dependencies;
        public bool Hidden;

        public async Task InstallMod()
        {
            await InstallMod(false);
        }

        public async Task InstallMod(bool asDependency)
        {
            Debug.WriteLine($"Installing {ModName}");

            if (!string.IsNullOrEmpty(Dependencies))
                foreach (var dependency in Dependencies.Split(','))
                {
                    ModManagerViewModel.Instance.RefreshModList(string.Empty);
                    if (ModManagerViewModel.Instance.InstalledMods.Any(x => x.ModName == dependency))
                    {
                        Console.WriteLine(dependency + " already installed");
                        continue;
                    }

                    Console.WriteLine("Installing as dependency " + dependency);
                    var modInfo = (await ModBrowserViewModel.Instance.Service.GetAllMods()).FirstOrDefault(x => x.ModName == dependency);
                    if (modInfo == null)
                    {
                        await ItemInstaller.Notify("Error", "Failed to install mod dependency, manual intervention is required. " + dependency);
                        continue;
                    }
                    await modInfo.InstallMod(true);
                }

            await ItemInstaller.InstallFromGameBanana(this);
            if (!asDependency)
                await ItemInstaller.Notify("Success", "Successfully installed " + ModName);
        }
    }
}
