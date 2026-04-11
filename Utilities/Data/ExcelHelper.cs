using ClosedXML.Excel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

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

        // Ghi kết quả vào đúng hàng trong sheet chỉ định của file Excel (dùng NPOI).
        // Tìm hàng có Test Case ID (cột C = index 2) khớp với testCaseId.
        // Ghi vào:
        //   Cột J (index 9):  Actual Result
        //   Cột K (index 10): Testscripts (tên method)
        //   Cột L (index 11): Result (Passed/Failed)
        //   Cột M (index 12): Screenshot — nhúng ảnh thật vào ô (NPOI)
        public void GhiKetQuaVaoSheet(
            string testCaseId,
            string tenMethod,
            bool isPassed,
            string actualResult,
            string duongDanScreenshot = "",
            string sheetName = "")
        {
            if (!File.Exists(_filePath)) return;
            if (string.IsNullOrEmpty(testCaseId)) return;
            if (string.IsNullOrEmpty(sheetName)) return;

            lock (_lock)
            {
                // Đọc file vào bộ nhớ để NPOI có thể ghi đè 
                XSSFWorkbook workbook;
                using (var readStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
                    workbook = new XSSFWorkbook(readStream);

                var sheet = workbook.GetSheet(sheetName);
                if (sheet == null)
                {
                    workbook.Close();
                    return;
                }

                // Tìm hàng có TestCase ID trong cột C (index 2)
                int dongTimThay = -1;
                for (int i = 0; i <= sheet.LastRowNum; i++)
                {
                    var r = sheet.GetRow(i);
                    if (r == null) continue;
                    var c = r.GetCell(2); // cột C
                    if (c == null) continue;

                    string cellText = "";
                    if (c.CellType == CellType.Formula)
                    {
                        if (c.CachedFormulaResultType == CellType.String)
                            cellText = c.StringCellValue;
                        else
                            cellText = c.ToString();
                    }
                    else if (c.CellType == CellType.String)
                    {
                        cellText = c.StringCellValue;
                    }
                    else
                    {
                        cellText = c.ToString();
                    }

                    if (cellText?.Trim() == testCaseId)
                    {
                        dongTimThay = i; // 0-based
                        break;
                    }
                }

                if (dongTimThay == -1)
                {
                    TestContext.WriteLine($"[EXCEL TRACE] FAILED to find {testCaseId} in sheet {sheetName}");
                    workbook.Close();
                    return;
                }

                var dongGhi = sheet.GetRow(dongTimThay) ?? sheet.CreateRow(dongTimThay);

                // Tạo style chung
                ICellStyle stylePass = workbook.CreateCellStyle();
                stylePass.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;
                stylePass.FillPattern = FillPattern.SolidForeground;
                stylePass.Alignment = HorizontalAlignment.Center;
                stylePass.VerticalAlignment = VerticalAlignment.Center;
                stylePass.WrapText = true;

                IFont fontPass = workbook.CreateFont();
                fontPass.IsBold = true;
                fontPass.Color = NPOI.HSSF.Util.HSSFColor.Black.Index; // ← BẮT BUỘC: đặt màu chữ đen
                stylePass.SetFont(fontPass);

                ICellStyle styleFail = workbook.CreateCellStyle();
                styleFail.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Red.Index;
                styleFail.FillPattern = FillPattern.SolidForeground;
                styleFail.Alignment = HorizontalAlignment.Center;
                styleFail.VerticalAlignment = VerticalAlignment.Center;
                styleFail.WrapText = true;

                IFont fontFail = workbook.CreateFont();
                fontFail.IsBold = true;
                fontFail.Color = NPOI.HSSF.Util.HSSFColor.White.Index; // chữ trắng nổi trên nền đỏ
                styleFail.SetFont(fontFail);

                // Ghi Actual Result (cột J = index 9)
                var cellActual = dongGhi.GetCell(9) ?? dongGhi.CreateCell(9);
                cellActual.SetCellType(CellType.String);
                cellActual.SetCellValue(actualResult);

                // Ghi Testscripts — tên method (cột K = index 10)
                var cellScript = dongGhi.GetCell(10) ?? dongGhi.CreateCell(10);
                cellScript.SetCellType(CellType.String);
                cellScript.SetCellValue(tenMethod);

                // Ghi Result (cột L = index 11)
                var cellResult = dongGhi.GetCell(11) ?? dongGhi.CreateCell(11);
                cellResult.SetCellType(CellType.String); // ← reset type về String trước khi ghi
                cellResult.SetCellValue(isPassed ? "Passed" : "Failed");
                cellResult.CellStyle = isPassed ? stylePass : styleFail;


                // Xóa text ô M (cột 12) — dù pass hay fail để không còn text cũ
                var cellM = dongGhi.GetCell(12) ?? dongGhi.CreateCell(12);
                cellM.SetCellValue("");

                // Nhúng ảnh vào cột M (index 12) — CHỈ khi Failed
                // Ảnh mới sẽ overlay lên ảnh cũ (nằm trên cùng = ảnh mới nhất)
                if (!isPassed && !string.IsNullOrEmpty(duongDanScreenshot) && File.Exists(duongDanScreenshot))
                {
                    // Đặt chiều cao hàng đủ lớn để thấy ảnh
                    dongGhi.HeightInPoints = 90;

                    // Load ảnh PNG vào workbook
                    byte[] anhBytes = File.ReadAllBytes(duongDanScreenshot);
                    int pictureIdx = workbook.AddPicture(anhBytes, PictureType.PNG);

                    // Tạo anchor: Col1=12(M), Row1=dongTimThay → Col2=13(N), Row2=dongTimThay+1
                    IDrawing drawingObj = sheet.CreateDrawingPatriarch();
                    ICreationHelper helper = workbook.GetCreationHelper();
                    IClientAnchor anchor = helper.CreateClientAnchor();
                    anchor.Col1 = 12;
                    anchor.Row1 = dongTimThay;
                    anchor.Col2 = 13;
                    anchor.Row2 = dongTimThay + 1;
                    anchor.AnchorType = AnchorType.MoveAndResize;

                    drawingObj.CreatePicture(anchor, pictureIdx);
                }
                else if (isPassed)
                {
                    // Passed → reset chiều cao hàng về bình thường
                    dongGhi.HeightInPoints = 15;
                }


                // Lưu file
                using (var writeStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                    workbook.Write(writeStream);

                workbook.Close();
            }
        }
    }
}
