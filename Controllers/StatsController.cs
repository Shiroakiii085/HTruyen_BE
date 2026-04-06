using HTruyen.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTruyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("most-read")]
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
                .Take(12)
                .ToListAsync();

            return Ok(stats);
        }
    }
}
