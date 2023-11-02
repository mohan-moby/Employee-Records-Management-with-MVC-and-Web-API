using Employee_Records_Management_Web_API.IServices;
using Employee_Records_Management_Web_API.Models;
using Employee_Records_Management_Web_API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Employee_Records_Management_Web_API.Controllers
{
    [Route("api/[controller]/[action]")]   
    [ApiController]
    public class EmployeeAPIController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IEmployeeService _employeeService;
        public EmployeeAPIController(IConfiguration configuration, IEmployeeService employeeService)
        {
            _configuration = configuration;
            _employeeService = employeeService;
        }

        #region Login Page
        [HttpPost]
        public IActionResult Login(LoginCredentials loginCredentials)
        {
            if (loginCredentials != null && !string.IsNullOrEmpty(loginCredentials.EmailAddress) && !string.IsNullOrEmpty(loginCredentials.Password))
            {
                var databaseCredentials = _employeeService.GetLoginCredentials(loginCredentials.EmailAddress);

                if (databaseCredentials != null && !string.IsNullOrEmpty(databaseCredentials.Password) && !string.IsNullOrEmpty(databaseCredentials.PasswordSalt))
                {
                    string salt = databaseCredentials.PasswordSalt;
                    string hashedPasswordFromDatabase = databaseCredentials.Password;
                    string combinedPassword = loginCredentials.Password + salt;

                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedPassword));
                        string hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

                        if (hashedPassword == hashedPasswordFromDatabase)
                        {
                            // Authentication successful. Generating a JWT token.
                            // Retrieving JWT configuration values from appsettings.json
                            var jwtSettings = _configuration.GetSection("Jwt");
                            var secretKey = jwtSettings["SecretKey"];
                            var issuer = jwtSettings["Issuer"];
                            var audience = jwtSettings["Audience"];

                            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loginCredentials.EmailAddress)
                    };

                            var token = new JwtSecurityToken(
                                issuer: issuer,
                                audience: audience,
                                claims: claims,
                                expires: DateTime.UtcNow.AddHours(1),
                                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                            );

                            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                            // Returning the JWT token and email to the client
                            return Ok(new { success = true, email = loginCredentials.EmailAddress, token = tokenString });
                        }
                    }
                }
            }

            // Authentication failed.
            return BadRequest(new { success = false, errors = new List<string> { "Invalid Email Address or Password." } });
        }
        #endregion

        #region Get Employee List
        [HttpGet]
        public ActionResult GetEmployees()
        {
            var employeeList = _employeeService.EmployeesList();
            return Ok(employeeList);
            //This line returns an HTTP response with a status code of 200 (OK) and includes the employeeList as the response body.
            //The Ok() method is a built-in helper method in ASP.NET Core that simplifies creating a successful HTTP response.
            //In this case, it indicates that the request to retrieve employees was successful, and it includes the list of employees in the response.
        }
        #endregion

        #region Get Employee Department
        [HttpGet]
        public ActionResult GetDepartments()
        {
            var departmentList = _employeeService.GetDepartments(); 
            return Ok(departmentList);
        }
        #endregion

        #region Sign Up Employee
        [HttpPost]
        public IActionResult SignUp(SignUpCredentials signUp)
        {
            if (ModelState.IsValid && signUp.EmailAddress != null)
            {
                // Check if the email already exists
                SignUpCredentials existingEmail = _employeeService.CheckEmailExists(signUp.EmailAddress);

                if (existingEmail != null && !string.IsNullOrEmpty(existingEmail.EmailAddress))
                {                    
                    return BadRequest(new { success = false, message = "Email address already exists." });
                }
                else
                {                   
                    bool isSaved = _employeeService.InsertUser(signUp);

                    if (isSaved)
                    {
                        return Ok(new { success = true, message = "User created successfully" });
                    }
                    else
                    {
                        return BadRequest(new { success = false, message = "Failed to create the user." });
                    }
                }
            }

            return BadRequest(new { success = false, message = "Invalid data provided." });
        }

        [HttpGet]
        public IActionResult CheckEmailExists(string email)
        {
            SignUpCredentials existingEmail = _employeeService.CheckEmailExists(email);

            bool emailExists = existingEmail != null;

            return Ok(new { exists = emailExists });
        }
        #endregion

        #region Create Endpoint
        [HttpPost]
        public IActionResult Create(EmployeeProfiles employee)
        {
            if (ModelState.IsValid)
            {
                bool isSaved = _employeeService.InsertEmployee(employee);
                if (isSaved)
                {
                    return Ok(new { success = true });
                }
            }

            return BadRequest(new { success = false, message = "Failed to create the employee." });
        }
        #endregion

        #region Update Employee By ID
        [HttpGet]
        public ActionResult GetEmployeeById(int id)
        {
            var employee = _employeeService.GetEmployeeById(id);
            return Ok(employee);
        }

        [HttpPost]
        public IActionResult UpdateEmployee(EmployeeProfiles updatedEmployee)
        {
            if (ModelState.IsValid)
            {
                bool isUpdated = _employeeService.UpdateEmployee(updatedEmployee);
                if (isUpdated)
                {
                    return Ok(new { success = true });
                }
            }

            return BadRequest(new { success = false, message = "Failed to update the employee." });
        }
        #endregion

        #region Delete Employee ID
        [HttpGet]
        public IActionResult DeleteEmployee(int id)
        {
            bool deleteResponse = false;
            if (id > 0)
            {
                deleteResponse = _employeeService.DeleteEmployee(id);
            }
            else
            {
                return NotFound(deleteResponse);
            }
            return Ok(deleteResponse);           
        }
        #endregion

    }
}