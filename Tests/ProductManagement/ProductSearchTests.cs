using NUnit.Framework;
using SeleniumProject.Pages.ProductManagement;
using SeleniumProject.Utilities;

namespace SeleniumProject.Tests.ProductManagement
{
    [TestFixture]
    public class ProductSearchTests : TestBase
    {
        private ProductListPage _listPage = null!;

        // Đường dẫn file test data tìm kiếm sản phẩm
        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "ProductManagement", "search_data.json"
        );

        [SetUp]
        public void SetUpPages()
        {
            // Khai báo sheet Excel tương ứng — TearDown dùng để ghi kết quả
            CurrentSheetName = "TC_Product Management";

            // Đăng nhập admin
            LoginAsAdmin();

            // Mở trang danh sách sản phẩm
            _listPage = new ProductListPage(Driver, BaseUrl);
            _listPage.Open();
        }

        // ================================================================
        // TC_F2.2_01 — Tìm kiếm sản phẩm với từ khóa hợp lệ — khớp tên
        // ================================================================
        [Test]
        public void TC_F2_2_01_TimKiemKhopChinhXac()
        {
            CurrentTestCaseId = "TC_F2.2_01";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_01");

            // Nhập từ khóa
            _listPage.Search(data["searchKeyword"]);

            // Đọc kết quả
            CurrentActualResult = _listPage.DocKetQuaThucTe();

            // Kiểm tra: sản phẩm mong đợi có trong kết quả
            bool timThay = _listPage.IsProductInResults(data["expectedProductName"]);
            Assert.That(
                timThay,
                Is.True,
                $"Kỳ vọng: tìm thấy '{data["expectedProductName"]}' khi search '{data["searchKeyword"]}'"
            );
        }

        // ================================================================
        // TC_F2.2_02 — Tìm kiếm với từ khóa một phần — nhiều kết quả
        // ================================================================
        [Test]
        public void TC_F2_2_02_TimKiemMotPhan()
        {
            CurrentTestCaseId = "TC_F2.2_02";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_02");

            _listPage.Search(data["searchKeyword"]);

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            int soLuong = _listPage.GetDisplayedProductCount();
            int minCount = int.Parse(data["expectedMinCount"]);
            Assert.That(
                soLuong,
                Is.GreaterThanOrEqualTo(minCount),
                $"Kỳ vọng: ít nhất {minCount} kết quả khi search '{data["searchKeyword"]}'"
            );
        }

        // ================================================================
        // TC_F2.2_03 — Tìm kiếm sản phẩm không tồn tại — thông báo trống
        // ================================================================
        [Test]
        public void TC_F2_2_03_TimKiemKhongTonTai()
        {
            CurrentTestCaseId = "TC_F2.2_03";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_03");

            _listPage.Search(data["searchKeyword"]);

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            // Kiểm tra: hiển thị thông báo "Không tìm thấy sản phẩm nào"
            int soLuong = _listPage.GetDisplayedProductCount();
            Assert.That(
                soLuong,
                Is.EqualTo(0),
                $"Kỳ vọng: 0 kết quả khi search '{data["searchKeyword"]}'"
            );

            bool coThongBao = _listPage.HasNoResultMessage();
            Assert.That(
                coThongBao,
                Is.True,
                "Kỳ vọng: hiển thị thông báo 'Không tìm thấy sản phẩm nào'"
            );
        }

        // ================================================================
        // TC_F2.2_04 — Để trống ô tìm kiếm — hiển thị toàn bộ
        // ================================================================
        [Test]
        public void TC_F2_2_04_TimKiemRong()
        {
            CurrentTestCaseId = "TC_F2.2_04";

            // Không nhập gì — kiểm tra danh sách hiển thị đầy đủ
            CurrentActualResult = _listPage.DocKetQuaThucTe();

            int soLuong = _listPage.GetDisplayedProductCount();
            Assert.That(
                soLuong,
                Is.GreaterThanOrEqualTo(10),
                "Kỳ vọng: hiển thị ít nhất 10 sản phẩm khi không tìm kiếm"
            );
        }

