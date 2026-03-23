using System.Text.Json;

namespace SeleniumProject.Utilities
{
    // JsonHelper — đọc dữ liệu test từ file JSON
    // Dùng chung cho tất cả test class, không cần copy vào từng file
    public static class JsonHelper
    {
        // Đọc dữ liệu của 1 test case từ file JSON theo testCase ID
        // filePath  : đường dẫn tuyệt đối đến file JSON
        // testCaseId: mã test case, ví dụ "TC-AP-P01", "TC_LOGIN_01"
        public static Dictionary<string, string> DocDuLieu(string filePath, string testCaseId)
        {
            // Bước 1: Đọc toàn bộ nội dung file JSON thành text
            string jsonText = File.ReadAllText(filePath);

            // Bước 2: Phân tích JSON thành danh sách test cases
            // Dùng JsonElement thay vì string vì có field số nguyên như "no": 1
            List<Dictionary<string, JsonElement>> tatCaTestCase =
                JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(jsonText)!;

            // Bước 3: Duyệt từng test case để tìm đúng ID
            for (int i = 0; i < tatCaTestCase.Count; i++)
            {
                Dictionary<string, JsonElement> testCaseHienTai = tatCaTestCase[i];

                // Lấy ID của test case đang xét
                string idHienTai = testCaseHienTai["testCase"].GetString()!;

                // Nếu không đúng ID thì bỏ qua, xét tiếp
                if (idHienTai != testCaseId)
                {
                    continue;
                }

                // Bước 4: Tìm thấy đúng ID — chuyển sang Dictionary<string, string>
                Dictionary<string, string> ketQua = new Dictionary<string, string>();

                foreach (KeyValuePair<string, JsonElement> truong in testCaseHienTai)
                {
                    string tenTruong = truong.Key;
                    string giaTri = truong.Value.ToString();
                    ketQua[tenTruong] = giaTri;
                }

                return ketQua;
            }

            // Bước 5: Không tìm thấy — ném exception rõ ràng
            throw new KeyNotFoundException(
                $"Không tìm thấy test case '{testCaseId}' trong file: {filePath}"
            );
        }
    }
}
