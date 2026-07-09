namespace BusinessLayer1.Models
{
    public enum UserRole
    {
        Admin = 1,
        AdmissionOfficer = 2,
        Clerk = 3
    }

    public class UserSession
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}
