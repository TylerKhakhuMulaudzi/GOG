using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace APPR_Try1.Pages
{
    public class PrivacyModel : PageModel
    {
        private readonly string connectionString = "Server=tcp:tyler-appr-server.database.windows.net,1433;Initial Catalog=tyler-database;Persist Security Info=False;User ID=ST10173943;Password=Mulaudzi2001;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public decimal TotalDonations { get; set; }
        public decimal CurrentMonthDonations { get; set; }
        public int TotalGoodsRecived { get; set; }
        public List<DonationSummaryItem> RecentDonations { get; set; }
        public List<DisasterSummaryItem> ActiveDisasters { get; set; }


        public class DonationSummaryItem
        {
            public string FullName { get; set; }
            public decimal Amount { get; set; }
            public DateTime DonationDate { get; set; }
            public string DonationType { get; set; }
            public bool IsAnonymous { get; set; }
        }
        public class DisasterSummaryItem
        {
            public string DisasterName { get; set; }
            public string Location { get; set; }
            public decimal AllocatedMoney { get; set; }
            public int AllocatedGoods { get; set; }
            public DateTime StartDate { get; set; }
            public string Status { get; set; }
        }


        private readonly ILogger<PrivacyModel> _logger;

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            TotalDonations = GetTotalDonations();
            CurrentMonthDonations = GetCurrentMonthDonations();
            RecentDonations = GetRecentDonations();
            TotalGoodsRecived = GetTotalGoodsReceived();
            ActiveDisasters = GetActiveDisasters();
        }
        private int GetTotalGoodsReceived()
        {
            int totalGoods = 0;
            try 
            {
                using (SqlConnection connection = new SqlConnection(connectionString)) 
                {
                connection.Open();
                    string query = "SELECT SUM(Quantity) FROM GoodsDonations";

                    using (SqlCommand command = new SqlCommand(query,connection)) 
                    {
                    object result = command.ExecuteScalar();
                        if (result != null) 
                        {
                        totalGoods = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
            return totalGoods;
        }
        private List<DisasterSummaryItem> GetActiveDisasters()
        {
        var disasters = new List<DisasterSummaryItem>();
            try 
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                { 
                connection.Open ();
                    string query = @"SELECT 
                                    d.DisasterName,
                                    d.Location,
                                    d.StartDate,
                                    d.Status,
                                    ISNULL(SUM(da.AllocatedAmount), 0) as AllocatedMoney,
                                    ISNULL(SUM(ga.AllocatedQuantity), 0) as AllocatedGoods
                                   FROM Disasters d
                                   LEFT JOIN DisasterAllocations da ON d.DisasterId = da.DisasterId
                                   LEFT JOIN GoodsAllocations ga ON d.DisasterId = ga.DisasterId
                                   WHERE d.Status = 'Active'
                                   GROUP BY d.DisasterName, d.Location, d.StartDate, d.Status";
                    using (SqlCommand command = new SqlCommand(query,connection)) 
                    {
                        using (SqlDataReader reader = command.ExecuteReader()) 
                        {
                            while (reader.Read())
                            {
                                disasters.Add(new DisasterSummaryItem 
                                { 
                                    DisasterName = reader.GetString(0),
                                    Location = reader.GetString(1),
                                    StartDate = reader.GetDateTime(2),
                                    Status = reader.GetString(3),
                                    AllocatedMoney = reader.GetDecimal(4),
                                    AllocatedGoods = reader.GetInt32(5)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception x) 
            {
                Console.WriteLine(x.Message);
            }
            return disasters;
        }
        private decimal GetTotalDonations() {
            decimal totalAmount = 0;
            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) 
                {
                    connection.Open();
                    string query = "SELECT SUM(Amount) FROM Donations";

                    using (SqlCommand command = new SqlCommand(query,connection)) 
                    {
                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value) 
                        {
                        totalAmount = Convert.ToDecimal(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            Console.WriteLine(ex.Message);
            }
            return totalAmount;
        }
        private decimal GetCurrentMonthDonations() 
        {
        decimal totalAmount = 0;
            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) 
                {
                connection.Open ();
                    string query = @"SELECT SUM(Amount) 
                                   FROM Donations 
                                   WHERE MONTH(DonationDate) = MONTH(GETDATE()) 
                                   AND YEAR(DonationDate) = YEAR(GETDATE())";

                    using (SqlCommand command = new SqlCommand(query,connection))
                    {
                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value) 
                        { 
                        totalAmount += Convert.ToDecimal(result);
                        }
                    }
                }
            } catch (Exception ex) 
            {
            Console.WriteLine (ex.Message);
            }
            return totalAmount;
        }
        private List<DonationSummaryItem> GetRecentDonations()
        {
        var donations = new List<DonationSummaryItem>();
            try {
                using (SqlConnection connection = new SqlConnection(connectionString)) 
                {
                connection.Open();
                    string query = @"SELECT TOP 10 
                                    FullName,
                                    Amount,
                                    DonationType,
                                    IsAnonymous
                                    FROM Donations
                                    ORDER BY DonationDate DESC";
                    using (SqlCommand command = new SqlCommand(query,connection)) 
                    {
                        using (SqlDataReader reader = command.ExecuteReader()) 
                        {
                        while (reader.Read()) 
                            {
                            donations.Add(new DonationSummaryItem
                            {
                                FullName = reader.GetString(0),
                                Amount = reader.GetDecimal(1),
                                DonationDate = reader.GetDateTime(2),
                                DonationType = reader.GetString(3),
                                IsAnonymous = reader.GetBoolean(4)
                            });
                            }
                        }
                    }
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
            return donations;
        }
    }

}
