# 🍉 Selenium NUnit Test Project — Fruitables

> **Framework kiểm thử tự động UI** cho website thương mại điện tử **Fruitables** (`vuatraicay.site`) — được xây dựng theo kiến trúc **Page Object Model**, tích hợp **báo cáo Excel tự động** và hỗ trợ **Data-Driven Testing** toàn diện.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Selenium](https://img.shields.io/badge/Selenium-4.41-43B02A?logo=selenium)](https://www.selenium.dev/)
[![NUnit](https://img.shields.io/badge/NUnit-4.3-009000)](https://nunit.org/)
[![Chrome](https://img.shields.io/badge/Chrome-Latest-4285F4?logo=googlechrome)](https://www.google.com/chrome/)

---

## 📋 Mục lục

- [Tổng quan](#-tổng-quan)
- [Kiến trúc project](#-kiến-trúc-project)
- [Các module test](#-các-module-test)
- [Tính năng nổi bật](#-tính-năng-nổi-bật)
- [Yêu cầu hệ thống](#-yêu-cầu-hệ-thống)
- [Cài đặt & Cấu hình](#-cài-đặt--cấu-hình)
- [Chạy test](#-chạy-test)
- [Thêm test mới](#-thêm-test-mới)
- [Quy ước đặt tên](#-quy-ước-đặt-tên)
- [Công nghệ sử dụng](#-công-nghệ-sử-dụng)
- [Tài liệu team](#-tài-liệu-team)

---

## 🎯 Tổng quan

Dự án cung cấp bộ test tự động end-to-end cho toàn bộ luồng nghiệp vụ của Fruitables — từ **đăng ký / đăng nhập**, **quản lý sản phẩm**, **giỏ hàng**, **thanh toán**, đến **quản lý đơn hàng** và **hồ sơ người dùng**.

Kết quả mỗi lần chạy được ghi tự động vào file **Excel báo cáo** (`204_local.xlsx`) kèm ảnh chụp màn hình khi test thất bại — giúp team theo dõi chất lượng phần mềm không cần can thiệp thủ công.

---

## 🗂 Kiến trúc project

```
SeleniumProject/
│
├── Pages/                              ← Page Object Model (POM)
│   ├── Auth/                           ← Trang đăng nhập, đăng ký
│   ├── CartManagement/                 ← Trang giỏ hàng, vận chuyển
│   ├── OrderManagement/                ← Trang danh sách & chi tiết đơn hàng
│   ├── ProductManagement/              ← Trang tạo / tìm kiếm / lọc sản phẩm
│   ├── ProfileManagement/              ← Trang hồ sơ, địa chỉ, avatar
│   └── CheckoutPage.cs                 ← Trang thanh toán
│
├── Tests/                              ← Test cases chia theo module
│   ├── Auth/
│   │   ├── AuthLoginTests.cs           ← Đăng nhập (valid / invalid / boundary)
│   │   └── RegisterTests.cs            ← Đăng ký tài khoản
│   ├── CartManagement/
│   │   ├── CartTests.cs                ← Thêm / xóa / cập nhật giỏ hàng
│   │   ├── CartAdvancedTests.cs        ← Kịch bản nâng cao
│   │   ├── CartApiTests.cs             ← Kiểm thử qua API endpoint
│   │   └── CartShippingTests.cs        ← Tính phí vận chuyển
│   ├── OrderManagement/
│   │   ├── OrderListTests.cs           ← Danh sách đơn hàng
│   │   ├── OrderDetailTests.cs         ← Chi tiết đơn hàng
│   │   ├── OrderSearchTests.cs         ← Tìm kiếm đơn hàng
│   │   ├── OrderFilterTests.cs         ← Lọc theo trạng thái / ngày
│   │   ├── OrderApproveTests.cs        ← Duyệt đơn hàng
│   │   ├── OrderCancelTests.cs         ← Hủy đơn hàng
│   │   ├── OrderPaymentTests.cs        ← Xác nhận thanh toán
│   │   ├── OrderNoteTests.cs           ← Ghi chú đơn hàng
│   │   ├── OrderPrintTests.cs          ← In đơn hàng
│   │   └── OrderStatusHistoryTests.cs  ← Lịch sử trạng thái
│   ├── ProductManagement/
│   │   ├── ProductCreateTests.cs       ← Thêm sản phẩm mới
│   │   ├── ProductListTests.cs         ← Danh sách sản phẩm
│   │   ├── ProductSearchTests.cs       ← Tìm kiếm sản phẩm
│   │   └── ProductFilterTests.cs       ← Lọc sản phẩm
│   ├── ProfileManagement/
│   │   ├── ProfileViewTests.cs         ← Xem hồ sơ
│   │   ├── ProfileEditTests.cs         ← Chỉnh sửa thông tin cá nhân
│   │   ├── ProfileAddressTests.cs      ← Quản lý địa chỉ
│   │   └── ProfileAvatarTests.cs       ← Upload / thay đổi avatar
│   ├── CheckoutTests.cs                ← Luồng thanh toán, mã giảm giá
│   └── Tools/                          ← Công cụ nội bộ (Excel sync, formatter…)
│
├── TestData/                           ← Dữ liệu test (JSON)
│   ├── Auth/
│   ├── CartManagement/
│   ├── OrderManagement/
│   ├── ProductManagement/
│   ├── ProfileManagement/
│   ├── Checkout/
│   └── Images/                         ← File dùng cho upload test
│       ├── valid_small.jpg / .png / .webp
│       └── invalid_format.pdf / .exe
│
├── Utilities/                          ← Dùng chung
│   ├── Browser/
│   │   ├── DriverFactory.cs            ← Khởi tạo Chrome WebDriver
│   │   └── WaitHelper.cs               ← Explicit wait helpers
│   ├── Data/
│   │   └── ExcelHelper.cs              ← Đọc/ghi kết quả vào Excel
│   ├── Files/
│   │   └── FileHelper.cs               ← Xử lý file upload / path
│   ├── JsonHelper.cs                   ← Đọc test data từ JSON
│   └── TestBase.cs                     ← Setup/Teardown + screenshot + ghi Excel
│
├── Reports/
│   └── Screenshots/                    ← Ảnh chụp tự động khi test fail
│
├── 204_local.xlsx                      ← File Excel báo cáo kết quả test
├── appsettings.json                    ← Cấu hình môi trường
└── howtocommit.md                      ← Quy ước commit message
```

---

## 🧪 Các module test

| Module | Số test class | Phạm vi kiểm thử |
|---|:---:|---|
| **Auth** | 2 | Đăng nhập, đăng ký — valid, invalid, boundary |
| **Product Management** | 4 | Tạo sản phẩm, danh sách, tìm kiếm, lọc |
| **Cart Management** | 4 | Thêm/xóa/cập nhật giỏ hàng, vận chuyển, API |
| **Order Management** | 10 | Xem, tìm kiếm, lọc, duyệt, hủy, thanh toán, ghi chú, in, lịch sử |
| **Profile Management** | 4 | Xem, chỉnh sửa, địa chỉ, avatar |
| **Checkout** | 1 | Luồng thanh toán end-to-end, mã giảm giá |

---

## ✨ Tính năng nổi bật

| Tính năng | Mô tả |
|---|---|
| 🏗 **Page Object Model** | Locator và action tách biệt hoàn toàn khỏi test logic |
| 📊 **Data-Driven Testing** | Test data lưu trong JSON, dễ thêm kịch bản mà không sửa code |
| 📝 **Báo cáo Excel tự động** | Kết quả Pass/Fail + Actual Result ghi thẳng vào `204_local.xlsx` |
| 📸 **Screenshot tự động** | Chụp màn hình ngay khi test fail, lưu phân loại theo module |
| 🔧 **Cấu hình linh hoạt** | URL, timeout, headless, tài khoản admin/customer đều đọc từ `appsettings.json` |
| 👥 **Multi-role support** | Đăng nhập sẵn bằng Admin hoặc Customer qua `TestBase` |
| 🔄 **Shared Driver** | Tùy chọn tái sử dụng browser session giữa các test để tăng tốc |

---

## 💻 Yêu cầu hệ thống

| Thành phần | Yêu cầu |
|---|---|
| **.NET SDK** | [.NET 9.0+](https://dotnet.microsoft.com/download) |
| **Trình duyệt** | Google Chrome (phiên bản mới nhất) |
| **IDE** | Visual Studio 2022 hoặc VS Code + C# Dev Kit |
| **Python** _(tùy chọn)_ | Python 3.x — chỉ cần nếu dùng scripts trong `Tools/` |

---

## ⚙ Cài đặt & Cấu hình

### 1. Clone & restore

```bash
git clone https://github.com/duchuy19012004/204-FruitWeb-SeleniumProject.git
cd 204-FruitWeb-SeleniumProject/SeleniumProject
dotnet restore
```

### 2. Cấu hình môi trường

Chỉnh `appsettings.json` cho phù hợp:

```json
{
  "BaseUrl": "https://vuatraicay.site",
  "Browser": "Chrome",
  "Timeout": 15,
  "Headless": false,
  "AdminEmail": "admin@example.com",
  "AdminPassword": "***",
  "CustomerEmail": "customer@example.com",
  "CustomerPassword": "***",
  "ReportExcelPath": "C:\\path\\to\\204_local.xlsx"
}
```

| Key | Mô tả | Mặc định |
|---|---|---|
| `BaseUrl` | URL website đang test | `https://vuatraicay.site` |
| `Timeout` | Thời gian chờ tối đa (giây) | `15` |
| `Headless` | Chạy Chrome ẩn (không hiện cửa sổ) | `false` |
| `ReportExcelPath` | Đường dẫn file Excel ghi kết quả | _(bỏ trống = không ghi)_ |

> **💡 Tip:** Tạo file `appsettings.local.json` để ghi đè cấu hình cá nhân mà không ảnh hưởng đến cấu hình chung của team.

---

## ▶ Chạy test

```bash
# Chạy toàn bộ test suite
dotnet test SeleniumProject.csproj

# Chạy 1 test case cụ thể
dotnet test --filter "FullyQualifiedName~TC_LOGIN_01"

# Chạy toàn bộ 1 module
dotnet test --filter "ClassName~OrderApproveTests"
dotnet test --filter "ClassName~ProfileEditTests"
dotnet test --filter "ClassName~CartTests"

# Chạy toàn bộ theo namespace
dotnet test --filter "Namespace~OrderManagement"
dotnet test --filter "Namespace~ProductManagement"
dotnet test --filter "Namespace~CartManagement"
dotnet test --filter "Namespace~ProfileManagement"

# Chạy chỉ happy-path (positive)
dotnet test --filter "TestCategory=Positive"
```

---

## 🛠 Thêm test mới

1. **Phân tích trang** — dùng DevTools (F12) để lấy CSS selector / ID của element
2. **Tạo Page Object** tại `Pages/<Module>/` — định nghĩa locator + action methods
3. **Thêm test data** vào `TestData/<Module>/<module>_data.json`
4. **Viết test class** tại `Tests/<Module>/` kế thừa `TestBase`
5. **Gán `CurrentTestCaseId`** và **`CurrentActualResult`** để tích hợp báo cáo Excel

```csharp
[Test]
public void TC_XX_01_MoTaNgan()
{
    CurrentTestCaseId = "TC_XX_01";

    // ... thao tác với Page Object ...

    CurrentActualResult = "Kết quả thực tế quan sát được";
    Assert.That(condition, Is.True);
}
```

> ⚠️ **Quan trọng:** Nếu quên gán `CurrentTestCaseId`, kết quả sẽ không được ghi vào file Excel báo cáo.

---

## 📐 Quy ước đặt tên

### Test Class
```
[Chức năng]Tests
```
Ví dụ: `OrderApproveTests`, `ProfileAvatarTests`, `CartShippingTests`

### Test Method
```
TC_[NHÓM]_[SỐ]_[MôTảNgắn]
```
Ví dụ: `TC_OA_01_ApproveOrder_ValidStatus_ShouldSucceed`

### Nhóm test case

| Ký hiệu | Ý nghĩa |
|:---:|---|
| `TC_AP_xx` | **Positive** — Happy path, luồng thành công |
| `TC_NE_xx` | **Negative** — Validation, bảo mật, lỗi đầu vào |
| `TC_BD_xx` | **Boundary** — Giá trị biên (min/max) |
| `TC_UX_xx` | **UX / Look & Feel** — Giao diện, hiển thị |

---

## 📦 Công nghệ sử dụng

| Package | Version | Mục đích |
|---|:---:|---|
| `Selenium.WebDriver` | 4.41 | Tự động hóa trình duyệt |
| `Selenium.Support` | 4.41 | SelectElement, PageFactory… |
| `NUnit` | 4.3 | Framework viết và chạy test |
| `NUnit3TestAdapter` | 5.0 | Adapter để `dotnet test` nhận diện NUnit |
| `WebDriverManager` | 2.17 | Tự động tải ChromeDriver phù hợp phiên bản |
| `DotNetSeleniumExtras.WaitHelpers` | 3.11 | Explicit wait conditions |
| `ClosedXML` | 0.105 | Đọc/ghi file Excel (.xlsx) |
| `NPOI` | 2.7 | Xử lý Excel nâng cao |
| `Microsoft.Extensions.Configuration` | 10.0 | Đọc cấu hình từ JSON |

---

## 📚 Tài liệu team

| File | Nội dung |
|---|---|
| [`howtocommit.md`](./howtocommit.md) | Quy ước đặt tên commit message (Conventional Commits) |
| [`204_local.xlsx`](./204_local.xlsx) | File Excel báo cáo kết quả test tự động |

---


