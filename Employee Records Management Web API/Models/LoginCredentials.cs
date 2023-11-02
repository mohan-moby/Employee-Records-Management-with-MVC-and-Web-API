namespace Employee_Records_Management_Web_API.Models
{
    public class LoginCredentials
    {
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string? EmailAddress { get; set; }
        public string? Password { get; set; }
        public bool? IsSaved { get; set; }
        public bool? IsDeleted { get; set; }
        public string? PasswordSalt { get; set; }
    }
}