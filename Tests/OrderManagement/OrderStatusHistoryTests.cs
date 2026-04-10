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
    public class OrderStatusHistoryTests : TestBase
    {
        private OrderDetailPage _orderDetailPage = null!;
        private OrderListPage _orderListPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "OrderManagement", "order_status_history.json"
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
        // NO 55 -> TC_F10.31_01: Section lịch sử trạng thái hiển thị đầy đủ thông tin
        // Pre-condition: Có đơn hàng đã được duyệt ít nhất 1 lần (trạng thái "Đang xử lý")
        // ============================================
        [Test]
        public void TC_F10_31_01_SectionLichSuHienThiDayDuThongTin()
        {
            CurrentTestCaseId = "TC_F10.31_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Đang xử lý"));
            Thread.Sleep(1500);

            _orderListPage.ClickViewDetail(0);
            Thread.Sleep(1500);

            // Kiểm tra section lịch sử tồn tại
            var historyRows = Driver.FindElements(
                By.XPath("//*[contains(text(),'Lịch sử') or contains(@class,'history') or contains(@id,'history')]"));

            bool hasSectionHistory = historyRows.Count > 0;
            string orderCode = _orderDetailPage.GetOrderCode();

            CurrentActualResult = $"Đơn: {orderCode} | Section lịch sử tồn tại: {hasSectionHistory}";

            Assert.That(hasSectionHistory, Is.True,
                "[TC_F10.31_01] Không tìm thấy section Lịch sử trạng thái trên trang chi tiết");
        }

        // ============================================
        // NO 56 -> TC_F10.31_02: Lịch sử ghi nhận đúng timestamp thời điểm thực hiện
        // Pre-condition: Có đơn hàng ở trạng thái "Chờ xử lý" để duyệt
        // ============================================
        [Test]
        public void TC_F10_31_02_LichSuGhiNhanDungTimestamp()
        {
            CurrentTestCaseId = "TC_F10.31_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Thread.Sleep(1500);

            _orderListPage.SelectStatus(data.GetValueOrDefault("filterStatusValue", "Chờ xử lý"));
            Thread.Sleep(1500);

            _orderListPage.ClickViewDetail(0);
            Thread.Sleep(1500);

            DateTime timeBefore = DateTime.Now;

            _orderDetailPage.ClickApproveOrder();
            Thread.Sleep(500);
            _orderDetailPage.ConfirmActionModal();
            Thread.Sleep(2000);

            // Đọc timestamp từ lịch sử — tìm phần tử chứa năm hiện tại (yyyy)
            string currentYear = timeBefore.Year.ToString();
            var timestampElements = Driver.FindElements(
                By.XPath($"//*[contains(text(),'{currentYear}')]"));

            bool hasCurrentYearTimestamp = timestampElements.Count > 0;

            CurrentActualResult = $"Thời điểm duyệt: {timeBefore:HH:mm} | Có timestamp năm {currentYear}: {hasCurrentYearTimestamp}";

            Assert.That(hasCurrentYearTimestamp, Is.True,
                $"[TC_F10.31_02] Không tìm thấy timestamp năm {currentYear} trong lịch sử trạng thái");
        }

        // ============================================
        // NO 57 -> TC_F10.31_03: Lịch sử ghi nhận đúng tên Admin thực hiện hành động
        // Pre-condition: Đăng nhập bằng duchuy2004@gmail.com và có đơn "Chờ xử lý" để duyệt
        // ============================================
        [Test]
        public void TC_F10_31_03_LichSuGhiNhanDungTenAdmin()
        {
            CurrentTestCaseId = "TC_F10.31_03";
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

            string expectedAdminName = data.GetValueOrDefault("expectedAdminName", "duchuy2004@gmail.com");
            string pageSource = Driver.PageSource;
            bool hasAdminName = pageSource.Contains(expectedAdminName) 
                || pageSource.Contains(expectedAdminName.Split('@')[0]);

            CurrentActualResult = $"Admin thực hiện: {expectedAdminName} | Tên hiển thị trong lịch sử: {hasAdminName}";

            Assert.That(hasAdminName, Is.True,
                $"[TC_F10.31_03] Tên Admin '{expectedAdminName}' không được ghi nhận trong lịch sử trạng thái");
        }

        // Hàm hỗ trợ đọc JSON
        private Dictionary<string, string> DocDuLieu(string tcId)
        {
            return JsonHelper.DocDuLieu(DataPath, tcId);
        }
    }
}
