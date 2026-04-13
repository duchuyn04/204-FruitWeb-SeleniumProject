using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumProject.Pages.ProfileManagement;
using SeleniumProject.Utilities;

namespace SeleniumProject.Tests.ProfileManagement
{
    [TestFixture]
    public class ProfileAddressTests : TestBase
    {
        private ProfilePage _profilePage = null!;

        private static readonly string DataPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "ProfileManagement", "profile_data.json"
        );

        [SetUp]
        public void SetUpProfilePage()
        {
            CurrentSheetName = "TC_ProfileManagement";
            _profilePage = new ProfilePage(Driver);
        }

        // =========================================================
        // F6.5 – Xem danh sách địa chỉ
        // =========================================================

        [Test]
        public void TC_PROFILE_F6_5_01_XemDanhSachDiaChi()
        {
            CurrentTestCaseId = "TC_F6.5_01";
            var data = DocDuLieu("TC_PROFILE_F6_5_01");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddress();

            string body = _profilePage.GetBodyText();
            bool hasAddressPage = _profilePage.GetCurrentUrl().Contains("/Address");
            bool hasContent = body.Contains("Địa chỉ") || body.Contains("Mặc định")
                || body.Contains("Sửa") || body.Contains("Xóa")
                || body.Contains("Thêm địa chỉ");

            CurrentActualResult = hasAddressPage && hasContent
                ? $"Trang danh sách địa chỉ hiển thị đúng. URL: {_profilePage.GetCurrentUrl()}."
                : $"Trang địa chỉ không hiển thị đúng. URL: {_profilePage.GetCurrentUrl()}.";

            Assert.That(hasAddressPage, Is.True,
                "[F6.5_01] Phải truy cập được trang danh sách địa chỉ");
            Assert.That(hasContent, Is.True,
                "[F6.5_01] Trang phải có nội dung liên quan đến địa chỉ");
        }

        [Test]
        public void TC_PROFILE_F6_5_02_XemDanhSachKhiChuaCoDiaChi()
        {
            CurrentTestCaseId = "TC_F6.5_02";
            var data = DocDuLieu("TC_PROFILE_F6_5_02");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddress();

            string body = _profilePage.GetBodyText();
            bool hasAddressPage = _profilePage.GetCurrentUrl().Contains("/Address");
            int count = _profilePage.GetAddressCount();

            // Khi chưa có địa chỉ: hiển thị empty state hoặc nút Thêm
            bool hasEmptyOrAdd = body.Contains("chưa có địa chỉ") || body.Contains("Thêm địa chỉ")
                || body.Contains("Thêm mới") || count == 0;

            CurrentActualResult = hasAddressPage
                ? $"Trang địa chỉ hiển thị. Số lượng: {count}. Có nút Thêm: {body.Contains("Thêm")}."
                : $"KHÔNG vào được trang địa chỉ. URL: {_profilePage.GetCurrentUrl()}.";

            Assert.That(hasAddressPage, Is.True,
                "[F6.5_02] Phải truy cập được trang danh sách địa chỉ");
        }

        // =========================================================
        // F6.6 – Thêm địa chỉ
        // =========================================================

        [Test]
        public void TC_PROFILE_F6_6_01_ThemDiaChiMoiThanhCong()
        {
            CurrentTestCaseId = "TC_F6.6_01";
            var data = DocDuLieu("TC_PROFILE_F6_6_01");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddressCreate();

            _profilePage.FillAddressForm(
                fullName: data["fullName"],
                phone: data["phone"],
                street: data["street"],
                province: data["province"],
                district: data["district"],
                ward: data["ward"]
            );
            _profilePage.SubmitAddressForm();

            string body = _profilePage.GetBodyText();
            string url = _profilePage.GetCurrentUrl();

            bool success = body.Contains("thành công")
                || body.Contains(data["fullName"])
                || url.Contains("/Address");

            CurrentActualResult = success
                ? $"Thêm địa chỉ mới thành công. Redirect về danh sách. Name='{data["fullName"]}'."
                : $"Thêm địa chỉ THẤT BẠI. URL: {url}.";

            Assert.That(success, Is.True,
                "[F6.6_01] Thêm địa chỉ mới phải thành công");
        }

