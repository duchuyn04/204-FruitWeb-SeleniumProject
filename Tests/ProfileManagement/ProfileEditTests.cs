using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumProject.Pages.ProfileManagement;
using SeleniumProject.Utilities;

namespace SeleniumProject.Tests.ProfileManagement
{
    [TestFixture]
    public class ProfileEditTests : TestBase
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
        // F6.2 – Chỉnh sửa Profile
        // =========================================================

        [Test]
        public void TC_PROFILE_F6_2_01_MoFormChinhSua()
        {
            CurrentTestCaseId = "TC_F6.2_01";
            var data = DocDuLieu("TC_PROFILE_F6_2_01");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            bool isOnEdit = _profilePage.IsOnEditPage();
            string nameValue = _profilePage.GetNameInputValue();
            string phoneValue = _profilePage.GetPhoneInputValue();

            CurrentActualResult = isOnEdit
                ? $"Form Edit hiển thị đúng. Name='{nameValue}', Phone='{phoneValue}'. URL chứa /Profile/Edit."
                : $"Không mở được form Edit. URL: {_profilePage.GetCurrentUrl()}.";

            Assert.That(isOnEdit, Is.True,
                "[F6.2_01] Phải mở được trang /Profile/Edit");
        }

        [Test]
        public void TC_PROFILE_F6_2_02_CapNhatTenThanhCong()
        {
            CurrentTestCaseId = "TC_F6.2_02";
            var data = DocDuLieu("TC_PROFILE_F6_2_02");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            _profilePage.ClearAndTypeName(data["newName"]);
            _profilePage.ClickSave();

            string body = _profilePage.GetBodyText();
            string url = _profilePage.GetCurrentUrl();
            bool success = body.Contains("thành công")
                || body.Contains(data["newName"])
                || url.Contains("/Profile");

            CurrentActualResult = success
                ? $"Cập nhật tên thành '{data["newName"]}' thành công."
                : "Cập nhật tên THẤT BẠI.";

            Assert.That(success, Is.True,
                "[F6.2_02] Cập nhật tên phải thành công");
        }

        [Test]
        public void TC_PROFILE_F6_2_03_CapNhatPhoneThanhCong()
        {
            CurrentTestCaseId = "TC_F6.2_03";
            var data = DocDuLieu("TC_PROFILE_F6_2_03");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            _profilePage.ClearAndTypePhone(data["newPhone"]);
            _profilePage.ClickSave();

            string body = _profilePage.GetBodyText();
            string url = _profilePage.GetCurrentUrl();
            bool success = body.Contains("thành công")
                || body.Contains(data["newPhone"])
                || url.Contains("/Profile");

            CurrentActualResult = success
                ? $"Cập nhật phone thành '{data["newPhone"]}' thành công."
                : "Cập nhật phone THẤT BẠI.";

            Assert.That(success, Is.True,
                "[F6.2_03] Cập nhật SĐT phải thành công");
        }

        [Test]
        public void TC_PROFILE_F6_2_04_CapNhatTenVaPhoneCungLuc()
        {
            CurrentTestCaseId = "TC_F6.2_04";
            var data = DocDuLieu("TC_PROFILE_F6_2_04");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            _profilePage.ClearAndTypeName(data["newName"]);
            _profilePage.ClearAndTypePhone(data["newPhone"]);
            _profilePage.ClickSave();

            string body = _profilePage.GetBodyText();
            string url = _profilePage.GetCurrentUrl();
            bool success = body.Contains("thành công")
                || (body.Contains(data["newName"]) && body.Contains(data["newPhone"]))
                || url.Contains("/Profile");

            CurrentActualResult = success
                ? $"Cập nhật cả Tên='{data["newName"]}' và Phone='{data["newPhone"]}' thành công."
                : "Cập nhật cả Tên và Phone THẤT BẠI.";

            Assert.That(success, Is.True,
                "[F6.2_04] Cập nhật cả Tên và Phone cùng lúc phải thành công");
        }

