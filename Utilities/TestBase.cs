using Microsoft.Extensions.Configuration;
using SeleniumProject.Pages.Auth;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;

namespace SeleniumProject.Utilities
{
    public class TestBase
    {
        protected IWebDriver Driver { get; private set; } = null!;
        protected WaitHelper Wait { get; private set; } = null!;

        // Đặt true trong subclass muốn dùng [OneTimeSetUp] — driver sẽ không bị tạo mới / quit mỗi test
        protected bool UseSharedDriver { get; set; } = false;

        // Gọi từ [OneTimeSetUp] của subclass dùng shared driver — khởi tạo driver và login
        protected void InitSharedDriver()
        {
            Driver = DriverFactory.InitializeDriver(Headless);
            Wait = new WaitHelper(Driver, Timeout);
        }

        // Mỗi test method tự gán vào đầu hàm để TearDown biết đang chạy TC nào
        // Ví dụ: CurrentTestCaseId = "TC_F2.5_01";
        protected string CurrentTestCaseId { get; set; } = "";

        // Kết quả thực tế quan sát được từ trình duyệt — gán TRƯỚC khi Assert
        // VD: CurrentActualResult = "Redirect về /Admin/Product thành công"
        //     CurrentActualResult = "Vẫn ở trang Create, không redirect"
        protected string CurrentActualResult { get; set; } = "";

        // Tên sheet Excel tương ứng với module đang test
        // Mỗi test class tự set trong SetUpPages() — ví dụ: "TC_Auth", "TC_Product Management"
        // Không set → TearDown sẽ bỏ qua việc ghi Excel
        protected string CurrentSheetName { get; set; } = "";

        // Đọc config từ appsettings.json
        private static readonly IConfiguration Config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        protected static string BaseUrl
        {
            get
            {
                string url = Config["BaseUrl"];
                if (url == null)
                {
                    return "https://vuatraicay.site";
                }
                return url;
            }
        }

        protected static int Timeout
        {
            get
            {
                string timeoutStr = Config["Timeout"];
                int timeout;
                bool ok = int.TryParse(timeoutStr, out timeout);
                if (ok)
                {
                    return timeout;
                }
                return 15;
            }
        }

        // Tài khoản admin dùng trong các test cần đăng nhập admin
        protected static string AdminEmail
        {
            get
            {
                string email = Config["AdminEmail"];
                if (email == null)
                {
                    return "";
                }
                return email;
            }
        }

        protected static string AdminPassword
        {
            get
            {
                string password = Config["AdminPassword"];
                if (password == null)
                {
                    return "";
                }
                return password;
            }
        }

        // Tài khoản Customer — dùng trong TC_F2.5_17 (Customer truy cập Admin)
        protected static string CustomerEmail
        {
            get { return Config["CustomerEmail"] ?? ""; }
        }

        protected static string CustomerPassword
        {
            get { return Config["CustomerPassword"] ?? ""; }
        }

        // Đăng nhập bằng tài khoản admin — dùng chung cho mọi test cần quyền admin
        // Gọi trong [SetUp] của từng test class thay vì lặp lại login code
        protected void LoginAsAdmin()
        {
            LoginPage loginPage = new LoginPage(Driver, BaseUrl);
            loginPage.Open();
            loginPage.Login(AdminEmail, AdminPassword);

            // Chờ đến khi URL không còn ở trang login — tức là redirect đã hoàn tất
            // Nếu không chờ, test tiếp theo gọi Page.Open() ngay khi browser vẫn đang redirect
            // gây ra race condition và browser bị treo ở dashboard
            Wait.WaitForUrlNotContains("/Account/Login");
        }

        // Đăng nhập bằng tài khoản Customer
        protected void LoginAsCustomer()
        {
            LoginPage loginPage = new LoginPage(Driver, BaseUrl);
            loginPage.Open();
            loginPage.Login(CustomerEmail, CustomerPassword);
            Wait.WaitForUrlNotContains("/Account/Login");
        }

        // True = Chrome chạy ẩn (headless), tốc độ nhanh hơn, không hiện cửa sổ
        // False = Chrome hiện cửa sổ (dùng khi debug muốn nhìn thấy)
        protected static bool Headless
        {
            get
            {
                string value = Config["Headless"];
                bool result;
                bool ok = bool.TryParse(value, out result);
                if (ok)
                {
                    return result;
                }
                return false;
            }
        }

        // Đường dẫn file Excel để ghi kết quả test
        protected static string ReportExcelPath
        {
            get
            {
                string path = Config["ReportExcelPath"];
                if (path == null)
                {
                    return "";
                }
                return path;
            }
        }

        // Đường dẫn lưu ảnh chụp màn hình khi test lỗi
        private static readonly string ScreenshotDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "Reports", "Screenshots"
        );

