using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumProject.Utilities;

namespace SeleniumProject.Pages.ProfileManagement
{
    public class ProfilePage
    {
        private readonly IWebDriver _driver;
        private readonly WaitHelper _wait;

        // URLs
        private const string BaseUrl = "http://localhost:5270";
        private const string ProfileUrl = BaseUrl + "/Profile";
        private const string EditUrl = BaseUrl + "/Profile/Edit";
        private const string LoginUrl = BaseUrl + "/Account/Login";
        private const string AddressUrl = BaseUrl + "/Address";
        private const string AddressCreateUrl = BaseUrl + "/Address/Create";

        // ===== LOCATORS – Login =====
        private By EmailInput => By.Id("Email");
        private By PasswordInput => By.Id("Password");
        private By LoginButton => By.CssSelector("button.btn-auth-primary");

        // ===== LOCATORS – Profile Index =====
        private By ProfileName => By.XPath("//label[contains(text(),'Họ và tên')]/following-sibling::p");
        private By ProfileEmail => By.XPath("//label[contains(text(),'Email')]/following-sibling::p");
        private By ProfilePhone => By.XPath("//label[contains(text(),'Số điện thoại')]/following-sibling::p");
        private By ProfileAvatar => By.CssSelector(".rounded-circle.img-thumbnail");
        private By ProfileDisplayName => By.CssSelector(".col-md-4 h5");
        private By ProfileMemberSince => By.CssSelector(".col-md-4 .text-muted.small");
        private By EditButton => By.CssSelector("a[href*='/Profile/Edit']");
        private By AddressLink => By.CssSelector("a[href*='/Address']");
        private By OrderHistoryLink => By.CssSelector("a[href*='/OrderHistory']");

        // ===== LOCATORS – Profile Edit =====
        private By NameInput => By.Id("Name");
        private By PhoneInput => By.Id("Phone");
        private By EmailDisabledInput => By.CssSelector("input[type='email'][disabled]");
        private By SaveButton => By.XPath("//button[@type='submit' and contains(.,'Lưu thay đổi')]");
        private By BackButton => By.CssSelector("a[href*='/Profile']");

        // Validation errors
        private By NameValidation => By.CssSelector("span[data-valmsg-for='Name']");
        private By PhoneValidation => By.CssSelector("span[data-valmsg-for='Phone']");

        // ===== LOCATORS – Avatar =====
        private By AvatarPreview => By.Id("avatarPreview");
        private By AvatarInput => By.Id("avatarInput");
        private By ChangeAvatarButton => By.XPath("//button[contains(.,'Đổi ảnh')]");
        private By DeleteAvatarButton => By.XPath("//button[contains(.,'Xóa ảnh')]");

        // ===== LOCATORS – Toast =====
        private By ToastSuccess => By.CssSelector(".toast.show .toast-header.bg-success, .toast.show .toast-header.bg-danger");
        private By ToastBody => By.CssSelector(".toast.show .toast-body");

        // ===== LOCATORS – Address =====
        private By AddressItems => By.CssSelector(".address-item, .card");
        private By AddAddressButton => By.CssSelector("a[href*='/Address/Create']");
        private By AddressFullName => By.Id("FullName");
        private By AddressPhone => By.Id("Phone");
        private By AddressProvince => By.Id("provinceSelect");
        private By AddressDistrict => By.Id("districtSelect");
        private By AddressWard => By.Id("wardSelect");
        private By AddressStreet => By.Id("StreetAddress");
        private By AddressIsDefault => By.Id("IsDefault");
        private By AddressSubmitButton => By.CssSelector("button[type='submit']");
        private By DefaultBadge => By.XPath("//*[contains(text(),'Mặc định')]");
        private By DeleteAddressButton => By.XPath("//button[contains(.,'Xóa')] | //form[contains(@action,'Delete')]//button");
        private By SetDefaultButton => By.XPath("//button[contains(.,'Đặt làm mặc định')] | //a[contains(.,'Đặt làm mặc định')]");

        // =====================================================================
        public ProfilePage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WaitHelper(driver);
        }

        // =====================================================================
        // AUTHENTICATION
        // =====================================================================

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