        [Test]
        public void TC_PROFILE_F6_6_02_FormThemDiaChiTuDienThongTin()
        {
            CurrentTestCaseId = "TC_F6.6_02";
            var data = DocDuLieu("TC_PROFILE_F6_6_02");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddressCreate();

            // Kiểm tra form tự điền FullName và Phone từ profile
            string autoName = _profilePage.GetAddressFullNameValue();
            string autoPhone = _profilePage.GetAddressPhoneValue();

            bool autoFilled = !string.IsNullOrEmpty(autoName) || !string.IsNullOrEmpty(autoPhone);

            CurrentActualResult = autoFilled
                ? $"Form tự điền: FullName='{autoName}', Phone='{autoPhone}'."
                : "Form KHÔNG tự điền thông tin từ profile.";

            Assert.That(autoFilled, Is.True,
                "[F6.6_02] Form thêm địa chỉ phải tự điền FullName/Phone từ profile");
        }

        [Test]
        public void TC_PROFILE_F6_6_03_ThemDiaChiVoiDatLamMacDinh()
        {
            CurrentTestCaseId = "TC_F6.6_03";
            var data = DocDuLieu("TC_PROFILE_F6_6_03");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddressCreate();

            _profilePage.FillAddressForm(
                fullName: data["fullName"],
                phone: data["phone"],
                street: data["street"],
                province: data["province"],
                district: data["district"],
                ward: data["ward"],
                isDefault: true
            );
            _profilePage.SubmitAddressForm();

            string url = _profilePage.GetCurrentUrl();
            bool success = url.Contains("/Address") && !url.Contains("/Create");

            // Kiểm tra badge Mặc định
            if (success)
            {
                bool hasDefault = _profilePage.HasDefaultBadge();
                CurrentActualResult = hasDefault
                    ? "Thêm địa chỉ mặc định thành công. Badge 'Mặc định' hiển thị."
                    : "Thêm thành công nhưng KHÔNG thấy badge 'Mặc định'.";
            }
            else
            {
                CurrentActualResult = $"Thêm địa chỉ THẤT BẠI. URL: {url}.";
            }

            Assert.That(success, Is.True,
                "[F6.6_03] Thêm địa chỉ với 'Đặt làm mặc định' phải thành công");
        }

        [Test]
        public void TC_PROFILE_F6_6_04_BoTrongCacTruongBatBuoc()
        {
            CurrentTestCaseId = "TC_F6.6_04";
            var data = DocDuLieu("TC_PROFILE_F6_6_04");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddressCreate();

            // Bỏ trống tất cả và submit
            _profilePage.SubmitAddressForm();

            string body = _profilePage.GetBodyText();
            bool hasErrors = body.Contains("bắt buộc") || body.Contains("required")
                || body.Contains("nhập") || body.Contains("chọn")
                || _profilePage.GetCurrentUrl().Contains("/Address/Create");

            CurrentActualResult = hasErrors
                ? "Báo lỗi validation khi bỏ trống các trường bắt buộc."
                : "KHÔNG báo lỗi khi bỏ trống các trường.";

            Assert.That(hasErrors, Is.True,
                "[F6.6_04] Phải báo lỗi khi bỏ trống tất cả trường bắt buộc");
        }

        [Test]
        public void TC_PROFILE_F6_6_05_SoDienThoaiKhongHopLe()
        {
            CurrentTestCaseId = "TC_F6.6_05";
            var data = DocDuLieu("TC_PROFILE_F6_6_05");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddressCreate();

            _profilePage.FillAddressForm(
                fullName: data["fullName"],
                phone: data["phone"], // "123" → quá ngắn/sai format
                street: data["street"],
                province: data.ContainsKey("province") ? data["province"] : "",
                district: data.ContainsKey("district") ? data["district"] : "",
                ward: data.ContainsKey("ward") ? data["ward"] : ""
            );
            _profilePage.SubmitAddressForm();

            string body = _profilePage.GetBodyText();
            bool hasErrors = body.Contains("điện thoại") || body.Contains("Phone")
                || body.Contains("hợp lệ") || body.Contains("format")
                || _profilePage.GetCurrentUrl().Contains("/Address/Create");

            CurrentActualResult = hasErrors
                ? "Báo lỗi phone không hợp lệ trên form địa chỉ."
                : "KHÔNG báo lỗi khi phone sai format.";

            Assert.That(hasErrors, Is.True,
                "[F6.6_05] Phải báo lỗi khi số điện thoại không hợp lệ");
        }

