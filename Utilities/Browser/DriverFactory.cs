using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SeleniumProject.Utilities
{
    public class DriverFactory
    {
        public static IWebDriver InitializeDriver(bool headless = false)
        {
            // Selenium Manager (tích hợp sẵn từ Selenium 4.6+) tự tìm ChromeDriver phù hợp
            // mà không cần gọi internet — tránh lỗi SSL khi không có kết nối mạng

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
