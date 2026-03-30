using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumProject.Utilities;

namespace SeleniumProject.Pages
{
    public class CheckoutPage
    {
        private readonly IWebDriver _driver;
        private readonly WaitHelper _wait;

        // URL
        private const string CheckoutUrl = "http://localhost:5270/Checkout";
        private const string CartUrl     = "http://localhost:5270/Cart";
        private const string LoginUrl    = "http://localhost:5270/Account/Login";
        private const string ShopUrl     = "http://localhost:5270/Shop";

        // ===== LOCATORS – Login =====
        private By EmailInput    => By.Id("Email");
        private By PasswordInput => By.Id("Password");
        private By LoginButton   => By.CssSelector("button.btn-auth-primary");

        // ===== LOCATORS – Product detail =====
        // class thật từ DOM: "btn-buy-now" và "btn-add-cart"
        private By AddToCartButton    => By.CssSelector("button.btn-add-cart");
        private By BuyNowButton       => By.CssSelector("button.btn-buy-now");
        // Update selector to handle /Checkout and avoid matching the hidden header dropdown link
        private By CartCheckoutButton => By.CssSelector("aside a[href*='/Checkout']");

        // ===== LOCATORS – Shipping form =====
        // Dropdown địa chỉ đã lưu (có khi user đã có địa chỉ mặc định)
        private By AddressSelect => By.Id("addressSelect");

        private By FullNameInput      => By.Id("FullNameInput");
        private By PhoneInput         => By.Id("Mobile");
        private By StreetAddressInput => By.Id("StreetAddressInput");
        private By NotesInput         => By.Id("Notes");

        // ===== LOCATORS – Địa danh (select thường, không phải Select2) =====
        private By ProvinceSelect => By.Id("provinceSelect");
        private By DistrictSelect => By.Id("districtSelect");
        private By WardSelect     => By.Id("wardSelect");

        // ===== LOCATORS – Payment =====
        private By PaymentCod      => By.Id("Delivery-1");
        private By PaymentTransfer => By.Id("Transfer-1");

        // ===== LOCATORS – Nút Đặt hàng =====
        // Không có ID → dùng XPath theo text
        private By PlaceOrderButton => By.XPath("//button[@type='submit' and contains(normalize-space(),'Đặt hàng')]");

        // class thật từ DOM: "invalid-feedback", ".field-validation-error" (ASP.NET), ".toast-body" (JS), ".validation-summary li" (JS tập trung lỗi)
        private By ValidationErrors => By.CssSelector(".invalid-feedback, .field-validation-error, .toast-body, .validation-summary li");

        // ===== LOCATORS – Confirmation page =====
        private By ConfirmationHeading => By.XPath("//h1[contains(., 'Cảm ơn')] | //h2[contains(., 'Cảm ơn')] | //*[contains(@class, 'thank-you')]");
        private By OrderNumberEl       => By.XPath("//*[contains(., 'Mã đơn hàng:')]//strong | //*[contains(@class, 'order-number')]");

        // ===== LOCATORS – Toast =====
        private By ToastBody => By.CssSelector(".toast-body");

        // =====================================================================
        public CheckoutPage(IWebDriver driver)
        {
            _driver = driver;
            _wait   = new WaitHelper(driver);
        }

        // =====================================================================
        // NAVIGATION
        // =====================================================================

        /// <summary>Đăng nhập</summary>
        public void Login(string email, string password)
        {
            _driver.Navigate().GoToUrl(LoginUrl);
            _wait.SlowType(EmailInput, email);
            _wait.SlowType(PasswordInput, password);
            _wait.WaitForClickable(LoginButton).Click();
            _wait.WaitForUrlContains("localhost:5270");
            Thread.Sleep(800);
        }

        /// <summary>Mở trang Checkout trực tiếp</summary>
        public void Open()
        {
            _driver.Navigate().GoToUrl(CheckoutUrl);
            Thread.Sleep(600);
        }

        /// <summary>Mở giỏ hàng</summary>
        public void OpenCart()
        {
            _driver.Navigate().GoToUrl(CartUrl);
            Thread.Sleep(600);
        }

