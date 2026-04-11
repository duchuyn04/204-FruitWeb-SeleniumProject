# HƯỚNG DẪN VIẾT TEST CASE AUTOMATION CHUẨN MỰC (SELENIUM POM)

Tài liệu này đúc kết phương pháp và kinh nghiệm từ toàn bộ các module đã xây dựng.  
Áp dụng bắt buộc cho **MỌI module chức năng mới** (Checkout, OrderManagement, Product, User, v.v…).

---

## 1. BA NGUYÊN TẮC CỐT LÕI — KHÔNG ĐƯỢC VI PHẠM

| # | Nguyên tắc | Đúng ✅ | Sai ❌ |
|---|---|---|---|
| 1 | **POM (Page Object Model)** | Test gọi `_page.ClickSave()` | Test gọi `Driver.FindElement(By.Id("btn"))` |
| 2 | **Data-Driven** | Dữ liệu lấy từ JSON qua `data["key"]` | Dữ liệu cứng trong code `"Nguyễn Văn A"` |
| 3 | **Báo cáo trung thực** | Luôn gán `CurrentActualResult` trước `Assert` | Để `Assert` chặn trước khi gán |

> **Quy tắc vàng:** Nếu trong file `Tests/XxxTests.cs` có chữ `By.`, `FindElement`, `FindElements`, hoặc chuỗi literal là địa chỉ/số điện thoại/tên người/URL cứng → **đó là vi phạm, phải sửa.**

---

## 2. CẤU TRÚC FILE — 3 TẦNG BẮT BUỘC

```
SeleniumProject/
├── Pages/
│   └── XxxPage.cs          ← Tầng 1: Toàn bộ locator + hành động + query
├── TestData/
│   └── Xxx/
│       └── xxx_data.json   ← Tầng 2: Toàn bộ dữ liệu test
└── Tests/
    └── XxxTests.cs         ← Tầng 3: Chỉ gọi Page Object + Assert
```

---

## 3. TẦNG 1 — PAGE OBJECT (`Pages/XxxPage.cs`)

### 3.1 Cấu trúc chuẩn

```csharp
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumProject.Utilities;

namespace SeleniumProject.Pages
{
    public class XxxPage
    {
        private readonly IWebDriver _driver;
        private readonly WaitHelper _wait;

        // ===== CONSTANTS (URL nội bộ đặt ở đây) =====
        private const string BaseUrl  = "http://localhost:5270";
        private const string XxxUrl   = BaseUrl + "/Xxx";

        // ===== LOCATORS (PRIVATE – test không bao giờ thấy) =====
        //   → Dùng By.Id khi có id rõ ràng
        //   → Dùng By.CssSelector cho class/attribute
        //   → Dùng By.XPath chỉ khi không có id/css ổn định
        private By NameInput     => By.Id("NameInput");
        private By SaveButton    => By.CssSelector("button[type='submit']");
        private By ErrorMessage  => By.CssSelector(".alert-danger, .field-validation-error");

        public XxxPage(IWebDriver driver)
        {
            _driver = driver;
            _wait   = new WaitHelper(driver);
        }

        // ===== NAVIGATION =====
        public void NavigateTo() => _driver.Navigate().GoToUrl(XxxUrl);

        // ===== ACTIONS (PUBLIC – test gọi vào đây) =====
        public void EnterName(string name) => _wait.SlowType(NameInput, name);
        public void ClickSave()            => _wait.WaitForClickable(SaveButton).Click();

        // ===== QUERIES (PUBLIC – trả về thông tin, test dùng để Assert) =====
        public bool IsErrorDisplayed()  => _driver.FindElements(ErrorMessage).Count > 0;
        public string GetCurrentUrl()   => _driver.Url;
        public string GetPageBodyText()
        {
            try { return _driver.FindElement(By.TagName("body")).Text; }
            catch { return string.Empty; }
        }
    }
}
```

### 3.2 Quy tắc đặt tên method

