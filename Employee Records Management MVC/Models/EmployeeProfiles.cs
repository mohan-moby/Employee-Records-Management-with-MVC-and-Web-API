namespace Employee_Records_Management_MVC.Models
{
    public class EmployeeProfiles
    {
        public int EmployeeID { get; set; }
        public int DepartmentID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public List<DepartmentList>? DepartmentList { get; set; }
        public string? DepartmentName { get; set; }
        public bool? IsSaved { get; set; }

    }
}
