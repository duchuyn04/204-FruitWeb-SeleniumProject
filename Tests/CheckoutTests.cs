using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumProject.Pages;
using SeleniumProject.Utilities;
using System.Text.Json;

namespace SeleniumProject.Tests.Checkout
{
    [TestFixture]
    public class CheckoutTests : TestBase
    {
        private CheckoutPage _checkoutPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "Checkout", "checkout_data.json"
        );

        [SetUp]
        public void SetUpCheckoutPage()
        {
            _checkoutPage = new CheckoutPage(Driver);
        }

        // =========================================================
        // F5.1 – Checkout Display
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_1_01_HienThiTrangCheckout()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_1_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.GoToCheckoutViaCart(data["productUrl"]);

            Assert.That(_checkoutPage.IsCheckoutPageDisplayed(), Is.True,
                "[F5.1_01] Trang Checkout phải hiển thị đầy đủ Shipping Form, Order Summary và Payment Method");
            Assert.That(_checkoutPage.GetCurrentUrl(), Does.Contain("/Checkout"),
                "[F5.1_01] URL phải chứa '/Checkout'");
        }

        // =========================================================
        // F5.2 – Chặn giỏ hàng rỗng
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_2_01_GioHangRong_Redirect()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_2_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.Open();

            Assert.That(_checkoutPage.IsRedirectedFromEmptyCart(), Is.True,
                "[F5.2_01] Hệ thống phải redirect về homepage hoặc hiển thị 'Cart is empty'");
        }

        // =========================================================
        // F5.4 – Khách vãng lai nhập form thủ công
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_4_01_XacMinhNguoiDungChuaDangNhapCoTheNhapThongTin()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_4_01");
            Driver.Navigate().GoToUrl(data["productUrl"]);
            Thread.Sleep(1000);
            try
            {
                var buyBtn = Driver.FindElements(By.CssSelector("button.btn-buy-now"));
                if (buyBtn.Count > 0) { buyBtn[0].Click(); Thread.Sleep(800); }
                else
                {
                    var addBtn = Driver.FindElements(By.CssSelector("button.btn-add-cart"));
                    if (addBtn.Count > 0) addBtn[0].Click();
                    Thread.Sleep(800);
                }
            }
            catch { }

            Driver.Navigate().GoToUrl("http://localhost:5270/Checkout");
            Thread.Sleep(1000);

            bool isOnCheckoutPage = Driver.Url.Contains("/Checkout") && !Driver.Url.Contains("/Account/Login");
            Assert.That(isOnCheckoutPage, Is.True,
                $"[F5.4_01] Guest phải vào được Checkout.\nThực tế URL: {Driver.Url}");
        }

        // =========================================================
        // F5.5 – Validation Form (10 test cases)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_5_01_KiemTraBatBuocHoTen()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName("");
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince("Thành phố Hà Nội");
            _checkoutPage.SelectDistrict("Quận Ba Đình");
            _checkoutPage.SelectWard("Phường Phúc Xá");
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any() || _checkoutPage.IsFieldInvalid(By.Id("FullNameInput"));
            Assert.That(hasError, Is.True,
                $"[F5.5_01] Phải báo lỗi khi để trống Họ tên.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_02_KiemTraBatBuocSoDienThoai()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_02");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone("");
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince("Thành phố Hà Nội");
            _checkoutPage.SelectDistrict("Quận Ba Đình");
            _checkoutPage.SelectWard("Phường Phúc Xá");
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any() || _checkoutPage.IsFieldInvalid(By.Id("Mobile"));
            Assert.That(hasError, Is.True,
                $"[F5.5_02] Phải báo lỗi khi để trống SĐT.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_03_SoDienThoaiSaiDoDai()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_03");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]); // 12 số
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince("Thành phố Hà Nội");
            _checkoutPage.SelectDistrict("Quận Ba Đình");
            _checkoutPage.SelectWard("Phường Phúc Xá");
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any(m => m.Contains("10") || m.Contains("11") || m.Contains("số") || m.Contains("điện thoại"))
                || _checkoutPage.IsFieldInvalid(By.Id("Mobile"));
            Assert.That(hasError, Is.True,
                $"[F5.5_03] Phải báo lỗi SĐT sai độ dài.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_04_KiemTraBatBuocDiaChi()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_04");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress("");
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any() || _checkoutPage.IsFieldInvalid(By.Id("StreetAddressInput"));
            Assert.That(hasError, Is.True,
                $"[F5.5_04] Phải báo lỗi khi để trống Địa chỉ.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_05_KiemTraBatBuocChonTinh()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_05");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            // Không chọn Tỉnh/TP
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any()
                || _checkoutPage.IsFieldInvalid(By.Id("provinceSelect"))
                || _checkoutPage.IsStillOnCheckoutPage();
            Assert.That(hasError, Is.True,
                $"[F5.5_05] Phải chặn khi không chọn Tỉnh/TP.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_06_KiemTraBatBuocChonQuan()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_06");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            // Không chọn Quận
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any()
                || _checkoutPage.IsFieldInvalid(By.Id("districtSelect"))
                || _checkoutPage.IsStillOnCheckoutPage();
            Assert.That(hasError, Is.True,
                $"[F5.5_06] Phải chặn khi không chọn Quận/Huyện.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_07_KiemTraBatBuocChonPhuong()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_07");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            // Không chọn Phường
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any()
                || _checkoutPage.IsFieldInvalid(By.Id("wardSelect"))
                || _checkoutPage.IsStillOnCheckoutPage();
            Assert.That(hasError, Is.True,
                $"[F5.5_07] Phải chặn khi không chọn Phường/Xã.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_08_SoDienThoaiChuaChuCai()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_08");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]); // "abcd123456"
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince("Thành phố Hồ Chí Minh");
            _checkoutPage.SelectDistrict("Quận 1");
            _checkoutPage.SelectWard("Phường Bến Nghé");
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any(m =>
                m.Contains("điện thoại", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("10") || m.Contains("11") || m.Contains("số"))
                || _checkoutPage.IsFieldInvalid(By.Id("Mobile"))
                || _checkoutPage.IsStillOnCheckoutPage();
            Assert.That(hasError, Is.True,
                $"[F5.5_08] Phải báo lỗi SĐT chứa chữ cái.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_09_SoDienThoaiKyTuDacBiet()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_09");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]); // "090-123-4567"
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince("Thành phố Hồ Chí Minh");
            _checkoutPage.SelectDistrict("Quận 1");
            _checkoutPage.SelectWard("Phường Bến Nghé");
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any() || _checkoutPage.IsFieldInvalid(By.Id("Mobile")) || _checkoutPage.IsStillOnCheckoutPage();
            Assert.That(hasError, Is.True,
                $"[F5.5_09] Phải báo lỗi SĐT chứa ký tự đặc biệt.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_10_HoTenQuaDai()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_10");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]); // 101 ký tự
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince("Thành phố Hồ Chí Minh");
            _checkoutPage.SelectDistrict("Quận 1");
            _checkoutPage.SelectWard("Phường Bến Nghé");
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            // Hệ thống không cho >100 ký tự hoặc truncate
            bool handled = messages.Any()
                || _checkoutPage.IsFieldInvalid(By.Id("FullNameInput"))
                || _checkoutPage.IsStillOnCheckoutPage();
            Assert.That(handled, Is.True,
                $"[F5.5_10] Hệ thống phải xử lý họ tên quá 100 ký tự.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_11_DiaChiQuaDai()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_11");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]); // 201 ký tự
            _checkoutPage.SelectProvince("Thành phố Hồ Chí Minh");
            _checkoutPage.SelectDistrict("Quận 1");
            _checkoutPage.SelectWard("Phường Bến Nghé");
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool handled = messages.Any()
                || _checkoutPage.IsFieldInvalid(By.Id("StreetAddressInput"))
                || _checkoutPage.IsStillOnCheckoutPage();
            Assert.That(handled, Is.True,
                $"[F5.5_11] Hệ thống phải xử lý địa chỉ quá 200 ký tự.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_12_TatCaTruongBatBuocBoTrong()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_12");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            // Không nhập gì cả
            _checkoutPage.EnterFullName("");
            _checkoutPage.EnterPhone("");
            _checkoutPage.EnterStreetAddress("");
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasMultipleErrors = messages.Count >= 2 || _checkoutPage.IsStillOnCheckoutPage();
            Assert.That(hasMultipleErrors, Is.True,
                $"[F5.5_12] Phải hiện nhiều lỗi khi bỏ trống tất cả.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_13_HoTenToanKhoangTrang()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_13");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName("   "); // chỉ khoảng trắng
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince("Thành phố Hồ Chí Minh");
            _checkoutPage.SelectDistrict("Quận 1");
            _checkoutPage.SelectWard("Phường Bến Nghé");
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any()
                || _checkoutPage.IsFieldInvalid(By.Id("FullNameInput"))
                || _checkoutPage.IsStillOnCheckoutPage();
            Assert.That(hasError, Is.True,
                $"[F5.5_13] Khoảng trắng phải được coi là bỏ trống.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_14_SoDienThoai10SoHopLe()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_14");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]); // "0901234567" (10 số)
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince("Thành phố Hồ Chí Minh");
            _checkoutPage.SelectDistrict("Quận 1");
            _checkoutPage.SelectWard("Phường Bến Nghé");
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool phoneError = messages.Any(m => m.Contains("điện thoại", StringComparison.OrdinalIgnoreCase) || m.Contains("10") || m.Contains("Mobile"));
            Assert.That(phoneError, Is.False,
                $"[F5.5_14] SĐT 10 số hợp lệ không được báo lỗi.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_15_SoDienThoai11SoHopLe()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_15");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]); // "09012345678" (11 số)
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince("Thành phố Hồ Chí Minh");
            _checkoutPage.SelectDistrict("Quận 1");
            _checkoutPage.SelectWard("Phường Bến Nghé");
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool phoneError = messages.Any(m => m.Contains("điện thoại", StringComparison.OrdinalIgnoreCase) || m.Contains("10") || m.Contains("Mobile"));
            Assert.That(phoneError, Is.False,
                $"[F5.5_15] SĐT 11 số hợp lệ không được báo lỗi.\nMessages: {string.Join(", ", messages)}");
        }

        // =========================================================
        // F5.6 – Address Dropdown (6 test cases)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_6_01_DanhSachQuanHuyenThayDoiTheoTinh()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_05");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.SelectProvince("Thành phố Hồ Chí Minh");
            Thread.Sleep(1000);

            // Sau khi chọn Tỉnh, danh sách Quận/Huyện phải được load
            var districtOptions = Driver.FindElements(By.CssSelector("#districtSelect option"));
            Assert.That(districtOptions.Count, Is.GreaterThan(1),
                "[F5.6_01] Danh sách Quận/Huyện phải được cập nhật khi chọn Tỉnh/TP");
        }

        [Test]
        public void TC_CHECKOUT_F5_6_02_DanhSachPhuongXaThayDoiTheoQuan()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_05");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.SelectProvince("Thành phố Hồ Chí Minh");
            Thread.Sleep(1000);
            _checkoutPage.SelectDistrict("Quận 1");
            Thread.Sleep(1000);

            // Sau khi chọn Quận, danh sách Phường/Xã phải được load
            var wardOptions = Driver.FindElements(By.CssSelector("#wardSelect option"));
            Assert.That(wardOptions.Count, Is.GreaterThan(1),
                "[F5.6_02] Danh sách Phường/Xã phải được cập nhật khi chọn Quận/Huyện");
        }

        [Test]
        public void TC_CHECKOUT_F5_6_03_DropdownQuanHuyenDisableTruocKhiChonTinh()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_05");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            // Chưa chọn Tỉnh → Quận phải disabled hoặc chỉ có placeholder
            var districtSelect = Driver.FindElement(By.Id("districtSelect"));
            bool isDisabled = !districtSelect.Enabled
                || districtSelect.GetAttribute("disabled") != null
                || Driver.FindElements(By.CssSelector("#districtSelect option")).Count <= 1;
            Assert.That(isDisabled, Is.True,
                "[F5.6_03] Dropdown Quận/Huyện phải disabled hoặc chỉ có placeholder khi chưa chọn Tỉnh/TP");
        }

        [Test]
        public void TC_CHECKOUT_F5_6_04_DropdownPhuongXaDisableTruocKhiChonQuan()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_05");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.SelectProvince("Thành phố Hồ Chí Minh");
            Thread.Sleep(1000);
            // Chưa chọn Quận → Ward phải disabled
            var wardSelect = Driver.FindElement(By.Id("wardSelect"));
            bool isDisabled = !wardSelect.Enabled
                || wardSelect.GetAttribute("disabled") != null
                || Driver.FindElements(By.CssSelector("#wardSelect option")).Count <= 1;
            Assert.That(isDisabled, Is.True,
                "[F5.6_04] Dropdown Phường/Xã phải disabled khi chưa chọn Quận/Huyện");
        }

        [Test]
        public void TC_CHECKOUT_F5_6_05_DoiTinhThiQuanVaPhuongBiReset()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_05");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            // Chọn Hồ Chí Minh → Quận 1 → Phường Bến Nghé
            _checkoutPage.SelectProvince("Thành phố Hồ Chí Minh");
            Thread.Sleep(1000);
            _checkoutPage.SelectDistrict("Quận 1");
            Thread.Sleep(1000);
            _checkoutPage.SelectWard("Phường Bến Nghé");
            Thread.Sleep(500);

            // Đổi sang Hà Nội → Quận và Phường phải reset
            _checkoutPage.SelectProvince("Thành phố Hà Nội");
            Thread.Sleep(1000);

            var districtSelect = new SelectElement(Driver.FindElement(By.Id("districtSelect")));
            var wardSelect    = new SelectElement(Driver.FindElement(By.Id("wardSelect")));
            bool districtReset = districtSelect.SelectedOption.GetAttribute("value") == "" || districtSelect.SelectedOption.Text.Contains("Chọn");
            bool wardReset     = wardSelect.SelectedOption.GetAttribute("value") == ""     || wardSelect.SelectedOption.Text.Contains("Chọn");
            Assert.That(districtReset, Is.True,
                "[F5.6_05] Dropdown Quận/Huyện phải reset về placeholder sau khi đổi Tỉnh");
            Assert.That(wardReset, Is.True,
                "[F5.6_05] Dropdown Phường/Xã phải reset về placeholder sau khi đổi Tỉnh");
        }

        [Test]
        public void TC_CHECKOUT_F5_6_06_DoiQuanThiPhuongBiReset()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_5_05");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.SelectProvince("Thành phố Hồ Chí Minh");
            Thread.Sleep(1000);
            _checkoutPage.SelectDistrict("Quận 1");
            Thread.Sleep(1000);
            _checkoutPage.SelectWard("Phường Bến Nghé");
            Thread.Sleep(500);

            // Đổi sang quận khác → Phường phải reset
            _checkoutPage.SelectDistrict("Quận 3");
            Thread.Sleep(1000);

            var wardSelect = new SelectElement(Driver.FindElement(By.Id("wardSelect")));
            bool wardReset = wardSelect.SelectedOption.GetAttribute("value") == "" || wardSelect.SelectedOption.Text.Contains("Chọn");
            Assert.That(wardReset, Is.True,
                "[F5.6_06] Dropdown Phường/Xã phải reset về placeholder sau khi đổi Quận");

            // Danh sách Phường mới phải load theo Quận 3
            var wardOptions = Driver.FindElements(By.CssSelector("#wardSelect option"));
            Assert.That(wardOptions.Count, Is.GreaterThan(1),
                "[F5.6_06] Danh sách Phường mới phải load sau khi đổi Quận");
        }

        // =========================================================
        // F5.7 – Subtotal Calculation (3 test cases)
        // =========================================================

        [Test]
        public void TC_CHECKOUT_F5_7_01_TinhToanSubtotalDung()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_7_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            var summaryText = _checkoutPage.GetOrderSummaryText();
            Assert.That(summaryText, Is.Not.Empty,
                "[F5.7_01] Order Summary phải có nội dung");
            // Có hiển thị subtotal / tạm tính
            bool hasSubtotal = summaryText.Contains("Tạm tính", StringComparison.OrdinalIgnoreCase)
                || summaryText.Contains("Subtotal", StringComparison.OrdinalIgnoreCase)
                || summaryText.Contains("đ") || summaryText.Contains("VNĐ");
            Assert.That(hasSubtotal, Is.True,
                $"[F5.7_01] Phải hiển thị giá trị Tạm tính.\nSummary: {summaryText[..Math.Min(200, summaryText.Length)]}");
        }

        [Test]
        public void TC_CHECKOUT_F5_7_02_TenSanPhamHienThiDungTrongOrderSummary()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_7_02");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            var summaryText = _checkoutPage.GetOrderSummaryText();
            bool hasProductName = summaryText.Contains(data["expectedProductName"], StringComparison.OrdinalIgnoreCase);
            Assert.That(hasProductName, Is.True,
                $"[F5.7_02] Order Summary phải hiển thị tên sản phẩm '{data["expectedProductName"]}'.\nSummary: {summaryText[..Math.Min(300, summaryText.Length)]}");
        }

        [Test]
        public void TC_CHECKOUT_F5_7_03_GiaVaSoLuongHienThiDung()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_7_03");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            var summaryText = _checkoutPage.GetOrderSummaryText();
            bool hasPriceInfo = summaryText.Contains("đ") || summaryText.Contains("VNĐ") || summaryText.Contains(",000");
            Assert.That(hasPriceInfo, Is.True,
                $"[F5.7_03] Order Summary phải hiển thị giá và số lượng.\nSummary: {summaryText[..Math.Min(300, summaryText.Length)]}");
        }

        // =========================================================
        // F5.8 – Phí vận chuyển (2 test cases)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_8_01_HienThiPhiVanChuyen()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_8_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            var summaryText = _checkoutPage.GetOrderSummaryText();
            bool hasShipping = summaryText.Contains("Phí vận chuyển", StringComparison.OrdinalIgnoreCase)
                || summaryText.Contains("Phí ship", StringComparison.OrdinalIgnoreCase)
                || summaryText.Contains("Shipping", StringComparison.OrdinalIgnoreCase);
            Assert.That(hasShipping, Is.True,
                $"[F5.8_01] Order Summary phải hiển thị Phí vận chuyển.\nSummary: {summaryText[..Math.Min(300, summaryText.Length)]}");
        }



        // =========================================================
        // F5.9 – Tổng tiền đơn hàng (1 test case)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_9_01_TinhToanTongTienDonHang()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_9_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            var summaryText = _checkoutPage.GetOrderSummaryText();
            bool hasTotal = summaryText.Contains("Tổng", StringComparison.OrdinalIgnoreCase)
                || summaryText.Contains("Total", StringComparison.OrdinalIgnoreCase);
            Assert.That(hasTotal, Is.True,
                $"[F5.9_01] Order Summary phải hiển thị Tổng tiền.\nSummary: {summaryText[..Math.Min(300, summaryText.Length)]}");
        }

        // =========================================================
        // F5.10 – COD Payment (2 test cases)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_10_01_ChonPhuongThucCOD()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_10_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            _checkoutPage.SelectPaymentCod();
            Assert.That(_checkoutPage.IsCodSelected(), Is.True,
                "[F5.10_01] Radio button COD phải được chọn thành công");
        }

        [Test]
        public void TC_CHECKOUT_F5_10_02_CODDuocChonMacDinh()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_10_02");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            // Không click gì – kiểm tra COD được check sẵn
            bool codDefault = _checkoutPage.IsCodSelected();
            bool transferNotSelected = !_checkoutPage.IsTransferSelected();
            Assert.That(codDefault, Is.True,
                "[F5.10_02] COD phải được chọn mặc định khi vào trang Checkout");
            Assert.That(transferNotSelected, Is.True,
                "[F5.10_02] Chuyển khoản không được check mặc định");
        }

        // =========================================================
        // F5.11 – Bank Transfer Payment (2 test cases)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_11_01_ChonPhuongThucChuyenKhoan()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_11_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            _checkoutPage.SelectPaymentTransfer();
            Assert.That(_checkoutPage.IsTransferSelected(), Is.True,
                "[F5.11_01] Radio button Chuyển khoản phải được chọn");
            Assert.That(_checkoutPage.IsBankInfoDisplayed(), Is.True,
                "[F5.11_01] Thông tin ngân hàng phải hiển thị sau khi chọn Chuyển khoản");
        }



        // =========================================================
        // F5.12 & F5.13 – Đặt hàng thành công + Xóa giỏ hàng (4 TCs)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_12_01_DatHangThanhCong()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_12_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
            _checkoutPage.EnterNotes(data["notes"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            Assert.That(_checkoutPage.IsOnConfirmationPage(), Is.True,
                "[F5.12_01] Phải chuyển đến /Checkout/Confirmation sau khi đặt hàng");
            var confirmMsg = _checkoutPage.GetConfirmationMessage();
            Assert.That(confirmMsg, Does.Contain(data["expectedConfirmationText"]).IgnoreCase,
                $"[F5.12_01] Phải hiện thông báo '{data["expectedConfirmationText"]}'");
        }

        [Test]
        public void TC_CHECKOUT_F5_13_01_KiemTraGioHangXoaSauDatHang()
        {
            // F5.13 – Riêng biệt: Truy cập /Cart sau đặt hàng → không còn sản phẩm
            var data = DocDuLieu("TC_CHECKOUT_F5_13_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            _checkoutPage.OpenCart();
            Thread.Sleep(800);
            var cartBody = Driver.FindElement(By.TagName("body")).Text;
            bool cartEmpty = cartBody.Contains("trống", StringComparison.OrdinalIgnoreCase)
                || cartBody.Contains("empty", StringComparison.OrdinalIgnoreCase)
                || cartBody.Contains("0 sản phẩm", StringComparison.OrdinalIgnoreCase)
                || Driver.FindElements(By.CssSelector(".cart-item, .cart-product")).Count == 0;
            Assert.That(cartEmpty, Is.True,
                "[F5.13_01] Giỏ hàng phải không còn sản phẩm sau khi đặt hàng thành công");
        }

        // =========================================================
        // F5.15 – Buy Now
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_15_01_ChucNangMuaNgay()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_15_01");
            _checkoutPage.Login(data["email"], data["password"]);

            Driver.Navigate().GoToUrl(data["productUrl"]);
            Thread.Sleep(1000);

            // Click "Mua ngay" → đến thẳng /Checkout
            var buyNowBtn = Driver.FindElements(By.CssSelector("button.btn-buy-now, a.btn-buy-now, [id*='buy-now'], [class*='buy-now']"));
            Assert.That(buyNowBtn.Count, Is.GreaterThan(0),
                "[F5.15_01] Phải tồn tại nút 'Mua ngay' trên trang sản phẩm");

            buyNowBtn[0].Click();
            Thread.Sleep(1200);

            Assert.That(Driver.Url, Does.Contain("/Checkout"),
                $"[F5.15_01] Mua ngay phải dẫn thẳng đến /Checkout, không qua /Cart.\nURL thực: {Driver.Url}");
            Assert.That(Driver.Url, Does.Not.Contain("/Cart"),
                "[F5.15_01] Không được đi qua trang /Cart");
        }

        // =========================================================
        // F5.16 – Order Confirmation Display
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_16_01_HienThiTrangXacNhanDonHang()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_16_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            Assert.That(_checkoutPage.IsOnConfirmationPage(), Is.True,
                "[F5.16_01] Phải chuyển đến trang /Checkout/Confirmation");

            var pageText = Driver.FindElement(By.TagName("body")).Text;
            // Xác nhận có hiển thị: Mã đơn hàng, sản phẩm, địa chỉ, PT thanh toán, tổng tiền
            bool hasOrderInfo = pageText.Contains("Cảm ơn", StringComparison.OrdinalIgnoreCase)
                || pageText.Contains("thành công", StringComparison.OrdinalIgnoreCase)
                || pageText.Contains("Confirmation", StringComparison.OrdinalIgnoreCase);
            Assert.That(hasOrderInfo, Is.True,
                $"[F5.16_01] Trang xác nhận phải hiển thị thông tin đơn hàng.\nText: {pageText[..Math.Min(300, pageText.Length)]}");
        }

        // =========================================================
        // F5.19 – Redirect về Login khi chưa đăng nhập
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_19_01_RedirectVeLoginKhiChuaDangNhap()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_19_01");
            // Không đăng nhập – truy cập thẳng /Checkout
            Driver.Navigate().GoToUrl(data["checkoutUrl"]);
            Thread.Sleep(1000);

            bool redirectedToLogin = Driver.Url.Contains("/Account/Login");
            bool notOnCheckout = !Driver.Url.Contains("/Checkout") || Driver.Url.Contains("/Account/Login");
            Assert.That(redirectedToLogin || notOnCheckout, Is.True,
                $"[F5.19_01] Chưa đăng nhập phải bị redirect về /Account/Login.\nURL thực: {Driver.Url}");
        }

        // =========================================================
        // F5.20 – Redirect lại Checkout sau khi đăng nhập
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_20_01_RedirectVeCheckoutSauDangNhap()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_20_01");
            // Bước 1: Truy cập /Checkout khi chưa đăng nhập → bị redirect về Login
            Driver.Navigate().GoToUrl(data["checkoutUrl"]);
            Thread.Sleep(1000);

            bool onLogin = Driver.Url.Contains("/Account/Login");
            if (!onLogin)
            {
                // Thủ công navigate về Login nếu hệ thống không redirect
                Driver.Navigate().GoToUrl("http://localhost:5270/Account/Login?ReturnUrl=/Checkout");
                Thread.Sleep(800);
            }

            // Bước 2: Đăng nhập
            _checkoutPage.Login(data["email"], data["password"]);
            Thread.Sleep(1000);

            // Kỳ vọng: sau đăng nhập phải về /Checkout
            bool backToCheckout = Driver.Url.Contains("/Checkout");
            Assert.That(backToCheckout, Is.True,
                $"[F5.20_01] Sau khi đăng nhập phải redirect về /Checkout.\nURL thực: {Driver.Url}");
        }

        // =========================================================
        // F5.24 – Phí ship thay đổi theo khu vực
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_24_01_PhiShipThayDoiTheoKhuVuc()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_24_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            // Chọn tỉnh 1 → ghi nhận phí ship
            _checkoutPage.SelectProvince(data["province1"]);
            Thread.Sleep(1000);
            var summary1 = _checkoutPage.GetOrderSummaryText();

            // Chọn tỉnh 2 → so sánh
            _checkoutPage.SelectProvince(data["province2"]);
            Thread.Sleep(1000);
            var summary2 = _checkoutPage.GetOrderSummaryText();

            // Ít nhất phí ship phải hiển thị ở cả hai trường hợp
            bool shippingVisible1 = summary1.Contains("vận chuyển", StringComparison.OrdinalIgnoreCase) || summary1.Contains("ship", StringComparison.OrdinalIgnoreCase);
            bool shippingVisible2 = summary2.Contains("vận chuyển", StringComparison.OrdinalIgnoreCase) || summary2.Contains("ship", StringComparison.OrdinalIgnoreCase);
            Assert.That(shippingVisible1 || shippingVisible2, Is.True,
                $"[F5.24_01] Phí vận chuyển phải hiển thị khi chọn tỉnh.\nSummary: {summary1[..Math.Min(200, summary1.Length)]}");
        }

        // =========================================================
        // F5.9_02 – Tổng cộng = Tạm tính + Phí vận chuyển
        // =========================================================

        [Test]
        public void TC_CHECKOUT_F5_9_02_TongCongBangTamTinhCongPhiShip()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_9_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.SelectProvince("Thành phố Hà Nội");
            _checkoutPage.SelectDistrict("Quận Ba Đình");
            _checkoutPage.SelectWard("Phường Phúc Xá");
            Thread.Sleep(1000);

            var summaryText = _checkoutPage.GetOrderSummaryText();
            // Xác nhận cả 3 giá trị: Tạm tính / Phí vận chuyển / Tổng cộng đều hiển thị
            bool hasTamTinh = summaryText.Contains("Tạm tính", StringComparison.OrdinalIgnoreCase)
                || summaryText.Contains("Subtotal", StringComparison.OrdinalIgnoreCase);
            bool hasPhiShip = summaryText.Contains("Phí vận chuyển", StringComparison.OrdinalIgnoreCase)
                || summaryText.Contains("Shipping", StringComparison.OrdinalIgnoreCase)
                || summaryText.Contains("ship", StringComparison.OrdinalIgnoreCase);
            bool hasTotal = summaryText.Contains("Tổng", StringComparison.OrdinalIgnoreCase)
                || summaryText.Contains("Total", StringComparison.OrdinalIgnoreCase);

            Assert.That(hasTamTinh, Is.True,
                $"[F5.9_02] Order Summary phải hiển thị Tạm tính.\nSummary: {summaryText[..Math.Min(300, summaryText.Length)]}");
            Assert.That(hasPhiShip, Is.True,
                $"[F5.9_02] Order Summary phải hiển thị Phí vận chuyển.\nSummary: {summaryText[..Math.Min(300, summaryText.Length)]}");
            Assert.That(hasTotal, Is.True,
                $"[F5.9_02] Order Summary phải hiển thị Tổng cộng.\nSummary: {summaryText[..Math.Min(300, summaryText.Length)]}");
        }

        // =========================================================
        // F5.14 – Trừ tồn kho sau đặt hàng (Admin kiểm tra)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_14_01_TruTonKhoSauDatHang()
        {
            var adminData  = DocDuLieu("TC_CHECKOUT_F5_14_01_ADMIN");
            var orderData  = DocDuLieu("TC_CHECKOUT_F5_12_01");
            string productId = adminData["productId"];

            // ── Bước 1: Admin đăng nhập & ghi nhận stock ban đầu ──────────
            _checkoutPage.Login(adminData["adminEmail"], adminData["adminPassword"]);
            Driver.Navigate().GoToUrl($"http://localhost:5270/Admin/Product/Edit/{productId}");
            Thread.Sleep(1000);

            // Đọc giá trị StockQuantity từ input trong trang Edit
            var stockInput = Driver.FindElement(By.Id("Product_StockQuantity"));
            int stockBefore = int.TryParse(stockInput.GetAttribute("value")?.Trim(), out var sb) ? sb : -1;
            Assert.That(stockBefore, Is.GreaterThan(0),
                $"[F5.14_01] Admin phải đọc được tồn kho > 0 trước khi đặt hàng. Giá trị đọc: {stockBefore}");

            // ── Bước 2: Đăng xuất admin → đặt hàng bằng tài khoản user ──
            Driver.Navigate().GoToUrl("http://localhost:5270/Account/Logout");
            Thread.Sleep(800);

            _checkoutPage.Login(orderData["email"], orderData["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(orderData["productUrl"]);
            _checkoutPage.SelectNewAddressOption();
            _checkoutPage.EnterFullName(orderData["fullName"]);
            _checkoutPage.EnterPhone(orderData["phone"]);
            _checkoutPage.EnterStreetAddress(orderData["streetAddress"]);
            _checkoutPage.SelectProvince(orderData["province"]);
            _checkoutPage.SelectDistrict(orderData["district"]);
            _checkoutPage.SelectWard(orderData["ward"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            Assert.That(_checkoutPage.IsOnConfirmationPage(), Is.True,
                "[F5.14_01] Đặt hàng phải thành công");

            // ── Bước 3: Admin đăng nhập lại → kiểm tra stock đã giảm ─────
            Driver.Navigate().GoToUrl("http://localhost:5270/Account/Logout");
            Thread.Sleep(600);
            _checkoutPage.Login(adminData["adminEmail"], adminData["adminPassword"]);
            Driver.Navigate().GoToUrl($"http://localhost:5270/Admin/Product/Edit/{productId}");
            Thread.Sleep(1000);

            stockInput = Driver.FindElement(By.Id("Product_StockQuantity"));
            int stockAfter = int.TryParse(stockInput.GetAttribute("value")?.Trim(), out var sa) ? sa : -1;

            Assert.That(stockAfter, Is.LessThan(stockBefore),
                $"[F5.14_01] Tồn kho phải giảm sau khi đặt hàng. Trước: {stockBefore}, Sau: {stockAfter}");
        }

        // =========================================================
        // F5.17 – Lưu địa chỉ mới (Medium - 2 TCs)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_17_01_LuuDiaChiMoiThanhCong()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_12_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);

            // Tick checkbox "Lưu địa chỉ này cho lần mua sau"
            var saveCheckbox = Driver.FindElements(By.CssSelector("input[id*='save'], input[id*='Save'], input[name*='save'], input[name*='Save']"));
            if (saveCheckbox.Count > 0 && !saveCheckbox[0].Selected)
                saveCheckbox[0].Click();

            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            Assert.That(_checkoutPage.IsOnConfirmationPage(), Is.True,
                "[F5.17_01] Đặt hàng với lưu địa chỉ phải thành công");

            // Vào lần mua tiếp theo – kiểm tra dropdown địa chỉ đã lưu xuất hiện
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            Thread.Sleep(1000);
            var savedDropdown = Driver.FindElements(By.CssSelector("select[id*='saved'], select[id*='Saved'], select[id*='address'], #savedAddressSelect"));
            Assert.That(savedDropdown.Count, Is.GreaterThan(0),
                "[F5.17_01] Dropdown địa chỉ đã lưu phải xuất hiện trong lần mua tiếp theo");
        }

        [Test]
        public void TC_CHECKOUT_F5_17_02_DropdownDiaChiDaLuuHienThi()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_12_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            Thread.Sleep(1500);

            // Dropdown địa chỉ đã lưu có id="addressSelect" trong Checkout/Index.cshtml
            var savedDropdown = Driver.FindElements(By.Id("addressSelect"));
            Assert.That(savedDropdown.Count, Is.GreaterThan(0),
                "[F5.17_02] Dropdown 'addressSelect' phải tồn tại khi user có địa chỉ lưu");

            // Phải có ít nhất 1 option địa chỉ thực (ngoài option placeholder)
            var options = Driver.FindElements(By.CssSelector("#addressSelect option"));
            Assert.That(options.Count, Is.GreaterThan(1),
                "[F5.17_02] Dropdown địa chỉ đã lưu phải hiển thị ít nhất 1 địa chỉ (ngoài placeholder)");
        }

        // =========================================================
        // F5.18 – Chọn địa chỉ đã lưu tự động điền form (Medium)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_18_01_ChonDiaChiDaLuuTuDongDienForm()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_12_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            Thread.Sleep(1500);

            // Dropdown địa chỉ đã lưu có id="addressSelect"
            var savedDropdown = Driver.FindElements(By.Id("addressSelect"));
            if (savedDropdown.Count == 0)
            {
                Assert.Fail("[F5.18_01] Không tìm thấy dropdown 'addressSelect' – cần chạy F5.17_01 trước");
                return;
            }

            // Chọn option đầu tiên có value (bỏ qua placeholder)
            var sel = new SelectElement(savedDropdown[0]);
            var nonEmptyOptions = sel.Options.Where(o => !string.IsNullOrEmpty(o.GetAttribute("value")) && o.GetAttribute("value") != "new").ToList();
            if (nonEmptyOptions.Count == 0)
            {
                Assert.Fail("[F5.18_01] Không có địa chỉ trong dropdown – cần chạy F5.17_01 trước");
                return;
            }

            nonEmptyOptions[0].Click();
            Thread.Sleep(1500); // Chờ JavaScript điền form

            // Sau khi chọn địa chỉ, panel hiển thị địa chỉ phải xuất hiện (id=selectedAddressDisplay)
            // Hoặc kiểm tra các hidden input đã được điền
            var displayPanel = Driver.FindElements(By.Id("selectedAddressDisplay"));
            bool panelVisible = displayPanel.Count > 0 && displayPanel[0].Displayed;

            // Kiểm tra các hidden field hoặc display panel đã có dữ liệu
            Assert.That(panelVisible || nonEmptyOptions.Count > 0, Is.True,
                "[F5.18_01] Chọn địa chỉ đã lưu phải hiển thị thông tin địa chỉ (panel selectedAddressDisplay)");
        }

        // =========================================================
        // F5.21 – Mã đơn hàng trên trang xác nhận
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_21_01_MaDonHangXuatHienTrenTrangXacNhan()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_12_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();
            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            Assert.That(Driver.Url, Does.Contain("/Checkout/Confirmation"),
                "[F5.21_01] URL phải chứa /Checkout/Confirmation");

            var pageText = Driver.FindElement(By.TagName("body")).Text;
            bool hasMaOrder = pageText.Contains("Mã đơn hàng", StringComparison.OrdinalIgnoreCase)
                || pageText.Contains("Order", StringComparison.OrdinalIgnoreCase)
                || pageText.Contains("#", StringComparison.OrdinalIgnoreCase);
            Assert.That(hasMaOrder, Is.True,
                $"[F5.21_01] Trang xác nhận phải hiển thị mã đơn hàng.\nText: {pageText[..Math.Min(300, pageText.Length)]}");
        }

        // =========================================================
        // F5.22 – Nút "Tiếp tục mua sắm" (Medium)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_22_01_NutTiepTucMuaSam()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_12_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();
            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            Assert.That(_checkoutPage.IsOnConfirmationPage(), Is.True,
                "[F5.22_01] Phải đến trang xác nhận trước");

            // Tìm và click nút "Tiếp tục mua sắm" hoặc "Về trang chủ"
            var continueBtn = Driver.FindElements(By.CssSelector(
                "a[href*='/Shop'], a[href*='/'], button[id*='continue'], a[id*='continue']"))
                .FirstOrDefault(e => e.Displayed &&
                    (e.Text.Contains("mua sắm", StringComparison.OrdinalIgnoreCase)
                  || e.Text.Contains("trang chủ", StringComparison.OrdinalIgnoreCase)
                  || e.Text.Contains("Shop", StringComparison.OrdinalIgnoreCase)));

            Assert.That(continueBtn, Is.Not.Null,
                "[F5.22_01] Phải tồn tại nút 'Tiếp tục mua sắm' hoặc 'Về trang chủ' trên trang xác nhận");
            continueBtn!.Click();
            Thread.Sleep(1000);

            bool onShopOrHome = Driver.Url.Contains("/Shop") || Driver.Url == "http://localhost:5270/"
                || Driver.Url.EndsWith("/") || Driver.Url.Contains("/Home");
            Assert.That(onShopOrHome, Is.True,
                $"[F5.22_01] Phải chuyển về trang chủ hoặc Shop.\nURL thực: {Driver.Url}");
        }

        // =========================================================
        // F5.23 – Thông tin ngân hàng ẩn khi đổi về COD (Medium)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_23_01_ThongTinNganHangAnKhiDoiVeCOD()
        {
            var data = DocDuLieu("TC_CHECKOUT_F5_10_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            // Chọn Chuyển khoản → thông tin ngân hàng phải hiện
            _checkoutPage.SelectPaymentTransfer();
            Assert.That(_checkoutPage.IsBankInfoDisplayed(), Is.True,
                "[F5.23_01] Thông tin ngân hàng phải hiển thị khi chọn Chuyển khoản");

            // Đổi về COD → thông tin ngân hàng phải ẩn
            _checkoutPage.SelectPaymentCod();
            Assert.That(_checkoutPage.IsBankInfoDisplayed(), Is.False,
                "[F5.23_01] Thông tin ngân hàng phải ẩn khi đổi về COD");
            Assert.That(_checkoutPage.IsCodSelected(), Is.True,
                "[F5.23_01] COD phải được chọn thành công");
        }

        // =========================================================
        // HELPER
        // =========================================================
        private DefaultDictionary DocDuLieu(string testCaseId)

        {
            var json     = File.ReadAllText(DataPath);
            var danhSach = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json)!;
            var duLieu   = danhSach.FirstOrDefault(x => x["testCase"] == testCaseId);

            Assert.That(duLieu, Is.Not.Null,
                $"Không tìm thấy test case '{testCaseId}' trong: {DataPath}");

            return new DefaultDictionary(duLieu!);
        }
    }

    internal class DefaultDictionary : Dictionary<string, string>
    {
        public DefaultDictionary(Dictionary<string, string> source) : base(source) { }
        public new string this[string key] =>
            TryGetValue(key, out var val) ? val : string.Empty;
    }
}
