using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;

namespace SeleniumProject.Utilities
{
    public class TestBase
    {
        protected IWebDriver Driver { get; private set; } = null!;
        protected WaitHelper Wait { get; private set; } = null!;

        protected const string BaseUrl = "https://vuatraicay.site";

        // Đường dẫn lưu ảnh chụp màn hình khi test lỗi
        private static readonly string ScreenshotDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "Reports", "Screenshots"
        );

        [SetUp]
        public void SetUp()
        {
            Driver = DriverFactory.InitializeDriver();
            Wait = new WaitHelper(Driver);
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
                // Tạo thư mục nếu chưa có
                Directory.CreateDirectory(ScreenshotDir);

                // Tên file: TenTest_ngaygio.png
                var tenTest = TestContext.CurrentContext.Test.Name;
                var thoiGian = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var tenFile = $"{tenTest}_{thoiGian}.png";
                var duongDan = Path.Combine(ScreenshotDir, tenFile);

                // Chụp và lưu
                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                screenshot.SaveAsFile(duongDan);

                // Đính kèm vào NUnit report để xem trực tiếp
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
