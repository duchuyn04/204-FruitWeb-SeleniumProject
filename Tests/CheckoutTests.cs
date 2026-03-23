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

        // Đường dẫn file test data
        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "Checkout", "checkout_data.json"
        );

        [SetUp]
        public void SetUpCheckoutPage()
        {
            _checkoutPage = new CheckoutPage(Driver);
        }

        // =====================================================================
        // TC_CHECKOUT_01 – F5.1_01
        // Kiểm tra hiển thị trang Checkout
        // Pre-condition:
        //   - Người dùng đã thêm ít nhất 1 sản phẩm vào giỏ hàng
        //   - Người dùng đang ở trang Giỏ hàng
        // Steps:
        //   Bước 1: Truy cập Cart
        //   Bước 2: Click nút "THANH TOÁN" (Checkout)
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_01_HienThiTrangCheckout()
        {
            var data = DocDuLieu("TC_CHECKOUT_01");

            // Pre-condition: đăng nhập, thêm sản phẩm vào giỏ
            _checkoutPage.Login(data["email"], data["password"]);

            // Bước 1: Thêm sản phẩm → vào Cart
            // Bước 2: Click THANH TOÁN → sang Checkout
            _checkoutPage.GoToCheckoutViaCart(data["productUrl"]);

            // Kiểm tra trang Checkout hiển thị đủ 3 phần:
            // Shipping Form, Order Summary, Payment Method
            Assert.That(_checkoutPage.IsCheckoutPageDisplayed(), Is.True,
                "Kỳ vọng [F5.1_01]: Trang Checkout phải hiển thị đầy đủ Shipping Form, Order Summary và Payment Method");

            Assert.That(_checkoutPage.GetCurrentUrl(), Does.Contain("/Checkout"),
                "Kỳ vọng [F5.1_01]: URL phải chứa '/Checkout'");
        }

        // =====================================================================
        // TC_CHECKOUT_02 – F5.2_01
        // Kiểm tra không cho thanh toán khi giỏ hàng rỗng
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_02_GioHangRong_Redirect()
        {
            var data = DocDuLieu("TC_CHECKOUT_02");

            _checkoutPage.Login(data["email"], data["password"]);

            // Truy cập thẳng URL Checkout khi cart rỗng
            _checkoutPage.Open();

            Assert.That(_checkoutPage.IsRedirectedFromEmptyCart(), Is.True,
                "Kỳ vọng [F5.2_01]: Hệ thống phải redirect về homepage hoặc hiển thị thông báo 'Cart is empty'");
        }

        // =====================================================================
        // TC_CHECKOUT_03 – F5.5_01
        // Kiểm tra bắt buộc nhập Họ tên
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_03_KiemTraBatBuocHoTen()
        {
            var data = DocDuLieu("TC_CHECKOUT_03");

            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption(); // Hiện form nhập địa chỉ mới

            // Để trống Họ tên
            _checkoutPage.EnterFullName("");
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any()
                || _checkoutPage.IsFieldInvalid(By.Id("FullNameInput"));

            Assert.That(hasError, Is.True,
                $"Kỳ vọng [F5.5_01]: Hệ thống phải báo lỗi khi để trống Họ tên.\nMessages: {string.Join(", ", messages)}");
        }

        // =====================================================================
        // TC_CHECKOUT_04 – F5.5_02
        // Kiểm tra bắt buộc nhập số điện thoại
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_04_KiemTraBatBuocSoDienThoai()
        {
            var data = DocDuLieu("TC_CHECKOUT_04");

            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(""); // bỏ trống
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any()
                || _checkoutPage.IsFieldInvalid(By.Id("Mobile"));

            Assert.That(hasError, Is.True,
                $"Kỳ vọng [F5.5_02]: Hệ thống phải báo lỗi khi để trống Số điện thoại.\nMessages: {string.Join(", ", messages)}");
        }

        // =====================================================================
        // TC_CHECKOUT_05 – F5.5_03
        // Kiểm tra số điện thoại phải đủ 10-11 số (nhập 12 số)
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_05_SoDienThoaiSaiDoDai()
        {
            var data = DocDuLieu("TC_CHECKOUT_05");

            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]); // 12 ký tự: "12345678909"
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any(m =>
                m.Contains("10") || m.Contains("11") || m.Contains("số")
                || m.Contains("điện thoại") || m.Contains("Mobile"))
                || _checkoutPage.IsFieldInvalid(By.Id("Mobile"));

            Assert.That(hasError, Is.True,
                $"Kỳ vọng [F5.5_03]: Hệ thống phải báo lỗi SĐT không đúng độ dài.\nMessages: {string.Join(", ", messages)}");
        }

        // =====================================================================
        // TC_CHECKOUT_06 – F5.5_04
        // Kiểm tra bắt buộc nhập Địa chỉ
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_06_KiemTraBatBuocDiaChi()
        {
            var data = DocDuLieu("TC_CHECKOUT_06");

            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(""); // bỏ trống
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any()
                || _checkoutPage.IsFieldInvalid(By.Id("StreetAddressInput"));

            Assert.That(hasError, Is.True,
                $"Kỳ vọng [F5.5_04]: Hệ thống phải báo lỗi khi để trống Địa chỉ.\nMessages: {string.Join(", ", messages)}");
        }

        // =====================================================================
        // TC_CHECKOUT_07 – F5.5_05
        // Kiểm tra bắt buộc chọn Tỉnh/Thành phố
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_07_KiemTraBatBuocChonTinh()
        {
            var data = DocDuLieu("TC_CHECKOUT_07");

            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            // KHÔNG chọn Tỉnh/Thành phố
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any()
                || _checkoutPage.IsFieldInvalid(By.Id("provinceSelect"))
                || _checkoutPage.IsStillOnCheckoutPage();

            Assert.That(hasError, Is.True,
                $"Kỳ vọng [F5.5_05]: Hệ thống phải chặn đặt hàng khi không chọn Tỉnh/Thành phố.\nMessages: {string.Join(", ", messages)}");
        }

        // =====================================================================
        // TC_CHECKOUT_08 – F5.5_06
        // Kiểm tra bắt buộc chọn Quận/Huyện
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_08_KiemTraBatBuocChonQuan()
        {
            var data = DocDuLieu("TC_CHECKOUT_08");

            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]); // chọn Tỉnh
            // KHÔNG chọn Quận/Huyện
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any()
                || _checkoutPage.IsFieldInvalid(By.Id("districtSelect"))
                || _checkoutPage.IsStillOnCheckoutPage();

            Assert.That(hasError, Is.True,
                $"Kỳ vọng [F5.5_06]: Hệ thống phải chặn khi không chọn Quận/Huyện.\nMessages: {string.Join(", ", messages)}");
        }

        // =====================================================================
        // TC_CHECKOUT_09 – F5.5_07
        // Kiểm tra bắt buộc chọn Phường/Xã
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_09_KiemTraBatBuocChonPhuong()
        {
            var data = DocDuLieu("TC_CHECKOUT_09");

            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.EnterFullName(data["fullName"]);
            _checkoutPage.EnterPhone(data["phone"]);
            _checkoutPage.EnterStreetAddress(data["streetAddress"]);
            _checkoutPage.SelectProvince(data["province"]);
            _checkoutPage.SelectDistrict(data["district"]); // chọn Quận
            // KHÔNG chọn Phường/Xã
            _checkoutPage.SelectPaymentCod();
            _checkoutPage.ClickPlaceOrder();

            var messages = _checkoutPage.GetValidationMessages();
            bool hasError = messages.Any()
                || _checkoutPage.IsFieldInvalid(By.Id("wardSelect"))
                || _checkoutPage.IsStillOnCheckoutPage();

            Assert.That(hasError, Is.True,
                $"Kỳ vọng [F5.5_07]: Hệ thống phải chặn khi không chọn Phường/Xã.\nMessages: {string.Join(", ", messages)}");
        }

        // =====================================================================
        // TC_CHECKOUT_10 – F5.6_01
        // Kiểm tra danh sách Quận/Huyện thay đổi theo Tỉnh
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_10_DanhSachQuanThayDoiTheoTinh()
        {
            var data = DocDuLieu("TC_CHECKOUT_10");

            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            // Trước khi chọn Tỉnh → District dropdown trống / chỉ có 1 placeholder
            var districtsBefore = _checkoutPage.GetAvailableDistricts();

            // Sau khi chọn Tỉnh → District phải có dữ liệu
            _checkoutPage.SelectProvince(data["province"]);
            var districtsAfter = _checkoutPage.GetAvailableDistricts();

            Assert.That(districtsAfter.Count, Is.GreaterThan(districtsBefore.Count),
                $"Kỳ vọng [F5.6_01]: District list phải có thêm options sau khi chọn Tỉnh '{data["province"]}'.\n" +
                $"Trước: {districtsBefore.Count}, Sau: {districtsAfter.Count}");
        }

        // =====================================================================
        // TC_CHECKOUT_11 – F5.6_02
        // Kiểm tra danh sách Phường/Xã thay đổi theo Quận
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_11_DanhSachPhuongThayDoiTheoQuan()
        {
            var data = DocDuLieu("TC_CHECKOUT_11");

            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption();

            _checkoutPage.SelectProvince(data["province"]);
            var wardsBefore = _checkoutPage.GetAvailableWards();

            _checkoutPage.SelectDistrict(data["district"]);
            var wardsAfter = _checkoutPage.GetAvailableWards();

            Assert.That(wardsAfter.Count, Is.GreaterThan(wardsBefore.Count),
                $"Kỳ vọng [F5.6_02]: Ward list phải có thêm options sau khi chọn Quận '{data["district"]}'.\n" +
                $"Trước: {wardsBefore.Count}, Sau: {wardsAfter.Count}");
        }

        // =====================================================================
        // TC_CHECKOUT_12 – F5.10_01
        // Kiểm tra chọn phương thức thanh toán COD
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_12_ChonPhuongThucCOD()
        {
            var data = DocDuLieu("TC_CHECKOUT_12");

            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            _checkoutPage.SelectPaymentCod();

            Assert.That(_checkoutPage.IsCodSelected(), Is.True,
                "Kỳ vọng [F5.10_01]: Radio button COD phải được chọn thành công");
        }

        // =====================================================================
        // TC_CHECKOUT_13 – F5.11_01
        // Kiểm tra chọn phương thức thanh toán Chuyển khoản
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_13_ChonPhuongThucChuyenKhoan()
        {
            var data = DocDuLieu("TC_CHECKOUT_13");

            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);

            _checkoutPage.SelectPaymentTransfer();

            Assert.That(_checkoutPage.IsTransferSelected(), Is.True,
                "Kỳ vọng [F5.11_01]: Radio button Chuyển khoản phải được chọn thành công");

            Assert.That(_checkoutPage.IsBankInfoDisplayed(), Is.True,
                "Kỳ vọng [F5.11_01]: Thông tin ngân hàng phải hiển thị sau khi chọn Chuyển khoản");
        }

        // =====================================================================
        // TC_CHECKOUT_14 – F5.12_01
        // Kiểm tra đặt hàng thành công
        // =====================================================================
        [Test]
        public void TC_CHECKOUT_14_DatHangThanhCong()
        {
            var data = DocDuLieu("TC_CHECKOUT_14");

            _checkoutPage.Login(data["email"], data["password"]);
            _checkoutPage.NavigateToCheckoutWithProduct(data["productUrl"]);
            _checkoutPage.SelectNewAddressOption(); // Chọn nhập địa chỉ mới

            // Nhập đầy đủ thông tin
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
                "Kỳ vọng [F5.12_01]: Sau khi đặt hàng phải chuyển đến /Checkout/Confirmation");

            var confirmMsg = _checkoutPage.GetConfirmationMessage();
            Assert.That(confirmMsg, Does.Contain(data["expectedConfirmationText"]).IgnoreCase,
                $"Kỳ vọng [F5.12_01]: Phải hiện thông báo '{data["expectedConfirmationText"]}'. Thực tế: '{confirmMsg}'");
        }

        // =====================================================================
        // HELPER
        // =====================================================================
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

    // Dictionary trả về string.Empty thay vì throw KeyNotFoundException
    internal class DefaultDictionary : Dictionary<string, string>
    {
        public DefaultDictionary(Dictionary<string, string> source) : base(source) { }

        public new string this[string key] =>
            TryGetValue(key, out var val) ? val : string.Empty;
    }
}
