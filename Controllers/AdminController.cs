using HTruyen.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTruyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("statistics/most-read")]
        public async Task<IActionResult> GetMostReadStatistics()
        {
            var stats = await _context.ReadingHistories
                .GroupBy(h => h.ComicSlug)
                .Select(g => new
                {
                    ComicSlug = g.Key,
                    ComicName = g.Max(h => h.ComicName),
                    ThumbUrl = g.Max(h => h.ThumbUrl),
                    ReadCount = g.Count(),
                    LastReadAt = g.Max(h => h.ReadAt)
                })
                .OrderByDescending(s => s.ReadCount)
                .Take(10)
                .ToListAsync();

            return Ok(stats);
        }

        [HttpGet("statistics/reader-counts")]
        public async Task<IActionResult> GetReaderCounts()
        {
            try
            {
                var now = DateTime.UtcNow;

                // Pull once then aggregate in memory to avoid provider-specific SQL translation issues.
                var histories = await _context.ReadingHistories
                    .AsNoTracking()
                    .Where(h => h.ReadAt >= now.AddYears(-2))
                    .Select(h => h.ReadAt)
                    .ToListAsync();

                var daily = histories
                    .Where(d => d >= now.AddDays(-30))
                    .GroupBy(d => d.Date)
                    .Select(g => new { Label = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() })
                    .OrderBy(x => x.Label)
                    .ToList();

                var monthly = histories
                    .Where(d => d >= now.AddMonths(-12))
                    .GroupBy(d => new { d.Year, d.Month })
                    .Select(g => new
                    {
                        Label = $"{g.Key.Year}-{g.Key.Month:00}",
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Label)
                    .ToList();

                var yearly = histories
                    .GroupBy(d => d.Year)
                    .Select(g => new { Label = g.Key.ToString(), Count = g.Count() })
                    .OrderBy(x => x.Label)
                    .ToList();

                return Ok(new { Daily = daily, Monthly = monthly, Yearly = yearly });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load reader statistics.", error = ex.Message });
            }
        }
    }
}
