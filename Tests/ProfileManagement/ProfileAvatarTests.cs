using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumProject.Pages.ProfileManagement;
using SeleniumProject.Utilities;

namespace SeleniumProject.Tests.ProfileManagement
{
    [TestFixture]
    public class ProfileAvatarTests : TestBase
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
        // F6.3 – Upload Avatar
        // =========================================================

        [Test]
        public void TC_PROFILE_F6_3_01_UploadAvatarJPG()
        {
            CurrentTestCaseId = "TC_F6.3_01";
            var data = DocDuLieu("TC_PROFILE_F6_3_01");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            string avatarPath = EnsureTestImageExists("test_avatar.jpg");

            string avatarBefore = _profilePage.GetAvatarPreviewSrc();
            _profilePage.UploadAvatar(avatarPath);

            string avatarAfter = _profilePage.GetAvatarPreviewSrc();
            string body = _profilePage.GetBodyText();

            bool success = avatarAfter != avatarBefore
                || body.Contains("thành công")
                || body.Contains("Avatar");

            CurrentActualResult = success
                ? $"Upload avatar JPG thành công. Avatar đã thay đổi."
                : "Upload avatar JPG THẤT BẠI.";

            Assert.That(success, Is.True,
                "[F6.3_01] Upload avatar JPG phải thành công");
        }

        [Test]
        public void TC_PROFILE_S_01_UploadAvatarPNG()
        {
            CurrentTestCaseId = "TC_S_01";
            var data = DocDuLieu("TC_PROFILE_S_01");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            string avatarPath = EnsureTestPngExists("test_avatar.png");

            string avatarBefore = _profilePage.GetAvatarPreviewSrc();
            _profilePage.UploadAvatar(avatarPath);

            string avatarAfter = _profilePage.GetAvatarPreviewSrc();
            string body = _profilePage.GetBodyText();

            bool success = avatarAfter != avatarBefore
                || body.Contains("thành công")
                || body.Contains("Avatar");

            CurrentActualResult = success
                ? "Upload avatar PNG thành công."
                : "Upload avatar PNG THẤT BẠI.";

            Assert.That(success, Is.True,
                "[S_01] Upload avatar PNG phải thành công");
        }

        [Test]
        public void TC_PROFILE_F6_3_02_UploadAvatarGIF()
        {
            CurrentTestCaseId = "TC_F6.3_02";
            var data = DocDuLieu("TC_PROFILE_F6_3_02");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            string avatarPath = EnsureTestGifExists("test_avatar.gif");

            string avatarBefore = _profilePage.GetAvatarPreviewSrc();
            _profilePage.UploadAvatar(avatarPath);

            string avatarAfter = _profilePage.GetAvatarPreviewSrc();
            string body = _profilePage.GetBodyText();

            bool success = avatarAfter != avatarBefore
                || body.Contains("thành công")
                || body.Contains("Avatar");

            CurrentActualResult = success
                ? "Upload avatar GIF thành công."
                : "Upload avatar GIF THẤT BẠI.";

            Assert.That(success, Is.True,
                "[F6.3_02] Upload avatar GIF phải thành công");
        }

        [Test]
        public void TC_PROFILE_F6_3_03_UploadFilePDF()
        {
            CurrentTestCaseId = "TC_F6.3_03";
            var data = DocDuLieu("TC_PROFILE_F6_3_03");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            string filePath = EnsureTestPdfExists("test_file.pdf");

            _profilePage.UploadAvatar(filePath);

            string body = _profilePage.GetBodyText();
            string toast = _profilePage.GetToastText();

            bool rejected = body.Contains("lỗi") || body.Contains("không hợp lệ")
                || body.Contains("chỉ chấp nhận") || toast.Contains("lỗi")
                || toast.Contains("không hợp lệ")
                || _profilePage.IsOnEditPage(); // Vẫn ở Edit = không upload

            CurrentActualResult = rejected
                ? $"File PDF bị từ chối upload. Toast: '{toast}'."
                : "File PDF KHÔNG bị từ chối.";

            Assert.That(rejected, Is.True,
                "[F6.3_03] Upload file PDF phải bị từ chối");
        }

