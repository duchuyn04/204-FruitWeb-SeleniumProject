using OpenQA.Selenium;
using SeleniumProject.Utilities;

namespace SeleniumProject.Pages.Auth
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly WaitHelper _wait;

        // URL trang đăng nhập — xây từ BaseUrl trong appsettings.json
        private readonly string LoginUrl;

        // Locators — khai báo cách tìm từng element trên trang
        private readonly By EmailInput = By.Id("Email");
        private readonly By PasswordInput = By.Id("Password");
        private readonly By LoginButton = By.CssSelector(".btn-auth-primary");
        private readonly By RememberMeCheckbox = By.Id("RememberMe");
        private readonly By ForgotPasswordLink = By.CssSelector("a[href='/Account/ForgotPassword']");
        private readonly By RegisterLink = By.CssSelector("a[href='/Account/Register']");
        private readonly By ToastMessage = By.CssSelector(".toast-body");

        public LoginPage(IWebDriver driver, string baseUrl)
        {
            _driver = driver;
            _wait = new WaitHelper(driver);
            LoginUrl = baseUrl.TrimEnd('/') + "/Account/Login";
        }

        // Mở trang đăng nhập
        public void Open()
        {
            _driver.Navigate().GoToUrl(LoginUrl);
        }

        // Nhập email (gõ từng ký tự để nhìn thấy quá trình)
        public void EnterEmail(string email)
        {
            _wait.SlowType(EmailInput, email);
        }

        // Nhập mật khẩu (gõ từng ký tự để nhìn thấy quá trình)
        public void EnterPassword(string password)
        {
            _wait.SlowType(PasswordInput, password);
        }

        // Click nút đăng nhập
        public void ClickLoginButton()
        {
            _wait.WaitForClickable(LoginButton).Click();
        }

        // Thực hiện toàn bộ luồng đăng nhập (email + password + click)
        public void Login(string email, string password)
        {
            EnterEmail(email);
            EnterPassword(password);
            ClickLoginButton();
        }

        // Tick vào ô "Ghi nhớ đăng nhập"
        public void CheckRememberMe()
        {
            var checkbox = _wait.WaitForVisible(RememberMeCheckbox);
            if (!checkbox.Selected)
                checkbox.Click();
        }

        // Click link "Quên mật khẩu?"
        public void ClickForgotPassword()
        {
            _wait.WaitForClickable(ForgotPasswordLink).Click();
        }

        // Click link "Đăng ký"
        public void ClickRegisterLink()
        {
            _wait.WaitForClickable(RegisterLink).Click();
        }

        // Lấy nội dung thông báo toast (thành công hoặc lỗi)
        public string GetToastMessage()
        {
            return _wait.WaitForVisible(ToastMessage).Text;
        }

        // Kiểm tra đã đăng nhập thành công chưa (dựa vào URL redirect về trang chủ)
        public bool IsLoginSuccessful()
        {
            return _driver.Url.Contains("/Home") || _driver.Url.Contains("vuatraicay.site/");
        }

        // Lấy URL hiện tại
        public string GetCurrentUrl()
        {
            return _driver.Url;
        }
    }
}