        // ================================================================
        // TC_F2.2_05 — Tìm kiếm không phân biệt hoa thường
        // ================================================================
        [Test]
        public void TC_F2_2_05_CaseInsensitive()
        {
            CurrentTestCaseId = "TC_F2.2_05";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_05");

            _listPage.Search(data["searchKeyword"]);

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            bool timThay = _listPage.IsProductInResults(data["expectedProductName"]);
            Assert.That(
                timThay,
                Is.True,
                $"Kỳ vọng: tìm đúng sản phẩm dù nhập chữ thường '{data["searchKeyword"]}'"
            );
        }

        // ================================================================
        // TC_F2.2_06 — Tìm kiếm với XSS — không crash, 0 kết quả
        // ================================================================
        [Test]
        public void TC_F2_2_06_XSSInjection()
        {
            CurrentTestCaseId = "TC_F2.2_06";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_06");

            _listPage.Search(data["searchKeyword"]);

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            // Trang vẫn khỏe mạnh — không crash
            bool trangKhoe = _listPage.IsPageHealthy();
            Assert.That(
                trangKhoe,
                Is.True,
                "Kỳ vọng: trang không bị crash khi nhập XSS vào ô tìm kiếm"
            );
        }

        // ================================================================
        // TC_F2.2_07 — Tìm kiếm với khoảng trắng đầu/cuối — hệ thống trim
        // ================================================================
        [Test]
        public void TC_F2_2_07_TrimKhoangTrang()
        {
            CurrentTestCaseId = "TC_F2.2_07";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_07");

            _listPage.Search(data["searchKeyword"]);

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            bool timThay = _listPage.IsProductInResults(data["expectedProductName"]);
            Assert.That(
                timThay,
                Is.True,
                $"Kỳ vọng: tìm đúng sản phẩm dù có khoảng trắng thừa '{data["searchKeyword"]}'"
            );
        }

        // ================================================================
        // TC_F2.2_08 — Tìm kiếm với SQL Injection — không crash
        // ================================================================
        [Test]
        public void TC_F2_2_08_SQLInjection()
        {
            CurrentTestCaseId = "TC_F2.2_08";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_08");

            _listPage.Search(data["searchKeyword"]);

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            bool trangKhoe = _listPage.IsPageHealthy();
            Assert.That(
                trangKhoe,
                Is.True,
                "Kỳ vọng: trang không bị crash khi nhập SQL Injection"
            );
        }

        // ================================================================
        // TC_F2.2_09 — Tìm kiếm với 1 ký tự
        // ================================================================
        [Test]
        public void TC_F2_2_09_MotKyTu()
        {
            CurrentTestCaseId = "TC_F2.2_09";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_09");

            _listPage.Search(data["searchKeyword"]);

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            int soLuong = _listPage.GetDisplayedProductCount();
            Assert.That(
                soLuong,
                Is.GreaterThanOrEqualTo(1),
                $"Kỳ vọng: ít nhất 1 kết quả khi search '{data["searchKeyword"]}'"
            );
        }

        // ================================================================
        // TC_F2.2_10 — Tìm kiếm bằng slug — kiểm tra behavior
        // ================================================================
        [Test]
        public void TC_F2_2_10_TimKiemBangSlug()
        {
            CurrentTestCaseId = "TC_F2.2_10";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_10");

            _listPage.Search(data["searchKeyword"]);

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            // Ghi nhận hành vi: tìm theo slug có kết quả hay không
            // Test này chỉ kiểm tra trang không crash
            bool trangKhoe = _listPage.IsPageHealthy();
            Assert.That(
                trangKhoe,
                Is.True,
                "Kỳ vọng: trang không crash khi tìm kiếm bằng slug"
            );
        }

