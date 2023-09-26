using FileHandling.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using static System.Net.Mime.MediaTypeNames;

namespace FileHandling.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FilesController : Controller
	{
        private readonly IWebHostEnvironment webHostEnvironment;

        public FilesController(IWebHostEnvironment webHostEnvironment)
		{
            this.webHostEnvironment = webHostEnvironment;
        }

		//-------Upload Single File-------//
		[HttpPost("UploadSingleFile")]
		public async Task<IActionResult> UploadSingleFile(IFormFile formFile, string fileName)
		{
			try
			{
				string folderPath = GetFolderPath();
                string BaseURL = GetBaseURL();
                if (!System.IO.Directory.Exists(folderPath))
				{
					System.IO.Directory.CreateDirectory(folderPath);

                }
				string filePath = folderPath + $"{fileName}{Path.GetExtension(formFile.FileName)}";
				using (FileStream fileStream = System.IO.File.Create(filePath))
				{
                    await formFile.CopyToAsync(fileStream);
                }

				return Ok(new
				{
					FileName = fileName + Path.GetExtension(formFile.FileName),
					FilePath = filePath,
					FileURL = BaseURL + fileName + Path.GetExtension(formFile.FileName)
                });
			}
			catch
			{
				return BadRequest("Something Went Wrong");
			}
		}

        //-------Upload Multi Files-------//
        [HttpPost("UploadMultiFiles")]
        public async Task<IActionResult> UploadMultiFiles(IFormFileCollection formFiles)
        {
            List<FilesDto> uploadedFiles = new List<FilesDto>();
            string folderPath = GetFolderPath();
            string BaseURL = GetBaseURL();
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);

            }
            foreach (var formFile in formFiles)
			{
                Guid fileName = Guid.NewGuid();
                string filePath = folderPath + $"{fileName}{Path.GetExtension(formFile.FileName)}";
                using (FileStream fileStream = System.IO.File.Create(filePath))
                {
                    await formFile.CopyToAsync(fileStream);
                    uploadedFiles.Add(new FilesDto
                    {
                        FileName = fileName + Path.GetExtension(formFile.FileName),
                        FilePath = filePath,
                        FileURL = BaseURL + fileName + Path.GetExtension(formFile.FileName)
                    });
                }

            }

            return Ok(uploadedFiles);
        }

        //-------Get Single File Info-------//
        [HttpGet("GetSingleFileInfo")]
        public IActionResult GetSingleInfoFile([FromQuery] string fileName, [FromQuery] string fileType)
        {
            try
            {
                string BaseURL = GetBaseURL();
                string folderPath = GetFolderPath();
                var filePath = folderPath + $"{fileName}.{fileType}";
                if (System.IO.File.Exists(filePath))
                {
                    return Ok(new FilesDto
                    {
                        FileName = $"{fileName}.{fileType}",
                        FilePath = filePath,
                        FileURL = BaseURL + fileName + "." + fileType
                    });
                }
                else
                {
                    return NotFound("File Not Found");
                }
            }
            catch
            {
                return BadRequest("Something Went Wrong");
            }
        }

        //-------Get Multi File Info-------//
        [HttpPost("GetMultFileInfo")]
        public IActionResult GetMultiInfoFile([FromBody] List<FileNameDto> files)
        {
            List<FilesDto> filesInfo = new List<FilesDto>();
            foreach (var file in files)
            {
                string BaseURL = GetBaseURL();
                string folderPath = GetFolderPath();
                var filePath = folderPath + $"{file.name}.{file.type}";
                if (System.IO.File.Exists(filePath))
                {
                    filesInfo.Add(new FilesDto
                    {
                        FileName = $"{file.name}.{file.type}",
                        FilePath = filePath,
                        FileURL = BaseURL + file.name + "." + file.type
                    });
                }
                else
                {
                    filesInfo.Add(new FilesDto
                    {
                        FileName = $"{file.name}.{file.type}",
                        FilePath = "Not Found",
                        FileURL = "Not Found"
                    });
                }
            }
            return Ok(filesInfo);
        }

        //-------Get Multi Files By Folder Name-------//
        [HttpGet("GetMultFilesByFolderName")]
        public IActionResult GetMultFilesByFolderName([FromQuery] string folderName)
        {
            string path = webHostEnvironment.WebRootPath + "/" + folderName;

            if (System.IO.Directory.Exists(path))
            {
                string BaseURL = GetBaseURL();
                List<FilesDto> files = new List<FilesDto>();
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                FileInfo[] filesInfo = directoryInfo.GetFiles();
                foreach (FileInfo fileInfo in filesInfo)
                {
                    files.Add(new FilesDto
                    {
                        FileName = $"{fileInfo.Name}",
                        FilePath = path + "/" + $"{fileInfo.Name}",
                        FileURL = BaseURL + $"{fileInfo.Name}"
                    });
                }
                return Ok(files);
            }
            else
            {
                return NotFound("Invalid Path");
            }
        }


        //-------Download File-------//
        [HttpGet("DownloadFile")]
        public async Task<IActionResult> DownloadFile([FromQuery] string fileName, [FromQuery] string fileType)
        {
            string folderPath = GetFolderPath();
            var filePath = folderPath + $"{fileName}.{fileType}";
            if (System.IO.File.Exists(filePath))
            {
                MemoryStream memory = new MemoryStream();
                using (FileStream file = new FileStream(filePath, FileMode.Open))
                {
                    await file.CopyToAsync(memory);
                }
                memory.Position = 0;
                return File(memory, $"image/{fileType}", fileName + "." + fileType);
            }
            else
            {
                return NotFound("File Not Found");
            }

        }

        //-------Remove Single File-------//
        [HttpDelete("Remove")]
        public IActionResult Remove([FromQuery] string fileName, [FromQuery] string fileType)
        {
            try
            {
                string folderPath = GetFolderPath();
                var filePath = folderPath + $"{fileName}.{fileType}";
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Ok("Done!");
                }
                else
                {
                    return NotFound("File Not Found");
                }
            }
            catch
            {
                return BadRequest("Something Went Wrong");
            }
        }

        //-------Remove Multi File-------//
        [HttpDelete("RemoveMultiFile")]
        public IActionResult RemoveMultiFile([FromQuery] string folderName)
        {
            string path = webHostEnvironment.WebRootPath + "/" + folderName;

            if (System.IO.Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                FileInfo[] filesInfo = directoryInfo.GetFiles();
                foreach (FileInfo fileInfo in filesInfo)
                {
                    fileInfo.Delete();
                }
                return Ok("File Not Found");
            }
            else
            {
                return NotFound("Invalid Path");
            }
        }

        //------------------------------------------------------------------------------------------------------------//
        [NonAction]
		private string GetFolderPath()
		{
			return webHostEnvironment.WebRootPath + "/Upload/Files/";
		}

        [NonAction]
        private string GetBaseURL()
        {
            return $"{Request.Scheme}://{Request.Host}{Request.PathBase}/Upload/Files/";
        }
    }
}

