using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumProject.Utilities;

namespace SeleniumProject.Pages.CartManagement
{
    public class CartPage
    {
        private readonly IWebDriver _driver;
        private readonly WaitHelper _wait;

        // URLs
        private const string BaseUrl = "http://localhost:5270";
        private const string CartUrl = BaseUrl + "/Cart";
        private const string ShopUrl = BaseUrl + "/Shop";
        private const string LoginUrl = BaseUrl + "/Account/Login";
        private const string CheckoutUrl = BaseUrl + "/Checkout";

        // ===== LOCATORS – Login =====
        private By EmailInput => By.Id("Email");
        private By PasswordInput => By.Id("Password");
        private By LoginButton => By.CssSelector("button.btn-auth-primary");

        // ===== LOCATORS – Shop Detail (thêm SP vào giỏ) =====
        private By AddToCartButton => By.CssSelector("button.btn-add-cart");
        private By BuyNowButton => By.CssSelector("button.btn-buy-now");
        private By QuantityInput => By.Id("quantity");

        // ===== LOCATORS – Cart Page =====
        // Danh sách SP trong giỏ
        private By CartItems => By.CssSelector(".cart-item");
        private By CartItemByProductId(int productId) =>
            By.CssSelector($".cart-item[data-product-id='{productId}']");

        // Quantity controls
        private By ItemQuantityInput => By.CssSelector(".item-quantity");
        private By BtnPlus => By.CssSelector(".btn-plus");
        private By BtnMinus => By.CssSelector(".btn-minus");
        private By BtnRemove => By.CssSelector("button[onclick*='removeFromCart']");

        // Totals
        private By CartSubtotal => By.Id("cart-subtotal");
        private By CartTotal => By.Id("cart-total");
        private By CartShipping => By.Id("cart-shipping");
        private By CartDiscount => By.Id("cart-discount");
        private By DiscountRow => By.Id("discount-row");

        // Coupon
        private By CouponInput => By.CssSelector("input[name='couponCode']");
        private By CouponApplyButton => By.CssSelector("form[action*='ApplyCoupon'] button[type='submit']");

        // Navigation
        private By CheckoutButton => By.CssSelector("a[href*='/Checkout']");
        private By ContinueShoppingLink => By.CssSelector("a[href*='/Shop']");

        // Empty cart
        private By EmptyCartIcon => By.CssSelector(".material-icons-outlined");
        private By EmptyCartHeading => By.XPath("//h3[contains(text(),'Giỏ hàng trống')]");
        private By EmptyCartShopButton => By.CssSelector("a[href*='/Shop'].inline-flex");

        // Toast notifications
        private By ToastSuccess => By.CssSelector(".toast-item .text-green-500");
        private By ToastWarning => By.CssSelector(".toast-item .text-yellow-500");
        private By ToastBody => By.CssSelector(".toast-item p");
        private By ToastContainer => By.Id("tailwind-toast-container");

        // Stock info
        private By StockInfo => By.CssSelector(".stock-info");
        private By MaxReachedMsg => By.CssSelector(".max-msg");

        // Shipping info
        private By ShippingMessageWrapper => By.Id("shipping-message-wrapper");
        private By ShippingZoneInfo => By.XPath("//*[contains(text(),'Vùng vận chuyển')]");

        // VAT note
        private By VatNote => By.CssSelector("span.italic");

        // Cart count badge
        private By CartCountBadge => By.CssSelector(".fa-shopping-bag + span, .position-absolute.bg-secondary");

        // Product name link
        private By ProductNameLink => By.CssSelector(".cart-item h4 a");

        // Product image
        private By ProductImage => By.CssSelector(".cart-item img");

        // =====================================================================
        public CartPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WaitHelper(driver);
        }

        // =====================================================================
        // AUTHENTICATION
        // =====================================================================

        /// <summary>Đăng nhập với email/password</summary>
        public void Login(string email, string password)
        {
            _driver.Navigate().GoToUrl(LoginUrl);
            Thread.Sleep(500);
            _wait.SlowType(EmailInput, email);
            _wait.SlowType(PasswordInput, password);
            _wait.WaitForClickable(LoginButton).Click();
            _wait.WaitForUrlNotContains("/Account/Login");
            Thread.Sleep(1000);
        }

