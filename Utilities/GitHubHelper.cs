using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace SeleniumProject.Utilities
{
    /// <summary>
    /// Helper để upload ảnh lên GitHub repository
    /// Tạo public link để ai cũng xem được
    /// </summary>
    public static class GitHubHelper
    {
        private static readonly HttpClient _client = new HttpClient();

        /// <summary>
        /// Upload ảnh lên GitHub và trả về public raw link
        /// </summary>
        public static async Task<string?> UploadImageAsync(string imagePath, IConfiguration config)
        {
            try
            {
                // Đọc config
                string? token = config["GitHub:Token"];
                string? owner = config["GitHub:Owner"];
                string? repo = config["GitHub:Repo"];
                string? branch = config["GitHub:Branch"] ?? "main";

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
                {
                    TestContext.WriteLine("[GitHub] Thiếu config GitHub trong appsettings.json");
                    return null;
                }

                if (token == "YOUR_GITHUB_TOKEN_HERE")
                {
                    TestContext.WriteLine("[GitHub] Chưa cấu hình GitHub Token");
                    return null;
                }

                if (!File.Exists(imagePath))
                {
                    TestContext.WriteLine($"[GitHub] File không tồn tại: {imagePath}");
                    return null;
                }

                // Đọc file ảnh thành base64
                byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
                string base64Image = Convert.ToBase64String(imageBytes);

                // Tạo đường dẫn file trên GitHub: screenshots/filename.png
                string fileName = Path.GetFileName(imagePath);
                string githubPath = $"screenshots/{fileName}";

                // Setup HTTP client
                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _client.DefaultRequestHeaders.UserAgent.ParseAdd("SeleniumProject/1.0");

                // Upload lên GitHub
                string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/contents/{githubPath}";
                
                // Kiểm tra file đã tồn tại chưa (để lấy SHA nếu cần update)
                var checkResponse = await _client.GetAsync(apiUrl);
                string? sha = null;
                
                if (checkResponse.IsSuccessStatusCode)
                {
                    // File đã tồn tại, lấy SHA để update
                    string checkBody = await checkResponse.Content.ReadAsStringAsync();
                    using var checkDoc = JsonDocument.Parse(checkBody);
                    if (checkDoc.RootElement.TryGetProperty("sha", out var shaElement))
                    {
                        sha = shaElement.GetString();
                    }
                }
                
                // Tạo request body (thêm SHA nếu file đã tồn tại)
                object requestBody;
                if (sha != null)
                {
                    requestBody = new
                    {
                        message = $"Update screenshot: {fileName}",
                        content = base64Image,
                        branch = branch,
                        sha = sha
                    };
                }
                else
                {
                    requestBody = new
                    {
                        message = $"Upload screenshot: {fileName}",
                        content = base64Image,
                        branch = branch
                    };
                }

                string jsonBody = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await _client.PutAsync(apiUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    TestContext.WriteLine($"[GitHub] Upload thất bại: {response.StatusCode}");
                    TestContext.WriteLine($"[GitHub] Response: {responseBody}");
                    return null;
                }

                // Tạo raw link
                string rawUrl = $"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/{githubPath}";
                TestContext.WriteLine($"[GitHub] ✓ Upload thành công: {rawUrl}");
                return rawUrl;
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[GitHub] Lỗi khi upload: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Upload ảnh đồng bộ (blocking)
        /// </summary>
        public static string? UploadImage(string imagePath, IConfiguration config)
        {
            return UploadImageAsync(imagePath, config).GetAwaiter().GetResult();
        }
    }
}
