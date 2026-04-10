using OpenQA.Selenium;
using SeleniumProject.Utilities;

namespace SeleniumProject.Pages.Auth
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly WaitHelper _wait;
        private readonly string _pageUrl;

        // ==============================================================
        // LOCATORS - Trang Đăng nhập (/Account/Login)
        // ==============================================================

        // --- Form fields ---
        private readonly By EmailInput      = By.Id("Email");
        private readonly By PasswordInput   = By.Id("Password");
        private readonly By RememberMeCheck = By.Id("RememberMe");

        // --- Submit ---
        private readonly By LoginButton     = By.CssSelector("button.btn-auth-primary");

        // --- Validation messages (ASP.NET client-side) ---
        private readonly By EmailError      = By.CssSelector("[data-valmsg-for='Email']");
        private readonly By PasswordError   = By.CssSelector("[data-valmsg-for='Password']");

        // --- Thông báo lỗi server-side ---
        private readonly By ServerError     = By.CssSelector(".alert-danger, .toast-body, .validation-summary-errors li");

        // ==============================================================
        public LoginPage(IWebDriver driver, string baseUrl)
        {
            _driver  = driver;
            _wait    = new WaitHelper(driver);
            _pageUrl = baseUrl + "/Account/Login";
        }

        public void Open() => _driver.Navigate().GoToUrl(_pageUrl);

        // --- Nhập form ---
        public void EnterEmail(string email)
        {
            var el = _wait.WaitForVisible(EmailInput);
            el.Clear();
            if (!string.IsNullOrEmpty(email))
                el.SendKeys(email);
        }

        public void EnterPassword(string password)
        {
            var el = _wait.WaitForVisible(PasswordInput);
            el.Clear();
            if (!string.IsNullOrEmpty(password))
                el.SendKeys(password);
        }

        public void SetRememberMe(bool remember)
        {
            var checkbox = _driver.FindElement(RememberMeCheck);
            if (checkbox.Selected != remember)
                checkbox.Click();
        }

        public void ClickLogin() => _driver.FindElement(LoginButton).Click();

        // Điền form và submit (rememberMe mặc định false)
        public void Login(string email, string password, bool rememberMe = false)
        {
            EnterEmail(email);
            EnterPassword(password);
            SetRememberMe(rememberMe);
            ClickLogin();
        }

        // --- Đọc lỗi validation ---
        public string GetEmailError()    => GetFieldError(EmailError);
        public string GetPasswordError() => GetFieldError(PasswordError);

        private string GetFieldError(By locator)
        {
            try
            {
                var el = _driver.FindElement(locator);
                return el.Displayed ? el.Text.Trim() : "";
            }
            catch { return ""; }
        }

        public List<string> GetAllValidationErrors()
        {
            try
            {
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
                var errors = _driver.FindElements(ServerError);
                return errors
                    .Where(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text))
                    .Select(e => e.Text.Trim())
                    .ToList();
            }
            finally
            {
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);
            }
        }

        public bool HasAnyError()
        {
            if (!string.IsNullOrEmpty(GetEmailError())) return true;
            if (!string.IsNullOrEmpty(GetPasswordError())) return true;
            return GetAllValidationErrors().Count > 0;
        }

        // --- Kiểm tra trạng thái ---
        public bool IsOnPage() => _driver.Url.Contains("/Account/Login");

        public bool IsLoggedIn() =>
            !_driver.Url.Contains("/Account/Login") &&
            !_driver.Url.Contains("/Account/Register");

        public bool IsRedirectedToAdmin() => _driver.Url.Contains("/Admin");

        public bool IsRedirectedToHome() =>
            _driver.Url.TrimEnd('/').EndsWith("5270") ||
            _driver.Url.Contains("localhost:5270/") && !_driver.Url.Contains("/Account") && !_driver.Url.Contains("/Admin");

        public string GetCurrentUrl() => _driver.Url;

        // Đăng nhập thành công = không còn ở trang Login
        public bool IsLoginSuccessful()
        {
            Thread.Sleep(800);
            return !_driver.Url.Contains("/Account/Login");
        }

        // Lấy nội dung toast message (server-side hoặc JS toast)
        public string GetToastMessage()
        {
            Thread.Sleep(800);
            try
            {
                var toasts = _driver.FindElements(By.CssSelector(".toast-body, .alert-danger, .alert-success, [class*='toast']"));
                foreach (var t in toasts)
                    if (t.Displayed && !string.IsNullOrWhiteSpace(t.Text))
                        return t.Text.Trim();
                return "";
            }
            catch { return ""; }
        }

        public string DocKetQuaThucTe()
        {
            try
            {
                var toasts = _driver.FindElements(By.CssSelector(".toast-body, .alert, [class*='toast']"));
                foreach (var t in toasts)
                    if (t.Displayed && !string.IsNullOrWhiteSpace(t.Text))
                        return "Toast: " + t.Text.Trim();

                return $"URL={_driver.Url} | EmailErr={GetEmailError()} | PassErr={GetPasswordError()}";
            }
            catch
            {
                return $"URL={_driver.Url}";
            }
        }
    }
}