        public void OpenProfile()
        {
            _driver.Navigate().GoToUrl(ProfileUrl);
            Thread.Sleep(800);
        }

        public void OpenEdit()
        {
            _driver.Navigate().GoToUrl(EditUrl);
            Thread.Sleep(800);
        }

        public void OpenAddress()
        {
            _driver.Navigate().GoToUrl(AddressUrl);
            Thread.Sleep(800);
        }

        public void OpenAddressCreate()
        {
            _driver.Navigate().GoToUrl(AddressCreateUrl);
            Thread.Sleep(800);
        }

        public void OpenAddressEdit(int addressId)
        {
            _driver.Navigate().GoToUrl($"{BaseUrl}/Address/Edit/{addressId}");
            Thread.Sleep(800);
        }

        // =====================================================================
        // PROFILE INDEX READS
        // =====================================================================

        public string GetProfileName()
        {
            try { return _driver.FindElement(ProfileName).Text.Trim(); }
            catch { return ""; }
        }

        public string GetProfileEmail()
        {
            try { return _driver.FindElement(ProfileEmail).Text.Trim(); }
            catch { return ""; }
        }

        public string GetProfilePhone()
        {
            try { return _driver.FindElement(ProfilePhone).Text.Trim(); }
            catch { return ""; }
        }

        public string GetAvatarSrc()
        {
            try { return _driver.FindElement(ProfileAvatar).GetAttribute("src") ?? ""; }
            catch { return ""; }
        }

        public string GetDisplayName()
        {
            try { return _driver.FindElement(ProfileDisplayName).Text.Trim(); }
            catch { return ""; }
        }

        public string GetMemberSinceText()
        {
            try { return _driver.FindElement(ProfileMemberSince).Text.Trim(); }
            catch { return ""; }
        }

        public bool IsOnProfilePage()
        {
            return _driver.Url.Contains("/Profile") && !_driver.Url.Contains("/Edit");
        }

        public bool IsOnEditPage()
        {
            return _driver.Url.Contains("/Profile/Edit");
        }

        public bool IsOnLoginPage()
        {
            return _driver.Url.Contains("/Account/Login");
        }

        // =====================================================================
        // PROFILE EDIT ACTIONS
        // =====================================================================

        public void ClearAndTypeName(string name)
        {
            var el = _wait.WaitForVisible(NameInput);
            el.Clear();
            if (!string.IsNullOrEmpty(name))
                el.SendKeys(name);
        }

        public void ClearAndTypePhone(string phone)
        {
            var el = _wait.WaitForVisible(PhoneInput);
            el.Clear();
            if (!string.IsNullOrEmpty(phone))
                el.SendKeys(phone);
        }

        public void ClickSave()
        {
            var btn = _wait.WaitForClickable(SaveButton);
            ScrollAndClick(btn);
            Thread.Sleep(1500);
        }

        public void ClickBack()
        {
            var btn = _driver.FindElement(By.CssSelector("a.btn-outline-secondary[href*='/Profile']"));
            ScrollAndClick(btn);
            Thread.Sleep(800);
        }

        public string GetNameInputValue()
        {
            try { return _driver.FindElement(NameInput).GetAttribute("value") ?? ""; }
            catch { return ""; }
        }

        public string GetPhoneInputValue()
        {
            try { return _driver.FindElement(PhoneInput).GetAttribute("value") ?? ""; }
            catch { return ""; }
        }

        public bool IsEmailDisabled()
        {
            try
            {
                var els = _driver.FindElements(EmailDisabledInput);
                return els.Count > 0;
            }
            catch { return false; }
        }

        public string GetNameValidationError()
        {
            try
            {
                var el = _driver.FindElement(NameValidation);
                return el.Displayed ? el.Text.Trim() : "";
            }
            catch { return ""; }
        }

        public string GetPhoneValidationError()
        {
            try
            {
                var el = _driver.FindElement(PhoneValidation);
                return el.Displayed ? el.Text.Trim() : "";
            }
            catch { return ""; }
        }

        public bool HasAnyValidationError()
        {
            var nameErr = GetNameValidationError();
            var phoneErr = GetPhoneValidationError();
            return !string.IsNullOrEmpty(nameErr) || !string.IsNullOrEmpty(phoneErr);
        }

