using HTruyen.Data;
using HTruyen.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HTruyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HistoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HistoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetHistories()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var histories = await _context.ReadingHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.ReadAt)
                .ToListAsync();

            return Ok(histories);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateHistory([FromBody] ReadingHistory history)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            history.UserId = userId;
            history.ReadAt = DateTime.UtcNow;

            var existing = await _context.ReadingHistories
                .FirstOrDefaultAsync(h => h.UserId == userId && h.ComicSlug == history.ComicSlug);

            if (existing != null)
            {
                existing.ChapterName = history.ChapterName;
                existing.ChapterApiData = history.ChapterApiData;
                existing.ScrollPosition = history.ScrollPosition;
                existing.ReadAt = history.ReadAt;
                _context.ReadingHistories.Update(existing);
            }
            else
            {
                _context.ReadingHistories.Add(history);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Lịch sử đã được lưu." });
        }

        [HttpDelete("{slug}")]
        public async Task<IActionResult> DeleteHistory(string slug)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var existing = await _context.ReadingHistories
                .FirstOrDefaultAsync(h => h.UserId == userId && h.ComicSlug == slug);

            if (existing != null)
            {
                _context.ReadingHistories.Remove(existing);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Đã xóa lịch sử đọc truyện này." });
        }
    }
}
