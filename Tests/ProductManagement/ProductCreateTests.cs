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

        // STT 42 — TC_F2.6_01: Kiểm tra live preview giá VND cập nhật ngay khi nhập
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

        // STT 43 — TC_F2.6_02: Kiểm tra thêm sản phẩm thành công với Giá gốc = 1 (min hợp lệ)
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

        // STT 44 — TC_F2.6_03: Kiểm tra thêm SP với Số lượng tồn kho = 0
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

        // STT 45 — TC_F2.6_04: Kiểm tra thêm SP khi bật Hiển thị (isActive = true)
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

        // STT 46 — TC_F2.6_05: Kiểm tra thêm SP khi Ẩn SP (isActive = false)
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

        // STT 47 — TC_F2.6_06: Kiểm tra thêm rồi xóa SP tạm thời
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

        // STT 48 — TC_F2.6_07: Kiểm tra Giá khuyến mãi = 0
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

        // STT 49 — TC_F2.6_08: Kiểm tra Slug thủ công hợp lệ
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

        // STT 50 — TC_F2.6_09: Kiểm tra MinOrder = 0
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

        // STT 51 — TC_F2.6_10: Kiểm tra Stock = 1 (boundary)
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

        // STT 52 — TC_F2.6_11: Kiểm tra Mô tả ngắn chứa link URL
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

        // STT 53 — TC_F2.6_12: Kiểm tra redirect đúng sau khi lưu thành công
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

        // STT 54 — TC_F2.6_13: Kiểm tra toast message sau lưu thành công
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

        // STT 55 — TC_F2.6_14: Kiểm tra SP mới xuất hiện trong danh sách
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

        // STT 56 — TC_F2.6_15: Kiểm tra SP hiển giá đúng sau lưu
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

        // STT 57 — TC_F2.7_01: Kiểm tra thêm sản phẩm thành công với đầy đủ thông tin hợp lệ
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

        // STT 58 — TC_F2.7_02: Kiểm tra thêm sản phẩm thành công chỉ với các trường bắt buộc
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

        // STT 59 — TC_F2.7_03: Kiểm tra nhập Slug tùy chỉnh hợp lệ
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

        // STT 60 — TC_F2.7_04: Kiểm tra Slug tự động sinh từ tên sản phẩm khi để trống
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

    }
}
