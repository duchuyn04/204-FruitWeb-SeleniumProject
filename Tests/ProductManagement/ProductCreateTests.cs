using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumProject.Pages.ProductManagement;
using SeleniumProject.Utilities;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;

namespace SeleniumProject.Tests.ProductManagement
{
    [TestFixture]
    public class ProductCreateTests : TestBase
    {
        private CreateProductPage _createPage = null!;
        private ProductListPage _listPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "ProductManagement", "product_create.json"
        );

        [SetUp]
        public void SetUpPages()
        {
            CurrentSheetName = "TC_Product Management";
            _createPage = new CreateProductPage(Driver, BaseUrl);
            _listPage = new ProductListPage(Driver, BaseUrl);
            
            LoginAsAdmin();
        }


        private void FillBasicProductInfo(Dictionary<string, string> data)
        {
            if (data.ContainsKey("productName")) _createPage.EnterName(data["productName"]);
            if (data.ContainsKey("category")) _createPage.SelectCategory(data["category"]);
            if (data.ContainsKey("price")) _createPage.EnterPrice(data["price"]);
        }

        // STT 42 - TC_F2.6_01: Kiểm tra live preview giá VND cập nhật ngay khi nhập
        [Test]
        public void TC_F2_6_01_CheckPriceLivePreview()
        {
            CurrentTestCaseId = "TC_F2.6_01";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();

            _createPage.EnterPrice(data["price1"]);
            Thread.Sleep(500); // Đợi js update
            string previewText1 = _createPage.GetPricePreviewText();
            Assert.That(previewText1.Replace(".", "").Replace(",", ""), Does.Contain(data["price1"]));

            // Xóa và nhập lại
            Driver.FindElement(By.Id("priceInput")).Clear();
            _createPage.EnterPrice(data["price2"]);
            Thread.Sleep(500);
            string previewText2 = _createPage.GetPricePreviewText();
            Assert.That(previewText2.Replace(".", "").Replace(",", ""), Does.Contain(data["price2"]));
            
            CurrentActualResult = $"Live preview hiển thị. Lần 1: '{previewText1}' | Lần 2: '{previewText2}'";
        }

        // STT 43 - TC_F2.6_02: Kiểm tra thêm sản phẩm thành công với Giá gốc = 1 (min hợp lệ)
        [Test]
        public void TC_F2_6_02_AddProduct_MinPrice()
        {
            CurrentTestCaseId = "TC_F2.6_02";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Thêm SP với giá = 1 => Kết quả: {_createPage.DocKetQuaThucTe()}";
            string currentUrl = Driver.Url;
            Assert.That(currentUrl.EndsWith("/Admin/Product"), Is.True, "Phải quay về danh sách sau khi thêm thành công");
        }

        // STT 44 - TC_F2.6_03: Kiểm tra thêm SP với Số lượng tồn kho = 0
        [Test]
        public void TC_F2_6_03_AddProduct_ZeroStock()
        {
            CurrentTestCaseId = "TC_F2.6_03";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.EnterStock(data["stock"]);
            
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Thêm SP với tồn kho = 0 => Kết quả: {_createPage.DocKetQuaThucTe()}";
            string currentUrl = Driver.Url;
            
            // Quy định: Được phép lưu (Tồn kho có thể = 0 đối với các sản phẩm chưa nhập hàng)
            Assert.That(currentUrl.EndsWith("/Admin/Product"), Is.True, "Phải quay về danh sách sau khi thêm thành công");
        }

        // STT 45 - TC_F2.6_04: Kiểm tra thêm SP khi bật Hiển thị (isActive = true)
        [Test]
        public void TC_F2_6_04_AddProduct_IsActiveTrue()
        {
            CurrentTestCaseId = "TC_F2.6_04";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            
            // Mặc định thường là true, nhưng chúng ta vẫn gán giá trị rõ ràng
            _createPage.SetIsActive(bool.Parse(data["isActive"]));
            
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Thêm SP (Hiển thị = True) => Kết quả: {_createPage.DocKetQuaThucTe()}";
            string currentUrl = Driver.Url;
            Assert.That(currentUrl.EndsWith("/Admin/Product"), Is.True, "Phải quay về danh sách sau khi thêm thành công");
        }

