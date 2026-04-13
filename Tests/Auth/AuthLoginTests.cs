using NUnit.Framework;
using SeleniumProject.Pages.Auth;
using SeleniumProject.Utilities;

namespace SeleniumProject.Tests.Auth
{
    [TestFixture]
    public class AuthLoginTests : TestBase
    {
        private LoginPage _loginPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "Auth", "login_data.json"
        );

        [SetUp]
        public void SetUpPages()
        {
            CurrentSheetName = "TC_Authentication";
            _loginPage = new LoginPage(Driver, BaseUrl);
        }

        // helper
        private Dictionary<string, string> DocDuLieu(string tcId)
            => JsonHelper.DocDuLieu(DataPath, tcId);

        // ============================================
        // NO 19 -> TC_F1.4_01: Đăng nhập thành công với tài khoản Admin hợp lệ
        // ============================================
        [Test]
        public void TC_F1_4_01_DangNhapThanhCong_Admin()
        {
            CurrentTestCaseId = "TC_F1.4_01";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Wait.WaitForUrlNotContains("/Account/Login");

            string currentUrl = Driver.Url;
            CurrentActualResult = $"Đăng nhập Admin thành công, chuyển về Dashboard. (URL: {currentUrl})";

            Assert.That(currentUrl.Contains("/Admin"), Is.True,
                "[TC_F1.4_01] Admin phải được chuyển hướng về /Admin sau khi đăng nhập");
        }

        // ============================================
        // NO 20 -> TC_F1.4_02: Đăng nhập thành công với Ghi nhớ đăng nhập được bật
        // ============================================
        [Test]
        public void TC_F1_4_02_DangNhapThanhCong_GhiNhoLogin()
        {
            CurrentTestCaseId = "TC_F1.4_02";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"], rememberMe: true);
            Wait.WaitForUrlNotContains("/Account/Login");

            string currentUrl = Driver.Url;
            CurrentActualResult = $"Đăng nhập thành công với RememberMe=true. (URL: {currentUrl})";

            Assert.That(_loginPage.IsLoginSuccessful(), Is.True,
                "[TC_F1.4_02] Phải đăng nhập thành công khi bật Ghi nhớ đăng nhập");
        }

        // ============================================
        // NO 21 -> TC_F1.4_03: Đăng nhập thành công với tài khoản Customer
        // ============================================
        [Test]
        public void TC_F1_4_03_DangNhapThanhCong_Customer()
        {
            CurrentTestCaseId = "TC_F1.4_03";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Wait.WaitForUrlNotContains("/Account/Login");

            string currentUrl = Driver.Url;
            CurrentActualResult = $"Đăng nhập Customer thành công, chuyển về trang chủ. (URL: {currentUrl})";

            Assert.That(_loginPage.IsLoginSuccessful(), Is.True,
                "[TC_F1.4_03] Customer phải đăng nhập thành công và chuyển về trang chủ");
        }

        // ============================================
        // NO 22 -> TC_F1.4_04: Đăng nhập thành công với tài khoản SuperAdmin
        // ============================================
        [Test]
        public void TC_F1_4_04_DangNhapThanhCong_SuperAdmin()
        {
            CurrentTestCaseId = "TC_F1.4_04";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Wait.WaitForUrlNotContains("/Account/Login");

            string currentUrl = Driver.Url;
            CurrentActualResult = $"SuperAdmin đăng nhập thành công. (URL: {currentUrl})";

            Assert.That(currentUrl.Contains("/Admin"), Is.True,
                "[TC_F1.4_04] SuperAdmin phải được chuyển hướng về /Admin Dashboard");
        }