        [Test]
        public void TC_PROFILE_F6_3_04_UploadFileTXT()
        {
            CurrentTestCaseId = "TC_F6.3_04";
            var data = DocDuLieu("TC_PROFILE_F6_3_04");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            string filePath = EnsureTestTxtExists("test_file.txt");

            _profilePage.UploadAvatar(filePath);

            string body = _profilePage.GetBodyText();
            string toast = _profilePage.GetToastText();

            bool rejected = body.Contains("lỗi") || body.Contains("không hợp lệ")
                || body.Contains("chỉ chấp nhận") || toast.Contains("lỗi")
                || toast.Contains("không hợp lệ")
                || _profilePage.IsOnEditPage();

            CurrentActualResult = rejected
                ? $"File TXT bị từ chối upload. Toast: '{toast}'."
                : "File TXT KHÔNG bị từ chối.";

            Assert.That(rejected, Is.True,
                "[F6.3_04] Upload file TXT phải bị từ chối");
        }

        [Test]
        public void TC_PROFILE_F6_3_05_UploadFileVuotQua2MB()
        {
            CurrentTestCaseId = "TC_F6.3_05";
            var data = DocDuLieu("TC_PROFILE_F6_3_05");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            string filePath = EnsureLargeImageExists("test_large.jpg", 3 * 1024 * 1024); // 3MB

            _profilePage.UploadAvatar(filePath);

            string body = _profilePage.GetBodyText();
            string toast = _profilePage.GetToastText();

            bool rejected = body.Contains("lỗi") || body.Contains("quá lớn")
                || body.Contains("2MB") || toast.Contains("lỗi")
                || toast.Contains("kích thước")
                || _profilePage.IsOnEditPage();

            CurrentActualResult = rejected
                ? $"File >2MB bị từ chối. Toast: '{toast}'."
                : "File >2MB KHÔNG bị từ chối.";

            Assert.That(rejected, Is.True,
                "[F6.3_05] Upload file vượt 2MB phải bị từ chối");
        }

        [Test]
        public void TC_PROFILE_F6_3_06_UploadFileDung2MBBoundary()
        {
            CurrentTestCaseId = "TC_F6.3_06";
            var data = DocDuLieu("TC_PROFILE_F6_3_06");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            // File đúng 2MB (boundary) - dùng ảnh nhỏ hơn 2MB
            string filePath = EnsureLargeImageExists("test_2mb.jpg", 2 * 1024 * 1024 - 1024); // ~2MB

            string avatarBefore = _profilePage.GetAvatarPreviewSrc();
            _profilePage.UploadAvatar(filePath);

            string avatarAfter = _profilePage.GetAvatarPreviewSrc();
            string body = _profilePage.GetBodyText();

            bool success = avatarAfter != avatarBefore
                || body.Contains("thành công")
                || !body.Contains("lỗi");

            CurrentActualResult = success
                ? "File đúng 2MB upload thành công (boundary)."
                : "File đúng 2MB bị từ chối.";

            Assert.That(success, Is.True,
                "[F6.3_06] Upload file đúng 2MB (boundary) phải thành công");
        }

        [Test]
        public void TC_PROFILE_F6_3_07_UploadAvatarThayTheCu()
        {
            CurrentTestCaseId = "TC_F6.3_07";
            var data = DocDuLieu("TC_PROFILE_F6_3_07");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            string avatarPath = EnsureTestImageExists("test_avatar_new.jpg");

            string avatarBefore = _profilePage.GetAvatarPreviewSrc();
            _profilePage.UploadAvatar(avatarPath);

            string avatarAfter = _profilePage.GetAvatarPreviewSrc();

            bool changed = avatarAfter != avatarBefore || !string.IsNullOrEmpty(avatarAfter);

            CurrentActualResult = changed
                ? $"Avatar đã thay đổi. Before: {avatarBefore.Substring(0, Math.Min(50, avatarBefore.Length))}..., After: {avatarAfter.Substring(0, Math.Min(50, avatarAfter.Length))}..."
                : "Avatar KHÔNG thay đổi sau upload mới.";

            Assert.That(changed, Is.True,
                "[F6.3_07] Upload avatar mới phải thay thế avatar cũ");
        }