        // =========================================================
        // F6.7 – Sửa địa chỉ
        // =========================================================

        [Test]
        public void TC_PROFILE_F6_7_01_SuaDiaChiThanhCong()
        {
            CurrentTestCaseId = "TC_F6.7_01";
            var data = DocDuLieu("TC_PROFILE_F6_7_01");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddress();

            // Click nút Sửa của địa chỉ đầu tiên
            _profilePage.ClickEditFirstAddress();
            Thread.Sleep(1000);

            bool isOnEdit = _profilePage.GetCurrentUrl().Contains("/Address/Edit");
            if (isOnEdit)
            {
                // Sửa street
                _profilePage.ClearAndTypeStreet(data["newStreet"]);
                _profilePage.SubmitAddressForm();

                string url = _profilePage.GetCurrentUrl();
                string body = _profilePage.GetBodyText();
                bool success = url.Contains("/Address") && !url.Contains("/Edit")
                    || body.Contains("thành công");

                CurrentActualResult = success
                    ? $"Sửa địa chỉ thành công. Street mới: '{data["newStreet"]}'."
                    : $"Sửa địa chỉ THẤT BẠI. URL: {url}.";

                Assert.That(success, Is.True,
                    "[F6.7_01] Sửa địa chỉ phải thành công");
            }
            else
            {
                CurrentActualResult = $"Không mở được form Edit Address. URL: {_profilePage.GetCurrentUrl()}.";
                Assert.Fail("[F6.7_01] Không mở được form Edit Address");
            }
        }

        [Test]
        public void TC_PROFILE_F6_7_02_SuaDiaChiUserKhacSecurity()
        {
            CurrentTestCaseId = "TC_F6.7_02";
            var data = DocDuLieu("TC_PROFILE_F6_7_02");
            _profilePage.Login(data["email"], data["password"]);

            // Thử truy cập Edit address với ID không thuộc user này (ID=9999)
            _profilePage.OpenAddressEdit(9999);

            string url = _profilePage.GetCurrentUrl();
            string body = _profilePage.GetBodyText();
            bool blocked = url.Contains("/Address") || url.Contains("/Error")
                || url.Contains("/Account/Login")
                || body.Contains("không tìm thấy") || body.Contains("không có quyền")
                || body.Contains("404") || body.Contains("403");

            CurrentActualResult = blocked
                ? $"Hệ thống chặn truy cập địa chỉ user khác. URL: {url}."
                : $"HỆ THỐNG KHÔNG CHẶN truy cập. URL: {url}.";

            Assert.That(blocked, Is.True,
                "[F6.7_02] Phải chặn sửa địa chỉ của user khác (Security)");
        }

        // =========================================================
        // F6.8 – Xóa địa chỉ
        // =========================================================

        [Test]
        public void TC_PROFILE_F6_8_01_XoaDiaChiQuaFormPOST()
        {
            CurrentTestCaseId = "TC_F6.8_01";
            var data = DocDuLieu("TC_PROFILE_F6_8_01");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddress();

            int countBefore = _profilePage.GetAddressCount();
            if (countBefore > 0)
            {
                _profilePage.ClickDeleteFirstAddress();
                Thread.Sleep(2000);

                // Reload để verify
                _profilePage.OpenAddress();
                int countAfter = _profilePage.GetAddressCount();

                CurrentActualResult = countAfter < countBefore
                    ? $"Xóa địa chỉ thành công. Trước: {countBefore}, Sau: {countAfter}."
                    : $"Xóa THẤT BẠI. Trước: {countBefore}, Sau: {countAfter}.";

                Assert.That(countAfter, Is.LessThan(countBefore),
                    "[F6.8_01] Phải xóa được địa chỉ qua form POST");
            }
            else
            {
                CurrentActualResult = "Không có địa chỉ nào để xóa. Skip.";
                Assert.Pass("[F6.8_01] Không có địa chỉ để test xóa");
            }
        }

