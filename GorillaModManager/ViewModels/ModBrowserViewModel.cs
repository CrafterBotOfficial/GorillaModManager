using GorillaModManager.Models.Persistence;
using GorillaModManager.Services;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GorillaModManager.Models.Mods;

namespace GorillaModManager.ViewModels
{
    public class ModBrowserViewModel : ViewModelBase
    {
        public static ModBrowserViewModel Instance;

        public List<BrowserMod> ModsForPage
        {
            get
            {
                return _modsForPage;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _modsForPage, value);
            }
        }

        List<BrowserMod> _modsForPage;

        public BrowserService Service;

        public bool ModsFetched
        {
            get
            {
                return _modsFetched;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _modsFetched, value);
            }
        }

        bool _modsFetched;

        int _currentPage = 0;

        public ModBrowserViewModel()
        {
            Instance = this;

            ModsFetched = false;
            Service = new BrowserService();

            SetModsForPage(_currentPage);
        }

        private async void SetModsForPage(int page)
        {
            ModsFetched = false;
            var mods = await Service.GetMods(page);
            SetVisibleMods(mods);
        }

        void SetVisibleMods(IEnumerable<BrowserMod> mods)
        {
            ModsForPage = mods.Where(x => !x.Hidden).ToList();
            ModsFetched = true;
        }

        public async void OnInstallClick(string modUrl)
        {
            Debug.WriteLine($"{modUrl}");

            BrowserMod browserMod = FindModForUrl(modUrl);
            InstallerMod mod = new InstallerMod(browserMod.Url, browserMod.ModName);

            if (mod == null)
            {
                SetModsForPage(_currentPage);
                return;
            }

            if (!Directory.Exists(Path.Combine(ManagerSettings.Default.GamePath, "BepInEx", "plugins")))
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Browser Failure.", "You have not setup bepinex properly or your game path is set incorrectly.",
                        ButtonEnum.Ok);

                await box.ShowAsync();
                return;
            }

            //await InstallationHandler.InstallFileFromUrl(mod, "BepInEx/plugins", true);
        }

        private BrowserMod FindModForUrl(string modUrl)
        {
            for (int i = 0; i < ModsForPage.Count; i++)
            {
                if (ModsForPage[i].Url == modUrl)
                {
                    return ModsForPage[i];
                }
            }

            return null;
        }
    }
}

