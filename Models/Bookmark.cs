namespace HTruyen.Models
{
    public class Bookmark
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ComicSlug { get; set; } = string.Empty;
        public string ComicName { get; set; } = string.Empty;
        public string ThumbUrl { get; set; } = string.Empty;
        public string LastChapter { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
    }
}
