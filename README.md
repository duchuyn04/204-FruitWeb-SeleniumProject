# Selenium NUnit Test Project — Fruitables

Framework kiểm thử tự động UI cho website [vuatraicay.site](https://vuatraicay.site).
Sử dụng **Selenium WebDriver** + **NUnit** + **Page Object Model (POM)** + **Data-Driven Testing**.

---

## Cấu trúc Project

```
SeleniumProject/
├── Pages/                    ← Page Object Model
│   ├── LoginPage.cs
│   └── RegisterPage.cs
├── Tests/                    ← Test cases chia theo module
│   ├── LoginTests.cs
│   └── TransferTests.cs
├── TestData/                 ← Dữ liệu test (JSON)
│   └── Auth/
│       └── login_data.json
├── Utilities/                ← Dùng chung
│   ├── DriverFactory.cs      ← Khởi tạo browser
│   ├── WaitHelper.cs         ← Chờ element
│   ├── TestBase.cs           ← Setup/Teardown + chụp ảnh khi lỗi
│   └── ExcelHelper.cs        ← Đọc/ghi Excel
└── Reports/
    └── Screenshots/          ← Ảnh chụp tự động khi test fail
```

---

## Yêu cầu hệ thống

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- Google Chrome (phiên bản mới nhất)
- VS Code hoặc Visual Studio 2022

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

## Chạy Test

```bash
# Chạy tất cả test
dotnet test SeleniumProject.csproj

# Chạy 1 test case cụ thể
dotnet test SeleniumProject.csproj --filter "FullyQualifiedName~TC_LOGIN_01"

# Chạy toàn bộ 1 module
dotnet test SeleniumProject.csproj --filter "ClassName~LoginTests"
```

---

## Thêm Test Mới

1. **Phân tích trang** — dùng DevTools (F12) để lấy CSS selector / ID của các element
2. **Tạo Page Object** trong `Pages/` — locators + actions
3. **Thêm test data** vào `TestData/<module>/<module>_data.json`
4. **Viết test case** trong `Tests/` kế thừa `TestBase`

---

## Tài liệu team

| File | Nội dung |
|---|---|
| `howtocommit.md` | Quy ước đặt tên commit message |
---

## Công nghệ sử dụng

| Package | Mục đích |
|---|---|
| `Selenium.WebDriver` 4.41 | Tự động hóa trình duyệt |
| `NUnit` 4.3 | Framework viết và chạy test |
| `WebDriverManager` | Tự động tải ChromeDriver |
| `DotNetSeleniumExtras.WaitHelpers` | Explicit wait conditions |
| `ClosedXML` | Đọc file Excel |
