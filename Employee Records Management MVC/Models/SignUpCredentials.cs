namespace Employee_Records_Management_MVC.Models
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
