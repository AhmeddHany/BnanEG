using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Bnan.Inferastructure.Extensions
{
    public static class FileExtensions
    {

        public static async Task<string> SaveImageAsync(this IFormFile file, IWebHostEnvironment webHostEnvironment, string folderName, string fileName, string extension)
        {
            string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, folderName);
            string filePath = Path.Combine(uploadsFolder, fileName + extension);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Check if the file exists and delete it
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            string virtualPath = filePath.Replace(webHostEnvironment.WebRootPath, "~").Replace("\\", "/");
            return virtualPath;
        }
        public static async Task<string> SaveImageAsync(this IFormFile file, IWebHostEnvironment webHostEnvironment, string folderName, string fileName, string extension, string oldPath)
        {
            string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, folderName);
            string filePath = Path.Combine(uploadsFolder, fileName + extension);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            if (!string.IsNullOrEmpty(oldPath))
            {
                string oldPathPysical = MapPath(webHostEnvironment, oldPath);
                // Check if the file exists and delete it
                if (File.Exists(oldPathPysical))
                {
                    File.Delete(oldPathPysical);
                }
            }


            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            string virtualPath = filePath.Replace(webHostEnvironment.WebRootPath, "~").Replace("\\", "/");
            return virtualPath;
        }
        public static async Task<bool?> RemoveFile(this IWebHostEnvironment webHostEnvironment, string folderName, string fileName, string extension)
        {
            string imagePath = Path.Combine(webHostEnvironment.WebRootPath, folderName, fileName + extension);

            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
                return true;
            }
            return false;
        }
        public static async Task<string?> SavePdf(this IWebHostEnvironment webHostEnvironment, string pdfFile, string lessor, string branch, string PdfNo, string Type)
        {

            byte[] pdfBytes = Convert.FromBase64String(pdfFile);
            // Define the base path for saving PDFs
            string basePath = Path.Combine(webHostEnvironment.WebRootPath, "images", "Company", lessor, "Branches", branch);

            // Define the subfolder based on the document type and language
            // Define the subfolder based on the document type
            string subfolder = Type switch
            {
                "Receipt" => "Receipt", // Always use "Receipts" regardless of language
                "BnanContract" => "Bnan Contract",
                "TaxInvoice" => "Tax Invoice",
                "ProformaInvoice" => "Proforma Invoice",
                _ => throw new ArgumentException("Invalid document type", nameof(Type))
            };

            // Construct the full file path
            string fullPath = Path.Combine(basePath, subfolder);

            // Ensure the directory exists
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            string filePath = Path.Combine(fullPath, $"{PdfNo}.pdf");
            if (File.Exists(filePath)) File.Delete(filePath);
            // Write the byte array to the file asynchronously
            await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

            // Construct the virtual path and return it
            string virtualPath = $"~{filePath.Replace(webHostEnvironment.WebRootPath, string.Empty).Replace("\\", "/")}";
            return virtualPath;
        }
        public static string CleanAndCheckBase64StringPdf(string base64)
        {
            // Check if the input is null or empty
            if (string.IsNullOrEmpty(base64)) return null;

            // Handle data URL scheme (e.g., "data:application/pdf;base64,")
            if (base64.Contains(","))
            {
                base64 = base64.Split(',')[1];
            }

            // Remove all non-base64 characters
            base64 = Regex.Replace(base64, @"[^A-Za-z0-9+/=]", string.Empty);

            // Fix padding
            int paddingCount = base64.Count(c => c == '=');
            if (paddingCount > 2)
            {
                base64 = base64.TrimEnd('=').PadRight(base64.Length - paddingCount + 2, '=');
            }

            // Ensure the base64 string length is a multiple of 4
            if (base64.Length % 4 != 0)
            {
                base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
            }

            // Validate the base64 string
            return IsValidBase64(base64) ? base64 : null;
        }
        private static bool IsValidBase64(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
        public static async Task<string?> SaveSigntureImage(this IWebHostEnvironment webHostEnvironment, string img, string RenterId, string oldPath, string folderName)
        {
            byte[] imgBytes = Convert.FromBase64String(img.Split(",")[1]); // Split to remove the prefix "data:application/pdf;base64,"
            string wwwrootPath = webHostEnvironment.WebRootPath;
            string fileNameImg = "Signture_" + DateTime.Now.ToString("yyyyMMddHHmmss"); // اسم مبني على التاريخ والوقت
            string fullPath = Path.Combine(wwwrootPath, folderName);
            // Ensure the directory exists
            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
            string filePath = Path.Combine(fullPath, fileNameImg + ".png");

            // Write the byte array to a file
            try
            {
                if (!string.IsNullOrEmpty(oldPath))
                {
                    string oldPathPysical = MapPath(webHostEnvironment, oldPath);
                    // Check if the file exists and delete it
                    if (File.Exists(oldPathPysical))
                    {
                        File.Delete(oldPathPysical);
                    }
                }

                await System.IO.File.WriteAllBytesAsync(filePath, imgBytes);
                // Set isTrue to true if the file write operation is successful
                bool isTrue = true;

                if (isTrue)
                {
                    string virtualPath = $"~{filePath.Replace(webHostEnvironment.WebRootPath, string.Empty).Replace("\\", "/")}";
                    return virtualPath;
                }
            }
            catch (Exception ex)
            {
                // Handle the exception here if needed
                // For now, let's throw it again
                throw;
            }

            // If the file write operation was unsuccessful, return null
            return null;
        }
        public static async Task<bool?> RemoveFile(this IWebHostEnvironment webHostEnvironment, string path)
        {
            var pathA = MapPath(webHostEnvironment, path);
            string imagePath = Path.Combine(webHostEnvironment.WebRootPath, pathA);

            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
                return true;
            }
            return false;
        }
        public static string MapPath(this IWebHostEnvironment webHostEnvironment, string virtualPath)
        {
            // Remove the leading "~/" if present
            virtualPath = virtualPath.TrimStart('~', '/');

            // Combine the web root path with the virtual path
            string fullPath = Path.Combine(webHostEnvironment.WebRootPath, virtualPath.Replace('/', Path.DirectorySeparatorChar));

            return fullPath;
        }
        public static async Task<bool?> CreateFolderLessor(IWebHostEnvironment webHostEnvironment, string lessorCode)
        {
            var directoryPaths = new List<string>
                    {
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Support Images"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Contract Company"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Users"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Users","CAS"+lessorCode),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches", "100"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches", "100", "Arabic Contract"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches", "100", "English Contract"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches", "100", "Arabic Receipt"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches", "100", "English Receipt"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches", "100", "Arabic Invoice"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches", "100", "English Invoice"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches", "100", "Documentions")
                    };

            foreach (var path in directoryPaths)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }

            return true;
        }
        public static async Task<bool?> CreateFolderBranch(IWebHostEnvironment webHostEnvironment, string lessorCode, string branchCode)
        {
            var directoryPaths = new List<string>
                    {
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches",branchCode),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches",branchCode, "Arabic Contract"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches",branchCode, "English Contract"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches",branchCode, "Arabic Receipt"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches",branchCode, "English Receipt"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches",branchCode, "Arabic Invoice"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches",branchCode, "English Invoice"),
                        GetDirectoryPath(webHostEnvironment.WebRootPath,"images","Company",lessorCode, "Branches",branchCode, "Documentions")
                    };

            foreach (var path in directoryPaths)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }

            return true;
        }
        private static string GetDirectoryPath(params string[] parts)
        {
            return Path.Combine(parts);
        }

    }
}