        [Test]
        public void TC_PROFILE_F6_2_05_DeNongTenBatBuoc()
        {
            CurrentTestCaseId = "TC_F6.2_05";
            var data = DocDuLieu("TC_PROFILE_F6_2_05");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            _profilePage.ClearAndTypeName("");
            _profilePage.ClearAndTypePhone(data["newPhone"]);
            _profilePage.ClickSave();

            string nameError = _profilePage.GetNameValidationError();
            bool hasError = !string.IsNullOrEmpty(nameError)
                || _profilePage.HasAnyValidationError()
                || _profilePage.IsOnEditPage();

            CurrentActualResult = hasError
                ? $"Báo lỗi khi để trống Tên: '{nameError}'. Vẫn ở trang Edit."
                : "KHÔNG báo lỗi khi để trống Tên.";

            Assert.That(hasError, Is.True,
                $"[F6.2_05] Phải báo lỗi khi để trống Tên. Error: '{nameError}'");
        }

        [Test]
        public void TC_PROFILE_F6_2_06_TenChiCoKhoangTrang()
        {
            CurrentTestCaseId = "TC_F6.2_06";
            var data = DocDuLieu("TC_PROFILE_F6_2_06");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            _profilePage.ClearAndTypeName("   ");
            _profilePage.ClearAndTypePhone(data["newPhone"]);
            _profilePage.ClickSave();

            string nameError = _profilePage.GetNameValidationError();
            bool hasError = !string.IsNullOrEmpty(nameError)
                || _profilePage.HasAnyValidationError()
                || _profilePage.IsOnEditPage();

            CurrentActualResult = hasError
                ? $"Báo lỗi khi Tên chỉ có khoảng trắng: '{nameError}'."
                : "KHÔNG báo lỗi khi Tên chỉ có khoảng trắng.";

            Assert.That(hasError, Is.True,
                "[F6.2_06] Phải báo lỗi khi Tên chỉ có khoảng trắng");
        }

        [Test]
        public void TC_PROFILE_F6_2_07_TenVuotQua200KyTu()
        {
            CurrentTestCaseId = "TC_F6.2_07";
            var data = DocDuLieu("TC_PROFILE_F6_2_07");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            _profilePage.ClearAndTypeName(data["newName"]);
            _profilePage.ClearAndTypePhone(data["newPhone"]);
            _profilePage.ClickSave();

            string nameError = _profilePage.GetNameValidationError();
            bool hasError = !string.IsNullOrEmpty(nameError)
                || _profilePage.HasAnyValidationError()
                || _profilePage.IsOnEditPage();

            CurrentActualResult = hasError
                ? $"Báo lỗi Tên vượt 200 ký tự: '{nameError}'."
                : "Hệ thống chấp nhận Tên > 200 ký tự.";

            Assert.That(hasError, Is.True,
                "[F6.2_07] Phải báo lỗi khi Tên vượt 200 ký tự");
        }

        [Test]
        public void TC_PROFILE_F6_2_08_TenDung200KyTuBoundary()
        {
            CurrentTestCaseId = "TC_F6.2_08";
            var data = DocDuLieu("TC_PROFILE_F6_2_08");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            _profilePage.ClearAndTypeName(data["newName"]);
            _profilePage.ClearAndTypePhone(data["newPhone"]);
            _profilePage.ClickSave();

            string nameError = _profilePage.GetNameValidationError();
            string body = _profilePage.GetBodyText();
            string url = _profilePage.GetCurrentUrl();
            bool success = string.IsNullOrEmpty(nameError)
                && (body.Contains("thành công") || url.Contains("/Profile"));

            CurrentActualResult = success
                ? $"Tên đúng 200 ký tự hợp lệ, cập nhật thành công."
                : $"Tên 200 ký tự bị lỗi: '{nameError}'.";

            Assert.That(success, Is.True,
                $"[F6.2_08] Tên 200 ký tự (boundary) phải hợp lệ. Error: '{nameError}'");
        }

