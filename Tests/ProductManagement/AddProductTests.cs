using NUnit.Framework;
using SeleniumProject.Pages.ProductManagement;
using SeleniumProject.Utilities;

namespace SeleniumProject.Tests.ProductManagement
{
    [TestFixture]
    public class AddProductTests : TestBase
    {
        private CreateProductPage _createPage = null!;

        // Đường dẫn file test data sản phẩm
        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "ProductManagement", "product_data.json"
        );

        [SetUp]
        public void SetUpPages()
        {
            // Đăng nhập admin (dùng chung từ TestBase)
            LoginAsAdmin();

            // Mở trang thêm sản phẩm
            _createPage = new CreateProductPage(Driver, BaseUrl);
            _createPage.Open();
        }

        // TC_F2.5_01 — Thêm sản phẩm thành công với đầy đủ thông tin hợp lệ
        [Test]
        public void TC_F2_5_01_ThemSanPhamDayDuThongTin()
        {
            // Đọc dữ liệu
            CurrentTestCaseId = "TC_F2.5_01";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.5_01");

            // Điền form
            _createPage.EnterName(data["productName"]);
            _createPage.EnterShortDescription(data["shortDescription"]);
            _createPage.SelectCategory(data["category"]);
            _createPage.EnterPrice(data["price"]);
            _createPage.EnterSalePrice(data["discountPrice"]);
            _createPage.EnterStock(data["stock"]);

            // Upload ảnh — bỏ qua nếu file chưa có trong TestData/Images/
            string resolvedPath = FileHelper.ResolveImagePath(data["imagePath"]);
            if (resolvedPath != null && File.Exists(resolvedPath))
            {
                _createPage.UploadImage(resolvedPath);
            }

            // Lưu
            _createPage.ClickSave();

            // Đọc kết quả thực tế từ DOM: URL + toast + lỗi validation
            CurrentActualResult = _createPage.DocKetQuaThucTe();

            // Kiểm tra kết quả: redirect về danh sách sản phẩm
            bool daRedirect = _createPage.IsRedirectedToList();
            Assert.That(
                daRedirect,
                Is.True,
                "Kỳ vọng: redirect về /Admin/Product sau khi lưu thành công"
            );
        }

        // TC_F2.5_02 — Thêm sản phẩm thành công chỉ với các trường bắt buộc
        [Test]
        public void TC_F2_5_02_ThemSanPhamChiTruongBatBuoc()
        {
            // Đọc dữ liệu
            CurrentTestCaseId = "TC_F2.5_02";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.5_02");

            // Chỉ điền các trường bắt buộc: Tên, Danh mục, Giá gốc
            _createPage.EnterName(data["productName"]);
            _createPage.SelectCategory(data["category"]);
            _createPage.EnterPrice(data["price"]);

            // Lưu (không điền ảnh, không discount, không stock)
            _createPage.ClickSave();

            // Đọc kết quả thực tế từ DOM
            CurrentActualResult = _createPage.DocKetQuaThucTe();

            // Kiểm tra kết quả: redirect về danh sách sản phẩm
            bool daRedirect02 = _createPage.IsRedirectedToList();
            Assert.That(
                daRedirect02,
                Is.True,
                "Kỳ vọng: redirect về /Admin/Product sau khi lưu với trường tối thiểu"
            );
        }

        // TC_F2.5_09 — Để trống Tên sản phẩm → phải hiện lỗi validation
        [Test]
        public void TC_F2_5_09_TenSanPhamTrong()
        {
            // Đọc dữ liệu
            CurrentTestCaseId = "TC_F2.5_09";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.5_09");

            // Bỏ qua tên sản phẩm (trống), điền các trường còn lại
            _createPage.SelectCategory(data["category"]);
            _createPage.EnterPrice(data["price"]);

            // Bấm lưu dù tên đang trống
            _createPage.ClickSave();

            // Đọc kết quả thực tế từ DOM
            CurrentActualResult = _createPage.DocKetQuaThucTe();

            // Kiểm tra 1: vẫn ở trang Create (không redirect đi)
            bool vanOrTrang09 = !_createPage.IsRedirectedToList();
            Assert.That(
                vanOrTrang09,
                Is.True,
                "Kỳ vọng: không redirect khi tên sản phẩm bị trống"
            );

            // Kiểm tra 2: có thông báo lỗi validation hiển thị
            bool coLoiHienThi09 = _createPage.HasValidationErrors();
            Assert.That(
                coLoiHienThi09,
                Is.True,
                "Kỳ vọng: hiển thị lỗi validation khi để trống tên sản phẩm"
            );
        }

