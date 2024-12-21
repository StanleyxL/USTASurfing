using PuppeteerSharp;
using System;
using System.Threading.Tasks;
namespace ISS.Common
{
    public class PuppeteerManager
    {
        public Browser browser;

        public async Task InitializeBrowserAsync()
        {
            if (browser == null)
            {
                await new BrowserFetcher().DownloadAsync();
                browser = (Browser?)await Puppeteer.LaunchAsync(new LaunchOptions { Headless = false });

            }
        }

        public  async Task CloseBrowserAsync()
        {
            if (browser != null)
            {
                await browser.CloseAsync();
                browser = null;
            }
        }

        public async Task<string> GetContentByPuppeteerAsync(string url)
        {
            if (browser == null)
            {
                await InitializeBrowserAsync();
                //throw new InvalidOperationException("Browser is not initialized. Call InitializeBrowserAsync first.");
            }

            var page = await browser.NewPageAsync();
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
