using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;

namespace SeleniumProject.Utilities
{
    public class ExcelHelper
    {
        private readonly string _filePath;

        public ExcelHelper(string filePath)
        {
            _filePath = filePath;
        }

        // Đọc toàn bộ dữ liệu từ sheet chỉ định trong file Excel.
        // Dòng đầu tiên được coi là tiêu đề cột (header).
        // Trả về danh sách Dictionary, mỗi phần tử là một dòng dữ liệu (key = tên cột).
        public List<Dictionary<string, string>> ReadSheet(string sheetName)
        {
            var result = new List<Dictionary<string, string>>();

            using var workbook = new XLWorkbook(_filePath);
            var sheet = workbook.Worksheet(sheetName);
            var rows = sheet.RangeUsed()?.RowsUsed().ToList();

            if (rows == null || rows.Count < 2) return result;

            // Lấy tên cột từ dòng đầu tiên (header)
            var headers = rows[0].Cells()
                .Select(c => c.GetValue<string>().Trim())
                .ToList();

            // Đọc dữ liệu từ dòng thứ 2 trở đi (bỏ qua dòng header)
            for (int i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var dict = new Dictionary<string, string>();

                for (int j = 0; j < headers.Count; j++)
                {
                    var cell = row.Cell(j + 1);
                    dict[headers[j]] = cell.GetValue<string>().Trim();
                }

                result.Add(dict);
            }

            return result;
        }

        // Đọc dữ liệu từ sheet và lọc theo giá trị cột "TestCase".
        // Dùng khi một sheet chứa data của nhiều test case khác nhau.
        public List<Dictionary<string, string>> ReadSheetByTestCase(string sheetName, string testCase)
        {
            return ReadSheet(sheetName)
                .Where(r => r.TryGetValue("TestCase", out var tc) && tc == testCase)
                .ToList();
        }

        // Ghi kết quả 1 test case vào sheet "KetQua" trong file Excel
        // Mỗi lần test chạy xong → thêm 1 dòng mới vào cuối sheet
        // Thread-safe: dùng lock để tránh đụng hàng khi chạy parallel
        private static readonly object _lock = new object();

        public void GhiKetQua(string testCaseId, string moTa, string trangThai, string ghiChu)
        {
            lock (_lock)
            {
                // Mở workbook nếu đã có, tạo mới nếu chưa có
                XLWorkbook workbook;
                if (File.Exists(_filePath))
                {
                    workbook = new XLWorkbook(_filePath);
                }
                else
                {
                    workbook = new XLWorkbook();
                }

                // Lấy sheet "KetQua" hoặc tạo mới nếu chưa có
                IXLWorksheet sheet;
                bool sheetMoi = !workbook.Worksheets.Any(w => w.Name == "KetQua");

                if (sheetMoi)
                {
                    sheet = workbook.Worksheets.Add("KetQua");

                    // Tạo dòng tiêu đề
                    sheet.Cell(1, 1).Value = "TestCase";
                    sheet.Cell(1, 2).Value = "Mô tả";
                    sheet.Cell(1, 3).Value = "Kết quả";
                    sheet.Cell(1, 4).Value = "Ghi chú";
                    sheet.Cell(1, 5).Value = "Thời gian";

                    // Định dạng tiêu đề: in đậm, nền xanh nhạt
                    IXLRange header = sheet.Range("A1:E1");
                    header.Style.Font.Bold = true;
                    header.Style.Fill.BackgroundColor = XLColor.LightBlue;
                }
                else
                {
                    sheet = workbook.Worksheet("KetQua");
                }

                // Tìm dòng trống tiếp theo để ghi
                int dongMoi;
                IXLRow dongCuoi = sheet.LastRowUsed();
                if (dongCuoi == null)
                {
                    dongMoi = 1;
                }
                else
                {
                    dongMoi = dongCuoi.RowNumber() + 1;
                }

                // Ghi dữ liệu vào dòng mới
                sheet.Cell(dongMoi, 1).Value = testCaseId;
                sheet.Cell(dongMoi, 2).Value = moTa;
                sheet.Cell(dongMoi, 3).Value = trangThai;
                sheet.Cell(dongMoi, 4).Value = ghiChu;
                sheet.Cell(dongMoi, 5).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                // Tô màu dòng theo kết quả: PASS = xanh lá, FAIL = đỏ
                IXLRange dongHienTai = sheet.Range(dongMoi, 1, dongMoi, 5);
                if (trangThai == "PASS")
                {
                    dongHienTai.Style.Fill.BackgroundColor = XLColor.LightGreen;
                }
                else if (trangThai == "FAIL")
                {
                    dongHienTai.Style.Fill.BackgroundColor = XLColor.LightSalmon;
                }

                // Tự động căn chỉnh độ rộng cột
                sheet.Columns().AdjustToContents();

                workbook.SaveAs(_filePath);
                workbook.Dispose();
            }
        }

