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
            // d là IWebDriver được truyền vào tự động bởi WebDriverWait
            // Hàm này chạy lặp lại cho đến khi element hiển thị hoặc hết timeout
            return _wait.Until(d =>
            {
                IWebElement el = d.FindElement(locator);
                if (el.Displayed)
                {
                    return el;
                }
                return null;
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

        // Chọn một option trong dropdown <select> theo text hiển thị
        // Ví dụ: SelectDropdown(CategoryDropdown, "Trái cây nội địa")
        public void SelectDropdown(By locator, string optionText)
        {
            // Chờ dropdown hiển thị
            IWebElement dropdownElement = WaitForVisible(locator);

            // SelectElement là class của Selenium dùng để thao tác với <select>
            OpenQA.Selenium.Support.UI.SelectElement select = new OpenQA.Selenium.Support.UI.SelectElement(dropdownElement);

            // Chọn option theo text hiển thị trên màn hình
            select.SelectByText(optionText);
        }

        // Tick hoặc bỏ tick checkbox theo trạng thái mong muốn
        // Ví dụ: SetCheckbox(ActiveCheckbox, true) → đảm bảo checkbox được tích
        public void SetCheckbox(By locator, bool shouldBeChecked)
        {
            IWebElement checkbox = WaitForVisible(locator);

            // Kiểm tra trạng thái hiện tại của checkbox
            bool isCurrentlyChecked = checkbox.Selected;

            // Chỉ click nếu trạng thái hiện tại khác với trạng thái mong muốn
            if (isCurrentlyChecked != shouldBeChecked)
            {
                checkbox.Click();
            }
        }

        // Cuộn trang để element xuất hiện trong vùng nhìn thấy của màn hình
        // Dùng khi form dài và element bị cuộn ra ngoài màn hình
        public void ScrollIntoView(By locator)
        {
            IWebElement element = WaitForVisible(locator);

            // Dùng JavaScript để cuộn element vào giữa màn hình
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", element);

            // Chờ 300ms để trang ổn định sau khi cuộn
            Thread.Sleep(300);
        }

        // Upload file bằng cách gán đường dẫn vào input[type=file]
        // Không cần click hay tương tác với dialog của hệ điều hành
        public void UploadFile(By locator, string absoluteFilePath)
        {
            // Với input[type=file], Selenium cho phép SendKeys đường dẫn trực tiếp
            IWebElement fileInput = _driver.FindElement(locator);
            fileInput.SendKeys(absoluteFilePath);
        }

        // Chờ toast (thông báo ngắn) xuất hiện và trả về nội dung text của nó
        // Dùng sau khi submit form để kiểm tra kết quả thành công hay thất bại
        public string WaitForToast(By toastLocator)
        {
            IWebElement toast = WaitForVisible(toastLocator);
            string toastText = toast.Text;
            return toastText;
        }
    }
}