| Loại | Prefix | Ví dụ |
|------|--------|-------|
| Điều hướng | `NavigateTo...` | `NavigateToCheckout()` |
| Nhập liệu | `Enter...` | `EnterFullName(string v)` |
| Chọn dropdown | `Select...` | `SelectProvince(string v)` |
| Click nút | `Click...` | `ClickPlaceOrder()` |
| Kiểm tra bool | `Is...` | `IsOnConfirmationPage()` |
| Kiểm tra bool | `Has...` | `HasSavedAddressDropdown()` |
| Đếm | `Get...Count()` | `GetDistrictOptionCount()` |
| Lấy text | `Get...Text()` | `GetPageBodyText()` |
| Tick/toggle | `Tick...` | `TickSaveAddressIfPresent()` |

### 3.3 Wrapper cho từng field validation (BẮT BUỘC khi test kiểm tra lỗi nhập liệu)

```csharp
// ❌ SAI — test biết element ID
// bool invalid = Driver.FindElement(By.Id("NameInput"))
//                      .GetAttribute("class").Contains("is-invalid");

// ✅ ĐÚNG — đóng gói trong Page Object
private By NameInput => By.Id("NameInput");

public bool IsFieldInvalid(By locator)
{
    try
    {
        var cls = _driver.FindElement(locator).GetAttribute("class") ?? "";
        return cls.Contains("is-invalid") || cls.Contains("input-validation-error");
    }
    catch { return false; }
}

public bool IsNameInvalid()  => IsFieldInvalid(NameInput);
public bool IsEmailInvalid() => IsFieldInvalid(EmailInput);
```

### 3.4 Wrapper cho dropdown state

```csharp
// Đếm option (kể cả placeholder)
public int GetDistrictOptionCount()
    => _driver.FindElements(By.CssSelector("#districtSelect option")).Count;

// Kiểm tra dropdown rỗng/disabled
public bool IsDistrictDropdownEmpty()
{
    try
    {
        var el = _driver.FindElement(DistrictSelect);
        return !el.Enabled || el.GetAttribute("disabled") != null
               || GetDistrictOptionCount() <= 1;
    }
    catch { return true; }
}

// Kiểm tra đã reset về placeholder
public bool IsDistrictResetToPlaceholder()
{
    try
    {
        var sel = new SelectElement(_driver.FindElement(DistrictSelect));
        return sel.SelectedOption.GetAttribute("value") == ""
               || sel.SelectedOption.Text.Contains("Chọn");
    }
    catch { return false; }
}
```

---

## 4. TẦNG 2 — TEST DATA (`TestData/Xxx/xxx_data.json`)

### 4.1 Cấu trúc chuẩn

```json
[
  {
    "testCase": "TC_XXX_F1_01",
    "testCaseId": "F1.01",
    "description": "Mô tả ngắn gọn test case là gì",
    "email": "user@example.com",
    "password": "password123",
    "productUrl": "http://localhost:5270/Shop/Detail/ten-san-pham",
    "checkoutUrl": "http://localhost:5270/Checkout",
    "fullName": "Nguyễn Văn A",
    "phone": "0901234567",
    "streetAddress": "123 Đường ABC",
    "province": "Thành phố Hồ Chí Minh",
    "district": "Quận 1",
    "ward": "Phường Bến Nghé"
  },
  {
    "testCase": "TC_XXX_F1_02",
    "testCaseId": "F1.02",
    "description": "Test case tiếp theo",
    "email": "user@example.com",
    "password": "password123",
    "fullName": "",
    "phone": "0901234567"
  }
]
```

### 4.2 Quy tắc dữ liệu

| Quy tắc | Lý do |
|---------|-------|
| **Mỗi test case một entry riêng** | Dữ liệu độc lập, dễ debug |
| **URL luôn để trong JSON, không hardcode trong C#** | Một chỗ đổi, tất cả test cập nhật |
| **Chuỗi rỗng `""` là dữ liệu hợp lệ** | Test validation bắt buộc nhập |
| **Dùng key đặt tên rõ ràng** | `province` thay vì `field3` |
| **Dữ liệu địa danh (tỉnh/quận/phường) trong JSON** | Không hardcode `"Thành phố Hà Nội"` trực tiếp |

