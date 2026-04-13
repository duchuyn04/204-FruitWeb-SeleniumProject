using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumProject.Pages.ProfileManagement;
using SeleniumProject.Utilities;

namespace SeleniumProject.Tests.ProfileManagement
{
    [TestFixture]
    public class ProfileViewTests : TestBase
    {
        private ProfilePage _profilePage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "ProfileManagement", "profile_data.json"
        );

        [SetUp]
        public void SetUpProfilePage()
        {
            CurrentSheetName = "TC_ProfileManagement";
            _profilePage = new ProfilePage(Driver);
        }

        // =========================================================
        // F6.1 – Xem Profile
        // =========================================================

        [Test]
        public void TC_PROFILE_F6_1_01_XemThongTinProfile()
        {
            CurrentTestCaseId = "TC_F6.1_01";
            var data = DocDuLieu("TC_PROFILE_F6_1_01");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenProfile();

            string name = _profilePage.GetProfileName();
            string email = _profilePage.GetProfileEmail();
            string phone = _profilePage.GetProfilePhone();
            string displayName = _profilePage.GetDisplayName();

            bool hasName = !string.IsNullOrEmpty(name);
            bool hasEmail = !string.IsNullOrEmpty(email) && email.Contains("@");

            CurrentActualResult = hasName && hasEmail
                ? $"Profile hiển thị đúng. Name='{name}', Email='{email}', Phone='{phone}', DisplayName='{displayName}'."
                : $"Profile thiếu thông tin. Name='{name}', Email='{email}'.";

            Assert.That(hasName, Is.True, "[F6.1_01] Phải hiển thị tên người dùng");
            Assert.That(hasEmail, Is.True, "[F6.1_01] Phải hiển thị email hợp lệ");
        }

        [Test]
        public void TC_PROFILE_F6_1_02_XemProfileKhiChuaCoPhone()
        {
            CurrentTestCaseId = "TC_F6.1_02";
            var data = DocDuLieu("TC_PROFILE_F6_1_02");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenProfile();

            string phone = _profilePage.GetProfilePhone();
            string body = _profilePage.GetBodyText();

            // Phone trống hoặc hiển thị "Chưa cập nhật"
            bool phoneEmptyOrPlaceholder = string.IsNullOrWhiteSpace(phone)
                || phone.Contains("Chưa cập nhật")
                || phone.Contains("Chưa có");

            CurrentActualResult = phoneEmptyOrPlaceholder
                ? $"Phone trống hoặc placeholder: '{phone}'."
                : $"Phone hiển thị: '{phone}'.";

            Assert.That(phoneEmptyOrPlaceholder, Is.True,
                $"[F6.1_02] Phone trống phải hiển thị placeholder hoặc trống. Actual: '{phone}'");
        }

        [Test]
        public void TC_PROFILE_F6_1_03_HienThiAvatarMacDinh()
        {
            CurrentTestCaseId = "TC_F6.1_03";
            var data = DocDuLieu("TC_PROFILE_F6_1_03");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenProfile();

            string avatarSrc = _profilePage.GetAvatarSrc();

            bool hasAvatar = !string.IsNullOrEmpty(avatarSrc);
            bool isDefault = avatarSrc.Contains("default") || avatarSrc.Contains("avatar")
                || avatarSrc.Contains("placeholder") || avatarSrc.Contains("noavatar");

            CurrentActualResult = hasAvatar
                ? $"Avatar hiển thị: '{avatarSrc}'. Là avatar mặc định: {isDefault}."
                : "KHÔNG thấy avatar.";

            Assert.That(hasAvatar, Is.True,
                "[F6.1_03] Phải hiển thị avatar (mặc định hoặc đã upload)");
        }

