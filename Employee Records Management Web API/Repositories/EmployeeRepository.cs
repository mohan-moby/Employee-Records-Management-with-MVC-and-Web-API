using Dapper;
using Employee_Records_Management_Web_API.IRepositories;
using Employee_Records_Management_Web_API.Models;
using System.Data;
using System.Data.SqlClient;


namespace Employee_Records_Management_Web_API.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string? _connectionString;

        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MyDbConnection");
        }

        // Open a database connection
        private IDbConnection OpenConnection()
        {
            IDbConnection dbConnection = new SqlConnection(_connectionString);
            dbConnection.Open();
            return dbConnection;
        }
        public List<EmployeeProfiles> EmployeesList()
        {
            using IDbConnection dbConnection = OpenConnection();
            var employees = dbConnection.Query<EmployeeProfiles>(Constants.EmployeesList).ToList();
            dbConnection.Close();
            return employees;
        }

        public List<DepartmentList> GetDepartments()
        {
            //EmployeeProfiles employee = new EmployeeProfiles();
            using IDbConnection dbConnection = OpenConnection();
            var DepartmentList = dbConnection.Query<DepartmentList>(Constants.DepartmentList).ToList();
            dbConnection.Close();
            return DepartmentList;
        }
        public bool InsertUser(SignUpCredentials signUp)
        {
            bool isSaved = false;
            if (signUp != null)
            {
                using IDbConnection dbConnection = OpenConnection();
                dbConnection.Execute(Constants.NewUser, signUp);
                isSaved = true;
                dbConnection.Close();
            }
            return isSaved;
        }
        public bool InsertEmployee(EmployeeProfiles employee)
        {
            bool isSaved = false;
            if (employee != null)
            {
                using IDbConnection dbConnection = OpenConnection();
                dbConnection.Execute(Constants.NewEmployee, employee);
                isSaved = true;
                dbConnection.Close();
            }
            return isSaved;
        }
        public EmployeeProfiles GetEmployeeById(int id)
        {
            var employee = new EmployeeProfiles();
            if (id > 0)
            {
                using IDbConnection dbConnection = OpenConnection();
                var employeeDetails = dbConnection.Query<EmployeeProfiles>(Constants.GetEmployee, new { EmployeeId = id }).SingleOrDefault();
                if (employeeDetails != null)
                {
                    employee = employeeDetails;
                }
                dbConnection.Close();
            }
            return employee;
        }
        public bool UpdateEmployee(EmployeeProfiles updateEmployee)
        {
            bool isUpdated = false;
            if (updateEmployee != null)
            {
                using IDbConnection dbConnection = OpenConnection();
                dbConnection.Execute(Constants.UpdateEmployeeDetailsById, updateEmployee);
                isUpdated = true;
                dbConnection.Close();
            }
            return isUpdated;
        }
        public bool DeleteEmployee(int employeeId)
        {
            if (employeeId > 0)
            {
                using IDbConnection dbConnection = OpenConnection();
                dbConnection.Execute(Constants.DeleteEmployee, new { EmployeeID = employeeId });
                dbConnection.Close();
            }
            return true;
        }
        public LoginCredentials GetLoginCredentials(string emailAddress)
        {
            using IDbConnection dbConnection = OpenConnection();
            var loginCredentials = dbConnection.Query<LoginCredentials>(Constants.GetLoginCredentials, new { EmailAddress = emailAddress }).SingleOrDefault();
            dbConnection.Close();
            if (loginCredentials != null)
            {
                return loginCredentials;
            }
            else
            {
                return new LoginCredentials();
            }
        }
        public SignUpCredentials CheckEmailExists(string emailAddress)
        {
            using IDbConnection dbConnection = OpenConnection();
            var existingEmail = dbConnection.Query<SignUpCredentials>(Constants.CheckEmailExists, new { EmailAddress = emailAddress }).SingleOrDefault();
            dbConnection.Close();            
            if (existingEmail != null)
            {
                return existingEmail;
            }
            else
            {
                return new SignUpCredentials();
            }
        }
    }
}