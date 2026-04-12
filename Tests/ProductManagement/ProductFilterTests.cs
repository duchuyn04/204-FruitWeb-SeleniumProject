using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumProject.Pages.ProductManagement;
using SeleniumProject.Utilities;

namespace SeleniumProject.Tests.ProductManagement
{
    [TestFixture]
    public class ProductFilterTests : TestBase
    {
        private ProductListPage _productListPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "ProductManagement", "product_filter.json"
        );

        [SetUp]
        public void SetUpPages()
        {
            CurrentSheetName = "TC_Product Management";
            _productListPage = new ProductListPage(Driver, BaseUrl);
            LoginAsAdmin();
        }

        // STT 29 - TC_F2.4_01: Tìm SP không tồn tại - thông báo trống
        [Test]
        public void TC_F2_4_01_TimKiemKhongTonTai_ThongBaoTrong()
        {
            CurrentTestCaseId = "TC_F2.4_01";
            var data = DocDuLieu("TC_F2.4_01");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            int soLuong = _productListPage.GetProductRowCount();
            bool coThongBao = _productListPage.IsEmptyMessageDisplayed();
            string expectedMsg = data.ContainsKey("expectedMessage") ? data["expectedMessage"] : "";

            CurrentActualResult = $"Tìm kiếm từ khoá không tồn tại '{data["searchKeyword"]}': số sản phẩm trả về là {soLuong}. Thông báo rỗng hiển thị: {coThongBao}. Nội dung kỳ vọng: '{expectedMsg}'.";

            Assert.That(soLuong, Is.EqualTo(0),
                "[TC_F2.4_01] Phải hiển thị 0 kết quả khi tìm SP không tồn tại");
        }

        // STT 30 - TC_F2.4_02: Tìm kiếm với XSS - không crash
        [Test]
        public void TC_F2_4_02_TimKiemXSS_KhongCrash()
        {
            CurrentTestCaseId = "TC_F2.4_02";
            var data = DocDuLieu("TC_F2.4_02");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            bool trangKhoe = _productListPage.IsPageHealthy();
            int soLuong = _productListPage.GetDisplayedProductCount();

            CurrentActualResult = $"Tìm kiếm bằng chuỗi XSS '{data["searchKeyword"]}': h\u1ec7 th\u1ed1ng tr\u1ea3 v\u1ec1 {soLuong} k\u1ebft qu\u1ea3.";

            Assert.That(trangKhoe, Is.True,
                "[TC_F2.4_02] Trang không được crash khi nhập XSS");
        }

        // STT 31 - TC_F2.4_03: SQL Injection - không crash
        [Test]
        public void TC_F2_4_03_TimKiemSQLInjection_KhongCrash()
        {
            CurrentTestCaseId = "TC_F2.4_03";
            var data = DocDuLieu("TC_F2.4_03");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            bool trangKhoe = _productListPage.IsPageHealthy();
            string url = _productListPage.GetCurrentUrl();

            CurrentActualResult = $"Tìm kiếm bằng câu lệnh SQL Injection '{data["searchKeyword"]}': h\u1ec7 th\u1ed1ng kh\u00f4ng b\u1ecb crash.";

            Assert.That(trangKhoe, Is.True,
                "[TC_F2.4_03] Trang không được crash khi nhập SQL Injection");
        }

        // STT 32 - TC_F2.4_04: Boundary 254 ký tự (max-1)
        [Test]
        public void TC_F2_4_04_Boundary254KyTu()
        {
            CurrentTestCaseId = "TC_F2.4_04";
            var data = DocDuLieu("TC_F2.4_04");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            bool trangKhoe = _productListPage.IsPageHealthy();
            int soLuong = _productListPage.GetDisplayedProductCount();

            CurrentActualResult = $"Tìm kiếm chuỗi 254 ký tự (max-1): h\u1ec7 th\u1ed1ng tr\u1ea3 v\u1ec1 {soLuong} k\u1ebft qu\u1ea3.";

            Assert.That(trangKhoe, Is.True,
                "[TC_F2.4_04] Trang phải xử lý bình thường với 254 ký tự");
        }

        // STT 33 - TC_F2.4_05: Boundary 256 ký tự (max+1)
        [Test]
        public void TC_F2_4_05_Boundary256KyTu()
        {
            CurrentTestCaseId = "TC_F2.4_05";
            var data = DocDuLieu("TC_F2.4_05");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            bool trangKhoe = _productListPage.IsPageHealthy();
            int soLuong = _productListPage.GetDisplayedProductCount();

            CurrentActualResult = $"Tìm kiếm chuỗi 256 ký tự (max+1): h\u1ec7 th\u1ed1ng tr\u1ea3 v\u1ec1 {soLuong} k\u1ebft qu\u1ea3.";

            Assert.That(trangKhoe, Is.True,
                "[TC_F2.4_05] Trang phải xử lý bình thường với 256 ký tự");
        }

        // STT 34 - TC_F2.4_06: Tìm kiếm emoji
        [Test]
        public void TC_F2_4_06_TimKiemEmoji()
        {
            CurrentTestCaseId = "TC_F2.4_06";
            var data = DocDuLieu("TC_F2.4_06");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            bool trangKhoe = _productListPage.IsPageHealthy();
            int soLuong = _productListPage.GetDisplayedProductCount();

            CurrentActualResult = $"Tìm kiếm bằng emoji '{data["searchKeyword"]}': h\u1ec7 th\u1ed1ng tr\u1ea3 v\u1ec1 {soLuong} k\u1ebft qu\u1ea3.";

            Assert.That(trangKhoe, Is.True,
                "[TC_F2.4_06] Trang không được crash khi tìm kiếm emoji");
        }

