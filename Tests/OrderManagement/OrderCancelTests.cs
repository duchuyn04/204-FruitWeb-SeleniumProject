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
    public class OrderCancelTests : TestBase
    {
        private OrderDetailPage _orderDetailPage = null!;
        private OrderListPage _orderListPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "OrderManagement", "order_cancel.json"
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
        // NO 48 -> TC_F10.26_01: Modal xác nhận hủy đơn hiện ra khi nhấn nút "Hủy đơn hàng"
        // ============================================
        [Test]
        public void TC_F10_26_01_ModalHuyDonHienRa()
        {
            CurrentTestCaseId = "TC_F10.26_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Chờ xử lý"));
            Thread.Sleep(1000);

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            _orderDetailPage.ClickCancelOrder();
            Thread.Sleep(1000);

            bool isModalVisible = _orderDetailPage.IsCancelModalVisible();

            CurrentActualResult = isModalVisible
                ? "Sau khi nhấn nút Hủy đơn hàng, modal xác nhận hủy xuất hiện."
                : "Sau khi nhấn nút Hủy đơn hàng, modal xác nhận không xuất hiện (không đúng kỳ vọng).";

            Assert.That(isModalVisible, Is.True,
                "[TC_F10.26_01] Modal xác nhận hủy đơn không hiện ra sau khi click nút Hủy đơn hàng");
        }

        // ============================================
        // NO 49 -> TC_F10.26_02: Trạng thái chuyển thành "Đã hủy" sau khi xác nhận hủy
        // Pre-condition: Có đơn hàng ở trạng thái "Chờ xử lý"
        // ============================================
        [Test]
        public void TC_F10_26_02_TrangThaiChuyenDaHuySauKhiXacNhan()
        {
            CurrentTestCaseId = "TC_F10.26_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Chờ xử lý"));
            Thread.Sleep(1000);

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            _orderDetailPage.ClickCancelOrder();
            Thread.Sleep(1000);

            _orderDetailPage.ConfirmCancelOrder();
            Thread.Sleep(2000);

            string newStatus = _orderDetailPage.GetOrderStatus();

            CurrentActualResult = $"Sau khi xác nhận hủy, trạng thái đơn chuyển thành '{newStatus}'.";

            Assert.That(newStatus, Does.Contain("Đã hủy"),
                $"[TC_F10.26_02] Trạng thái không chuyển thành 'Đã hủy', hiện tại: {newStatus}");
        }

        // ============================================
        // NO 50 -> TC_F10.27_01: Click "Hủy bỏ" trong modal giữ nguyên trạng thái đơn
        // ============================================
        [Test]
        public void TC_F10_27_01_HuyBoTrongModalGiuNguyenTrangThai()
        {
            CurrentTestCaseId = "TC_F10.27_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Chờ xử lý"));
            Thread.Sleep(1000);

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            string statusBefore = _orderDetailPage.GetOrderStatus();

            _orderDetailPage.ClickCancelOrder();
            Thread.Sleep(1000);

            _orderDetailPage.DismissCancelModal();
            Thread.Sleep(1000);

            string statusAfter = _orderDetailPage.GetOrderStatus();

            CurrentActualResult = $"Trạng thái trước khi mở modal: '{statusBefore}'. Sau khi nhấn Hủy bỏ trong modal, trạng thái vẫn là: '{statusAfter}'.";

            Assert.That(statusAfter, Is.EqualTo(statusBefore),
                $"[TC_F10.27_01] Trạng thái đã thay đổi sau khi click Hủy bỏ: {statusAfter}");
        }

        // ============================================
        // NO 51 -> TC_F10.27_02: Nhấn Escape đóng modal mà không hủy đơn
        // ============================================
        [Test]
        public void TC_F10_27_02_NhanEscapeDongModalKhongHuyDon()
        {
            CurrentTestCaseId = "TC_F10.27_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Chờ xử lý"));
            Thread.Sleep(1000);

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            string statusBefore = _orderDetailPage.GetOrderStatus();

            _orderDetailPage.ClickCancelOrder();
            Thread.Sleep(1000);

            _orderDetailPage.PressEscapeKey();
            Thread.Sleep(1000);

            string statusAfter = _orderDetailPage.GetOrderStatus();
            bool isModalClosed = !_orderDetailPage.IsCancelModalVisible();

            CurrentActualResult = $"Trạng thái trước khi mở modal: '{statusBefore}'. Sau khi nhấn Escape, modal đóng: {isModalClosed}, trạng thái đơn vẫn là: '{statusAfter}'.";

            Assert.That(isModalClosed, Is.True,
                "[TC_F10.27_02] Modal không đóng sau khi nhấn Escape");
            Assert.That(statusAfter, Is.EqualTo(statusBefore),
                $"[TC_F10.27_02] Trạng thái đã thay đổi sau khi nhấn Escape: {statusAfter}");
        }

        // Hàm hỗ trợ đọc JSON
        private Dictionary<string, string> DocDuLieu(string tcId)
        {
            return JsonHelper.DocDuLieu(DataPath, tcId);
        }
    }
}
