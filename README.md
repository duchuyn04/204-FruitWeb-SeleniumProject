# Selenium NUnit Test Project

Repository này chứa framework kiểm thử tự động UI cho ứng dụng FruitWeb.
Nó sử dụng **Selenium WebDriver** để tự động hóa trình duyệt và **NUnit** để chạy test và kiểm tra kết quả (assertions). Project được cấu trúc theo design pattern **Page Object Model (POM)** kết hợp với **Data-Driven Testing (Kiểm thử hướng dữ liệu)**.

## Cấu trúc Project

Project được tổ chức một cách rõ ràng để đảm bảo tính dễ bảo trì và khả năng tái sử dụng cao:

- `Tests/`: Chứa các kịch bản kiểm thử (test scripts) thực tế. Các class trong này sử dụng các attribute của NUnit (như `[TestFixture]`, `[Test]`, `[SetUp]`).
  - *Ví dụ: `LoginTests.cs`, `TransferTests.cs`*
- `Pages/`: Chứa các class Page Object đại diện cho các trang khác nhau của ứng dụng web. Mỗi class đóng gói các locators (cách tìm phần tử) và actions (hành động, thao tác) tương ứng với trang đó.
  - *Ví dụ: `LoginPage.cs`, `RegisterPage.cs`, `TransferFundsPage.cs`*
- `Utilities/`: Chứa các class hỗ trợ (helper classes), cấu hình, và logic thiết lập cốt lõi (ví dụ: khởi tạo WebDriver, đọc file cấu hình).
  - *Ví dụ: `DriverFactory.cs`*
- `TestData/`: Chứa các file dữ liệu bên ngoài dùng cho Data-Driven Testing. Việc tách biệt dữ liệu giúp các kịch bản test linh hoạt hơn.
  - *Ví dụ: `users.json`*

## Yêu cầu hệ thống

Để chạy project này, hãy đảm bảo bạn đã cài đặt:

1.  [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (hoặc phiên bản được chỉ định trong file project của bạn).
2.  Một IDE hỗ trợ như [Visual Studio 2022](https://visualstudio.microsoft.com/) hoặc [VS Code](https://code.visualstudio.com/) với các extension cho C#.
3.  Trình duyệt web (ví dụ: Google Chrome, Mozilla Firefox) đã được cài đặt trên máy.
    *   *Lưu ý: Việc sử dụng `Selenium.WebDriver` ở các phiên bản gần đây thường tự động quản lý driver của trình duyệt, nhưng hãy đảm bảo trình duyệt của bạn được cập nhật mới nhất.*

## Cài đặt và Thiết lập

1.  Clone repository về máy:
    ```bash
    git clone https://github.com/duchuy19012004/204-FruitWeb-SeleniumProject.git
    ```
2.  Di chuyển vào thư mục project:
    ```bash
    cd SeleniumProject
    ```
3.  Khôi phục (restore) các package NuGet:
    ```bash
    dotnet restore
    ```

## Thêm Test Mới

1.  **Tạo / Cập nhật Page Objects:**
    Nếu bạn đang viết test cho một trang mới, hãy tạo một class mới trong thư mục `Pages`. Thêm các biến `By` locator cho các phần tử bạn cần tương tác và các phương thức `public` để thực hiện hành động.
2.  **Thêm Dữ liệu Test:**
    Nếu bài test của bạn cần dữ liệu đầu vào, hãy thêm nó vào `TestData/users.json` hoặc tạo một file dữ liệu mới.
3.  **Tạo Test Class:**
    Tạo một class mới trong thư mục `Tests`. Thêm attribute `[TestFixture]` cho class. Khởi tạo các Page Object liên quan và viết các phương thức `[Test]` gọi đến các hành động đó. Sử dụng class `Assert` của NUnit để xác minh kết quả.

## Cách chạy Test

### Từ Visual Studio
1.  Mở cửa sổ **Test Explorer** (`Test > Test Explorer`).
2.  Build (F6 hoặc Ctrl+Shift+B) lại solution.
3.  Click vào nút "Run All Tests" (Chạy tất cả) hoặc chọn các test cụ thể để chạy.

### Dùng Command Line / Terminal
Để chạy tất cả các test trong project, hãy thực thi lệnh sau trong thư mục chứa file `.csproj` (thư mục `SeleniumProject`):
```bash
dotnet test
```

## Công nghệ sử dụng

- **C#**: Ngôn ngữ lập trình chính
- **Selenium WebDriver**: Tự động hóa thao tác trình duyệt
- **NUnit**: Framework viết và chạy test
- **Selenium Support**: Các công cụ hỗ trợ thêm của Selenium (ví dụ: `WebDriverWait`)
