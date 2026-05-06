using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private const string BasePrompt =
        "Create a hairstyle analysis graphic using this portrait. "
        + "Show side-by-side hairstyles comparisons to highlight which hairstyles suit the subject best. "
        + "Make it visual-first, with short labels only and no paragraphs give all below hair styles: "
        + "Short Hairstyles Buzz Cut Crew Cut Textured Crop Ivy League "
        + "Medium-Length Hairstyles Quiff Pompadour Side Part Bro Flow "
        + "Long Hairstyles Man Bun Shoulder-Length Hair Long Waves Half Bun "
        + "Fade & Undercut Styles Low Fade Mid Fade High Fade Undercut "
        + "Curly & Textured Styles Curly Fade Afro Curly Fringe Taper Fade "
        + "Trendy / Modern Styles Modern Mullet Two-Block Cut Wolf Cut Spiky Hair";

    private static readonly Dictionary<string, string> StylePrompts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["default"] = "",
        ["anime"] = " Render all hairstyles in Japanese anime art style with vibrant colors, large expressive eyes, and cel-shaded look.",
        ["pixar"] = " Render all hairstyles in Pixar 3D animation style with soft lighting, smooth skin, and a friendly cartoon look.",
        ["linkedin"] = " Render all hairstyles as clean, professional corporate headshots suitable for LinkedIn with studio lighting and neutral background.",
        ["cyberpunk"] = " Render all hairstyles in cyberpunk futuristic style with neon glow effects, vibrant hair colors, and a dark sci-fi atmosphere.",
        ["watercolor"] = " Render all hairstyles in delicate watercolor painting style with soft washes of color and artistic brush strokes.",
        ["retro"] = " Render all hairstyles in 1980s retro style with bold colors, vintage filters, and nostalgic aesthetic."
    };

    private readonly IConfiguration _config;

    public ImageController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(IFormFile image, [FromForm] string? style)
    {
        if (image == null)
            return BadRequest("Image missing");

        using var memoryStream = new MemoryStream();
        await image.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();

        var endpoint = _config["AzureOpenAI:Endpoint"];
        var apiKey = _config["AzureOpenAI:ApiKey"];
        var deployment = _config["AzureOpenAI:Deployment"];

        // Build prompt with style suffix
        var styleSuffix = "";
        if (!string.IsNullOrEmpty(style) && StylePrompts.ContainsKey(style))
            styleSuffix = StylePrompts[style];

        var prompt = BasePrompt + styleSuffix;

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var content = new MultipartFormDataContent();

        // Image file
        var imageContent = new ByteArrayContent(bytes);
        imageContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType ?? "image/jpeg");
        content.Add(imageContent, "image", image.FileName);

        // Prompt and parameters (matching GPT-image-1 API)
        content.Add(new StringContent(prompt), "prompt");
        content.Add(new StringContent("1"), "n");
        content.Add(new StringContent("1024x1024"), "size");
        content.Add(new StringContent("medium"), "quality");
        content.Add(new StringContent("100"), "output_compression");
        content.Add(new StringContent("jpeg"), "output_format");

        var url =
            $"{endpoint}openai/deployments/{deployment}/images/edits?api-version=2025-04-01-preview";

        var response = await client.PostAsync(url, content);
        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, result);

        return Content(result, "application/json");
    }
}
