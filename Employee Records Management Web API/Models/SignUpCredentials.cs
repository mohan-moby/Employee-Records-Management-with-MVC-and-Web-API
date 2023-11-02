namespace Employee_Records_Management_Web_API.Models
{
    public class SignUpCredentials
    {
        public string? UserName { get; set; }
        public string? EmailAddress { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? PasswordSalt { get; set; }
    }

}
