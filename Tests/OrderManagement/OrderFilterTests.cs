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
    public class OrderFilterTests : TestBase
    {
        private OrderListPage _orderListPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "OrderManagement", "order_filter.json"
        );

        [SetUp]
        public void SetUpPages()
        {
            CurrentSheetName = "TC_OrderManagement";
            _orderListPage = new OrderListPage(Driver, BaseUrl);
            LoginAsAdmin();
        }

        // ============================================
        // NO 28 -> TC_F10.16_01: Lọc trạng thái "Chờ xử lý"
        // ============================================
        [Test]
        public void TC_F10_16_01_LocTrangThaiChoXuLy()
        {
            CurrentTestCaseId = "TC_F10.16_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Chờ xử lý"));
            Thread.Sleep(1500);

            var rows = _orderListPage.GetAllRows();
            int rowCount = rows.Count;

            // Kiểm tra tất cả badge trạng thái đều là "Chờ xử lý"
            bool allMatch = true;
            for (int i = 0; i < rowCount; i++)
            {
                string badge = _orderListPage.GetStatusBadgeOfRow(i);
                if (!badge.Contains("Chờ xử lý"))
                {
                    allMatch = false;
                    break;
                }
            }

            CurrentActualResult = $"Số hàng sau lọc: {rowCount} | Tất cả là Chờ xử lý: {allMatch}";

            Assert.That(allMatch, Is.True,
                "[TC_F10.16_01] Có hàng không phải trạng thái 'Chờ xử lý' sau khi lọc");
        }

        // ============================================
        // NO 29 -> TC_F10.16_02: Lọc trạng thái "Đã hủy"
        // ============================================
        [Test]
        public void TC_F10_16_02_LocTrangThaiDaHuy()
        {
            CurrentTestCaseId = "TC_F10.16_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Đã hủy"));
            Thread.Sleep(1500);

            var rows = _orderListPage.GetAllRows();
            int rowCount = rows.Count;

            bool allMatch = true;
            for (int i = 0; i < rowCount; i++)
            {
                string badge = _orderListPage.GetStatusBadgeOfRow(i);
                if (!badge.Contains("Đã hủy"))
                {
                    allMatch = false;
                    break;
                }
            }

            CurrentActualResult = $"Số hàng sau lọc: {rowCount} | Tất cả là Đã hủy: {allMatch}";

            Assert.That(allMatch, Is.True,
                "[TC_F10.16_02] Có hàng không phải trạng thái 'Đã hủy' sau khi lọc");
        }

        // ============================================
        // NO 30 -> TC_F10.16_03: Chọn "Tất cả" trạng thái hiển thị lại toàn bộ
        // ============================================
        [Test]
        public void TC_F10_16_03_ChonTatCaTrangThaiHienThiLai()
        {
            CurrentTestCaseId = "TC_F10.16_03";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            int totalBefore = _orderListPage.GetTotalOrderCount();

            // Lọc theo Chờ xử lý
            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusBefore", "Chờ xử lý"));
            Thread.Sleep(1500);

            int filteredCount = _orderListPage.GetTotalOrderCount();

            // Reset về Tất cả
            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusAll", "Tất cả"));
            Thread.Sleep(1500);

            int totalAfter = _orderListPage.GetTotalOrderCount();

            CurrentActualResult = $"Tổng ban đầu: {totalBefore} | Sau lọc: {filteredCount} | Sau reset: {totalAfter}";

            Assert.That(totalAfter, Is.EqualTo(totalBefore),
                $"[TC_F10.16_03] Sau khi chọn 'Tất cả', số đơn ({totalAfter}) không khớp với ban đầu ({totalBefore})");
        }

        // ============================================
        // NO 31 -> TC_F10.17_01: Lọc thanh toán "Chờ thanh toán"
        // ============================================
        [Test]
        public void TC_F10_17_01_LocThanhToanChoThanhToan()
        {
            CurrentTestCaseId = "TC_F10.17_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            _orderListPage.SelectPaymentStatus(data.GetValueOrDefault("filterPaymentStatusValue", "Chờ thanh toán"));
            Thread.Sleep(1500);

            var rows = _orderListPage.GetAllRows();
            int rowCount = rows.Count;

            bool allMatch = true;
            for (int i = 0; i < rowCount; i++)
            {
                string badge = _orderListPage.GetPaymentBadgeOfRow(i);
                if (!badge.Contains("Chờ thanh toán"))
                {
                    allMatch = false;
                    break;
                }
            }

            CurrentActualResult = $"Số hàng sau lọc: {rowCount} | Tất cả là Chờ thanh toán: {allMatch}";

            Assert.That(allMatch, Is.True,
                "[TC_F10.17_01] Có hàng không phải trạng thái thanh toán 'Chờ thanh toán' sau khi lọc");
        }

        // ============================================
        // NO 32 -> TC_F10.17_02: Lọc thanh toán "Đã thanh toán"
        // ============================================
        [Test]
        public void TC_F10_17_02_LocThanhToanDaThanhToan()
        {
            CurrentTestCaseId = "TC_F10.17_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            _orderListPage.SelectPaymentStatus(data.GetValueOrDefault("filterPaymentStatusValue", "Đã thanh toán"));
            Thread.Sleep(1500);

            var rows = _orderListPage.GetAllRows();
            int rowCount = rows.Count;

            bool allMatch = true;
            for (int i = 0; i < rowCount; i++)
            {
                string badge = _orderListPage.GetPaymentBadgeOfRow(i);
                if (!badge.Contains("Đã thanh toán"))
                {
                    allMatch = false;
                    break;
                }
            }

            CurrentActualResult = $"Số hàng sau lọc: {rowCount} | Tất cả là Đã thanh toán: {allMatch}";

            Assert.That(allMatch, Is.True,
                "[TC_F10.17_02] Có hàng không phải trạng thái thanh toán 'Đã thanh toán' sau khi lọc");
        }

        // ============================================
        // NO 33 -> TC_F10.17_03: Chọn "Tất cả" thanh toán hiển thị lại toàn bộ
        // ============================================
        [Test]
        public void TC_F10_17_03_ChonTatCaThanhToanHienThiLai()
        {
            CurrentTestCaseId = "TC_F10.17_03";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            int totalBefore = _orderListPage.GetTotalOrderCount();

            _orderListPage.SelectPaymentStatus(data.GetValueOrDefault("filterPaymentStatusBefore", "Chờ thanh toán"));
            Thread.Sleep(1500);

            _orderListPage.SelectPaymentStatus(data.GetValueOrDefault("filterPaymentStatusAll", "Tất cả"));
            Thread.Sleep(1500);

            int totalAfter = _orderListPage.GetTotalOrderCount();

            CurrentActualResult = $"Tổng ban đầu: {totalBefore} | Sau reset: {totalAfter}";

            Assert.That(totalAfter, Is.EqualTo(totalBefore),
                $"[TC_F10.17_03] Sau khi chọn 'Tất cả', số đơn ({totalAfter}) không khớp với ban đầu ({totalBefore})");
        }

        // ============================================
        // NO 34 -> TC_F10.18_01: Lọc khoảng ngày hợp lệ
        // ============================================
        [Test]
        public void TC_F10_18_01_LocKhoangNgayHopLe()
        {
            CurrentTestCaseId = "TC_F10.18_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            string fromDate = data.GetValueOrDefault("fromDate", "01/01/2025");
            string toDate = data.GetValueOrDefault("toDate", "12/31/2025");

            _orderListPage.SetFromDate(fromDate);
            _orderListPage.SetToDate(toDate);
            Thread.Sleep(1500);

            bool pageHealthy = _orderListPage.IsPageHealthy();
            int rowCount = _orderListPage.GetAllRows().Count;

            CurrentActualResult = $"Từ {fromDate} đến {toDate} | Số đơn: {rowCount} | Trang ổn: {pageHealthy}";

            Assert.That(pageHealthy, Is.True,
                "[TC_F10.18_01] Trang bị lỗi khi lọc khoảng ngày hợp lệ");
        }

        // ============================================
        // NO 35 -> TC_F10.18_02: Lọc chỉ "Từ ngày" không có "Đến ngày"
        // ============================================
        [Test]
        public void TC_F10_18_02_LocChiTuNgayKhongDenNgay()
        {
            CurrentTestCaseId = "TC_F10.18_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            string fromDate = data.GetValueOrDefault("fromDate", "03/01/2025");

            _orderListPage.SetFromDate(fromDate);
            // Không set toDate — để trống
            Thread.Sleep(1500);

            bool pageHealthy = _orderListPage.IsPageHealthy();
            int rowCount = _orderListPage.GetAllRows().Count;

            CurrentActualResult = $"Từ ngày {fromDate}, Đến ngày: trống | Số đơn: {rowCount} | Trang ổn: {pageHealthy}";

            Assert.That(pageHealthy, Is.True,
                "[TC_F10.18_02] Trang bị lỗi khi chỉ nhập Từ ngày mà không có Đến ngày");
        }

        // ============================================
        // NO 36 -> TC_F10.19_01: Kết hợp lọc trạng thái đơn + thanh toán
        // ============================================
        [Test]
        public void TC_F10_19_01_KetHopLocTrangThaiVaThanhToan()
        {
            CurrentTestCaseId = "TC_F10.19_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            string filterStatus = data.GetValueOrDefault("filterStatusValue", "Chờ xử lý");
            string filterPayment = data.GetValueOrDefault("filterPaymentStatusValue", "Chờ thanh toán");

            _orderListPage.SelectStatus(filterStatus);
            Thread.Sleep(500);
            _orderListPage.SelectPaymentStatus(filterPayment);
            Thread.Sleep(1500);

            var rows = _orderListPage.GetAllRows();
            int rowCount = rows.Count;

            bool allMatch = true;
            for (int i = 0; i < rowCount; i++)
            {
                string statusBadge = _orderListPage.GetStatusBadgeOfRow(i);
                string paymentBadge = _orderListPage.GetPaymentBadgeOfRow(i);
                if (!statusBadge.Contains(filterStatus) || !paymentBadge.Contains(filterPayment))
                {
                    allMatch = false;
                    break;
                }
            }

            CurrentActualResult = $"Số đơn lọc được: {rowCount} | Cả 2 điều kiện đúng: {allMatch}";

            Assert.That(allMatch, Is.True,
                "[TC_F10.19_01] Có hàng không thỏa mãn đồng thời cả 2 điều kiện lọc");
        }

        // ============================================
        // NO 37 -> TC_F10.19_02: Kết hợp 3 bộ lọc (trạng thái + thanh toán + ngày)
        // ============================================
        [Test]
        public void TC_F10_19_02_KetHop3BoLoc()
        {
            CurrentTestCaseId = "TC_F10.19_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Đã hủy"));
            Thread.Sleep(500);
            _orderListPage.SelectPaymentStatus(data.GetValueOrDefault("filterPaymentStatusValue", "Chờ thanh toán"));
            Thread.Sleep(500);
            _orderListPage.SetFromDate(data.GetValueOrDefault("fromDate", "01/01/2025"));
            _orderListPage.SetToDate(data.GetValueOrDefault("toDate", "01/31/2025"));
            Thread.Sleep(1500);

            bool pageHealthy = _orderListPage.IsPageHealthy();
            int rowCount = _orderListPage.GetAllRows().Count;

            CurrentActualResult = $"Số đơn với 3 bộ lọc: {rowCount} | Trang ổn: {pageHealthy}";

            Assert.That(pageHealthy, Is.True,
                "[TC_F10.19_02] Trang bị lỗi khi áp dụng 3 bộ lọc cùng lúc");
        }

        // ============================================
        // NO 38 -> TC_F10.20_01: "Từ ngày" sau "Đến ngày" báo lỗi
        // ============================================
        [Test]
        public void TC_F10_20_01_TuNgaySauDenNgayBaoLoi()
        {
            CurrentTestCaseId = "TC_F10.20_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            _orderListPage.SetFromDate(data.GetValueOrDefault("fromDate", "12/31/2025"));
            _orderListPage.SetToDate(data.GetValueOrDefault("toDate", "01/01/2025"));
            Thread.Sleep(1500);

            // Kết quả hợp lệ: hoặc trang báo lỗi, hoặc không có kết quả nào
            bool pageHealthy = _orderListPage.IsPageHealthy();
            int rowCount = _orderListPage.GetAllRows().Count;

            string pageSource = Driver.PageSource.ToLower();
            bool hasErrorMsg = pageSource.Contains("không hợp lệ") || pageSource.Contains("invalid")
                || pageSource.Contains("lỗi") || rowCount == 0;

            CurrentActualResult = $"Số đơn trả về: {rowCount} | Báo lỗi hoặc rỗng: {hasErrorMsg} | Trang ổn: {pageHealthy}";

            // Mong đợi: hệ thống không crash và trả về 0 kết quả hoặc báo lỗi
            Assert.That(pageHealthy, Is.True,
                "[TC_F10.20_01] Trang bị crash khi nhập Từ ngày sau Đến ngày");
            Assert.That(hasErrorMsg, Is.True,
                "[TC_F10.20_01] Hệ thống không báo lỗi hay trả về rỗng khi Từ ngày > Đến ngày");
        }

        // ============================================
        // NO 39 -> TC_F10.20_02: Từ ngày = Đến ngày (cùng 1 ngày) vẫn hoạt động đúng
        // ============================================
        [Test]
        public void TC_F10_20_02_TuNgayBangDenNgay()
        {
            CurrentTestCaseId = "TC_F10.20_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            string sameDate = data.GetValueOrDefault("fromDate", "03/15/2025");
            _orderListPage.SetFromDate(sameDate);
            _orderListPage.SetToDate(data.GetValueOrDefault("toDate", "03/15/2025"));
            Thread.Sleep(1500);

            bool pageHealthy = _orderListPage.IsPageHealthy();
            int rowCount = _orderListPage.GetAllRows().Count;

            CurrentActualResult = $"Lọc cùng ngày {sameDate} | Số đơn: {rowCount} | Trang ổn: {pageHealthy}";

            Assert.That(pageHealthy, Is.True,
                "[TC_F10.20_02] Trang bị lỗi khi Từ ngày = Đến ngày");
        }

        // Hàm hỗ trợ đọc JSON
        private Dictionary<string, string> DocDuLieu(string tcId)
        {
            return JsonHelper.DocDuLieu(DataPath, tcId);
        }
    }
}