        /// <summary>
        /// Pre-condition cho TC_CHECKOUT_01 (F5.1_01):
        /// Bước 1 – Thêm sản phẩm vào giỏ hàng (từ trang sản phẩm).
        /// Bước 2 – Vào trang Giỏ hàng (/Cart).
        /// Bước 3 – Click nút "THANH TOÁN" để sang trang Checkout.
        /// </summary>
        public void GoToCheckoutViaCart(string productUrl)
        {
            // Bước 1: Vào trang sản phẩm và thêm vào giỏ
            _driver.Navigate().GoToUrl(productUrl);
            Thread.Sleep(1000);

            try
            {
                _wait.WaitForClickable(AddToCartButton).Click();
            }
            catch
            {
                // Fallback: thử nút Mua ngay nếu không có Thêm vào giỏ
                _wait.WaitForClickable(BuyNowButton).Click();
                if (_driver.Url.Contains("/Checkout")) return; // Mua ngay đưa thẳng lên Checkout
            }

            Thread.Sleep(800);

            // Bước 2: Vào trang Giỏ hàng
            _driver.Navigate().GoToUrl(CartUrl);
            Thread.Sleep(800);

            // Bước 3: Click nút THANH TOÁN trên trang Cart
            var cartBtn = _wait.WaitForClickable(CartCheckoutButton);
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "var els = document.querySelectorAll('.back-to-top, footer, header, .sticky-header');" +
                "els.forEach(e => e.style.display = 'none');" +
                "arguments[0].scrollIntoView({block: 'center'});", cartBtn);
            Thread.Sleep(500);
            cartBtn.Click();
            Thread.Sleep(800);
        }

        /// <summary>
        /// Dùng cho các test cần đến trang Checkout nhanh (không quan tâm đến luồng Cart).
        /// Click "Mua ngay" trên trang sản phẩm → thẳng đến /Checkout.
        /// </summary>
        public void NavigateToCheckoutWithProduct(string productUrl)
        {
            _driver.Navigate().GoToUrl(productUrl);
            Thread.Sleep(1000);

            try
            {
                _wait.WaitForClickable(BuyNowButton).Click();
                Thread.Sleep(1000);
                if (!_driver.Url.Contains("/Checkout"))
                    _driver.Navigate().GoToUrl(CheckoutUrl);
            }
            catch
            {
                try
                {
                    _wait.WaitForClickable(AddToCartButton).Click();
                    Thread.Sleep(800);
                }
                catch { }
                _driver.Navigate().GoToUrl(CheckoutUrl);
            }

            Thread.Sleep(800);
        }

        // =====================================================================
        // FORM ACTIONS
        // =====================================================================

        /// <summary>
        /// Nếu trang Checkout có dropdown địa chỉ đã lưu (#addressSelect),
        /// chọn option "+ Thêm địa chỉ mới" (value="0") để hiện form nhập địa chỉ mới.
        /// </summary>
        public void SelectNewAddressOption()
        {
            try
            {
                var els = _driver.FindElements(AddressSelect);
                if (els.Count == 0) return; // Không có dropdown → form đã hiện sẵn

                var sel = new SelectElement(els[0]);

                // Ưu tiên chọn theo value="0" (+ Thêm địa chỉ mới)
                var byValue = sel.Options.FirstOrDefault(o => o.GetAttribute("value") == "0");
                if (byValue != null)
                {
                    sel.SelectByValue("0");
                }
                else
                {
                    // Fallback: tìm text chứa "mới"
                    var byText = sel.Options.FirstOrDefault(o =>
                        o.Text.Contains("mới", StringComparison.OrdinalIgnoreCase) ||
                        o.Text.Contains("Thêm", StringComparison.OrdinalIgnoreCase));
                    if (byText != null)
                        sel.SelectByText(byText.Text);
                    else
                        sel.SelectByIndex(sel.Options.Count - 1);
                }

                Thread.Sleep(700); // Đợi form hiện ra (có animation)
            }
            catch { /* Không có addressSelect → form đã hiện sẵn, bỏ qua */ }
        }

