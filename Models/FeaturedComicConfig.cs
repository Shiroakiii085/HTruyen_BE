namespace HTruyen.Models
{
    public class FeaturedComicConfig
    {
        public int Id { get; set; } = 1;
        public string ComicSlug { get; set; } = string.Empty;
        public string ComicName { get; set; } = string.Empty;
        public string ThumbUrl { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedByUserId { get; set; }
    }
}