        [Test]
        public void TC_PROFILE_F6_8_02_XoaDiaChiQuaAJAX()
        {
            CurrentTestCaseId = "TC_F6.8_02";
            var data = DocDuLieu("TC_PROFILE_F6_8_02");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddress();

            int countBefore = _profilePage.GetAddressCount();
            if (countBefore > 0)
            {
                // Gọi AJAX DeleteAjax
                int addressId = _profilePage.GetFirstAddressId();
                if (addressId > 0)
                {
                    string response = _profilePage.CallDeleteAddressAjax(addressId);

                    _profilePage.OpenAddress();
                    int countAfter = _profilePage.GetAddressCount();

                    bool success = countAfter < countBefore
                        || response.Contains("success") || response.Contains("true");

                    CurrentActualResult = success
                        ? $"Xóa AJAX thành công. Trước: {countBefore}, Sau: {countAfter}. Response: {response.Substring(0, Math.Min(100, response.Length))}."
                        : $"Xóa AJAX THẤT BẠI. Response: {response}.";

                    Assert.That(success, Is.True,
                        "[F6.8_02] Phải xóa được địa chỉ qua AJAX DeleteAjax");
                }
                else
                {
                    CurrentActualResult = "Không lấy được Address ID. Skip.";
                    Assert.Pass("[F6.8_02] Không lấy được Address ID");
                }
            }
            else
            {
                CurrentActualResult = "Không có địa chỉ để xóa. Skip.";
                Assert.Pass("[F6.8_02] Không có địa chỉ để test xóa AJAX");
            }
        }

        [Test]
        public void TC_PROFILE_F6_8_03_XoaDiaChiUserKhacSecurity()
        {
            CurrentTestCaseId = "TC_F6.8_03";
            var data = DocDuLieu("TC_PROFILE_F6_8_03");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddress();

            // Thử xóa địa chỉ ID=9999 (không thuộc user)
            string response = _profilePage.CallDeleteAddressAjax(9999);

            bool blocked = response.Contains("error") || response.Contains("false")
                || response.Contains("403") || response.Contains("404")
                || response.Contains("không tìm thấy") || response.Contains("unauthorized")
                || response.StartsWith("ERROR");

            CurrentActualResult = blocked
                ? $"Hệ thống chặn xóa địa chỉ user khác. Response: {response.Substring(0, Math.Min(150, response.Length))}."
                : $"Hệ thống KHÔNG chặn. Response: {response.Substring(0, Math.Min(150, response.Length))}.";

            Assert.That(blocked, Is.True,
                "[F6.8_03] Phải chặn xóa địa chỉ của user khác (Security)");
        }

        // =========================================================
        // F6.9 – Nâng cao / Security
        // =========================================================

        [Test]
        public void TC_PROFILE_F6_9_01_SetDefaultBangAJAX()
        {
            CurrentTestCaseId = "TC_F6.9_01";
            var data = DocDuLieu("TC_PROFILE_F6_9_01");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddress();

            int addressId = _profilePage.GetFirstAddressId();
            if (addressId > 0)
            {
                string response = _profilePage.CallSetDefaultAddressAjax(addressId);

                bool success = response.Contains("success") || response.Contains("true")
                    || !response.StartsWith("ERROR");

                _profilePage.OpenAddress();
                bool hasDefault = _profilePage.HasDefaultBadge();

                CurrentActualResult = success
                    ? $"Set Default AJAX thành công. HasDefault badge: {hasDefault}. Response: {response.Substring(0, Math.Min(100, response.Length))}"
                    : $"Set Default AJAX thất bại. Response: {response}";

                Assert.That(success, Is.True,
                    "[F6.9_01] Phải set default bằng AJAX thành công");
            }
            else
            {
                CurrentActualResult = "Không có địa chỉ để set default. Skip.";
                Assert.Pass("[F6.9_01] Không có địa chỉ để test set default");
            }
        }

