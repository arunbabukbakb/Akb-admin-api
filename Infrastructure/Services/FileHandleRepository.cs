
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.Extensions.Logging;
using Models.ViewModels;

namespace Infrastructure.Services
{
    public class FileHandleRepository : IFileHandleRepository
    {
        private readonly string _uploadFolderPath;
        private readonly ILogger<FileHandleRepository> _logger;

        public FileHandleRepository(IWebHostEnvironment webHostEnvironment, ILogger<FileHandleRepository> logger)
        {
            if (string.IsNullOrEmpty(webHostEnvironment.WebRootPath))
            {
                webHostEnvironment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }
            _uploadFolderPath = Path.Combine(webHostEnvironment.WebRootPath, "upload");
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string filename, string path, bool thumb = false)
        {
            try
            {

                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is missing or empty.");

                var uploadPath = Path.Combine(_uploadFolderPath, path);
                var thumbnailUploadPath = Path.Combine(_uploadFolderPath, path + "/thumb");

                // Create the Uploads folder if it doesn't exist
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                // Create the thumb Uploads folder if it doesn't exist
                if (!Directory.Exists(thumbnailUploadPath))
                {
                    Directory.CreateDirectory(thumbnailUploadPath);
                }
                var filePath = Path.Combine(uploadPath, filename);
                //thumb details
                var thumbUpPath = Path.Combine(thumbnailUploadPath, "thumb_" + filename);

                // Save the file to the specified path
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                //Thumbnail upload
                if (thumb == true)
                {
                    // Generate thumbnail
                    using (var image = Image.Load(filePath))
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(50),
                            Mode = ResizeMode.Max
                        })); ;

                        image.Save(thumbUpPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in ErrorController.");
            }

            return filename;
        }


        //Upload multiple files with thumbnail
        public async Task<IEnumerable<FileUploadResponse>> UploadFilesAsync(IEnumerable<IFormFile> files, string path, bool thumb = false)
        {
            if (files == null || !files.Any())
                throw new ArgumentException("No files provided.");

            var uploadPath = Path.Combine(_uploadFolderPath, path);
            var thumbnailUploadPath = Path.Combine(_uploadFolderPath, path + "/thumb");

            // Create the Uploads folder if it doesn't exist
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            // Create the Thumbnails folder if it doesn't exist
            if (!Directory.Exists(thumbnailUploadPath))
            {
                Directory.CreateDirectory(thumbnailUploadPath);
            }

            var uploadedFileNames = new List<FileUploadResponse>();

            foreach (var file in files)
            {
                var fileObject = new FileUploadResponse();

                if (file.Length == 0)
                {
                    // Log or handle empty files as needed
                    continue;
                }
                var firstname = Guid.NewGuid().ToString();
                var extension = System.IO.Path.GetExtension(file.FileName);

                var fileUpPath = Path.Combine(uploadPath, firstname + extension);
                fileObject.FileName = firstname + extension;
                var thumbUpPath = Path.Combine(thumbnailUploadPath, "thumb_" + firstname + extension);
                fileObject.ThumbName = "thumb_" + firstname + extension;

                // Save the file to the specified path
                using (var stream = new FileStream(fileUpPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                if (thumb == true)
                {
                    // Generate thumbnail
                    using (var image = Image.Load(fileUpPath))
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(50),
                            Mode = ResizeMode.Max
                        })); ;

                        image.Save(thumbUpPath);
                    }
                }

                uploadedFileNames.Add(fileObject);
            }

            return uploadedFileNames;
        }



        public void RemoveFile(string? fileName, string path, bool thumb = false)
        {
            var filePath = Path.Combine(_uploadFolderPath, path + (thumb == true ? "/thumb" : "") + "/" + (thumb == true ? "thumb_" : "") + fileName);

            // Check if the file exists before attempting to delete
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
