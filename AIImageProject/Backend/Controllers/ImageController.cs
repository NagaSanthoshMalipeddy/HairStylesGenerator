using Microsoft.ApplicationInsights;
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
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TelemetryClient _telemetry;
    private readonly ILogger<ImageController> _logger;

    public ImageController(
        IConfiguration config,
        IHttpClientFactory httpClientFactory,
        TelemetryClient telemetry,
        ILogger<ImageController> logger)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
        _telemetry = telemetry;
        _logger = logger;
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

        if (string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(deployment))
        {
            _logger.LogError("Azure OpenAI configuration is missing.");
            return StatusCode(500, "Azure OpenAI configuration is missing on the server.");
        }

        // Build prompt with style suffix
        var styleSuffix = "";
        if (!string.IsNullOrEmpty(style) && StylePrompts.ContainsKey(style))
            styleSuffix = StylePrompts[style];

        var prompt = BasePrompt + styleSuffix;

        // Use the named client configured in Program.cs with a 4-minute timeout
        // (default HttpClient.Timeout of 100s is too short for image generation).
        var client = _httpClientFactory.CreateClient("AzureOpenAI");
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

        // Track the Azure OpenAI call as a custom App Insights dependency for end-to-end visibility.
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var success = false;
        int statusCode = 0;

        try
        {
            var response = await client.PostAsync(url, content, HttpContext.RequestAborted);
            statusCode = (int)response.StatusCode;
            success = response.IsSuccessStatusCode;
            var result = await response.Content.ReadAsStringAsync(HttpContext.RequestAborted);

            if (!success)
            {
                _logger.LogWarning(
                    "Azure OpenAI returned {StatusCode} for style '{Style}'.",
                    statusCode, style ?? "default");
                return StatusCode(statusCode, result);
            }

            // Track style usage as a custom event so you can chart popular styles in App Insights.
            _telemetry.TrackEvent("HairstyleGenerated", new Dictionary<string, string>
            {
                ["style"] = style ?? "default",
                ["deployment"] = deployment
            });

            return Content(result, "application/json");
        }
        catch (TaskCanceledException ex) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            // HttpClient timed out (vs. the caller actually disconnecting).
            statusCode = StatusCodes.Status504GatewayTimeout;
            _logger.LogWarning(ex,
                "Azure OpenAI image generation timed out after {Elapsed}s for style '{Style}'.",
                stopwatch.Elapsed.TotalSeconds, style ?? "default");
            _telemetry.TrackException(ex, new Dictionary<string, string>
            {
                ["style"] = style ?? "default",
                ["deployment"] = deployment,
                ["reason"] = "HttpClientTimeout"
            });
            return StatusCode(statusCode, new
            {
                error = "Image generation timed out. Try again with a simpler prompt or smaller image."
            });
        }
        catch (Exception ex)
        {
            _telemetry.TrackException(ex, new Dictionary<string, string>
            {
                ["style"] = style ?? "default",
                ["deployment"] = deployment
            });
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _telemetry.TrackDependency(
                dependencyTypeName: "Azure OpenAI",
                target: new Uri(endpoint).Host,
                dependencyName: $"images/edits ({deployment})",
                data: url,
                startTime: startTime,
                duration: stopwatch.Elapsed,
                resultCode: statusCode.ToString(),
                success: success);
        }
    }
}