        // STT 46 - TC_F2.6_05: Kiểm tra thêm SP khi Ẩn SP (isActive = false)
        [Test]
        public void TC_F2_6_05_AddProduct_IsActiveFalse()
        {
            CurrentTestCaseId = "TC_F2.6_05";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            
            _createPage.SetIsActive(bool.Parse(data["isActive"]));
            
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Thêm SP (Hiển thị = False) => Kết quả: {_createPage.DocKetQuaThucTe()}";
            string currentUrl = Driver.Url;
            Assert.That(currentUrl.EndsWith("/Admin/Product"), Is.True, "Phải quay về danh sách sau khi thêm thành công");
        }

        // STT 47 - TC_F2.6_06: Kiểm tra thêm rồi xóa SP tạm thời
        [Test]
        public void TC_F2_6_06_AddProduct_TemporaryDeletionCheck()
        {
            CurrentTestCaseId = "TC_F2.6_06";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            string currentUrl = Driver.Url;
            Assert.That(currentUrl.EndsWith("/Admin/Product"), Is.True, "Phải quay về danh sách sau khi thêm thành công");
            
            if (data.ContainsKey("productName"))
            {
                // Thực hiện tìm kiếm và xóa
                _listPage.DeleteProductSoft(data["productName"]);
                _listPage.Search(data["productName"]);
                
                bool isDeleted = _listPage.HasNoResultMessage();
                
                // Thu thập kết quả thực tế ghi vào báo cáo sao cho đọc thuận miệng và đúng nghĩa nhất
                string emptyMessage = isDeleted ? _listPage.GetNoResultMessage() : "Vẫn còn sản phẩm";
                CurrentActualResult = $"Xóa mềm SP '{data["productName"]}' thành công. Kết quả tra cứu lại: '{emptyMessage}'";
                
                Assert.That(isDeleted, Is.True, "Sản phẩm phải không còn trong danh sách (vì đã xóa mềm)");
            }
        }

        // STT 48 - TC_F2.6_07: Kiểm tra Giá khuyến mãi = 0
        [Test]
        public void TC_F2_6_07_AddProduct_SalePriceZero()
        {
            CurrentTestCaseId = "TC_F2.6_07";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.EnterSalePrice(data["salePrice"]);
            
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Thêm SP với giá KM = 0 => Kết quả: {_createPage.DocKetQuaThucTe()}";
            string currentUrl = Driver.Url;
            
            // Plan ghi: "Lưu thành công với giá KM = 0 hoặc báo lỗi"
            // Kiểm tra trang có redirect thành công hay hiển thị lỗi validation
            if (currentUrl.EndsWith("/Admin/Product/Create"))
            {
                bool hasErrors = _createPage.HasValidationErrors();
                Assert.That(hasErrors, Is.True);
            }
        }

        // STT 49 - TC_F2.6_08: Kiểm tra Slug thủ công hợp lệ
        [Test]
        public void TC_F2_6_08_AddProduct_CustomSlug()
        {
            CurrentTestCaseId = "TC_F2.6_08";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.EnterSlug(data["slug"]);
            
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Thêm SP với custom slug '{data["slug"]}' => Kết quả: {_createPage.DocKetQuaThucTe()}";
            string currentUrl = Driver.Url;
            Assert.That(currentUrl.EndsWith("/Admin/Product"), Is.True, "Phải quay về danh sách sau khi thêm thành công");
        }

