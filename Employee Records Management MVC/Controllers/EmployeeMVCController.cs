using Employee_Records_Management_MVC.Models;
using Employee_Records_Management_MVC.Constants;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using Employee_Records_Management_MVC;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Employee_Records_Management_Web_API.Controllers
{
    public class EmployeeMVCController : Controller
    {
        // Defining the base address of the Web API.
        Uri baseAddress = new Uri("https://localhost:7062/api/");

        // Creating an instance of HttpClient to make HTTP requests.
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public EmployeeMVCController(IConfiguration configuration)
        {
            // Creating an HttpClient instance.
            _httpClient = new HttpClient();

            // Setting the base address for the HttpClient.
            _httpClient.BaseAddress = baseAddress;

            // Inject the IConfiguration for accessing appsettings.json
            _configuration = configuration;
        }

        #region Sign Up
        /// <summary>
        /// Sign UP
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }
        #endregion

        #region SignUP Post

        [HttpPost]
        public IActionResult SignUp(SignUpCredentials signUpDetails)
        {
            if (ModelState.IsValid && signUpDetails != null)
            {
                if (!string.IsNullOrEmpty(signUpDetails.EmailAddress) && !string.IsNullOrEmpty(signUpDetails.Password))
                {
                    // Check if the email already exists
                    HttpResponseMessage checkEmailResponse = _httpClient.GetAsync($"{Constants.CheckEmailExists}/?email=" + signUpDetails.EmailAddress).Result;

                    if (checkEmailResponse.IsSuccessStatusCode)
                    {
                        var emailCheckData = checkEmailResponse.Content.ReadAsStringAsync().Result;
                        var emailExists = JsonConvert.DeserializeAnonymousType(emailCheckData, new { exists = false });

                        if (emailExists != null && emailExists.exists)
                        {
                            ModelState.AddModelError("EmailAddress", "Email address is already registered.");
                            return View(signUpDetails); // Return the view immediately
                        }
                    }

                    // Generating Random Salt
                    string salt = SaltGenerator.CreateSalt(6);

                    // Combining the provided password with the generated salt
                    string combinedPassword = string.Concat(signUpDetails.Password, salt);

                    // Hashing the combined password using SHA-256
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedPassword));
                        string hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

                        // Create a new model for the API request
                        var apiRequest = new SignUpCredentials
                        {
                            UserName = signUpDetails.UserName,
                            Password = hashedPassword,
                            EmailAddress = signUpDetails.EmailAddress,
                            PasswordSalt = salt // Storing salt along with the hashed password
                        };

                        string jsonData = JsonConvert.SerializeObject(apiRequest);

                        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = _httpClient.PostAsync(Constants.SignUpEmployee, content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            ViewBag.SuccessMessage = "User created successfully.";
                            return RedirectToAction("Login");
                        }
                    }
                }
            }

            ViewBag.ErrorMessage = "Registration failed. Please try again.";
            return View(signUpDetails);
        }


        #endregion

        #region Login
        /// <summary>
        /// Login
        /// </summary>
        /// <returns></returns>
        public IActionResult Login()
        {
            return View();
        }
        #endregion

        #region LoginPOST
        /// <summary>
        /// LoginPOST
        /// </summary>
        /// <param name="logindetails"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult LoginPOST(LoginCredentials logindetails)
        {
            if (ModelState.IsValid)
            {
                if (logindetails != null && !string.IsNullOrEmpty(logindetails.EmailAddress) && !string.IsNullOrEmpty(logindetails.Password))
                {
                    var apiRequest = new LoginCredentials
                    {
                        EmailAddress = logindetails.EmailAddress,
                        Password = logindetails.Password
                    };

                    string jsonData = JsonConvert.SerializeObject(apiRequest);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = _httpClient.PostAsync(Constants.LoginEmployee, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var data = response.Content.ReadAsStringAsync().Result;
                        var loginResult = JsonConvert.DeserializeAnonymousType(data, new { success = false, email = "", token = "" });

                        //DeserializeAnonymousType is a method provided by JsonConvert for deserializing JSON data into an anonymous type.
                        //An anonymous type is a C# feature that allows us to create objects with properties on the fly without defining a specific class.

                        if (loginResult != null && loginResult.success == true)
                        {
                            string userEmail = loginResult.email;
                            string jwtToken = loginResult.token;

                            var jwtSettings = _configuration.GetSection("Jwt");
                            var secretKey = jwtSettings["SecretKey"];

                            var tokenHandler = new JwtSecurityTokenHandler();
                            var key = Encoding.UTF8.GetBytes(secretKey);
                            var tokenDescriptor = new SecurityTokenDescriptor
                            {
                                Subject = new ClaimsIdentity(new[]
                                {
                                    new Claim(ClaimTypes.Name, userEmail) // Use the extracted email
                                }),
                                Expires = DateTime.UtcNow.AddHours(1),
                                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                            };
                            var token = tokenHandler.CreateToken(tokenDescriptor);
                            var jsonToken = tokenHandler.WriteToken(token);

                            var cookieOptions = new CookieOptions
                            {
                                Expires = DateTime.Now.AddMinutes(1),
                            };
                            Response.Cookies.Append("JWTTestToken", jsonToken, cookieOptions);

                            var redirectUrl = Url.Action("Index", "EmployeeMVC");
                            return Json(new { success = true, redirect = redirectUrl, token = jsonToken });
                        }
                    }
                    else
                    {
                        // Handle API-specific errors here
                        var apiResponse = response.Content.ReadAsStringAsync().Result;
                        var apiErrors = JsonConvert.DeserializeAnonymousType(apiResponse, new { success = false, errors = new List<string>() });
                        if (apiErrors != null && apiErrors.errors.Any())
                        {
                            foreach (var apiError in apiErrors.errors)
                            {
                                ModelState.AddModelError(string.Empty, apiError); // Adding API-specific errors to ModelState
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("emailAddress", "Invalid Email Address or Password.");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("password", "Email Address and password are required.");
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, errors = errors });
        }
        #endregion

        #region Logout
        /// <summary>
        /// Logout
        /// </summary>
        /// <returns></returns>
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); //The act of signing the user out using 'HttpContext.SignOutAsync' is the primary action to end the user's session and invalidate authentication.                        
            HttpContext.Session.Clear(); // Clearing Session data
            return RedirectToAction("Login");
        }
        #endregion

        #region Index
        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            List<EmployeeProfiles> employeeProfiles = new List<EmployeeProfiles>();
            HttpResponseMessage response = _httpClient.GetAsync(Constants.GetEmployeeList).Result;
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                if (data != null)
                {
                    employeeProfiles = JsonConvert.DeserializeObject<List<EmployeeProfiles>>(data) ?? new List<EmployeeProfiles>();
                }
            }
            return View(employeeProfiles);
        }
        #endregion

        #region Create - GET
        /// <summary>
        /// Create - GET
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            var employeeData = new EmployeeProfiles();
            // Retrieve the department list from the API.
            HttpResponseMessage response = _httpClient.GetAsync(Constants.GetDepartments).Result;
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                if (data != null)
                {
                    var departmentList = JsonConvert.DeserializeObject<List<DepartmentList>>(data);
                    employeeData.DepartmentList = departmentList;
                }
            }

            return View(employeeData);
        }
        #endregion

        #region Create - POST
        /// <summary>
        /// Create - POST
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Create(EmployeeProfiles employee)
        {
            if (ModelState.IsValid && employee != null)
            {
                // Serializing the employee object to JSON.
                string jsonData = JsonConvert.SerializeObject(employee);

                // Defining the HTTP content with JSON data.
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // Send a POST request to the API to create the employee.
                HttpResponseMessage response = _httpClient.PostAsync(Constants.CreateEmployee, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var redirectUrl = Url.Action("Index", "EmployeeMVC");
                    return Json(new { success = true, redirect = redirectUrl });
                }
            }
            ViewBag.ErrorMessage = "Failed to create the employee.";
            return View(employee);
        }
        #endregion

        #region Edit - GET
        /// <summary>
        /// Edit - GET
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            var employee = new EmployeeProfiles();

            // Make an HTTP GET request to the API to retrieve the employee by ID.
            HttpResponseMessage employeeResponse = _httpClient.GetAsync($"{Constants.GetEmployeeById}/?id=" + id).Result;

            if (employeeResponse.IsSuccessStatusCode)
            {
                var data = employeeResponse.Content.ReadAsStringAsync().Result;
                if (data != null)
                {
                    employee = JsonConvert.DeserializeObject<EmployeeProfiles>(data);
                }
            }
            if (employee == null)
            {
                return NotFound();
            }

            // Make an HTTP GET request to the API to retrieve the department list.
            HttpResponseMessage departmentResponse = _httpClient.GetAsync(Constants.GetDepartments).Result;

            if (departmentResponse.IsSuccessStatusCode)
            {
                var data = departmentResponse.Content.ReadAsStringAsync().Result;
                if (data != null)
                {
                    var departmentList = JsonConvert.DeserializeObject<List<DepartmentList>>(data);
                    employee.DepartmentList = departmentList;
                }
            }
            return View(employee);
        }
        #endregion

        #region Edit - POST
        /// <summary>
        /// Edit - POST
        /// </summary>
        /// <param name="updatedEmployee"></param>
        /// <returns></returns>
        [HttpPost]
        //[Authorize]
        public IActionResult Edit(EmployeeProfiles updatedEmployee)
        {
            if (ModelState.IsValid)
            {
                // Serializing the updated employee object to JSON.
                string jsonData = JsonConvert.SerializeObject(updatedEmployee);

                // Defining the HTTP content with JSON data.
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync(Constants.UpdateEmployee, content).Result;
                //($"{Constants.GetEmployeeById}/?id=" + id).Result;
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }

            ViewBag.ErrorMessage = "Failed to update the employee.";
            return View(updatedEmployee);
        }
        #endregion

        #region Delete - GET
        /// <summary>
        /// Delete - GET
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {
            var employee = new EmployeeProfiles();

            // Make an HTTP GET request to the API to retrieve the employee by ID.
            HttpResponseMessage employeeResponse = _httpClient.GetAsync($"{Constants.GetEmployeeById}/?id=" + id).Result;

            if (employeeResponse.IsSuccessStatusCode)
            {
                var data = employeeResponse.Content.ReadAsStringAsync().Result;
                if (data != null)
                {
                    employee = JsonConvert.DeserializeObject<EmployeeProfiles>(data);
                }
            }

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }
        #endregion

        #region DeleteEmployee - POST
        /// <summary>
        /// DeleteEmployee - POST
        /// </summary>
        /// <param name="EmployeeID"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult DeleteEmployee(int EmployeeID)
        {
            // Make an HTTP DELETE request to the API to delete the employee by ID.
            HttpResponseMessage response = _httpClient.GetAsync($"{Constants.DeleteEmployee}/?id=" + EmployeeID).Result;

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Failed to delete the employee.";
            return View();
        }
        #endregion

    }
}