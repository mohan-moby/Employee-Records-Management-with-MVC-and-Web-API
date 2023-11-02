using Employee_Records_Management_Web_API.Models;

namespace Employee_Records_Management_Web_API.IServices
{
    public interface IEmployeeService
    {
        List<EmployeeProfiles> EmployeesList();
        bool InsertEmployee(EmployeeProfiles employee);
        bool InsertUser(SignUpCredentials signUp);
        List<DepartmentList> GetDepartments();
        EmployeeProfiles GetEmployeeById(int id);
        bool UpdateEmployee(EmployeeProfiles updateEmployee);
        bool DeleteEmployee(int employeeId);
        LoginCredentials? GetLoginCredentials(string emailAddress);
        SignUpCredentials CheckEmailExists(string emailAddress);
    }
}
