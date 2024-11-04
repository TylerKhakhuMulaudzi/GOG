using APPR_Try1.Model;
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
    public class UserRegistrationPerfTests
    {
        private readonly string connectionString = "Server=tcp:tyler-appr-server.database.windows.net,1433;Initial Catalog=tyler-database;Persist Security Info=False;User ID=ST10173943;Password=Mulaudzi2001;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private Counter _opsCounter;
        private Counter _errorCounter;
        private RegisterPageModel _registerPage;
        private List<UserModel> _testUsers;

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            _opsCounter = context.GetCounter("OperationsPerSecond");
            _errorCounter = context.GetCounter("ErrorsPerSecond");
            _registerPage = new RegisterPageModel();
            _testUsers = GenerateTestUsers(1000); 
        }

        private List<UserModel> GenerateTestUsers(int count)
        {
            var users = new List<UserModel>();
            for (int i = 0; i < count; i++)
            {
                users.Add(new UserModel
                {
                    UserName = $"TestUser{i}",
                    LastName = $"LastName{i}",
                    Email = $"testuser{i}@test.com",
                    ContactNumber = $"0{i.ToString().PadLeft(9, '0')}", 
                    Password = $"TestPass{i}!123"
                });
            }
            return users;
        }

        [PerfBenchmark(
            Description = "Tests the performance of user registration under load",
            NumberOfIterations = 100,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 10000,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion("OperationsPerSecond", MustBe.GreaterThan, 20)]
        [CounterThroughputAssertion("ErrorsPerSecond", MustBe.LessThanOrEqualTo, 2)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo, 31457280)] 
        public void UserRegistrationBenchmark()
        {
            try
            {
                
                var randomIndex = new Random().Next(_testUsers.Count);
                var testUser = _testUsers[randomIndex];

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO users(userName, lastName, Email, contactNumb, Password) " +
                                 "VALUES(@userName, @lastName, @Email, @ContactNumb, @Password)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userName", testUser.UserName);
                        command.Parameters.AddWithValue("@lastName", testUser.LastName);
                        command.Parameters.AddWithValue("@Email", testUser.Email);
                        command.Parameters.AddWithValue("@ContactNumb", testUser.ContactNumber);
                        command.Parameters.AddWithValue("@Password", testUser.Password);

                        command.ExecuteNonQuery();
                        _opsCounter.Increment();
                    }
                }
            }
            catch (Exception ex)
            {
                _errorCounter.Increment();
                Console.WriteLine($"Error in UserRegistrationBenchmark: {ex.Message}");
            }
        }

        [PerfBenchmark(
            Description = "Tests concurrent user registration attempts",
            NumberOfIterations = 50,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 15000,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion("OperationsPerSecond", MustBe.GreaterThan, 15)]
        [CounterThroughputAssertion("ErrorsPerSecond", MustBe.LessThanOrEqualTo, 3)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo, 41943040)] 
        public void ConcurrentUserRegistrationBenchmark()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++) 
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var randomIndex = new Random().Next(_testUsers.Count);
                        var testUser = _testUsers[randomIndex];

                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            string query = "INSERT INTO users(userName, lastName, Email, contactNumb, Password) " +
                                         "VALUES(@userName, @lastName, @Email, @ContactNumb, @Password)";

                            using (var command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@userName", testUser.UserName);
                                command.Parameters.AddWithValue("@lastName", testUser.LastName);
                                command.Parameters.AddWithValue("@Email", testUser.Email);
                                command.Parameters.AddWithValue("@ContactNumb", testUser.ContactNumber);
                                command.Parameters.AddWithValue("@Password", testUser.Password);

                                command.ExecuteNonQuery();
                                _opsCounter.Increment();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _errorCounter.Increment();
                        Console.WriteLine($"Error in ConcurrentUserRegistrationBenchmark: {ex.Message}");
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
        }

        [PerfCleanup]
        public void Cleanup()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM users WHERE userName LIKE 'TestUser%'";
                    using (var command = new SqlCommand(query, connection))
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
