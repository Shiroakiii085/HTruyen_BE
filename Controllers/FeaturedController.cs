using System.Security.Claims;
using System.Text.Json;
using HTruyen.Data;
using HTruyen.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTruyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeaturedController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        public FeaturedController(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient("OTruyenClient");
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrent()
        {
            var featured = await _context.FeaturedComicConfigs.AsNoTracking().FirstOrDefaultAsync(f => f.Id == 1);
            if (featured == null)
            {
                return Ok(new { });
            }

            return Ok(featured);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword, [FromQuery] int page = 1)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { message = "Keyword is required." });
            }

            var response = await _httpClient.GetAsync($"tim-kiem?keyword={Uri.EscapeDataString(keyword)}&page={page}");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }

            var content = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;
            if (!root.TryGetProperty("data", out var dataEl) || !dataEl.TryGetProperty("items", out var itemsEl))
            {
                return Ok(new { items = Array.Empty<object>() });
            }

            var items = itemsEl.EnumerateArray()
                .Select(item => new
                {
                    slug = item.TryGetProperty("slug", out var slugEl) ? slugEl.GetString() : "",
                    name = item.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : "",
                    thumb_url = item.TryGetProperty("thumb_url", out var thumbEl) ? thumbEl.GetString() : ""
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.slug) && !string.IsNullOrWhiteSpace(x.name))
                .Take(20)
                .ToList();

            return Ok(new { items });
        }

        public class SetFeaturedRequest
        {
            public string ComicSlug { get; set; } = string.Empty;
            public string ComicName { get; set; } = string.Empty;
            public string ThumbUrl { get; set; } = string.Empty;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("set")]
        public async Task<IActionResult> SetFeatured([FromBody] SetFeaturedRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ComicSlug) || string.IsNullOrWhiteSpace(request.ComicName))
            {
                return BadRequest(new { message = "ComicSlug and ComicName are required." });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? userId = null;
            if (int.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            var featured = await _context.FeaturedComicConfigs.FirstOrDefaultAsync(f => f.Id == 1);
            if (featured == null)
            {
                featured = new FeaturedComicConfig { Id = 1 };
                _context.FeaturedComicConfigs.Add(featured);
            }

            featured.ComicSlug = request.ComicSlug.Trim();
            featured.ComicName = request.ComicName.Trim();
            featured.ThumbUrl = request.ThumbUrl?.Trim() ?? string.Empty;
            featured.UpdatedAt = DateTime.UtcNow;
            featured.UpdatedByUserId = userId;

            await _context.SaveChangesAsync();
            return Ok(featured);
        }
    }
}
