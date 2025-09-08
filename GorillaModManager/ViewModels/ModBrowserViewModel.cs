using System.Collections.Generic;
using System.Collections.ObjectModel;
using GorillaModManager.Models.Persistence;
using GorillaModManager.Services;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GorillaModManager.Models.Mods;
using System;
using System.Globalization;
using System.Windows.Input;

namespace GorillaModManager.ViewModels
{
    public class ModBrowserViewModel : ViewModelBase
    {
        public static ModBrowserViewModel Instance;
        public const int ENTRIES_PER_PAGE = 6;

        private string _pageSearchTextBox;
        public string PageSearchTextBox
        {
            get => _pageSearchTextBox;
            set => this.RaiseAndSetIfChanged(ref _pageSearchTextBox, value);
        }

        public ObservableCollection<BrowserMod> ModsForPage { get; set; }

        public BrowserService Service;

        public bool ModsFetched
        {
            get => _modsFetched;
            set => this.RaiseAndSetIfChanged(ref _modsFetched, value);
        }

        bool _modsFetched;
        int _currentPage = 0;

        public ModBrowserViewModel()
        {
            Instance = this;

            ModsFetched = false;
            ModsForPage = new ObservableCollection<BrowserMod>(new BrowserMod[0]);
            Service = new BrowserService();

            SetModsForPage(_currentPage);
        }

        private async void SetModsForPage(int page)
        {
            ModsFetched = false;

            // TODO REDO
            BrowserMod[] tempArray = await Service.GetMods(page);
            ModsForPage.Clear();
            foreach (var info in tempArray)
                ModsForPage.Add(info);

            ModsFetched = true;
        }

        // void SetVisibleMods(IEnumerable<BrowserMod> mods)
        // {
        // ModsForPage = mods.Where(x => !x.Hidden).ToList();
        // ModsFetched = true;
        // }

        public void OnPageChanged(string buttonName)
        {
            if (string.IsNullOrEmpty(buttonName)) return;

            int totalPages = Service.VisisbleModCount / ENTRIES_PER_PAGE;
            int newPage = buttonName switch
            {
                "Prev" => _currentPage - 1,
                "Next" => _currentPage + 1,
                _ => _currentPage,
            };

            if (newPage > totalPages || newPage < 0)
            {
                Console.WriteLine("Max/min page reached");
                return;
            }
            _currentPage = newPage;
            SetModsForPage(_currentPage);
            PageSearchTextBox = (newPage + 1).ToString();
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

