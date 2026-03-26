using NUnit.Framework;
using SeleniumProject.Pages.Auth;
using SeleniumProject.Pages.ProductManagement;
using SeleniumProject.Utilities;

namespace SeleniumProject.Tests.ProductManagement
{
    // Test class riêng cho các TC kiểm tra quyền truy cập trang Admin
    // Không gọi LoginAsAdmin() trong SetUp để test trường hợp chưa đăng nhập
    [TestFixture]
    public class ProductAccessTests : TestBase
    {
        // TC_F2.5_16 — Truy cập trang Create Product khi chưa đăng nhập
        // Kỳ vọng: bị redirect về trang Login
        [Test]
        public void TC_F2_5_16_TruyCap_KhiChuaDangNhap()
        {
            CurrentTestCaseId = "TC_F2.5_16";
            CurrentSheetName = "TC_Product Management";

            // Không login — truy cập thẳng trang Admin
            var createPage = new CreateProductPage(Driver, BaseUrl);
            createPage.Open();

            // Chờ browser redirect xong
            Wait.WaitForUrlContains("/Account/Login");

            string urlHienTai = Driver.Url;
            CurrentActualResult = $"URL sau khi truy cập không có quyền: {urlHienTai}";

            Assert.That(
                urlHienTai,
                Does.Contain("/Account/Login"),
                "Kỳ vọng: redirect về trang Login khi chưa đăng nhập"
            );
        }

        // TC_F2.5_17 — Tài khoản Customer truy cập trang Admin
        // Kỳ vọng: bị từ chối (403 Forbidden hoặc redirect về trang chủ)
        // Yêu cầu: CustomerEmail và CustomerPassword đã được điền trong appsettings.json
        [Test]
        public void TC_F2_5_17_TruyCap_BangTaiKhoanCustomer()
        {
            CurrentTestCaseId = "TC_F2.5_17";
            CurrentSheetName = "TC_Product Management";

            // Bỏ qua nếu chưa cấu hình tài khoản Customer
            if (string.IsNullOrEmpty(CustomerEmail) || string.IsNullOrEmpty(CustomerPassword))
            {
                Assert.Ignore("Bỏ qua: CustomerEmail/CustomerPassword chưa được cấu hình trong appsettings.json");
                return;
            }

            // Đăng nhập bằng tài khoản Customer
            LoginAsCustomer();

            // Thử truy cập trang Admin Create Product
            var createPage = new CreateProductPage(Driver, BaseUrl);
            createPage.Open();

            string urlHienTai = Driver.Url;
            CurrentActualResult = $"URL sau khi Customer truy cập Admin: {urlHienTai}";

            // Kỳ vọng: không vào được Admin (403, redirect về /, hoặc redirect Login)
            bool bi_chan = !urlHienTai.Contains("/Admin/Product/Create");
            Assert.That(
                bi_chan,
                Is.True,
                "Kỳ vọng: Customer không được phép truy cập trang Admin"
            );
        }
    }
}