        // =====================================================================
        // AVATAR ACTIONS
        // =====================================================================

        public void UploadAvatar(string filePath)
        {
            try
            {
                var input = _driver.FindElement(AvatarInput);
                input.SendKeys(filePath);
                Thread.Sleep(2000); // Auto submit
            }
            catch { }
        }

        public void ClickDeleteAvatar()
        {
            try
            {
                var btn = _wait.WaitForClickable(DeleteAvatarButton);
                // Accept confirm dialog
                ((IJavaScriptExecutor)_driver).ExecuteScript(
                    "window.confirm = function() { return true; };");
                ScrollAndClick(btn);
                Thread.Sleep(1500);
            }
            catch { }
        }

        public string GetAvatarPreviewSrc()
        {
            try { return _driver.FindElement(AvatarPreview).GetAttribute("src") ?? ""; }
            catch { return ""; }
        }

        public bool IsDeleteAvatarButtonVisible()
        {
            try
            {
                var els = _driver.FindElements(DeleteAvatarButton);
                return els.Count > 0 && els[0].Displayed;
            }
            catch { return false; }
        }

        // =====================================================================
        // ADDRESS READS & ACTIONS
        // =====================================================================

        public int GetAddressCount()
        {
            try
            {
                // Tìm các card chứa thông tin địa chỉ (không tính header/title)
                var items = _driver.FindElements(By.CssSelector(".card .card-body"));
                // Loại bỏ card không phải địa chỉ (VD: card trống, card header)
                return items.Count(el =>
                {
                    string text = el.Text;
                    return text.Contains("Sửa") || text.Contains("Xóa") || text.Contains("Mặc định");
                });
            }
            catch { return 0; }
        }

        public bool HasDefaultBadge()
        {
            try
            {
                var badges = _driver.FindElements(DefaultBadge);
                return badges.Count > 0 && badges.Any(b => b.Displayed);
            }
            catch { return false; }
        }

        public void FillAddressForm(string fullName, string phone, string street,
            string province = "", string district = "", string ward = "", bool isDefault = false)
        {
            var js = (IJavaScriptExecutor)_driver;

            // Mỗi trường try-catch riêng để 1 lỗi không phá hỏng tất cả
            if (!string.IsNullOrEmpty(fullName))
            {
                try
                {
                    var el = _wait.WaitForVisible(AddressFullName);
                    el.Clear();
                    el.SendKeys(fullName);
                }
                catch { /* FullName fill failed */ }
            }
            if (!string.IsNullOrEmpty(phone))
            {
                try
                {
                    var el = _wait.WaitForVisible(AddressPhone);
                    el.Clear();
                    el.SendKeys(phone);
                }
                catch { /* Phone fill failed */ }
            }
            if (!string.IsNullOrEmpty(street))
            {
                try
                {
                    var el = _wait.WaitForVisible(AddressStreet);
                    el.Clear();
                    el.SendKeys(street);
                }
                catch { /* Street fill failed */ }
            }

            // Dropdown Province → District → Ward dùng custom search UI
            // nên phải select bằng JS thay vì Selenium SelectElement
            if (!string.IsNullOrEmpty(province))
            {
                try
                {
                    // Chờ dropdown Province hết disabled (API load xong)
                    WaitForDropdownEnabled(AddressProvince, 15);
                    Thread.Sleep(1000);
                    // Dùng JS tìm option chứa text province và set value + dispatch change
                    SelectDropdownByTextJS("provinceSelect", province);
                    Thread.Sleep(3000); // Chờ District load từ API (cascading)
                }
                catch { /* Province selection failed */ }
            }
            if (!string.IsNullOrEmpty(district))
            {
                try
                {
                    WaitForDropdownEnabled(AddressDistrict, 15);
                    Thread.Sleep(1000);
                    SelectDropdownByTextJS("districtSelect", district);
                    Thread.Sleep(3000); // Chờ Ward load từ API (cascading)
                }
                catch { /* District selection failed */ }
            }
            if (!string.IsNullOrEmpty(ward))
            {
                try
                {
                    WaitForDropdownEnabled(AddressWard, 15);
                    Thread.Sleep(1000);
                    SelectDropdownByTextJS("wardSelect", ward);
                    Thread.Sleep(500);
                }
                catch { /* Ward selection failed */ }
            }
            if (isDefault)
            {
                try
                {
                    var checkbox = _driver.FindElement(AddressIsDefault);
                    if (!checkbox.Selected)
                        js.ExecuteScript("arguments[0].click();", checkbox);
                }
                catch { /* IsDefault checkbox click failed */ }
            }
        }

