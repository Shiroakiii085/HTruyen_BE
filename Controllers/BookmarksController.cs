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
    public class BookmarksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookmarksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBookmarks()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var bookmarks = await _context.Bookmarks
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.AddedAt)
                .ToListAsync();

            return Ok(bookmarks);
        }

        [HttpPost]
        public async Task<IActionResult> AddBookmark([FromBody] Bookmark bookmark)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bookmark.UserId = userId;
            bookmark.AddedAt = DateTime.UtcNow;

            if (await _context.Bookmarks.AnyAsync(b => b.UserId == userId && b.ComicSlug == bookmark.ComicSlug))
                return BadRequest("Truyện này đã có trong danh sách yêu thích.");

            _context.Bookmarks.Add(bookmark);
            await _context.SaveChangesAsync();

            return Ok(bookmark);
        }

        [HttpDelete("{slug}")]
        public async Task<IActionResult> RemoveBookmark(string slug)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var bookmark = await _context.Bookmarks.FirstOrDefaultAsync(b => b.UserId == userId && b.ComicSlug == slug);
            
            if (bookmark == null) return NotFound("Không tìm thấy trong yêu thích.");

            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa khỏi yêu thích." });
        }

        [HttpGet("check/{slug}")]
        public async Task<IActionResult> CheckBookmark(string slug)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var exists = await _context.Bookmarks.AnyAsync(b => b.UserId == userId && b.ComicSlug == slug);
            return Ok(new { isBookmarked = exists });
        }
    }
}