        [SetUp]
        public void SetUp()
        {
            // UseSharedDriver = true → driver được tạo ở [OneTimeSetUp] của subclass, không tạo lại
            if (!UseSharedDriver)
            {
                Driver = DriverFactory.InitializeDriver(Headless);
                Wait = new WaitHelper(Driver, Timeout);
            }
        }

        [TearDown]
        public void TearDown()
        {
            var ketQua = TestContext.CurrentContext.Result;
            bool loi   = ketQua.Outcome.Status == TestStatus.Failed;

            // Chụp màn hình nếu test vừa bị lỗi — lưu đường dẫn để ghi vào Excel
            string duongDanScreenshot = "";
            if (loi)
            {
                duongDanScreenshot = ChupManHinhKhiLoi();
            }

            // Ghi kết quả vào file Excel
            if (!string.IsNullOrEmpty(ReportExcelPath))
            {
                try
                {
                    string tenTest = TestContext.CurrentContext.Test.MethodName ?? "";
                    string ghiChu  = ketQua.Message ?? "";

                    // Xóa stack trace khỏi ghi chú — chỉ lấy dòng đầu
                    if (ghiChu.Contains("\n"))
                    {
                        ghiChu = ghiChu.Split('\n')[0].Trim();
                    }

                    ExcelHelper excel = new ExcelHelper(ReportExcelPath);

                    // Ghi vào sheet TC_Product Management theo đúng hàng Test Case ID
                    excel.GhiKetQuaVaoSheet(
                        testCaseId:          CurrentTestCaseId,
                        tenMethod:           tenTest,
                        isPassed:            !loi,
                        actualResult:        CurrentActualResult,
                        duongDanScreenshot:  duongDanScreenshot,
                        sheetName:           CurrentSheetName
                    );
                }
                catch (Exception excelEx)
                {
                    // Ghi lỗi ra output của test — để biết nguyên nhân mà vẫn không fail test
                    TestContext.WriteLine("[EXCEL ERROR] GhiKetQuaVaoSheet thất bại: " + excelEx.Message);
                    TestContext.WriteLine("[EXCEL ERROR] Chi tiết: " + excelEx.GetType().Name);
                }
            }

            // UseSharedDriver = true → driver giữ sống cho test tiếp theo, chỉ quit ở [OneTimeTearDown]
            if (!UseSharedDriver)
            {
                Driver?.Quit();
                Driver?.Dispose();
            }
        }

        // Chụp và lưu ảnh màn hình vào thư mục Reports/Screenshots/{Module}/
        // Module được tự động lấy từ namespace của test class (phần ngay trước tên class)
        // VD: "SeleniumProject.Tests.Auth.LoginTests" -> module = "Auth"
        // Trả về đường dẫn file đã lưu, hoặc chuỗi rỗng nếu không chụp được
        private string ChupManHinhKhiLoi()
        {
            try
            {
                // Bước 1: Lấy tên đầy đủ của class đang chạy test
                // VD: "SeleniumProject.Tests.Auth.LoginTests"
                string fullClassName = TestContext.CurrentContext.Test.ClassName;
                if (fullClassName == null)
                {
                    fullClassName = "";
                }

                // Bước 2: Tách thành mảng theo dấu chấm
                // VD: ["SeleniumProject", "Tests", "Auth", "LoginTests"]
                string[] parts = fullClassName.Split('.');

                // Bước 3: Lấy phần tử áp cuối làm tên module
                // Mảng 4 phần tử: index 0,1,2,3 → áp cuối là index 2 = "Auth"
                string module;
                if (parts.Length >= 2)
                {
                    module = parts[parts.Length - 2]; // lấy phần tử áp cuối
                }
                else
                {
                    module = "Unknown";
                }

                // Bước 4: Tạo thư mục con theo module (tự tạo nếu chưa có)
                // VD: Reports/Screenshots/Auth/
                string moduleDir = Path.Combine(ScreenshotDir, module);
                Directory.CreateDirectory(moduleDir);

                // Bước 5: Tạo tên file theo tên test + thời gian
                string tenTest = TestContext.CurrentContext.Test.MethodName;
                if (tenTest == null)
                {
                    tenTest = "UnknownTest";
                }

                string thoiGian = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string tenFile = tenTest + "_" + thoiGian + ".png";
                string duongDan = Path.Combine(moduleDir, tenFile);

                // Bước 6: Chụp và lưu ảnh
                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                screenshot.SaveAsFile(duongDan);

                TestContext.AddTestAttachment(duongDan, "Screenshot khi lỗi");
                TestContext.WriteLine("Screenshot đã lưu: " + duongDan);

                // Trả về đường dẫn để TearDown dùng khi ghi vào Excel
                return duongDan;
            }
            catch (Exception ex)
            {
                TestContext.WriteLine("Không thể chụp màn hình: " + ex.Message);

                // Trả về rỗng nếu chụp thất bại
                return "";
            }
        }
    }
}
