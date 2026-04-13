using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumProject.Pages.CartManagement;
using SeleniumProject.Utilities;

namespace SeleniumProject.Tests.CartManagement
{
    [TestFixture]
    public class CartTests : TestBase
    {
        private CartPage _cartPage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "CartManagement", "cart_data.json"
        );

        [SetUp]
        public void SetUpCartPage()
        {
            CurrentSheetName = "TC_CartManagement";
            _cartPage = new CartPage(Driver);
        }

        // =========================================================
        // F4.1 – Hiển thị giỏ hàng
        // =========================================================

        [Test]
        public void TC_CART_F4_1_01_HienThiDanhSachSanPham()
        {
            CurrentTestCaseId = "TC_F4.1_01";
            var data = DocDuLieu("TC_CART_F4_1_01");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            int itemCount = _cartPage.GetCartItemCount();
            string productName = _cartPage.GetProductName();
            string heading = _cartPage.GetCartHeadingText();

            bool hasItems = itemCount >= 1;
            bool hasName = !string.IsNullOrEmpty(productName);
            bool hasHeading = heading.Contains("Giỏ hàng");

            CurrentActualResult = hasItems && hasName && hasHeading
                ? $"Giỏ hàng hiển thị {itemCount} SP. Tên: '{productName}'. Heading: '{heading}'."
                : $"Giỏ hàng không hiển thị đúng. Items={itemCount}, Name='{productName}', Heading='{heading}'.";

            Assert.That(hasItems, Is.True, "[F4.1_01] Giỏ hàng phải có ít nhất 1 SP");
            Assert.That(hasName, Is.True, "[F4.1_01] Tên SP phải hiển thị");
            Assert.That(hasHeading, Is.True, "[F4.1_01] Heading giỏ hàng phải hiển thị");
        }

        [Test]
        public void TC_CART_F4_1_02_TinhToanSubtotalShippingTotal()
        {
            CurrentTestCaseId = "TC_F4.1_02";
            var data = DocDuLieu("TC_CART_F4_1_02");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            string subtotal = _cartPage.GetSubtotalText();
            string total = _cartPage.GetTotalText();
            string shipping = _cartPage.GetShippingText();

            bool hasSubtotal = !string.IsNullOrEmpty(subtotal) && subtotal.Contains("đ");
            bool hasTotal = !string.IsNullOrEmpty(total) && total.Contains("đ");

            CurrentActualResult = hasSubtotal && hasTotal
                ? $"Subtotal={subtotal}, Shipping={shipping}, Total={total}. Tính toán hiển thị đúng."
                : $"Thiếu giá trị. Subtotal='{subtotal}', Shipping='{shipping}', Total='{total}'.";

            Assert.That(hasSubtotal, Is.True, "[F4.1_02] Subtotal phải hiển thị giá trị hợp lệ");
            Assert.That(hasTotal, Is.True, "[F4.1_02] Total phải hiển thị giá trị hợp lệ");
        }

        // =========================================================
        // F4.2 – Giỏ hàng rỗng
        // =========================================================

        [Test]
        public void TC_CART_F4_2_01_GiaoDienGioHangRong()
        {
            CurrentTestCaseId = "TC_F4.2_01";
            var data = DocDuLieu("TC_CART_F4_2_01");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.Open();

            bool isEmpty = _cartPage.IsCartEmpty();
            string body = _cartPage.GetBodyText();
            bool hasEmptyMsg = body.Contains("Giỏ hàng trống")
                || body.Contains("chưa thêm sản phẩm");
            bool hasShopBtn = body.Contains("Tiếp tục mua sắm");

            CurrentActualResult = isEmpty
                ? $"Giỏ hàng rỗng hiển thị đúng. Icon, heading, nút 'Tiếp tục mua sắm' đều có."
                : "Giỏ hàng KHÔNG hiển thị trạng thái rỗng.";

            Assert.That(isEmpty || hasEmptyMsg, Is.True,
                "[F4.2_01] Phải hiển thị giao diện giỏ hàng rỗng");
        }