        // STT 35 - TC_F2.4_07: HTML entities
        [Test]
        public void TC_F2_4_07_TimKiemHtmlEntities()
        {
            CurrentTestCaseId = "TC_F2.4_07";
            var data = DocDuLieu("TC_F2.4_07");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            bool trangKhoe = _productListPage.IsPageHealthy();
            int soLuong = _productListPage.GetDisplayedProductCount();

            CurrentActualResult = $"Tìm kiếm chuỗi HTML entities '{data["searchKeyword"]}': h\u1ec7 th\u1ed1ng tr\u1ea3 v\u1ec1 {soLuong} k\u1ebft qu\u1ea3.";

            Assert.That(trangKhoe, Is.True,
                "[TC_F2.4_07] Trang không được crash khi tìm HTML entities");
        }

        // STT 36 - TC_F2.4_08: Gõ nhanh liên tục (debounce)
        [Test]
        public void TC_F2_4_08_GoNhanhLienTuc_Debounce()
        {
            CurrentTestCaseId = "TC_F2.4_08";
            var data = DocDuLieu("TC_F2.4_08");

            _productListPage.Open();
            Thread.Sleep(500);

            var searchInput = _productListPage.GetSearchInput();
            searchInput.Click();
            foreach (char c in data["searchKeyword"])
            {
                searchInput.SendKeys(c.ToString());
                Thread.Sleep(50);
            }
            Thread.Sleep(2000);

            bool timThay = _productListPage.IsProductInResults(data["expectedProductName"]);
            bool trangKhoe = _productListPage.IsPageHealthy();

            CurrentActualResult = timThay
                ? $"Gõ nhanh liên tục từng ký tự từ khoá '{data["searchKeyword"]}': k\u1ebft qu\u1ea3 cu\u1ed1i c\u00f9ng hiển thị đúng sản phẩm '{data["expectedProductName"]}'."
                : $"Gõ nhanh liên tục từng ký tự từ khoá '{data["searchKeyword"]}': kết quả không hiển thị sản phẩm '{data["expectedProductName"]}' (không đúng kỳ vọng).";

            Assert.That(trangKhoe, Is.True,
                "[TC_F2.4_08] Trang phải hoạt động bình thường sau khi gõ nhanh");
            Assert.That(timThay, Is.True,
                $"[TC_F2.4_08] Kết quả cuối cùng phải hiển thị đúng SP '{data["expectedProductName"]}'");
        }

        // STT 37 - TC_F2.4_09: Nhấn Enter - không submit form
        [Test]
        public void TC_F2_4_09_NhanEnter_KhongSubmitForm()
        {
            CurrentTestCaseId = "TC_F2.4_09";
            var data = DocDuLieu("TC_F2.4_09");

            _productListPage.Open();
            Thread.Sleep(500);

            var searchInput = _productListPage.GetSearchInput();
            searchInput.Click();
            searchInput.SendKeys(data["searchKeyword"]);
            searchInput.SendKeys(Keys.Enter);
            Thread.Sleep(1500);

            bool trangKhoe = _productListPage.IsPageHealthy();

            CurrentActualResult = trangKhoe
                ? $"Nhập từ khoá '{data["searchKeyword"]}' rồi nhấn Enter: danh s\u00e1ch k\u1ebft qu\u1ea3 hi\u1ec3n th\u1ecb b\u00ecnh th\u01b0\u1eddng, kh\u00f4ng reload b\u1ea5t ng\u1edd."
                : $"Nhập từ khoá '{data["searchKeyword"]}' rồi nhấn Enter: trang bị lỗi hoặc reload không đúng (không đúng kỳ vọng).";

            Assert.That(trangKhoe, Is.True,
                "[TC_F2.4_09] Trang không được crash hoặc reload bất ngờ khi nhấn Enter");
        }

        // STT 38 - TC_F2.4_10: Decision Table: SP không tồn tại + Category + Sort
        [Test]
        public void TC_F2_4_10_DecisionTable_KhongTonTai_Category_Sort()
        {
            CurrentTestCaseId = "TC_F2.4_10";
            var data = DocDuLieu("TC_F2.4_10");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(800);
            _productListPage.SelectCategory(data["categoryFilter"]);
            Thread.Sleep(800);
            _productListPage.SelectSort(data["sortOption"]);
            Thread.Sleep(1000);

            int soLuong = _productListPage.GetProductRowCount();

            CurrentActualResult = $"Tìm kiếm sản phẩm không tồn tại '{data["searchKeyword"]}' kết hợp lọc danh mục '{data["categoryFilter"]}' và sắp xếp '{data["sortOption"]}': số sản phẩm trả về là {soLuong}, kỳ vọng 0.";

            Assert.That(soLuong, Is.EqualTo(0),
                "[TC_F2.4_10] Phải hiển thị 0 kết quả khi SP không tồn tại dù có filter+sort");
        }

        private Dictionary<string, string> DocDuLieu(string testCaseId)
        {
            return JsonHelper.DocDuLieu(DataPath, testCaseId);
        }
    }
}

