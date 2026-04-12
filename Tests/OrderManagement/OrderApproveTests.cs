using NUnit.Framework;
using SeleniumProject.Pages.OrderManagement;
using SeleniumProject.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SeleniumProject.Tests.OrderManagement
{
    [TestFixture]
    public class OrderApproveTests : TestBase
    {
        private OrderDetailPage _orderDetailPage = null!;
        private OrderListPage _orderListPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "OrderManagement", "order_approve.json"
        );

        [SetUp]
        public void SetUpPages()
        {
            CurrentSheetName = "TC_OrderManagement";
            _orderDetailPage = new OrderDetailPage(Driver);
            _orderListPage = new OrderListPage(Driver, BaseUrl);
            LoginAsAdmin();
        }

        // ============================================
        // NO 40 -> TC_F10.22_01: Nút "Duyệt đơn & Chuẩn bị hàng" hiển thị khi trạng thái "Chờ xử lý"
        // ============================================
        [Test]
        public void TC_F10_22_01_NutDuyetDonHienThiKhiChoXuLy()
        {
            CurrentTestCaseId = "TC_F10.22_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Chờ xử lý"));
            Thread.Sleep(1000);

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            bool isApproveVisible = _orderDetailPage.IsApproveButtonVisible();
            string orderStatus = _orderDetailPage.GetOrderStatus();

            CurrentActualResult = isApproveVisible
                ? $"Đơn hàng đang ở trạng thái '{orderStatus}', nút Duyệt đơn hiển thị."
                : $"Đơn hàng đang ở trạng thái '{orderStatus}', nút Duyệt đơn không hiển thị (không đúng kỳ vọng).";

            Assert.That(isApproveVisible, Is.True,
                "[TC_F10.22_01] Nút 'Duyệt đơn & Chuẩn bị hàng' không hiển thị khi đơn ở trạng thái Chờ xử lý");
        }

        // ============================================
        // NO 41 -> TC_F10.22_02: Trạng thái chuyển sang "Đang xử lý" sau khi duyệt
        // Pre-condition: Có đơn hàng ở trạng thái "Chờ xử lý"
        // ============================================
        [Test]
        public void TC_F10_22_02_TrangThaiChuyenSangDangXuLySauKhiDuyet()
        {
            CurrentTestCaseId = "TC_F10.22_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Chờ xử lý"));
            Thread.Sleep(1000);

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            _orderDetailPage.ClickApproveOrder();
            Thread.Sleep(500);
            _orderDetailPage.ConfirmActionModal();
            Thread.Sleep(2000);

            string newStatus = _orderDetailPage.GetOrderStatus();
            bool isApproveHidden = !_orderDetailPage.IsApproveButtonVisible();

            CurrentActualResult = $"Sau khi duyệt đơn, trạng thái chuyển thành '{newStatus}'. Nút Duyệt đơn đã ẩn đi: {isApproveHidden}.";

            Assert.That(newStatus, Does.Contain("Đang xử lý"),
                $"[TC_F10.22_02] Trạng thái không chuyển thành 'Đang xử lý', hiện tại: {newStatus}");
            Assert.That(isApproveHidden, Is.True,
                "[TC_F10.22_02] Nút 'Duyệt đơn' vẫn còn hiển thị sau khi đã duyệt");
        }

        // ============================================
        // NO 42 -> TC_F10.22_03: Lịch sử trạng thái ghi lại hành động duyệt đơn
        // Pre-condition: Có đơn hàng ở trạng thái "Chờ xử lý"
        // ============================================
        [Test]
        public void TC_F10_22_03_LichSuGhiLaiHanhDongDuyetDon()
        {
            CurrentTestCaseId = "TC_F10.22_03";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Chờ xử lý"));
            Thread.Sleep(1000);

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            _orderDetailPage.ClickApproveOrder();
            Thread.Sleep(500);
            _orderDetailPage.ConfirmActionModal();
            Thread.Sleep(2000);

            bool hasHistorySection = _orderDetailPage.HasHistorySection();

            CurrentActualResult = hasHistorySection
                ? $"Sau khi duyệt đơn, section lịch sử trạng thái xuất hiện. Trạng thái hiện tại: {_orderDetailPage.GetOrderStatus()}."
                : $"Sau khi duyệt đơn, không tìm thấy section lịch sử trạng thái.";

            Assert.That(hasHistorySection, Is.True,
                "[TC_F10.22_03] Không tìm thấy section Lịch sử trạng thái sau khi duyệt đơn");
        }

        // ============================================
        // NO 43 -> TC_F10.23_01: Nút "Duyệt đơn" không hiển thị trên đơn "Đã hủy"
        // ============================================
        [Test]
        public void TC_F10_23_01_NutDuyetAnKhiDonDaHuy()
        {
            CurrentTestCaseId = "TC_F10.23_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Đã hủy"));
            Thread.Sleep(1000);

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            bool isApproveVisible = _orderDetailPage.IsApproveButtonVisible();
            string orderStatus = _orderDetailPage.GetOrderStatus();

            CurrentActualResult = isApproveVisible
                ? $"Đơn hàng đang ở trạng thái '{orderStatus}', nút Duyệt đơn vẫn hiển thị (không đúng kỳ vọng)."
                : $"Đơn hàng đang ở trạng thái '{orderStatus}', nút Duyệt đơn đã được ẩn đúng.";

            Assert.That(isApproveVisible, Is.False,
                "[TC_F10.23_01] Nút 'Duyệt đơn' vẫn hiển thị trên đơn đã hủy");
        }

        // ============================================
        // NO 44 -> TC_F10.23_02: Nút "Duyệt đơn" không hiển thị trên đơn "Đang xử lý"
        // ============================================
        [Test]
        public void TC_F10_23_02_NutDuyetAnKhiDonDangXuLy()
        {
            CurrentTestCaseId = "TC_F10.23_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Đang xử lý"));
            Thread.Sleep(1000);

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            bool isApproveVisible = _orderDetailPage.IsApproveButtonVisible();
            string orderStatus = _orderDetailPage.GetOrderStatus();

            CurrentActualResult = isApproveVisible
                ? $"Đơn hàng đang ở trạng thái '{orderStatus}', nút Duyệt đơn vẫn hiển thị (không đúng kỳ vọng)."
                : $"Đơn hàng đang ở trạng thái '{orderStatus}', nút Duyệt đơn đã được ẩn đúng.";

            Assert.That(isApproveVisible, Is.False,
                "[TC_F10.23_02] Nút 'Duyệt đơn' vẫn hiển thị trên đơn đang xử lý");
        }

        // Hàm hỗ trợ đọc JSON
        private Dictionary<string, string> DocDuLieu(string tcId)
        {
            return JsonHelper.DocDuLieu(DataPath, tcId);
        }
    }
}
