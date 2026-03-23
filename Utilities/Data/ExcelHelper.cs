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
    }
}
