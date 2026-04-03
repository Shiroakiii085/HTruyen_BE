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
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            history.UserId = userId;
            history.ReadAt = DateTime.UtcNow;

            var existing = await _context.ReadingHistories
                .FirstOrDefaultAsync(h => h.UserId == userId && h.ComicSlug == history.ComicSlug);

            bool isNewChapter = false;

            if (existing != null)
            {
                if (existing.ChapterName != history.ChapterName) 
                {
                    isNewChapter = true;
                }
                existing.ChapterName = history.ChapterName;
                existing.ChapterApiData = history.ChapterApiData;
                existing.ScrollPosition = history.ScrollPosition;
                existing.ReadAt = history.ReadAt;
                
                // Update ComicName and ThumbUrl if provided (fixes old records)
                if (!string.IsNullOrEmpty(history.ComicName)) existing.ComicName = history.ComicName;
                if (!string.IsNullOrEmpty(history.ThumbUrl)) existing.ThumbUrl = history.ThumbUrl;

                _context.ReadingHistories.Update(existing);
            }
            else
            {
                isNewChapter = true;
                _context.ReadingHistories.Add(history);
            }

            if (isNewChapter)
            {
                AddExp(user, 10);
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Lịch sử đã được lưu.", expAdded = isNewChapter ? 10 : 0 });
        }

        private void AddExp(User user, int amount)
        {
            if (user.Level >= 15) return; // Max level

            user.Exp += amount;
            int reqExp = GetRequiredExp(user.Level);
            
            while (reqExp > 0 && user.Exp >= reqExp)
            {
                user.Exp -= reqExp;
                user.Level++;
                if (user.Level >= 15)
                {
                    user.Exp = 0;
                    break;
                }
                reqExp = GetRequiredExp(user.Level);
            }
        }

        private int GetRequiredExp(int level)
        {
            return level switch
            {
                1 => 100,
                2 => 300,
                3 => 1000,
                4 => 2000,
                5 => 3000,
                6 => 4000,
                7 => 5000,
                8 => 6000,
                9 => 7000,
                10 => 8000,
                11 => 11000,
                12 => 12000,
                13 => 13000,
                14 => 14000,
                _ => int.MaxValue
            };
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
