using NUnit.Framework;
using SeleniumProject.Pages.Auth;
using SeleniumProject.Utilities;

namespace SeleniumProject.Tests.Auth
{
    [TestFixture]
    public class RegisterTests : TestBase
    {
        private RegisterPage _registerPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "Auth", "register_data.json"
        );

        [SetUp]
        public void SetUpPages()
        {
            CurrentSheetName = "TC_Auth";
            _registerPage = new RegisterPage(Driver, BaseUrl);
        }

        // helper
        private Dictionary<string, string> DocDuLieu(string tcId)
            => JsonHelper.DocDuLieu(DataPath, tcId);

        // ============================================
        // NO 1 -> TC_F1.1_01: Đăng ký thành công với thông tin hợp lệ đầy đủ
        // ============================================
        [Test]
        public void TC_F1_1_01_DangKyThanhCong_ThongTinDayDu()
        {
            CurrentTestCaseId = "TC_F1.1_01";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Wait.WaitForUrlContains("/Account/Login");

            string currentUrl = Driver.Url;
            CurrentActualResult = $"Đăng ký thành công, chuyển hướng về trang đăng nhập. (URL: {currentUrl})";

            Assert.That(currentUrl.Contains("/Account/Login"), Is.True,
                "[TC_F1.1_01] Sau đăng ký thành công phải redirect về /Account/Login");
        }

        // ============================================
        // NO 2 -> TC_F1.1_02: Đăng ký thành công với họ tên có dấu tiếng Việt
        // ============================================
        [Test]
        public void TC_F1_1_02_DangKyThanhCong_HoTenTiengViet()
        {
            CurrentTestCaseId = "TC_F1.1_02";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Wait.WaitForUrlContains("/Account/Login");

            string currentUrl = Driver.Url;
            CurrentActualResult = $"Hệ thống chấp nhận tên Unicode tiếng Việt, redirect về login. (URL: {currentUrl})";

            Assert.That(currentUrl.Contains("/Account/Login"), Is.True,
                "[TC_F1.1_02] Hệ thống phải chấp nhận tên tiếng Việt có dấu");
        }

        // ============================================
        // NO 3 -> TC_F1.1_03: Đăng ký thành công với mật khẩu 6 ký tự (boundary min)
        // ============================================
        [Test]
        public void TC_F1_1_03_DangKyThanhCong_MatKhau6KyTu()
        {
            CurrentTestCaseId = "TC_F1.1_03";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Wait.WaitForUrlContains("/Account/Login");

            string currentUrl = Driver.Url;
            CurrentActualResult = $"Mật khẩu 6 ký tự được chấp nhận, redirect về login. (URL: {currentUrl})";

            Assert.That(currentUrl.Contains("/Account/Login"), Is.True,
                "[TC_F1.1_03] Mật khẩu đúng 6 ký tự (boundary min) phải được chấp nhận");
        }

        // ============================================
        // NO 4 -> TC_F1.1_04: Đăng ký thành công với email dạng subdomain hợp lệ
        // ============================================
        [Test]
        public void TC_F1_1_04_DangKyThanhCong_EmailSubdomain()
        {
            CurrentTestCaseId = "TC_F1.1_04";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Wait.WaitForUrlContains("/Account/Login");

            string currentUrl = Driver.Url;
            CurrentActualResult = $"Email subdomain được chấp nhận, redirect về login. (URL: {currentUrl})";

            Assert.That(currentUrl.Contains("/Account/Login"), Is.True,
                "[TC_F1.1_04] Email dạng subdomain hợp lệ phải được chấp nhận");
        }

        // ============================================
        // NO 5 -> TC_F1.1_05: Đăng ký thành công với họ và tên đúng 1 ký tự (boundary min)
        // ============================================
        [Test]
        public void TC_F1_1_05_DangKyThanhCong_HoTen1KyTu()
        {
            CurrentTestCaseId = "TC_F1.1_05";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Wait.WaitForUrlContains("/Account/Login");

            string currentUrl = Driver.Url;
            CurrentActualResult = $"Họ và tên 1 ký tự được chấp nhận, redirect về login. (URL: {currentUrl})";

            Assert.That(currentUrl.Contains("/Account/Login"), Is.True,
                "[TC_F1.1_05] Họ và tên 1 ký tự (boundary min) phải được chấp nhận");
        }

