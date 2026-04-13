using System.Net.Http.Headers;
using System.Text.Json;

namespace SeleniumProject.Utilities
{
    /// <summary>
    /// Helper để upload ảnh lên Imgur và lấy public link
    /// Không cần đăng nhập, ai cũng xem được
    /// </summary>
    public static class ImgurHelper
    {
        // Imgur Client ID (anonymous upload) - miễn phí, không cần tài khoản
        // Đây là Client ID public, có thể dùng cho anonymous upload
        private const string ClientId = "546c25a59c58ad7";
        private const string UploadUrl = "https://api.imgur.com/3/image";

        /// <summary>
        /// Upload ảnh lên Imgur và trả về public link
        /// </summary>
        /// <param name="imagePath">Đường dẫn file ảnh local</param>
        /// <returns>URL public của ảnh trên Imgur, hoặc null nếu thất bại</returns>
        public static async Task<string?> UploadImageAsync(string imagePath)
        {
            try
            {
                if (!File.Exists(imagePath))
                {
                    TestContext.WriteLine($"[Imgur] File không tồn tại: {imagePath}");
                    return null;
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", ClientId);

                // Đọc file ảnh thành base64
                byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
                string base64Image = Convert.ToBase64String(imageBytes);

                // Tạo form data
                var content = new MultipartFormDataContent
                {
                    { new StringContent(base64Image), "image" },
                    { new StringContent("base64"), "type" }
                };

                // Upload lên Imgur
                var response = await client.PostAsync(UploadUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    TestContext.WriteLine($"[Imgur] Upload thất bại: {response.StatusCode}");
                    TestContext.WriteLine($"[Imgur] Response: {responseBody}");
                    return null;
                }

                // Parse JSON response
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("data", out var data) && 
                    data.TryGetProperty("link", out var link))
                {
                    string imageUrl = link.GetString() ?? "";
                    TestContext.WriteLine($"[Imgur] ✓ Upload thành công: {imageUrl}");
                    return imageUrl;
                }

                TestContext.WriteLine($"[Imgur] Không tìm thấy link trong response");
                return null;
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[Imgur] Lỗi khi upload: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Upload ảnh đồng bộ (blocking)
        /// </summary>
        public static string? UploadImage(string imagePath)
        {
            return UploadImageAsync(imagePath).GetAwaiter().GetResult();
        }
    }
}
