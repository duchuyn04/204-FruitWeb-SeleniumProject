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
    public class OrderPrintTests : TestBase
    {
        private OrderDetailPage _orderDetailPage = null!;
        private OrderListPage _orderListPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "OrderManagement", "order_print.json"
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
        // NO 58 -> TC_F10.32_01: Nút "In đơn hàng" hiển thị trên trang chi tiết và có thể click
        // ============================================
        [Test]
        public void TC_F10_32_01_NutInDonHienThiVaClickDuoc()
        {
            CurrentTestCaseId = "TC_F10.32_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            bool btnExists = _orderDetailPage.IsPrintButtonExists();
            bool btnDisplayed = _orderDetailPage.IsPrintButtonDisplayed();
            bool btnEnabled = _orderDetailPage.IsPrintButtonEnabled();

            string printBtnText = data.GetValueOrDefault("printButtonText", "In đơn hàng");
            CurrentActualResult = $"Nút '{printBtnText}' - Tồn tại: {btnExists} | Hiển thị: {btnDisplayed} | Click được: {btnEnabled}";

            Assert.That(btnExists, Is.True,
                $"[TC_F10.32_01] Không tìm thấy nút '{printBtnText}' trên trang chi tiết đơn hàng");
            Assert.That(btnDisplayed, Is.True,
                $"[TC_F10.32_01] Nút '{printBtnText}' bị ẩn");
            Assert.That(btnEnabled, Is.True,
                $"[TC_F10.32_01] Nút '{printBtnText}' bị vô hiệu hoá (disabled)");
        }

        // ============================================
        // NO 59 -> TC_F10.32_02: Click nút "In đơn hàng" không làm thay đổi URL trang
        // (Xác nhận hành động in được kích hoạt mà không điều hướng sang trang khác)
        // ============================================
        [Test]
        public void TC_F10_32_02_ClickInDonKhongThayDoiUrl()
        {
            CurrentTestCaseId = "TC_F10.32_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            string urlBefore = Driver.Url;

            _orderDetailPage.ClickPrint();
            Thread.Sleep(1500);

            // Đóng dialog in nếu có (native print dialog không thể tương tác qua Selenium)
            // Chỉ kiểm tra URL không thay đổi
            string urlAfter = Driver.Url;

            CurrentActualResult = $"URL trước: {urlBefore} | URL sau click In: {urlAfter} | URL không đổi: {urlBefore == urlAfter}";

            Assert.That(urlAfter, Is.EqualTo(urlBefore),
                $"[TC_F10.32_02] URL trang đã thay đổi sau khi click In đơn hàng: {urlAfter}");
        }

        // Hàm hỗ trợ đọc JSON
        private Dictionary<string, string> DocDuLieu(string tcId)
        {
            return JsonHelper.DocDuLieu(DataPath, tcId);
        }
    }
}
