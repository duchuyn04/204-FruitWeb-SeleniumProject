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
    public class OrderPaymentTests : TestBase
    {
        private OrderDetailPage _orderDetailPage = null!;
        private OrderListPage _orderListPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "OrderManagement", "order_payment.json"
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
        // NO 45 -> TC_F10.24_01: Nút "Xác nhận đã thu tiền" hiển thị khi thanh toán "Chờ thanh toán"
        // ============================================
        [Test]
        public void TC_F10_24_01_NutXacNhanThuTienHienThi()
        {
            CurrentTestCaseId = "TC_F10.24_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.SelectPaymentStatus(data.GetValueOrDefault("filterPaymentStatusValue", "Chờ thanh toán"));
            Thread.Sleep(1000);

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            bool isPaymentBtnVisible = _orderDetailPage.IsConfirmPaymentButtonVisible();
            string paymentStatus = _orderDetailPage.GetPaymentStatus();

            CurrentActualResult = isPaymentBtnVisible
                ? $"Đơn có trạng thái thanh toán '{paymentStatus}', nút Xác nhận đã thu tiền hiển thị."
                : $"Đơn có trạng thái thanh toán '{paymentStatus}', nút Xác nhận đã thu tiền không hiển thị (không đúng kỳ vọng).";

            Assert.That(isPaymentBtnVisible, Is.True,
                "[TC_F10.24_01] Nút 'Xác nhận đã thu tiền' không hiển thị khi thanh toán là Chờ thanh toán");
        }

        // ============================================
        // NO 46 -> TC_F10.24_02: Modal xác nhận thu tiền hiện ra khi nhấn nút
        // ============================================
        [Test]
        public void TC_F10_24_02_ModalXacNhanThuTienHienRa()
        {
            CurrentTestCaseId = "TC_F10.24_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.SelectPaymentStatus(data.GetValueOrDefault("filterPaymentStatusValue", "Chờ thanh toán"));
            Thread.Sleep(1000);

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            _orderDetailPage.ClickConfirmPayment();
            Thread.Sleep(1000);

            bool isModalVisible = _orderDetailPage.IsPaymentModalVisible();

            CurrentActualResult = isModalVisible
                ? "Sau khi nhấn nút Xác nhận đã thu tiền, modal xác nhận xuất hiện."
                : "Sau khi nhấn nút Xác nhận đã thu tiền, modal không xuất hiện (không đúng kỳ vọng).";

            Assert.That(isModalVisible, Is.True,
                "[TC_F10.24_02] Modal xác nhận thu tiền không hiện ra sau khi click nút");
        }

        // ============================================
        // NO 47 -> TC_F10.24_03: Trạng thái thanh toán chuyển thành "Đã thanh toán" sau khi xác nhận
        // Pre-condition: Có đơn hàng với thanh toán "Chờ thanh toán"
        // ============================================
        [Test]
        public void TC_F10_24_03_TrangThaiThanhToanChuyenSauKhiXacNhan()
        {
            CurrentTestCaseId = "TC_F10.24_03";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.SelectPaymentStatus(data.GetValueOrDefault("filterPaymentStatusValue", "Chờ thanh toán"));
            Thread.Sleep(1000);

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            _orderDetailPage.ClickConfirmPayment();
            Thread.Sleep(1000);

            _orderDetailPage.ConfirmPaymentCollection();
            Thread.Sleep(2000);

            string newPaymentStatus = _orderDetailPage.GetPaymentStatus();
            bool isPaymentBtnHidden = !_orderDetailPage.IsConfirmPaymentButtonVisible();

            CurrentActualResult = $"Sau khi xác nhận thu tiền, trạng thái thanh toán chuyển thành '{newPaymentStatus}'. Nút thu tiền đã ẩn: {isPaymentBtnHidden}.";

            Assert.That(newPaymentStatus, Does.Contain("Đã thanh toán"),
                $"[TC_F10.24_03] Trạng thái thanh toán không chuyển thành 'Đã thanh toán', hiện tại: {newPaymentStatus}");
            Assert.That(isPaymentBtnHidden, Is.True,
                "[TC_F10.24_03] Nút 'Xác nhận đã thu tiền' vẫn hiển thị sau khi đã xác nhận");
        }

        // Hàm hỗ trợ đọc JSON
        private Dictionary<string, string> DocDuLieu(string tcId)
        {
            return JsonHelper.DocDuLieu(DataPath, tcId);
        }
    }
}
