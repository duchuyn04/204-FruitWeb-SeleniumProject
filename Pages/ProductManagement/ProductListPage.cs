using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumProject.Utilities;

namespace SeleniumProject.Pages.ProductManagement
{
    public class ProductListPage
    {
        private readonly IWebDriver _driver;
        private readonly WaitHelper _wait;

        // URL trang danh sách sản phẩm
        private readonly string _pageUrl;

        // ==============================================================
        // LOCATORS — địa chỉ của từng element trên trang danh sách
        // ==============================================================

        // --- Bộ lọc & Tìm kiếm ---
        private readonly By SearchInput          = By.Id("filterSearch");
        private readonly By CategoryDropdown     = By.Id("filterCategory");
        private readonly By SortDropdown         = By.Id("filterSort");
        private readonly By LoadingIndicator     = By.Id("loadingIndicator");

        // --- Bảng danh sách sản phẩm ---
        private readonly By ProductTable         = By.CssSelector("table.table");
        private readonly By ProductRows          = By.CssSelector("table.table tbody tr");
        private readonly By ProductNameCells     = By.CssSelector("table.table tbody tr td:nth-child(2)");

        // --- Bộ đếm sản phẩm ---
        private readonly By ProductCountHeader   = By.CssSelector(".card-header");

        // --- Thông báo không tìm thấy ---
        private readonly By NoResultMessage      = By.CssSelector(".text-center.py-5");

        // --- Phân trang ---
        private readonly By PaginationLinks      = By.CssSelector("nav ul.pagination li a");
        private readonly By PaginationNextBtn    = By.XPath("//nav//a[text()='Sau']");
        private readonly By PaginationPrevBtn    = By.XPath("//nav//a[text()='Trước']");

        // --- Nút chức năng ---
        private readonly By TrashButton          = By.CssSelector("a[href='/Admin/Product/Trash']");
        private readonly By AddProductButton     = By.CssSelector("a[href='/Admin/Product/Create']");

        // ==============================================================
        // CONSTRUCTOR
        // ==============================================================

        public ProductListPage(IWebDriver driver, string baseUrl)
        {
            _driver = driver;
            _wait = new WaitHelper(driver);
            _pageUrl = baseUrl.TrimEnd('/') + "/Admin/Product";
        }

        // ==============================================================
        // ACTIONS — các hành động trên trang
        // ==============================================================

        // Mở trang danh sách sản phẩm
        public void Open()
        {
            _driver.Navigate().GoToUrl(_pageUrl);
        }

        // Mở trang danh sách sản phẩm với URL tùy chỉnh (có query string)
        public void OpenWithUrl(string fullUrl)
        {
            _driver.Navigate().GoToUrl(fullUrl);
        }

        // Nhập từ khóa vào ô tìm kiếm — trang tự động filter (debounce)
        public void Search(string keyword)
        {
            IWebElement searchBox = _wait.WaitForVisible(SearchInput);
            searchBox.Clear();
            searchBox.SendKeys(keyword);

            // Chờ debounce hoàn tất — trang cập nhật kết quả qua AJAX/URL
            Thread.Sleep(800);
        }

        // Xóa trắng ô tìm kiếm — danh sách trở về trạng thái ban đầu
        public void ClearSearch()
        {
            IWebElement searchBox = _wait.WaitForVisible(SearchInput);
            searchBox.Clear();

            // Gửi phím để kích hoạt sự kiện input/change
            searchBox.SendKeys(" ");
            Thread.Sleep(200);
            searchBox.Clear();

            // Chờ debounce hoàn tất
            Thread.Sleep(800);
        }

        // Chọn danh mục lọc
        public void SelectCategory(string categoryName)
        {
            _wait.SelectDropdown(CategoryDropdown, categoryName);

            // Chờ filter cập nhật kết quả
            Thread.Sleep(800);
        }

        // Chọn kiểu sắp xếp
        public void SelectSort(string sortOption)
        {
            _wait.SelectDropdown(SortDropdown, sortOption);

            // Chờ sort cập nhật kết quả
            Thread.Sleep(800);
        }

        // Mở dropdown thao tác của một sản phẩm cụ thể và chọn Xóa tạm thời
        public void DeleteProductSoft(string productName)
        {
            Search(productName);
            Thread.Sleep(1000);

            // Tìm hàng chứa tên SP
            IReadOnlyCollection<IWebElement> rows = _driver.FindElements(ProductRows);
            IWebElement targetRow = null;
            foreach (IWebElement row in rows)
            {
                if (row.Text.Contains(productName))
                {
                    targetRow = row;
                    break;
                }
            }

            if (targetRow != null)
            {
                // Cuộn tới dòng đó
                IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", targetRow);
                Thread.Sleep(300);

                // Click dropdown toggle
                IWebElement dropdownToggle = targetRow.FindElement(By.CssSelector(".dropdown-toggle"));
                dropdownToggle.Click();
                Thread.Sleep(500);

                // Click 'Chuyển vào thùng rác'
                IWebElement softDeleteOpt = targetRow.FindElement(By.XPath(".//a[contains(@onclick, 'confirmSoftDelete')]"));
                softDeleteOpt.Click();
                Thread.Sleep(500);

                // Click confirm trong Modal
                IWebElement confirmBtn = _wait.WaitForClickable(By.CssSelector("#softDeleteForm button[type='submit']"));
                confirmBtn.Click();
                Thread.Sleep(1000); // Chờ reload trang sau khi xóa
            }
        }

        // ==============================================================
        // QUERIES — hỏi thông tin từ trang
        // ==============================================================

