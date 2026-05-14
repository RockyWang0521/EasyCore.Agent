namespace AspCoreAgent
{
    public class ChatWithImageRequest
    {
        public string Message { get; set; } = string.Empty;

        public string? SessionId { get; set; }

        public IFormFile Image { get; set; } = default!;
    }
}
