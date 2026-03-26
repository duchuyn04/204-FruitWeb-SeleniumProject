using OpenQA.Selenium;
using SeleniumProject.Utilities;

namespace SeleniumProject.Pages.ProductManagement
{
    public class CreateProductPage
    {
        private readonly IWebDriver _driver;
        private readonly WaitHelper _wait;

        // URL trang thêm sản phẩm — xây từ BaseUrl
        private readonly string _pageUrl;

        // ==============================================================
        // LOCATORS — địa chỉ của từng element trên form
        // ==============================================================

        // --- Thông tin cơ bản ---
        private readonly By NameInput            = By.Id("Product_Name");
        private readonly By SlugInput            = By.Id("Product_Slug");
        private readonly By ShortDescInput       = By.Id("Product_ShortDescription");
        private readonly By DescriptionInput     = By.Id("Product_Description");

        // --- Cài đặt (sidebar phải) ---
        private readonly By CategoryDropdown     = By.Id("Product_CategoryId");
        private readonly By IsActiveCheckbox     = By.Id("Product_IsActive");
        private readonly By IsFeaturedCheckbox   = By.Id("Product_IsFeatured");

        // --- Giá & Kho hàng ---
        private readonly By PriceInput           = By.Id("priceInput");
        private readonly By SalePriceInput       = By.Id("salePriceInput");
        private readonly By StockInput           = By.Id("Product_StockQuantity");
        private readonly By MinOrderInput        = By.Id("Product_MinOrderQuantity");

        // --- Thông tin chi tiết ---
        private readonly By UnitInput            = By.Id("Product_Unit");
        private readonly By WeightInput          = By.Id("Product_Weight");
        private readonly By CountryOriginInput   = By.Id("Product_CountryOrigin");
        private readonly By QualityInput         = By.Id("Product_Quality");

        // --- Hình ảnh ---
        private readonly By ImageFileInput       = By.Id("imageUploadInput");
        private readonly By ImagePreview         = By.CssSelector(".image-preview-container img");

        // --- Tags ---
        private readonly By TagsInput            = By.Id("tagsInput");
        private readonly By TagBadges            = By.CssSelector(".tag-badge");

        // --- Nút bấm ---
        private readonly By SaveButton           = By.CssSelector("button.btn-primary");
        private readonly By CancelButton         = By.CssSelector("a.btn-outline-secondary");
        private readonly By BackButton           = By.CssSelector("a[href='/Admin/Product'].btn-outline-secondary");

        // --- Thông báo toast ---
        private readonly By ToastMessage         = By.CssSelector(".toast-body");

        // --- Live preview giá (hiển thị ngay khi nhập giá gốc) ---
        // TODO: Xác nhận lại selector này với HTML thực tế của trang
        private readonly By PricePreviewText     = By.CssSelector(".price-display");

        // --- Thông báo lỗi validation (inline, dưới mỗi ô nhập) ---
        private readonly By ValidationErrors     = By.CssSelector(".text-danger");

        // --- Thông báo lỗi từ server (banner đỏ ở đầu trang) ---
        // VD: "Slug '...' đã tồn tại"
        private readonly By ServerErrorBanner    = By.CssSelector(".alert-danger, .alert.alert-danger");

        // ==============================================================
        // CONSTRUCTOR
        // ==============================================================

        public CreateProductPage(IWebDriver driver, string baseUrl)
        {
            _driver = driver;
            _wait = new WaitHelper(driver);
            _pageUrl = baseUrl.TrimEnd('/') + "/Admin/Product/Create";
        }

        // ==============================================================
        // ACTIONS — các hành động trên form
        // ==============================================================

        // Mở trang thêm sản phẩm
        public void Open()
        {
            _driver.Navigate().GoToUrl(_pageUrl);
        }

        // --- Thông tin cơ bản ---

        public void EnterName(string name)
        {
            _wait.SlowType(NameInput, name);
        }

        public void EnterSlug(string slug)
        {
            _wait.SlowType(SlugInput, slug);
        }

        public void EnterShortDescription(string text)
        {
            _wait.SlowType(ShortDescInput, text);
        }

        public void EnterDescription(string text)
        {
            _wait.SlowType(DescriptionInput, text);
        }

        // --- Cài đặt ---

        public void SelectCategory(string categoryName)
        {
            _wait.SelectDropdown(CategoryDropdown, categoryName);
        }

        // Bật/tắt checkbox Kích hoạt sản phẩm
        public void SetIsActive(bool shouldBeActive)
        {
            _wait.SetCheckbox(IsActiveCheckbox, shouldBeActive);
        }

