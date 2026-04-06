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
        public int Level { get; set; } = 1; // Nhất Cảnh mặc định
        public int Exp { get; set; } = 0; // EXP hiện tại trong cấp
        public DateTime? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigational properties
        public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public virtual ICollection<ReadingHistory> ReadingHistories { get; set; } = new List<ReadingHistory>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