        // STT 50 - TC_F2.6_09: Kiểm tra MinOrder = 0
        [Test]
        public void TC_F2_6_09_AddProduct_MinOrderZero()
        {
            CurrentTestCaseId = "TC_F2.6_09";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.EnterMinOrder(data["minOrder"]);
            
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Thêm SP với số lượng tối thiểu = 0 => Kết quả: {_createPage.DocKetQuaThucTe()}";
            string currentUrl = Driver.Url;
            
            // Quy định: Không cho phép thêm thành công khi MinOrder = 0
            Assert.That(currentUrl.EndsWith("/Admin/Product/Create"), Is.True, "Form không được lưu, phải ở lại trang Create");
            Assert.That(_createPage.HasValidationErrors(), Is.True, "Phải hiển thị lỗi validation khi số lượng đặt tối thiểu = 0");
        }

        // STT 51 - TC_F2.6_10: Kiểm tra Stock = 1 (boundary)
        [Test]
        public void TC_F2_6_10_AddProduct_StockOne()
        {
            CurrentTestCaseId = "TC_F2.6_10";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.EnterStock(data["stock"]);
            
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Thêm SP với tồn kho = 1 => Kết quả: {_createPage.DocKetQuaThucTe()}";
            string currentUrl = Driver.Url;
            Assert.That(currentUrl.EndsWith("/Admin/Product"), Is.True, "Phải quay về danh sách sau khi thêm thành công");
        }

        // STT 52 - TC_F2.6_11: Kiểm tra Mô tả ngắn chứa link URL
        [Test]
        public void TC_F2_6_11_AddProduct_ShortDescriptionUrl()
        {
            CurrentTestCaseId = "TC_F2.6_11";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.EnterShortDescription(data["shortDescription"]);
            
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Thêm SP mô tả ngắn chứa URL => Kết quả: {_createPage.DocKetQuaThucTe()}";
            string currentUrl = Driver.Url;
            Assert.That(currentUrl.EndsWith("/Admin/Product"), Is.True, "Phải quay về danh sách sau khi thêm thành công");
        }

        // STT 53 - TC_F2.6_12: Kiểm tra redirect đúng sau khi lưu thành công
        [Test]
        public void TC_F2_6_12_AddProduct_CheckRedirect()
        {
            CurrentTestCaseId = "TC_F2.6_12";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            string currentUrl = Driver.Url;
            CurrentActualResult = $"URL sau khi bấm lưu: {currentUrl}";
            Assert.That(currentUrl.EndsWith(data["expectedUrl"]), Is.True, "Trang phải redirect đúng về danh sách");
        }

        // STT 54 - TC_F2.6_13: Kiểm tra toast message sau lưu thành công
        [Test]
        public void TC_F2_6_13_AddProduct_CheckToastMessage()
        {
            CurrentTestCaseId = "TC_F2.6_13";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            
            _createPage.ClickSave();
            // Lấy toast nhanh ngay sau khi bấm save
            string actualToast = "";
            try { actualToast = _createPage.GetToastMessage(); } catch {}
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Toast hiển thị: '{actualToast}'";
            Assert.That(actualToast.ToLower(), Does.Contain(data["expectedToast"].ToLower()), "Phải hiện đúng thông báo toast");
        }

        // STT 55 - TC_F2.6_14: Kiểm tra SP mới xuất hiện trong danh sách
        [Test]
        public void TC_F2_6_14_AddProduct_CheckNewItemInList()
        {
            CurrentTestCaseId = "TC_F2.6_14";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            
            _createPage.ClickSave();
            Thread.Sleep(1000); // Chờ chuyển trang
            
            _listPage.Search(data["productName"]);
            bool isPresent = _listPage.IsProductInResults(data["productName"]);
            
            CurrentActualResult = $"Tiến hành tìm '{data["productName"]}' trong list. Tìm thấy: {isPresent}";
            Assert.That(isPresent, Is.True, "Sản phẩm mới phải xuất hiện trong kết quả tìm kiếm");
        }