        // Bật/tắt checkbox Sản phẩm nổi bật
        public void SetIsFeatured(bool shouldBeFeatured)
        {
            _wait.SetCheckbox(IsFeaturedCheckbox, shouldBeFeatured);
        }

        // --- Giá & Kho hàng ---

        public void EnterPrice(string price)
        {
            _wait.SlowType(PriceInput, price);
        }

        public void EnterSalePrice(string salePrice)
        {
            _wait.SlowType(SalePriceInput, salePrice);
        }

        public void EnterStock(string stock)
        {
            _wait.SlowType(StockInput, stock);
        }

        public void EnterMinOrder(string minOrder)
        {
            _wait.SlowType(MinOrderInput, minOrder);
        }

        // --- Thông tin chi tiết ---

        public void EnterUnit(string unit)
        {
            _wait.SlowType(UnitInput, unit);
        }

        public void EnterWeight(string weight)
        {
            _wait.SlowType(WeightInput, weight);
        }

        public void EnterCountryOrigin(string country)
        {
            _wait.SlowType(CountryOriginInput, country);
        }

        public void EnterQuality(string quality)
        {
            _wait.SlowType(QualityInput, quality);
        }

        // --- Hình ảnh ---

        // Upload 1 ảnh — truyền đường dẫn tuyệt đối
        public void UploadImage(string absoluteFilePath)
        {
            _wait.UploadFile(ImageFileInput, absoluteFilePath);
        }

        // Upload nhiều ảnh — nhận danh sách đường dẫn phân cách bằng dấu phẩy
        // Ví dụ: "C:\img1.jpg,C:\img2.png" → upload lần lượt từng file
        public void UploadImages(string commaSeparatedPaths)
        {
            string[] paths = commaSeparatedPaths.Split(',');
            foreach (string path in paths)
            {
                string trimmed = path.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    UploadImage(trimmed);
                }
            }
        }

        // --- Tags ---

        // Nhập 1 tag rồi nhấn Enter để thêm thành badge
        public void AddTag(string tag)
        {
            IWebElement tagInput = _wait.WaitForVisible(TagsInput);
            tagInput.SendKeys(tag);
            tagInput.SendKeys(Keys.Enter);
        }

        // Nhập nhiều tag cùng lúc — phân cách bằng dấu phẩy
        // Ví dụ: "trái cây,nhập khẩu"
        public void AddTags(string commaSeparatedTags)
        {
            string[] tags = commaSeparatedTags.Split(',');
            foreach (string tag in tags)
            {
                string trimmedTag = tag.Trim();
                if (trimmedTag.Length > 0)
                {
                    AddTag(trimmedTag);
                }
            }
        }

        // --- Nút bấm ---

        public void ClickSave()
        {
            _wait.ScrollIntoView(SaveButton);
            _wait.WaitForClickable(SaveButton).Click();
        }

        public void ClickCancel()
        {
            _wait.WaitForClickable(CancelButton).Click();
        }

        public void ClickBack()
        {
            _wait.WaitForClickable(BackButton).Click();
        }

        // ==============================================================
        // QUERIES — hỏi thông tin sau khi thao tác
        // ==============================================================

        // Lấy nội dung toast (thành công hoặc lỗi)
        public string GetToastMessage()
        {
            return _wait.WaitForToast(ToastMessage);
        }

        // Lấy tất cả thông báo lỗi validation trên form
        public IReadOnlyList<string> GetValidationErrors()
        {
            IReadOnlyCollection<IWebElement> errorElements = _driver.FindElements(ValidationErrors);
            List<string> errors = new List<string>();
            foreach (IWebElement el in errorElements)
            {
                string text = el.Text.Trim();
                if (text.Length > 0)
                {
                    errors.Add(text);
                }
            }
            return errors;
        }

        // Kiểm tra có lỗi validation nào hiển thị không
        public bool HasValidationErrors()
        {
            IReadOnlyList<string> errors = GetValidationErrors();
            return errors.Count > 0;
        }

        // Lấy slug hiện tại trong ô Slug (dùng để verify slug tự sinh)
        public string GetSlugValue()
        {
            IWebElement slugField = _wait.WaitForVisible(SlugInput);
            return slugField.GetAttribute("value");
        }

        // Kiểm tra ảnh preview có hiển thị sau khi upload không
        public bool IsImagePreviewDisplayed()
        {
            IReadOnlyCollection<IWebElement> previews = _driver.FindElements(ImagePreview);
            return previews.Count > 0;
        }

