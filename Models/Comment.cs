using System.ComponentModel.DataAnnotations;

namespace HTruyen.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ComicSlug { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigational properties
        public virtual User? User { get; set; }
    }
}
