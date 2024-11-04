using APPR_Try1.Model;
using NBench;
using System.Data.SqlClient;

namespace stressTest
{
    public class VolunteerStressTest
    {
        private Counter _operationsCounter;
        private const string OperationsCounterName = "OperationsCounter";
        private readonly string connectionString = "Server=tcp:tyler-appr-server.database.windows.net,1433;Initial Catalog=tyler-database;Persist Security Info=False;User ID=ST10173943;Password=Mulaudzi2001;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        private List<VolunteerModels> _testData;

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            _operationsCounter = context.GetCounter(OperationsCounterName);
            _testData = GenerateTestData(100); 
        }

        private List<VolunteerModels> GenerateTestData(int count)
        {
            var data = new List<VolunteerModels>();
            for (int i = 0; i < count; i++)
            {
                data.Add(new VolunteerModels
                {
                    FullName = $"Test User {i}",
                    Email = $"testuser{i}@example.com",
                    Phone = $"123456789{i.ToString().PadLeft(1, '0')}",
                    Interests = $"Interest {i}",
                    QualificationData = GenerateDummyFileData()
                });
            }
            return data;
        }

        private byte[] GenerateDummyFileData()
        {
            return new byte[1024 * 1024];
        }

        [PerfBenchmark(
            Description = "Tests the performance of volunteer registration process",
            NumberOfIterations = 3,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 60000, 
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion(OperationsCounterName, MustBe.GreaterThan, 50.0d)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo, 100000000.0d)] // 100MB limit
        public void VolunteerRegistrationStressTest(BenchmarkContext context)
        {
            foreach (var volunteer in _testData)
            {
                try
                {
                    using (var stream = new MemoryStream(volunteer.QualificationData))
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
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

                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    _operationsCounter.Increment();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during stress test: {ex.Message}");
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
                    string query = "DELETE FROM VolunteerData WHERE FullName LIKE 'Test User%'";
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