        // STT 56 - TC_F2.6_15: Kiểm tra SP hiển giá đúng sau lưu
        [Test]
        public void TC_F2_6_15_AddProduct_CheckPriceInList()
        {
            CurrentTestCaseId = "TC_F2.6_15";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            if(data.ContainsKey("salePrice")) _createPage.EnterSalePrice(data["salePrice"]);
            
            _createPage.ClickSave();
            Thread.Sleep(1500); 
            
            _listPage.Search(data["productName"]);
            bool isPresent = _listPage.IsProductInResults(data["productName"]);
            
            CurrentActualResult = $"Search '{data["productName"]}'. Tìm thấy: {isPresent}. Dữ liệu DOM: {_createPage.DocKetQuaThucTe()}";
            Assert.That(isPresent, Is.True, "Phải có sản phẩm hiển thị trong lưới");
        }

        // STT 57 - TC_F2.7_01: Kiểm tra thêm sản phẩm thành công với đầy đủ thông tin hợp lệ
        [Test]
        public void TC_F2_7_01_AddProduct_FullValidInfo()
        {
            CurrentTestCaseId = "TC_F2.7_01";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.EnterShortDescription(data["shortDescription"]);
            _createPage.EnterSalePrice(data["salePrice"]);
            _createPage.EnterStock(data["stock"]);
            
            // Nếu có ảnh, cấu hình đường dẫn tương đối từ TestData
            if (data.ContainsKey("imageName"))
            {
                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Images", data["imageName"]);
                if (File.Exists(imagePath))
                {
                    _createPage.UploadImage(imagePath);
                }
            }
            
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Thêm SP Full Info => Kết quả: {_createPage.DocKetQuaThucTe()}";
            string currentUrl = Driver.Url;
            Assert.That(currentUrl.EndsWith("/Admin/Product"), Is.True, "Phải quay về danh sách sau khi thêm thành công");
        }

