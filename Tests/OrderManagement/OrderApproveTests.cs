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
            Thread.Sleep(1500);

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Chờ xử lý"));
            Thread.Sleep(1500);

            _orderListPage.ClickViewDetail(0);
            Thread.Sleep(1500);

            bool isApproveVisible = _orderDetailPage.IsApproveButtonVisible();
            string orderStatus = _orderDetailPage.GetOrderStatus();

            CurrentActualResult = $"Trạng thái đơn: {orderStatus} | Nút Duyệt hiển thị: {isApproveVisible}";

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
            Thread.Sleep(1500);

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Chờ xử lý"));
            Thread.Sleep(1500);

            _orderListPage.ClickViewDetail(0);
            Thread.Sleep(1500);

            _orderDetailPage.ClickApproveOrder();
            Thread.Sleep(500);
            _orderDetailPage.ConfirmActionModal();
            Thread.Sleep(2000);

            string newStatus = _orderDetailPage.GetOrderStatus();
            bool isApproveHidden = !_orderDetailPage.IsApproveButtonVisible();

            CurrentActualResult = $"Trạng thái sau duyệt: {newStatus} | Nút duyệt đã ẩn: {isApproveHidden}";

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
            Thread.Sleep(1500);

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Chờ xử lý"));
            Thread.Sleep(1500);

            _orderListPage.ClickViewDetail(0);
            Thread.Sleep(1500);

            _orderDetailPage.ClickApproveOrder();
            Thread.Sleep(500);
            _orderDetailPage.ConfirmActionModal();
            Thread.Sleep(2000);

            // Cuộn xuống section lịch sử và kiểm tra
            var historySection = Driver.FindElements(
                By.XPath("//*[contains(text(),'Lịch sử') or contains(text(),'lich su')]"));
            bool hasHistorySection = historySection.Count > 0;

            CurrentActualResult = $"Section lịch sử tồn tại: {hasHistorySection} | Trạng thái mới: {_orderDetailPage.GetOrderStatus()}";

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
            Thread.Sleep(1500);

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Đã hủy"));
            Thread.Sleep(1500);

            _orderListPage.ClickViewDetail(0);
            Thread.Sleep(1500);

            bool isApproveVisible = _orderDetailPage.IsApproveButtonVisible();
            string orderStatus = _orderDetailPage.GetOrderStatus();

            CurrentActualResult = $"Trạng thái đơn: {orderStatus} | Nút Duyệt hiển thị: {isApproveVisible}";

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
            Thread.Sleep(1500);

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Đang xử lý"));
            Thread.Sleep(1500);

            _orderListPage.ClickViewDetail(0);
            Thread.Sleep(1500);

            bool isApproveVisible = _orderDetailPage.IsApproveButtonVisible();
            string orderStatus = _orderDetailPage.GetOrderStatus();

            CurrentActualResult = $"Trạng thái đơn: {orderStatus} | Nút Duyệt hiển thị: {isApproveVisible}";

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
