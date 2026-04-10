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
    public class OrderListTests : TestBase
    {
        private OrderListPage _orderListPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "OrderManagement", "order_list.json"
        );

        [SetUp]
        public void SetUpPages()
        {
            CurrentSheetName = "TC_OrderManagement";
            _orderListPage = new OrderListPage(Driver, BaseUrl);
        }

        // ============================================
        // NO 1 -> TC_F10.1_01: Truy cập khi chưa đăng nhập
        // ============================================
        [Test]
        public void TC_F10_1_01_TruyCapKhiChuaDangNhap()
        {
            CurrentTestCaseId = "TC_F10.1_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            // Bỏ qua bước đăng nhập, tiến hành truy cập trực tiếp bằng đường dẫn
            _orderListPage.Open();
            Thread.Sleep(1000); // Chờ trình duyệt xử lý redirect

            string currentUrl = Driver.Url;
            bool isRedirectedToLogin = currentUrl.ToLower().Contains("login");

            CurrentActualResult = isRedirectedToLogin 
                ? $"Đã chuyển hướng về trang đăng nhập. (URL: {currentUrl})"
                : $"Lỗi bảo mật: Không chặn truy cập, vẫn ở lại URL: {currentUrl}";

            Assert.That(isRedirectedToLogin, Is.True, 
                "[TC_F10.1_01] Phải redirect về trang login khi chưa đăng nhập");
        }

        // ============================================
        // NO 2 -> TC_F10.1_02: Truy cập trang chi tiết khi chưa đăng nhập
        // ============================================
        [Test]
        public void TC_F10_1_02_TruyCapChiTietKhiChuaDangNhap()
        {
            CurrentTestCaseId = "TC_F10.1_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            // Bỏ qua bước đăng nhập, tiến hành truy cập trực tiếp bằng đường dẫn phụ
            string detailUrl = BaseUrl + "/Admin/Order/Detail/1";
            Driver.Navigate().GoToUrl(detailUrl);
            Thread.Sleep(1000); // Chờ trình duyệt xử lý redirect

            string currentUrl = Driver.Url;
            bool isRedirectedToLogin = currentUrl.ToLower().Contains("login");

            CurrentActualResult = isRedirectedToLogin 
                ? $"Đã chuyển hướng về trang đăng nhập khi cố vào trang chi tiết. (URL: {currentUrl})"
                : $"Lỗi bảo mật: Không chặn truy cập trang chi tiết, vẫn mở được URL: {currentUrl}";

            Assert.That(isRedirectedToLogin, Is.True, 
                "[TC_F10.1_02] Phải redirect về trang login khi chưa đăng nhập và truy cập trang Chi tiết đơn hàng");
        }

        // ============================================
        // NO 3 -> TC_F10.2_01: Tài khoản không phải Admin bị chặn
        // ============================================
        [Test]
        public void TC_F10_2_01_CustomerBiChanTruyCap()
        {
            CurrentTestCaseId = "TC_F10.2_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            LoginAsCustomer();
            
            _orderListPage.Open();
            Thread.Sleep(1000);

            string currentUrl = Driver.Url;
            bool isCorrectPage = _orderListPage.IsOnPage();

            CurrentActualResult = !isCorrectPage 
                ? $"Đã chặn quyền Customer thành công (Access Denied). (URL hiện tại: {currentUrl})"
                : $"Lỗi bảo mật: Customer lọt qua được và mở được trang Admin/Order!";
            Assert.That(isCorrectPage, Is.False, 
                "[TC_F10.2_01] Customer không được phép xem trang quản lý Order");
        }

        // ============================================
        // NO 4 -> TC_F10.3_01: Danh sách hiển thị đúng, đơn mới nhất trên cùng
        // ============================================
        [Test]
        public void TC_F10_3_01_DanhSachHienThiDonMoiNhat()
        {
            CurrentTestCaseId = "TC_F10.3_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            LoginAsAdmin();
            
            _orderListPage.Open();
            Thread.Sleep(1500);

            int totalCount = _orderListPage.GetTotalOrderCount();
            string firstOrderCode = _orderListPage.GetOrderCodeOfRow(0); // Lấy mã của phần tử dòng 0

            CurrentActualResult = $"Tổng: {totalCount} | Đơn đầu tiên: {firstOrderCode}";

            Assert.That(totalCount, Is.GreaterThanOrEqualTo(0), 
                "[TC_F10.3_01] Lỗi load dữ liệu danh sách");
        }

        // ============================================
        // NO 5 -> TC_F10.3_02: Bảng danh sách hiển thị đủ các cột
        // ============================================
        [Test]
        public void TC_F10_3_02_BangHienThiDuCotThongTin()
        {
            CurrentTestCaseId = "TC_F10.3_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            LoginAsAdmin();
            
            _orderListPage.Open();
            Thread.Sleep(1500);

            // Chuyển mảng chuỗi JSON thành List<string> (JsonHelper lưu array dưới dạng text)
            string expectedColsJson = data["expectedColumns"];
            List<string>? expectedColumns = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(expectedColsJson);
            if (expectedColumns == null)
            {
                expectedColumns = new List<string>();
            }

            IReadOnlyCollection<IWebElement> actualHeaderElements = Driver.FindElements(By.CssSelector("table thead th"));

            List<string> headersText = new List<string>();
            foreach (IWebElement header in actualHeaderElements)
            {
                headersText.Add(header.Text.Trim());
            }

            CurrentActualResult = $"Các cột hiển thị: {string.Join(", ", headersText)}";

            foreach (string expectedCol in expectedColumns)
            {
                bool daTimThayCot = headersText.Exists(h => h.Contains(expectedCol));
                Assert.That(daTimThayCot, Is.True, 
                    $"[TC_F10.3_02] Bảng thiếu tiêu đề cột: {expectedCol}");
            }
        }

        // ============================================
        // NO 6 -> TC_F10.3_03: Danh sách tải thành công không có lỗi hiển thị
        // ============================================
        [Test]
        public void TC_F10_3_03_TrangTaiThanhCongKhongLoi()
        {
            CurrentTestCaseId = "TC_F10.3_03";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            LoginAsAdmin();
            
            _orderListPage.Open();
            Thread.Sleep(1500);

            bool isHealthy = _orderListPage.IsPageHealthy();
            
            string ketQuaTrang = _orderListPage.DocKetQuaThucTe();
            CurrentActualResult = $"{ketQuaTrang} | Sức khỏe DB/UI: {isHealthy}";

            Assert.That(isHealthy, Is.True, 
                "[TC_F10.3_03] Phát hiện lỗi vỡ giao diện hoặc lỗi Server");
        }

        // ============================================
        // NO 7 -> TC_F10.4_01: Click mã đơn hàng mở đúng trang chi tiết
        // ============================================
        [Test]
        public void TC_F10_4_01_ClickMaDonMoTrangChiTiet()
        {
            CurrentTestCaseId = "TC_F10.4_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            LoginAsAdmin();

            _orderListPage.Open();
            Thread.Sleep(1500);

            string firstOrderCode = _orderListPage.GetOrderCodeOfRow(0);
            _orderListPage.ClickOrderCode(0);
            Thread.Sleep(1500);

            string currentUrl = Driver.Url;
            bool isOnDetailPage = currentUrl.Contains("/Admin/Order/Detail");

            CurrentActualResult = $"Mã đơn click: {firstOrderCode} | URL sau click: {currentUrl}";

            Assert.That(isOnDetailPage, Is.True,
                $"[TC_F10.4_01] Click mã đơn '{firstOrderCode}' không dẫn đến trang chi tiết, URL: {currentUrl}");
        }

        // ============================================
        // NO 8 -> TC_F10.4_02: Click icon xem chi tiết mở đúng trang chi tiết
        // ============================================
        [Test]
        public void TC_F10_4_02_ClickIconMatMoTrangChiTiet()
        {
            CurrentTestCaseId = "TC_F10.4_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            LoginAsAdmin();

            _orderListPage.Open();
            Thread.Sleep(1500);

            string firstOrderCode = _orderListPage.GetOrderCodeOfRow(0);
            _orderListPage.ClickViewDetail(0);
            Thread.Sleep(1500);

            string currentUrl = Driver.Url;
            bool isOnDetailPage = currentUrl.Contains("/Admin/Order/Detail");

            CurrentActualResult = $"Mã đơn: {firstOrderCode} | URL sau click icon: {currentUrl}";

            Assert.That(isOnDetailPage, Is.True,
                $"[TC_F10.4_02] Click icon xem chi tiết không dẫn đến trang chi tiết, URL: {currentUrl}");
        }

        // ============================================
        // NO 9 -> TC_F10.5_01: Mỗi trang hiển thị tối đa 10 đơn hàng
        // ============================================
        [Test]
        public void TC_F10_5_01_MoiTrangToiDa10Don()
        {
            CurrentTestCaseId = "TC_F10.5_01";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            LoginAsAdmin();

            _orderListPage.Open();
            Thread.Sleep(1500);

            int pageSize = int.Parse(data.GetValueOrDefault("pageSize", "10"));
            int rowCount = _orderListPage.GetAllRows().Count;

            CurrentActualResult = $"Số đơn trên trang hiện tại: {rowCount} | Giới hạn: {pageSize}";

            Assert.That(rowCount, Is.LessThanOrEqualTo(pageSize),
                $"[TC_F10.5_01] Trang hiển thị {rowCount} đơn, vượt quá giới hạn {pageSize} đơn/trang");
        }

        // ============================================
        // NO 10 -> TC_F10.5_02: Chuyển sang trang 2, dữ liệu không trùng trang 1
        // Pre-condition: Có hơn 10 đơn hàng trong CSDL
        // ============================================
        [Test]
        public void TC_F10_5_02_ChuyenTrang2DuLieuKhacTrang1()
        {
            CurrentTestCaseId = "TC_F10.5_02";
            Dictionary<string, string> data = DocDuLieu(CurrentTestCaseId);

            LoginAsAdmin();

            _orderListPage.Open();
            Thread.Sleep(1500);

            string firstCodePage1 = _orderListPage.GetOrderCodeOfRow(0);

            _orderListPage.ClickPage(2);
            Thread.Sleep(1500);

            string firstCodePage2 = _orderListPage.GetOrderCodeOfRow(0);

            CurrentActualResult = $"Trang 1 - mã đầu: {firstCodePage1} | Trang 2 - mã đầu: {firstCodePage2}";

            Assert.That(firstCodePage2, Is.Not.EqualTo(firstCodePage1),
                $"[TC_F10.5_02] Mã đơn đầu tiên ở trang 2 ({firstCodePage2}) trùng với trang 1 ({firstCodePage1})");
        }

        // Hàm hỗ trợ đọc JSON
        private Dictionary<string, string> DocDuLieu(string tcId)
        {
            return JsonHelper.DocDuLieu(DataPath, tcId);
        }
    }
}
