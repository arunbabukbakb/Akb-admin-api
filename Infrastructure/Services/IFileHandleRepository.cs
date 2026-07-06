using Models.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
	public interface IFileHandleRepository
	{
        Task<string> UploadFileAsync(IFormFile file,string filename,string path, bool thumb = false);
		Task<IEnumerable<FileUploadResponse>> UploadFilesAsync(IEnumerable<IFormFile> files, string path, bool thumb = false);
		public void RemoveFile(string? fileName, string path, bool thumb = false);
	}
}