        // ============================================
        // NO 23 -> TC_F1.4_05: Chuyển hướng đúng trang sau khi đăng nhập từ URL được bảo vệ
        // ============================================
        [Test]
        public void TC_F1_4_05_ChuyenHuongDung_SauLoginTuUrlBaoVe()
        {
            CurrentTestCaseId = "TC_F1.4_05";
            var data = DocDuLieu(CurrentTestCaseId);

            // Truy cập URL bảo vệ → bị redirect về login
            Driver.Navigate().GoToUrl(BaseUrl + data["protectedUrl"]);
            Wait.WaitForUrlContains("/Account/Login");

            _loginPage.Login(data["email"], data["password"]);
            Wait.WaitForUrlNotContains("/Account/Login");

            string currentUrl = Driver.Url;
            CurrentActualResult = $"Đăng nhập thành công sau redirect từ URL bảo vệ. (URL: {currentUrl})";

            Assert.That(_loginPage.IsLoginSuccessful(), Is.True,
                "[TC_F1.4_05] Phải đăng nhập thành công và chuyển đúng trang được bảo vệ");
        }

        // ============================================
        // NO 24 -> TC_F1.4_06: Đăng nhập thành công với mật khẩu đúng 6 ký tự (boundary min)
        // ============================================
        [Test]
        public void TC_F1_4_06_DangNhapThanhCong_MatKhau6KyTu()
        {
            CurrentTestCaseId = "TC_F1.4_06";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Wait.WaitForUrlNotContains("/Account/Login");

            string currentUrl = Driver.Url;
            CurrentActualResult = $"Đăng nhập thành công với mật khẩu 6 ký tự. (URL: {currentUrl})";

            Assert.That(_loginPage.IsLoginSuccessful(), Is.True,
                "[TC_F1.4_06] Mật khẩu đúng 6 ký tự (boundary min) phải đăng nhập thành công");
        }

