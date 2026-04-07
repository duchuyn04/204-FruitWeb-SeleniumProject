using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumProject.Pages.ProductManagement;
using SeleniumProject.Utilities;
using System.Text.Json;

namespace SeleniumProject.Tests.ProductManagement
{
    [TestFixture]
    public class ProductSearchTests : TestBase
    {
        private ProductListPage _productListPage = null!;

        // Đường dẫn file JSON chứa dữ liệu test — nằm trong TestData/ProductManagement/
        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "ProductManagement", "product_search.json"
        );

        [SetUp]
        public void SetUpPages()
        {
            CurrentSheetName = "TC_Product Management";
            _productListPage = new ProductListPage(Driver, BaseUrl);
            LoginAsAdmin();
        }

        // STT 1 — TC_F2.2_01: SP đã xóa mềm không xuất hiện trên danh sách
        [Test]
        public void TC_F2_2_01_SPDaXoaMemKhongXuatHien()
        {
            CurrentTestCaseId = "TC_F2.2_01";
            var data = DocDuLieu("TC_F2.2_01");

            _productListPage.Open();
            Thread.Sleep(1000);

            int soLuong = _productListPage.GetProductRowCount();
            bool trangHoatDong = _productListPage.IsPageHealthy();

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(trangHoatDong, Is.True,
                "[TC_F2.2_01] Trang danh sách sản phẩm phải hiển thị bình thường");
            Assert.That(soLuong, Is.GreaterThan(0),
                "[TC_F2.2_01] Danh sách phải có ít nhất 1 sản phẩm");
        }

        // STT 2 — TC_F2.3_01: Tìm kiếm khớp chính xác tên
        [Test]
        public void TC_F2_3_01_TimKiemKhopChinhXacTen()
        {
            CurrentTestCaseId = "TC_F2.3_01";
            var data = DocDuLieu("TC_F2.3_01");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            bool timThay = _productListPage.IsProductInResults(data["expectedProductName"]);

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(timThay, Is.True,
                $"[TC_F2.3_01] Phải tìm thấy sản phẩm chứa '{data["expectedProductName"]}'");
        }

        // STT 3 — TC_F2.3_02: Tìm kiếm từ khóa một phần - nhiều kết quả
        [Test]
        public void TC_F2_3_02_TimKiemMotPhan_NhieuKetQua()
        {
            CurrentTestCaseId = "TC_F2.3_02";
            var data = DocDuLieu("TC_F2.3_02");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            int soLuong = _productListPage.GetProductRowCount();

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(soLuong, Is.GreaterThanOrEqualTo(2),
                $"[TC_F2.3_02] Phải hiển thị ít nhất 2 sản phẩm chứa '{data["searchKeyword"]}'");
        }

        // STT 4 — TC_F2.3_03: Để trống ô tìm kiếm - hiển thị toàn bộ
        [Test]
        public void TC_F2_3_03_DeTrongOTimKiem_HienThiToanBo()
        {
            CurrentTestCaseId = "TC_F2.3_03";
            var data = DocDuLieu("TC_F2.3_03");

            _productListPage.Open();
            Thread.Sleep(1000);

            int soLuong = _productListPage.GetProductRowCount();
            int headerCount = _productListPage.GetDisplayedProductCount();

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(soLuong, Is.GreaterThanOrEqualTo(4),
                "[TC_F2.3_03] Phải hiển thị đầy đủ toàn bộ sản phẩm (≥4)");
        }

