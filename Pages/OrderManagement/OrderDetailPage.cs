using OpenQA.Selenium;
using SeleniumProject.Utilities;

namespace SeleniumProject.Pages.OrderManagement
{
    public class OrderDetailPage
    {
        private readonly IWebDriver _driver;
        private readonly WaitHelper _wait;

        // ==============================================================
        // LOCATORS - Trang Chi tiết Đơn hàng (/Admin/Order/Detail/{id})
        // ==============================================================

        // --- Header ---
        // Selector: h1.text-xl - chứa mã đơn + badge trạng thái + ngày tạo
        private readonly By OrderHeader         = By.CssSelector("h1.text-xl");
        private readonly By OrderStatusBadge    = By.CssSelector("h1.text-xl .badge:first-of-type, h1.text-xl span.badge:first-child");
        private readonly By PaymentStatusBadge  = By.CssSelector("h1.text-xl .badge:last-of-type, h1.text-xl span.badge:last-child");

        // --- Nút Header ---
        // Nút Quay lại: a[href="/Admin/Order"] trong header
        private readonly By BackBtn             = By.CssSelector("a[href='/Admin/Order']");
        // Nút Dark mode: button#darkModeToggle
        private readonly By DarkModeBtn         = By.Id("darkModeToggle");
        // Nút In đơn hàng: button chứa text "In đơn hàng"
        private readonly By PrintBtn            = By.XPath("//button[contains(., 'In đơn hàng')]");

        // --- Bảng Sản phẩm trong đơn ---
        private readonly By ProductTable        = By.CssSelector("table");
        private readonly By ProductRows         = By.CssSelector("table tbody tr");
        private readonly By SubtotalLabel       = By.XPath("//span[contains(text(),'Tạm tính')]/following-sibling::span");
        private readonly By ShippingFeeLabel    = By.XPath("//span[contains(text(),'Phí vận chuyển')]/following-sibling::span");
        private readonly By TotalLabel          = By.XPath("//*[contains(text(),'Tổng cộng:')]/following-sibling::*");

        // --- Section Ghi chú nội bộ ---
        // Textarea ghi chú: #noteContent
        private readonly By NoteTextarea        = By.Id("noteContent");
        private readonly By NoteCharCounter     = By.XPath("//textarea[@id='noteContent']/following-sibling::*[contains(text(),'/1000')]");
        // Nút Thêm ghi chú: button#addNoteBtn
        private readonly By AddNoteBtn          = By.Id("addNoteBtn");
        private readonly By NoteList            = By.CssSelector("[class*='note'], .note-item");

        // --- Nút hành động chính ---
        // Nút Xác nhận đã thu tiền (xanh): button.bg-emerald-600
        private readonly By ConfirmPaymentBtn   = By.CssSelector("button.w-full.bg-emerald-600");
        // Nút Duyệt đơn & Chuẩn bị hàng (xanh dương): button.bg-primary
        private readonly By ApproveOrderBtn     = By.CssSelector("button.w-full.bg-primary");
        // Nút Hủy đơn hàng: button có text "Hủy đơn hàng"
        private readonly By CancelOrderBtn      = By.XPath("//button[contains(., 'Hủy đơn hàng') and contains(@class,'w-full')]");

        // --- Modal Xác nhận chung (actionModal) ---
        private readonly By ActionModal         = By.Id("actionModal");
        private readonly By ActionModalMessage  = By.Id("actionMessage");
        private readonly By ActionModalConfirm  = By.CssSelector("#actionModal .btn-primary");
        private readonly By ActionModalCancel   = By.CssSelector("#actionModal [data-bs-dismiss='modal']");

        // --- Modal Hủy đơn (cancelModal) ---
        private readonly By CancelModal         = By.Id("cancelModal");
        private readonly By CancelReasonInput   = By.CssSelector("#cancelModal textarea[name='notes']");
        private readonly By CancelConfirmBtn    = By.CssSelector("#cancelModal .btn-danger"); // "Xác nhận hủy đơn"
        private readonly By CancelModalDismiss  = By.CssSelector("#cancelModal .btn-secondary");

        // --- Modal Hoàn hàng (returnModal) ---
        private readonly By ReturnModal         = By.Id("returnModal");
        private readonly By ReturnReasonInput   = By.CssSelector("#returnModal textarea[name='notes']");
        private readonly By ReturnConfirmBtn    = By.CssSelector("#returnModal .btn-warning"); // "Xác nhận hoàn hàng"

        // --- Modal Thu tiền (paymentModal) ---
        private readonly By PaymentModal        = By.Id("paymentModal");
        private readonly By PaymentNoteInput    = By.CssSelector("#paymentModal textarea[name='notes']");
        private readonly By PaymentConfirmBtn   = By.CssSelector("#paymentModal .btn-success"); // "Xác nhận đã thu tiền"

        // --- Textarea Ghi chú action (trong modal chung) ---
        private readonly By ActionNoteInput     = By.Id("actionNotes");