        // ============================================
        // NO 6 -> TC_F1.2_01: Báo lỗi khi để trống tất cả các trường
        // ============================================
        [Test]
        public void TC_F1_2_01_BaoLoi_TrongTatCaTruong()
        {
            CurrentTestCaseId = "TC_F1.2_01";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Thread.Sleep(800);

            bool stillOnPage = _registerPage.IsOnPage();
            string ketQua = _registerPage.DocKetQuaThucTe();
            CurrentActualResult = $"Ở lại trang đăng ký: {stillOnPage} | {ketQua}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.2_01] Phải ở lại trang đăng ký khi để trống tất cả các trường");
        }

        // ============================================
        // NO 7 -> TC_F1.2_02: Báo lỗi khi để trống trường Họ và tên
        // ============================================
        [Test]
        public void TC_F1_2_02_BaoLoi_TrongHoVaTen()
        {
            CurrentTestCaseId = "TC_F1.2_02";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Thread.Sleep(800);

            string nameError = _registerPage.GetNameError();
            bool stillOnPage = _registerPage.IsOnPage();
            CurrentActualResult = $"Lỗi field Họ tên: '{nameError}' | URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.2_02] Phải ở lại trang đăng ký khi thiếu Họ và tên");
            Assert.That(nameError, Is.Not.Empty,
                "[TC_F1.2_02] Phải hiển thị lỗi cho trường Họ và tên");
        }

        // ============================================
        // NO 8 -> TC_F1.2_03: Báo lỗi khi Email sai định dạng
        // ============================================
        [Test]
        public void TC_F1_2_03_BaoLoi_EmailSaiDinhDang()
        {
            CurrentTestCaseId = "TC_F1.2_03";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Thread.Sleep(800);

            string emailError = _registerPage.GetEmailError();
            bool stillOnPage = _registerPage.IsOnPage();
            CurrentActualResult = $"Lỗi field Email: '{emailError}' | URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.2_03] Phải ở lại trang đăng ký khi email sai định dạng");
            Assert.That(emailError, Is.Not.Empty,
                "[TC_F1.2_03] Phải hiển thị lỗi cho trường Email sai định dạng");
        }

        // ============================================
        // NO 9 -> TC_F1.2_04: Báo lỗi khi Email đã tồn tại trong hệ thống
        // ============================================
        [Test]
        public void TC_F1_2_04_BaoLoi_EmailDaTonTai()
        {
            CurrentTestCaseId = "TC_F1.2_04";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Thread.Sleep(1200);

            bool stillOnPage = _registerPage.IsOnPage();
            bool hasError = _registerPage.HasAnyError();
            CurrentActualResult = hasError
                ? $"Hệ thống chặn email trùng, hiện thông báo lỗi. URL: {Driver.Url}"
                : $"URL: {Driver.Url} | {_registerPage.DocKetQuaThucTe()}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.2_04] Phải ở lại trang đăng ký khi email đã tồn tại");
            Assert.That(hasError, Is.True,
                "[TC_F1.2_04] Phải hiển thị thông báo lỗi khi email đã tồn tại");
        }

        // ============================================
        // NO 10 -> TC_F1.2_05: Báo lỗi khi mật khẩu 5 ký tự (dưới boundary min)
        // ============================================
        [Test]
        public void TC_F1_2_05_BaoLoi_MatKhau5KyTu()
        {
            CurrentTestCaseId = "TC_F1.2_05";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Thread.Sleep(800);

            string passError = _registerPage.GetPasswordError();
            bool stillOnPage = _registerPage.IsOnPage();
            CurrentActualResult = $"Lỗi field Mật khẩu: '{passError}' | URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.2_05] Phải ở lại trang đăng ký khi mật khẩu < 6 ký tự");
            Assert.That(passError, Is.Not.Empty,
                "[TC_F1.2_05] Phải hiển thị lỗi mật khẩu quá ngắn");
        }

        // ============================================
        // NO 11 -> TC_F1.2_06: Báo lỗi khi Xác nhận mật khẩu không khớp
        // ============================================
        [Test]
        public void TC_F1_2_06_BaoLoi_XacNhanMatKhauKhongKhop()
        {
            CurrentTestCaseId = "TC_F1.2_06";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Thread.Sleep(800);

            string confirmError = _registerPage.GetConfirmPasswordError();
            bool stillOnPage = _registerPage.IsOnPage();
            CurrentActualResult = $"Lỗi field Xác nhận MK: '{confirmError}' | URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.2_06] Phải ở lại trang đăng ký khi xác nhận mật khẩu không khớp");
            Assert.That(confirmError, Is.Not.Empty,
                "[TC_F1.2_06] Phải hiển thị lỗi xác nhận mật khẩu không khớp");
        }

        // ============================================
        // NO 12 -> TC_F1.2_07: Báo lỗi khi Họ và tên vượt quá 200 ký tự (201 ký tự)
        // ============================================
        [Test]
        public void TC_F1_2_07_BaoLoi_HoTenVuot200KyTu()
        {
            CurrentTestCaseId = "TC_F1.2_07";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Thread.Sleep(1000);

            bool stillOnPage = _registerPage.IsOnPage();
            bool hasError = _registerPage.HasAnyError();
            CurrentActualResult = hasError
                ? $"Hệ thống chặn tên > 200 ký tự, hiển thị lỗi. URL: {Driver.Url}"
                : $"URL: {Driver.Url} | {_registerPage.DocKetQuaThucTe()}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.2_07] Phải ở lại trang đăng ký khi tên > 200 ký tự");
            Assert.That(hasError, Is.True,
                "[TC_F1.2_07] Phải hiển thị lỗi khi Họ và tên vượt quá 200 ký tự");
        }

        // ============================================
        // NO 13 -> TC_F1.2_08: Báo lỗi khi Họ và tên vượt quá 200 ký tự (201 ký tự, dữ liệu khác)
        // ============================================
        [Test]
        public void TC_F1_2_08_BaoLoi_HoTenVuot200KyTu_DuLieuKhac()
        {
            CurrentTestCaseId = "TC_F1.2_08";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Thread.Sleep(1000);

            bool stillOnPage = _registerPage.IsOnPage();
            bool hasError = _registerPage.HasAnyError();
            CurrentActualResult = hasError
                ? $"Hệ thống chặn tên > 200 ký tự (dữ liệu khác), hiển thị lỗi. URL: {Driver.Url}"
                : $"URL: {Driver.Url} | {_registerPage.DocKetQuaThucTe()}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.2_08] Phải ở lại trang đăng ký khi tên > 200 ký tự");
            Assert.That(hasError, Is.True,
                "[TC_F1.2_08] Phải hiển thị lỗi khi Họ và tên vượt quá 200 ký tự");
        }

        // ============================================
        // NO 14 -> TC_F1.2_09: Báo lỗi khi Email chứa khoảng trắng
        // ============================================
        [Test]
        public void TC_F1_2_09_BaoLoi_EmailChuaKhoangTrang()
        {
            CurrentTestCaseId = "TC_F1.2_09";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Thread.Sleep(800);

            string emailError = _registerPage.GetEmailError();
            bool stillOnPage = _registerPage.IsOnPage();
            CurrentActualResult = $"Lỗi field Email: '{emailError}' | URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.2_09] Phải ở lại trang đăng ký khi email chứa khoảng trắng");
            Assert.That(emailError, Is.Not.Empty,
                "[TC_F1.2_09] Phải hiển thị lỗi khi email chứa khoảng trắng");
        }

        // ============================================
        // NO 15 -> TC_F1.2_10: Báo lỗi khi để trống trường Email
        // ============================================
        [Test]
        public void TC_F1_2_10_BaoLoi_TrongEmail()
        {
            CurrentTestCaseId = "TC_F1.2_10";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Thread.Sleep(800);

            string emailError = _registerPage.GetEmailError();
            bool stillOnPage = _registerPage.IsOnPage();
            CurrentActualResult = $"Lỗi field Email: '{emailError}' | URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.2_10] Phải ở lại trang đăng ký khi Email để trống");
            Assert.That(emailError, Is.Not.Empty,
                "[TC_F1.2_10] Phải hiển thị lỗi khi Email để trống");
        }

        // ============================================
        // NO 16 -> TC_F1.2_11: Báo lỗi khi để trống trường Mật khẩu
        // ============================================
        [Test]
        public void TC_F1_2_11_BaoLoi_TrongMatKhau()
        {
            CurrentTestCaseId = "TC_F1.2_11";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Thread.Sleep(800);

            string passError = _registerPage.GetPasswordError();
            bool stillOnPage = _registerPage.IsOnPage();
            CurrentActualResult = $"Lỗi field Mật khẩu: '{passError}' | URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.2_11] Phải ở lại trang đăng ký khi Mật khẩu để trống");
            Assert.That(passError, Is.Not.Empty,
                "[TC_F1.2_11] Phải hiển thị lỗi khi Mật khẩu để trống");
        }

        // ============================================
        // NO 17 -> TC_F1.2_12: Báo lỗi khi Email chỉ chứa ký tự đặc biệt
        // ============================================
        [Test]
        public void TC_F1_2_12_BaoLoi_EmailKyTuDacBiet()
        {
            CurrentTestCaseId = "TC_F1.2_12";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Thread.Sleep(800);

            string emailError = _registerPage.GetEmailError();
            bool stillOnPage = _registerPage.IsOnPage();
            CurrentActualResult = $"Lỗi field Email: '{emailError}' | URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.2_12] Phải ở lại trang đăng ký khi Email chỉ gồm ký tự đặc biệt");
            Assert.That(emailError, Is.Not.Empty,
                "[TC_F1.2_12] Phải hiển thị lỗi khi Email sai định dạng");
        }

        // ============================================
        // NO 18 -> TC_F1.2_13: Báo lỗi khi Họ và tên toàn số và ký tự đặc biệt
        // ============================================
        [Test]
        public void TC_F1_2_13_BaoLoi_HoTenToanSoVaKyTuDacBiet()
        {
            CurrentTestCaseId = "TC_F1.2_13";
            var data = DocDuLieu(CurrentTestCaseId);

            _registerPage.Open();
            _registerPage.Register(data["fullName"], data["email"], data["password"], data["confirmPassword"]);
            Thread.Sleep(1200);

            bool stillOnPage = _registerPage.IsOnPage();
            bool hasError = _registerPage.HasAnyError();
            CurrentActualResult = hasError
                ? $"Hệ thống chặn tên toàn số/ký tự đặc biệt. URL: {Driver.Url}"
                : $"URL: {Driver.Url} | {_registerPage.DocKetQuaThucTe()}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.2_13] Phải ở lại trang đăng ký khi tên toàn số và ký tự đặc biệt");
            Assert.That(hasError, Is.True,
                "[TC_F1.2_13] Phải báo lỗi khi Họ và tên toàn số và ký tự đặc biệt");
        }
    }
}