### 4.3 Đặt tên key theo tính năng — không theo UI label

```json
// ❌ Sai — gắn với UI, nếu label đổi tên phải sửa JSON
{ "Họ và tên": "Nguyễn Văn A" }

// ✅ Đúng — gắn với concept nghiệp vụ
{ "fullName": "Nguyễn Văn A" }
```

---

## 5. TẦNG 3 — TEST CLASS (`Tests/XxxTests.cs`)

### 5.1 Cấu trúc class chuẩn

```csharp
using NUnit.Framework;
using SeleniumProject.Pages;

namespace SeleniumProject.Tests
{
    [TestFixture]
    public class XxxTests : TestBase
    {
        private XxxPage _xxxPage = null!;

        [SetUp]
        public void SetUpXxx()
        {
            CurrentSheetName = "F1";   // Tên sheet Excel báo cáo
            _xxxPage = new XxxPage(Driver);
        }

        // =========================================================
        // F1.01 – Happy case
        // =========================================================
        [Test]
        public void TC_XXX_F1_01_TenMoTaRoRang()
        {
            CurrentTestCaseId = "TC_F1.01";
            var data = DocDuLieu("TC_XXX_F1_01");

            // 1. Điều hướng và Setup
            _xxxPage.Login(data["email"], data["password"]);
            _xxxPage.NavigateTo();
            _xxxPage.SelectNewMode();               // nếu cần

            // 2. Thao tác Input — ĐỌC TỪ data[], KHÔNG CỨNG
            _xxxPage.EnterName(data["fullName"]);
            _xxxPage.EnterPhone(data["phone"]);

            // 3. Submit
            _xxxPage.ClickSave();

            // 4. Gán kết quả TRƯỚC Assert
            CurrentActualResult = _xxxPage.IsOnSuccessPage()
                ? "Lưu thành công, chuyển về trang danh sách."
                : $"Không chuyển trang. URL: {_xxxPage.GetCurrentUrl()}.";

            // 5. Assert
            Assert.That(_xxxPage.IsOnSuccessPage(), Is.True,
                "[F1.01] Phải chuyển về trang thành công sau khi lưu.");
        }
    }
}
```

### 5.2 Template đầy đủ cho test validation (form lỗi)

```csharp
[Test]
public void TC_XXX_F1_02_BatBuocNhapHoTen()
{
    CurrentTestCaseId = "TC_F1.02";
    var data = DocDuLieu("TC_XXX_F1_02");

    _xxxPage.Login(data["email"], data["password"]);
    _xxxPage.NavigateTo();

    _xxxPage.EnterName(data["fullName"]);   // data["fullName"] == ""
    _xxxPage.EnterPhone(data["phone"]);
    _xxxPage.ClickSave();

    // Dùng wrapper method trong Page Object — KHÔNG dùng By.* ở đây
    bool hasError  = _xxxPage.GetValidationMessages().Any()
                     || _xxxPage.IsNameInvalid();

    CurrentActualResult = hasError
        ? "Hệ thống báo lỗi khi không nhập Họ tên — đúng yêu cầu."
        : "Hệ thống KHÔNG báo lỗi khi bỏ trống Họ tên — SAI.";

    Assert.That(hasError, Is.True,
        "[F1.02] Phải có thông báo lỗi khi bỏ trống Họ tên.");
}
```

### 5.3 Template cho test dropdown / state

