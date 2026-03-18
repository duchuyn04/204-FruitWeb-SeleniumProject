using NUnit.Framework;
using OpenQA.Selenium;

namespace SeleniumProject.Utilities
{
    public class TestBase
    {
        protected IWebDriver Driver { get; private set; } = null!;
        protected WaitHelper Wait { get; private set; } = null!;

        protected const string BaseUrl = "https://vuatraicay.site";

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
            Driver?.Quit();
            Driver?.Dispose();
        }
    }
}