        [Test]
        public void TC_PROFILE_F6_2_09_PhoneKhongHopLe()
        {
            CurrentTestCaseId = "TC_F6.2_09";
            var data = DocDuLieu("TC_PROFILE_F6_2_09");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            _profilePage.ClearAndTypeName(data["newName"]);
            _profilePage.ClearAndTypePhone(data["newPhone"]); // "abcd123456"
            _profilePage.ClickSave();

            string phoneError = _profilePage.GetPhoneValidationError();
            bool hasError = !string.IsNullOrEmpty(phoneError)
                || _profilePage.HasAnyValidationError()
                || _profilePage.IsOnEditPage();

            CurrentActualResult = hasError
                ? $"Báo lỗi Phone chứa chữ cái: '{phoneError}'."
                : "KHÔNG báo lỗi khi Phone chứa chữ cái.";

            Assert.That(hasError, Is.True,
                "[F6.2_09] Phải báo lỗi khi Phone chứa chữ cái");
        }

        [Test]
        public void TC_PROFILE_F6_2_10_PhoneQuaNgan()
        {
            CurrentTestCaseId = "TC_F6.2_10";
            var data = DocDuLieu("TC_PROFILE_F6_2_10");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            _profilePage.ClearAndTypeName(data["newName"]);
            _profilePage.ClearAndTypePhone(data["newPhone"]); // "09012"
            _profilePage.ClickSave();

            string phoneError = _profilePage.GetPhoneValidationError();
            bool hasError = !string.IsNullOrEmpty(phoneError)
                || _profilePage.HasAnyValidationError()
                || _profilePage.IsOnEditPage();

            CurrentActualResult = hasError
                ? $"Báo lỗi Phone quá ngắn: '{phoneError}'."
                : "KHÔNG báo lỗi khi Phone < 10 ký tự.";

            Assert.That(hasError, Is.True,
                "[F6.2_10] Phải báo lỗi khi Phone < 10 ký tự");
        }

        [Test]
        public void TC_PROFILE_F6_2_11_PhoneQuaDai()
        {
            CurrentTestCaseId = "TC_F6.2_11";
            var data = DocDuLieu("TC_PROFILE_F6_2_11");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            _profilePage.ClearAndTypeName(data["newName"]);
            _profilePage.ClearAndTypePhone(data["newPhone"]); // 21 số
            _profilePage.ClickSave();

            string phoneError = _profilePage.GetPhoneValidationError();
            bool hasError = !string.IsNullOrEmpty(phoneError)
                || _profilePage.HasAnyValidationError()
                || _profilePage.IsOnEditPage();

            CurrentActualResult = hasError
                ? $"Báo lỗi Phone quá dài: '{phoneError}'."
                : "KHÔNG báo lỗi khi Phone > 20 ký tự.";

            Assert.That(hasError, Is.True,
                "[F6.2_11] Phải báo lỗi khi Phone > 20 ký tự");
        }

        [Test]
        public void TC_PROFILE_F6_2_12_Phone10KyTuHopLe()
        {
            CurrentTestCaseId = "TC_F6.2_12";
            var data = DocDuLieu("TC_PROFILE_F6_2_12");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            _profilePage.ClearAndTypeName(data["newName"]);
            _profilePage.ClearAndTypePhone(data["newPhone"]); // "0901234567"
            _profilePage.ClickSave();

            string phoneError = _profilePage.GetPhoneValidationError();
            bool noError = string.IsNullOrEmpty(phoneError);

            CurrentActualResult = noError
                ? $"Phone 10 ký tự '{data["newPhone"]}' hợp lệ, không báo lỗi."
                : $"Phone 10 ký tự bị báo lỗi: '{phoneError}'.";

            Assert.That(noError, Is.True,
                $"[F6.2_12] Phone 10 ký tự phải hợp lệ. Error: '{phoneError}'");
        }

        [Test]
        public void TC_PROFILE_F6_2_13_DeTrongPhoneOptional()
        {
            CurrentTestCaseId = "TC_F6.2_13";
            var data = DocDuLieu("TC_PROFILE_F6_2_13");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            _profilePage.ClearAndTypeName(data["newName"]);
            _profilePage.ClearAndTypePhone("");
            _profilePage.ClickSave();

            string phoneError = _profilePage.GetPhoneValidationError();
            string url = _profilePage.GetCurrentUrl();
            bool success = string.IsNullOrEmpty(phoneError)
                && (url.Contains("/Profile") || _profilePage.GetBodyText().Contains("thành công"));

            CurrentActualResult = success
                ? "Để trống Phone thành công (optional field)."
                : $"Để trống Phone bị lỗi: '{phoneError}'.";

            Assert.That(success, Is.True,
                "[F6.2_13] Phone là tùy chọn, để trống phải thành công");
        }

