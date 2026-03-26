using NUnit.Framework;
using SeleniumProject.Pages.ProductManagement;
using SeleniumProject.Utilities;

namespace SeleniumProject.Tests.ProductManagement
{
    // ---------------------------------------------------------------
    // Class này dùng SHARED DRIVER — login 1 lần cho cả fixture
    // → Nhanh hơn nhiều so với login lại mỗi test
    // Phù hợp cho các test validation (bỏ trống field) vì chúng
    //   không tạo sản phẩm, không thay đổi state của app
    // ---------------------------------------------------------------
    [TestFixture]
    public class ProductValidationTests : TestBase
    {
        private CreateProductPage _createPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "ProductManagement", "product_data.json"
        );

        // Chạy 1 lần duy nhất trước tất cả test trong class này
        [OneTimeSetUp]
        public void OneTimeSetUpSharedDriver()
        {
            // Bật cờ — TestBase.SetUp và TearDown sẽ KHÔNG tạo mới / quit driver
            UseSharedDriver = true;

            // Khởi tạo driver và login 1 lần
            InitSharedDriver();
            CurrentSheetName = "TC_Product Management";
            LoginAsAdmin();
        }

        // Chạy 1 lần sau khi tất cả test xong — đóng browser
        [OneTimeTearDown]
        public void OneTimeTearDownSharedDriver()
        {
            Driver?.Quit();
            Driver?.Dispose();
        }

        // Trước mỗi test: mở lại trang Create để có form trống
        [SetUp]
        public new void SetUp()
        {
            // Gọi base.SetUp() — nhưng vì UseSharedDriver=true, nó sẽ không tạo driver mới
            base.SetUp();
            _createPage = new CreateProductPage(Driver, BaseUrl);
            _createPage.Open();
        }

        // ==============================================================
        // TC_F2.5_09 — Để trống Tên sản phẩm
        // ==============================================================
        [Test]
        public void TC_F2_5_09_DeTrong_TenSanPham()
        {
            CurrentTestCaseId = "TC_F2.5_09";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.5_09");

            // Không nhập tên — điền phần còn lại
            _createPage.SelectCategory(data["category"]);
            _createPage.EnterPrice(data["price"]);

            _createPage.ClickSave();

            CurrentActualResult = _createPage.DocKetQuaThucTe();

            Assert.That(
                !_createPage.IsRedirectedToList(),
                Is.True,
                "Kỳ vọng: không redirect khi để trống Tên sản phẩm"
            );
        }

        // ==============================================================
        // Nhóm: Lỗi Validation khi lưu
        // Gồm TC_F2.5_08, 10, 11, 12, 13, 19 — cùng pattern:
        //   fill (có/không) → ClickSave → vẫn ở trang, hiển thị lỗi
        // ==============================================================
        private static IEnumerable<TestCaseData> LoiValidationKhiLuu()
        {
            yield return new TestCaseData("TC_F2.5_08")
                .SetName("TC_F2.5_08 - Submit form rỗng hoàn toàn");

            yield return new TestCaseData("TC_F2.5_10")
                .SetName("TC_F2.5_10 - Không chọn Danh mục");

            yield return new TestCaseData("TC_F2.5_11")
                .SetName("TC_F2.5_11 - Để trống Giá gốc");

            yield return new TestCaseData("TC_F2.5_12")
                .SetName("TC_F2.5_12 - Giá khuyến mãi lớn hơn Giá gốc");

            yield return new TestCaseData("TC_F2.5_13")
                .SetName("TC_F2.5_13 - Nhập Giá gốc âm");

            yield return new TestCaseData("TC_F2.5_19")
                .SetName("TC_F2.5_19 - Tên sản phẩm vượt max length");
        }

        [TestCaseSource(nameof(LoiValidationKhiLuu))]
        public void TC_KiemTraLoiValidation(string testCaseId)
        {
            CurrentTestCaseId = testCaseId;

            var data = JsonHelper.DocDuLieu(DataPath, testCaseId);

            if (!string.IsNullOrEmpty(data["productName"]))
                _createPage.EnterName(data["productName"]);

            if (!string.IsNullOrEmpty(data["category"]))
                _createPage.SelectCategory(data["category"]);

            if (!string.IsNullOrEmpty(data["price"]))
                _createPage.EnterPrice(data["price"]);

            if (!string.IsNullOrEmpty(data["discountPrice"]))
                _createPage.EnterSalePrice(data["discountPrice"]);

            _createPage.ClickSave();

            CurrentActualResult = _createPage.DocKetQuaThucTe();

            Assert.That(
                !_createPage.IsRedirectedToList(),
                Is.True,
                $"Kỳ vọng [{testCaseId}]: không redirect khi có lỗi validation"
            );

            Assert.That(
                _createPage.HasValidationErrors(),
                Is.True,
                $"Kỳ vọng [{testCaseId}]: hiển thị thông báo lỗi trên form"
            );
        }
    }
}