        // Đọc trạng thái checkbox Kích hoạt
        public bool IsActiveChecked()
        {
            IWebElement checkbox = _wait.WaitForVisible(IsActiveCheckbox);
            return checkbox.Selected;
        }

        // Đọc trạng thái checkbox Nổi bật
        public bool IsFeaturedChecked()
        {
            IWebElement checkbox = _wait.WaitForVisible(IsFeaturedCheckbox);
            return checkbox.Selected;
        }

        // Đọc giá trị mặc định của dropdown Danh mục
        public string GetCategoryDefaultText()
        {
            IWebElement dropdown = _wait.WaitForVisible(CategoryDropdown);
            OpenQA.Selenium.Support.UI.SelectElement select = new OpenQA.Selenium.Support.UI.SelectElement(dropdown);
            return select.SelectedOption.Text;
        }

        // Đếm số tag badge đang hiển thị
        public int GetTagCount()
        {
            IReadOnlyCollection<IWebElement> badges = _driver.FindElements(TagBadges);
            return badges.Count;
        }

        // Đọc text live preview giá (ví dụ: "75.000 VND") hiển thị ngay dưới ô nhập giá
        // Dùng trong TC_F2.5_07 để xác nhận preview cập nhật ngay khi nhập
        public string GetPricePreviewText()
        {
            try
            {
                IWebElement previewEl = _wait.WaitForVisible(PricePreviewText);
                return previewEl.Text.Trim();
            }
            catch
            {
                // Trả về chuỗi rỗng nếu element chưa hiển thị
                return string.Empty;
            }
        }

        // Lấy URL hiện tại — dùng để kiểm tra redirect sau khi lưu
        public string GetCurrentUrl()
        {
            return _driver.Url;
        }

        // Kiểm tra đã redirect về trang danh sách sau khi lưu thành công
        public bool IsRedirectedToList()
        {
            return _driver.Url.Contains("/Admin/Product") && !_driver.Url.Contains("/Create");
        }

        // Lấy thông báo lỗi đầu tiên hiển thị trên form
        // Trả về chuỗi rỗng nếu không có lỗi nào
        public string GetFirstValidationError()
        {
            IReadOnlyList<string> danhSachLoi = GetValidationErrors();
            if (danhSachLoi.Count > 0)
            {
                return danhSachLoi[0];
            }
            return "";
        }

        // Lấy thông báo lỗi từ server (banner đỏ ở đầu trang)
        // VD: "Slug '...' đã tồn tại"
        public string GetServerError()
        {
            try
            {
                IReadOnlyCollection<IWebElement> banners = _driver.FindElements(ServerErrorBanner);
                foreach (IWebElement banner in banners)
                {
                    string text = banner.Text.Trim();
                    if (text.Length > 0)
                    {
                        return text;
                    }
                }
            }
            catch
            {
                // Không có banner lỗi
            }
            return "";
        }

        // Đọc kết quả thực tế mà hệ thống trả ra sau khi submit form
        // Ưu tiên: Lỗi server (banner) → Toast → Lỗi validation inline → URL
        // Dùng để gán vào CurrentActualResult trong mỗi test case
        public string DocKetQuaThucTe()
        {
            string urlHienTai = _driver.Url;

            // Ưu tiên 1: Lỗi từ server hiển thị trên banner đỏ đầu trang
            // VD: "Slug '...' đã tồn tại"
            string loiServer = GetServerError();
            if (!string.IsNullOrEmpty(loiServer))
            {
                return $"Lỗi server: '{loiServer}' | URL: {urlHienTai}";
            }

            // Ưu tiên 2: Toast notification — đọc NGAY LẬP TỨC (không dùng WaitForToast
            // vì WaitForToast block đến hết timeout nếu toast đã biến mất sau redirect)
            IReadOnlyCollection<IWebElement> toastElements = _driver.FindElements(ToastMessage);
            foreach (IWebElement toastEl in toastElements)
            {
                string toastText = toastEl.Text.Trim();
                if (toastText.Length > 0)
                {
                    return $"Toast: '{toastText}' | URL: {urlHienTai}";
                }
            }

            // Ưu tiên 3: Lỗi validation inline dưới mỗi ô nhập
            string loiValidation = GetFirstValidationError();
            if (!string.IsNullOrEmpty(loiValidation))
            {
                return $"Lỗi validation: '{loiValidation}' | URL: {urlHienTai}";
            }

            // Mặc định: chỉ trả về URL hiện tại
            return $"URL: {urlHienTai}";
        }
    }
}