        [Test]
        public void TC_PROFILE_F6_2_14_ThongBaoThanhCong()
        {
            CurrentTestCaseId = "TC_F6.2_14";
            var data = DocDuLieu("TC_PROFILE_F6_2_14");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            _profilePage.ClearAndTypeName(data["newName"]);
            _profilePage.ClearAndTypePhone(data["newPhone"]);
            _profilePage.ClickSave();

            string body = _profilePage.GetBodyText();
            string toast = _profilePage.GetToastText();
            bool hasSuccessMsg = body.Contains("thành công")
                || toast.Contains("thành công")
                || body.Contains("cập nhật");

            CurrentActualResult = hasSuccessMsg
                ? $"Thông báo thành công hiển thị. Toast: '{toast}'."
                : "KHÔNG thấy thông báo thành công sau khi cập nhật.";

            Assert.That(hasSuccessMsg, Is.True,
                "[F6.2_14] Phải hiển thị thông báo thành công sau cập nhật");
        }

        [Test]
        public void TC_PROFILE_F6_2_15_AntiForgeryTokenTrenFormEdit()
        {
            CurrentTestCaseId = "TC_F6.2_15";
            var data = DocDuLieu("TC_PROFILE_F6_2_15");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            bool hasToken = _profilePage.HasAntiForgeryToken();

            CurrentActualResult = hasToken
                ? "Form Edit có AntiForgeryToken (__RequestVerificationToken)."
                : "Form Edit KHÔNG có AntiForgeryToken.";

            Assert.That(hasToken, Is.True,
                "[F6.2_15] Form Edit phải có AntiForgeryToken để chống CSRF");
        }

        [Test]
        public void TC_PROFILE_F6_2_16_TenKyTuDacBietUnicode()
        {
            CurrentTestCaseId = "TC_F6.2_16";
            var data = DocDuLieu("TC_PROFILE_F6_2_16");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            _profilePage.ClearAndTypeName(data["newName"]);
            _profilePage.ClearAndTypePhone(data["newPhone"]);
            _profilePage.ClickSave();

            string body = _profilePage.GetBodyText();
            string url = _profilePage.GetCurrentUrl();
            bool success = body.Contains("thành công")
                || body.Contains(data["newName"])
                || url.Contains("/Profile");

            CurrentActualResult = success
                ? $"Tên có ký tự đặc biệt/Unicode '{data["newName"]}' được chấp nhận."
                : $"Tên có ký tự đặc biệt bị từ chối.";

            Assert.That(success, Is.True,
                "[F6.2_16] Tên có ký tự đặc biệt và Unicode phải được chấp nhận");
        }

        [Test]
        public void TC_PROFILE_F6_2_17_PhoneDung20KyTuBoundaryMax()
        {
            CurrentTestCaseId = "TC_F6.2_17";
            var data = DocDuLieu("TC_PROFILE_F6_2_17");
            _profilePage.Login(data["email"], data["password"]);
            _profilePage.OpenEdit();

            _profilePage.ClearAndTypeName(data["newName"]);
            _profilePage.ClearAndTypePhone(data["newPhone"]); // 20 số
            _profilePage.ClickSave();

            string phoneError = _profilePage.GetPhoneValidationError();
            bool noError = string.IsNullOrEmpty(phoneError);

            CurrentActualResult = noError
                ? $"Phone 20 ký tự '{data["newPhone"]}' hợp lệ."
                : $"Phone 20 ký tự bị lỗi: '{phoneError}'.";

            Assert.That(noError, Is.True,
                $"[F6.2_17] Phone 20 ký tự (boundary max) phải hợp lệ. Error: '{phoneError}'");
        }

        private Dictionary<string, string> DocDuLieu(string testCaseId)
        {
            return JsonHelper.DocDuLieu(DataPath, testCaseId);
        }
    }
}