        /// <summary>Xóa và nhập Họ tên</summary>
        public void EnterFullName(string name)
        {
            var el = _wait.WaitForVisible(FullNameInput);
            el.Clear();
            if (!string.IsNullOrEmpty(name))
                _wait.SlowType(FullNameInput, name);
        }

        /// <summary>Xóa và nhập Số điện thoại</summary>
        public void EnterPhone(string phone)
        {
            var el = _wait.WaitForVisible(PhoneInput);
            el.Clear();
            if (!string.IsNullOrEmpty(phone))
                _wait.SlowType(PhoneInput, phone);
        }

        /// <summary>Xóa và nhập Địa chỉ đường phố</summary>
        public void EnterStreetAddress(string address)
        {
            var el = _wait.WaitForVisible(StreetAddressInput);
            el.Clear();
            if (!string.IsNullOrEmpty(address))
                _wait.SlowType(StreetAddressInput, address);
        }

        /// <summary>Nhập Ghi chú</summary>
        public void EnterNotes(string notes)
        {
            var el = _wait.WaitForVisible(NotesInput);
            el.Clear();
            if (!string.IsNullOrEmpty(notes))
                _wait.SlowType(NotesInput, notes);
        }

        /// <summary>Chọn Tỉnh/Thành phố theo tên (SelectElement thông thường)</summary>
        public void SelectProvince(string provinceName)
        {
            var sel = new SelectElement(_wait.WaitForVisible(ProvinceSelect));
            try { sel.SelectByText(provinceName); }
            catch
            {
                // Nếu không khớp chính xác, tìm option chứa tên
                var opt = sel.Options.FirstOrDefault(o =>
                    o.Text.Contains(provinceName, StringComparison.OrdinalIgnoreCase));
                if (opt != null) sel.SelectByText(opt.Text);
            }
            Thread.Sleep(800); // Đợi AJAX load Districts
        }

        /// <summary>Chọn Quận/Huyện theo tên</summary>
        public void SelectDistrict(string districtName)
        {
            // Đợi options được load (AJAX sau khi chọn tỉnh)
            Thread.Sleep(800);
            var sel = new SelectElement(_wait.WaitForVisible(DistrictSelect));
            try { sel.SelectByText(districtName); }
            catch
            {
                var opt = sel.Options.FirstOrDefault(o =>
                    o.Text.Contains(districtName, StringComparison.OrdinalIgnoreCase));
                if (opt != null) sel.SelectByText(opt.Text);
            }
            Thread.Sleep(800); // Đợi AJAX load Wards
        }

        /// <summary>Chọn Phường/Xã theo tên</summary>
        public void SelectWard(string wardName)
        {
            Thread.Sleep(800);
            var sel = new SelectElement(_wait.WaitForVisible(WardSelect));
            try { sel.SelectByText(wardName); }
            catch
            {
                var opt = sel.Options.FirstOrDefault(o =>
                    o.Text.Contains(wardName, StringComparison.OrdinalIgnoreCase));
                if (opt != null) sel.SelectByText(opt.Text);
            }
        }

