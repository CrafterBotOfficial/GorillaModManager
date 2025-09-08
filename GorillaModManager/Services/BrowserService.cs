using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Threading;
using GorillaModManager.Models.Mods;
using GorillaModManager.ViewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MsBox.Avalonia;
using ReactiveUI;

namespace GorillaModManager.Services
{
    public class BrowserService
    {
#if DEBUG
        public static string REMOTE_URL = "http://localhost:8000/";
#else
        public static string REMOTE_URL = "https://raw.githubusercontent.com/CrafterBotOfficial/GorillaTagModManager_Mods/refs/heads/master/"; //"http://localhost:8000/";
#endif

        public int VisisbleModCount
        {
            get
            {
                if (cachedMods == null)
                {
                    Console.WriteLine("Getter can not be called yet.");
                    return 0;
                }
                return cachedMods.Where(x => !x.Hidden).Count();
            }
        }

        private BrowserMod[] cachedMods;

        public async Task<BrowserMod[]> GetAllMods()
        {
            if (cachedMods == null)
            {
                string manifest = await FetchManifest();
                if (manifest == null) return null;

                cachedMods = Newtonsoft.Json.JsonConvert.DeserializeObject<BrowserMod[]>(manifest);
                if (cachedMods == null)
                {
                    await Notify("Error", "Got bad manifest from server, please report on Github.");
                    return null;
                }
            }
            return cachedMods;
        }

        /// <summary>Only returns none-hidden mods </summary>
        public async Task<BrowserMod[]> GetMods(int page)
        {
            Console.WriteLine("Displaying page: " + page);
            var filteredMods = (await GetAllMods()).Where(x => !x.Hidden);
            return ChunkArray(filteredMods.ToArray(), page);
        }

        private async Task<string> FetchManifest()
        {
            using var httpClient = new HttpClient();
            var requestTask = httpClient.GetAsync(REMOTE_URL + "/manifest.json");

            var resultTask = requestTask.ContinueWith(async task =>
            {
                if (task.IsCanceled)
                {
                    await Notify("Connection Error", "The request was canceled (likely a timeout).");
                    return null;
                }

                if (task.IsFaulted)
                {
                    string exception = task.Exception?.Message;
                    await Notify("Connection Error", "An exception occured while trying to connect\n" + exception);
                    return null;
                }

                var result = task.Result;
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    await Notify("Connection Error", "Failed to get manifest from remote server, check your internet connection.");
                    // return await FetchManifest();
                    return null;
                }

                string resultString = await result.Content.ReadAsStringAsync();
                if (resultString == null || resultString.Length == 0)
                {
                    await Notify("Fatal", "Failed to read data from remote server.");
                    return null;
                }
                return resultString;
            })
            .Unwrap();

            return await resultTask;
        }

        private async Task Notify(string title, string message)
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                await MessageBoxManager.GetMessageBoxStandard(title, message, MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
            });
        }

        private BrowserMod[] ChunkArray(BrowserMod[] original, int groupIndex)
        {
            var result = original.Chunk(ModBrowserViewModel.ENTRIES_PER_PAGE).ToArray()[groupIndex];
            return result;
        }
    }
}