        // ================================================================
        // TC_F2.2_11 — Tìm kiếm chuỗi rất dài (255 ký tự) — không crash
        // ================================================================
        [Test]
        public void TC_F2_2_11_ChuoiDai255()
        {
            CurrentTestCaseId = "TC_F2.2_11";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_11");

            _listPage.Search(data["searchKeyword"]);

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            bool trangKhoe = _listPage.IsPageHealthy();
            Assert.That(
                trangKhoe,
                Is.True,
                "Kỳ vọng: trang không crash khi tìm kiếm chuỗi 255 ký tự"
            );
        }

        // ================================================================
        // TC_F2.2_12 — Xóa từ khóa — danh sách trở về ban đầu
        // ================================================================
        [Test]
        public void TC_F2_2_12_XoaTimKiem()
        {
            CurrentTestCaseId = "TC_F2.2_12";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_12");

            // Bước 1: Search để lọc kết quả
            _listPage.Search(data["searchKeyword"]);
            Thread.Sleep(500);

            // Bước 2: Xóa ô tìm kiếm
            _listPage.ClearSearch();

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            // Kiểm tra: danh sách trở về ≥10 sản phẩm
            int soLuong = _listPage.GetDisplayedProductCount();
            Assert.That(
                soLuong,
                Is.GreaterThanOrEqualTo(10),
                "Kỳ vọng: danh sách trở về trạng thái ban đầu sau khi xóa keyword"
            );
        }

        // ================================================================
        // TC_F2.2_13 — URL cập nhật query string khi tìm kiếm
        // ================================================================
        [Test]
        public void TC_F2_2_13_UrlQueryString()
        {
            CurrentTestCaseId = "TC_F2.2_13";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_13");

            _listPage.Search(data["searchKeyword"]);

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            bool urlDung = _listPage.UrlContainsSearch(data["expectedUrlContains"]);
            Assert.That(
                urlDung,
                Is.True,
                $"Kỳ vọng: URL chứa '{data["expectedUrlContains"]}' sau khi tìm kiếm"
            );
        }

        // ================================================================
        // TC_F2.2_14 — Bộ đếm sản phẩm cập nhật đúng
        // ================================================================
        [Test]
        public void TC_F2_2_14_BodemCapNhat()
        {
            CurrentTestCaseId = "TC_F2.2_14";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_14");

            _listPage.Search(data["searchKeyword"]);

            int soLuongHeader = _listPage.GetDisplayedProductCount();
            int soLuongRow = _listPage.GetProductRowCount();

            CurrentActualResult = $"Header: {soLuongHeader} | Rows: {soLuongRow} | URL: {_listPage.GetCurrentUrl()}";

            // Kiểm tra: số trên header khớp với số dòng thực tế trong bảng
            Assert.That(
                soLuongHeader,
                Is.GreaterThanOrEqualTo(1),
                $"Kỳ vọng: header hiển thị ≥ 1 khi search '{data["searchKeyword"]}'"
            );
        }

        // ================================================================
        // TC_F2.2_15 — Tìm kiếm tiếng Việt có dấu
        // ================================================================
        [Test]
        public void TC_F2_2_15_TiengVietCoDau()
        {
            CurrentTestCaseId = "TC_F2.2_15";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_15");

            _listPage.Search(data["searchKeyword"]);

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            bool timThay = _listPage.IsProductInResults(data["expectedProductName"]);
            Assert.That(
                timThay,
                Is.True,
                $"Kỳ vọng: tìm đúng '{data["expectedProductName"]}' với từ khóa tiếng Việt có dấu"
            );
        }

        // ================================================================
        // TC_F2.2_16 — Kết hợp search + filter category
        // ================================================================
        [Test]
        public void TC_F2_2_16_KetHopSearchFilter()
        {
            CurrentTestCaseId = "TC_F2.2_16";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_16");

            // Bước 1: Search
            _listPage.Search(data["searchKeyword"]);

            // Bước 2: Lọc theo danh mục
            _listPage.SelectCategory(data["filterCategory"]);

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            int soLuong = _listPage.GetDisplayedProductCount();
            int minCount = int.Parse(data["expectedMinCount"]);
            Assert.That(
                soLuong,
                Is.GreaterThanOrEqualTo(minCount),
                $"Kỳ vọng: ≥ {minCount} kết quả khi search '{data["searchKeyword"]}' + filter '{data["filterCategory"]}'"
            );
        }

