using HTruyen.Data;
using HTruyen.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace HTruyen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        // Basic Vietnamese profanity list
        private static readonly string[] BadWords = { "địt", "đụ", "lồn", "cặc", "buồi", "đéo", "đm", "vcl", "vcc", "dcm", "cl" };

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{comicSlug}")]
        public async Task<IActionResult> GetComments(string comicSlug, [FromQuery] string? chapterName)
        {
            var query = _context.Comments.Where(c => c.ComicSlug == comicSlug);
            
            if (!string.IsNullOrEmpty(chapterName))
            {
                query = query.Where(c => c.ChapterName == chapterName);
            }

            var comments = await query
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    c.ChapterName,
                    User = new
                    {
                        c.User!.Username,
                        c.User.Avatar,
                        c.User.Level,
                        c.User.Role
                    }
                })
                .ToListAsync();

            return Ok(comments);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostComment([FromBody] CommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Nội dung bình luận không được để trống.");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return Unauthorized();

            var content = request.Content;
            foreach (var word in BadWords)
            {
                var pattern = @"\b" + Regex.Escape(word) + @"\b";
                content = Regex.Replace(content, pattern, "***", RegexOptions.IgnoreCase);
            }

            var comment = new Comment
            {
                UserId = userId,
                ComicSlug = request.ComicSlug,
                ChapterName = request.ChapterName,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);
            return Ok(new
            {
                comment.Id,
                comment.Content,
                comment.CreatedAt,
                comment.ChapterName,
                User = new
                {
                    user!.Username,
                    user.Avatar,
                    user.Level,
                    user.Role
                }
            });
        }
    }

    public class CommentRequest
    {
        public string ComicSlug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ChapterName { get; set; }
    }
}