        // STT 58 - TC_F2.7_02: Kiểm tra thêm sản phẩm thành công chỉ với các trường bắt buộc
        [Test]
        public void TC_F2_7_02_AddProduct_RequiredFieldsOnly()
        {
            CurrentTestCaseId = "TC_F2.7_02";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data); // Các thuộc tính đã được cover
            
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Thêm SP Required Only => Kết quả: {_createPage.DocKetQuaThucTe()}";
            string currentUrl = Driver.Url;
            Assert.That(currentUrl.EndsWith("/Admin/Product"), Is.True, "Phải quay về danh sách sau khi thêm thành công");
        }

        // STT 59 - TC_F2.7_03: Kiểm tra nhập Slug tùy chỉnh hợp lệ
        [Test]
        public void TC_F2_7_03_AddProduct_CustomSlugValid()
        {
            CurrentTestCaseId = "TC_F2.7_03";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.EnterSlug(data["slug"]);
            
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Thêm SP custom slug '{data["slug"]}' => Kết quả: {_createPage.DocKetQuaThucTe()}";
            string currentUrl = Driver.Url;
            Assert.That(currentUrl.EndsWith("/Admin/Product"), Is.True, "Phải quay về danh sách sau khi lưu");
        }

        // STT 60 - TC_F2.7_04: Kiểm tra Slug tự động sinh từ tên sản phẩm khi để trống
        [Test]
        public void TC_F2_7_04_AddProduct_AutoSlug()
        {
            CurrentTestCaseId = "TC_F2.7_04";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);

            _createPage.Open();
            FillBasicProductInfo(data);
            
            Driver.FindElement(By.Id("Product_Slug")).Clear();
            
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Thêm SP bỏ trống slug (Kỳ vọng: {data["expectedSlug"]}) => Kết quả: {_createPage.DocKetQuaThucTe()}";
            string currentUrl = Driver.Url;
            Assert.That(currentUrl.EndsWith("/Admin/Product"), Is.True, "Phải quay về danh sách");
        }

        // STT 61 - TC_F2.7_05: Kiểm tra hệ thống báo lỗi khi submit form rỗng hoàn toàn
        [Test]
        public void TC_F2_7_05_EmptyForm()
        {
            CurrentTestCaseId = "TC_F2.7_05";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Form rỗng => Kết quả: {_createPage.DocKetQuaThucTe()}";
            Assert.That(Driver.Url.EndsWith("/Admin/Product/Create"), Is.True, "Phải ở lại form Create");
            Assert.That(_createPage.HasValidationErrors(), Is.True, "Phải có lỗi validation");
        }

        // STT 62 - TC_F2.7_06: Kiểm tra hệ thống báo lỗi khi để trống Tên sản phẩm
        [Test]
        public void TC_F2_7_06_EmptyProductName()
        {
            CurrentTestCaseId = "TC_F2.7_06";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            Driver.FindElement(By.Id("Product_Name")).Clear(); // Đảm bảo rỗng
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Tên SP rỗng => Kết quả: {_createPage.DocKetQuaThucTe()}";
            Assert.That(_createPage.HasValidationErrors(), Is.True, "Phải có lỗi validation tên sản phẩm");
        }

        // STT 63 - TC_F2.7_07: Kiểm tra hệ thống báo lỗi khi không chọn Danh mục
        [Test]
        public void TC_F2_7_07_NoCategory()
        {
            CurrentTestCaseId = "TC_F2.7_07";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            // Ignore select category string "-- Chọn danh mục --"
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Bỏ trống Danh mục => Kết quả: {_createPage.DocKetQuaThucTe()}";
            Assert.That(_createPage.HasValidationErrors(), Is.True, "Phải có lỗi validation danh mục");
        }

        // STT 64 - TC_F2.7_08: Kiểm tra hệ thống báo lỗi khi để trống Giá gốc
        [Test]
        public void TC_F2_7_08_EmptyPrice()
        {
            CurrentTestCaseId = "TC_F2.7_08";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            Driver.FindElement(By.Id("priceInput")).Clear();
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Bỏ trống giá gốc => Kết quả: {_createPage.DocKetQuaThucTe()}";
            Assert.That(_createPage.HasValidationErrors(), Is.True, "Phải có lỗi validation giá gốc");
        }

        // STT 65 - TC_F2.7_09: Kiểm tra hệ thống báo lỗi khi Giá khuyến mãi lớn hơn Giá gốc
        [Test]
        public void TC_F2_7_09_SalePriceGreaterThanPrice()
        {
            CurrentTestCaseId = "TC_F2.7_09";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.EnterSalePrice(data["salePrice"]);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Giá KM > Giá gốc => Kết quả: {_createPage.DocKetQuaThucTe()}";
            Assert.That(_createPage.HasValidationErrors(), Is.True, "Phải có lỗi validation giá khuyến mãi");
        }

        // STT 66 - TC_F2.7_10: Kiểm tra hệ thống báo lỗi khi nhập Giá gốc âm
        [Test]
        public void TC_F2_7_10_NegativePrice()
        {
            CurrentTestCaseId = "TC_F2.7_10";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Giá gốc âm => Kết quả: {_createPage.DocKetQuaThucTe()}";
            Assert.That(_createPage.HasValidationErrors(), Is.True, "Phải có lỗi validation giá gốc");
        }

        // STT 67 - TC_F2.7_11: Kiểm tra hệ thống chặn truy cập khi chưa đăng nhập
        [Test]
        public void TC_F2_7_11_UnauthenticatedAccess()
        {
            CurrentTestCaseId = "TC_F2.7_11";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            
            // Xóa cookie để đăng xuất
            Driver.Manage().Cookies.DeleteAllCookies();
            
            Driver.Navigate().GoToUrl(BaseUrl + data["directUrl"]);
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Truy cập chưa đăng nhập => Chuyển hướng tới: {Driver.Url}";
            Assert.That(Driver.Url.Contains(data["expectedRedirect"]), Is.True, "Phải redirect về trang Login");
            
            // Khôi phục lại trạng thái cho test khác
            LoginAsAdmin();
        }

        // STT 68 - TC_F2.7_12: Kiểm tra hệ thống trả về 403 khi tài khoản không phải Admin
        [Test]
        public void TC_F2_7_12_UnauthorizedAccessCustomer()
        {
            CurrentTestCaseId = "TC_F2.7_12";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            
            // Login bằng tài khoản customer
            Driver.Manage().Cookies.DeleteAllCookies();
            LoginAsCustomer();
            
            Driver.Navigate().GoToUrl(BaseUrl + data["directUrl"]);
            Thread.Sleep(1000);
            
            string pageSource = Driver.PageSource;
            CurrentActualResult = $"Truy cập bằng Customer => URL: {Driver.Url}, Có chữ Access Denied: {pageSource.Contains("AccessDenied") || pageSource.Contains("403")}";
            
            Assert.That(pageSource.Contains("AccessDenied") || pageSource.Contains("403") || Driver.Url.Contains("AccessDenied"), Is.True, "Phải trả về 403 hoặc Access Denied");
            
            // Khôi phục
            Driver.Manage().Cookies.DeleteAllCookies();
            LoginAsAdmin();
        }

        // STT 69 - TC_F2.7_13: Kiểm tra thêm sản phẩm thành công với Tên sản phẩm 1 ký tự (min)
        [Test]
        public void TC_F2_7_13_NameMinLength()
        {
            CurrentTestCaseId = "TC_F2.7_13";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Tên SP 1 ký tự => Kết quả: {_createPage.DocKetQuaThucTe()}";
            Assert.That(Driver.Url.EndsWith("/Admin/Product"), Is.True, "Phải quay về danh sách sau khi thêm");
        }

        // STT 70 - TC_F2.7_14: Kiểm tra hệ thống từ chối Tên sản phẩm vượt max length
        [Test]
        public void TC_F2_7_14_NameMaxLength()
        {
            CurrentTestCaseId = "TC_F2.7_14";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Tên SP vượt max => Kết quả: {_createPage.DocKetQuaThucTe()}";
            Assert.That(_createPage.HasValidationErrors(), Is.True, "Phải có lỗi validation (tùy thuộc DB limit)");
        }

        // STT 71 - TC_F2.7_15: Kiểm tra Slug bị trùng với SP đã tồn tại
        [Test]
        public void TC_F2_7_15_DuplicateSlug()
        {
            CurrentTestCaseId = "TC_F2.7_15";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            
            // Bước 1: Tạo SP bị trùng
            _createPage.Open();
            FillBasicProductInfo(data);
            Driver.FindElement(By.Id("Product_Slug")).Clear();
            Driver.FindElement(By.Id("Product_Slug")).SendKeys(data["slug"]);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            
            CurrentActualResult = $"Tạo SP trùng Slug => Kết quả: {_createPage.DocKetQuaThucTe()}";
            Assert.That(_createPage.HasValidationErrors() || _createPage.GetToastMessage().Contains("lỗi") || _createPage.GetToastMessage().Contains("tồn tại"), Is.True, "Phải báo lỗi do trùng slug");
        }

        // STT 72 - TC_F2.7_16: Kiểm tra Giá gốc nhập số thập phân
        [Test]
        public void TC_F2_7_16_DecimalPrice()
        {
            CurrentTestCaseId = "TC_F2.7_16";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Giá thập phân => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 73 - TC_F2.7_17: Kiểm tra MinOrder nhập giá trị âm bị từ chối
        [Test]
        public void TC_F2_7_17_NegativeMinOrder()
        {
            CurrentTestCaseId = "TC_F2.7_17";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.EnterMinOrder(data["minOrder"]);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"MinOrder âm => Kết quả: {_createPage.DocKetQuaThucTe()}";
            Assert.That(_createPage.HasValidationErrors(), Is.True, "Phải báo lỗi");
        }

        // STT 74 - TC_F2.7_18: Kiểm tra Giá gốc nhập chữ thay vì số
        [Test]
        public void TC_F2_7_18_StringPrice()
        {
            CurrentTestCaseId = "TC_F2.7_18";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Giá nhập chữ => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 75 - TC_F2.7_19: Kiểm tra Giá gốc rất lớn (999999999)
        [Test]
        public void TC_F2_7_19_ExtremelyHighPrice()
        {
            CurrentTestCaseId = "TC_F2.7_19";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Giá rất lớn => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 76 - TC_F2.7_20: Kiểm tra Số lượng tồn kho âm bị từ chối
        [Test]
        public void TC_F2_7_20_NegativeStock()
        {
            CurrentTestCaseId = "TC_F2.7_20";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.EnterStock(data["stock"]);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Stock âm => Kết quả: {_createPage.DocKetQuaThucTe()}";
            Assert.That(_createPage.HasValidationErrors(), Is.True, "Phải báo lỗi");
        }

        // STT 77 - TC_F2.7_21: Kiểm tra Tên SP chứa ký tự đặc biệt (@#$%)
        [Test]
        public void TC_F2_7_21_SpecialCharacterInName()
        {
            CurrentTestCaseId = "TC_F2.7_21";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Tên SP chứa ký tự đặc biệt => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 78 - TC_F2.7_22: Kiểm tra XSS injection trong Tên SP
        [Test]
        public void TC_F2_7_22_XSSInjection()
        {
            CurrentTestCaseId = "TC_F2.7_22";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"XSS Payload => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 79 - TC_F2.7_23: Kiểm tra SQL Injection trong Tên SP
        [Test]
        public void TC_F2_7_23_SQLInjection()
        {
            CurrentTestCaseId = "TC_F2.7_23";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"SQLi Payload => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 80 - TC_F2.7_24: Kiểm tra Mô tả ngắn vượt 500 ký tự
        [Test]
        public void TC_F2_7_24_LongShortDescription()
        {
            CurrentTestCaseId = "TC_F2.7_24";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            Driver.FindElement(By.Id("Product_ShortDescription")).SendKeys(data["shortDescription"]);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Mô tả ngắn cực dài => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 81 - TC_F2.7_25: Kiểm tra Giá khuyến mãi âm
        [Test]
        public void TC_F2_7_25_NegativeSalePrice()
        {
            CurrentTestCaseId = "TC_F2.7_25";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.EnterSalePrice(data["salePrice"]);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Giá KM âm => Kết quả: {_createPage.DocKetQuaThucTe()}";
            Assert.That(_createPage.HasValidationErrors(), Is.True, "Phải báo lỗi");
        }

        // STT 82 - TC_F2.7_26: Kiểm tra Slug chứa ký tự đặc biệt và khoảng trắng
        [Test]
        public void TC_F2_7_26_SpecialCharacterInSlug()
        {
            CurrentTestCaseId = "TC_F2.7_26";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            Driver.FindElement(By.Id("Product_Slug")).SendKeys(data["slug"]);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Slug chứa đặc biệt => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 83 - TC_F2.7_27: Kiểm tra Giá khuyến mãi bằng Giá gốc
        [Test]
        public void TC_F2_7_27_SalePriceEqualsPrice()
        {
            CurrentTestCaseId = "TC_F2.7_27";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.EnterSalePrice(data["salePrice"]);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Giá KM = Giá gốc => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 84 - TC_F2.7_28: Kiểm tra Tên SP chỉ chứa khoảng trắng
        [Test]
        public void TC_F2_7_28_WhitespaceName()
        {
            CurrentTestCaseId = "TC_F2.7_28";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Tên SP toàn khoảng trắng => Kết quả: {_createPage.DocKetQuaThucTe()}";
            Assert.That(_createPage.HasValidationErrors(), Is.True, "Phải báo lỗi");
        }

        // STT 85 - TC_F2.7_29: Kiểm tra Giá gốc = 0
        [Test]
        public void TC_F2_7_29_ZeroPrice()
        {
            CurrentTestCaseId = "TC_F2.7_29";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Giá gốc = 0 => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 86 - TC_F2.7_30: Kiểm tra Stock rất lớn (999999)
        [Test]
        public void TC_F2_7_30_ExtremelyHighStock()
        {
            CurrentTestCaseId = "TC_F2.7_30";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.EnterStock(data["stock"]);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Stock rất lớn => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 87 - TC_F2.7_31: Kiểm tra Tên SP trùng SP đã có
        [Test]
        public void TC_F2_7_31_DuplicateName()
        {
            CurrentTestCaseId = "TC_F2.7_31";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            // Giả định SP "Táo Envy New Zealand R1" đã được tạo ở STT khác trong DB
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Thêm SP trùng tên => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 88 - TC_F2.7_32: Kiểm tra Mô tả ngắn để trống
        [Test]
        public void TC_F2_7_32_EmptyShortDescription()
        {
            CurrentTestCaseId = "TC_F2.7_32";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Mô tả ngắn trống => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 89 - TC_F2.7_33: Kiểm tra Tên SP chứa Emoji
        [Test]
        public void TC_F2_7_33_EmojiInName()
        {
            CurrentTestCaseId = "TC_F2.7_33";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Tên SP chứa Emoji => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 90 - TC_F2.7_34: Kiểm tra trim space đầu/cuối Tên
        [Test]
        public void TC_F2_7_34_TrimName()
        {
            CurrentTestCaseId = "TC_F2.7_34";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Tên SP chứa space đầu cuối => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 91 - TC_F2.7_35: Kiểm tra MinOrder rất lớn (999999)
        [Test]
        public void TC_F2_7_35_ExtremelyHighMinOrder()
        {
            CurrentTestCaseId = "TC_F2.7_35";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.EnterMinOrder(data["minOrder"]);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"MinOrder rất lớn => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 92 - TC_F2.7_36: Kiểm tra Tên SP 255 ký tự (max DB column)
        [Test]
        public void TC_F2_7_36_MaxLengthName()
        {
            CurrentTestCaseId = "TC_F2.7_36";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickSave();
            Thread.Sleep(1000);
            CurrentActualResult = $"Tên SP dài 255 ký tự => Kết quả: {_createPage.DocKetQuaThucTe()}";
        }

        // STT 93 - TC_F2.7_37: Kiểm tra nhấn Cancel/Quay lại không lưu
        [Test]
        public void TC_F2_7_37_CancelButton()
        {
            CurrentTestCaseId = "TC_F2.7_37";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            _createPage.ClickCancel(); // Click Hủy
            Thread.Sleep(1000);
            CurrentActualResult = $"Click Hủy => URL hiện tại: {Driver.Url}";
            Assert.That(Driver.Url.EndsWith("/Admin/Product"), Is.True, "Phải quay về trang list");
        }

        // STT 94 - TC_F2.7_38: Kiểm tra tạo SP khi mất kết nối mạng
        [Test]
        public void TC_F2_7_38_NetworkOffline()
        {
            CurrentTestCaseId = "TC_F2.7_38";
            var data = JsonHelper.DocDuLieu(DataPath, CurrentTestCaseId);
            _createPage.Open();
            FillBasicProductInfo(data);
            
            // Giả lập offline (Yêu cầu Selenium 4+ với browser gốc Chromium)
            var driverType = Driver.GetType();
            if (driverType.Name.Contains("Chrome") || driverType.Name.Contains("Edge"))
            {
                dynamic chromiumDriver = Driver;
                chromiumDriver.NetworkConditions = new OpenQA.Selenium.Chromium.ChromiumNetworkConditions()
                {
                    IsOffline = true
                };
            }
            
            try
            {
                _createPage.ClickSave();
                Thread.Sleep(2000);
                CurrentActualResult = $"Lưu khi rớt mạng => Kết quả: Mạng bị sập không thể load form tiếp";
            }
            catch (Exception ex)
            {
                CurrentActualResult = $"Lưu khi rớt mạng => Có Exception throw: {ex.Message}";
            }
            
            // Khôi phục online
            if (driverType.Name.Contains("Chrome") || driverType.Name.Contains("Edge"))
            {
                dynamic chromiumDriver = Driver;
                chromiumDriver.NetworkConditions = new OpenQA.Selenium.Chromium.ChromiumNetworkConditions()
                {
                    IsOffline = false
                };
            }
        }
    }
}