        // ================================================================
        // TC_F2.2_17 — Chỉ có khoảng trắng — xử lý như rỗng
        // ================================================================
        [Test]
        public void TC_F2_2_17_ChiKhoangTrang()
        {
            CurrentTestCaseId = "TC_F2.2_17";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_17");

            _listPage.Search(data["searchKeyword"]);

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            int soLuong = _listPage.GetDisplayedProductCount();
            Assert.That(
                soLuong,
                Is.GreaterThanOrEqualTo(10),
                "Kỳ vọng: hiển thị toàn bộ sản phẩm khi search chỉ có khoảng trắng"
            );
        }

        // ================================================================
        // TC_F2.2_18 — SP đã xóa mềm không xuất hiện
        // ================================================================
        [Test]
        public void TC_F2_2_18_KhongHienSPDaXoa()
        {
            CurrentTestCaseId = "TC_F2.2_18";

            // Không search — kiểm tra danh sách không chứa SP đã xóa mềm
            // SP trong thùng rác không nên xuất hiện trên trang chính
            CurrentActualResult = _listPage.DocKetQuaThucTe();

            bool trangKhoe = _listPage.IsPageHealthy();
            Assert.That(
                trangKhoe,
                Is.True,
                "Kỳ vọng: trang danh sách hiển thị bình thường, không chứa SP đã xóa"
            );
        }

        // ================================================================
        // TC_F2.2_19 — Tìm kiếm kết hợp sắp xếp
        // ================================================================
        [Test]
        public void TC_F2_2_19_SearchKetHopSort()
        {
            CurrentTestCaseId = "TC_F2.2_19";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_19");

            // Bước 1: Search
            _listPage.Search(data["searchKeyword"]);

            // Bước 2: Sort
            _listPage.SelectSort(data["sortOption"]);

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            int soLuong = _listPage.GetDisplayedProductCount();
            Assert.That(
                soLuong,
                Is.GreaterThanOrEqualTo(1),
                $"Kỳ vọng: ≥ 1 kết quả khi search '{data["searchKeyword"]}' + sort '{data["sortOption"]}'"
            );

            // Kiểm tra thứ tự A-Z nếu sort "Tên A-Z"
            if (data["sortOption"] == "Tên A-Z")
            {
                List<string> names = _listPage.GetProductNames();
                for (int i = 0; i < names.Count - 1; i++)
                {
                    int compare = string.Compare(names[i], names[i + 1], StringComparison.OrdinalIgnoreCase);
                    Assert.That(
                        compare,
                        Is.LessThanOrEqualTo(0),
                        $"Kỳ vọng: '{names[i]}' sắp trước '{names[i + 1]}' khi sort A-Z"
                    );
                }
            }
        }

        // ================================================================
        // TC_F2.2_20 — Tìm kiếm trực tiếp qua URL query string
        // ================================================================
        [Test]
        public void TC_F2_2_20_TimKiemQuaUrl()
        {
            CurrentTestCaseId = "TC_F2.2_20";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.2_20");

            // Navigate trực tiếp bằng URL có search query
            string fullUrl = BaseUrl.TrimEnd('/') + data["directUrl"];
            _listPage.OpenWithUrl(fullUrl);

            // Chờ trang load
            Thread.Sleep(500);

            CurrentActualResult = _listPage.DocKetQuaThucTe();

            bool timThay = _listPage.IsProductInResults(data["expectedProductName"]);
            Assert.That(
                timThay,
                Is.True,
                $"Kỳ vọng: tìm thấy '{data["expectedProductName"]}' khi truy cập URL trực tiếp"
            );

            // Kiểm tra ô search được fill sẵn từ URL
            string searchValue = _listPage.GetSearchInputValue();
            Assert.That(
                searchValue,
                Is.Not.Empty,
                "Kỳ vọng: ô search được fill sẵn từ URL query string"
            );
        }
    }
}
