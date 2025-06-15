namespace AP_Project.Models.Users
{
    public class User
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }
}
