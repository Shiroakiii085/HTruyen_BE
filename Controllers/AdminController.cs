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
            var now = DateTime.UtcNow;

            // Daily stats (last 30 days)
            var dailyLimit = now.AddDays(-30);
            var daily = await _context.ReadingHistories
                .Where(h => h.ReadAt >= dailyLimit)
                .GroupBy(h => h.ReadAt.Date)
                .Select(g => new { Label = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() })
                .OrderBy(x => x.Label)
                .ToListAsync();

            // Monthly stats (last 12 months)
            var monthlyLimit = now.AddMonths(-12);
            var monthly = await _context.ReadingHistories
                .Where(h => h.ReadAt >= monthlyLimit)
                .GroupBy(h => new { h.ReadAt.Year, h.ReadAt.Month })
                .Select(g => new { 
                    Label = g.Key.Year + "-" + (g.Key.Month < 10 ? "0" : "") + g.Key.Month, 
                    Count = g.Count() 
                })
                .OrderBy(x => x.Label)
                .ToListAsync();

            // Yearly stats
            var yearly = await _context.ReadingHistories
                .GroupBy(h => h.ReadAt.Year)
                .Select(g => new { Label = g.Key.ToString(), Count = g.Count() })
                .OrderBy(x => x.Label)
                .ToListAsync();

            return Ok(new { Daily = daily, Monthly = monthly, Yearly = yearly });
        }
    }
}
