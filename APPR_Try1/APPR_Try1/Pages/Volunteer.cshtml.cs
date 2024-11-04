using APPR_Try1.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace APPR_Try1.Pages
{
    public class VolunteerModel : PageModel
    {
        private readonly string connectionString = "Server=tcp:tyler-appr-server.database.windows.net,1433;Initial Catalog=tyler-database;Persist Security Info=False;User ID=ST10173943;Password=Mulaudzi2001;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        [BindProperty]
        public VolunteerModels Volunteer { get; set; }

        public void OnGet()
        {
            Volunteer = new VolunteerModels();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var fileUpload = Request.Form.Files["qualification"];
            if (fileUpload != null && fileUpload.Length > 0)
            {
                using (var memory = new MemoryStream())
                {
                    fileUpload.CopyTo(memory);
                    Volunteer.QualificationData = memory.ToArray();
                   
                }
            }

            try
            {
                EnterVolunteerData(Volunteer);
                TempData["SuccessMessage"] = "Volunteer application submitted successfully!";
                return RedirectToPage("/Index");
                
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"There was an error: {ex.Message}");
                return Page();
            }
        }

        private void EnterVolunteerData(VolunteerModels volunteer)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    //_logger.LogInformation("Database connection opened");

                    string query = @"INSERT INTO VolunteerData 
                             (FullName, Email, Phone, Interests)
                             VALUES 
                             (@FullName, @Email, @Phone, @Interests)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FullName", volunteer.FullName);
                        command.Parameters.AddWithValue("@Email", volunteer.Email);
                        command.Parameters.AddWithValue("@Phone", volunteer.Phone);
                        command.Parameters.AddWithValue("@Interests", volunteer.Interests);

                        //_logger.LogInformation("Executing SQL command");
                        //int rowsAffected = command.ExecuteNonQuery();
                        //_logger.LogInformation($"{rowsAffected} rows affected");
                    }

                }
                catch (Exception ex)
                {

                    Console.WriteLine("sucess");
                }
            }
        }
        }
    }
                //catch (SqlException sqlEx)
                //{
                //    _logger.LogError(sqlEx, "SQL error occurred");
                //    throw;
                //}
                //catch (Exception ex)
                //{
                //    _logger.LogError(ex, "Unexpected error occurred");
                //    throw;
                //}
                //finally
                //{
                //    connection.Close();
                //    _logger.LogInformation("Database connection closed");
                //}
      