        // ============================================
        // NO 25 -> TC_F1.5_01: Báo lỗi khi để trống cả Email và Mật khẩu
        // ============================================
        [Test]
        public void TC_F1_5_01_BaoLoi_TrongCaEmail_MatKhau()
        {
            CurrentTestCaseId = "TC_F1.5_01";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Thread.Sleep(800);

            bool stillOnPage = _loginPage.IsOnPage();
            CurrentActualResult = $"Vẫn ở trang đăng nhập khi cả 2 trường trống. URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.5_01] Phải ở lại trang đăng nhập khi để trống Email và Mật khẩu");
        }

        // ============================================
        // NO 26 -> TC_F1.5_02: Báo lỗi khi để trống Email
        // ============================================
        [Test]
        public void TC_F1_5_02_BaoLoi_TrongEmail()
        {
            CurrentTestCaseId = "TC_F1.5_02";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Thread.Sleep(800);

            string emailError = _loginPage.GetEmailError();
            bool stillOnPage = _loginPage.IsOnPage();
            CurrentActualResult = $"Lỗi field Email: '{emailError}' | URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.5_02] Phải ở lại trang đăng nhập khi Email để trống");
            Assert.That(emailError, Is.Not.Empty,
                "[TC_F1.5_02] Phải hiển thị lỗi khi Email để trống");
        }

        // ============================================
        // NO 27 -> TC_F1.5_03: Báo lỗi khi để trống Mật khẩu
        // ============================================
        [Test]
        public void TC_F1_5_03_BaoLoi_TrongMatKhau()
        {
            CurrentTestCaseId = "TC_F1.5_03";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Thread.Sleep(800);

            string passError = _loginPage.GetPasswordError();
            bool stillOnPage = _loginPage.IsOnPage();
            CurrentActualResult = $"Lỗi field Mật khẩu: '{passError}' | URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.5_03] Phải ở lại trang đăng nhập khi Mật khẩu để trống");
            Assert.That(passError, Is.Not.Empty,
                "[TC_F1.5_03] Phải hiển thị lỗi khi Mật khẩu để trống");
        }

        // ============================================
        // NO 28 -> TC_F1.5_04: Báo lỗi khi Email sai định dạng
        // ============================================
        [Test]
        public void TC_F1_5_04_BaoLoi_EmailSaiDinhDang()
        {
            CurrentTestCaseId = "TC_F1.5_04";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Thread.Sleep(800);

            string emailError = _loginPage.GetEmailError();
            bool stillOnPage = _loginPage.IsOnPage();
            CurrentActualResult = $"Lỗi field Email: '{emailError}' | URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.5_04] Phải ở lại trang đăng nhập khi Email sai định dạng");
            Assert.That(emailError, Is.Not.Empty,
                "[TC_F1.5_04] Phải hiển thị lỗi định dạng Email");
        }

        // ============================================
        // NO 29 -> TC_F1.5_05: Báo lỗi khi Email không tồn tại trong hệ thống
        // ============================================
        [Test]
        public void TC_F1_5_05_BaoLoi_EmailKhongTonTai()
        {
            CurrentTestCaseId = "TC_F1.5_05";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Thread.Sleep(1200);

            string toastMsg = _loginPage.GetToastMessage();
            bool stillOnPage = _loginPage.IsOnPage();
            CurrentActualResult = !string.IsNullOrEmpty(toastMsg)
                ? $"Toast lỗi: '{toastMsg}'"
                : $"URL: {Driver.Url} | {_loginPage.DocKetQuaThucTe()}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.5_05] Phải ở lại trang đăng nhập khi email không tồn tại");
        }

        // ============================================
        // NO 30 -> TC_F1.5_06: Báo lỗi khi Mật khẩu sai
        // ============================================
        [Test]
        public void TC_F1_5_06_BaoLoi_MatKhauSai()
        {
            CurrentTestCaseId = "TC_F1.5_06";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Thread.Sleep(1200);

            string toastMsg = _loginPage.GetToastMessage();
            bool stillOnPage = _loginPage.IsOnPage();
            CurrentActualResult = !string.IsNullOrEmpty(toastMsg)
                ? $"Toast lỗi: '{toastMsg}'"
                : $"URL: {Driver.Url} | {_loginPage.DocKetQuaThucTe()}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.5_06] Phải ở lại trang đăng nhập khi mật khẩu sai");
        }

        // ============================================
        // NO 31 -> TC_F1.5_07: Báo lỗi khi mật khẩu đúng nhưng sai chữ hoa/thường
        // ============================================
        [Test]
        public void TC_F1_5_07_BaoLoi_SaiChuHoa()
        {
            CurrentTestCaseId = "TC_F1.5_07";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Thread.Sleep(1200);

            bool stillOnPage = _loginPage.IsOnPage();
            string toastMsg = _loginPage.GetToastMessage();
            CurrentActualResult = stillOnPage
                ? $"Đúng: Mật khẩu case-sensitive, ở lại trang login. Toast: '{toastMsg}'"
                : $"Lỗi: Cho phép đăng nhập dù sai chữ hoa/thường. URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.5_07] Mật khẩu phải case-sensitive, phải ở lại trang đăng nhập");
        }

        // ============================================
        // NO 32 -> TC_F1.5_08: Đăng nhập thành công khi Email có chữ hoa lẫn thường (email normalize)
        // ============================================
        [Test]
        public void TC_F1_5_08_DangNhapThanhCong_EmailHoaThuong()
        {
            CurrentTestCaseId = "TC_F1.5_08";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Wait.WaitForUrlNotContains("/Account/Login");

            string currentUrl = Driver.Url;
            CurrentActualResult = $"Đăng nhập thành công dù email có chữ hoa lẫn thường. (URL: {currentUrl})";

            Assert.That(_loginPage.IsLoginSuccessful(), Is.True,
                "[TC_F1.5_08] Hệ thống phải normalize email (case-insensitive), cho phép đăng nhập");
        }

        // ============================================
        // NO 33 -> TC_F1.5_09: Báo lỗi khi đăng nhập với tài khoản bị khóa
        // ============================================
        [Test]
        public void TC_F1_5_09_BaoLoi_TaiKhoanBiKhoa()
        {
            CurrentTestCaseId = "TC_F1.5_09";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Thread.Sleep(1200);

            bool stillOnPage = _loginPage.IsOnPage();
            string toastMsg = _loginPage.GetToastMessage();
            CurrentActualResult = stillOnPage
                ? $"Hệ thống chặn tài khoản bị khóa. Toast: '{toastMsg}'. URL: {Driver.Url}"
                : $"Lỗi bảo mật: Tài khoản bị khóa vẫn đăng nhập được. URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.5_09] Phải chặn và hiển thị lỗi khi tài khoản bị khóa");
        }

        // ============================================
        // NO 34 -> TC_F1.5_10: Báo lỗi khi đăng nhập lần 2 với tài khoản bị khóa (mật khẩu khác)
        // ============================================
        [Test]
        public void TC_F1_5_10_BaoLoi_TaiKhoanBiKhoa_MatKhauKhac()
        {
            CurrentTestCaseId = "TC_F1.5_10";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Thread.Sleep(1200);

            bool stillOnPage = _loginPage.IsOnPage();
            string toastMsg = _loginPage.GetToastMessage();
            CurrentActualResult = stillOnPage
                ? $"Vẫn bị chặn với mật khẩu khác trên tài khoản bị khóa. Toast: '{toastMsg}'. URL: {Driver.Url}"
                : $"Lỗi bảo mật: Tài khoản bị khóa vẫn đăng nhập được. URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.5_10] Phải tiếp tục chặn tài khoản bị khóa dù thử mật khẩu khác");
        }

        // ============================================
        // NO 35 -> TC_F1.5_11: Báo lỗi khi mật khẩu chỉ có 5 ký tự (dưới boundary min)
        // ============================================
        [Test]
        public void TC_F1_5_11_BaoLoi_MatKhau5KyTu()
        {
            CurrentTestCaseId = "TC_F1.5_11";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Thread.Sleep(800);

            string passError = _loginPage.GetPasswordError();
            bool stillOnPage = _loginPage.IsOnPage();
            CurrentActualResult = $"Lỗi field Mật khẩu: '{passError}' | URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.5_11] Phải ở lại trang đăng nhập khi mật khẩu < 6 ký tự");
            Assert.That(passError, Is.Not.Empty,
                "[TC_F1.5_11] Phải hiển thị lỗi khi mật khẩu < 6 ký tự");
        }

        // ============================================
        // NO 36 -> TC_F1.5_12: Báo lỗi định dạng khi Email chứa khoảng trắng
        // ============================================
        [Test]
        public void TC_F1_5_12_BaoLoi_EmailChuaKhoangTrang()
        {
            CurrentTestCaseId = "TC_F1.5_12";
            var data = DocDuLieu(CurrentTestCaseId);

            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Thread.Sleep(800);

            string emailError = _loginPage.GetEmailError();
            bool stillOnPage = _loginPage.IsOnPage();
            CurrentActualResult = $"Lỗi field Email: '{emailError}' | URL: {Driver.Url}";

            Assert.That(stillOnPage, Is.True,
                "[TC_F1.5_12] Phải ở lại trang đăng nhập khi email chứa khoảng trắng");
            Assert.That(emailError, Is.Not.Empty,
                "[TC_F1.5_12] Phải hiển thị lỗi khi email chứa khoảng trắng");
        }

        // ============================================
        // NO 37 -> TC_F1.5_13: Chặn Customer truy cập trang Admin Dashboard
        // ============================================
        [Test]
        public void TC_F1_5_13_ChanCustomer_TruyCapAdmin()
        {
            CurrentTestCaseId = "TC_F1.5_13";
            var data = DocDuLieu(CurrentTestCaseId);

            // Đăng nhập bằng tài khoản Customer
            _loginPage.Open();
            _loginPage.Login(data["email"], data["password"]);
            Wait.WaitForUrlNotContains("/Account/Login");

            // Thử truy cập thẳng trang Admin
            Driver.Navigate().GoToUrl(BaseUrl + data["accessUrl"]);
            Thread.Sleep(1000);

            string currentUrl = Driver.Url;
            bool isBlockedFromAdmin = !currentUrl.Contains("/Admin/Dashboard");
            CurrentActualResult = isBlockedFromAdmin
                ? $"Customer bị chặn truy cập Admin Dashboard. URL hiện tại: {currentUrl}"
                : $"Lỗi bảo mật: Customer truy cập được Admin Dashboard. URL: {currentUrl}";

            Assert.That(isBlockedFromAdmin, Is.True,
                "[TC_F1.5_13] Customer không được phép truy cập /Admin/Dashboard");
        }
    }
}
