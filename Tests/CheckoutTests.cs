using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumProject.Pages;
using SeleniumProject.Utilities;

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
            CurrentSheetName = "TC_Checkout";
            _checkoutPage = new CheckoutPage(Driver, BaseUrl);
        }

        // Helper: đọc dữ liệu và login bằng tài khoản trong JSON
        private Dictionary<string, string> DocDuLieuVaLogin(string testCaseId)
        {
            var data = JsonHelper.DocDuLieu(DataPath, testCaseId);
            CurrentTestCaseId = testCaseId;
            _checkoutPage.Login(data["email"], data["password"]);
            return data;
        }

        // =====================================================================
        // TC_CHECKOUT_01 – F5.1_01
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_01_HienThiTrangCheckout()
        {
            var data = DocDuLieuVaLogin("TC_CHECKOUT_01");

            _checkoutPage.GoToCheckoutViaCart(data["productUrl"]);

            CurrentActualResult = _checkoutPage.IsCheckoutPageDisplayed()
                ? "Trang Checkout hiển thị đầy đủ 3 phần"
                : "Trang Checkout không hiển thị đủ";

            Assert.That(_checkoutPage.IsCheckoutPageDisplayed(), Is.True,
                "Kỳ vọng [F5.1_01]: Trang Checkout phải hiển thị đầy đủ Shipping Form, Order Summary và Payment Method");

            Assert.That(_checkoutPage.GetCurrentUrl(), Does.Contain("/Checkout"),
                "Kỳ vọng [F5.1_01]: URL phải chứa '/Checkout'");
        }

        // =====================================================================
        // TC_CHECKOUT_02 – F5.2_01
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_02_GioHangRong_Redirect()
        {
            var data = DocDuLieuVaLogin("TC_CHECKOUT_02");

            _checkoutPage.Open();

            CurrentActualResult = _checkoutPage.IsRedirectedFromEmptyCart()
                ? "Redirect về homepage hoặc thông báo giỏ hàng rỗng"
                : "Vẫn ở trang Checkout dù giỏ rỗng";

            Assert.That(_checkoutPage.IsRedirectedFromEmptyCart(), Is.True,
                "Kỳ vọng [F5.2_01]: Hệ thống phải redirect về homepage hoặc hiển thị thông báo 'Cart is empty'");
        }

        // =====================================================================
        // TC_CHECKOUT_03 – F5.5_01
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_03_KiemTraBatBuocHoTen()
        {
            var data = DocDuLieuVaLogin("TC_CHECKOUT_03");

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

            CurrentActualResult = hasError
                ? $"Có lỗi validation: {string.Join(", ", messages)}"
                : "Không hiển thị lỗi khi bỏ trống Họ tên";

            Assert.That(hasError, Is.True,
                $"Kỳ vọng [F5.5_01]: Hệ thống phải báo lỗi khi để trống Họ tên.\nMessages: {string.Join(", ", messages)}");
        }

        // =====================================================================
        // TC_CHECKOUT_04 – F5.5_02
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_04_KiemTraBatBuocSoDienThoai()
        {
            var data = DocDuLieuVaLogin("TC_CHECKOUT_04");

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

            CurrentActualResult = hasError
                ? $"Có lỗi validation: {string.Join(", ", messages)}"
                : "Không hiển thị lỗi khi bỏ trống SĐT";

            Assert.That(hasError, Is.True,
                $"Kỳ vọng [F5.5_02]: Hệ thống phải báo lỗi khi để trống Số điện thoại.\nMessages: {string.Join(", ", messages)}");
        }

        // =====================================================================
        // TC_CHECKOUT_05 – F5.5_03
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_05_SoDienThoaiSaiDoDai()
        {
            var data = DocDuLieuVaLogin("TC_CHECKOUT_05");

            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince("Thành phố Hà Nội");
            _checkoutPage.SelectDistrict("Quận Ba Đình");
            _checkoutPage.SelectWard("Phường Phúc Xá");
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any(m =>
                m.Contains("10") || m.Contains("11") || m.Contains("số")
                || m.Contains("điện thoại") || m.Contains("Mobile"))
                || _checkoutPage.IsFieldInvalid(By.Id("Mobile"));

            CurrentActualResult = hasError
                ? $"Có lỗi: {string.Join(", ", messages)}"
                : "Không hiển thị lỗi khi SĐT sai độ dài";

            Assert.That(hasError, Is.True,
                $"Kỳ vọng [F5.5_03]: Hệ thống phải báo lỗi SĐT không đúng độ dài.\nMessages: {string.Join(", ", messages)}");
        }

        // =====================================================================
        // TC_CHECKOUT_06 – F5.5_04
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_06_KiemTraBatBuocDiaChi()
        {
            var data = DocDuLieuVaLogin("TC_CHECKOUT_06");

            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress("");
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any() || _checkoutPage.IsFieldInvalid(By.Id("StreetAddressInput"));

            CurrentActualResult = hasError
                ? $"Có lỗi: {string.Join(", ", messages)}"
                : "Không hiển thị lỗi khi bỏ trống Địa chỉ";

            Assert.That(hasError, Is.True,
                $"Kỳ vọng [F5.5_04]: Hệ thống phải báo lỗi khi để trống Địa chỉ.\nMessages: {string.Join(", ", messages)}");
        }

        // =====================================================================
        // TC_CHECKOUT_07 – F5.5_05
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_07_KiemTraBatBuocChonTinh()
        {
            var data = DocDuLieuVaLogin("TC_CHECKOUT_07");

            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any()
                || _checkoutPage.IsFieldInvalid(By.Id("provinceSelect"))
                || _checkoutPage.IsStillOnCheckoutPage();

            CurrentActualResult = hasError
                ? $"Có lỗi hoặc vẫn ở trang Checkout: {string.Join(", ", messages)}"
                : "Không chặn khi không chọn Tỉnh";

            Assert.That(hasError, Is.True,
                $"Kỳ vọng [F5.5_05]: Hệ thống phải chặn đặt hàng khi không chọn Tỉnh/Thành phố.\nMessages: {string.Join(", ", messages)}");
        }

        // =====================================================================
        // TC_CHECKOUT_08 – F5.5_06
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_08_KiemTraBatBuocChonQuan()
        {
            var data = DocDuLieuVaLogin("TC_CHECKOUT_08");

            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any()
                || _checkoutPage.IsFieldInvalid(By.Id("districtSelect"))
                || _checkoutPage.IsStillOnCheckoutPage();

            CurrentActualResult = hasError
                ? $"Có lỗi hoặc vẫn ở trang Checkout: {string.Join(", ", messages)}"
                : "Không chặn khi không chọn Quận";

            Assert.That(hasError, Is.True,
                $"Kỳ vọng [F5.5_06]: Hệ thống phải chặn khi không chọn Quận/Huyện.\nMessages: {string.Join(", ", messages)}");
        }

        // =====================================================================
        // TC_CHECKOUT_09 – F5.5_07
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_09_KiemTraBatBuocChonPhuong()
        {
            var data = DocDuLieuVaLogin("TC_CHECKOUT_09");

            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any()
                || _checkoutPage.IsFieldInvalid(By.Id("wardSelect"))
                || _checkoutPage.IsStillOnCheckoutPage();

            CurrentActualResult = hasError
                ? $"Có lỗi hoặc vẫn ở trang Checkout: {string.Join(", ", messages)}"
                : "Không chặn khi không chọn Phường";

            Assert.That(hasError, Is.True,
                $"Kỳ vọng [F5.5_07]: Hệ thống phải chặn khi không chọn Phường/Xã.\nMessages: {string.Join(", ", messages)}");
        }

        // =====================================================================
        // TC_CHECKOUT_10 – F5.6_01
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_10_DanhSachQuanThayDoiTheoTinh()
        {
            var data = DocDuLieuVaLogin("TC_CHECKOUT_10");

            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            var districtsBefore = _checkoutPage.GetAvailableDistricts();
            _checkoutPage.SelectProvince(data["province"]);
            var districtsAfter = _checkoutPage.GetAvailableDistricts();

            CurrentActualResult = $"Trước: {districtsBefore.Count} quận, Sau: {districtsAfter.Count} quận";

            Assert.That(districtsAfter.Count, Is.GreaterThan(districtsBefore.Count),
                $"Kỳ vọng [F5.6_01]: District list phải có thêm options sau khi chọn Tỉnh '{data["province"]}'.\n" +
                $"Trước: {districtsBefore.Count}, Sau: {districtsAfter.Count}");
        }

        // =====================================================================
        // TC_CHECKOUT_11 – F5.6_02
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_11_DanhSachPhuongThayDoiTheoQuan()
        {
            var data = DocDuLieuVaLogin("TC_CHECKOUT_11");

            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.SelectProvince(data["province"]);
            var wardsBefore = _checkoutPage.GetAvailableWards();

            _checkoutPage.SelectDistrict(data["district"]);
            var wardsAfter = _checkoutPage.GetAvailableWards();

            CurrentActualResult = $"Trước: {wardsBefore.Count} phường, Sau: {wardsAfter.Count} phường";

            Assert.That(wardsAfter.Count, Is.GreaterThan(wardsBefore.Count),
                $"Kỳ vọng [F5.6_02]: Ward list phải có thêm options sau khi chọn Quận '{data["district"]}'.\n" +
                $"Trước: {wardsBefore.Count}, Sau: {wardsAfter.Count}");
        }

        // =====================================================================
        // TC_CHECKOUT_12 – F5.10_01
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_12_ChonPhuongThucCOD()
        {
            var data = DocDuLieuVaLogin("TC_CHECKOUT_12");

            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectPaymentCod();

            CurrentActualResult = _checkoutPage.IsCodSelected()
                ? "Radio COD đã được chọn"
                : "Radio COD chưa được chọn";

            Assert.That(_checkoutPage.IsCodSelected(), Is.True,
                "Kỳ vọng [F5.10_01]: Radio button COD phải được chọn thành công");
        }

        // =====================================================================
        // TC_CHECKOUT_13 – F5.11_01
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_13_ChonPhuongThucChuyenKhoan()
        {
            var data = DocDuLieuVaLogin("TC_CHECKOUT_13");

            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectPaymentTransfer();

            CurrentActualResult = _checkoutPage.IsTransferSelected() && _checkoutPage.IsBankInfoDisplayed()
                ? "Chuyển khoản được chọn, thông tin ngân hàng hiển thị"
                : "Chuyển khoản chưa được chọn hoặc thiếu thông tin ngân hàng";

            Assert.That(_checkoutPage.IsTransferSelected(), Is.True,
                "Kỳ vọng [F5.11_01]: Radio button Chuyển khoản phải được chọn thành công");

            Assert.That(_checkoutPage.IsBankInfoDisplayed(), Is.True,
                "Kỳ vọng [F5.11_01]: Thông tin ngân hàng phải hiển thị sau khi chọn Chuyển khoản");
        }

        // =====================================================================
        // TC_CHECKOUT_14 – F5.12_01
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_14_DatHangThanhCong()
        {
            var data = DocDuLieuVaLogin("TC_CHECKOUT_14");

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

            var confirmMsg = _checkoutPage.GetConfirmationMessage();
            CurrentActualResult = _checkoutPage.IsOnConfirmationPage()
                ? $"Redirect về trang xác nhận: {confirmMsg}"
                : "Không redirect đến trang xác nhận";

            Assert.That(_checkoutPage.IsOnConfirmationPage(), Is.True,
                "Kỳ vọng [F5.12_01]: Sau khi đặt hàng phải chuyển đến /Checkout/Confirmation");

            Assert.That(confirmMsg, Does.Contain(data["expectedConfirmationText"]).IgnoreCase,
                $"Kỳ vọng [F5.12_01]: Phải hiện thông báo '{data["expectedConfirmationText"]}'. Thực tế: '{confirmMsg}'");
        }
    }
}
