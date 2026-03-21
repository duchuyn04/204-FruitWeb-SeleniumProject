using NUnit.Framework;
using SeleniumProject.Pages;
using SeleniumProject.Utilities;
using System.Text.Json;

namespace SeleniumProject.Tests.Auth
{
    [TestFixture]
    public class LoginTests : TestBase
    {
        private LoginPage _loginPage = null!;

        // Đường dẫn file test data
        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "Auth", "login_data.json"
        );

        [SetUp]
        public void SetUpLoginPage()
        {
            // TestBase.SetUp() tự chạy trước (mở Chrome)
            // Sau đó method này chạy tiếp để mở trang login
            _loginPage = new LoginPage(Driver, BaseUrl);
            _loginPage.Open();
        }

        // TC_LOGIN_01 - Case riêng: đăng nhập thành công
        [Test]
        public void TC_LOGIN_01_DangNhapThanhCong()
        {
            var data = DocDuLieu("TC_LOGIN_01");

            _loginPage.Login(data["email"], data["password"]);

            // Check URL redirect về trang chủ
            Assert.That(_loginPage.IsLoginSuccessful(), Is.True,
                "Kỳ vọng: redirect về trang chủ sau khi đăng nhập thành công");

            // Check toast chào mừng — xác nhận login thực sự thành công, không phải false positive
            Assert.That(_loginPage.GetToastMessage(), Does.Contain("Chào mừng"),
                "Kỳ vọng: hiện toast chào mừng tên người dùng");
        }

        // TC_LOGIN_02, 03 - Nhóm: đăng nhập lỗi → hiện toast
        // Dùng TestCaseSource vì 2 test này có Assert giống nhau
        private static IEnumerable<TestCaseData> DuLieuDangNhapToastLoi()
        {
            yield return new TestCaseData("TC_LOGIN_02")
                .SetName("TC_LOGIN_02 - Mật khẩu sai");

            yield return new TestCaseData("TC_LOGIN_03")
                .SetName("TC_LOGIN_03 - Email không tồn tại");
        }

        [TestCaseSource(nameof(DuLieuDangNhapToastLoi))]
        public void TC_LOGIN_DangNhapThatBai_ToastLoi(string testCaseId)
        {
            var data = DocDuLieu(testCaseId);

            _loginPage.Login(data["email"], data["password"]);

            var toastMsg = _loginPage.GetToastMessage();
            Assert.That(toastMsg, Does.Contain("không đúng"),
                $"Kỳ vọng [{testCaseId}]: hiện thông báo lỗi 'không đúng'");
        }

        // TC_LOGIN_04, 05, 06 - Nhóm: đăng nhập lỗi → vẫn ở trang login
        // Dùng TestCaseSource vì 3 test này có Assert giống nhau
        private static IEnumerable<TestCaseData> DuLieuDangNhapVanOrTrang()
        {
            yield return new TestCaseData("TC_LOGIN_04")
                .SetName("TC_LOGIN_04 - Email để trống");

            yield return new TestCaseData("TC_LOGIN_05")
                .SetName("TC_LOGIN_05 - Mật khẩu để trống");

            yield return new TestCaseData("TC_LOGIN_06")
                .SetName("TC_LOGIN_06 - Email sai định dạng");
        }

        [TestCaseSource(nameof(DuLieuDangNhapVanOrTrang))]
        public void TC_LOGIN_DangNhapThatBai_VanOrTrang(string testCaseId)
        {
            var data = DocDuLieu(testCaseId);

            _loginPage.Login(data["email"], data["password"]);

            Assert.That(_loginPage.GetCurrentUrl(), Does.Contain("/Account/Login"),
                $"Kỳ vọng [{testCaseId}]: vẫn ở trang đăng nhập");
        }

        // Hàm đọc dữ liệu từ JSON theo testCase ID
        private Dictionary<string, string> DocDuLieu(string testCaseId)
        {
            string json = File.ReadAllText(DataPath);
            List<Dictionary<string, string>> danhSach = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json)!;

            // Duyệt qua từng phần tử trong danh sách, tìm đúng testCase
            Dictionary<string, string> duLieu = null;
            foreach (Dictionary<string, string> item in danhSach)
            {
                if (item["testCase"] == testCaseId)
                {
                    duLieu = item;
                    break;
                }
            }

            Assert.That(duLieu, Is.Not.Null, $"Không tìm thấy test case '{testCaseId}' trong file JSON");
            return duLieu!;
        }
    }
}