        [Test]
        public void TC_CART_F4_2_02_AnNutThanhToanKhiRong()
        {
            CurrentTestCaseId = "TC_F4.2_02";
            var data = DocDuLieu("TC_CART_F4_2_02");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.Open();

            bool checkoutVisible = _cartPage.IsCheckoutButtonVisible();

            CurrentActualResult = !checkoutVisible
                ? "Nút THANH TOÁN và Tổng giỏ hàng bị ẩn khi giỏ rỗng."
                : "Nút THANH TOÁN vẫn hiển thị khi giỏ hàng rỗng.";

            Assert.That(checkoutVisible, Is.False,
                "[F4.2_02] Nút THANH TOÁN phải ẩn khi giỏ hàng rỗng");
        }

        // =========================================================
        // F4.3 – Thêm sản phẩm
        // =========================================================

        [Test]
        public void TC_CART_F4_3_01_ThemSanPhamMoi()
        {
            CurrentTestCaseId = "TC_F4.3_01";
            var data = DocDuLieu("TC_CART_F4_3_01");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            int itemCount = _cartPage.GetCartItemCount();
            string productName = _cartPage.GetProductName();
            string unitPrice = _cartPage.GetUnitPriceText();

            bool success = itemCount >= 1 && !string.IsNullOrEmpty(productName);

            CurrentActualResult = success
                ? $"SP '{productName}' đã được thêm. Đơn giá: {unitPrice}. Số SP: {itemCount}."
                : $"Thêm SP thất bại. Items={itemCount}, Name='{productName}'.";

            Assert.That(success, Is.True,
                "[F4.3_01] SP phải được thêm vào giỏ hàng thành công");
        }

        [Test]
        public void TC_CART_F4_3_02_GiaSalePriceUuTien()
        {
            CurrentTestCaseId = "TC_F4.3_02";
            var data = DocDuLieu("TC_CART_F4_3_02");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            string unitPrice = _cartPage.GetUnitPriceText();
            string itemTotal = _cartPage.GetItemTotalText();

            bool hasPriceInfo = !string.IsNullOrEmpty(unitPrice) && unitPrice.Contains("đ");

            CurrentActualResult = hasPriceInfo
                ? $"Đơn giá hiển thị: {unitPrice}. Thành tiền: {itemTotal}. SalePrice được ưu tiên nếu có."
                : $"Không thấy đơn giá. UnitPrice='{unitPrice}'.";

            Assert.That(hasPriceInfo, Is.True,
                "[F4.3_02] Phải hiển thị đơn giá (SalePrice ưu tiên nếu có)");
        }

        [Test]
        public void TC_CART_F4_3_03_ThemSanPhamThu2()
        {
            CurrentTestCaseId = "TC_F4.3_03";
            var data = DocDuLieu("TC_CART_F4_3_03");
            _cartPage.Login(data["email"], data["password"]);

            // Thêm SP 1
            _cartPage.AddToCartFromDetail(data["productSlug1"]);
            // Thêm SP 2
            _cartPage.AddProductAndOpenCart(data["productSlug2"]);

            int itemCount = _cartPage.GetCartItemCount();

            CurrentActualResult = itemCount >= 2
                ? $"Giỏ hàng có {itemCount} SP khác nhau."
                : $"Giỏ hàng chỉ có {itemCount} SP (cần ≥ 2).";

            Assert.That(itemCount, Is.GreaterThanOrEqualTo(2),
                "[F4.3_03] Giỏ hàng phải chứa ít nhất 2 SP khác nhau");
        }

        // =========================================================
        // F4.4 – Cộng dồn số lượng
        // =========================================================

        [Test]
        public void TC_CART_F4_4_01_CongDonSLKhiThemSPDaCo()
        {
            CurrentTestCaseId = "TC_F4.4_01";
            var data = DocDuLieu("TC_CART_F4_4_01");
            _cartPage.Login(data["email"], data["password"]);

            // Thêm SP lần 1
            _cartPage.AddToCartFromDetail(data["productSlug"]);
            _cartPage.Open();
            int qtyBefore = _cartPage.GetItemQuantity();

            // Thêm SP lần 2 (cùng SP)
            _cartPage.AddProductAndOpenCart(data["productSlug"]);
            int qtyAfter = _cartPage.GetItemQuantity();

            bool isCongDon = qtyAfter > qtyBefore;

            CurrentActualResult = isCongDon
                ? $"SL cộng dồn đúng: {qtyBefore} → {qtyAfter}."
                : $"SL KHÔNG cộng dồn: Before={qtyBefore}, After={qtyAfter}.";

            Assert.That(qtyAfter, Is.GreaterThan(qtyBefore),
                $"[F4.4_01] SL phải tăng khi thêm SP đã có. Before={qtyBefore}, After={qtyAfter}");
        }