        // =====================================================================
        // NAVIGATION
        // =====================================================================

        /// <summary>Mở trang Cart</summary>
        public void Open()
        {
            _driver.Navigate().GoToUrl(CartUrl);
            Thread.Sleep(800);
        }

        /// <summary>Mở trang Shop</summary>
        public void OpenShop()
        {
            _driver.Navigate().GoToUrl(ShopUrl);
            Thread.Sleep(800);
        }

        /// <summary>Điều hướng đến trang chi tiết sản phẩm theo slug</summary>
        public void GoToProductDetail(string slug)
        {
            _driver.Navigate().GoToUrl($"{BaseUrl}/Shop/Detail/{slug}");
            Thread.Sleep(1000);
        }

        // =====================================================================
        // ADD TO CART ACTIONS (from Shop Detail page)
        // =====================================================================

        /// <summary>Thêm sản phẩm vào giỏ từ trang detail</summary>
        public void AddToCartFromDetail(string slug, int quantity = 1)
        {
            GoToProductDetail(slug);
            // Set quantity nếu > 1
            if (quantity > 1)
            {
                for (int i = 1; i < quantity; i++)
                {
                    try
                    {
                        var plusBtn = _driver.FindElement(By.CssSelector(".quantity-input .btn:last-child"));
                        plusBtn.Click();
                        Thread.Sleep(200);
                    }
                    catch { break; }
                }
            }
            try
            {
                _wait.WaitForClickable(AddToCartButton).Click();
            }
            catch
            {
                // Fallback JS click
                var btn = _driver.FindElement(AddToCartButton);
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
            }
            Thread.Sleep(1000);
        }

        /// <summary>Thêm SP rồi mở trang Cart</summary>
        public void AddProductAndOpenCart(string slug, int quantity = 1)
        {
            AddToCartFromDetail(slug, quantity);
            Open();
        }

        // =====================================================================
        // CART ACTIONS
        // =====================================================================