        // TC_F2.5_03 — Slug tự động sinh từ tên sản phẩm khi để trống
        [Test]
        public void TC_F2_5_03_SlugTuDongSinh()
        {
            CurrentTestCaseId = "TC_F2.5_03";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.5_03");

            _createPage.EnterName(data["productName"]);
            _createPage.SelectCategory(data["category"]);
            _createPage.EnterPrice(data["price"]);

            // Lưu sản phẩm — server sẽ tự sinh slug từ tên
            _createPage.ClickSave();

            // Đọc kết quả thực tế từ DOM
            CurrentActualResult = _createPage.DocKetQuaThucTe();

            // Kiểm tra: form lưu thành công (slug trống không gây lỗi)
            bool daRedirect03 = _createPage.IsRedirectedToList();
            Assert.That(
                daRedirect03,
                Is.True,
                "Kỳ vọng: lưu thành công khi để trống slug (server tự sinh)"
            );
        }

        // TC_F2.5_04 — Nhập Slug tùy chỉnh hợp lệ
        [Test]
        public void TC_F2_5_04_SlugTuyChinhHopLe()
        {
            CurrentTestCaseId = "TC_F2.5_04";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.5_04");

            _createPage.EnterName(data["productName"]);
            _createPage.EnterSlug(data["slug"]);
            _createPage.SelectCategory(data["category"]);
            _createPage.EnterPrice(data["price"]);

            _createPage.ClickSave();

            // Đọc kết quả thực tế từ DOM
            CurrentActualResult = _createPage.DocKetQuaThucTe();

            bool daRedirect04 = _createPage.IsRedirectedToList();
            Assert.That(
                daRedirect04,
                Is.True,
                "Kỳ vọng: lưu thành công với slug tùy chỉnh hợp lệ"
            );
        }

        // TC_F2.5_05 — Thêm Tags và hiển thị dạng badge ngay trên form
        [Test]
        public void TC_F2_5_05_TagsBadge()
        {
            CurrentTestCaseId = "TC_F2.5_05";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.5_05");

            _createPage.EnterName(data["productName"]);
            _createPage.SelectCategory(data["category"]);
            _createPage.EnterPrice(data["price"]);

            // Thêm tags — mỗi tag nhập rồi nhấn Enter → hiện badge
            _createPage.AddTags(data["tags"]);

            // Kiểm tra badge xuất hiện ngay trên form trước khi lưu
            int soBadge = _createPage.GetTagCount();
            CurrentActualResult = soBadge > 0
                ? $"Tags hiển thị {soBadge} badge ngay sau khi nhập"
                : "Tags không hiển thị badge sau khi nhập";

            Assert.That(
                soBadge,
                Is.GreaterThan(0),
                "Kỳ vọng: tag hiển thị dạng badge ngay sau khi nhập"
            );

            // Lưu và đọc kết quả thực tế sau khi submit
            _createPage.ClickSave();
            CurrentActualResult = soBadge > 0
                ? _createPage.DocKetQuaThucTe() + $" | Badge đã hiển thị: {soBadge}"
                : _createPage.DocKetQuaThucTe();

            Assert.That(
                _createPage.IsRedirectedToList(),
                Is.True,
                "Kỳ vọng: lưu thành công sau khi thêm tags"
            );
        }

        // TC_F2.5_06 — Upload nhiều ảnh
        // [Skip] Bỏ qua — cần file ảnh thật trong TestData/Images/
        // Sẽ bổ sung khi có ảnh test: valid_small.jpg, valid_small.png, valid_small.webp

        // TC_F2.5_07 — Live preview giá VND cập nhật ngay khi nhập
        [Test]
        public void TC_F2_5_07_LivePreviewGia()
        {
            CurrentTestCaseId = "TC_F2.5_07";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.5_07");

            // Nhập giá → live preview phải cập nhật ngay
            _createPage.EnterPrice(data["price"]);

            string expectedPreview = data["expectedPreviewPrice"];
            string actualPreview   = _createPage.GetPricePreviewText();

            // Ghi kết quả thực tế quan sát từ trình duyệt
            CurrentActualResult = string.IsNullOrEmpty(actualPreview)
                ? "Không tìm thấy element live preview giá"
                : $"Live preview hiển thị: '{actualPreview}'";

            // TODO: Nếu test fail vì selector sai, kiểm tra lại PricePreviewText trong CreateProductPage.cs
            Assert.That(
                actualPreview,
                Does.Contain(expectedPreview),
                $"Kỳ vọng: live preview hiển thị '{expectedPreview}' sau khi nhập giá"
            );
        }

        // TC_F2.5_14 — Upload file không phải ảnh (PDF)
        // [Skip] Bỏ qua — cần file TestData/Images/invalid_format.pdf

        // TC_F2.5_15 — Upload ảnh vượt 5MB
        // [Skip] Bỏ qua — cần tích hợp FileHelper.GenerateTempFile để sinh file lớn

        // TC_F2.5_16 — Truy cập khi chưa đăng nhập
        // [Skip] Bỏ qua — cần TestClass riêng không gọi LoginAsAdmin() trong SetUp

