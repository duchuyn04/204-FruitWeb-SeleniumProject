using System.IO;

namespace SeleniumProject.Utilities
{
    // FileHelper — hỗ trợ các thao tác liên quan đến file trong test upload
    public static class FileHelper
    {
        // Thư mục chứa các file ảnh test có sẵn trong project
        // Ví dụ: valid_small.jpg, invalid_format.pdf, invalid_format.exe
        private static readonly string ImagesDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "TestData", "Images"
        );

        // Lấy đường dẫn tuyệt đối của một file ảnh trong thư mục TestData/Images/
        // Ví dụ: GetImagePath("valid_small.jpg") → "C:\...\TestData\Images\valid_small.jpg"
        public static string GetImagePath(string fileName)
        {
            string fullPath = Path.Combine(ImagesDir, fileName);
            return fullPath;
        }

        // Sinh ra một file tạm thời với kích thước chính xác theo byte
        // Dùng cho test boundary: generate:5242880 = 5MB
        // File được lưu vào thư mục Temp của hệ điều hành
        // Gọi DeleteTempFile() sau khi test xong để dọn dẹp
        public static string GenerateTempFile(int sizeInBytes, string extension = ".jpg")
        {
            // Tạo tên file ngẫu nhiên để tránh trùng khi chạy nhiều test cùng lúc
            string fileName = "test_" + Guid.NewGuid().ToString("N") + extension;
            string filePath = Path.Combine(Path.GetTempPath(), fileName);

            // Tạo mảng byte với kích thước đúng bằng sizeInBytes
            byte[] data = new byte[sizeInBytes];

            // Điền dữ liệu ngẫu nhiên vào (tránh file toàn số 0 bị từ chối)
            Random random = new Random();
            random.NextBytes(data);

            // Ghi ra đĩa
            File.WriteAllBytes(filePath, data);

            return filePath;
        }

        // Xóa file tạm sau khi test xong
        // Gọi trong [TearDown] để dọn sạch
        public static void DeleteTempFile(string filePath)
        {
            // Chỉ xóa nếu file thực sự tồn tại
            bool fileExists = File.Exists(filePath);
            if (fileExists)
            {
                File.Delete(filePath);
            }
        }

        // Phân tích giá trị imagePath trong JSON:
        // - "valid_small.jpg"    → trả về đường dẫn từ TestData/Images/
        // - "generate:5242880"  → sinh file tạm 5MB, trả về đường dẫn
        // - ""                  → trả về null (không cần upload)
        public static string ResolveImagePath(string imagePath)
        {
            // Trường hợp 1: không cần upload
            if (string.IsNullOrEmpty(imagePath))
            {
                return null;
            }

            // Trường hợp 2: sinh file theo kích thước
            // Định dạng: "generate:SỐ_BYTES" ví dụ "generate:5242880"
            bool isGenerate = imagePath.StartsWith("generate:");
            if (isGenerate)
            {
                // Tách lấy phần số sau dấu ":"
                string sizeStr = imagePath.Substring("generate:".Length);

                int sizeInBytes;
                bool parsed = int.TryParse(sizeStr, out sizeInBytes);

                if (parsed)
                {
                    return GenerateTempFile(sizeInBytes);
                }
                else
                {
                    return null;
                }
            }

            // Trường hợp 3: file có sẵn trong TestData/Images/
            return GetImagePath(imagePath);
        }
    }
}
