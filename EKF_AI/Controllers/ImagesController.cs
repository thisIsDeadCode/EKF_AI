using EKF_AI.DataBase;
using EKF_AI.DataBase.Models;
using EKF_AI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

namespace EKF_AI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly ILogger<ImagesController> _logger;
        private readonly ApplicationContext _context;
        private readonly ClassificationService _classificationService;

        public ImagesController(ILogger<ImagesController> logger, ApplicationContext context, ClassificationService classificationService)
        {
            _logger = logger;
            _context = context;
            _classificationService = classificationService;
        }

        [HttpGet("get-all")]
        public async Task<IEnumerable<Image>> GetAll()
        {
            var result = _context.Images.ToList();
            await LoadBase64(result);
            return result;
        }

        [HttpGet("get-all-to-processing")]
        public async Task<IEnumerable<Image>> GetAllToProcessing()
        {
            var result = _context.Images.Where(x => x.HasProcessed == false).ToList();
            await LoadBase64(result);
            return result;
        }

        [HttpPost("load-possible-result")]
        public Image LoadPossibleResult([FromQuery] string imageId, [FromBody] List<string> annotations)
        {
            var image = _context.Images.FirstOrDefault(x => x.Id == imageId);

            if (image == null)
            {
                return null;
            }

            var results = _classificationService.GetElectricalElements(image.Id, image.Path, string.Join("\n", annotations), 40);

            LoadResult(imageId, "Перебор объектов на картинке", -1, string.Join("\n", results));

            return image;
        }



        [HttpPost("load-result")]
        public Image LoadResult([FromQuery] string imageId, [FromQuery] string name, [FromQuery] int precision, [FromQuery] string result)
        {
            var image = _context.Images.FirstOrDefault(x => x.Id == imageId);

            if (image == null)
            {
                return null;
            }

            image.HasProcessed = true;
            image.Name = name;
            image.Precision = precision;
            image.Result = result;

            _context.SaveChanges();

            return image;
        }

        private async Task LoadBase64(List<Image> images)
        {
            foreach (var image in images)
            {
                image.Base64 = await GetBase64(image.Path);
            }
        }


        private async Task<string> GetBase64(string path)
        {
            byte[] bytes = await System.IO.File.ReadAllBytesAsync(path);
            string file = Convert.ToBase64String(bytes);

            return file;
        }
    }
}