        // Ghi kết quả vào đúng hàng trong sheet "TC_Product Management" của file 204.xlsx
        // Tìm hàng có Test Case ID (cột C) khớp với testCaseId
        // Sau đó ghi vào:
        //   Cột J (10): Actual Result
        //   Cột K (11): Testscripts (tên method)
        //   Cột L (12): Result (Passed/Failed)
        public void GhiKetQuaVaoSheet(
            string testCaseId,
            string tenMethod,
            bool isPassed,
            string actualResult,
            string duongDanScreenshot = "",
            string sheetName = "TC_Product Management")
        {
            if (!File.Exists(_filePath)) return;
            if (string.IsNullOrEmpty(testCaseId)) return;

            lock (_lock)
            {
                XLWorkbook workbook = new XLWorkbook(_filePath);

                // Kiểm tra sheet tồn tại không
                bool sheetTonTai = workbook.Worksheets.Any(w => w.Name == sheetName);
                if (!sheetTonTai)
                {
                    workbook.Dispose();
                    return;
                }

                IXLWorksheet sheet = workbook.Worksheet(sheetName);

                // Tìm hàng có Test Case ID trong cột C (3)
                // Lưu ý: các hàng bị merge — chỉ hàng đầu của nhóm có giá trị
                int dongTimThay = -1;

                foreach (IXLCell cell in sheet.Column(3).CellsUsed())
                {
                    if (cell.GetString().Trim() == testCaseId)
                    {
                        dongTimThay = cell.Address.RowNumber;
                        break;
                    }
                }

                // Không tìm thấy testCaseId trong sheet — bỏ qua
                if (dongTimThay == -1)
                {
                    workbook.Dispose();
                    return;
                }

                // Đọc Expected Result từ cột I (9) để điền vào Actual Result khi Pass
                string expectedResult = sheet.Cell(dongTimThay, 9).GetString().Trim();

                // Ghi Actual Result (cột J = 10)
                // Ghi đúng kết quả thực tế quan sát từ trình duyệt
                IXLCell cellActual = sheet.Cell(dongTimThay, 10);
                cellActual.Value = actualResult;

                // Ghi Testscripts — tên method tương ứng (cột K = 11)
                sheet.Cell(dongTimThay, 11).Value = tenMethod;

                // Ghi Result: Passed hoặc Failed (cột L = 12)
                IXLCell cellResult = sheet.Cell(dongTimThay, 12);
                cellResult.Value = isPassed ? "Passed" : "Failed";
                cellResult.Style.Font.Bold = true;

                // Tô màu ô Result
                if (isPassed)
                {
                    cellResult.Style.Fill.BackgroundColor = XLColor.LightGreen;
                }
                else
                {
                    cellResult.Style.Fill.BackgroundColor = XLColor.LightSalmon;
                }
                
                // Cột M (13): Screenshot — ghi đường dẫn ảnh thay vì nhúng ảnh trực tiếp
                // Lý do: ClosedXML.Drawings có bug với picture.Delete() và workbook.Save()
                // khi file đã có ảnh nhúng sẵn → dùng hyperlink đơn giản và ổn định hơn
                IXLCell cellScreenshot = sheet.Cell(dongTimThay, 13);
                if (!isPassed && !string.IsNullOrEmpty(duongDanScreenshot) && File.Exists(duongDanScreenshot))
                {
                    // Test FAIL: ghi đường dẫn ảnh dưới dạng hyperlink có thể click
                    string tenFile = Path.GetFileName(duongDanScreenshot);
                    cellScreenshot.Value = tenFile;
                    cellScreenshot.SetHyperlink(new XLHyperlink(duongDanScreenshot));
                    cellScreenshot.Style.Font.FontColor   = XLColor.Blue;
                    cellScreenshot.Style.Font.Underline   = XLFontUnderlineValues.Single;
                }
                else
                {
                    // Test PASS: xóa link cũ (nếu có từ lần fail trước)
                    cellScreenshot.Value = "";
                    cellScreenshot.SetHyperlink(null);
                    cellScreenshot.Style.Font.FontColor = XLColor.Black;
                }

                workbook.Save();
                workbook.Dispose();
            }
        }
    }
}
