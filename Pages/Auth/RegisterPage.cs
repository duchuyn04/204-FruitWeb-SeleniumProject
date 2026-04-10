using OpenQA.Selenium;
using SeleniumProject.Utilities;

namespace SeleniumProject.Pages.Auth
{
    public class RegisterPage
    {
        private readonly IWebDriver _driver;
        private readonly WaitHelper _wait;
        private readonly string _pageUrl;

        // ==============================================================
        // LOCATORS - Trang Đăng ký (/Account/Register)
        // ==============================================================

        // --- Form fields ---
        private readonly By NameInput            = By.Id("Name");
        private readonly By EmailInput           = By.Id("Email");
        private readonly By PasswordInput        = By.Id("Password");
        private readonly By ConfirmPasswordInput = By.Id("ConfirmPassword");

        // --- Submit ---
        private readonly By RegisterButton       = By.CssSelector("button.btn-auth-primary");

        // --- Validation messages (ASP.NET client-side) ---
        private readonly By NameError            = By.CssSelector("[data-valmsg-for='Name']");
        private readonly By EmailError           = By.CssSelector("[data-valmsg-for='Email']");
        private readonly By PasswordError        = By.CssSelector("[data-valmsg-for='Password']");
        private readonly By ConfirmPasswordError = By.CssSelector("[data-valmsg-for='ConfirmPassword']");

        // --- Thông báo lỗi server-side (alert / toast) ---
        private readonly By ServerError          = By.CssSelector(".alert-danger, .toast-body, .validation-summary-errors li");

        // ==============================================================
        public RegisterPage(IWebDriver driver, string baseUrl)
        {
            _driver  = driver;
            _wait    = new WaitHelper(driver);
            _pageUrl = baseUrl + "/Account/Register";
        }

        public void Open() => _driver.Navigate().GoToUrl(_pageUrl);

        // --- Nhập form ---
        public void EnterName(string name)
        {
            var el = _wait.WaitForVisible(NameInput);
            el.Clear();
            if (!string.IsNullOrEmpty(name))
                el.SendKeys(name);
        }

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

        public void EnterConfirmPassword(string confirmPassword)
        {
            var el = _wait.WaitForVisible(ConfirmPasswordInput);
            el.Clear();
            if (!string.IsNullOrEmpty(confirmPassword))
                el.SendKeys(confirmPassword);
        }

        public void ClickRegister() => _driver.FindElement(RegisterButton).Click();

        // Điền đầy đủ form và submit
        public void Register(string name, string email, string password, string confirmPassword)
        {
            EnterName(name);
            EnterEmail(email);
            EnterPassword(password);
            EnterConfirmPassword(confirmPassword);
            ClickRegister();
        }

        // --- Đọc lỗi validation ---
        public string GetNameError()            => GetFieldError(NameError);
        public string GetEmailError()           => GetFieldError(EmailError);
        public string GetPasswordError()        => GetFieldError(PasswordError);
        public string GetConfirmPasswordError() => GetFieldError(ConfirmPasswordError);

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
            var errors = new[] { NameError, EmailError, PasswordError, ConfirmPasswordError };
            return errors.Any(loc =>
            {
                try
                {
                    var el = _driver.FindElement(loc);
                    return el.Displayed && !string.IsNullOrWhiteSpace(el.Text);
                }
                catch { return false; }
            }) || GetAllValidationErrors().Count > 0;
        }

        // --- Kiểm tra trạng thái ---
        public bool IsOnPage()     => _driver.Url.Contains("/Account/Register");
        public bool IsRegistered() => _driver.Url.Contains("/Account/Login") || _driver.Url.Contains("/Account/Register") == false;

        public string DocKetQuaThucTe()
        {
            try
            {
                var toasts = _driver.FindElements(By.CssSelector(".toast-body, .alert, [class*='toast']"));
                foreach (var t in toasts)
                    if (t.Displayed && !string.IsNullOrWhiteSpace(t.Text))
                        return "Toast: " + t.Text.Trim();

                return $"URL={_driver.Url} | NameErr={GetNameError()} | EmailErr={GetEmailError()} | PassErr={GetPasswordError()}";
            }
            catch
            {
                return $"URL={_driver.Url}";
            }
        }
    }
}
