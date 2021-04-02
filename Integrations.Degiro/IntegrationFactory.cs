using System.Linq;
using System.Threading.Tasks;
using Integrations.Degiro.Models.Configuration;
using PlaywrightSharp;

namespace Integrations.Degiro
{
    public class IntegrationFactory
    {
        private readonly DegiroConfiguration _configuration;

        public IntegrationFactory(DegiroConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IIntegration> Create()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync();
            var page = await browser.NewPageAsync();

            await page.GoToAsync(_configuration.Login.LoginUrl);

            await page.WaitForLoadStateAsync(LifecycleEvent.DOMContentLoaded);

            await page.FillAsync($"xpath={_configuration.Login.XPaths.UsernameTextBox}", _configuration.Credentials.Username);
            await page.FillAsync($"xpath={_configuration.Login.XPaths.PasswordTextBox}", _configuration.Credentials.Password);
            await page.ClickAsync($"xpath={_configuration.Login.XPaths.LoginButton}");

            //It takes some time to get the right cookie back, so let's wait for page to fully redirect
            await page.WaitForLoadStateAsync(LifecycleEvent.Networkidle);

            var cookies = await page.Context.GetCookiesAsync(page.Url);
            var jSessionId = cookies.Single(_ => _.Name == _configuration.Login.SessionCookieName).Value;

            return new Integration(_configuration.Requests, jSessionId);
        }
    }
}
