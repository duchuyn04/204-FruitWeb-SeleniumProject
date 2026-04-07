using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumProject.Pages.ProductManagement;
using SeleniumProject.Utilities;

namespace SeleniumProject.Tests.ProductManagement
{
    [TestFixture]
    public class ProductListTests : TestBase
    {
        private ProductListPage _productListPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "ProductManagement", "product_list.json"
        );

        [SetUp]
        public void SetUpPages()
        {
            CurrentSheetName = "TC_Product Management";
            _productListPage = new ProductListPage(Driver, BaseUrl);
            LoginAsAdmin();
        }

        // STT 39 - TC_F2.5_01: State Transition: đổi category → kết quả cập nhật
        [Test]
        public void TC_F2_5_01_StateTransition_DoiCategory()
        {
            CurrentTestCaseId = "TC_F2.5_01";
            var data = DocDuLieu("TC_F2.5_01");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(800);

            // Chọn category 1
            _productListPage.SelectCategory(data["categoryFilter1"]);
            Thread.Sleep(1000);
            int soLuong1 = _productListPage.GetProductRowCount();

            // Đổi sang category 2
            _productListPage.SelectCategory(data["categoryFilter2"]);
            Thread.Sleep(1000);
            int soLuong2 = _productListPage.GetProductRowCount();

            CurrentActualResult = $"Category 1: {soLuong1} SP | Category 2: {soLuong2} SP";

            Assert.That(_productListPage.IsPageHealthy(), Is.True,
                "[TC_F2.5_01] Trang phải hoạt động bình thường sau khi đổi category");
        }

        // STT 40 - TC_F2.5_02: Layout không vỡ khi 0 kết quả
        [Test]
        public void TC_F2_5_02_LayoutKhongVoKhi0KetQua()
        {
            CurrentTestCaseId = "TC_F2.5_02";
            var data = DocDuLieu("TC_F2.5_02");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            int soLuong = _productListPage.GetProductRowCount();
            bool trangKhoe = _productListPage.IsPageHealthy();

            bool coSidebar = Driver.FindElements(By.CssSelector(".sidebar, nav, .nav")).Count > 0;
            bool coHeader = Driver.FindElements(By.CssSelector("header, .navbar, .top-bar")).Count > 0;

            CurrentActualResult = $"Số SP: {soLuong} | Trang khỏe: {trangKhoe} | Sidebar: {coSidebar} | Header: {coHeader}";

            Assert.That(trangKhoe, Is.True,
                "[TC_F2.5_02] Trang phải hoạt động bình thường khi 0 kết quả");
            Assert.That(soLuong, Is.EqualTo(0),
                "[TC_F2.5_02] Phải hiển thị 0 sản phẩm");
        }

        // STT 41 - TC_F2.5_03: Look & Feel: Dropdown Category & Sort hiển thị giá trị mặc định đúng
        [Test]
        public void TC_F2_5_03_GiaTriMacDinh_DropdownCategorySort()
        {
            CurrentTestCaseId = "TC_F2.5_03";
            var data = DocDuLieu("TC_F2.5_03");

            _productListPage.Open();
            Thread.Sleep(500);

            string categoryVal = _productListPage.GetCurrentCategoryValue();
            string sortVal = _productListPage.GetCurrentSortValue();

            CurrentActualResult = $"Category: '{categoryVal}' | Sort: '{sortVal}'";

            Assert.That(categoryVal, Is.EqualTo(data["expectedCategory"]),
                "[TC_F2.5_03] Category mặc định không đúng");
            Assert.That(sortVal, Is.EqualTo(data["expectedSort"]),
                "[TC_F2.5_03] Sort mặc định không đúng");
        }

        private Dictionary<string, string> DocDuLieu(string testCaseId)
        {
            return JsonHelper.DocDuLieu(DataPath, testCaseId);
        }
    }
}
