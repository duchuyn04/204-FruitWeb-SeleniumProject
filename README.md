# Selenium NUnit Test Project — Fruitables 🍉

Framework kiểm thử tự động UI cho website **Fruitables** (vuatraicay.site).  
Sử dụng **Selenium WebDriver** + **NUnit** + **Page Object Model (POM)** + **Data-Driven Testing** + **Báo cáo Excel tự động**.

---

## Cấu trúc Project

```
SeleniumProject/
├── Pages/                              ← Page Object Model
│   ├── Auth/
│   │   └── LoginPage.cs                ← Trang đăng nhập
│   ├── ProductManagement/
│   │   └── CreateProductPage.cs        ← Trang tạo sản phẩm (Admin)
│   └── CheckoutPage.cs                 ← Trang thanh toán
│
├── Tests/                              ← Test cases chia theo module
│   ├── Auth/
│   │   └── LoginTests.cs               ← Test đăng nhập
│   ├── ProductManagement/
│   │   ├── AddProductTests.cs           ← Test thêm sản phẩm
│   │   ├── ProductValidationTests.cs    ← Test validation sản phẩm
│   │   └── ProductAccessTests.cs        ← Test phân quyền truy cập
│   └── CheckoutTests.cs                ← Test luồng thanh toán
│
├── TestData/                           ← Dữ liệu test (JSON + file)
│   ├── Auth/
│   │   └── login_data.json
│   ├── ProductManagement/
│   │   └── product_data.json
│   ├── Checkout/
│   │   └── checkout_data.json
│   └── Images/                         ← File dùng cho upload test
│       ├── valid_small.jpg
│       ├── valid_small.png
│       ├── valid_small.webp
│       ├── invalid_format.pdf
│       └── invalid_format.exe
│
├── Utilities/                          ← Dùng chung
│   ├── Browser/
│   │   ├── DriverFactory.cs            ← Khởi tạo Chrome WebDriver
│   │   └── WaitHelper.cs               ← Explicit wait (click, URL, element…)
│   ├── Data/
│   │   └── ExcelHelper.cs              ← Đọc/ghi kết quả test vào Excel
│   ├── Files/
│   │   └── FileHelper.cs               ← Xử lý file (tạo file test, lấy path…)
│   ├── JsonHelper.cs                   ← Đọc test data từ JSON
│   └── TestBase.cs                     ← Setup/Teardown + screenshot + ghi Excel
│
├── Reports/
│   └── Screenshots/                    ← Ảnh chụp tự động khi test fail
│       ├── Auth/
│       ├── ProductManagement/
│       └── Checkout/
│
├── 204.xlsx                            ← File Excel báo cáo kết quả test
├── appsettings.json                    ← Cấu hình (URL, browser, tài khoản…)
├── howtocommit.md                      ← Quy ước commit message
└── NOTES.md                            ← Ghi chú nội bộ (không commit)
```

---

## Các module test

| Module | Page Object | Test Class | Mô tả |
|---|---|---|---|
| **Auth** | `LoginPage.cs` | `LoginTests.cs` | Đăng nhập với nhiều bộ dữ liệu |
| **Product Management** | `CreateProductPage.cs` | `AddProductTests.cs`, `ProductValidationTests.cs`, `ProductAccessTests.cs` | Thêm sản phẩm, validation form, phân quyền Admin/Customer |
| **Checkout** | `CheckoutPage.cs` | `CheckoutTests.cs` | Luồng thanh toán, áp mã giảm giá |

---

## Tính năng nổi bật

- ✅ **Page Object Model** — Tách biệt locator và logic thao tác
- ✅ **Data-Driven Testing** — Test data lưu trong file JSON, dễ mở rộng
- ✅ **Báo cáo Excel tự động** — Kết quả Pass/Fail + Actual Result ghi trực tiếp vào file `204.xlsx`
- ✅ **Screenshot tự động** — Chụp màn hình khi test fail, lưu theo module
- ✅ **Shared Driver** — Tùy chọn tái sử dụng browser session giữa các test
- ✅ **Cấu hình linh hoạt** — URL, timeout, headless, tài khoản đều đọc từ `appsettings.json`
- ✅ **Hỗ trợ đa vai trò** — Login sẵn Admin hoặc Customer qua `TestBase`

---

