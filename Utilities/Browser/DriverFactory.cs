using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace SeleniumProject.Utilities
{
    public class DriverFactory
    {
        public static IWebDriver InitializeDriver(bool headless = false)
        {
            new DriverManager().SetUpDriver(new ChromeConfig(), WebDriverManager.Helpers.VersionResolveStrategy.MatchingBrowser);

            var options = new ChromeOptions();

            if (headless)
            {
                // Chạy ẩn (nhanh hơn, không hiện cửa sổ Chrome)
                options.AddArgument("--headless=new");
                options.AddArgument("--window-size=1920,1080");
            }
            else
            {
                // Chạy có cửa sổ (dùng khi debug, muốn nhìn thấy)
                options.AddArgument("--start-maximized");
            }

            options.AddArgument("--disable-notifications");

            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);

            return driver;
        }
    }
}
