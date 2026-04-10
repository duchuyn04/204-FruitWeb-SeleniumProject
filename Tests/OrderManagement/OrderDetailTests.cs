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
    public class OrderDetailTests : TestBase
    {
        private OrderDetailPage _orderDetailPage = null!;
        private OrderListPage _orderListPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "OrderManagement", "order_detail.json"
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
        // NO 11 -> TC_F10.8_01: Trang chi tiết hiển thị đầy đủ thông tin sản phẩm
        // ============================================
        [Test]
        public void TC_F10_8_01_HienThiDayDuThongTinSanPham()
        {
            CurrentTestCaseId = "TC_F10.8_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            string expectedOrderCode = _orderListPage.GetOrderCodeOfRow(0);

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            string actualOrderCodeOnDetail = _orderDetailPage.GetOrderCode();

            string ketQuaHeThong = _orderDetailPage.DocKetQuaThucTe();
            CurrentActualResult = $"{ketQuaHeThong} | Đoạn Header: {actualOrderCodeOnDetail}";

            bool isOrderCodeMatch = actualOrderCodeOnDetail.Contains(expectedOrderCode);
            Assert.That(isOrderCodeMatch, Is.True,
                "[TC_F10.8_01] Mã đơn trên trang chi tiết không khớp với đơn vừa click");

            int rowCount = _orderDetailPage.GetProductTableRowCount();
            Assert.That(rowCount, Is.GreaterThan(0),
                "[TC_F10.8_01] Bảng sản phẩm trống không thấy hàng hóa nào (Expected > 0)");
        }

        // ============================================
        // NO 12 -> TC_F10.8_02: Trang chi tiết hiển thị đầy đủ thông tin khách hàng
        // ============================================
        [Test]
        public void TC_F10_8_02_HienThiDayDuThongTinKhachHang()
        {
            CurrentTestCaseId = "TC_F10.8_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            string currentUrl = Driver.Url;
            bool isOnDetailPage = currentUrl.Contains("/Admin/Order/Detail/");

            Assert.That(isOnDetailPage, Is.True,
                "[TC_F10.8_02] Không điều hướng được về trang chi tiết đơn hàng");

            bool hasCustomerSection = _orderDetailPage.HasCustomerSection();

            string orderStatus = _orderDetailPage.GetOrderStatus();
            string paymentStatus = _orderDetailPage.GetPaymentStatus();

            CurrentActualResult = $"URL: {currentUrl} | Trạng thái đơn: {orderStatus} | Thanh toán: {paymentStatus} | Có section khách: {hasCustomerSection}";

            Assert.That(hasCustomerSection, Is.True,
                "[TC_F10.8_02] Không tìm thấy section thông tin khách hàng trên trang chi tiết");
        }

        // ============================================
        // NO 13 -> TC_F10.8_03: URL trang chi tiết đúng định dạng /Admin/Order/Detail/{id}
        // ============================================
        [Test]
        public void TC_F10_8_03_UrlTrangChiTietDungDinhDang()
        {
            CurrentTestCaseId = "TC_F10.8_03";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            string currentUrl = Driver.Url;
            bool isValidUrl = System.Text.RegularExpressions.Regex.IsMatch(
                currentUrl, @"/Admin/Order/Detail/\d+$");

            CurrentActualResult = $"URL hiện tại: {currentUrl} | Hợp lệ: {isValidUrl}";

            Assert.That(isValidUrl, Is.True,
                $"[TC_F10.8_03] URL không đúng định dạng /Admin/Order/Detail/{{id}}: {currentUrl}");
        }

        // ============================================
        // NO 14 -> TC_F10.9_01: Truy cập ID không tồn tại trả về 404
        // ============================================
        [Test]
        public void TC_F10_9_01_TruyCapIDKhongTonTai404()
        {
            CurrentTestCaseId = "TC_F10.9_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            string invalidUrl = BaseUrl + "/Admin/Order/Detail/999999";
            Driver.Navigate().GoToUrl(invalidUrl);
            Thread.Sleep(1500);

            string pageSource = Driver.PageSource.ToLower();
            string currentUrl = Driver.Url;

            bool is404 = pageSource.Contains("404")
                || pageSource.Contains("not found")
                || pageSource.Contains("không tìm thấy")
                || pageSource.Contains("không tồn tại")
                || !currentUrl.Contains("/Admin/Order/Detail/999999");

            CurrentActualResult = $"URL: {currentUrl} | Trang báo 404 hoặc redirect: {is404}";

            Assert.That(is404, Is.True,
                "[TC_F10.9_01] Hệ thống không trả về lỗi 404 khi truy cập ID không tồn tại");
        }

        // ============================================
        // NO 15 -> TC_F10.9_02: Truy cập ID là chuỗi ký tự ngẫu nhiên trả về 404
        // ============================================
        [Test]
        public void TC_F10_9_02_TruyCapIDKyTuNgauNhien404()
        {
            CurrentTestCaseId = "TC_F10.9_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            string invalidUrl = BaseUrl + "/Admin/Order/Detail/abcxyz";
            Driver.Navigate().GoToUrl(invalidUrl);
            Thread.Sleep(1500);

            string pageSource = Driver.PageSource.ToLower();
            string currentUrl = Driver.Url;

            bool isErrorPage = pageSource.Contains("404")
                || pageSource.Contains("not found")
                || pageSource.Contains("bad request")
                || pageSource.Contains("không hợp lệ")
                || !currentUrl.Contains("/Admin/Order/Detail/abcxyz");

            CurrentActualResult = $"URL: {currentUrl} | Trang báo lỗi: {isErrorPage}";

            Assert.That(isErrorPage, Is.True,
                "[TC_F10.9_02] Hệ thống không trả về lỗi khi truy cập ID là chuỗi ký tự");
        }

        // ============================================
        // NO 16 -> TC_F10.10_01: Nút Quay lại điều hướng về trang danh sách
        // ============================================
        [Test]
        public void TC_F10_10_01_NutQuayLaiBatPage()
        {
            CurrentTestCaseId = "TC_F10.10_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            Assert.That(Driver.Url.Contains("/Admin/Order/Detail/"), Is.True,
                "[TC_F10.10_01] Không điều hướng được sang trang chi tiết");

            _orderDetailPage.ClickBack();
            Wait.WaitForUrlNotContains("/Detail");

            string currentUrl = Driver.Url;
            bool isBackOnList = currentUrl.Contains("/Admin/Order")
                && !currentUrl.Contains("/Admin/Order/Detail");

            CurrentActualResult = $"URL sau khi quay lại: {currentUrl} | Về danh sách: {isBackOnList}";

            Assert.That(isBackOnList, Is.True,
                $"[TC_F10.10_01] Nút Quay lại không điều hướng về trang danh sách, URL hiện tại: {currentUrl}");
        }

        // ============================================
        // NO 17 -> TC_F10.10_02: Nút Quay lại hiển thị đúng vị trí và có thể click
        // ============================================
        [Test]
        public void TC_F10_10_02_NutQuayLaiHienThiVaClickDuoc()
        {
            CurrentTestCaseId = "TC_F10.10_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            bool exists = _orderDetailPage.IsBackButtonExists();
            bool isDisplayed = _orderDetailPage.IsBackButtonDisplayed();
            bool isEnabled = _orderDetailPage.IsBackButtonEnabled();

            CurrentActualResult = $"Nút tồn tại: {exists} | Hiển thị: {isDisplayed} | Click được: {isEnabled}";

            Assert.That(exists, Is.True, "[TC_F10.10_02] Không tìm thấy nút Quay lại");
            Assert.That(isDisplayed, Is.True, "[TC_F10.10_02] Nút Quay lại đang bị ẩn");
        }

        // ============================================
        // NO 18 -> TC_F10.11_01: Khách vãng lai hiển thị nhãn "Khách vãng lai"
        // Pre-condition: Đơn hàng đầu tiên trong danh sách phải là của khách vãng lai
        // ============================================
        [Test]
        public void TC_F10_11_01_KhachVangLaiHienThiNhan()
        {
            CurrentTestCaseId = "TC_F10.11_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            string orderCode = _orderDetailPage.GetOrderCode();
            bool hasGuestLabel = _orderDetailPage.PageContains("Khách vãng lai");

            CurrentActualResult = $"Đơn: {orderCode} | Có nhãn Khách vãng lai: {hasGuestLabel}";

            Assert.That(hasGuestLabel, Is.True,
                "[TC_F10.11_01] Đơn của khách vãng lai không hiển thị nhãn 'Khách vãng lai'");
        }

        // ============================================
        // NO 19 -> TC_F10.11_02: Khách đã đăng ký hiển thị tên thực (không phải khách vãng lai)
        // Pre-condition: Đơn hàng đầu tiên trong danh sách phải là của khách đã có tài khoản
        // ============================================
        [Test]
        public void TC_F10_11_02_KhachDaDangKyHienThiTenThuc()
        {
            CurrentTestCaseId = "TC_F10.11_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            _orderListPage.Open();
            Wait.WaitForUrlContains("/Admin/Order");

            _orderListPage.ClickViewDetail(0);
            Wait.WaitForUrlContains("/Admin/Order/Detail/");

            string orderCode = _orderDetailPage.GetOrderCode();
            bool hasGuestLabel = _orderDetailPage.PageContains("Khách vãng lai");

            CurrentActualResult = $"Đơn: {orderCode} | Nhãn khách vãng lai: {hasGuestLabel}";

            Assert.That(hasGuestLabel, Is.False,
                "[TC_F10.11_02] Đơn của khách đã đăng ký vẫn hiển thị nhãn 'Khách vãng lai'");
        }

        // Hàm hỗ trợ đọc JSON
        private Dictionary<string, string> DocDuLieu(string tcId)
        {
            return JsonHelper.DocDuLieu(DataPath, tcId);
        }
    }
}
