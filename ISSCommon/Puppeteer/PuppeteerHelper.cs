using PuppeteerSharp;
using System;
using System.Threading.Tasks;
namespace ISS.Common
{
    public static class PuppeteerHelper
    {
        private static Browser _browser;

        public static async Task InitializeBrowserAsync()
        {
            if (_browser == null)
            {
                await new BrowserFetcher().DownloadAsync();
                _browser = (Browser?)await Puppeteer.LaunchAsync(new LaunchOptions { Headless = false });

            }
        }

        public static async Task CloseBrowserAsync()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser = null;
            }
        }

        public static async Task<string> GetContentByPuppeteerAsync(string url)
        {
            if (_browser == null)
            {
                await InitializeBrowserAsync();
                //throw new InvalidOperationException("Browser is not initialized. Call InitializeBrowserAsync first.");
            }

            var page = await _browser.NewPageAsync();
            page.DefaultNavigationTimeout = 60000; // 60 seconds

            try
            {
                await page.GoToAsync(url, new NavigationOptions
                {
                    Timeout = 60000, // 30 seconds
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                });

                var content = await page.GetContentAsync();
                return content;
            }
            catch (TimeoutException ex)
            {
                // Logger.Log($"Navigation to {url} timed out: {ex.Message}");
                return string.Empty;
            }
            finally
            {
                await page.CloseAsync();
            }
        }
    }
}