        /// <summary>Chọn phương thức thanh toán COD</summary>
        public void SelectPaymentCod()
        {
            var radio = _wait.WaitForClickable(PaymentCod);
            if (!radio.Selected)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", radio);
            }
        }

        /// <summary>Chọn phương thức thanh toán Chuyển khoản</summary>
        public void SelectPaymentTransfer()
        {
            var radio = _wait.WaitForClickable(PaymentTransfer);
            if (!radio.Selected)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", radio);
            }
        }

        /// <summary>Click nút Đặt hàng</summary>
        public void ClickPlaceOrder()
        {
            var btn = _wait.WaitForClickable(PlaceOrderButton);
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "var els = document.querySelectorAll('.back-to-top, footer, header, .sticky-header');" +
                "els.forEach(e => e.style.display = 'none');" +
                "arguments[0].scrollIntoView({block: 'center'});", btn);
            Thread.Sleep(500);
            btn.Click();
        }

        // =====================================================================
        // STATE CHECKS
        // =====================================================================

        /// <summary>Kiểm tra trang Checkout đã hiển thị (có form và payment)</summary>
        public bool IsCheckoutPageDisplayed()
        {
            try
            {
                bool hasNameField    = _driver.FindElements(FullNameInput).Count > 0
                                       || _driver.FindElements(AddressSelect).Count > 0;
                bool hasPayment      = _driver.FindElements(PaymentCod).Count > 0;
                bool urlOk           = _driver.Url.Contains("/Checkout");
                return hasNameField && hasPayment && urlOk;
            }
            catch { return false; }
        }

        /// <summary>
        /// Sau khi truy cập Checkout khi cart rỗng,
        /// hệ thống phải redirect khỏi /Checkout hoặc hiển thị thông báo.
        /// </summary>
        public bool IsRedirectedFromEmptyCart()
        {
            Thread.Sleep(800);
            var url  = _driver.Url;
            var body = _driver.FindElement(By.TagName("body")).Text;
            return !url.Contains("/Checkout")
                || body.Contains("empty", StringComparison.OrdinalIgnoreCase)
                || body.Contains("trống", StringComparison.OrdinalIgnoreCase)
                || body.Contains("Cart is empty", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Lấy tất cả thông báo lỗi validation (class invalid-feedback)</summary>
        public List<string> GetValidationMessages()
        {
            Thread.Sleep(2000); // Đợi Toast, Validation, và Scroll animation hoàn tất
            try
            {
                var els = _driver.FindElements(ValidationErrors);
                return els
                    .Where(m => m.Displayed && !string.IsNullOrWhiteSpace(m.Text))
                    .Select(m => m.Text.Trim())
                    .ToList();
            }
            catch { return new List<string>(); }
        }

        /// <summary>Kiểm tra có validation error chứa chuỗi cho trước không</summary>
        public bool HasValidationError(string text)
        {
            return GetValidationMessages()
                .Any(m => m.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Kiểm tra field có class "is-invalid" hoặc "input-validation-error"</summary>
        public bool IsFieldInvalid(By locator)
        {
            try
            {
                var cls = _driver.FindElement(locator).GetAttribute("class") ?? "";
                return cls.Contains("is-invalid") || cls.Contains("input-validation-error");
            }
            catch { return false; }
        }

        /// <summary>Sau khi click Đặt hàng, nếu có lỗi thì vẫn ở trang Checkout</summary>
        public bool IsStillOnCheckoutPage()
        {
            Thread.Sleep(500);
            return _driver.Url.Contains("/Checkout") && !_driver.Url.Contains("Confirmation");
        }

        /// <summary>Lấy danh sách Districts trong dropdown (sau khi chọn Province)</summary>
        public IList<string> GetAvailableDistricts()
        {
            Thread.Sleep(800);
            var sel = new SelectElement(_driver.FindElement(DistrictSelect));
            return sel.Options
                .Select(o => o.Text.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
        }

        /// <summary>Lấy danh sách Wards trong dropdown (sau khi chọn District)</summary>
        public IList<string> GetAvailableWards()
        {
            Thread.Sleep(800);
            var sel = new SelectElement(_driver.FindElement(WardSelect));
            return sel.Options
                .Select(o => o.Text.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
        }

        /// <summary>Kiểm tra COD radio đang được chọn</summary>
        public bool IsCodSelected()
        {
            try { return _driver.FindElement(PaymentCod).Selected; }
            catch { return false; }
        }

        /// <summary>Kiểm tra Chuyển khoản radio đang được chọn</summary>
        public bool IsTransferSelected()
        {
            try { return _driver.FindElement(PaymentTransfer).Selected; }
            catch { return false; }
        }

        /// <summary>
        /// Kiểm tra thông tin ngân hàng hiển thị sau khi chọn Chuyển khoản.
        /// Tìm trong body text các từ khoá liên quan đến ngân hàng.
        /// </summary>
        public bool IsBankInfoDisplayed()
        {
            Thread.Sleep(600);
            try
            {
                var body = _driver.FindElement(By.TagName("body")).Text;
                return body.Contains("STK", StringComparison.OrdinalIgnoreCase)
                    || body.Contains("Ngân hàng", StringComparison.OrdinalIgnoreCase)
                    || body.Contains("ngân hàng", StringComparison.OrdinalIgnoreCase)
                    || body.Contains("QR", StringComparison.OrdinalIgnoreCase)
                    || body.Contains("chuyển khoản", StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        // =====================================================================
        // CONFIRMATION PAGE
        // =====================================================================

        /// <summary>Kiểm tra URL đã chuyển đến /Checkout/Confirmation</summary>
        public bool IsOnConfirmationPage()
        {
            try
            {
                _wait.WaitForUrlContains("/Checkout/Confirmation");
                return _driver.Url.Contains("/Checkout/Confirmation");
            }
            catch { return false; }
        }

        /// <summary>Lấy text tiêu đề / lời cảm ơn trên trang xác nhận</summary>
        public string GetConfirmationMessage()
        {
            try { return _wait.WaitForVisible(ConfirmationHeading).Text; }
            catch { return _driver.FindElement(By.TagName("body")).Text; }
        }

        /// <summary>Lấy mã đơn hàng trên trang xác nhận</summary>
        public string GetOrderNumber()
        {
            try { return _driver.FindElement(OrderNumberEl).Text; }
            catch { return string.Empty; }
        }

        /// <summary>Lấy URL hiện tại</summary>
        public string GetCurrentUrl() => _driver.Url;

        public IWebDriver GetDriver() => _driver;

        /// <summary>Lấy text của element theo locator</summary>
        public string GetElementText(By locator)
        {
            try { return _driver.FindElement(locator).Text; }
            catch { return string.Empty; }
        }

        // =====================================================================
        // ORDER SUMMARY HELPERS
        // =====================================================================

        /// <summary>
        /// Lấy toàn bộ text của khu vực Order Summary trên trang Checkout.
        /// Thử tìm container riêng; nếu không tìm thấy, trả về body text.
        /// </summary>
        public string GetOrderSummaryText()
        {
            Thread.Sleep(500);
            try
            {
                var candidates = new[]
                {
                    ".order-summary", ".checkout-summary", "[class*='order-summary']",
                    "[class*='cart-summary']", ".summary-section", "aside .summary"
                };
                foreach (var sel in candidates)
                {
                    var els = _driver.FindElements(By.CssSelector(sel));
                    if (els.Count > 0 && !string.IsNullOrWhiteSpace(els[0].Text))
                        return els[0].Text;
                }
                // Fallback: toàn bộ body
                return _driver.FindElement(By.TagName("body")).Text;
            }
            catch { return string.Empty; }
        }

        /// <summary>
        /// Kiểm tra Order Summary / body text có chứa chuỗi cho trước không.
        /// </summary>
        public bool OrderSummaryContains(string text)
            => GetOrderSummaryText().Contains(text, StringComparison.OrdinalIgnoreCase);

        // =====================================================================
        // CART HELPERS
        // =====================================================================

        /// <summary>
        /// Điều hướng đến /Cart và kiểm tra giỏ hàng có trống không.
        /// </summary>
        public bool IsCartEmpty()
        {
            try
            {
                _driver.Navigate().GoToUrl(CartUrl);
                Thread.Sleep(800);
                var body = _driver.FindElement(By.TagName("body")).Text;
                bool emptyByText = body.Contains("trống", StringComparison.OrdinalIgnoreCase)
                    || body.Contains("empty", StringComparison.OrdinalIgnoreCase)
                    || body.Contains("No items", StringComparison.OrdinalIgnoreCase)
                    || body.Contains("0 sản phẩm", StringComparison.OrdinalIgnoreCase);
                bool noItemElements = _driver.FindElements(
                    By.CssSelector(".cart-item, .cart-product, .cart-row, tr.product-row")).Count == 0;
                return emptyByText || noItemElements;
            }
            catch { return false; }
        }
    }
}
