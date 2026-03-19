using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;

namespace SeleniumProject.Utilities
{
    public class TestBase
    {
        protected IWebDriver Driver { get; private set; } = null!;
        protected WaitHelper Wait { get; private set; } = null!;

        // Đọc config từ appsettings.json, ưu tiên appsettings.local.json nếu có
        private static readonly IConfiguration Config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.local.json", optional: true) // ghi đè local nếu có
            .Build();

        protected static string BaseUrl
        {
            get
            {
                string url = Config["BaseUrl"];
                if (url == null)
                {
                    return "https://vuatraicay.site";
                }
                return url;
            }
        }

        protected static int Timeout
        {
            get
            {
                string timeoutStr = Config["Timeout"];
                int timeout;
                bool ok = int.TryParse(timeoutStr, out timeout);
                if (ok)
                {
                    return timeout;
                }
                return 15;
            }
        }

        // Đường dẫn lưu ảnh chụp màn hình khi test lỗi
        private static readonly string ScreenshotDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "Reports", "Screenshots"
        );

        [SetUp]
        public void SetUp()
        {
            Driver = DriverFactory.InitializeDriver();
            Wait = new WaitHelper(Driver, Timeout);
            // Không navigate ở đây — mỗi test class tự gọi Page.Open()
        }

        [TearDown]
        public void TearDown()
        {
            // Chụp màn hình nếu test vừa bị lỗi
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                ChupManHinhKhiLoi();
            }

            Driver?.Quit();
            Driver?.Dispose();
        }

        // Chụp và lưu ảnh màn hình vào thư mục Reports/Screenshots
        private void ChupManHinhKhiLoi()
        {
            try
            {
                Directory.CreateDirectory(ScreenshotDir);

                var tenTest = TestContext.CurrentContext.Test.Name;
                var thoiGian = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var tenFile = $"{tenTest}_{thoiGian}.png";
                var duongDan = Path.Combine(ScreenshotDir, tenFile);

                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                screenshot.SaveAsFile(duongDan);

                TestContext.AddTestAttachment(duongDan, "Screenshot khi lỗi");
                TestContext.WriteLine($"Screenshot đã lưu: {duongDan}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Không thể chụp màn hình: {ex.Message}");
            }
        }
    }
}