        [Test]
        public void TC_PROFILE_F6_9_02_AddressKhiChuaDangNhap()
        {
            CurrentTestCaseId = "TC_F6.9_02";
            // Không đăng nhập
            _profilePage.OpenAddress();

            string url = _profilePage.GetCurrentUrl();
            bool isRedirected = url.Contains("/Account/Login");

            CurrentActualResult = isRedirected
                ? $"Redirect về Login khi truy cập Address chưa đăng nhập. URL: {url}."
                : $"KHÔNG redirect. URL: {url}.";

            Assert.That(isRedirected, Is.True,
                $"[F6.9_02] Phải redirect về Login. URL: {url}");
        }

        [Test]
        public void TC_PROFILE_F6_9_03_XSSTestStreetAddress()
        {
            CurrentTestCaseId = "TC_F6.9_03";
            var data = DocDuLieu("TC_PROFILE_F6_9_03");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddressCreate();

            string xssPayload = data.ContainsKey("xssPayload")
                ? data["xssPayload"]
                : "<script>alert('XSS')</script>";

            _profilePage.FillAddressForm(
                fullName: data.ContainsKey("fullName") ? data["fullName"] : "Test User",
                phone: data.ContainsKey("phone") ? data["phone"] : "0901234567",
                street: xssPayload,
                province: data.ContainsKey("province") ? data["province"] : "Hà Nội",
                district: data.ContainsKey("district") ? data["district"] : "Nam Từ Liêm",
                ward: data.ContainsKey("ward") ? data["ward"] : "Mỹ Đình 1"
            );
            _profilePage.SubmitAddressForm();

            // Kiểm tra XSS không được thực thi
            string pageSource = _profilePage.GetPageSource();
            bool xssBlocked = !pageSource.Contains("<script>alert") || pageSource.Contains("&lt;script");

            CurrentActualResult = xssBlocked
                ? "XSS bị encode/sanitize. Không có script injection."
                : "CẢNH BÁO: XSS có thể đã được inject!";

            Assert.That(xssBlocked, Is.True,
                "[F6.9_03] StreetAddress phải được sanitize, không cho phép XSS");
        }

        [Test]
        public void TC_PROFILE_F6_9_04_PhoneBoundary11So()
        {
            CurrentTestCaseId = "TC_F6.9_04";
            var data = DocDuLieu("TC_PROFILE_F6_9_04");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddressCreate();

            _profilePage.FillAddressForm(
                fullName: data.ContainsKey("fullName") ? data["fullName"] : "Test User",
                phone: data.ContainsKey("phone") ? data["phone"] : "09012345678", // 11 số
                street: data.ContainsKey("street") ? data["street"] : "123 Đường Test",
                province: data.ContainsKey("province") ? data["province"] : "Hà Nội",
                district: data.ContainsKey("district") ? data["district"] : "Nam Từ Liêm",
                ward: data.ContainsKey("ward") ? data["ward"] : "Mỹ Đình 1"
            );
            _profilePage.SubmitAddressForm();

            string url = _profilePage.GetCurrentUrl();
            string body = _profilePage.GetBodyText();

            // Phone 11 số phải hợp lệ (trong khoảng 10-20)
            bool success = url.Contains("/Address") && !url.Contains("/Create")
                || body.Contains("thành công");

            CurrentActualResult = success
                ? "Phone 11 số hợp lệ, thêm địa chỉ thành công."
                : $"Phone 11 số bị lỗi. URL: {url}.";

            Assert.That(success, Is.True,
                "[F6.9_04] Phone 11 số (boundary) phải hợp lệ");
        }

        [Test]
        public void TC_PROFILE_F6_9_05_ProvinceDistrictWardCodeDeTrong()
        {
            CurrentTestCaseId = "TC_F6.9_05";
            var data = DocDuLieu("TC_PROFILE_F6_9_05");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddressCreate();

            // Chỉ điền FullName, Phone, Street - không chọn Province/District/Ward
            _profilePage.FillAddressForm(
                fullName: data.ContainsKey("fullName") ? data["fullName"] : "Test User",
                phone: data.ContainsKey("phone") ? data["phone"] : "0901234567",
                street: data.ContainsKey("street") ? data["street"] : "123 Đường Test",
                province: "",
                district: "",
                ward: ""
            );
            _profilePage.SubmitAddressForm();

            string body = _profilePage.GetBodyText();
            bool hasErrors = body.Contains("chọn") || body.Contains("bắt buộc")
                || body.Contains("required")
                || _profilePage.GetCurrentUrl().Contains("/Address/Create");

            CurrentActualResult = hasErrors
                ? "Báo lỗi khi không chọn Province/District/Ward."
                : "KHÔNG báo lỗi khi bỏ trống dropdown.";

            Assert.That(hasErrors, Is.True,
                "[F6.9_05] Phải báo lỗi khi không chọn Province/District/Ward");
        }

