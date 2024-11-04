using APPR_Try1.Model;
using NBench;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stressTest
{
    public class DonationStressTest
    {
        private Counter _donationCounter;
        private Counter _largeTransactionCounter;
        private Counter _concurrentTransactionCounter;

        private const string DonationCounterName = "DonationCounter";
        private const string LargeTransactionCounterName = "LargeTransactionCounter";
        private const string ConcurrentTransactionCounterName = "ConcurrentTransactionCounter";

        private readonly string connectionString = "Server=tcp:tyler-appr-server.database.windows.net,1433;Initial Catalog=tyler-database;Persist Security Info=False;User ID=ST10173943;Password=Mulaudzi2001;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        private List<DonateModels> _testDonations;
        private readonly string[] _donationTypes = { "OneTime", "Monthly", "Annual" };
        private readonly string[] _paymentMethods = { "CreditCard", "DebitCard", "BankTransfer", "PayPal" };

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            try
            {
                _donationCounter = context.GetCounter("DonationCounter");
                _largeTransactionCounter = context.GetCounter("LargeTransactionCounter");
                _concurrentTransactionCounter = context.GetCounter(ConcurrentTransactionCounterName);
                _testDonations = GenerateTestDonations(200);
            }
            catch (KeyNotFoundException ex) 
            {
                throw new NBenchException("Error retrieving counter: " + ex.Message, ex);
            }
        }

        private List<DonateModels> GenerateTestDonations(int count)
        {
            var donations = new List<DonateModels>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                var isAnonymous = random.Next(2) == 1;
                donations.Add(new DonateModels
                {
                    FullName = isAnonymous ? "Anonymous Donor" : $"Test Donor {i}",
                    Email = isAnonymous ? $"anonymous{i}@example.com" : $"donor{i}@example.com",
                    Amount = random.Next(10, 10000), // Random amount between $10 and $10,000
                    DonationType = _donationTypes[random.Next(_donationTypes.Length)],
                    Message = random.Next(2) == 1 ? $"Test donation message {i}" : null,
                    IsAnonymous = isAnonymous,
                    PaymentMethod = _paymentMethods[random.Next(_paymentMethods.Length)]
                });
            }
            return donations;
        }

        [PerfBenchmark(
            Description = "Tests basic donation processing performance",
            NumberOfIterations = 3,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 30000, 
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(DonationCounterName, MustBe.GreaterThan, 20.0d)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo, 50000000.0d)] // 50MB limit
        public void BasicDonationProcessingTest(BenchmarkContext context)
        {
            foreach (var donation in _testDonations.Take(50))
            {
                try
                {
                    ProcessTestDonation(donation);
                    _donationCounter.Increment();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in basic donation test: {ex.Message}");
                }
            }
        }

        [PerfBenchmark(
            Description = "Tests large donation processing performance",
            NumberOfIterations = 3,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 30000,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(LargeTransactionCounterName, MustBe.GreaterThan, 10.0d)]
        public void LargeDonationProcessingTest(BenchmarkContext context)
        {
            var largeDonations = _testDonations
                .Where(d => d.Amount > 5000)
                .Take(20);

            foreach (var donation in largeDonations)
            {
                try
                {
                    ProcessTestDonation(donation);
                    _largeTransactionCounter.Increment();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in large donation test: {ex.Message}");
                }
            }
        }

        [PerfBenchmark(
            Description = "Tests concurrent donation processing performance",
            NumberOfIterations = 3,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 30000,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(ConcurrentTransactionCounterName, MustBe.GreaterThan, 30.0d)]
        public async Task ConcurrentDonationProcessingTest(BenchmarkContext context)
        {
            var tasks = _testDonations.Skip(50).Take(50).Select(async donation =>
            {
                try
                {
                    await ProcessTestDonationAsync(donation);
                    _concurrentTransactionCounter.Increment();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in concurrent donation test: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);
        }

        private void ProcessTestDonation(DonateModels donation)
        {
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

        private async Task ProcessTestDonationAsync(DonateModels donation)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
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

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        [PerfCleanup]
        public void Cleanup()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Donations WHERE FullName LIKE 'Test Donor%' OR FullName = 'Anonymous Donor'";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }
    }
}