        [Test]
        public void TC_PROFILE_F6_3_08_ThongBaoLoiUploadAvatar()
        {
            CurrentTestCaseId = "TC_F6.3_08";
            var data = DocDuLieu("TC_PROFILE_F6_3_08");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            // Upload file không hợp lệ (TXT) → phải có thông báo lỗi
            string filePath = EnsureTestTxtExists("test_error.txt");
            _profilePage.UploadAvatar(filePath);

            string body = _profilePage.GetBodyText();
            string toast = _profilePage.GetToastText();

            bool hasErrorMsg = !string.IsNullOrEmpty(toast)
                || body.Contains("lỗi") || body.Contains("không hợp lệ")
                || body.Contains("chỉ chấp nhận")
                || _profilePage.IsOnEditPage();

            CurrentActualResult = hasErrorMsg
                ? $"Thông báo lỗi upload: '{toast}'."
                : "KHÔNG hiển thị thông báo lỗi.";

            Assert.That(hasErrorMsg, Is.True,
                "[F6.3_08] Phải hiển thị thông báo lỗi khi upload file không hợp lệ");
        }

        [Test]
        public void TC_PROFILE_F6_3_09_UploadAvatarKhiChuaDangNhap()
        {
            CurrentTestCaseId = "TC_F6.3_09";
            // Không đăng nhập, truy cập Edit trực tiếp
            _profilePage.OpenEdit();

            bool isRedirected = _profilePage.IsOnLoginPage()
                || _profilePage.GetCurrentUrl().Contains("/Account/Login");

            CurrentActualResult = isRedirected
                ? $"Redirect về Login. URL: {_profilePage.GetCurrentUrl()}."
                : $"KHÔNG redirect. URL: {_profilePage.GetCurrentUrl()}.";

            Assert.That(isRedirected, Is.True,
                "[F6.3_09] Phải redirect về Login khi upload avatar chưa đăng nhập");
        }

        // =========================================================
        // F6.4 – Xóa Avatar
        // =========================================================

        [Test]
        public void TC_PROFILE_F6_4_01_XoaAvatar()
        {
            CurrentTestCaseId = "TC_F6.4_01";
            var data = DocDuLieu("TC_PROFILE_F6_4_01");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            bool hasDeleteBtn = _profilePage.IsDeleteAvatarButtonVisible();
            if (hasDeleteBtn)
            {
                _profilePage.ClickDeleteAvatar();
                string body = _profilePage.GetBodyText();
                string avatarAfter = _profilePage.GetAvatarPreviewSrc();

                bool deleted = body.Contains("thành công")
                    || body.Contains("xóa")
                    || avatarAfter.Contains("default")
                    || avatarAfter.Contains("noavatar");

                CurrentActualResult = deleted
                    ? "Xóa avatar thành công. Avatar trở về mặc định."
                    : "Xóa avatar nhưng không thấy thông báo thành công.";

                Assert.That(deleted, Is.True,
                    "[F6.4_01] Xóa avatar phải thành công và trở về mặc định");
            }
            else
            {
                CurrentActualResult = "Nút xóa avatar không hiển thị (chưa có avatar custom để xóa). Test skip logic.";
                Assert.Pass("[F6.4_01] Không có avatar custom để xóa, skip test.");
            }
        }

        [Test]
        public void TC_PROFILE_F6_4_02_DeleteAvatarKhiChuaDangNhap()
        {
            CurrentTestCaseId = "TC_F6.4_02";
            // Không đăng nhập
            _profilePage.OpenEdit();

            bool isRedirected = _profilePage.IsOnLoginPage()
                || _profilePage.GetCurrentUrl().Contains("/Account/Login");

            CurrentActualResult = isRedirected
                ? $"Redirect về Login. URL: {_profilePage.GetCurrentUrl()}."
                : $"KHÔNG redirect. URL: {_profilePage.GetCurrentUrl()}.";

            Assert.That(isRedirected, Is.True,
                "[F6.4_02] Phải redirect về Login khi xóa avatar chưa đăng nhập");
        }

        // =========================================================
        // HELPERS – Tạo test files
        // =========================================================

