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
            Thread.Sleep(1500);

            _orderListPage.SelectPaymentStatus(data.GetValueOrDefault("filterPaymentStatusValue", "Chờ thanh toán"));
            Thread.Sleep(1500);

            _orderListPage.ClickViewDetail(0);
            Thread.Sleep(1500);

            bool isPaymentBtnVisible = _orderDetailPage.IsConfirmPaymentButtonVisible();
            string paymentStatus = _orderDetailPage.GetPaymentStatus();

            CurrentActualResult = $"Trạng thái thanh toán: {paymentStatus} | Nút thu tiền hiển thị: {isPaymentBtnVisible}";

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
            Thread.Sleep(1500);

            _orderListPage.SelectPaymentStatus(data.GetValueOrDefault("filterPaymentStatusValue", "Chờ thanh toán"));
            Thread.Sleep(1500);

            _orderListPage.ClickViewDetail(0);
            Thread.Sleep(1500);

            _orderDetailPage.ClickConfirmPayment();
            Thread.Sleep(1000);

            bool isModalVisible = _orderDetailPage.IsPaymentModalVisible();

            CurrentActualResult = $"Modal xác nhận thu tiền hiển thị: {isModalVisible}";

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
            Thread.Sleep(1500);

            _orderListPage.SelectPaymentStatus(data.GetValueOrDefault("filterPaymentStatusValue", "Chờ thanh toán"));
            Thread.Sleep(1500);

            _orderListPage.ClickViewDetail(0);
            Thread.Sleep(1500);

            _orderDetailPage.ClickConfirmPayment();
            Thread.Sleep(1000);

            _orderDetailPage.ConfirmPaymentCollection();
            Thread.Sleep(2000);

            string newPaymentStatus = _orderDetailPage.GetPaymentStatus();
            bool isPaymentBtnHidden = !_orderDetailPage.IsConfirmPaymentButtonVisible();

            CurrentActualResult = $"Trạng thái thanh toán sau xác nhận: {newPaymentStatus} | Nút thu tiền đã ẩn: {isPaymentBtnHidden}";

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