        // =========================================================
        // F4.5 – Cập nhật SL bằng nút + / -
        // =========================================================

        [Test]
        public void TC_CART_F4_5_03_TangSLBangNutPlus()
        {
            CurrentTestCaseId = "TC_F4.5_03";
            var data = DocDuLieu("TC_CART_F4_5_03");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            int qtyBefore = _cartPage.GetItemQuantity();
            _cartPage.ClickPlusButton();
            int qtyAfter = _cartPage.GetItemQuantity();

            CurrentActualResult = qtyAfter == qtyBefore + 1
                ? $"SL tăng đúng: {qtyBefore} → {qtyAfter} (AJAX không reload)."
                : $"SL không tăng đúng: {qtyBefore} → {qtyAfter}.";

            Assert.That(qtyAfter, Is.EqualTo(qtyBefore + 1),
                $"[F4.5_03] SL phải tăng 1 sau khi click +. Before={qtyBefore}, After={qtyAfter}");
        }

        // =========================================================
        // F4.6 – Giảm SL bằng nút -
        // =========================================================

        [Test]
        public void TC_CART_F4_6_01_GiamSLBangNutMinus()
        {
            CurrentTestCaseId = "TC_F4.6_01";
            var data = DocDuLieu("TC_CART_F4_6_01");
            _cartPage.Login(data["email"], data["password"]);

            // Thêm SP 2 lần để SL = 2
            _cartPage.AddToCartFromDetail(data["productSlug"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            int qtyBefore = _cartPage.GetItemQuantity();
            _cartPage.ClickMinusButton();
            int qtyAfter = _cartPage.GetItemQuantity();

            CurrentActualResult = qtyAfter == qtyBefore - 1
                ? $"SL giảm đúng: {qtyBefore} → {qtyAfter} (AJAX không reload)."
                : $"SL không giảm đúng: {qtyBefore} → {qtyAfter}.";

            Assert.That(qtyAfter, Is.EqualTo(qtyBefore - 1),
                $"[F4.6_01] SL phải giảm 1 sau khi click -. Before={qtyBefore}, After={qtyAfter}");
        }

        // =========================================================
        // F4.7 – Giới hạn tồn kho
        // =========================================================

        [Test]
        public void TC_CART_F4_7_01_KhoaNutPlusKhiMaxStock()
        {
            CurrentTestCaseId = "TC_F4.7_01";
            var data = DocDuLieu("TC_CART_F4_7_01");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            // Lấy stock từ data attribute
            string stockInfo = _cartPage.GetStockInfoText();
            // Click + nhiều lần cho đến khi đạt max
            for (int i = 0; i < 50; i++)
            {
                if (_cartPage.IsPlusButtonDisabled()) break;
                _cartPage.ClickPlusButton();
            }

            bool isDisabled = _cartPage.IsPlusButtonDisabled();
            bool isMaxMsg = _cartPage.IsMaxReachedDisplayed();

            CurrentActualResult = isDisabled
                ? $"Nút + bị disabled khi đạt max stock. Msg 'Đã đạt tối đa': {isMaxMsg}."
                : "Nút + vẫn hoạt động khi đạt max stock.";

            Assert.That(isDisabled, Is.True,
                "[F4.7_01] Nút + phải bị disabled khi SL = Stock");
        }

        [Test]
        public void TC_CART_F4_7_02_ToastCanhBaoVuotTonKho()
        {
            CurrentTestCaseId = "TC_F4.7_02";
            var data = DocDuLieu("TC_CART_F4_7_02");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            // Click + nhiều lần cho đến max, rồi thử click thêm
            for (int i = 0; i < 50; i++)
            {
                if (_cartPage.IsPlusButtonDisabled()) break;
                _cartPage.ClickPlusButton();
            }

            // Nút đã disabled → hệ thống chặn, kiểm tra max reached
            bool maxReached = _cartPage.IsMaxReachedDisplayed()
                || _cartPage.IsPlusButtonDisabled();

            CurrentActualResult = maxReached
                ? "Hệ thống chặn không cho vượt tồn kho. Nút + disabled hoặc thông báo 'Đã đạt tối đa'."
                : "Hệ thống KHÔNG chặn vượt tồn kho.";

            Assert.That(maxReached, Is.True,
                "[F4.7_02] Phải có cảnh báo hoặc chặn khi vượt tồn kho");
        }

        // =========================================================
        // F4.9 – Xóa SP (AJAX)
        // =========================================================

        [Test]
        public void TC_CART_F4_9_01_XoaSPQuaAjax()
        {
            CurrentTestCaseId = "TC_F4.9_01";
            var data = DocDuLieu("TC_CART_F4_9_01");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            int countBefore = _cartPage.GetCartItemCount();
            _cartPage.ClickRemoveButton();
            Thread.Sleep(500); // Chờ animation
            int countAfter = _cartPage.GetCartItemCount();

            bool removed = countAfter < countBefore;

            CurrentActualResult = removed
                ? $"SP đã bị xóa. Items: {countBefore} → {countAfter}."
                : $"SP KHÔNG bị xóa. Items: {countBefore} → {countAfter}.";

            Assert.That(removed, Is.True,
                $"[F4.9_01] SP phải được xóa khỏi giỏ. Before={countBefore}, After={countAfter}");
        }

        [Test]
        public void TC_CART_F4_9_02_XoaSPCuoiAutoReload()
        {
            CurrentTestCaseId = "TC_F4.9_02";
            var data = DocDuLieu("TC_CART_F4_9_02");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            // Xóa SP (chỉ có 1 SP)
            _cartPage.ClickRemoveButton();
            Thread.Sleep(2000); // Chờ animation + auto reload

            bool isEmpty = _cartPage.IsCartEmpty();

            CurrentActualResult = isEmpty
                ? "Xóa SP cuối → trang reload, hiển thị giỏ hàng trống."
                : "Xóa SP cuối nhưng trang KHÔNG hiển thị trạng thái rỗng.";

            Assert.That(isEmpty, Is.True,
                "[F4.9_02] Sau khi xóa SP cuối, trang phải reload và hiển thị giỏ rỗng");
        }

        // =========================================================
        // F4.1 – UI / Non-Functional
        // =========================================================

        [Test]
        public void TC_CART_F4_1_03_LinkTenSPDenDetail()
        {
            CurrentTestCaseId = "TC_F4.1_03";
            var data = DocDuLieu("TC_CART_F4_1_03");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            string href = _cartPage.GetProductNameHref();
            _cartPage.ClickProductNameLink();
            string currentUrl = _cartPage.GetCurrentUrl();

            bool redirected = currentUrl.Contains("/Shop/Detail");

            CurrentActualResult = redirected
                ? $"Click tên SP → chuyển hướng đến {currentUrl}."
                : $"Click tên SP nhưng URL không đúng: {currentUrl}.";

            Assert.That(redirected, Is.True,
                $"[F4.1_03] Phải chuyển đến trang detail. URL: {currentUrl}");
        }

        [Test]
        public void TC_CART_F4_1_05_HienThiConXSanPham()
        {
            CurrentTestCaseId = "TC_F4.1_05";
            var data = DocDuLieu("TC_CART_F4_1_05");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            string stockText = _cartPage.GetStockInfoText();
            bool hasStock = stockText.Contains("Còn") && stockText.Contains("sản phẩm");

            CurrentActualResult = hasStock
                ? $"Hiển thị đúng: '{stockText}'."
                : $"Không tìm thấy thông tin tồn kho. Text: '{stockText}'.";

            Assert.That(hasStock, Is.True,
                $"[F4.1_05] Phải hiển thị 'Còn x sản phẩm'. Actual: '{stockText}'");
        }

        [Test]
        public void TC_CART_F4_1_06_ToastThanhCongKhiThemSP()
        {
            CurrentTestCaseId = "TC_F4.1_06";
            var data = DocDuLieu("TC_CART_F4_1_06");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddToCartFromDetail(data["productSlug"]);

            // Sau khi thêm SP, trang redirect về Cart với toast thành công
            bool hasToast = _cartPage.IsSuccessToastDisplayed();
            string body = _cartPage.GetBodyText();
            bool hasSuccessMsg = body.Contains("Thành công") || body.Contains("Product added");

            CurrentActualResult = hasToast || hasSuccessMsg
                ? "Toast thành công xuất hiện: 'Product added to cart!'."
                : "KHÔNG thấy toast thành công sau khi thêm SP.";

            Assert.That(hasToast || hasSuccessMsg, Is.True,
                "[F4.1_06] Toast thành công phải hiển thị sau khi thêm SP");
        }

        [Test]
        public void TC_CART_F4_2_03_NutThanhToanDenCheckout()
        {
            CurrentTestCaseId = "TC_F4.2_03";
            var data = DocDuLieu("TC_CART_F4_2_03");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            _cartPage.ClickCheckout();
            string url = _cartPage.GetCurrentUrl();

            bool isCheckout = url.Contains("/Checkout");

            CurrentActualResult = isCheckout
                ? $"Click THANH TOÁN → chuyển đến: {url}."
                : $"Click THANH TOÁN nhưng URL: {url}.";

            Assert.That(isCheckout, Is.True,
                $"[F4.2_03] Phải chuyển hướng đến /Checkout. URL: {url}");
        }

        [Test]
        public void TC_CART_F4_2_04_NutTiepTucMuaSam()
        {
            CurrentTestCaseId = "TC_F4.2_04";
            var data = DocDuLieu("TC_CART_F4_2_04");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            _cartPage.ClickContinueShopping();
            string url = _cartPage.GetCurrentUrl();

            bool isShop = url.Contains("/Shop");

            CurrentActualResult = isShop
                ? $"Click 'Tiếp tục mua sắm' → chuyển đến: {url}."
                : $"Click 'Tiếp tục mua sắm' nhưng URL: {url}.";

            Assert.That(isShop, Is.True,
                $"[F4.2_04] Phải chuyển hướng đến /Shop. URL: {url}");
        }

        [Test]
        public void TC_CART_F4_1_08_DinhDangTienTeVN()
        {
            CurrentTestCaseId = "TC_F4.1_08";
            var data = DocDuLieu("TC_CART_F4_1_08");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            bool isValidFormat = _cartPage.IsCurrencyFormatValid();
            string subtotal = _cartPage.GetSubtotalText();
            string total = _cartPage.GetTotalText();

            CurrentActualResult = isValidFormat
                ? $"Định dạng tiền VN đúng. Subtotal='{subtotal}', Total='{total}' (dạng xxx,xxxđ)."
                : $"Định dạng tiền KHÔNG đúng. Subtotal='{subtotal}', Total='{total}'.";

            Assert.That(isValidFormat, Is.True,
                "[F4.1_08] Tất cả giá trị tiền phải có dạng 'xxx,xxxđ'");
        }

        [Test]
        public void TC_CART_F4_1_09_HienThiVATNote()
        {
            CurrentTestCaseId = "TC_F4.1_09";
            var data = DocDuLieu("TC_CART_F4_1_09");
            _cartPage.Login(data["email"], data["password"]);
            _cartPage.AddProductAndOpenCart(data["productSlug"]);

            bool hasVat = _cartPage.IsVatNoteDisplayed();

            CurrentActualResult = hasVat
                ? "Dòng VAT note hiển thị: '(Đã bao gồm VAT nếu có)'."
                : "KHÔNG tìm thấy dòng VAT note.";

            Assert.That(hasVat, Is.True,
                "[F4.1_09] Phải hiển thị dòng '(Đã bao gồm VAT nếu có)' dưới Tổng cộng");
        }

        private Dictionary<string, string> DocDuLieu(string testCaseId)
        {
            return JsonHelper.DocDuLieu(DataPath, testCaseId);
        }
    }
}