        /// <summary>
        /// Chọn option trong dropdown bằng JavaScript, tìm theo text (chứa keyword).
        /// Form dùng custom search UI nên Selenium SelectElement không hoạt động.
        /// Cách này set value trực tiếp và dispatch 'change' event để trigger cascading loads.
        /// </summary>
        private void SelectDropdownByTextJS(string selectId, string text)
        {
            var js = (IJavaScriptExecutor)_driver;
            var result = js.ExecuteScript(@"
                var sel = document.getElementById(arguments[0]);
                if (!sel) return 'NOT_FOUND';
                var keyword = arguments[1].toLowerCase();
                var matched = false;
                for (var i = 0; i < sel.options.length; i++) {
                    var optText = sel.options[i].text.toLowerCase();
                    if (optText.indexOf(keyword) !== -1 && sel.options[i].value !== '') {
                        sel.value = sel.options[i].value;
                        sel.dispatchEvent(new Event('change', { bubbles: true }));
                        matched = true;
                        break;
                    }
                }
                return matched ? 'OK' : 'NO_MATCH';
            ", selectId, text);
        }

        public void SubmitAddressForm()
        {
            try
            {
                var btn = _driver.FindElement(AddressSubmitButton);
                ScrollAndClick(btn);
                Thread.Sleep(2000);
            }
            catch
            {
                // Fallback: click bằng JS
                try
                {
                    var btn = _driver.FindElement(By.CssSelector("button.btn-primary[type='submit']"));
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
                    Thread.Sleep(2000);
                }
                catch { }
            }
        }

        /// <summary>Chờ dropdown hết disabled và có options</summary>
        private void WaitForDropdownEnabled(By locator, int timeoutSeconds)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(d =>
            {
                try
                {
                    var el = d.FindElement(locator);
                    bool notDisabled = el.GetAttribute("disabled") == null;
                    var sel = new SelectElement(el);
                    bool hasOptions = sel.Options.Count > 1;
                    return notDisabled && hasOptions;
                }
                catch { return false; }
            });
        }

        // =====================================================================
        // TOAST & STATUS
        // =====================================================================

        public string GetToastText()
        {
            try
            {
                Thread.Sleep(500);
                var els = _driver.FindElements(ToastBody);
                return els.Count > 0 ? els.Last().Text.Trim() : "";
            }
            catch { return ""; }
        }

        public string GetBodyText()
        {
            try { return _driver.FindElement(By.TagName("body")).Text; }
            catch { return ""; }
        }

        public string GetCurrentUrl() => _driver.Url;

        public string GetPageSource()
        {
            try { return _driver.PageSource; }
            catch { return ""; }
        }

        // =====================================================================
        // AVATAR EXTENDED
        // =====================================================================

        public bool IsAvatarImageLoaded()
        {
            try
            {
                var img = _driver.FindElement(ProfileAvatar);
                var js = (IJavaScriptExecutor)_driver;
                var naturalWidth = js.ExecuteScript("return arguments[0].naturalWidth;", img)?.ToString();
                return img.Displayed && naturalWidth != "0";
            }
            catch { return false; }
        }

        // =====================================================================
        // ANTI-FORGERY TOKEN
        // =====================================================================

        public bool HasAntiForgeryToken()
        {
            try
            {
                var tokens = _driver.FindElements(
                    By.CssSelector("input[name='__RequestVerificationToken']"));
                return tokens.Count > 0;
            }
            catch { return false; }
        }

        // =====================================================================
        // ADDRESS EXTENDED
        // =====================================================================

        public string GetAddressFullNameValue()
        {
            try { return _driver.FindElement(AddressFullName).GetAttribute("value") ?? ""; }
            catch { return ""; }
        }

        public string GetAddressPhoneValue()
        {
            try { return _driver.FindElement(AddressPhone).GetAttribute("value") ?? ""; }
            catch { return ""; }
        }

