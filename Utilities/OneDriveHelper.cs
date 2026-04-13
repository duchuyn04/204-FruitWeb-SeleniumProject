using Microsoft.Extensions.Configuration;

namespace SeleniumProject.Utilities
{
    /// <summary>
    /// Helper để sync file Excel từ local sang OneDrive
    /// Tránh conflict khi OneDrive đang sync
    /// </summary>
    public static class OneDriveHelper
    {
        /// <summary>
        /// Copy file Excel từ local sang OneDrive
        /// Đợi OneDrive release file lock trước khi copy
        /// </summary>
        public static void SyncToOneDrive(IConfiguration config)
        {
            string? localPath = config["ReportExcelPath"];
            string? oneDrivePath = config["OneDriveExcelPath"];

            if (string.IsNullOrEmpty(localPath) || string.IsNullOrEmpty(oneDrivePath))
            {
                TestContext.WriteLine("[OneDrive] Không có cấu hình OneDriveExcelPath, bỏ qua sync");
                return;
            }

            // Chuyển sang absolute path nếu là relative
            if (!Path.IsPathRooted(localPath))
            {
                localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, localPath);
            }

            if (!File.Exists(localPath))
            {
                TestContext.WriteLine($"[OneDrive] File local không tồn tại: {localPath}");
                return;
            }

            try
            {
                // Tạo thư mục OneDrive nếu chưa có
                string? oneDriveDir = Path.GetDirectoryName(oneDrivePath);
                if (!string.IsNullOrEmpty(oneDriveDir))
                {
                    Directory.CreateDirectory(oneDriveDir);
                }

                // Đợi OneDrive release file lock (retry 3 lần)
                int maxRetries = 3;
                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        // Copy file với overwrite
                        File.Copy(localPath, oneDrivePath, overwrite: true);
                        
                        TestContext.WriteLine($"[OneDrive] ✓ Đã sync file sang OneDrive: {oneDrivePath}");
                        
                        // Copy thư mục Screenshots nếu có
                        CopyScreenshotsFolder(localPath, oneDrivePath);
                        
                        return;
                    }
                    catch (IOException ex) when (i < maxRetries - 1)
                    {
                        // File đang bị lock bởi OneDrive, đợi 2 giây rồi thử lại
                        TestContext.WriteLine($"[OneDrive] File đang bị lock, thử lại lần {i + 2}...");
                        Thread.Sleep(2000);
                    }
                }

                TestContext.WriteLine("[OneDrive] ✗ Không thể sync sau 3 lần thử, OneDrive đang lock file");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[OneDrive] Lỗi khi sync: {ex.Message}");
            }
        }

        /// <summary>
        /// Copy thư mục Screenshots từ local sang OneDrive
        /// </summary>
        private static void CopyScreenshotsFolder(string localExcelPath, string oneDriveExcelPath)
        {
            try
            {
                // Tìm thư mục Screenshots bên cạnh file Excel local
                string? localDir = Path.GetDirectoryName(localExcelPath);
                if (string.IsNullOrEmpty(localDir)) return;

                string localScreenshotsDir = Path.Combine(localDir, "Screenshots");
                if (!Directory.Exists(localScreenshotsDir)) return;

                // Tạo thư mục Screenshots bên cạnh file Excel OneDrive
                string? oneDriveDir = Path.GetDirectoryName(oneDriveExcelPath);
                if (string.IsNullOrEmpty(oneDriveDir)) return;

                string oneDriveScreenshotsDir = Path.Combine(oneDriveDir, "Screenshots");
                Directory.CreateDirectory(oneDriveScreenshotsDir);

                // Copy tất cả file ảnh
                foreach (string sourceFile in Directory.GetFiles(localScreenshotsDir))
                {
                    string fileName = Path.GetFileName(sourceFile);
                    string destFile = Path.Combine(oneDriveScreenshotsDir, fileName);
                    File.Copy(sourceFile, destFile, overwrite: true);
                }

                TestContext.WriteLine($"[OneDrive] ✓ Đã sync thư mục Screenshots");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[OneDrive] Lỗi khi copy Screenshots: {ex.Message}");
            }
        }
    }
}
