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
    public class OrderNoteTests : TestBase
    {
        private OrderDetailPage _orderDetailPage = null!;
        private OrderListPage _orderListPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "OrderManagement", "order_note.json"
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
        // NO 52 -> TC_F10.28_01: Textarea ghi chú hiển thị placeholder và bộ đếm ký tự
        // ============================================
        [Test]
        public void TC_F10_28_01_TextareaHienThiPlaceholderVaBoDem()
        {
            CurrentTestCaseId = "TC_F10.28_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            bool textareaExists = _orderDetailPage.IsNoteTextareaDisplayed();

            string charCounter = _orderDetailPage.GetNoteCharCount();
            string expectedCounter = data.GetValueOrDefault("expectedCounterInitial", "0/1000");

            CurrentActualResult = $"Textarea hiển thị: {textareaExists} | Bộ đếm: {charCounter}";

            Assert.That(textareaExists, Is.True,
                "[TC_F10.28_01] Không tìm thấy textarea ghi chú nội bộ");
            Assert.That(charCounter, Does.Contain(expectedCounter),
                $"[TC_F10.28_01] Bộ đếm ký tự không hiển thị đúng, kỳ vọng: {expectedCounter}, thực tế: {charCounter}");
        }

        // ============================================
        // NO 53 -> TC_F10.28_02: Bộ đếm ký tự cập nhật real-time khi nhập nội dung
        // ============================================
        [Test]
        public void TC_F10_28_02_BoDemKyTuCapNhatRealTime()
        {
            CurrentTestCaseId = "TC_F10.28_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            string noteText = data.GetValueOrDefault("noteInput", "Ghi chú kiểm tra");
            _orderDetailPage.EnterNote(noteText);
            Thread.Sleep(500);

            string charCounter = _orderDetailPage.GetNoteCharCount();
            int expectedCount = noteText.Length;

            CurrentActualResult = $"Nhập: '{noteText}' ({expectedCount} ký tự) | Bộ đếm hiển thị: {charCounter}";

            Assert.That(charCounter, Does.Contain(expectedCount.ToString()),
                $"[TC_F10.28_02] Bộ đếm không khớp với số ký tự đã nhập ({expectedCount}), hiển thị: {charCounter}");
        }

        // ============================================
        // NO 54 -> TC_F10.28_03: Ghi chú mới xuất hiện ngay trong danh sách không cần reload
        // ============================================
        [Test]
        public void TC_F10_28_03_GhiChuMoiHienThiNgayKhongReload()
        {
            CurrentTestCaseId = "TC_F10.28_03";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            string urlBefore = Driver.Url;
            string noteText = data.GetValueOrDefault("noteInput", "Đã kiểm tra đơn hàng - Admin Huy");

            _orderDetailPage.EnterNote(noteText);
            Thread.Sleep(500);

            _orderDetailPage.ClickAddNote();
            Thread.Sleep(2000);

            string urlAfter = Driver.Url;
            bool noteAppearedInPage = _orderDetailPage.PageContains(noteText);

            CurrentActualResult = $"URL thay đổi: {urlBefore != urlAfter} | Ghi chú hiển thị ngay: {noteAppearedInPage}";

            Assert.That(urlAfter, Is.EqualTo(urlBefore),
                "[TC_F10.28_03] Trang bị reload sau khi thêm ghi chú (URL đã thay đổi)");
            Assert.That(noteAppearedInPage, Is.True,
                "[TC_F10.28_03] Ghi chú mới không xuất hiện ngay trên trang sau khi thêm");
        }

        // Hàm hỗ trợ đọc JSON
        private Dictionary<string, string> DocDuLieu(string tcId)
        {
            return JsonHelper.DocDuLieu(DataPath, tcId);
        }
    }
}