        // ==============================================================
        public OrderDetailPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WaitHelper(driver);
        }

        // --- Header info ---
        public string GetOrderCode()
        {
            var header = _driver.FindElement(OrderHeader);
            // Lay dong dau text, bo badge phia sau
            return header.Text.Split('\n')[0].Trim();
        }

        public string GetOrderStatus()
        {
            try
            {
                var badges = _driver.FindElements(By.CssSelector("h1.text-xl span.badge"));
                return badges.Count > 0 ? badges[0].Text.Trim() : "";
            }
            catch { return ""; }
        }

        public string GetPaymentStatus()
        {
            try
            {
                var badges = _driver.FindElements(By.CssSelector("h1.text-xl span.badge"));
                return badges.Count > 1 ? badges[1].Text.Trim() : "";
            }
            catch { return ""; }
        }

        // --- Header buttons ---
        public void ClickBack() => _driver.FindElement(BackBtn).Click();
        public void ClickPrint() => _driver.FindElement(PrintBtn).Click();
        public void ClickDarkMode() => _driver.FindElement(DarkModeBtn).Click();

        // --- Ghi chú nội bộ ---
        public void EnterNote(string text)
        {
            var ta = _driver.FindElement(NoteTextarea);
            ta.Clear();
            ta.SendKeys(text);
        }

        public string GetNoteCharCount()
        {
            try { return _driver.FindElement(NoteCharCounter).Text.Trim(); }
            catch { return ""; }
        }

        public void ClickAddNote() => _driver.FindElement(AddNoteBtn).Click();

        // --- Hành động đơn hàng ---
        public void ClickApproveOrder()
        {
            _wait.WaitForVisible(ApproveOrderBtn);
            _driver.FindElement(ApproveOrderBtn).Click();
        }

        public void ClickConfirmPayment()
        {
            _wait.WaitForVisible(ConfirmPaymentBtn);
            _driver.FindElement(ConfirmPaymentBtn).Click();
        }

        public void ClickCancelOrder()
        {
            _wait.WaitForVisible(CancelOrderBtn);
            _driver.FindElement(CancelOrderBtn).Click();
        }

        // --- Modal Hủy đơn ---
        public bool IsCancelModalVisible()
            => _driver.FindElements(CancelModal).Count > 0 && _driver.FindElement(CancelModal).Displayed;

        public void EnterCancelReason(string reason)
            => _driver.FindElement(CancelReasonInput).SendKeys(reason);

        public void ConfirmCancelOrder() => _driver.FindElement(CancelConfirmBtn).Click();
        public void DismissCancelModal() => _driver.FindElement(CancelModalDismiss).Click();

        // --- Modal Thu tiền ---
        public bool IsPaymentModalVisible()
            => _driver.FindElements(PaymentModal).Count > 0 && _driver.FindElement(PaymentModal).Displayed;

        public void EnterPaymentNote(string note)
            => _driver.FindElement(PaymentNoteInput).SendKeys(note);

        public void ConfirmPaymentCollection() => _driver.FindElement(PaymentConfirmBtn).Click();

        // --- Modal Hoàn hàng ---
        public bool IsReturnModalVisible()
            => _driver.FindElements(ReturnModal).Count > 0 && _driver.FindElement(ReturnModal).Displayed;

        public void EnterReturnReason(string reason)
            => _driver.FindElement(ReturnReasonInput).SendKeys(reason);

        public void ConfirmReturn() => _driver.FindElement(ReturnConfirmBtn).Click();

        // --- Modal chung (Duyệt đơn) ---
        public string GetActionModalMessage()
        {
            _wait.WaitForVisible(ActionModal);
            return _driver.FindElement(ActionModalMessage).Text.Trim();
        }

        public void ConfirmActionModal() => _driver.FindElement(ActionModalConfirm).Click();
        public void DismissActionModal() => _driver.FindElement(ActionModalCancel).Click();

        // --- Helpers ---
        public bool IsOnPage(int orderId)
            => _driver.Url.Contains($"/Admin/Order/Detail/{orderId}");

        public bool IsApproveButtonVisible()
        {
            try { return _driver.FindElements(ApproveOrderBtn).Count > 0; }
            catch { return false; }
        }

        public bool IsConfirmPaymentButtonVisible()
        {
            try { return _driver.FindElements(ConfirmPaymentBtn).Count > 0; }
            catch { return false; }
        }

        public string DocKetQuaThucTe()
        {
            try
            {
                // Toast
                var toasts = _driver.FindElements(By.CssSelector(".toast-body, .alert, [class*='toast']"));
                foreach (var t in toasts)
                    if (t.Displayed && !string.IsNullOrWhiteSpace(t.Text))
                        return "Toast: " + t.Text.Trim();

                return $"URL={_driver.Url} | Status={GetOrderStatus()} | Payment={GetPaymentStatus()}";
            }
            catch
            {
                return $"URL={_driver.Url}";
            }
        }
    }
}
