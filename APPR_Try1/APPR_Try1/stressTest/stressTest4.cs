using APPR_Try1.Pages;
using NBench;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stressTest
{
    public class DonationSystemPerfTests
    {
        private readonly string connectionString = "Server=tcp:tyler-appr-server.database.windows.net,1433;Initial Catalog=tyler-database;Persist Security Info=False;User ID=ST10173943;Password=Mulaudzi2001;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private Counter _opsCounter;
        private PrivacyModel _privacyModel;

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            _opsCounter = context.GetCounter("OperationsPerSecond");
            _privacyModel = new PrivacyModel(null); 
        }

        [PerfBenchmark(
            Description = "Tests the performance of getting total donations",
            NumberOfIterations = 100,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 10000,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion("OperationsPerSecond", MustBe.GreaterThan, 50)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo, 10485760)]
        public void TotalDonationsBenchmark()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT SUM(Amount) FROM Donations";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.ExecuteScalar();
                        _opsCounter.Increment();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TotalDonationsBenchmark: {ex.Message}");
            }
        }

        [PerfBenchmark(
            Description = "Tests the performance of getting active disasters with allocations",
            NumberOfIterations = 50,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 10000,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion("OperationsPerSecond", MustBe.GreaterThan, 30)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo, 20971520)] 
        public void ActiveDisastersBenchmark()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
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
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                
                                var disasterName = reader.GetString(0);
                                var location = reader.GetString(1);
                            }
                        }
                        _opsCounter.Increment();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ActiveDisastersBenchmark: {ex.Message}");
            }
        }

        [PerfBenchmark(
            Description = "Tests the performance of getting recent donations",
            NumberOfIterations = 75,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 10000,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion("OperationsPerSecond", MustBe.GreaterThan, 40)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo, 15728640)] // 15MB
        public void RecentDonationsBenchmark()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT TOP 10 
                                    FullName,
                                    Amount,
                                    DonationType,
                                    IsAnonymous
                                    FROM Donations
                                    ORDER BY DonationDate DESC";
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var fullName = reader.GetString(0);
                                var amount = reader.GetDecimal(1);
                            }
                        }
                        _opsCounter.Increment();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RecentDonationsBenchmark: {ex.Message}");
            }
        }

        [PerfCleanup]
        public void Cleanup()
        {
            
        }
    }
}
