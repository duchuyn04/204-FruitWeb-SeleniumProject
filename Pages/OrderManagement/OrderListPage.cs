using OpenQA.Selenium;
using SeleniumProject.Utilities;

namespace SeleniumProject.Pages.OrderManagement
{
    public class OrderListPage
    {
        private readonly IWebDriver _driver;
        private readonly WaitHelper _wait;
        private readonly string _pageUrl;

        // ==============================================================
        // LOCATORS - Trang Danh sách Đơn hàng (/Admin/Order)
        // ==============================================================

        // --- Thanh lọc ---
        private readonly By SearchInput         = By.Id("filterSearch");
        private readonly By StatusDropdown      = By.Id("filterStatus");
        private readonly By PaymentDropdown     = By.Id("filterPaymentStatus");
        private readonly By FromDateInput       = By.Id("filterFromDate");
        private readonly By ToDateInput         = By.Id("filterToDate");
        private readonly By TotalCountHidden    = By.Id("orderTotalCount");

        // --- Bảng danh sách ---
        private readonly By TableBody           = By.CssSelector("table tbody");
        private readonly By TableRows           = By.CssSelector("table tbody tr");
        // Mã đơn (cột 1): a.fw-bold.text-primary trong td[0]
        private readonly By OrderCodeLinks      = By.CssSelector("table tbody tr td:first-child a.fw-bold.text-primary");
        // Nút xem chi tiết (cột cuối)
        private readonly By ViewDetailBtns      = By.CssSelector("table tbody tr td:last-child a.btn.btn-sm.btn-outline-primary");
        // Badge trạng thái đơn (cột 4 - index 3)
        private readonly By StatusBadges        = By.CssSelector("table tbody tr td:nth-child(4) span.badge");
        // Badge trạng thái thanh toán (cột 5 - index 4)
        private readonly By PaymentBadges       = By.CssSelector("table tbody tr td:nth-child(5) span.badge");

        // --- Pagination ---
        private readonly By PaginationLinks     = By.CssSelector(".pagination .page-link");
        private readonly By PaginationPrev      = By.CssSelector(".pagination .page-link:first-child");
        private readonly By PaginationNext      = By.CssSelector(".pagination .page-link:last-child");

        // ==============================================================
        public OrderListPage(IWebDriver driver, string baseUrl)
        {
            _driver = driver;
            _wait = new WaitHelper(driver);
            _pageUrl = baseUrl + "/Admin/Order";
        }

        public void Open() => _driver.Navigate().GoToUrl(_pageUrl);

        // --- Tổng số đơn hàng ---
        public int GetTotalOrderCount()
        {
            var el = _driver.FindElement(TotalCountHidden);
            return int.TryParse(el.GetAttribute("value"), out int count) ? count : 0;
        }

        // --- Tìm kiếm ---
        public void SearchByOrderCode(string code)
        {
            var input = _driver.FindElement(SearchInput);
            input.Clear();
            input.SendKeys(code);
        }

        public string GetSearchValue()
            => _driver.FindElement(SearchInput).GetAttribute("value") ?? "";

        // --- Lọc trạng thái ---
        public void SelectStatus(string statusText)
        {
            var select = new OpenQA.Selenium.Support.UI.SelectElement(_driver.FindElement(StatusDropdown));
            select.SelectByText(statusText);
        }

        public void SelectPaymentStatus(string statusText)
        {
            var select = new OpenQA.Selenium.Support.UI.SelectElement(_driver.FindElement(PaymentDropdown));
            select.SelectByText(statusText);
        }

        // --- Lọc ngày ---
        public void SetFromDate(string date) // format: mm/dd/yyyy
        {
            var el = _driver.FindElement(FromDateInput);
            el.Clear();
            el.SendKeys(date);
        }

        public void SetToDate(string date)
        {
            var el = _driver.FindElement(ToDateInput);
            el.Clear();
            el.SendKeys(date);
        }

        // --- Bảng ---
        public IReadOnlyCollection<IWebElement> GetAllRows()
            => _driver.FindElements(TableRows);

        public IWebElement GetFirstRow()
            => _driver.FindElements(TableRows)[0];

        public string GetOrderCodeOfRow(int rowIndex = 0)
        {
            var links = _driver.FindElements(OrderCodeLinks);
            return rowIndex < links.Count ? links[rowIndex].Text.Trim() : "";
        }

        public string GetStatusBadgeOfRow(int rowIndex = 0)
        {
            var badges = _driver.FindElements(StatusBadges);
            return rowIndex < badges.Count ? badges[rowIndex].Text.Trim() : "";
        }

        public string GetPaymentBadgeOfRow(int rowIndex = 0)
        {
            var badges = _driver.FindElements(PaymentBadges);
            return rowIndex < badges.Count ? badges[rowIndex].Text.Trim() : "";
        }

        // --- Điều hướng sang chi tiết ---
        public void ClickViewDetail(int rowIndex = 0)
        {
            var btns = _driver.FindElements(ViewDetailBtns);
            if (rowIndex < btns.Count) btns[rowIndex].Click();
        }

        public void ClickOrderCode(int rowIndex = 0)
        {
            var links = _driver.FindElements(OrderCodeLinks);
            if (rowIndex < links.Count) links[rowIndex].Click();
        }

        // --- Phân trang ---
        public void ClickNextPage() => _driver.FindElement(PaginationNext).Click();
        public void ClickPrevPage() => _driver.FindElement(PaginationPrev).Click();
        public void ClickPage(int pageNum)
        {
            var links = _driver.FindElements(PaginationLinks);
            var target = links.FirstOrDefault(l => l.Text.Trim() == pageNum.ToString());
            target?.Click();
        }

        // --- Helpers ---
        public bool HasRows() => _driver.FindElements(TableRows).Count > 0;
        public bool IsOnPage() => _driver.Url.Contains("/Admin/Order");

        public bool IsPageHealthy()
        {
            try
            {
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
                var errors = _driver.FindElements(By.CssSelector(".field-validation-error, .alert-danger"));
                return errors.Count == 0;
            }
            finally
            {
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);
            }
        }

        public string DocKetQuaThucTe()
        {
            try
            {
                var toasts = _driver.FindElements(By.CssSelector(".toast-body, .alert, [class*='toast']"));
                foreach (var t in toasts)
                    if (t.Displayed && !string.IsNullOrWhiteSpace(t.Text))
                        return "Toast: " + t.Text.Trim();

                return $"URL={_driver.Url} | Total={GetTotalOrderCount()}";
            }
            catch
            {
                return $"URL={_driver.Url}";
            }
        }

        // Lấy danh sách tiêu đề cột bảng (thay Driver.FindElements trong test)
        public List<string> GetTableHeaders()
        {
            var headers = _driver.FindElements(By.CssSelector("table thead th"));
            var result = new List<string>();
            foreach (var h in headers) result.Add(h.Text.Trim());
            return result;
        }
    }
}