```csharp
[Test]
public void TC_XXX_F1_03_DropdownQuanHuyenLoadTheoTinh()
{
    CurrentTestCaseId = "TC_F1.03";
    var data = DocDuLieu("TC_XXX_F1_03");

    _xxxPage.Login(data["email"], data["password"]);
    _xxxPage.NavigateTo();

    _xxxPage.SelectProvince(data["province"]);   // ← từ JSON
    Thread.Sleep(1000);

    // ✅ Dùng wrapper — KHÔNG dùng Driver.FindElements(By.CssSelector("#districtSelect option"))
    int count = _xxxPage.GetDistrictOptionCount();

    CurrentActualResult = count > 1
        ? $"Dropdown Quận load {count} options sau khi chọn Tỉnh."
        : "Dropdown Quận KHÔNG load sau khi chọn Tỉnh.";

    Assert.That(count, Is.GreaterThan(1),
        "[F1.03] Dropdown Quận/Huyện phải load sau khi chọn Tỉnh/TP.");
}
```

---

## 6. CHECKLIST KIỂM TRA TRƯỚC KHI COMMIT

Chạy 2 lệnh grep này, nếu có kết quả trả về → **phải sửa trước khi commit:**

```powershell
# Kiểm tra vi phạm POM (By.* dùng trực tiếp trong Tests/)
Select-String -Path "Tests\*.cs" -Pattern "By\.(Id|CssSelector|TagName|XPath|Name)\(|Driver\.(FindElement|FindElements)\("

# Kiểm tra data cứng (chuỗi địa danh, URL localhost trong Tests/)
Select-String -Path "Tests\*.cs" -Pattern "localhost:\d+|Thành phố|Quận \d|Phường |""0\d{9,10}"""
```

**Kết quả chấp nhận được:**
- `Driver.Url` → ✅ OK (chỉ đọc URL để Assert, không tìm element)
- `Driver.Navigate()` → ✅ OK nếu URL lấy từ `data["key"]` trong JSON
- `By.*` trong `Pages/*.cs` → ✅ OK (đây là nơi duy nhất được phép)

---

## 7. CÁC LỖI PHỔ BIẾN VÀ CÁCH SỬA

### Lỗi 1 — Hardcode địa danh

```csharp
// ❌ SAI
_checkoutPage.SelectProvince("Thành phố Hồ Chí Minh");
_checkoutPage.SelectDistrict("Quận 1");

// ✅ ĐÚNG — dữ liệu trong JSON
_checkoutPage.SelectProvince(data["province"]);
_checkoutPage.SelectDistrict(data["district"]);
```

**Fix:** Thêm key `province`, `district`, `ward` vào entry JSON tương ứng.

---

### Lỗi 2 — Dùng By.* trong Test

```csharp
// ❌ SAI — test biết cấu trúc DOM
var districtEl = Driver.FindElement(By.Id("districtSelect"));
bool empty = !districtEl.Enabled;

// ✅ ĐÚNG — Page Object đóng gói logic DOM
bool empty = _checkoutPage.IsDistrictDropdownEmpty();
```

**Fix:** Thêm method wrapper vào `Pages/XxxPage.cs`, test chỉ gọi method đó.

---

### Lỗi 3 — Hardcode URL trong Test

```csharp
// ❌ SAI
Driver.Navigate().GoToUrl("http://localhost:5270/Account/Login?ReturnUrl=/Checkout");

// ✅ ĐÚNG — URL trong JSON
Driver.Navigate().GoToUrl(data["loginWithReturnUrl"]);
// Hoặc dùng wrapper:
_checkoutPage.NavigateToLoginWithReturn(data["returnUrl"]);
```

**Fix:** Thêm key URL vào JSON entry; nếu tái sử dụng nhiều lần thì đóng gói vào constant trong `Pages/XxxPage.cs`.

---

### Lỗi 4 — Gán CurrentActualResult sau Assert

```csharp
// ❌ SAI — nếu Assert.That fail, dòng gán CurrentActualResult không chạy
Assert.That(success, Is.True, "Thất bại");
CurrentActualResult = "Kết quả: " + success;   ← đã chết rồi

// ✅ ĐÚNG — gán TRƯỚC Assert
CurrentActualResult = success
    ? "Thành công — đúng kỳ vọng."
    : "Thất bại — sai kỳ vọng.";
Assert.That(success, Is.True, "Thất bại");
```

---

