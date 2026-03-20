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

        // Chụp và lưu ảnh màn hình vào thư mục Reports/Screenshots/{Module}/
        // Module được tự động lấy từ namespace của test class (phần ngay trước tên class)
        // VD: "SeleniumProject.Tests.Auth.LoginTests" -> module = "Auth"
        private void ChupManHinhKhiLoi()
        {
            try
            {
                // Bước 1: Lấy tên đầy đủ của class đang chạy test
                // VD: "SeleniumProject.Tests.Auth.LoginTests"
                string fullClassName = TestContext.CurrentContext.Test.ClassName;
                if (fullClassName == null)
                {
                    fullClassName = "";
                }

                // Bước 2: Tách thành mảng theo dấu chấm
                // VD: ["SeleniumProject", "Tests", "Auth", "LoginTests"]
                string[] parts = fullClassName.Split('.');

                // Bước 3: Lấy phần tử áp cuối làm tên module
                // Mảng 4 phần tử: index 0,1,2,3 → áp cuối là index 2 = "Auth"
                string module;
                if (parts.Length >= 2)
                {
                    module = parts[parts.Length - 2]; // lấy phần tử áp cuối
                }
                else
                {
                    module = "Unknown";
                }

                // Bước 4: Tạo thư mục con theo module (tự tạo nếu chưa có)
                // VD: Reports/Screenshots/Auth/
                string moduleDir = Path.Combine(ScreenshotDir, module);
                Directory.CreateDirectory(moduleDir);

                // Bước 5: Tạo tên file theo tên test + thời gian
                string tenTest = TestContext.CurrentContext.Test.MethodName;
                if (tenTest == null)
                {
                    tenTest = "UnknownTest";
                }

                string thoiGian = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string tenFile = tenTest + "_" + thoiGian + ".png";
                string duongDan = Path.Combine(moduleDir, tenFile);

                // Bước 6: Chụp và lưu ảnh
                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                screenshot.SaveAsFile(duongDan);

                TestContext.AddTestAttachment(duongDan, "Screenshot khi lỗi");
                TestContext.WriteLine("Screenshot đã lưu: " + duongDan);
            }
            catch (Exception ex)
            {
                TestContext.WriteLine("Không thể chụp màn hình: " + ex.Message);
            }
        }
    }
}
