using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using APPR_Try1.Model;
namespace APPR_Try1.Pages
{
    public class DonateModel : PageModel
    {
        private readonly string connectionString = "Server=tcp:tyler-appr-server.database.windows.net,1433;Initial Catalog=tyler-database;Persist Security Info=False;User ID=ST10173943;Password=Mulaudzi2001;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        [BindProperty]
        public DonateModels Donation { get; set; }

        public void OnGet()
        {
            Donation = new DonateModels();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Process the donation
                ProcessDonation(Donation);

                // Redirect to a thank you page
                return RedirectToPage("Index", new { amount = Donation.Amount });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while processing your donation: {ex.Message}");
                return Page();
            }
        }

        private void ProcessDonation(DonateModels donation)
        {
            // This is where you would integrate with a payment gateway
            // For now, we'll just save the donation details to the database

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Donations 
                                 (FullName, Email, Amount, DonationType, Message, IsAnonymous, PaymentMethod, DonationDate) 
                                 VALUES 
                                 (@FullName, @Email, @Amount, @DonationType, @Message, @IsAnonymous, @PaymentMethod, @DonationDate)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FullName", donation.FullName);
                    command.Parameters.AddWithValue("@Email", donation.Email);
                    command.Parameters.AddWithValue("@Amount", donation.Amount);
                    command.Parameters.AddWithValue("@DonationType", donation.DonationType);
                    command.Parameters.AddWithValue("@Message", donation.Message ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@IsAnonymous", donation.IsAnonymous);
                    command.Parameters.AddWithValue("@PaymentMethod", donation.PaymentMethod);
                    command.Parameters.AddWithValue("@DonationDate", DateTime.Now);

                    command.ExecuteNonQuery();
                }
            }

        }
    }
}