## Yêu cầu hệ thống

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- Google Chrome (phiên bản mới nhất)
- Visual Studio 2022 hoặc VS Code (với C# Dev Kit)

---

## Cài đặt

```bash
# 1. Clone project
git clone https://github.com/duchuy19012004/204-FruitWeb-SeleniumProject.git

# 2. Vào thư mục project
cd 204-FruitWeb-SeleniumProject/SeleniumProject

# 3. Restore NuGet packages
dotnet restore
```

---

## Cấu hình

Chỉnh sửa `appsettings.json` cho phù hợp với môi trường:

```json
{
  "BaseUrl": "https://localhost:7035",
  "Browser": "Chrome",
  "Timeout": 15,
  "Headless": false,
  "AdminEmail": "admin@example.com",
  "AdminPassword": "***",
  "CustomerEmail": "customer@example.com",
  "CustomerPassword": "***",
  "ReportExcelPath": "C:\\path\\to\\204.xlsx"
}
```

| Key | Mô tả | Mặc định |
|---|---|---|
| `BaseUrl` | URL website đang test | `https://vuatraicay.site` |
| `Timeout` | Thời gian chờ tối đa (giây) | `15` |
| `Headless` | Chạy Chrome ẩn (không hiện cửa sổ) | `false` |
| `ReportExcelPath` | Đường dẫn file Excel ghi kết quả | _(bỏ trống = không ghi)_ |

> **Tip:** Tạo file `appsettings.local.json` để ghi đè cấu hình cá nhân mà không ảnh hưởng team.

---

## Chạy Test

```bash
# Chạy tất cả test
dotnet test SeleniumProject.csproj

# Chạy 1 test case cụ thể
dotnet test SeleniumProject.csproj --filter "FullyQualifiedName~TC_LOGIN_01"

# Chạy toàn bộ 1 module
dotnet test SeleniumProject.csproj --filter "ClassName~LoginTests"
dotnet test SeleniumProject.csproj --filter "ClassName~AddProductTests"
dotnet test SeleniumProject.csproj --filter "ClassName~CheckoutTests"

# Chạy tất cả test trong namespace ProductManagement
dotnet test SeleniumProject.csproj --filter "Namespace~ProductManagement"
```

---

## Thêm Test Mới

1. **Phân tích trang** — Dùng DevTools (F12) để lấy CSS selector / ID của các element
2. **Tạo Page Object** trong `Pages/<Module>/` — định nghĩa locator + action
3. **Thêm test data** vào `TestData/<Module>/<module>_data.json`
4. **Viết test class** trong `Tests/<Module>/` kế thừa `TestBase`
5. **Gán `CurrentTestCaseId`** và **`CurrentActualResult`** trong mỗi test method để tích hợp báo cáo Excel

```csharp
[Test]
public void TC_XX_01_MoTaNgan()
{
    CurrentTestCaseId = "TC_XX_01";
    // ... thao tác test ...
    CurrentActualResult = "Kết quả thực tế quan sát được";
    Assert.That(condition, Is.True);
}
```

---

## Quy ước đặt tên

### Test Class
- Format: `[Chức năng]Tests` — ví dụ: `LoginTests`, `AddProductTests`, `CheckoutTests`

### Test Method
- Format: `TC_[Nhóm]_[Số]_[MôTảNgắn]`
- Ví dụ: `TC_AP_01_AddProduct_ValidData_ShouldSucceed`

### Nhóm Test Case

| Ký hiệu | Ý nghĩa |
|---|---|
| `TC-AP-xx` | Positive — Happy path |
| `TC-NE-xx` | Negative — Validation & Security |
| `TC-BD-xx` | Boundary — Giá trị biên |
| `TC-UX-xx` | Look & Feel / UX |

---

## Tài liệu team

| File | Nội dung |
|---|---|
| `howtocommit.md` | Quy ước đặt tên commit message (Conventional Commits) |
| `NOTES.md` | Ghi chú nội bộ, tài khoản test, URL quan trọng |
| `204.xlsx` | File Excel báo cáo kết quả test tự động |

---

## Công nghệ sử dụng

| Package | Version | Mục đích |
|---|---|---|
| `Selenium.WebDriver` | 4.41 | Tự động hóa trình duyệt |
| `Selenium.Support` | 4.41 | SelectElement, PageFactory… |
| `NUnit` | 4.3 | Framework viết và chạy test |
| `NUnit3TestAdapter` | 5.0 | Adapter để `dotnet test` nhận diện NUnit |
| `WebDriverManager` | 2.17 | Tự động tải ChromeDriver |
| `DotNetSeleniumExtras.WaitHelpers` | 3.11 | Explicit wait conditions |
| `ClosedXML` | 0.105 | Đọc/ghi file Excel (.xlsx) |
| `NPOI` | 2.7 | Xử lý file Excel (backup) |
| `Microsoft.Extensions.Configuration` | 10.0 | Đọc cấu hình từ JSON |