        public void ClearAndTypeStreet(string street)
        {
            try
            {
                var el = _wait.WaitForVisible(AddressStreet);
                el.Clear();
                if (!string.IsNullOrEmpty(street))
                    el.SendKeys(street);
            }
            catch { }
        }

        public void ClickEditFirstAddress()
        {
            try
            {
                var btn = _driver.FindElement(
                    By.XPath("(//a[contains(@href,'/Address/Edit')])[1]"));
                ScrollAndClick(btn);
            }
            catch { }
        }

        public void ClickDeleteFirstAddress()
        {
            try
            {
                // Accept confirm dialog trước
                ((IJavaScriptExecutor)_driver).ExecuteScript(
                    "window.confirm = function() { return true; };");
                var btn = _driver.FindElement(DeleteAddressButton);
                ScrollAndClick(btn);
                Thread.Sleep(1500);
            }
            catch { }
        }

        public bool ClickSetDefaultButton()
        {
            try
            {
                var btns = _driver.FindElements(SetDefaultButton);
                if (btns.Count > 0)
                {
                    ScrollAndClick(btns[0]);
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        public int GetFirstAddressId()
        {
            try
            {
                // Tìm data-address-id hoặc link Edit có ID
                var editLink = _driver.FindElement(
                    By.XPath("(//a[contains(@href,'/Address/Edit/')])[1]"));
                string href = editLink.GetAttribute("href") ?? "";
                // Extract ID từ /Address/Edit/123
                var parts = href.Split('/');
                var idStr = parts.LastOrDefault() ?? "";
                return int.TryParse(idStr, out var id) ? id : 0;
            }
            catch
            {
                // Fallback: tìm data attribute
                try
                {
                    var card = _driver.FindElement(By.CssSelector("[data-address-id]"));
                    var idStr = card.GetAttribute("data-address-id") ?? "";
                    return int.TryParse(idStr, out var id) ? id : 0;
                }
                catch { return 0; }
            }
        }

        // =====================================================================
        // AJAX CALLS
        // =====================================================================

        public string CallDeleteAddressAjax(int addressId)
        {
            var js = (IJavaScriptExecutor)_driver;
            try
            {
                var result = js.ExecuteAsyncScript(@"
                    var callback = arguments[arguments.length - 1];
                    fetch('/Address/DeleteAjax', {
                        method: 'POST',
                        headers: {'Content-Type': 'application/json',
                                  'X-Requested-With': 'XMLHttpRequest'},
                        body: JSON.stringify({ id: arguments[0] })
                    })
                    .then(r => r.text())
                    .then(t => callback(t))
                    .catch(e => callback('ERROR:' + e.message));
                ", addressId);
                return result?.ToString() ?? "";
            }
            catch (Exception ex) { return $"ERROR:{ex.Message}"; }
        }

        public string CallSetDefaultAddressAjax(int addressId)
        {
            var js = (IJavaScriptExecutor)_driver;
            try
            {
                var result = js.ExecuteAsyncScript(@"
                    var callback = arguments[arguments.length - 1];
                    fetch('/Address/SetDefaultAjax', {
                        method: 'POST',
                        headers: {'Content-Type': 'application/json',
                                  'X-Requested-With': 'XMLHttpRequest'},
                        body: JSON.stringify({ id: arguments[0] })
                    })
                    .then(r => r.text())
                    .then(t => callback(t))
                    .catch(e => callback('ERROR:' + e.message));
                ", addressId);
                return result?.ToString() ?? "";
            }
            catch (Exception ex) { return $"ERROR:{ex.Message}"; }
        }

        // =====================================================================
        // HELPERS
        // =====================================================================

        private void ScrollAndClick(IWebElement element)
        {
            var js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript(
                "var els = document.querySelectorAll('.back-to-top, footer, .fixed-top');" +
                "els.forEach(e => e.style.display = 'none');" +
                "arguments[0].scrollIntoView({block: 'center'});", element);
            Thread.Sleep(400);
            try { element.Click(); }
            catch (ElementClickInterceptedException)
            {
                js.ExecuteScript("arguments[0].click();", element);
            }
        }
    }
}
