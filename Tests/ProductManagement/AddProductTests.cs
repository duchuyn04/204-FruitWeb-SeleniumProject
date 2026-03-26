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
            // Khai báo sheet Excel tương ứng với module này
            // TearDown dùng để tìm đúng hàng và ghi kết quả
            CurrentSheetName = "TC_Product Management";

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

        // TC_F2.5_06 — Upload nhiều ảnh và ảnh đầu tiên là ảnh chính
        [Test]
        public void TC_F2_5_06_UploadNhieuAnh()
        {
            CurrentTestCaseId = "TC_F2.5_06";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.5_06");

            _createPage.EnterName(data["productName"]);
            _createPage.SelectCategory(data["category"]);
            _createPage.EnterPrice(data["price"]);

            // Giải các đường dẫn phân cách bằng dấu phẩy → đầy đủ absolute path
            string[] fileNames = data["imagePath"].Split(',');
            string resolvedPaths = string.Join(",",
                fileNames.Select(f => FileHelper.ResolveImagePath(f.Trim()))
                         .Where(p => p != null && File.Exists(p)));

            _createPage.UploadImages(resolvedPaths);
            _createPage.ClickSave();

            CurrentActualResult = _createPage.DocKetQuaThucTe();

            Assert.That(
                _createPage.IsRedirectedToList(),
                Is.True,
                "Kỳ vọng: lưu thành công khi upload nhiều ảnh hợp lệ"
            );
        }

        // TC_F2.5_14 — Upload file không phải ảnh (PDF) → server từ chối hoặc lỗi
        [Test]
        public void TC_F2_5_14_UploadFilePDF()
        {
            CurrentTestCaseId = "TC_F2.5_14";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.5_14");

            _createPage.EnterName(data["productName"]);
            _createPage.SelectCategory(data["category"]);
            _createPage.EnterPrice(data["price"]);

            string pdfPath = FileHelper.ResolveImagePath(data["imagePath"]);

            // Upload file PDF qua input
            if (pdfPath != null && File.Exists(pdfPath))
            {
                _createPage.UploadImage(pdfPath);
            }

            _createPage.ClickSave();

            CurrentActualResult = _createPage.DocKetQuaThucTe();

            // Kỳ vọng: không redirect (lỗi upload/validation), HOẶC redirect (server chấp nhận nhưng bỏ qua)
            // → kiểm tra: vẫn còn ở trang Create hoặc có lỗi hiển thị
            bool bi_loi = !_createPage.IsRedirectedToList() || _createPage.HasValidationErrors();
            Assert.That(
                bi_loi,
                Is.True,
                "Kỳ vọng: server từ chối hoặc báo lỗi khi upload file PDF"
            );
        }

        // TC_F2.5_15 — Upload ảnh vượt 5MB → server từ chối
        [Test]
        public void TC_F2_5_15_UploadAnhVuot5MB()
        {
            CurrentTestCaseId = "TC_F2.5_15";
            var data = JsonHelper.DocDuLieu(DataPath, "TC_F2.5_15");

            _createPage.EnterName(data["productName"]);
            _createPage.SelectCategory(data["category"]);
            _createPage.EnterPrice(data["price"]);

            // generate:5242981 → sinh file tạm >5MB
            string largePath = FileHelper.ResolveImagePath(data["imagePath"]);

            try
            {
                if (largePath != null && File.Exists(largePath))
                {
                    _createPage.UploadImage(largePath);
                }

                _createPage.ClickSave();

                CurrentActualResult = _createPage.DocKetQuaThucTe();

                bool bi_loi = !_createPage.IsRedirectedToList() || _createPage.HasValidationErrors();
                Assert.That(
                    bi_loi,
                    Is.True,
                    "Kỳ vọng: server từ chối hoặc báo lỗi khi upload file vượt 5MB"
                );
            }
            finally
            {
                // Dọn file tạm dù test pass hay fail
                FileHelper.DeleteTempFile(largePath);
            }
        }


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