        [Test]
        public void TC_PROFILE_F6_1_04_HienThiAvatarDaUpload()
        {
            CurrentTestCaseId = "TC_F6.1_04";
            var data = DocDuLieu("TC_PROFILE_F6_1_04");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenProfile();

            string avatarSrc = _profilePage.GetAvatarSrc();
            bool hasAvatar = !string.IsNullOrEmpty(avatarSrc);

            // Kiểm tra ảnh có load thành công (naturalWidth > 0)
            bool imageLoaded = _profilePage.IsAvatarImageLoaded();

            CurrentActualResult = hasAvatar && imageLoaded
                ? $"Avatar đã upload hiển thị đúng. Src='{avatarSrc}'."
                : $"Avatar không hiển thị. Src='{avatarSrc}', Loaded={imageLoaded}.";

            Assert.That(hasAvatar, Is.True,
                "[F6.1_04] Phải hiển thị avatar đã upload");
            Assert.That(imageLoaded, Is.True,
                "[F6.1_04] Ảnh avatar phải load thành công");
        }

        [Test]
        public void TC_PROFILE_F6_1_05_TruyCApKhiChuaDangNhap()
        {
            CurrentTestCaseId = "TC_F6.1_05";
            // Không đăng nhập, truy cập trực tiếp /Profile
            _profilePage.OpenProfile();

            bool isRedirected = _profilePage.IsOnLoginPage()
                || _profilePage.GetCurrentUrl().Contains("/Account/Login");

            CurrentActualResult = isRedirected
                ? $"Redirect đến trang Login: {_profilePage.GetCurrentUrl()}."
                : $"KHÔNG redirect. URL: {_profilePage.GetCurrentUrl()}.";

            Assert.That(isRedirected, Is.True,
                $"[F6.1_05] Phải redirect về Login khi chưa đăng nhập. URL: {_profilePage.GetCurrentUrl()}");
        }

        [Test]
        public void TC_PROFILE_F6_1_06_EmailReadOnlyTrenFormEdit()
        {
            CurrentTestCaseId = "TC_F6.1_06";
            var data = DocDuLieu("TC_PROFILE_F6_1_06");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            bool isDisabled = _profilePage.IsEmailDisabled();

            CurrentActualResult = isDisabled
                ? "Email hiển thị read-only (disabled) trên form Edit."
                : "Email KHÔNG bị disabled trên form Edit.";

            Assert.That(isDisabled, Is.True,
                "[F6.1_06] Email phải là read-only trên form Edit");
        }

        [Test]
        public void TC_PROFILE_F6_1_07_HienThiNgayTaoTaiKhoan()
        {
            CurrentTestCaseId = "TC_F6.1_07";
            var data = DocDuLieu("TC_PROFILE_F6_1_07");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenProfile();

            string memberSince = _profilePage.GetMemberSinceText();

            bool hasMemberSince = !string.IsNullOrEmpty(memberSince)
                && (memberSince.Contains("Thành viên từ") || memberSince.Contains("/"));

            CurrentActualResult = hasMemberSince
                ? $"Ngày tạo tài khoản hiển thị: '{memberSince}'."
                : $"KHÔNG thấy ngày tạo tài khoản. Text: '{memberSince}'.";

            Assert.That(hasMemberSince, Is.True,
                $"[F6.1_07] Phải hiển thị ngày tạo tài khoản. Actual: '{memberSince}'");
        }

        [Test]
        public void TC_PROFILE_F6_1_08_TruyCApEditKhiChuaDangNhap()
        {
            CurrentTestCaseId = "TC_F6.1_08";
            _profilePage.OpenEdit();

            bool isRedirected = _profilePage.IsOnLoginPage()
                || _profilePage.GetCurrentUrl().Contains("/Account/Login");

            CurrentActualResult = isRedirected
                ? $"Redirect đến Login: {_profilePage.GetCurrentUrl()}."
                : $"KHÔNG redirect. URL: {_profilePage.GetCurrentUrl()}.";

            Assert.That(isRedirected, Is.True,
                $"[F6.1_08] Phải redirect về Login khi chưa đăng nhập. URL: {_profilePage.GetCurrentUrl()}");
        }

        private Dictionary<string, string> DocDuLieu(string testCaseId)
        {
            return JsonHelper.DocDuLieu(DataPath, testCaseId);
        }
    }
}
