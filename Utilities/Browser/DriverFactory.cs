using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace SeleniumProject.Utilities
{
    public class DriverFactory
    {
        public static IWebDriver InitializeDriver()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());

            var options = new ChromeOptions();
            // options.AddArgument("--headless");   // Bỏ comment dòng này nếu muốn chạy ẩn browser (không hiện cửa sổ)
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");

            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);

            return driver;
        }
    }
}
