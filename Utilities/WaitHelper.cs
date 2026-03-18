using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SeleniumProject.Utilities
{
    public class WaitHelper
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public WaitHelper(IWebDriver driver, int timeoutSeconds = 15)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
        }

        // Chờ cho đến khi element xuất hiện và hiển thị trên trang
        public IWebElement WaitForVisible(By locator)
        {
            return _wait.Until(d =>
            {
                var el = d.FindElement(locator);
                return el.Displayed ? el : null;
            });
        }

        // Chờ cho đến khi element có thể click được
        public IWebElement WaitForClickable(By locator)
        {
            return _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator));
        }

        // Chờ cho đến khi text mong muốn xuất hiện bên trong element
        public bool WaitForText(By locator, string text)
        {
            return _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.TextToBePresentInElementLocated(locator, text));
        }

        // Chờ cho đến khi URL của trang chứa chuỗi ký tự chỉ định
        public bool WaitForUrlContains(string urlPart)
        {
            return _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.UrlContains(urlPart));
        }

        // Chờ cho đến khi element biến mất hoặc ẩn đi khỏi trang
        public bool WaitForInvisible(By locator)
        {
            return _wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(locator));
        }

        // Gõ chậm từng ký tự để có thể quan sát automation đang nhập gì
        // delayMs: thời gian chờ giữa mỗi ký tự (mặc định 80ms)
        public void SlowType(By locator, string text, int delayMs = 80)
        {
            var element = WaitForVisible(locator);
            element.Clear();
            foreach (char c in text)
            {
                element.SendKeys(c.ToString());
                Thread.Sleep(delayMs);
            }
        }
    }
}
