using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ChunkUploading.Controllers
{
    public partial class FileUploaderController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        public class ChunkMetadata
        {
            public int Index { get; set; }
            public int TotalCount { get; set; }
            public int FileSize { get; set; }
            public string FileName { get; set; }
            public string FileType { get; set; }
            public string FileGuid { get; set; }
        }

        public ActionResult ChunkUploading()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadChunk(IFormFile file, string chunkMetadata)
        {
            var tempPath = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
            // Removes temporary files
            //RemoveTempFilesAfterDelay(tempPath, new TimeSpan(0, 5, 0));

            try
            {
                if (!string.IsNullOrEmpty(chunkMetadata))
                {
                    var metaDataObject = JsonConvert.DeserializeObject<ChunkMetadata>(chunkMetadata);
                    CheckFileExtensionValid(metaDataObject.FileName);

                    // Uncomment to save the file
                    //var tempFilePath = Path.Combine(tempPath, metaDataObject.FileGuid + ".tmp");
                    //if(!Directory.Exists(tempPath))
                    //    Directory.CreateDirectory(tempPath);

                    //AppendContentToFile(tempFilePath, file);

                    //if(metaDataObject.Index == (metaDataObject.TotalCount - 1))
                    //    ProcessUploadedFile(tempFilePath, metaDataObject.FileName);
                }
            }
            catch
            {
                return BadRequest();
            }
            return Ok();
        }
        void RemoveTempFilesAfterDelay(string path, TimeSpan delay)
        {
            var dir = new DirectoryInfo(path);
            if (dir.Exists)
                foreach (var file in dir.GetFiles("*.tmp").Where(f => f.LastWriteTimeUtc.Add(delay) < DateTime.UtcNow))
                    file.Delete();
        }
        void CheckFileExtensionValid(string fileName)
        {
            fileName = fileName.ToLower();
            string[] imageExtensions = { ".jpg", ".jpeg", ".gif", ".png" };

            var isValidExtenstion = imageExtensions.Any(ext => {
                return fileName.LastIndexOf(ext) > -1;
            });
            if (!isValidExtenstion)
                throw new Exception("Not allowed file extension");
        }
        void CheckMaxFileSize(FileStream stream)
        {
            if (stream.Length > 4000000)
                throw new Exception("File is too large");
        }
        void ProcessUploadedFile(string tempFilePath, string fileName)
        {
            // Check if the uploaded file is a valid image
            var path = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
            System.IO.File.Copy(tempFilePath, Path.Combine(path, fileName));
        }
        void AppendContentToFile(string path, IFormFile content)
        {
            using (var stream = new FileStream(path, FileMode.Append, FileAccess.Write))
            {
                content.CopyTo(stream);
                CheckMaxFileSize(stream);
            }
        }
    }
}