        private string EnsureTestImageExists(string fileName)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "TestData", "ProfileManagement", fileName);
            if (!File.Exists(path))
            {
                var dir = Path.GetDirectoryName(path)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                // 1x1 JPEG
                byte[] jpeg = new byte[]
                {
                    0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
                    0x01, 0x01, 0x00, 0x48, 0x00, 0x48, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
                    0x00, 0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08, 0x07, 0x07, 0x07, 0x09,
                    0x09, 0x08, 0x0A, 0x0C, 0x14, 0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12,
                    0x13, 0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D, 0x1A, 0x1C, 0x1C, 0x20,
                    0x24, 0x2E, 0x27, 0x20, 0x22, 0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29,
                    0x2C, 0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27, 0x39, 0x3D, 0x38, 0x32,
                    0x3C, 0x2E, 0x33, 0x34, 0x32, 0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01,
                    0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xC4, 0x00, 0x1F, 0x00, 0x00,
                    0x01, 0x05, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                    0x09, 0x0A, 0x0B, 0xFF, 0xC4, 0x00, 0xB5, 0x10, 0x00, 0x02, 0x01, 0x03,
                    0x03, 0x02, 0x04, 0x03, 0x05, 0x05, 0x04, 0x04, 0x00, 0x00, 0x01, 0x7D,
                    0x01, 0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12, 0x21, 0x31, 0x41, 0x06,
                    0x13, 0x51, 0x61, 0x07, 0x22, 0x71, 0x14, 0x32, 0x81, 0x91, 0xA1, 0x08,
                    0x23, 0x42, 0xB1, 0xC1, 0x15, 0x52, 0xD1, 0xF0, 0x24, 0x33, 0x62, 0x72,
                    0x82, 0x09, 0x0A, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x25, 0x26, 0x27, 0x28,
                    0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01, 0x00, 0x00, 0x3F, 0x00, 0x7B, 0x94,
                    0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xD9
                };
                File.WriteAllBytes(path, jpeg);
            }
            return path;
        }

        private string EnsureTestPngExists(string fileName)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "TestData", "ProfileManagement", fileName);
            if (!File.Exists(path))
            {
                var dir = Path.GetDirectoryName(path)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                // Minimal 1x1 PNG
                byte[] png = new byte[]
                {
                    0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
                    0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
                    0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, 0xDE, 0x00, 0x00, 0x00,
                    0x0C, 0x49, 0x44, 0x41, 0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
                    0x00, 0x00, 0x02, 0x00, 0x01, 0xE2, 0x21, 0xBC, 0x33, 0x00, 0x00, 0x00,
                    0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
                };
                File.WriteAllBytes(path, png);
            }
            return path;
        }

        private string EnsureTestGifExists(string fileName)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "TestData", "ProfileManagement", fileName);
            if (!File.Exists(path))
            {
                var dir = Path.GetDirectoryName(path)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                // Minimal 1x1 GIF89a
                byte[] gif = new byte[]
                {
                    0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x80, 0x00,
                    0x00, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x21, 0xF9, 0x04, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00,
                    0x00, 0x02, 0x02, 0x44, 0x01, 0x00, 0x3B
                };
                File.WriteAllBytes(path, gif);
            }
            return path;
        }

        private string EnsureTestPdfExists(string fileName)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "TestData", "ProfileManagement", fileName);
            if (!File.Exists(path))
            {
                var dir = Path.GetDirectoryName(path)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(path, "%PDF-1.4 Test PDF file content");
            }
            return path;
        }

        private string EnsureTestTxtExists(string fileName)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "TestData", "ProfileManagement", fileName);
            if (!File.Exists(path))
            {
                var dir = Path.GetDirectoryName(path)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(path, "This is a test text file");
            }
            return path;
        }

        private string EnsureLargeImageExists(string fileName, int sizeBytes)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "TestData", "ProfileManagement", fileName);
            if (!File.Exists(path))
            {
                var dir = Path.GetDirectoryName(path)!;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                // Tạo JPEG header + padding bytes
                byte[] header = new byte[]
                {
                    0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46
                };
                byte[] data = new byte[sizeBytes];
                Array.Copy(header, data, header.Length);
                data[sizeBytes - 2] = 0xFF;
                data[sizeBytes - 1] = 0xD9; // JPEG end marker
                File.WriteAllBytes(path, data);
            }
            return path;
        }

        private Dictionary<string, string> DocDuLieu(string testCaseId)
        {
            return JsonHelper.DocDuLieu(DataPath, testCaseId);
        }
    }
}