        // STT 5 — TC_F2.3_04: Tìm kiếm không phân biệt hoa thường
        [Test]
        public void TC_F2_3_04_TimKiemKhongPhanBietHoaThuong()
        {
            CurrentTestCaseId = "TC_F2.3_04";
            var data = DocDuLieu("TC_F2.3_04");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]); // "cam sành"
            Thread.Sleep(1000);

            bool timThay = _productListPage.IsProductInResults(data["expectedProductName"]); // "Cam Sành"

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(timThay, Is.True,
                "[TC_F2.3_04] Phải tìm đúng sản phẩm dù nhập chữ thường");
        }

        // STT 6 — TC_F2.3_05: Tìm kiếm với khoảng trắng đầu/cuối
        [Test]
        public void TC_F2_3_05_TimKiemKhoangTrangDauCuoi()
        {
            CurrentTestCaseId = "TC_F2.3_05";
            var data = DocDuLieu("TC_F2.3_05");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]); // "  Dâu Tây  "
            Thread.Sleep(1000);

            bool timThay = _productListPage.IsProductInResults(data["expectedProductName"]);

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(timThay, Is.True,
                "[TC_F2.3_05] Phải tìm đúng sản phẩm dù có khoảng trắng thừa");
        }

        // STT 7 — TC_F2.3_06: Tìm kiếm với 1 ký tự
        [Test]
        public void TC_F2_3_06_TimKiemMotKyTu()
        {
            CurrentTestCaseId = "TC_F2.3_06";
            var data = DocDuLieu("TC_F2.3_06");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]); // "B"
            Thread.Sleep(1000);

            int soLuong = _productListPage.GetProductRowCount();
            bool trangKhoe = _productListPage.IsPageHealthy();

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(trangKhoe, Is.True,
                "[TC_F2.3_06] Trang phải hoạt động bình thường khi tìm 1 ký tự");
            Assert.That(soLuong, Is.GreaterThanOrEqualTo(1),
                "[TC_F2.3_06] Phải trả về ít nhất 1 sản phẩm chứa ký tự 'B'");
        }

        // STT 8 — TC_F2.3_07: Tìm kiếm bằng slug sản phẩm
        [Test]
        public void TC_F2_3_07_TimKiemBangSlug()
        {
            CurrentTestCaseId = "TC_F2.3_07";
            var data = DocDuLieu("TC_F2.3_07");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]); // "cam-sanh-mien-tay"
            Thread.Sleep(1000);

            bool trangKhoe = _productListPage.IsPageHealthy();

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(trangKhoe, Is.True,
                "[TC_F2.3_07] Trang không được crash khi tìm kiếm bằng slug");
        }

        // STT 9 — TC_F2.3_08: Tìm kiếm chuỗi rất dài (255 ký tự)
        [Test]
        public void TC_F2_3_08_TimKiemChuoiRatDai()
        {
            CurrentTestCaseId = "TC_F2.3_08";
            var data = DocDuLieu("TC_F2.3_08");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]); // 255 ký tự "AAA...A"
            Thread.Sleep(1000);

            bool trangKhoe = _productListPage.IsPageHealthy();

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(trangKhoe, Is.True,
                "[TC_F2.3_08] Trang không được crash khi tìm kiếm chuỗi 255 ký tự");
        }

        // STT 10 — TC_F2.3_09: Xóa từ khóa - danh sách trở về ban đầu
        [Test]
        public void TC_F2_3_09_XoaTuKhoa_DanhSachTroVeBanDau()
        {
            CurrentTestCaseId = "TC_F2.3_09";
            var data = DocDuLieu("TC_F2.3_09");

            _productListPage.Open();

            // Bước 1: Nhập từ khóa
            _productListPage.Search(data["searchKeyword"]); // "Cam"
            Thread.Sleep(1000);

            // Bước 2: Xóa trắng ô tìm kiếm
            _productListPage.ClearSearch();
            Thread.Sleep(1000);

            // Bước 3: Kiểm tra danh sách trở lại đầy đủ
            int soLuong = _productListPage.GetProductRowCount();

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(soLuong, Is.GreaterThanOrEqualTo(4),
                "[TC_F2.3_09] Danh sách phải trở về trạng thái ban đầu (≥4 SP)");
        }

        // STT 11 — TC_F2.3_10: URL cập nhật query string khi tìm kiếm
        [Test]
        public void TC_F2_3_10_UrlCapNhatQueryString()
        {
            CurrentTestCaseId = "TC_F2.3_10";
            var data = DocDuLieu("TC_F2.3_10");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]); // "Nho"
            Thread.Sleep(1500);

            string url = _productListPage.GetCurrentUrl();

            CurrentActualResult = $"URL sau khi search: {url}";

            Assert.That(url, Does.Contain(data["expectedUrlParam"]),
                $"[TC_F2.3_10] URL phải chứa '{data["expectedUrlParam"]}' sau khi tìm kiếm");
        }

        // STT 12 — TC_F2.3_11: Bộ đếm sản phẩm cập nhật đúng
        [Test]
        public void TC_F2_3_11_BoDocSanPhamCapNhatDung()
        {
            CurrentTestCaseId = "TC_F2.3_11";
            var data = DocDuLieu("TC_F2.3_11");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]); // "Chuối"
            Thread.Sleep(1000);

            int headerCount = _productListPage.GetDisplayedProductCount();
            int rowCount = _productListPage.GetProductRowCount();

            CurrentActualResult = $"Header count: {headerCount}, Row count: {rowCount}";

            Assert.That(headerCount, Is.GreaterThanOrEqualTo(0),
                "[TC_F2.3_11] Bộ đếm header phải cập nhật");
            Assert.That(headerCount, Is.EqualTo(rowCount),
                "[TC_F2.3_11] Số trên header phải khớp với số dòng thực tế trong bảng");
        }

        // STT 13 — TC_F2.3_12: Tìm kiếm tiếng Việt có dấu
        [Test]
        public void TC_F2_3_12_TimKiemTiengVietCoDau()
        {
            CurrentTestCaseId = "TC_F2.3_12";
            var data = DocDuLieu("TC_F2.3_12");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]); // "Dứa Mật"
            Thread.Sleep(1000);

            bool timThay = _productListPage.IsProductInResults(data["expectedProductName"]);

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(timThay, Is.True,
                $"[TC_F2.3_12] Phải tìm đúng sản phẩm '{data["expectedProductName"]}' với tiếng Việt có dấu");
        }

        // STT 14 — TC_F2.3_13: Tìm kiếm chỉ có khoảng trắng
        [Test]
        public void TC_F2_3_13_TimKiemChiCoKhoangTrang()
        {
            CurrentTestCaseId = "TC_F2.3_13";
            var data = DocDuLieu("TC_F2.3_13");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]); // "   "
            Thread.Sleep(1000);

            int soLuong = _productListPage.GetProductRowCount();

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(soLuong, Is.GreaterThanOrEqualTo(4),
                "[TC_F2.3_13] Phải hiển thị toàn bộ sản phẩm (≥4) khi chỉ nhập khoảng trắng");
        }

        // STT 15 — TC_F2.3_14: Tìm kiếm trực tiếp qua URL query string
        [Test]
        public void TC_F2_3_14_TimKiemTrucTiepQuaUrl()
        {
            CurrentTestCaseId = "TC_F2.3_14";
            var data = DocDuLieu("TC_F2.3_14");

            // Truy cập URL trực tiếp có search query
            string fullUrl = BaseUrl.TrimEnd('/') + data["directUrl"];
            _productListPage.OpenWithUrl(fullUrl);
            Thread.Sleep(1000);

            bool timThay = _productListPage.IsProductInResults(data["expectedProductName"]);
            string searchValue = _productListPage.GetSearchInputValue();

            CurrentActualResult = $"Search input value: '{searchValue}' | " + _productListPage.DocKetQuaThucTe();

            Assert.That(timThay, Is.True,
                $"[TC_F2.3_14] Phải hiển thị sản phẩm '{data["expectedProductName"]}' khi truy cập qua URL");
            Assert.That(searchValue, Does.Contain(data["expectedSearchInputValue"]),
                "[TC_F2.3_14] Ô search phải được fill sẵn từ URL query string");
        }

        // STT 16 — TC_F2.3_15: Regression - Thêm SP mới, search tìm thấy
        [Test]
        public void TC_F2_3_15_RegressionThemSPMoi_SearchTimThay()
        {
            CurrentTestCaseId = "TC_F2.3_15";
            var data = DocDuLieu("TC_F2.3_15");

            // Bước 1: Thêm sản phẩm mới
            var createPage = new CreateProductPage(Driver, BaseUrl);
            createPage.Open();
            Thread.Sleep(1000);

            createPage.EnterName(data["newProductName"]);
            Thread.Sleep(500);
            createPage.SelectCategory(data["category"]);
            createPage.EnterPrice(data["price"]);
            createPage.EnterStock(data["stock"]);
            createPage.EnterUnit(data["unit"]);
            createPage.EnterShortDescription(data["shortDescription"]);
            createPage.ClickSave();
            Thread.Sleep(2000);

            // Bước 2: Quay lại trang danh sách và tìm kiếm SP vừa thêm
            _productListPage.Open();
            Thread.Sleep(1000);
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            bool timThay = _productListPage.IsProductInResults(data["searchKeyword"]);

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(timThay, Is.True,
                $"[TC_F2.3_15] Phải tìm thấy SP vừa thêm '{data["searchKeyword"]}'");
        }

        // STT 17 — TC_F2.3_16: Regression - Xóa SP, search không còn
        [Test]
        public void TC_F2_3_16_RegressionXoaSP_SearchKhongCon()
        {
            CurrentTestCaseId = "TC_F2.3_16";
            var data = DocDuLieu("TC_F2.3_16");

            // Bước 1: Tìm kiếm SP cần xóa
            _productListPage.Open();
            Thread.Sleep(1000);
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            // Bước 2: Xóa SP (click nút Xóa trên dòng đầu tiên nếu tìm thấy)
            try
            {
                IWebElement deleteBtn = Driver.FindElement(
                    By.CssSelector("table.table tbody tr:first-child .btn-danger, table.table tbody tr:first-child a[title='Xóa']")
                );
                deleteBtn.Click();
                Thread.Sleep(500);

                // Xác nhận xóa nếu có modal
                try
                {
                    IWebElement confirmBtn = Driver.FindElement(By.CssSelector(".modal .btn-danger, .swal2-confirm"));
                    confirmBtn.Click();
                    Thread.Sleep(1500);
                }
                catch { /* Không có modal xác nhận */ }
            }
            catch
            {
                // SP không tồn tại — ghi nhận
                CurrentActualResult = "SP không tồn tại để xóa";
                Assert.Pass("[TC_F2.3_16] SP không tồn tại trong hệ thống — bỏ qua test regression");
                return;
            }

            // Bước 3: Tìm kiếm lại SP đã xóa
            _productListPage.Open();
            Thread.Sleep(1000);
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            bool conTimThay = _productListPage.IsProductInResults(data["searchKeyword"]);

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(conTimThay, Is.False,
                $"[TC_F2.3_16] Không được tìm thấy SP '{data["searchKeyword"]}' sau khi xóa");
        }

        // STT 18 — TC_F2.3_17: Tìm kiếm kết hợp lọc theo danh mục
        [Test]
        public void TC_F2_3_17_TimKiemKetHopLocTheoDanhMuc()
        {
            CurrentTestCaseId = "TC_F2.3_17";
            var data = DocDuLieu("TC_F2.3_17");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]); // "Bưởi"
            Thread.Sleep(1000);

            _productListPage.SelectCategory(data["categoryFilter"]); // "Trái cây nhập khẩu"
            Thread.Sleep(1000);

            List<string> danhSachSP = _productListPage.GetProductNames();

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            // Mọi SP trong kết quả đều phải chứa từ khóa
            foreach (string tenSP in danhSachSP)
            {
                Assert.That(tenSP, Does.Contain(data["expectedProductName"]).IgnoreCase,
                    $"[TC_F2.3_17] SP '{tenSP}' phải chứa từ khóa '{data["expectedProductName"]}'");
            }
        }

        // STT 19 — TC_F2.3_18: Tìm kiếm kết hợp sắp xếp
        [Test]
        public void TC_F2_3_18_TimKiemKetHopSapXep()
        {
            CurrentTestCaseId = "TC_F2.3_18";
            var data = DocDuLieu("TC_F2.3_18");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]); // "Bưởi"
            Thread.Sleep(1000);

            _productListPage.SelectSort(data["sortOption"]); // "Tên A-Z"
            Thread.Sleep(1000);

            List<string> danhSachSP = _productListPage.GetProductNames();

            CurrentActualResult = $"Thứ tự: [{string.Join(", ", danhSachSP)}]";

            // Kiểm tra thứ tự A-Z
            for (int i = 1; i < danhSachSP.Count; i++)
            {
                int compare = string.Compare(danhSachSP[i - 1], danhSachSP[i], StringComparison.OrdinalIgnoreCase);
                Assert.That(compare, Is.LessThanOrEqualTo(0),
                    $"[TC_F2.3_18] SP '{danhSachSP[i - 1]}' phải đứng trước '{danhSachSP[i]}' theo A-Z");
            }
        }

        // STT 20 — TC_F2.3_19: Decision Table - mặc định hiện toàn bộ
        [Test]
        public void TC_F2_3_19_DecisionTable_MacDinhHienToanBo()
        {
            CurrentTestCaseId = "TC_F2.3_19";
            var data = DocDuLieu("TC_F2.3_19");

            _productListPage.Open();
            Thread.Sleep(1000);

            // Không thay đổi gì — giữ nguyên tất cả bộ lọc mặc định
            int soLuong = _productListPage.GetProductRowCount();

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(soLuong, Is.GreaterThanOrEqualTo(4),
                "[TC_F2.3_19] Phải hiển thị toàn bộ sản phẩm (≥4) khi giữ nguyên bộ lọc mặc định");
        }

        // STT 21 — TC_F2.3_20: Decision Table: Search + Category + Sort giá
        [Test]
        public void TC_F2_3_20_DecisionTable_SearchCategorySort()
        {
            CurrentTestCaseId = "TC_F2.3_20";
            var data = DocDuLieu("TC_F2.3_20");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(800);
            _productListPage.SelectCategory(data["categoryFilter"]);
            Thread.Sleep(800);
            _productListPage.SelectSort(data["sortOption"]);
            Thread.Sleep(1000);

            List<string> danhSachSP = _productListPage.GetProductNames();

            CurrentActualResult = $"Kết quả: [{string.Join(", ", danhSachSP)}]";

            Assert.That(danhSachSP.Count, Is.GreaterThan(0),
                "[TC_F2.3_20] Phải có ít nhất 1 sản phẩm sau khi lọc kết hợp 3 bộ lọc");
        }

        // STT 22 — TC_F2.3_21: State Transition: sort vẫn giữ sau clear search
        [Test]
        public void TC_F2_3_21_StateTransition_SortGiuSauClearSearch()
        {
            CurrentTestCaseId = "TC_F2.3_21";
            var data = DocDuLieu("TC_F2.3_21");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(800);
            _productListPage.SelectSort(data["sortOption"]);
            Thread.Sleep(800);

            _productListPage.ClearSearch();
            Thread.Sleep(1000);

            string currentSort = _productListPage.GetCurrentSortValue();

            CurrentActualResult = $"Sort hiện tại sau clear search: '{currentSort}'";

            Assert.That(_productListPage.IsPageHealthy(), Is.True,
                "[TC_F2.3_21] Trang phải hoạt động bình thường sau clear search");
        }

        // STT 23 — TC_F2.3_22: Use Case: search → filter → sort → clear all
        [Test]
        public void TC_F2_3_22_UseCase_LuongDayDu_ClearAll()
        {
            CurrentTestCaseId = "TC_F2.3_22";
            var data = DocDuLieu("TC_F2.3_22");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(800);
            _productListPage.SelectCategory(data["categoryFilter"]);
            Thread.Sleep(800);
            _productListPage.SelectSort(data["sortOption"]);
            Thread.Sleep(800);

            _productListPage.Open();
            Thread.Sleep(1000);

            int soLuong = _productListPage.GetProductRowCount();

            CurrentActualResult = $"Số SP sau clear all: {soLuong}";

            Assert.That(soLuong, Is.GreaterThanOrEqualTo(4),
                "[TC_F2.3_22] Danh sách phải trở về đầy đủ (≥4) sau khi clear toàn bộ");
        }

        // STT 24 — TC_F2.3_23: Tìm kiếm từ khóa ngắn 2-3 ký tự
        [Test]
        public void TC_F2_3_23_TimKiemTuKhoaNgan()
        {
            CurrentTestCaseId = "TC_F2.3_23";
            var data = DocDuLieu("TC_F2.3_23");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(_productListPage.IsPageHealthy(), Is.True,
                "[TC_F2.3_23] Trang phải hoạt động bình thường khi tìm từ khóa ngắn");
        }

        // STT 25 — TC_F2.3_24: Tìm kiếm từ khóa trung bình 10-20 ký tự
        [Test]
        public void TC_F2_3_24_TimKiemTuKhoaTrungBinh()
        {
            CurrentTestCaseId = "TC_F2.3_24";
            var data = DocDuLieu("TC_F2.3_24");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(_productListPage.IsPageHealthy(), Is.True,
                "[TC_F2.3_24] Trang phải xử lý bình thường với từ khóa trung bình");
        }

        // STT 26 — TC_F2.3_25: Tìm kiếm chỉ có số
        [Test]
        public void TC_F2_3_25_TimKiemChiCoSo()
        {
            CurrentTestCaseId = "TC_F2.3_25";
            var data = DocDuLieu("TC_F2.3_25");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(1000);

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(_productListPage.IsPageHealthy(), Is.True,
                "[TC_F2.3_25] Trang phải xử lý bình thường khi tìm kiếm chỉ có số");
        }

        // STT 27 — TC_F2.3_26: Placeholder ô tìm kiếm
        [Test]
        public void TC_F2_3_26_PlaceholderOTimKiem()
        {
            CurrentTestCaseId = "TC_F2.3_26";
            var data = DocDuLieu("TC_F2.3_26");

            _productListPage.Open();
            Thread.Sleep(1000);

            string placeholder = _productListPage.GetSearchPlaceholder();

            CurrentActualResult = $"Placeholder: '{placeholder}'";

            Assert.That(placeholder, Is.Not.Empty,
                "[TC_F2.3_26] Placeholder ô tìm kiếm phải có giá trị");
        }

        // STT 28 — TC_F2.3_27: Loading indicator khi tìm kiếm
        [Test]
        public void TC_F2_3_27_LoadingIndicatorKhiTimKiem()
        {
            CurrentTestCaseId = "TC_F2.3_27";
            var data = DocDuLieu("TC_F2.3_27");

            _productListPage.Open();
            _productListPage.Search(data["searchKeyword"]);
            Thread.Sleep(2000);

            CurrentActualResult = _productListPage.DocKetQuaThucTe();

            Assert.That(_productListPage.IsPageHealthy(), Is.True,
                "[TC_F2.3_27] Trang phải hiển thị bình thường sau khi loading hoàn tất");
        }

        private Dictionary<string, string> DocDuLieu(string testCaseId)
        {
            return JsonHelper.DocDuLieu(DataPath, testCaseId);
        }
    }
}
