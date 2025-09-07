using Avalonia;
using Avalonia.Threading;
using GorillaModManager.Install;
using GorillaModManager.Models.Mods;
using GorillaModManager.Models.Persistence;
using GorillaModManager.Utils;
using MsBox.Avalonia;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace GorillaModManager.Services
{
    public static class ItemInstaller
    {
        public static async Task InstallFromGameBanana(BrowserMod modToInstall)
        {
            if (string.IsNullOrEmpty(ManagerSettings.Default.GamePath))
            {
                await Notify("Game not found", "Please enter Gorilla Tags path into the settings.");
                return;
            }

            if (!modToInstall.Url.EndsWith(".git")) {
                Console.WriteLine("Installing from URL");
                await InstallFromUrl(modToInstall.Url, "BepInEx/plugins");
                return;
            }

            string buildDirectory = Path.Combine(await GetBuildDirectory(), modToInstall.CommitHash);
            if (buildDirectory == null) return;

            var stateMachine = new IStep[]
            {
                new DownloadStep(),
                new PrepBuild(),
                new BuildStep(),
            };

            foreach (var step in stateMachine)
            {
                bool success = await step.Run(buildDirectory, modToInstall);
                if (!success)
                {
                    await Notify("Failed step", "Failed install step");
                    break;
                }
            }
        }

        private static async Task<string> GetBuildDirectory()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "gorilla_build_dir");
            if (Directory.Exists(path)) return path;

            try { Directory.CreateDirectory(path); }
            catch (Exception ex)
            {
                await Notify("Error", "Couldn't create build folder at " + path + " because:\n" + ex.Message);
                return null;
            }
            return path;
        }

        public static async Task Notify(string title, string message)
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                await MessageBoxManager.GetMessageBoxStandard(title, message, MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
            });
        }

        public static async Task InstallFromUrl(string url, string localPath)
        {
            using var client = HttpUtils.MakeGMClient();

            string fullPath = Path.Combine(ManagerSettings.Default.GamePath, localPath);
            byte[] data = await client.GetByteArrayAsync(url);
            ZipFile.ExtractToDirectory(new MemoryStream(data), fullPath, true);
        }
    }
}
