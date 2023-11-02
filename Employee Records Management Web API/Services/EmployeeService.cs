using Employee_Records_Management_Web_API.Models;
using Employee_Records_Management_Web_API.IRepositories;
using Employee_Records_Management_Web_API.IServices;

namespace Employee_Records_Management_Web_API.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }
        public List<EmployeeProfiles> EmployeesList()
        {
            var employeesList = _employeeRepository.EmployeesList();
            return employeesList;
        }
        public List<DepartmentList> GetDepartments()
        {
            var employees = _employeeRepository.GetDepartments();
            return employees;
        }
        public bool InsertEmployee(EmployeeProfiles new_employee)
        {
            return _employeeRepository.InsertEmployee(new_employee);
        }
        public bool InsertUser(SignUpCredentials signUp)
        {
            return _employeeRepository.InsertUser(signUp);
        }
        public EmployeeProfiles GetEmployeeById(int id)
        {
            var employee = _employeeRepository.GetEmployeeById(id);
            return employee;
        }
        public bool UpdateEmployee(EmployeeProfiles updateEmployee)
        {
            var updatedemployee = _employeeRepository.UpdateEmployee(updateEmployee);
            return updatedemployee;
        }
        public bool DeleteEmployee(int employeeId)
        {
            return _employeeRepository.DeleteEmployee(employeeId);
        }
        public LoginCredentials? GetLoginCredentials(string emailAddress)
        {
            return _employeeRepository.GetLoginCredentials(emailAddress);
        }   
        public SignUpCredentials CheckEmailExists(string emailAddress)
        {
            return _employeeRepository.CheckEmailExists(emailAddress);
        }
    }
}