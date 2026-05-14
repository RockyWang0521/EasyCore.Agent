using AspCoreAgent;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/yolo")]
public class YoloController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly YoloOnnxDetector _detector;

    public YoloController(
        IWebHostEnvironment env,
        YoloOnnxDetector detector)
    {
        _env = env;
        _detector = detector;
    }

    [HttpPost("detect")]
    public async Task<IActionResult> Detect(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("请上传图片");

        var webRoot = _env.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            Directory.CreateDirectory(webRoot);
        }

        var uploadDir = Path.Combine(webRoot, "uploads");
        var resultDir = Path.Combine(webRoot, "results");

        Directory.CreateDirectory(uploadDir);
        Directory.CreateDirectory(resultDir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{ext}";

        var uploadPath = Path.Combine(uploadDir, fileName);

        await using (var stream = System.IO.File.Create(uploadPath))
        {
            await file.CopyToAsync(stream);
        }

        var resultFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_result.jpg";
        var resultPath = Path.Combine(resultDir, resultFileName);

        var detections = _detector.DetectAndDraw(
            imagePath: uploadPath,
            outputPath: resultPath,
            confThreshold: 0.35f,
            iouThreshold: 0.45f);

        var downloadUrl = $"{Request.Scheme}://{Request.Host}/api/yolo/download/{resultFileName}";

        //return Ok(new
        //{
        //    hasDamage = detections.Count > 0,
        //    count = detections.Count,
        //    detections,
        //    downloadUrl
        //});

        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        var path = Path.Combine(webRoot, "results", resultFileName);

        if (!System.IO.File.Exists(path))
            return NotFound("文件不存在");

        return PhysicalFile(path, "image/jpeg", fileName);
    }

    [HttpGet("download/{fileName}")]
    public IActionResult Download(string fileName)
    {
        var webRoot = _env.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        var path = Path.Combine(webRoot, "results", fileName);

        if (!System.IO.File.Exists(path))
            return NotFound("文件不存在");

        return PhysicalFile(path, "image/jpeg", fileName);
    }
}