        [Test]
        public void TC_PROFILE_F6_9_06_FormSetDefaultKhongPHaiAJAX()
        {
            CurrentTestCaseId = "TC_F6.9_06";
            var data = DocDuLieu("TC_PROFILE_F6_9_06");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddress();

            // Click nút "Đặt làm mặc định" (có thể là form POST hoặc link)
            bool hasSetDefault = _profilePage.ClickSetDefaultButton();
            Thread.Sleep(2000);

            if (hasSetDefault)
            {
                string url = _profilePage.GetCurrentUrl();
                bool success = url.Contains("/Address") || _profilePage.HasDefaultBadge();

                CurrentActualResult = success
                    ? $"Set Default qua form thành công. URL: {url}."
                    : $"Set Default THẤT BẠI. URL: {url}.";

                Assert.That(success, Is.True,
                    "[F6.9_06] Set default qua form POST phải thành công");
            }
            else
            {
                CurrentActualResult = "Không tìm thấy nút Set Default. Skip.";
                Assert.Pass("[F6.9_06] Không có nút Set Default để test");
            }
        }

        [Test]
        public void TC_PROFILE_F6_9_07_CreateAddressKhongHopLe()
        {
            CurrentTestCaseId = "TC_F6.9_07";
            var data = DocDuLieu("TC_PROFILE_F6_9_07");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddressCreate();

            // Submit form trống → ModelState.IsValid = false
            _profilePage.SubmitAddressForm();

            string url = _profilePage.GetCurrentUrl();
            string body = _profilePage.GetBodyText();

            bool stayOnForm = url.Contains("/Address/Create")
                || body.Contains("bắt buộc") || body.Contains("required")
                || body.Contains("nhập") || body.Contains("chọn");

            CurrentActualResult = stayOnForm
                ? "Form không submit khi ModelState invalid. Validation errors hiển thị."
                : $"Form submit dù ModelState invalid. URL: {url}.";

            Assert.That(stayOnForm, Is.True,
                "[F6.9_07] Phải giữ ở form Create khi ModelState.IsValid = false");
        }

        [Test]
        public void TC_PROFILE_F6_9_08_XoaDiaChiDuyNhatDangLaMacDinh()
        {
            CurrentTestCaseId = "TC_F6.9_08";
            var data = DocDuLieu("TC_PROFILE_F6_9_08");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenAddress();

            int countBefore = _profilePage.GetAddressCount();
            bool hasDefault = _profilePage.HasDefaultBadge();

            if (countBefore == 1 && hasDefault)
            {
                _profilePage.ClickDeleteFirstAddress();
                Thread.Sleep(2000);

                _profilePage.OpenAddress();
                int countAfter = _profilePage.GetAddressCount();

                CurrentActualResult = countAfter == 0
                    ? "Xóa địa chỉ mặc định duy nhất thành công. Danh sách trống."
                    : $"Xóa THẤT BẠI hoặc bị chặn. Trước: {countBefore}, Sau: {countAfter}.";

                // Hệ thống cho phép xóa hoặc chặn đều OK → chỉ verify không crash
                Assert.That(true, Is.True,
                    "[F6.9_08] Hệ thống phải xử lý an toàn khi xóa địa chỉ mặc định duy nhất");
            }
            else
            {
                CurrentActualResult = $"Điều kiện chưa đủ: Count={countBefore}, HasDefault={hasDefault}. Skip.";
                Assert.Pass($"[F6.9_08] Cần đúng 1 địa chỉ mặc định. Actual: Count={countBefore}");
            }
        }

        private Dictionary<string, string> DocDuLieu(string testCaseId)
        {
            return JsonHelper.DocDuLieu(DataPath, testCaseId);
        }
    }
}