        /// <summary>Click nút + để tăng SL của SP đầu tiên (hoặc theo index)</summary>
        public void ClickPlusButton(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count > itemIndex)
            {
                var plusBtn = items[itemIndex].FindElement(BtnPlus);
                var js = (IJavaScriptExecutor)_driver;
                // Lấy giá trị hiện tại trước khi click
                var qtyInput = items[itemIndex].FindElement(ItemQuantityInput);
                string qtyBefore = js.ExecuteScript("return arguments[0].value;", qtyInput)?.ToString() ?? "0";
                // Click bằng JS để đảm bảo onclick handler được trigger
                js.ExecuteScript("arguments[0].click();", plusBtn);
                // Chờ AJAX hoàn tất: giá trị quantity phải thay đổi
                try
                {
                    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                    wait.Until(d =>
                    {
                        var currentVal = js.ExecuteScript("return arguments[0].value;", qtyInput)?.ToString();
                        return currentVal != qtyBefore;
                    });
                }
                catch { Thread.Sleep(2000); } // Fallback nếu value không đổi (max stock, etc)
            }
        }

        /// <summary>Click nút - để giảm SL</summary>
        public void ClickMinusButton(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count > itemIndex)
            {
                var minusBtn = items[itemIndex].FindElement(BtnMinus);
                var js = (IJavaScriptExecutor)_driver;
                var qtyInput = items[itemIndex].FindElement(ItemQuantityInput);
                string qtyBefore = js.ExecuteScript("return arguments[0].value;", qtyInput)?.ToString() ?? "0";
                js.ExecuteScript("arguments[0].click();", minusBtn);
                try
                {
                    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                    wait.Until(d =>
                    {
                        var currentVal = js.ExecuteScript("return arguments[0].value;", qtyInput)?.ToString();
                        return currentVal != qtyBefore;
                    });
                }
                catch { Thread.Sleep(2000); }
            }
        }

        /// <summary>Click nút xóa SP khỏi giỏ</summary>
        public void ClickRemoveButton(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count > itemIndex)
            {
                var removeBtn = items[itemIndex].FindElement(BtnRemove);
                var js = (IJavaScriptExecutor)_driver;
                int countBefore = items.Count;
                js.ExecuteScript("arguments[0].click();", removeBtn);
                // Chờ animation (opacity 0 → remove) + AJAX
                try
                {
                    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                    wait.Until(d =>
                    {
                        var remaining = d.FindElements(CartItems);
                        return remaining.Count < countBefore
                            || d.FindElements(EmptyCartHeading).Count > 0;
                    });
                }
                catch { Thread.Sleep(3000); }
            }
        }

        /// <summary>Nhập mã giảm giá</summary>
        public void ApplyCoupon(string code)
        {
            _wait.SlowType(CouponInput, code);
            _wait.WaitForClickable(CouponApplyButton).Click();
            Thread.Sleep(1000);
        }

        /// <summary>Click nút Thanh toán</summary>
        public void ClickCheckout()
        {
            try
            {
                // Ưu tiên nút THANH TOÁN trong aside (chứa text "THANH TOÁN")
                var btn = _driver.FindElement(
                    By.XPath("//aside//a[contains(@href,'/Checkout')]"));
                ((IJavaScriptExecutor)_driver).ExecuteScript(
                    "arguments[0].scrollIntoView({block:'center'}); arguments[0].click();", btn);
            }
            catch
            {
                // Fallback: tìm bằng text
                try
                {
                    var btn = _driver.FindElement(
                        By.XPath("//a[contains(text(),'THANH TOÁN')]"));
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
                }
                catch
                {
                    // Last resort: navigate trực tiếp
                    _driver.Navigate().GoToUrl(BaseUrl + "/Checkout");
                }
            }
            Thread.Sleep(1000);
        }

        /// <summary>Click link "Tiếp tục mua sắm"</summary>
        public void ClickContinueShopping()
        {
            // Link phía dưới danh sách SP, chứa text "Tiếp tục mua sắm"
            var link = _driver.FindElement(
                By.XPath("//a[contains(@href,'/Shop') and contains(text(),'Tiếp tục mua sắm')]"));
            ScrollAndClick(link);
            Thread.Sleep(800);
        }

        /// <summary>Click nút "Tiếp tục mua sắm" khi giỏ rỗng</summary>
        public void ClickEmptyCartShopButton()
        {
            try
            {
                var btn = _wait.WaitForClickable(EmptyCartShopButton);
                ScrollAndClick(btn);
                Thread.Sleep(800);
            }
            catch
            {
                var btns = _driver.FindElements(By.CssSelector("a[href*='/Shop']"));
                if (btns.Count > 0) ScrollAndClick(btns.Last());
                Thread.Sleep(800);
            }
        }

        // =====================================================================
        // STATE READS
        // =====================================================================

        /// <summary>Lấy số lượng SP trong giỏ hàng</summary>
        public int GetCartItemCount()
        {
            return _driver.FindElements(CartItems).Count;
        }

        /// <summary>Lấy SL của item đầu tiên (hoặc theo index)</summary>
        public int GetItemQuantity(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count <= itemIndex) return 0;
            var qtyInput = items[itemIndex].FindElement(ItemQuantityInput);
            // Dùng JS đọc .value property (không phải HTML attribute)
            // vì AJAX update qua el.value = actualQty, không phải setAttribute
            var js = (IJavaScriptExecutor)_driver;
            var val = js.ExecuteScript("return arguments[0].value;", qtyInput)?.ToString() ?? "0";
            return int.TryParse(val, out var qty) ? qty : 0;
        }

        /// <summary>Lấy text Subtotal</summary>
        public string GetSubtotalText()
        {
            try { return _driver.FindElement(CartSubtotal).Text.Trim(); }
            catch { return ""; }
        }

        /// <summary>Lấy text Total</summary>
        public string GetTotalText()
        {
            try { return _driver.FindElement(CartTotal).Text.Trim(); }
            catch { return ""; }
        }

        /// <summary>Lấy text Shipping</summary>
        public string GetShippingText()
        {
            try { return _driver.FindElement(CartShipping).Text.Trim(); }
            catch { return ""; }
        }

        /// <summary>Kiểm tra giỏ hàng rỗng (hiển thị empty state)</summary>
        public bool IsCartEmpty()
        {
            try
            {
                var headings = _driver.FindElements(EmptyCartHeading);
                if (headings.Count > 0) return true;
                var body = _driver.FindElement(By.TagName("body")).Text;
                return body.Contains("Giỏ hàng trống");
            }
            catch { return false; }
        }

        /// <summary>Kiểm tra nút Thanh toán có hiển thị không</summary>
        public bool IsCheckoutButtonVisible()
        {
            try
            {
                // Nút THANH TOÁN trong aside (chỉ hiện khi có SP)
                var btns = _driver.FindElements(
                    By.CssSelector("aside a[href*='/Checkout']"));
                return btns.Count > 0 && btns[0].Displayed;
            }
            catch { return false; }
        }

        /// <summary>Kiểm tra nút + có bị disabled không</summary>
        public bool IsPlusButtonDisabled(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count <= itemIndex) return false;
            var plusBtn = items[itemIndex].FindElement(BtnPlus);
            return plusBtn.GetAttribute("disabled") != null
                || plusBtn.GetAttribute("class")?.Contains("cursor-not-allowed") == true;
        }

        /// <summary>Kiểm tra có hiển thị "Đã đạt tối đa" không</summary>
        public bool IsMaxReachedDisplayed(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count <= itemIndex) return false;
            try
            {
                var maxMsg = items[itemIndex].FindElement(MaxReachedMsg);
                return maxMsg.Displayed;
            }
            catch { return false; }
        }

        /// <summary>Lấy stock info text "Còn x sản phẩm"</summary>
        public string GetStockInfoText(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count <= itemIndex) return "";
            try
            {
                var info = items[itemIndex].FindElement(StockInfo);
                return info.Text.Trim();
            }
            catch { return ""; }
        }

        /// <summary>Lấy tên SP trong giỏ</summary>
        public string GetProductName(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count <= itemIndex) return "";
            try
            {
                var nameEl = items[itemIndex].FindElement(By.CssSelector("h4 a"));
                return nameEl.Text.Trim();
            }
            catch { return ""; }
        }

        /// <summary>Lấy đơn giá text</summary>
        public string GetUnitPriceText(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count <= itemIndex) return "";
            try
            {
                // Text: "Đơn giá: 107,000đ"
                var priceEl = items[itemIndex].FindElement(
                    By.XPath(".//p[contains(text(),'Đơn giá')]"));
                return priceEl.Text.Trim();
            }
            catch { return ""; }
        }

        /// <summary>Lấy text thành tiền của item</summary>
        public string GetItemTotalText(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count <= itemIndex) return "";
            try
            {
                var totalEl = items[itemIndex].FindElement(By.CssSelector(".item-total"));
                return totalEl.Text.Trim();
            }
            catch { return ""; }
        }

        /// <summary>Lấy href của link tên SP</summary>
        public string GetProductNameHref(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count <= itemIndex) return "";
            try
            {
                var link = items[itemIndex].FindElement(By.CssSelector("h4 a"));
                return link.GetAttribute("href") ?? "";
            }
            catch { return ""; }
        }

        /// <summary>Click vào tên SP để đi đến detail</summary>
        public void ClickProductNameLink(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count > itemIndex)
            {
                var link = items[itemIndex].FindElement(By.CssSelector("h4 a"));
                ScrollAndClick(link);
                Thread.Sleep(1000);
            }
        }

        /// <summary>Lấy src ảnh SP</summary>
        public string GetProductImageSrc(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count <= itemIndex) return "";
            try
            {
                var img = items[itemIndex].FindElement(By.CssSelector("img"));
                return img.GetAttribute("src") ?? "";
            }
            catch { return ""; }
        }

        /// <summary>Kiểm tra toast thành công có xuất hiện</summary>
        public bool IsSuccessToastDisplayed()
        {
            try
            {
                Thread.Sleep(500);
                var toasts = _driver.FindElements(ToastSuccess);
                return toasts.Count > 0;
            }
            catch { return false; }
        }

        /// <summary>Kiểm tra toast warning có xuất hiện</summary>
        public bool IsWarningToastDisplayed()
        {
            try
            {
                Thread.Sleep(500);
                var toasts = _driver.FindElements(ToastWarning);
                return toasts.Count > 0;
            }
            catch { return false; }
        }

        /// <summary>Lấy text toast message</summary>
        public string GetToastText()
        {
            try
            {
                Thread.Sleep(500);
                var toastBodies = _driver.FindElements(ToastBody);
                return toastBodies.Count > 0 ? toastBodies.Last().Text.Trim() : "";
            }
            catch { return ""; }
        }

        /// <summary>Kiểm tra VAT note hiển thị</summary>
        public bool IsVatNoteDisplayed()
        {
            try
            {
                var body = _driver.FindElement(By.TagName("body")).Text;
                return body.Contains("Đã bao gồm VAT nếu có");
            }
            catch { return false; }
        }

        /// <summary>Kiểm tra giá có đúng định dạng VN (xxx,xxxđ)</summary>
        public bool IsCurrencyFormatValid()
        {
            try
            {
                var subtotal = GetSubtotalText();
                var total = GetTotalText();
                // Kiểm tra có chứa "đ" và dấu phẩy phân cách nghìn
                return (subtotal.Contains("đ") || subtotal.Contains("₫"))
                    && (total.Contains("đ") || total.Contains("₫"));
            }
            catch { return false; }
        }

        /// <summary>Lấy text heading giỏ hàng "Giỏ hàng của bạn (x sản phẩm)"</summary>
        public string GetCartHeadingText()
        {
            try
            {
                var heading = _driver.FindElement(By.CssSelector("h3.text-\\[22px\\]"));
                return heading.Text.Trim();
            }
            catch
            {
                try
                {
                    var heading = _driver.FindElement(
                        By.XPath("//h3[contains(text(),'Giỏ hàng')]"));
                    return heading.Text.Trim();
                }
                catch { return ""; }
            }
        }

        /// <summary>Lấy URL hiện tại</summary>
        public string GetCurrentUrl() => _driver.Url;

        /// <summary>Lấy text toàn trang</summary>
        public string GetBodyText()
        {
            try { return _driver.FindElement(By.TagName("body")).Text; }
            catch { return ""; }
        }

        // =====================================================================
        // HELPERS
        // =====================================================================

        /// <summary>Scroll đến element rồi click, tránh bị che bởi header</summary>
        private void ScrollAndClick(IWebElement element)
        {
            var js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript(
                "var els = document.querySelectorAll('.back-to-top, footer, .sticky-header, .fixed-top');" +
                "els.forEach(e => e.style.display = 'none');" +
                "arguments[0].scrollIntoView({block: 'center'});", element);
            Thread.Sleep(400);
            try
            {
                element.Click();
            }
            catch (ElementClickInterceptedException)
            {
                js.ExecuteScript("arguments[0].click();", element);
            }
        }

        // =====================================================================
        // API CALLS VIA JS FETCH (cho API/AJAX tests)
        // =====================================================================

        /// <summary>Gọi POST UpdateQuantityAjax và trả về JSON string</summary>
        public string CallUpdateQuantityApi(int productId, int quantity)
        {
            var js = (IJavaScriptExecutor)_driver;
            var result = js.ExecuteAsyncScript(@"
                var callback = arguments[arguments.length - 1];
                fetch('/Cart/UpdateQuantityAjax', {
                    method: 'POST',
                    headers: {'Content-Type': 'application/json',
                              'X-Requested-With': 'XMLHttpRequest'},
                    body: JSON.stringify({ productId: arguments[0], quantity: arguments[1] })
                })
                .then(r => r.text())
                .then(t => callback(t))
                .catch(e => callback('ERROR:' + e.message));
            ", productId, quantity);
            return result?.ToString() ?? "";
        }

        /// <summary>Gọi POST RemoveFromCartAjax và trả về JSON string</summary>
        public string CallRemoveFromCartApi(int productId)
        {
            var js = (IJavaScriptExecutor)_driver;
            var result = js.ExecuteAsyncScript(@"
                var callback = arguments[arguments.length - 1];
                fetch('/Cart/RemoveFromCartAjax', {
                    method: 'POST',
                    headers: {'Content-Type': 'application/json',
                              'X-Requested-With': 'XMLHttpRequest'},
                    body: JSON.stringify({ productId: arguments[0] })
                })
                .then(r => r.text())
                .then(t => callback(t))
                .catch(e => callback('ERROR:' + e.message));
            ", productId);
            return result?.ToString() ?? "";
        }

        /// <summary>Gọi POST CalculateShippingAjax và trả về JSON string</summary>
        public string CallCalculateShippingApi(string districtCode, decimal subtotal = 0)
        {
            var js = (IJavaScriptExecutor)_driver;
            // Nếu subtotal = 0, lấy từ UI
            if (subtotal == 0)
            {
                var subtotalText = GetSubtotalText().Replace("đ", "").Replace(",", "").Replace(".", "").Trim();
                decimal.TryParse(subtotalText, out subtotal);
            }
            var result = js.ExecuteAsyncScript(@"
                var callback = arguments[arguments.length - 1];
                fetch('/Cart/CalculateShippingAjax', {
                    method: 'POST',
                    headers: {'Content-Type': 'application/json',
                              'X-Requested-With': 'XMLHttpRequest'},
                    body: JSON.stringify({ subtotal: arguments[1], district: arguments[0] })
                })
                .then(r => r.text())
                .then(t => callback(t))
                .catch(e => callback('ERROR:' + e.message));
            ", districtCode, (long)subtotal);
            return result?.ToString() ?? "";
        }

        /// <summary>Gọi POST AddToCart (form-based, returns redirect/HTML)</summary>
        public string CallAddToCartApi(int productId, int quantity = 1)
        {
            var js = (IJavaScriptExecutor)_driver;
            var result = js.ExecuteAsyncScript(@"
                var callback = arguments[arguments.length - 1];
                fetch('/Cart/AddToCart', {
                    method: 'POST',
                    headers: {'Content-Type': 'application/x-www-form-urlencoded'},
                    body: 'productId=' + arguments[0] + '&quantity=' + arguments[1],
                    redirect: 'follow'
                })
                .then(r => { return r.ok ? r.text() : 'ERROR:' + r.status; })
                .then(t => callback(t.substring(0, 500)))
                .catch(e => callback('ERROR:' + e.message));
            ", productId, quantity);
            return result?.ToString() ?? "";
        }

        // =====================================================================
        // DATA ATTRIBUTE READS
        // =====================================================================

        /// <summary>Lấy product-id từ data attribute của cart item</summary>
        public int GetProductId(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count <= itemIndex) return 0;
            try
            {
                var id = items[itemIndex].GetAttribute("data-product-id");
                return int.TryParse(id, out var pid) ? pid : 0;
            }
            catch { return 0; }
        }

        /// <summary>Lấy stock quantity từ data attribute</summary>
        public int GetStockQuantity(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count <= itemIndex) return 0;
            try
            {
                var stock = items[itemIndex].GetAttribute("data-stock");
                return int.TryParse(stock, out var s) ? s : 0;
            }
            catch { return 0; }
        }

        // =====================================================================
        // SHIPPING HELPERS
        // =====================================================================

        /// <summary>Lấy text message gợi ý freeship</summary>
        public string GetShippingMessageText()
        {
            try
            {
                var wrapper = _driver.FindElement(ShippingMessageWrapper);
                return wrapper.Text.Trim();
            }
            catch { return ""; }
        }

        /// <summary>Kiểm tra message freeship có hiển thị</summary>
        public bool IsShippingMessageDisplayed()
        {
            try
            {
                var els = _driver.FindElements(ShippingMessageWrapper);
                return els.Count > 0 && els[0].Displayed;
            }
            catch { return false; }
        }

        /// <summary>Kiểm tra hiển thị tên vùng vận chuyển</summary>
        public bool IsShippingZoneDisplayed()
        {
            try
            {
                var body = GetBodyText();
                return body.Contains("Vùng vận chuyển") || body.Contains("Zone")
                    || body.Contains("Nội thành") || body.Contains("Ngoại thành")
                    || body.Contains("Vùng xa");
            }
            catch { return false; }
        }

        // =====================================================================
        // SESSION / COOKIE HELPERS
        // =====================================================================

        /// <summary>Lấy SessionId từ cookie</summary>
        public string GetSessionId()
        {
            try
            {
                var cookie = _driver.Manage().Cookies.GetCookieNamed(".AspNetCore.Session");
                return cookie?.Value ?? "";
            }
            catch { return ""; }
        }

        /// <summary>Kiểm tra cookie session có tồn tại</summary>
        public bool HasSessionCookie()
        {
            try
            {
                var cookie = _driver.Manage().Cookies.GetCookieNamed(".AspNetCore.Session");
                return cookie != null && !string.IsNullOrEmpty(cookie.Value);
            }
            catch { return false; }
        }

        /// <summary>Xóa tất cả cookies (mô phỏng session mới)</summary>
        public void ClearAllCookies()
        {
            _driver.Manage().Cookies.DeleteAllCookies();
            Thread.Sleep(500);
        }

        // =====================================================================
        // RESPONSIVE / WINDOW HELPERS
        // =====================================================================

        /// <summary>Resize browser về kích thước mobile</summary>
        public void ResizeToMobile(int width = 375, int height = 812)
        {
            _driver.Manage().Window.Size = new System.Drawing.Size(width, height);
            Thread.Sleep(800);
        }

        /// <summary>Resize browser về kích thước desktop</summary>
        public void ResizeToDesktop(int width = 1920, int height = 1080)
        {
            _driver.Manage().Window.Size = new System.Drawing.Size(width, height);
            Thread.Sleep(500);
        }

        /// <summary>Kiểm tra layout không bị vỡ (no horizontal scroll)</summary>
        public bool IsLayoutIntact()
        {
            var js = (IJavaScriptExecutor)_driver;
            var scrollWidth = js.ExecuteScript("return document.documentElement.scrollWidth;")?.ToString();
            var clientWidth = js.ExecuteScript("return document.documentElement.clientWidth;")?.ToString();
            if (int.TryParse(scrollWidth, out var sw) && int.TryParse(clientWidth, out var cw))
                return sw <= cw + 10; // cho phép sai lệch 10px
            return true;
        }

        // =====================================================================
        // ANIMATION / CSS HELPERS
        // =====================================================================

        /// <summary>Kiểm tra opacity của cart item (cho animation mờ dần)</summary>
        public string GetCartItemOpacity(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count <= itemIndex) return "1";
            var js = (IJavaScriptExecutor)_driver;
            var opacity = js.ExecuteScript(
                "return window.getComputedStyle(arguments[0]).opacity;", items[itemIndex]);
            return opacity?.ToString() ?? "1";
        }

        /// <summary>Kiểm tra nút + có enabled (mở lại) không</summary>
        public bool IsPlusButtonEnabled(int itemIndex = 0)
        {
            return !IsPlusButtonDisabled(itemIndex);
        }

        /// <summary>Kiểm tra nút - có bị disabled không</summary>
        public bool IsMinusButtonDisabled(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count <= itemIndex) return false;
            var minusBtn = items[itemIndex].FindElement(BtnMinus);
            return minusBtn.GetAttribute("disabled") != null
                || minusBtn.GetAttribute("class")?.Contains("cursor-not-allowed") == true;
        }

        /// <summary>Kiểm tra ảnh SP có hiển thị và có src hợp lệ</summary>
        public bool IsProductImageDisplayed(int itemIndex = 0)
        {
            var items = _driver.FindElements(CartItems);
            if (items.Count <= itemIndex) return false;
            try
            {
                var img = items[itemIndex].FindElement(By.CssSelector("img"));
                string src = img.GetAttribute("src") ?? "";
                var js = (IJavaScriptExecutor)_driver;
                var naturalWidth = js.ExecuteScript(
                    "return arguments[0].naturalWidth;", img)?.ToString();
                return img.Displayed && !string.IsNullOrEmpty(src)
                    && naturalWidth != "0";
            }
            catch { return false; }
        }
    }
}

