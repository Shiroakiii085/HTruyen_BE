using Microsoft.AspNetCore.Mvc;

namespace HTruyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ProxyController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("OTruyenClient");
        }

        [HttpGet("home")]
        public async Task<IActionResult> GetHome()
        {
            var response = await _httpClient.GetAsync("home");
            if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet("danh-sach/{type}")]
        public async Task<IActionResult> GetList(string type, [FromQuery] int page = 1)
        {
            var response = await _httpClient.GetAsync($"danh-sach/{type}?page={page}");
            if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet("the-loai")]
        public async Task<IActionResult> GetCategories()
        {
            var response = await _httpClient.GetAsync("the-loai");
            if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet("the-loai/{slug}")]
        public async Task<IActionResult> GetCategoryDetail(string slug, [FromQuery] int page = 1)
        {
            var response = await _httpClient.GetAsync($"the-loai/{slug}?page={page}");
            if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet("truyen-tranh/{slug}")]
        public async Task<IActionResult> GetComicDetail(string slug)
        {
            var response = await _httpClient.GetAsync($"truyen-tranh/{slug}");
            if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
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
            // Direct call to chapter CDN using the configured client
            var response = await _httpClient.GetAsync(apiUrl);
            if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    }
}
