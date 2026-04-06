using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace HTruyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public ProxyController(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClient = httpClientFactory.CreateClient("OTruyenClient");
            _cache = cache;
        }

        private async Task<IActionResult> GetCachedResponse(string key, string url)
        {
            if (_cache.TryGetValue(key, out string? cachedContent))
            {
                return Content(cachedContent!, "application/json");
            }

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);
                
                var content = await response.Content.ReadAsStringAsync();
                _cache.Set(key, content, CacheDuration);
                
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("home")]
        public async Task<IActionResult> GetHome()
        {
            return await GetCachedResponse("proxy_home", "home");
        }

        [HttpGet("danh-sach/{type}")]
        public async Task<IActionResult> GetList(string type, [FromQuery] int page = 1)
        {
            return await GetCachedResponse($"proxy_list_{type}_{page}", $"danh-sach/{type}?page={page}");
        }

        [HttpGet("the-loai")]
        public async Task<IActionResult> GetCategories()
        {
            return await GetCachedResponse("proxy_categories", "the-loai");
        }

        [HttpGet("the-loai/{slug}")]
        public async Task<IActionResult> GetCategoryDetail(string slug, [FromQuery] int page = 1)
        {
            return await GetCachedResponse($"proxy_cat_{slug}_{page}", $"the-loai/{slug}?page={page}");
        }

        [HttpGet("truyen-tranh/{slug}")]
        public async Task<IActionResult> GetComicDetail(string slug)
        {
            return await GetCachedResponse($"proxy_comic_{slug}", $"truyen-tranh/{slug}");
        }

        [HttpGet("tim-kiem")]
        public async Task<IActionResult> Search([FromQuery] string keyword, [FromQuery] int page = 1)
        {
            var response = await _httpClient.GetAsync($"tim-kiem?keyword={Uri.EscapeDataString(keyword)}&page={page}");
            if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet("chapter")]
        public async Task<IActionResult> GetChapterContent([FromQuery] string apiUrl)
        {
            // Cache chapter metadata for a long time as it's static
            return await GetCachedResponse($"proxy_chapter_{apiUrl}", apiUrl);
        }
    }
}