        // TC_F2.5_17 — Tài khoản Customer truy cập Admin
        // [Skip] Bỏ qua — cần thêm CustomerEmail/CustomerPassword vào appsettings.json

        // ==============================================================
        // Nhóm: Lỗi Validation khi lưu
        // Gồm TC_F2.5_08, 10, 11, 12, 13, 19 — cùng pattern:
        //   fill (có/không) → ClickSave → vẫn ở trang, hiển thị lỗi
        // ==============================================================
        private static IEnumerable<TestCaseData> LoiValidationKhiLuu()
        {
            yield return new TestCaseData("TC_F2.5_08")
                .SetName("TC_F2.5_08 - Submit form rỗng hoàn toàn");

            yield return new TestCaseData("TC_F2.5_10")
                .SetName("TC_F2.5_10 - Không chọn Danh mục");

            yield return new TestCaseData("TC_F2.5_11")
                .SetName("TC_F2.5_11 - Để trống Giá gốc");

            yield return new TestCaseData("TC_F2.5_12")
                .SetName("TC_F2.5_12 - Giá khuyến mãi lớn hơn Giá gốc");

            yield return new TestCaseData("TC_F2.5_13")
                .SetName("TC_F2.5_13 - Nhập Giá gốc âm");

            yield return new TestCaseData("TC_F2.5_19")
                .SetName("TC_F2.5_19 - Tên sản phẩm vượt max length");
        }

        [TestCaseSource(nameof(LoiValidationKhiLuu))]
        public void TC_KiemTraLoiValidation(string testCaseId)
        {
            // Gán để TearDown ghi kết quả đúng hàng trong Excel
            CurrentTestCaseId = testCaseId;

            var data = JsonHelper.DocDuLieu(DataPath, testCaseId);

            // Điền các trường nếu có giá trị — để trống nếu test yêu cầu bỏ trống
            if (!string.IsNullOrEmpty(data["productName"]))
            {
                _createPage.EnterName(data["productName"]);
            }

            if (!string.IsNullOrEmpty(data["category"]))
            {
                _createPage.SelectCategory(data["category"]);
            }

            if (!string.IsNullOrEmpty(data["price"]))
            {
                _createPage.EnterPrice(data["price"]);
            }

            if (!string.IsNullOrEmpty(data["discountPrice"]))
            {
                _createPage.EnterSalePrice(data["discountPrice"]);
            }

            // Bấm lưu dù form chưa đủ điều kiện
            _createPage.ClickSave();

            // Đọc kết quả thực tế từ DOM
            CurrentActualResult = _createPage.DocKetQuaThucTe();

            // Kiểm tra 1: không redirect (vẫn ở trang Create)
            bool vanOrTrangN = !_createPage.IsRedirectedToList();
            Assert.That(
                vanOrTrangN,
                Is.True,
                $"Kỳ vọng [{testCaseId}]: không redirect khi có lỗi validation"
            );

            // Kiểm tra 2: có lỗi validation hiển thị trên form
            bool coLoiHienThiN = _createPage.HasValidationErrors();
            Assert.That(
                coLoiHienThiN,
                Is.True,
                $"Kỳ vọng [{testCaseId}]: hiển thị thông báo lỗi trên form"
            );
        }

        // ==============================================================
        // Nhóm: Giá trị biên — thành công
        // Gồm TC_F2.5_18 (tên 1 ký tự) và TC_F2.5_20 (giá = 1)
        // ==============================================================
        private static IEnumerable<TestCaseData> BienGioiThanhCong()
        {
            yield return new TestCaseData("TC_F2.5_18")
                .SetName("TC_F2.5_18 - Tên sản phẩm 1 ký tự (min)");

            yield return new TestCaseData("TC_F2.5_20")
                .SetName("TC_F2.5_20 - Giá gốc = 1 (min hợp lệ)");
        }

        [TestCaseSource(nameof(BienGioiThanhCong))]
        public void TC_ThemSanPhamBienGioi_ThanhCong(string testCaseId)
        {
            // Gán để TearDown ghi kết quả đúng hàng trong Excel
            CurrentTestCaseId = testCaseId;

            var data = JsonHelper.DocDuLieu(DataPath, testCaseId);

            _createPage.EnterName(data["productName"]);
            _createPage.SelectCategory(data["category"]);
            _createPage.EnterPrice(data["price"]);

            _createPage.ClickSave();

            // Đọc kết quả thực tế từ DOM
            CurrentActualResult = _createPage.DocKetQuaThucTe();

            bool daRedirectBien = _createPage.IsRedirectedToList();
            Assert.That(
                daRedirectBien,
                Is.True,
                $"Kỳ vọng [{testCaseId}]: lưu thành công tại giá trị biên"
            );
        }
    }
}
