namespace HTruyen.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // User or Admin
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigational properties
        public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public virtual ICollection<ReadingHistory> ReadingHistories { get; set; } = new List<ReadingHistory>();
    }
}
