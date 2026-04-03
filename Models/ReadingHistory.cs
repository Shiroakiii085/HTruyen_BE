namespace HTruyen.Models
{
    public class ReadingHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ComicSlug { get; set; } = string.Empty;
        public string ComicName { get; set; } = string.Empty;
        public string ThumbUrl { get; set; } = string.Empty;
        public string ChapterName { get; set; } = string.Empty;
        public string ChapterApiData { get; set; } = string.Empty;
        public int ScrollPosition { get; set; } = 0;
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
    }
}