        // Đếm số hàng sản phẩm hiển thị trong bảng
        public int GetProductRowCount()
        {
            IReadOnlyCollection<IWebElement> rows = _driver.FindElements(ProductRows);
            return rows.Count;
        }

        // Lấy tất cả tên sản phẩm đang hiển thị trong bảng
        public List<string> GetProductNames()
        {
            IReadOnlyCollection<IWebElement> cells = _driver.FindElements(ProductNameCells);
            List<string> names = new List<string>();
            foreach (IWebElement cell in cells)
            {
                // Tên sản phẩm nằm trong thẻ con đầu tiên (thẻ <strong> hoặc <span>)
                // Lấy text đầy đủ rồi lấy dòng đầu (loại slug bên dưới)
                string fullText = cell.Text.Trim();
                if (!string.IsNullOrEmpty(fullText))
                {
                    string[] lines = fullText.Split('\n');
                    names.Add(lines[0].Trim());
                }
            }
            return names;
        }

        // Kiểm tra một sản phẩm cụ thể có xuất hiện trong kết quả không
        public bool IsProductInResults(string productName)
        {
            List<string> names = GetProductNames();
            foreach (string name in names)
            {
                if (name.Contains(productName))
                {
                    return true;
                }
            }
            return false;
        }

        // Lấy số hiển thị trong header "Danh sách sản phẩm (N)"
        public int GetDisplayedProductCount()
        {
            try
            {
                IWebElement header = _wait.WaitForVisible(ProductCountHeader);
                string text = header.Text.Trim();

                // Parse số từ "Danh sách sản phẩm (15)" → 15
                int startIndex = text.IndexOf('(');
                int endIndex = text.IndexOf(')');
                if (startIndex >= 0 && endIndex > startIndex)
                {
                    string numberStr = text.Substring(startIndex + 1, endIndex - startIndex - 1);
                    int count;
                    if (int.TryParse(numberStr, out count))
                    {
                        return count;
                    }
                }
            }
            catch
            {
                // Header không hiển thị
            }
            return -1;
        }

        // Kiểm tra bảng sản phẩm có hiển thị hay không
        public bool IsProductTableDisplayed()
        {
            IReadOnlyCollection<IWebElement> tables = _driver.FindElements(ProductTable);
            return tables.Count > 0;
        }

        // Lấy thông báo "Không tìm thấy sản phẩm nào"
        public string GetNoResultMessage()
        {
            try
            {
                IReadOnlyCollection<IWebElement> msgs = _driver.FindElements(NoResultMessage);
                foreach (IWebElement msg in msgs)
                {
                    string text = msg.Text.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        return text;
                    }
                }
            }
            catch
            {
                // Không có thông báo
            }
            return "";
        }

        // Kiểm tra có hiển thị thông báo "Không tìm thấy" không
        public bool HasNoResultMessage()
        {
            string msg = GetNoResultMessage();
            return !string.IsNullOrEmpty(msg);
        }

        // Lấy giá trị hiện tại trong ô tìm kiếm
        public string GetSearchInputValue()
        {
            IWebElement searchBox = _wait.WaitForVisible(SearchInput);
            return searchBox.GetAttribute("value");
        }

        // Lấy URL hiện tại
        public string GetCurrentUrl()
        {
            return _driver.Url;
        }

        // Kiểm tra URL có chứa query string tìm kiếm không
        public bool UrlContainsSearch(string expectedParam)
        {
            return _driver.Url.Contains(expectedParam);
        }

        // Kiểm tra trang có bị crash/error page không
        public bool IsPageHealthy()
        {
            try
            {
                // Trang khỏe = heading "Quản lý sản phẩm" vẫn hiển thị
                IWebElement heading = _driver.FindElement(By.CssSelector("h4"));
                return heading.Text.Contains("Quản lý sản phẩm");
            }
            catch
            {
                return false;
            }
        }

        // Đọc kết quả thực tế mà hệ thống trả ra sau khi search
        // Ưu tiên: Thông báo trống → Số lượng kết quả + tên SP → URL
        public string DocKetQuaThucTe()
        {
            string urlHienTai = _driver.Url;
            int soLuong = GetDisplayedProductCount();

            // Nếu 0 kết quả và có thông báo trống
            if (soLuong == 0 && HasNoResultMessage())
            {
                return $"Không tìm thấy: '{GetNoResultMessage()}' | Count: {soLuong} | URL: {urlHienTai}";
            }

            // Có kết quả → liệt kê tên SP tìm thấy
            List<string> names = GetProductNames();
            string danhSach = names.Count > 0 ? string.Join(", ", names) : "(trống)";

            return $"Tìm thấy {soLuong} SP: [{danhSach}] | URL: {urlHienTai}";
        }

        public IWebElement GetSearchInput()
        {
            return _wait.WaitForVisible(SearchInput);
        }

        public string GetSearchPlaceholder()
        {
            IWebElement searchBox = _wait.WaitForVisible(SearchInput);
            return searchBox.GetAttribute("placeholder");
        }

        public bool IsEmptyMessageDisplayed()
        {
            return HasNoResultMessage();
        }

        public string GetCurrentSortValue()
        {
            try
            {
                IWebElement sortDropdown = _wait.WaitForVisible(SortDropdown);
                SelectElement select = new SelectElement(sortDropdown);
                return select.SelectedOption.Text.Trim();
            }
            catch
            {
                return "";
            }
        }

        public string GetCurrentCategoryValue()
        {
            try
            {
                IWebElement categoryDropdown = _wait.WaitForVisible(CategoryDropdown);
                SelectElement select = new SelectElement(categoryDropdown);
                return select.SelectedOption.Text.Trim();
            }
            catch
            {
                return "";
            }
        }
    }
}