### Lỗi 5 — Logic phức tạp trong Test (vi phạm Declarative)

```csharp
// ❌ SAI — if/else/loop trong Test
var options = Driver.FindElements(By.CssSelector("#wardSelect option"));
bool valid = false;
foreach (var opt in options)
{
    if (!string.IsNullOrEmpty(opt.GetAttribute("value"))) { valid = true; break; }
}

// ✅ ĐÚNG — đóng gói vào Page Object
bool valid = _checkoutPage.GetWardOptionCount() > 1;
```

---

## 8. MẪU HOÀN CHỈNH — TỪ JSON → PAGE → TEST

### JSON (`TestData/Order/order_data.json`)
```json
{
  "testCase": "TC_ORDER_F10_01",
  "testCaseId": "F10.01",
  "email": "user@test.com",
  "password": "pass123",
  "productUrl": "http://localhost:5270/Shop/Detail/nho-den-uc",
  "checkoutUrl": "http://localhost:5270/Checkout",
  "fullName": "Nguyễn Thị B",
  "phone": "0987654321",
  "streetAddress": "456 Đường XYZ",
  "province": "Thành phố Hà Nội",
  "district": "Quận Ba Đình",
  "ward": "Phường Phúc Xá"
}
```

### Page Object (`Pages/OrderPage.cs`) — snippet
```csharp
private const string CheckoutUrl = "http://localhost:5270/Checkout";

private By FullNameInput => By.Id("FullNameInput");
private By ConfirmHeading => By.XPath("//h1[contains(.,'Cảm ơn')]");

public void EnterFullName(string v) => _wait.SlowType(FullNameInput, v);
public bool IsOnConfirmationPage()
{
    try { _wait.WaitForUrlContains("/Confirmation"); return true; }
    catch { return false; }
}
public bool IsFullNameInvalid() => IsFieldInvalid(FullNameInput);
```

### Test (`Tests/OrderTests.cs`) — snippet
```csharp
[Test]
public void TC_ORDER_F10_01_DatHangThanhCong()
{
    CurrentTestCaseId = "TC_F10.01";
    var data = DocDuLieu("TC_ORDER_F10_01");

    _orderPage.Login(data["email"], data["password"]);
    _orderPage.NavigateToCheckoutWithProduct(data["productUrl"]);
    _orderPage.SelectNewAddressOption();
    _orderPage.EnterFullName(data["fullName"]);
    _orderPage.EnterPhone(data["phone"]);
    _orderPage.EnterStreetAddress(data["streetAddress"]);
    _orderPage.SelectProvince(data["province"]);
    _orderPage.SelectDistrict(data["district"]);
    _orderPage.SelectWard(data["ward"]);
    _orderPage.SelectPaymentCod();
    _orderPage.ClickPlaceOrder();

    bool success = _orderPage.IsOnConfirmationPage();

    CurrentActualResult = success
        ? "Đặt hàng thành công, chuyển về trang xác nhận."
        : $"Đặt hàng THẤT BẠI. URL: {_orderPage.GetCurrentUrl()}.";

    Assert.That(success, Is.True,
        "[F10.01] Phải chuyển về trang /Checkout/Confirmation.");
}
```

---

## 9. TỔNG KẾT — CHECKLIST CHO DEV QA

Trước khi commit bất kỳ file test nào, xác nhận đủ 6 điều:

- [ ] **Không có `By.*` trong `Tests/`** — chỉ có trong `Pages/`
- [ ] **Không có `Driver.FindElement/s(...)` trong `Tests/`**
- [ ] **Không có chuỗi literal địa danh/số điện thoại/họ tên trong `Tests/`**
- [ ] **Không có URL `localhost:xxxx` hardcode trong `Tests/`** — URL nằm trong JSON hoặc constants trong `Pages/`
- [ ] **`CurrentActualResult` được gán TRƯỚC mọi `Assert.That()`**
- [ ] **Không có `if/else`, vòng lặp, logic DOM phức tạp trong Test method** — đóng gói vào Page Object