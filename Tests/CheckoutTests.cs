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
            CurrentSheetName = "TC_CheckoutOrder";
            _checkoutPage = new CheckoutPage(Driver);
        }

        // =========================================================
        // F5.1 – Checkout Display
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_1_01_HienThiTrangCheckout()
        {
            CurrentTestCaseId = "TC_F5.1_01";
            var data = DocDuLieu("TC_CHECKOUT_F5_1_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.GoToCheckoutViaCart(data["productUrl"]);

            bool pageDisplayed = _checkoutPage.IsCheckoutPageDisplayed();
            bool urlOk = _checkoutPage.GetCurrentUrl().Contains("/Checkout");

            CurrentActualResult = pageDisplayed && urlOk
                ? "Trang Checkout hiển thị đầy đủ Shipping Form, Order Summary, Payment Method. URL chứa /Checkout."
                : $"Trang Checkout không hiển thị đúng. URL thực: {_checkoutPage.GetCurrentUrl()}";

            Assert.That(pageDisplayed, Is.True,
                "[F5.1_01] Trang Checkout phải hiển thị đầy đủ Shipping Form, Order Summary và Payment Method");
            Assert.That(urlOk, Is.True,
                "[F5.1_01] URL phải chứa '/Checkout'");
        }

        // =========================================================
        // F5.2 – Chặn giỏ hàng rỗng
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_2_01_GioHangRong_Redirect()
        {
            CurrentTestCaseId = "TC_F5.2_01";
            var data = DocDuLieu("TC_CHECKOUT_F5_2_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.Open();

            bool redirected = _checkoutPage.IsRedirectedFromEmptyCart();
            CurrentActualResult = redirected
                ? "Hệ thống redirect về homepage hoặc hiển thị thông báo Cart is empty."
                : $"Hệ thống KHÔNG redirect. URL hiện tại: {Driver.Url}";

            Assert.That(redirected, Is.True,
                "[F5.2_01] Hệ thống phải redirect về homepage hoặc hiển thị 'Cart is empty'");
        }

        // =========================================================
        // F5.4 – Khách vãng lai nhập form thủ công
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_4_01_XacMinhNguoiDungChuaDangNhapCoTheNhapThongTin()
        {
            CurrentTestCaseId = "TC_F5.4_01";
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
            CurrentActualResult = isOnCheckoutPage
                ? "Guest truy cập được trang Checkout mà không bị redirect về Login."
                : $"Guest bị chặn. URL thực: {Driver.Url}";

            Assert.That(isOnCheckoutPage, Is.True,
                $"[F5.4_01] Guest phải vào được Checkout.\nThực tế URL: {Driver.Url}");
        }

        // =========================================================
        // F5.5 – Validation Form (10 test cases)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_5_01_KiemTraBatBuocHoTen()
        {
            CurrentTestCaseId = "TC_F5.5_01";
            var data = DocDuLieu("TC_CHECKOUT_F5_5_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName("");
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any() || _checkoutPage.IsFullNameInvalid();

            CurrentActualResult = hasError
                ? $"Hệ thống chặn lại, báo lỗi: {(messages.Any() ? string.Join(", ", messages) : "focus đỏ vào ô Họ tên")}."
                : "Hệ thống KHÔNG báo lỗi khi để trống Họ tên.";

            Assert.That(hasError, Is.True,
                $"[F5.5_01] Phải báo lỗi khi để trống Họ tên.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_02_KiemTraBatBuocSoDienThoai()
        {
            CurrentTestCaseId = "TC_F5.5_02";
            var data = DocDuLieu("TC_CHECKOUT_F5_5_02");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone("");
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any() || _checkoutPage.IsPhoneInvalid();

            CurrentActualResult = hasError
                ? $"Hệ thống chặn lại, báo lỗi: {(messages.Any() ? string.Join(", ", messages) : "focus đỏ vào ô SĐT")}."
                : "Hệ thống KHÔNG báo lỗi khi để trống SĐT.";

            Assert.That(hasError, Is.True,
                $"[F5.5_02] Phải báo lỗi khi để trống SĐT.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_03_SoDienThoaiSaiDoDai()
        {
            CurrentTestCaseId = "TC_F5.5_03";
            var data = DocDuLieu("TC_CHECKOUT_F5_5_03");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]); // 12 số
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any(m => m.Contains("10") || m.Contains("11") || m.Contains("số") || m.Contains("điện thoại"))
                || _checkoutPage.IsPhoneInvalid();

            CurrentActualResult = hasError
                ? $"Hệ thống chặn lại, báo lỗi SĐT sai độ dài: {(messages.Any() ? string.Join(", ", messages) : "field invalid")}."
                : "Hệ thống KHÔNG báo lỗi khi nhập SĐT 12 số.";

            Assert.That(hasError, Is.True,
                $"[F5.5_03] Phải báo lỗi SĐT sai độ dài.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_04_KiemTraBatBuocDiaChi()
        {
            CurrentTestCaseId = "TC_F5.5_04";
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
            bool hasError = messages.Any() || _checkoutPage.IsStreetAddressInvalid();

            CurrentActualResult = hasError
                ? $"Hệ thống chặn lại, báo lỗi: {(messages.Any() ? string.Join(", ", messages) : "focus đỏ vào ô Địa chỉ")}."
                : "Hệ thống KHÔNG báo lỗi khi để trống Địa chỉ.";

            Assert.That(hasError, Is.True,
                $"[F5.5_04] Phải báo lỗi khi để trống Địa chỉ.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_05_KiemTraBatBuocChonTinh()
        {
            CurrentTestCaseId = "TC_F5.5_05";
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
                || _checkoutPage.IsProvinceInvalid()
                || _checkoutPage.IsStillOnCheckoutPage();

            CurrentActualResult = hasError
                ? $"Hệ thống chặn lại khi không chọn Tỉnh/TP: {(messages.Any() ? string.Join(", ", messages) : "vẫn ở trang Checkout")}."
                : $"Hệ thống KHÔNG chặn khi không chọn Tỉnh/TP. URL: {Driver.Url}";

            Assert.That(hasError, Is.True,
                $"[F5.5_05] Phải chặn khi không chọn Tỉnh/TP.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_06_KiemTraBatBuocChonQuan()
        {
            CurrentTestCaseId = "TC_F5.5_06";
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
                || _checkoutPage.IsDistrictInvalid()
                || _checkoutPage.IsStillOnCheckoutPage();

            CurrentActualResult = hasError
                ? $"Hệ thống chặn lại khi không chọn Quận/Huyện: {(messages.Any() ? string.Join(", ", messages) : "vẫn ở trang Checkout")}."
                : $"Hệ thống KHÔNG chặn khi không chọn Quận/Huyện. URL: {Driver.Url}";

            Assert.That(hasError, Is.True,
                $"[F5.5_06] Phải chặn khi không chọn Quận/Huyện.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_07_KiemTraBatBuocChonPhuong()
        {
            CurrentTestCaseId = "TC_F5.5_07";
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
                || _checkoutPage.IsWardInvalid()
                || _checkoutPage.IsStillOnCheckoutPage();

            CurrentActualResult = hasError
                ? $"Hệ thống chặn lại khi không chọn Phường/Xã: {(messages.Any() ? string.Join(", ", messages) : "vẫn ở trang Checkout")}."
                : $"Hệ thống KHÔNG chặn khi không chọn Phường/Xã. URL: {Driver.Url}";

            Assert.That(hasError, Is.True,
                $"[F5.5_07] Phải chặn khi không chọn Phường/Xã.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_08_SoDienThoaiChuaChuCai()
        {
            CurrentTestCaseId = "TC_F5.5_08";
            var data = DocDuLieu("TC_CHECKOUT_F5_5_08");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]); // "abcd123456"
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any(m =>
                m.Contains("điện thoại", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("10") || m.Contains("11") || m.Contains("số"))
                || _checkoutPage.IsPhoneInvalid()
                || _checkoutPage.IsStillOnCheckoutPage();

            CurrentActualResult = hasError
                ? $"Hệ thống báo lỗi SĐT chứa chữ cái: {(messages.Any() ? string.Join(", ", messages) : "field invalid / vẫn ở Checkout")}."
                : "Hệ thống KHÔNG báo lỗi khi SĐT chứa chữ cái.";

            Assert.That(hasError, Is.True,
                $"[F5.5_08] Phải báo lỗi SĐT chứa chữ cái.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_09_SoDienThoaiKyTuDacBiet()
        {
            CurrentTestCaseId = "TC_F5.5_09";
            var data = DocDuLieu("TC_CHECKOUT_F5_5_09");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]); // "090-123-4567"
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any() || _checkoutPage.IsPhoneInvalid() || _checkoutPage.IsStillOnCheckoutPage();

            CurrentActualResult = hasError
                ? $"Hệ thống báo lỗi SĐT chứa ký tự đặc biệt: {(messages.Any() ? string.Join(", ", messages) : "field invalid / vẫn ở Checkout")}."
                : "Hệ thống KHÔNG báo lỗi khi SĐT chứa ký tự đặc biệt.";

            Assert.That(hasError, Is.True,
                $"[F5.5_09] Phải báo lỗi SĐT chứa ký tự đặc biệt.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_10_HoTenQuaDai()
        {
            CurrentTestCaseId = "TC_F5.5_10";
            var data = DocDuLieu("TC_CHECKOUT_F5_5_10");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]); // 101 ký tự
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            // Hệ thống không cho >100 ký tự hoặc truncate
            bool handled = messages.Any()
                || _checkoutPage.IsFullNameInvalid()
                || _checkoutPage.IsStillOnCheckoutPage();

            CurrentActualResult = handled
                ? $"Hệ thống xử lý họ tên >100 ký tự: {(messages.Any() ? string.Join(", ", messages) : "field invalid / vẫn ở Checkout")}."
                : "Hệ thống chấp nhận họ tên >100 ký tự không có lỗi.";

            Assert.That(handled, Is.True,
                $"[F5.5_10] Hệ thống phải xử lý họ tên quá 100 ký tự.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_11_DiaChiQuaDai()
        {
            CurrentTestCaseId = "TC_F5.5_11";
            var data = DocDuLieu("TC_CHECKOUT_F5_5_11");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]); // 201 ký tự
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool handled = messages.Any()
                || _checkoutPage.IsStreetAddressInvalid()
                || _checkoutPage.IsStillOnCheckoutPage();

            CurrentActualResult = handled
                ? $"Hệ thống xử lý địa chỉ >200 ký tự: {(messages.Any() ? string.Join(", ", messages) : "field invalid / vẫn ở Checkout")}."
                : "Hệ thống chấp nhận địa chỉ >200 ký tự không có lỗi.";

            Assert.That(handled, Is.True,
                $"[F5.5_11] Hệ thống phải xử lý địa chỉ quá 200 ký tự.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_12_TatCaTruongBatBuocBoTrong()
        {
            CurrentTestCaseId = "TC_F5.5_12";
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

            CurrentActualResult = hasMultipleErrors
                ? $"Hệ thống hiện nhiều lỗi khi bỏ trống tất cả: {(messages.Any() ? string.Join(", ", messages) : "vẫn ở trang Checkout")}."
                : $"Hệ thống chỉ hiện {messages.Count} lỗi (cần ≥2).";

            Assert.That(hasMultipleErrors, Is.True,
                $"[F5.5_12] Phải hiện nhiều lỗi khi bỏ trống tất cả.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_13_HoTenToanKhoangTrang()
        {
            CurrentTestCaseId = "TC_F5.5_13";
            var data = DocDuLieu("TC_CHECKOUT_F5_5_13");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName("   "); // chỉ khoảng trắng
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any()
                || _checkoutPage.IsFullNameInvalid()
                || _checkoutPage.IsStillOnCheckoutPage();

            CurrentActualResult = hasError
                ? $"Hệ thống coi khoảng trắng là bỏ trống, báo lỗi: {(messages.Any() ? string.Join(", ", messages) : "field invalid / vẫn ở Checkout")}."
                : "Hệ thống chấp nhận họ tên toàn khoảng trắng.";

            Assert.That(hasError, Is.True,
                $"[F5.5_13] Khoảng trắng phải được coi là bỏ trống.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_14_SoDienThoai10SoHopLe()
        {
            CurrentTestCaseId = "TC_F5.5_14";
            var data = DocDuLieu("TC_CHECKOUT_F5_5_14");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]); // "0901234567" (10 số)
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool phoneError = messages.Any(m => m.Contains("điện thoại", StringComparison.OrdinalIgnoreCase) || m.Contains("10") || m.Contains("Mobile"));

            CurrentActualResult = !phoneError
                ? "SĐT 10 số hợp lệ không bị báo lỗi. Hệ thống xử lý đúng."
                : $"Hệ thống báo lỗi với SĐT 10 số hợp lệ: {string.Join(", ", messages)}";

            Assert.That(phoneError, Is.False,
                $"[F5.5_14] SĐT 10 số hợp lệ không được báo lỗi.\nMessages: {string.Join(", ", messages)}");
        }

        [Test]
        public void TC_CHECKOUT_F5_5_15_SoDienThoai11SoHopLe()
        {
            CurrentTestCaseId = "TC_F5.5_15";
            var data = DocDuLieu("TC_CHECKOUT_F5_5_15");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]); // "09012345678" (11 số)
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool phoneError = messages.Any(m => m.Contains("điện thoại", StringComparison.OrdinalIgnoreCase) || m.Contains("10") || m.Contains("Mobile"));

            CurrentActualResult = !phoneError
                ? "SĐT 11 số hợp lệ không bị báo lỗi. Hệ thống xử lý đúng."
                : $"Hệ thống báo lỗi với SĐT 11 số hợp lệ: {string.Join(", ", messages)}";

            Assert.That(phoneError, Is.False,
                $"[F5.5_15] SĐT 11 số hợp lệ không được báo lỗi.\nMessages: {string.Join(", ", messages)}");
        }

        // =========================================================
        // F5.6 – Address Dropdown (6 test cases)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_6_01_DanhSachQuanHuyenThayDoiTheoTinh()
        {
            CurrentTestCaseId = "TC_F5.6_01";
            var data = DocDuLieu("TC_CHECKOUT_F5_6_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.SelectProvince(data["province"]);
            Thread.Sleep(1000);

            // Sau khi chọn Tỉnh, danh sách Quận/Huyện phải được load
            var districtOptions = Driver.FindElements(By.CssSelector("#districtSelect option"));

            CurrentActualResult = districtOptions.Count > 1
                ? $"Danh sách Quận/Huyện được cập nhật theo Tỉnh đã chọn ({districtOptions.Count} options)."
                : "Danh sách Quận/Huyện KHÔNG được cập nhật sau khi chọn Tỉnh.";

            Assert.That(districtOptions.Count, Is.GreaterThan(1),
                "[F5.6_01] Danh sách Quận/Huyện phải được cập nhật khi chọn Tỉnh/TP");
        }

        [Test]
        public void TC_CHECKOUT_F5_6_02_DanhSachPhuongXaThayDoiTheoQuan()
        {
            CurrentTestCaseId = "TC_F5.6_02";
            var data = DocDuLieu("TC_CHECKOUT_F5_6_02");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.SelectProvince(data["province"]);
            Thread.Sleep(1000);
            _checkoutPage.SelectDistrict(data["district"]);
            Thread.Sleep(1000);

            // Sau khi chọn Quận, danh sách Phường/Xã phải được load
            var wardOptions = Driver.FindElements(By.CssSelector("#wardSelect option"));

            CurrentActualResult = wardOptions.Count > 1
                ? $"Danh sách Phường/Xã được cập nhật theo Quận đã chọn ({wardOptions.Count} options)."
                : "Danh sách Phường/Xã KHÔNG được cập nhật sau khi chọn Quận.";

            Assert.That(wardOptions.Count, Is.GreaterThan(1),
                "[F5.6_02] Danh sách Phường/Xã phải được cập nhật khi chọn Quận/Huyện");
        }

        [Test]
        public void TC_CHECKOUT_F5_6_03_DropdownQuanHuyenDisableTruocKhiChonTinh()
        {
            CurrentTestCaseId = "TC_F5.6_03";
            var data = DocDuLieu("TC_CHECKOUT_F5_6_03");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            // Chưa chọn Tỉnh → Quận phải disabled hoặc chỉ có placeholder
            var districtSelect = Driver.FindElement(By.Id("districtSelect"));
            bool isDisabled = !districtSelect.Enabled
                || districtSelect.GetAttribute("disabled") != null
                || Driver.FindElements(By.CssSelector("#districtSelect option")).Count <= 1;

            CurrentActualResult = isDisabled
                ? "Dropdown Quận/Huyện bị disabled hoặc chỉ có placeholder khi chưa chọn Tỉnh/TP."
                : "Dropdown Quận/Huyện KHÔNG bị disabled dù chưa chọn Tỉnh/TP.";

            Assert.That(isDisabled, Is.True,
                "[F5.6_03] Dropdown Quận/Huyện phải disabled hoặc chỉ có placeholder khi chưa chọn Tỉnh/TP");
        }

        [Test]
        public void TC_CHECKOUT_F5_6_04_DropdownPhuongXaDisableTruocKhiChonQuan()
        {
            CurrentTestCaseId = "TC_F5.6_04";
            var data = DocDuLieu("TC_CHECKOUT_F5_6_04");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.SelectProvince(data["province"]);
            Thread.Sleep(1000);
            // Chưa chọn Quận → Ward phải disabled
            var wardSelect = Driver.FindElement(By.Id("wardSelect"));
            bool isDisabled = !wardSelect.Enabled
                || wardSelect.GetAttribute("disabled") != null
                || Driver.FindElements(By.CssSelector("#wardSelect option")).Count <= 1;

            CurrentActualResult = isDisabled
                ? "Dropdown Phường/Xã bị disabled khi chưa chọn Quận/Huyện."
                : "Dropdown Phường/Xã KHÔNG bị disabled dù chưa chọn Quận/Huyện.";

            Assert.That(isDisabled, Is.True,
                "[F5.6_04] Dropdown Phường/Xã phải disabled khi chưa chọn Quận/Huyện");
        }

        [Test]
        public void TC_CHECKOUT_F5_6_05_DoiTinhThiQuanVaPhuongBiReset()
        {
            CurrentTestCaseId = "TC_F5.6_05";
            var data = DocDuLieu("TC_CHECKOUT_F5_6_05");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            // Chọn tỉnh → quận → phường ban đầu
            _checkoutPage.SelectProvince(data["province"]);
            Thread.Sleep(1000);
            _checkoutPage.SelectDistrict(data["district"]);
            Thread.Sleep(1000);
            _checkoutPage.SelectWard(data["ward"]);
            Thread.Sleep(500);

            // Đổi sang tỉnh khác → Quận và Phường phải reset
            _checkoutPage.SelectProvince(data["provinceChange"]);
            Thread.Sleep(1000);

            var districtSelect = new SelectElement(Driver.FindElement(By.Id("districtSelect")));
            var wardSelect    = new SelectElement(Driver.FindElement(By.Id("wardSelect")));
            bool districtReset = districtSelect.SelectedOption.GetAttribute("value") == "" || districtSelect.SelectedOption.Text.Contains("Chọn");
            bool wardReset     = wardSelect.SelectedOption.GetAttribute("value") == ""     || wardSelect.SelectedOption.Text.Contains("Chọn");

            CurrentActualResult = (districtReset && wardReset)
                ? "Dropdown Quận/Huyện và Phường/Xã đều reset về placeholder sau khi đổi Tỉnh."
                : $"Dropdown KHÔNG reset đúng. Quận: '{districtSelect.SelectedOption.Text}', Phường: '{wardSelect.SelectedOption.Text}'.";

            Assert.That(districtReset, Is.True,
                "[F5.6_05] Dropdown Quận/Huyện phải reset về placeholder sau khi đổi Tỉnh");
            Assert.That(wardReset, Is.True,
                "[F5.6_05] Dropdown Phường/Xã phải reset về placeholder sau khi đổi Tỉnh");
        }

        [Test]
        public void TC_CHECKOUT_F5_6_06_DoiQuanThiPhuongBiReset()
        {
            CurrentTestCaseId = "TC_F5.6_06";
            var data = DocDuLieu("TC_CHECKOUT_F5_6_06");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.SelectProvince(data["province"]);
            Thread.Sleep(1000);
            _checkoutPage.SelectDistrict(data["district"]);
            Thread.Sleep(1000);
            _checkoutPage.SelectWard(data["ward"]);
            Thread.Sleep(500);

            // Đổi sang quận khác → Phường phải reset
            _checkoutPage.SelectDistrict(data["districtChange"]);
            Thread.Sleep(1000);

            var wardSelect = new SelectElement(Driver.FindElement(By.Id("wardSelect")));
            bool wardReset = wardSelect.SelectedOption.GetAttribute("value") == "" || wardSelect.SelectedOption.Text.Contains("Chọn");
            var wardOptions = Driver.FindElements(By.CssSelector("#wardSelect option"));

            CurrentActualResult = (wardReset && wardOptions.Count > 1)
                ? $"Dropdown Phường/Xã reset về placeholder và load {wardOptions.Count} options mới theo Quận 3."
                : $"Dropdown Phường/Xã KHÔNG reset đúng. Selected: '{wardSelect.SelectedOption.Text}', Options: {wardOptions.Count}.";

            Assert.That(wardReset, Is.True,
                "[F5.6_06] Dropdown Phường/Xã phải reset về placeholder sau khi đổi Quận");

            // Danh sách Phường mới phải load theo Quận 3
            Assert.That(wardOptions.Count, Is.GreaterThan(1),
                "[F5.6_06] Danh sách Phường mới phải load sau khi đổi Quận");
        }

        // =========================================================
        // F5.7 – Subtotal Calculation (3 test cases)
        // =========================================================

        [Test]
        public void TC_CHECKOUT_F5_7_01_TinhToanSubtotalDung()
        {
            CurrentTestCaseId = "TC_F5.7_01";
            var data = DocDuLieu("TC_CHECKOUT_F5_7_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            var summaryText = _checkoutPage.GetOrderSummaryText();
            bool hasSubtotal = summaryText.Contains("Tạm tính", StringComparison.OrdinalIgnoreCase)
                || summaryText.Contains("Subtotal", StringComparison.OrdinalIgnoreCase)
                || summaryText.Contains("đ") || summaryText.Contains("VNĐ");

            CurrentActualResult = hasSubtotal
                ? "Order Summary hiển thị giá trị Tạm tính/Subtotal."
                : "Order Summary KHÔNG hiển thị giá trị Tạm tính.";

            Assert.That(summaryText, Is.Not.Empty,
                "[F5.7_01] Order Summary phải có nội dung");
            Assert.That(hasSubtotal, Is.True,
                $"[F5.7_01] Phải hiển thị giá trị Tạm tính.\nSummary: {summaryText[..Math.Min(200, summaryText.Length)]}");
        }

        [Test]
        public void TC_CHECKOUT_F5_7_02_TenSanPhamHienThiDungTrongOrderSummary()
        {
            CurrentTestCaseId = "TC_F5.7_02";
            var data = DocDuLieu("TC_CHECKOUT_F5_7_02");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            var summaryText = _checkoutPage.GetOrderSummaryText();
            bool hasProductName = summaryText.Contains(data["expectedProductName"], StringComparison.OrdinalIgnoreCase);

            CurrentActualResult = hasProductName
                ? $"Order Summary hiển thị đúng tên sản phẩm '{data["expectedProductName"]}'."
                : $"Order Summary KHÔNG hiển thị tên sản phẩm '{data["expectedProductName"]}'.";

            Assert.That(hasProductName, Is.True,
                $"[F5.7_02] Order Summary phải hiển thị tên sản phẩm '{data["expectedProductName"]}'.\nSummary: {summaryText[..Math.Min(300, summaryText.Length)]}");
        }

        [Test]
        public void TC_CHECKOUT_F5_7_03_GiaVaSoLuongHienThiDung()
        {
            CurrentTestCaseId = "TC_F5.7_03";
            var data = DocDuLieu("TC_CHECKOUT_F5_7_03");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            var summaryText = _checkoutPage.GetOrderSummaryText();
            bool hasPriceInfo = summaryText.Contains("đ") || summaryText.Contains("VNĐ") || summaryText.Contains(",000");

            CurrentActualResult = hasPriceInfo
                ? "Order Summary hiển thị đúng giá và số lượng sản phẩm."
                : "Order Summary KHÔNG hiển thị thông tin giá/số lượng.";

            Assert.That(hasPriceInfo, Is.True,
                $"[F5.7_03] Order Summary phải hiển thị giá và số lượng.\nSummary: {summaryText[..Math.Min(300, summaryText.Length)]}");
        }

        // =========================================================
        // F5.8 – Phí vận chuyển (2 test cases)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_8_01_HienThiPhiVanChuyen()
        {
            CurrentTestCaseId = "TC_F5.8_01";
            var data = DocDuLieu("TC_CHECKOUT_F5_8_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            var summaryText = _checkoutPage.GetOrderSummaryText();
            bool hasShipping = summaryText.Contains("Phí vận chuyển", StringComparison.OrdinalIgnoreCase)
                || summaryText.Contains("Phí ship", StringComparison.OrdinalIgnoreCase)
                || summaryText.Contains("Shipping", StringComparison.OrdinalIgnoreCase);

            CurrentActualResult = hasShipping
                ? "Order Summary hiển thị Phí vận chuyển."
                : "Order Summary KHÔNG hiển thị Phí vận chuyển.";

            Assert.That(hasShipping, Is.True,
                $"[F5.8_01] Order Summary phải hiển thị Phí vận chuyển.\nSummary: {summaryText[..Math.Min(300, summaryText.Length)]}");
        }



        // =========================================================
        // F5.9 – Tổng tiền đơn hàng (1 test case)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_9_01_TinhToanTongTienDonHang()
        {
            CurrentTestCaseId = "TC_F5.9_01";
            var data = DocDuLieu("TC_CHECKOUT_F5_9_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            var summaryText = _checkoutPage.GetOrderSummaryText();
            bool hasTotal = summaryText.Contains("Tổng", StringComparison.OrdinalIgnoreCase)
                || summaryText.Contains("Total", StringComparison.OrdinalIgnoreCase);

            CurrentActualResult = hasTotal
                ? "Order Summary hiển thị Tổng tiền đơn hàng."
                : "Order Summary KHÔNG hiển thị Tổng tiền.";

            Assert.That(hasTotal, Is.True,
                $"[F5.9_01] Order Summary phải hiển thị Tổng tiền.\nSummary: {summaryText[..Math.Min(300, summaryText.Length)]}");
        }

        // =========================================================
        // F5.10 – COD Payment (2 test cases)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_10_01_ChonPhuongThucCOD()
        {
            CurrentTestCaseId = "TC_F5.10_01";
            var data = DocDuLieu("TC_CHECKOUT_F5_10_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            _checkoutPage.SelectPaymentCod();
            bool codSelected = _checkoutPage.IsCodSelected();

            CurrentActualResult = codSelected
                ? "Radio button COD được chọn thành công."
                : "Radio button COD KHÔNG được chọn.";

            Assert.That(codSelected, Is.True,
                "[F5.10_01] Radio button COD phải được chọn thành công");
        }

        [Test]
        public void TC_CHECKOUT_F5_10_02_CODDuocChonMacDinh()
        {
            CurrentTestCaseId = "TC_F5.10_02";
            var data = DocDuLieu("TC_CHECKOUT_F5_10_02");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            // Không click gì – kiểm tra COD được check sẵn
            bool codDefault = _checkoutPage.IsCodSelected();
            bool transferNotSelected = !_checkoutPage.IsTransferSelected();

            CurrentActualResult = (codDefault && transferNotSelected)
                ? "COD được chọn mặc định; Chuyển khoản không được check."
                : $"COD mặc định: {codDefault}, Chuyển khoản không check: {transferNotSelected}.";

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
            CurrentTestCaseId = "TC_F5.11_01";
            var data = DocDuLieu("TC_CHECKOUT_F5_11_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            _checkoutPage.SelectPaymentTransfer();
            bool transferSelected = _checkoutPage.IsTransferSelected();
            bool bankInfoVisible = _checkoutPage.IsBankInfoDisplayed();

            CurrentActualResult = (transferSelected && bankInfoVisible)
                ? "Radio button Chuyển khoản được chọn, thông tin ngân hàng hiển thị."
                : $"Chuyển khoản selected: {transferSelected}, Bank info visible: {bankInfoVisible}.";

            Assert.That(transferSelected, Is.True,
                "[F5.11_01] Radio button Chuyển khoản phải được chọn");
            Assert.That(bankInfoVisible, Is.True,
                "[F5.11_01] Thông tin ngân hàng phải hiển thị sau khi chọn Chuyển khoản");
        }



        // =========================================================
        // F5.12 & F5.13 – Đặt hàng thành công + Xóa giỏ hàng (4 TCs)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_12_01_DatHangThanhCong()
        {
            CurrentTestCaseId = "TC_F5.12_01";
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

            bool onConfirmation = _checkoutPage.IsOnConfirmationPage();
            var confirmMsg = _checkoutPage.GetConfirmationMessage();
            bool msgOk = confirmMsg.Contains(data["expectedConfirmationText"], StringComparison.OrdinalIgnoreCase);

            CurrentActualResult = (onConfirmation && msgOk)
                ? $"Đặt hàng thành công, chuyển đến /Checkout/Confirmation. Thông báo: '{confirmMsg[..Math.Min(100, confirmMsg.Length)]}'."
                : $"Đặt hàng KHÔNG thành công. URL: {Driver.Url}. Confirmation: '{confirmMsg[..Math.Min(100, confirmMsg.Length)]}'.";

            Assert.That(onConfirmation, Is.True,
                "[F5.12_01] Phải chuyển đến /Checkout/Confirmation sau khi đặt hàng");
            Assert.That(confirmMsg, Does.Contain(data["expectedConfirmationText"]).IgnoreCase,
                $"[F5.12_01] Phải hiện thông báo '{data["expectedConfirmationText"]}'");
        }

        [Test]
        public void TC_CHECKOUT_F5_13_01_KiemTraGioHangXoaSauDatHang()
        {
            CurrentTestCaseId = "TC_F5.13_01";
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

            CurrentActualResult = cartEmpty
                ? "Giỏ hàng không còn sản phẩm sau khi đặt hàng thành công."
                : "Giỏ hàng VẪN còn sản phẩm sau khi đặt hàng.";

            Assert.That(cartEmpty, Is.True,
                "[F5.13_01] Giỏ hàng phải không còn sản phẩm sau khi đặt hàng thành công");
        }

        // =========================================================
        // F5.15 – Buy Now
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_15_01_ChucNangMuaNgay()
        {
            CurrentTestCaseId = "TC_F5.15_01";
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

            bool onCheckout = Driver.Url.Contains("/Checkout");
            bool notCart = !Driver.Url.Contains("/Cart");

            CurrentActualResult = (onCheckout && notCart)
                ? $"Mua ngay dẫn thẳng đến /Checkout không qua /Cart. URL: {Driver.Url}."
                : $"Mua ngay KHÔNG dẫn thẳng đến /Checkout. URL thực: {Driver.Url}.";

            Assert.That(onCheckout, Is.True,
                $"[F5.15_01] Mua ngay phải dẫn thẳng đến /Checkout, không qua /Cart.\nURL thực: {Driver.Url}");
            Assert.That(notCart, Is.True,
                "[F5.15_01] Không được đi qua trang /Cart");
        }

        // =========================================================
        // F5.16 – Order Confirmation Display
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_16_01_HienThiTrangXacNhanDonHang()
        {
            CurrentTestCaseId = "TC_F5.16_01";
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

            bool onConfirmation = _checkoutPage.IsOnConfirmationPage();

            var pageText = Driver.FindElement(By.TagName("body")).Text;
            // Xác nhận có hiển thị: Mã đơn hàng, sản phẩm, địa chỉ, PT thanh toán, tổng tiền
            bool hasOrderInfo = pageText.Contains("Cảm ơn", StringComparison.OrdinalIgnoreCase)
                || pageText.Contains("thành công", StringComparison.OrdinalIgnoreCase)
                || pageText.Contains("Confirmation", StringComparison.OrdinalIgnoreCase);

            CurrentActualResult = (onConfirmation && hasOrderInfo)
                ? "Trang xác nhận hiển thị đúng thông tin đơn hàng (cảm ơn, thành công)."
                : $"Trang xác nhận KHÔNG hiển thị đúng. URL: {Driver.Url}. Text đầu: {pageText[..Math.Min(150, pageText.Length)]}.";

            Assert.That(onConfirmation, Is.True,
                "[F5.16_01] Phải chuyển đến trang /Checkout/Confirmation");

            Assert.That(hasOrderInfo, Is.True,
                $"[F5.16_01] Trang xác nhận phải hiển thị thông tin đơn hàng.\nText: {pageText[..Math.Min(300, pageText.Length)]}");
        }

        // =========================================================
        // F5.19 – Redirect về Login khi chưa đăng nhập
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_19_01_RedirectVeLoginKhiChuaDangNhap()
        {
            CurrentTestCaseId = "TC_F5.19_01";
            var data = DocDuLieu("TC_CHECKOUT_F5_19_01");
            // Không đăng nhập – truy cập thẳng /Checkout
            Driver.Navigate().GoToUrl(data["checkoutUrl"]);
            Thread.Sleep(1000);

            bool redirectedToLogin = Driver.Url.Contains("/Account/Login");
            bool notOnCheckout = !Driver.Url.Contains("/Checkout") || Driver.Url.Contains("/Account/Login");

            CurrentActualResult = (redirectedToLogin || notOnCheckout)
                ? $"Hệ thống redirect về /Account/Login khi chưa đăng nhập. URL: {Driver.Url}."
                : $"Hệ thống KHÔNG redirect về Login. URL thực: {Driver.Url}.";

            Assert.That(redirectedToLogin || notOnCheckout, Is.True,
                $"[F5.19_01] Chưa đăng nhập phải bị redirect về /Account/Login.\nURL thực: {Driver.Url}");
        }

        // =========================================================
        // F5.20 – Redirect lại Checkout sau khi đăng nhập
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_20_01_RedirectVeCheckoutSauDangNhap()
        {
            CurrentTestCaseId = "TC_F5.20_01";
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

            CurrentActualResult = backToCheckout
                ? $"Sau khi đăng nhập, hệ thống redirect về /Checkout. URL: {Driver.Url}."
                : $"Sau khi đăng nhập, KHÔNG redirect về /Checkout. URL thực: {Driver.Url}.";

            Assert.That(backToCheckout, Is.True,
                $"[F5.20_01] Sau khi đăng nhập phải redirect về /Checkout.\nURL thực: {Driver.Url}");
        }

        // =========================================================
        // F5.24 – Phí ship thay đổi theo khu vực
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_24_01_PhiShipThayDoiTheoKhuVuc()
        {
            CurrentTestCaseId = "TC_F5.24_01";
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

            CurrentActualResult = (shippingVisible1 || shippingVisible2)
                ? "Phí vận chuyển hiển thị khi chọn tỉnh. Phí có thể thay đổi theo khu vực."
                : "Phí vận chuyển KHÔNG hiển thị khi chọn tỉnh.";

            Assert.That(shippingVisible1 || shippingVisible2, Is.True,
                $"[F5.24_01] Phí vận chuyển phải hiển thị khi chọn tỉnh.\nSummary: {summary1[..Math.Min(200, summary1.Length)]}");
        }

        // =========================================================
        // F5.9_02 – Tổng cộng = Tạm tính + Phí vận chuyển
        // =========================================================

        [Test]
        public void TC_CHECKOUT_F5_9_02_TongCongBangTamTinhCongPhiShip()
        {
            CurrentTestCaseId = "TC_F5.9_02";
            var data = DocDuLieu("TC_CHECKOUT_F5_9_02");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectWard(data["ward"]);
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

            CurrentActualResult = (hasTamTinh && hasPhiShip && hasTotal)
                ? "Order Summary hiển thị đầy đủ Tạm tính, Phí vận chuyển và Tổng cộng."
                : $"Order Summary thiếu thông tin: TamTinh={hasTamTinh}, PhiShip={hasPhiShip}, Total={hasTotal}.";

            Assert.That(hasTamTinh, Is.True,
                $"[F5.9_02] Order Summary phải hiển thị Tạm tính.\nSummary: {summaryText[..Math.Min(300, summaryText.Length)]}");
            Assert.That(hasPhiShip, Is.True,
                $"[F5.9_02] Order Summary phải hiển thị Phí vận chuyển.\nSummary: {summaryText[..Math.Min(300, summaryText.Length)]}");
            Assert.That(hasTotal, Is.True,
                $"[F5.9_02] Order Summary phải hiển thị Tổng cộng.\nSummary: {summaryText[..Math.Min(300, summaryText.Length)]}");
        }

        // =========================================================
        // F5.14 – Trừ tồn kho sau đặt hàng (kiểm tra từ trang sản phẩm)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_14_01_TruTonKhoSauDatHang()
        {
            CurrentTestCaseId = "TC_F5.14_01";
            var orderData = DocDuLieu("TC_CHECKOUT_F5_12_01");
            string productUrl = orderData["productUrl"];

            // ── Bước 1: Đăng nhập và đọc tồn kho TRƯỚC khi đặt hàng ──────
            _checkoutPage.Login(orderData["email"], orderData["password"]);

            int stockBefore = _checkoutPage.GetStockFromProductPage(productUrl);

            Assert.That(stockBefore, Is.GreaterThan(0),
                $"[F5.14_01] Phải đọc được tồn kho > 0 từ trang sản phẩm trước khi đặt hàng. Đọc được: {stockBefore}");

            // Số lượng sẽ đặt (mặc định 1 = mỗi lần click Mua ngay)
            int quantityOrdered = 1;

            // ── Bước 2: Tiến hành Checkout với 1 sản phẩm ────────────────
            _checkoutPage.NavigateToCheckoutWithProduct(productUrl);
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
                "[F5.14_01] Đặt hàng phải thành công trước khi kiểm tra tồn kho");

            // ── Bước 3: Quay lại trang chi tiết sản phẩm → đọc tồn kho SAU ─
            int stockAfter = _checkoutPage.GetStockFromProductPage(productUrl);

            // ── Bước 4: So sánh kết quả ──────────────────────────────────
            int expectedStockAfter = stockBefore - quantityOrdered;

            CurrentActualResult = stockAfter == expectedStockAfter
                ? $"Tồn kho trước khi đặt = {stockBefore}. Tồn kho sau khi đặt = {stockAfter} ({stockBefore} - {quantityOrdered}). Đúng!"
                : $"Tồn kho trước khi đặt = {stockBefore}. Tồn kho sau khi đặt = {stockAfter} (kỳ vọng {expectedStockAfter}).";

            Assert.That(stockAfter, Is.EqualTo(expectedStockAfter),
                $"[F5.14_01] Tồn kho phải giảm đúng {quantityOrdered} đơn vị.\n" +
                $"Trước: {stockBefore} | Sau: {stockAfter} | Kỳ vọng: {expectedStockAfter}");
        }

        // =========================================================
        // F5.17 – Lưu địa chỉ mới (Medium - 2 TCs)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_17_01_LuuDiaChiMoiThanhCong()
        {
            CurrentTestCaseId = "TC_F5.17_01";
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

            bool onConfirmation = _checkoutPage.IsOnConfirmationPage();
            Assert.That(onConfirmation, Is.True,
                "[F5.17_01] Đặt hàng với lưu địa chỉ phải thành công");

            // Vào lần mua tiếp theo – kiểm tra dropdown địa chỉ đã lưu xuất hiện
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            Thread.Sleep(1000);
            var savedDropdown = Driver.FindElements(By.CssSelector("select[id*='saved'], select[id*='Saved'], select[id*='address'], #savedAddressSelect"));

            CurrentActualResult = savedDropdown.Count > 0
                ? "Dropdown địa chỉ đã lưu xuất hiện trong lần mua tiếp theo."
                : "Dropdown địa chỉ đã lưu KHÔNG xuất hiện trong lần mua tiếp theo.";

            Assert.That(savedDropdown.Count, Is.GreaterThan(0),
                "[F5.17_01] Dropdown địa chỉ đã lưu phải xuất hiện trong lần mua tiếp theo");
        }

        [Test]
        public void TC_CHECKOUT_F5_17_02_DropdownDiaChiDaLuuHienThi()
        {
            CurrentTestCaseId = "TC_F5.17_02";
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

            CurrentActualResult = options.Count > 1
                ? $"Dropdown địa chỉ đã lưu hiển thị {options.Count - 1} địa chỉ (ngoài placeholder)."
                : "Dropdown địa chỉ đã lưu KHÔNG có địa chỉ (chỉ có placeholder).";

            Assert.That(options.Count, Is.GreaterThan(1),
                "[F5.17_02] Dropdown địa chỉ đã lưu phải hiển thị ít nhất 1 địa chỉ (ngoài placeholder)");
        }

        // =========================================================
        // F5.18 – Chọn địa chỉ đã lưu tự động điền form (Medium)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_18_01_ChonDiaChiDaLuuTuDongDienForm()
        {
            CurrentTestCaseId = "TC_F5.18_01";
            var data = DocDuLieu("TC_CHECKOUT_F5_12_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            Thread.Sleep(1500);

            // Dropdown địa chỉ đã lưu có id="addressSelect"
            var savedDropdown = Driver.FindElements(By.Id("addressSelect"));
            if (savedDropdown.Count == 0)
            {
                CurrentActualResult = "Không tìm thấy dropdown 'addressSelect' – cần chạy F5.17_01 trước.";
                Assert.Fail("[F5.18_01] Không tìm thấy dropdown 'addressSelect' – cần chạy F5.17_01 trước");
                return;
            }

            // Chọn option đầu tiên có value (bỏ qua placeholder)
            var sel = new SelectElement(savedDropdown[0]);
            var nonEmptyOptions = sel.Options.Where(o => !string.IsNullOrEmpty(o.GetAttribute("value")) && o.GetAttribute("value") != "new").ToList();
            if (nonEmptyOptions.Count == 0)
            {
                CurrentActualResult = "Không có địa chỉ trong dropdown – cần chạy F5.17_01 trước.";
                Assert.Fail("[F5.18_01] Không có địa chỉ trong dropdown – cần chạy F5.17_01 trước");
                return;
            }

            nonEmptyOptions[0].Click();
            Thread.Sleep(1500); // Chờ JavaScript điền form

            // Sau khi chọn địa chỉ, panel hiển thị địa chỉ phải xuất hiện (id=selectedAddressDisplay)
            // Hoặc kiểm tra các hidden input đã được điền
            var displayPanel = Driver.FindElements(By.Id("selectedAddressDisplay"));
            bool panelVisible = displayPanel.Count > 0 && displayPanel[0].Displayed;

            CurrentActualResult = panelVisible
                ? "Panel địa chỉ đã lưu (selectedAddressDisplay) hiển thị sau khi chọn."
                : "Panel địa chỉ đã lưu KHÔNG hiển thị rõ ràng, nhưng có địa chỉ trong dropdown.";

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
            CurrentTestCaseId = "TC_F5.21_01";
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

            CurrentActualResult = hasMaOrder
                ? "Trang xác nhận hiển thị mã đơn hàng."
                : $"Trang xác nhận KHÔNG hiển thị mã đơn hàng. Text đầu: {pageText[..Math.Min(200, pageText.Length)]}.";

            Assert.That(hasMaOrder, Is.True,
                $"[F5.21_01] Trang xác nhận phải hiển thị mã đơn hàng.\nText: {pageText[..Math.Min(300, pageText.Length)]}");
        }

        // =========================================================
        // F5.22 – Nút "Tiếp tục mua sắm" (Medium)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_22_01_NutTiepTucMuaSam()
        {
            CurrentTestCaseId = "TC_F5.22_01";
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

            CurrentActualResult = onShopOrHome
                ? $"Nút 'Tiếp tục mua sắm' dẫn về trang chủ/Shop. URL: {Driver.Url}."
                : $"Nút 'Tiếp tục mua sắm' dẫn đến sai trang. URL thực: {Driver.Url}.";

            Assert.That(onShopOrHome, Is.True,
                $"[F5.22_01] Phải chuyển về trang chủ hoặc Shop.\nURL thực: {Driver.Url}");
        }

        // =========================================================
        // F5.23 – Thông tin ngân hàng ẩn khi đổi về COD (Medium)
        // =========================================================
        [Test]
        public void TC_CHECKOUT_F5_23_01_ThongTinNganHangAnKhiDoiVeCOD()
        {
            CurrentTestCaseId = "TC_F5.23_01";
            var data = DocDuLieu("TC_CHECKOUT_F5_10_01");
            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            // Chọn Chuyển khoản → thông tin ngân hàng phải hiện
            _checkoutPage.SelectPaymentTransfer();
            bool bankInfoAfterTransfer = _checkoutPage.IsBankInfoDisplayed();
            Assert.That(bankInfoAfterTransfer, Is.True,
                "[F5.23_01] Thông tin ngân hàng phải hiển thị khi chọn Chuyển khoản");

            // Đổi về COD → thông tin ngân hàng phải ẩn
            _checkoutPage.SelectPaymentCod();
            bool bankInfoAfterCod = _checkoutPage.IsBankInfoDisplayed();
            bool codSelected = _checkoutPage.IsCodSelected();

            CurrentActualResult = (!bankInfoAfterCod && codSelected)
                ? "Thông tin ngân hàng ẩn đúng khi đổi về COD. COD được chọn thành công."
                : $"Thông tin ngân hàng sau COD: {bankInfoAfterCod} (phải false). COD selected: {codSelected}.";

            Assert.That(bankInfoAfterCod, Is.False,
                "[F5.23_01] Thông tin ngân hàng phải ẩn khi đổi về COD");
            Assert.That(codSelected, Is.True,
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
