using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumProject.Pages.OrderManagement;
using SeleniumProject.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SeleniumProject.Tests.OrderManagement
{
    [TestFixture]
    public class OrderSearchTests : TestBase
    {
        private OrderListPage _orderListPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "OrderManagement", "order_search.json"
        );

        [SetUp]
        public void SetUpPages()
        {
            CurrentSheetName = "TC_OrderManagement";
            _orderListPage = new OrderListPage(Driver, BaseUrl);
            LoginAsAdmin();
        }

        // ============================================
        // NO 20 -> TC_F10.13_01: Không bị lỗi khi nhập XSS
        // ============================================
        [Test]
        public void TC_F10_13_01_KhongLoiKhiNhapXSS()
        {
            CurrentTestCaseId = "TC_F10.13_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1000);

            // Xử lý đọc maliciousInput thay vì searchKeyword
            string maliciousInput = data.GetValueOrDefault("maliciousInput", "<script>alert('xss')</script>");

            _orderListPage.SearchByOrderCode(maliciousInput);
            Thread.Sleep(1500); // Đợi Web load xong bảng

            bool isHealthy = _orderListPage.IsPageHealthy();
            int soLuongTimThay = _orderListPage.GetTotalOrderCount();
            
            CurrentActualResult = isHealthy 
                ? $"Xử lý an toàn. Tổng trả về: {soLuongTimThay} kết quả." 
                : "LỖI BẢO MẬT: Web bị vỡ trang hoặc hiện mã lỗi khi nhập XSS!";

            Assert.That(isHealthy, Is.True, 
                "[TC_F10.13_01] Trang bị crash hoặc lỗi hiển thị khi tiêm mã XSS");
        }

        // ============================================
        // NO 21 -> TC_F10.13_02: Không bị lỗi khi nhập SQL Injection
        // ============================================
        [Test]
        public void TC_F10_13_02_KhongLoiKhiNhapSQLInjection()
        {
            CurrentTestCaseId = "TC_F10.13_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1000);

            string maliciousInput = data.GetValueOrDefault("maliciousInput", "' OR '1'='1");
            
            _orderListPage.SearchByOrderCode(maliciousInput);
            Thread.Sleep(1500);

            bool isHealthy = _orderListPage.IsPageHealthy();
            int soLuongTimThay = _orderListPage.GetTotalOrderCount();
            
            CurrentActualResult = isHealthy 
                ? $"Xử lý an toàn. Tổng trả về: {soLuongTimThay} kết quả." 
                : "LỖI BẢO MẬT: Nhập SQL Injection gây lỗi phía Server/UI!";

            Assert.That(isHealthy, Is.True, 
                "[TC_F10.13_02] Gây lỗi khi nhập SQL Injection");
        }

        // ============================================
        // NO 22 -> TC_F10.13_03: Chống chịu chuỗi cực dài
        // ============================================
        [Test]
        public void TC_F10_13_03_ChongChiuChuoiKyTuDai()
        {
            CurrentTestCaseId = "TC_F10.13_03";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1000);

            string maliciousInput = data.GetValueOrDefault("maliciousInput", "AAAAAAAAAAAAAAAAAAAA");
            _orderListPage.SearchByOrderCode(maliciousInput);
            Thread.Sleep(1500);

            bool isHealthy = _orderListPage.IsPageHealthy();
            int soLuongTimThay = _orderListPage.GetTotalOrderCount();
            
            CurrentActualResult = isHealthy 
                ? $"Web ổn định khi nhập chuỗi siêu dài. Kết quả trả về: {soLuongTimThay}" 
                : "LỖI: Trình duyệt bị treo hoặc Server sập do chuỗi quá dài";

            Assert.That(isHealthy, Is.True, 
                "[TC_F10.13_03] Nhập dữ liệu quá dài gây vỡ/crash");
        }

        // ============================================
        // NO 23 -> TC_F10.14_01: Tìm kiếm real-time hiển thị đúng kết quả ngay khi gõ mã đơn
        // ============================================
        [Test]
        public void TC_F10_14_01_TimKiemRealTimeHienThiDungKetQua()
        {
            CurrentTestCaseId = "TC_F10.14_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            // Lấy mã đơn đầu tiên để dùng làm từ khóa tìm kiếm
            string firstOrderCode = _orderListPage.GetOrderCodeOfRow(0);

            _orderListPage.SearchByOrderCode(firstOrderCode);
            Thread.Sleep(1500);

            int resultCount = _orderListPage.GetAllRows().Count;
            string firstResultCode = _orderListPage.GetOrderCodeOfRow(0);

            CurrentActualResult = $"Tìm kiếm: '{firstOrderCode}' | Kết quả: {resultCount} | Mã đơn đầu tiên: {firstResultCode}";

            Assert.That(resultCount, Is.GreaterThan(0),
                $"[TC_F10.14_01] Không có kết quả nào khi tìm kiếm mã đơn: {firstOrderCode}");
            Assert.That(firstResultCode, Does.Contain(firstOrderCode),
                $"[TC_F10.14_01] Kết quả đầu tiên '{firstResultCode}' không chứa từ khóa '{firstOrderCode}'");
        }

        // ============================================
        // NO 24 -> TC_F10.14_02: Tìm kiếm không phân biệt hoa thường
        // ============================================
        [Test]
        public void TC_F10_14_02_TimKiemKhongPhanBietHoaThuong()
        {
            CurrentTestCaseId = "TC_F10.14_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            // Tìm bằng chữ HOA
            _orderListPage.SearchByOrderCode("ORD");
            Thread.Sleep(1500);
            int upperCount = _orderListPage.GetAllRows().Count;

            // Xóa và tìm bằng chữ thường
            _orderListPage.SearchByOrderCode("ord");
            Thread.Sleep(1500);
            int lowerCount = _orderListPage.GetAllRows().Count;

            CurrentActualResult = $"Kết quả 'ORD': {upperCount} | Kết quả 'ord': {lowerCount}";

            Assert.That(lowerCount, Is.EqualTo(upperCount),
                $"[TC_F10.14_02] Kết quả tìm kiếm hoa/thường khác nhau: HOA={upperCount}, thường={lowerCount}");
        }

        // ============================================
        // NO 25 -> TC_F10.14_03: Xóa nội dung ô tìm kiếm trả về toàn bộ danh sách
        // ============================================
        [Test]
        public void TC_F10_14_03_XoaTimKiemTraVeToànBoDanhSach()
        {
            CurrentTestCaseId = "TC_F10.14_03";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            int totalBefore = _orderListPage.GetTotalOrderCount();

            _orderListPage.SearchByOrderCode("ORD");
            Thread.Sleep(1500);

            // Xóa toàn bộ nội dung ô tìm kiếm
            _orderListPage.SearchByOrderCode("");
            Thread.Sleep(1500);

            int totalAfter = _orderListPage.GetTotalOrderCount();

            CurrentActualResult = $"Tổng ban đầu: {totalBefore} | Sau khi xóa tìm kiếm: {totalAfter}";

            Assert.That(totalAfter, Is.EqualTo(totalBefore),
                $"[TC_F10.14_03] Sau khi xóa tìm kiếm, số đơn ({totalAfter}) không khớp với ban đầu ({totalBefore})");
        }

        // ============================================
        // NO 26 -> TC_F10.15_01: Tìm kiếm mã đơn không tồn tại hiển thị bảng trống
        // ============================================
        [Test]
        public void TC_F10_15_01_TimKiemMaDonKhongTonTaiBangTrong()
        {
            CurrentTestCaseId = "TC_F10.15_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            string notExistCode = data.GetValueOrDefault("searchKeyword", "ORD-99999999-ZZZZZZZZ");
            _orderListPage.SearchByOrderCode(notExistCode);
            Thread.Sleep(1500);

            int resultCount = _orderListPage.GetAllRows().Count;
            bool pageHealthy = _orderListPage.IsPageHealthy();

            CurrentActualResult = $"Tìm kiếm: '{notExistCode}' | Số kết quả: {resultCount} | Trang ổn: {pageHealthy}";

            Assert.That(pageHealthy, Is.True,
                "[TC_F10.15_01] Trang bị lỗi khi tìm kiếm mã đơn không tồn tại");
            Assert.That(resultCount, Is.EqualTo(0),
                $"[TC_F10.15_01] Vẫn còn {resultCount} kết quả khi tìm kiếm mã không tồn tại");
        }

        // ============================================
        // NO 27 -> TC_F10.15_02: Tìm kiếm từ khóa ngẫu nhiên không liên quan hiển thị bảng trống
        // ============================================
        [Test]
        public void TC_F10_15_02_TimKiemTuKhoaNgauNhienBangTrong()
        {
            CurrentTestCaseId = "TC_F10.15_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            string randomKeyword = data.GetValueOrDefault("searchKeyword", "ABCDEFGH12345678");
            _orderListPage.SearchByOrderCode(randomKeyword);
            Thread.Sleep(1500);

            int resultCount = _orderListPage.GetAllRows().Count;
            bool pageHealthy = _orderListPage.IsPageHealthy();

            CurrentActualResult = $"Tìm kiếm: '{randomKeyword}' | Số kết quả: {resultCount} | Trang ổn: {pageHealthy}";

            Assert.That(pageHealthy, Is.True,
                "[TC_F10.15_02] Trang bị lỗi khi tìm kiếm từ khóa ngẫu nhiên");
            Assert.That(resultCount, Is.EqualTo(0),
                $"[TC_F10.15_02] Vẫn còn {resultCount} kết quả khi tìm kiếm từ khóa ngẫu nhiên không liên quan");
        }

        // Hàm hỗ trợ đọc JSON
        private Dictionary<string, string> DocDuLieu(string tcId)
        {
            return JsonHelper.DocDuLieu(DataPath, tcId);
        }
    }
}
