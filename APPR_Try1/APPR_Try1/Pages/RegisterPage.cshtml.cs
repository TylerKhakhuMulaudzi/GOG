using APPR_Try1.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace APPR_Try1.Pages
{
    public class RegisterPageModel : PageModel
    {
        private readonly string connectionString = "Server=tcp:tyler-appr-server.database.windows.net,1433;Initial Catalog=tyler-database;Persist Security Info=False;User ID=ST10173943;Password=Mulaudzi2001;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        [BindProperty]
        public UserModel User { get; set; }

        public void OnGet()
        {
            User = new UserModel();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                InsertUser(User);
                TempData["SuccessMessage"] = "Registration successful!";
                return RedirectToPage("/Index"); // Redirect to home page or login page
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return Page();
            }
        }

        private void InsertUser(UserModel user)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO users(userName, lastName, Email, contactNumb,Password) VALUES(@userName, @lastName, @Email, @ContactNumb, @Password)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userName", user.UserName);
                    command.Parameters.AddWithValue("@lastName", user.LastName);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@ContactNumb", user.ContactNumber);
                    command.Parameters.AddWithValue("@Password", user.Password);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}


