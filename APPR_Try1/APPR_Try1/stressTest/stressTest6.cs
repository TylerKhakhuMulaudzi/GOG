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
    public class UserAuthenticationPerfTests
    {
        private readonly string connectionString = "Server=tcp:tyler-appr-server.database.windows.net,1433;Initial Catalog=tyler-database;Persist Security Info=False;User ID=ST10173943;Password=Mulaudzi2001;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private Counter _loginAttemptCounter;
        private Counter _successfulLoginCounter;
        private Counter _failedLoginCounter;
        private Counter _errorCounter;
        private signUpModel _signUpModel;
        private List<SignInUserModel> _validTestUsers;
        private List<SignInUserModel> _invalidTestUsers;

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            _loginAttemptCounter = context.GetCounter("LoginAttemptsPerSecond");
            _successfulLoginCounter = context.GetCounter("SuccessfulLoginsPerSecond");
            _failedLoginCounter = context.GetCounter("FailedLoginsPerSecond");
            _errorCounter = context.GetCounter("ErrorsPerSecond");

            _signUpModel = new signUpModel();

            _validTestUsers = GenerateValidTestUsers(500);
            _invalidTestUsers = GenerateInvalidTestUsers(500);

            SetupTestUsers();
        }

        private List<SignInUserModel> GenerateValidTestUsers(int count)
        {
            var users = new List<SignInUserModel>();
            for (int i = 0; i < count; i++)
            {
                users.Add(new SignInUserModel
                {
                    Email = $"validuser{i}@test.com",
                    Password = $"ValidPass{i}!123"
                });
            }
            return users;
        }

        private List<SignInUserModel> GenerateInvalidTestUsers(int count)
        {
            var users = new List<SignInUserModel>();
            for (int i = 0; i < count; i++)
            {
                users.Add(new SignInUserModel
                {
                    Email = $"invaliduser{i}@test.com",
                    Password = $"InvalidPass{i}!123"
                });
            }
            return users;
        }

        private void SetupTestUsers()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    foreach (var user in _validTestUsers.Take(10)) 
                    {
                        string query = "INSERT INTO users(Email, Password, userName) VALUES(@Email, @Password, @UserName)";
                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Email", user.Email);
                            command.Parameters.AddWithValue("@Password", user.Password);
                            command.Parameters.AddWithValue("@UserName", "TestUser");
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up test users: {ex.Message}");
            }
        }

        [PerfBenchmark(
            Description = "Tests authentication performance with valid credentials",
            NumberOfIterations = 100,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 10000,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion("LoginAttemptsPerSecond", MustBe.GreaterThan, 50)]
        [CounterThroughputAssertion("SuccessfulLoginsPerSecond", MustBe.GreaterThan, 45)]
        [CounterThroughputAssertion("ErrorsPerSecond", MustBe.LessThanOrEqualTo, 1)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo, 26214400)] 
        public void ValidCredentialAuthenticationBenchmark()
        {
            try
            {
                var randomIndex = new Random().Next(10); 
                var testUser = _validTestUsers[randomIndex];

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(1) FROM users WHERE Email=@Email AND Password=@Password";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", testUser.Email);
                        command.Parameters.AddWithValue("@Password", testUser.Password);

                        _loginAttemptCounter.Increment();
                        int count = (int)command.ExecuteScalar();

                        if (count > 0)
                            _successfulLoginCounter.Increment();
                        else
                            _failedLoginCounter.Increment();
                    }
                }
            }
            catch (Exception ex)
            {
                _errorCounter.Increment();
                Console.WriteLine($"Error in ValidCredentialAuthenticationBenchmark: {ex.Message}");
            }
        }

        [PerfBenchmark(
            Description = "Tests authentication performance with invalid credentials",
            NumberOfIterations = 50,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 10000,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion("LoginAttemptsPerSecond", MustBe.GreaterThan, 50)]
        [CounterThroughputAssertion("FailedLoginsPerSecond", MustBe.GreaterThan, 45)]
        [CounterThroughputAssertion("ErrorsPerSecond", MustBe.LessThanOrEqualTo, 1)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo, 26214400)] 
        public void InvalidCredentialAuthenticationBenchmark()
        {
            try
            {
                var randomIndex = new Random().Next(_invalidTestUsers.Count);
                var testUser = _invalidTestUsers[randomIndex];

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(1) FROM users WHERE Email=@Email AND Password=@Password";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", testUser.Email);
                        command.Parameters.AddWithValue("@Password", testUser.Password);

                        _loginAttemptCounter.Increment();
                        int count = (int)command.ExecuteScalar();

                        if (count > 0)
                            _successfulLoginCounter.Increment();
                        else
                            _failedLoginCounter.Increment();
                    }
                }
            }
            catch (Exception ex)
            {
                _errorCounter.Increment();
                Console.WriteLine($"Error in InvalidCredentialAuthenticationBenchmark: {ex.Message}");
            }
        }

        [PerfBenchmark(
            Description = "Tests concurrent authentication attempts",
            NumberOfIterations = 25,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 15000,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion("LoginAttemptsPerSecond", MustBe.GreaterThan, 100)]
        [CounterThroughputAssertion("ErrorsPerSecond", MustBe.LessThanOrEqualTo, 5)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo, 52428800)] 
        public void ConcurrentAuthenticationBenchmark()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++) 
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        bool useValidUser = new Random().Next(2) == 0;
                        var testUser = useValidUser
                            ? _validTestUsers[new Random().Next(10)]
                            : _invalidTestUsers[new Random().Next(_invalidTestUsers.Count)];

                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            string query = "SELECT COUNT(1) FROM users WHERE Email=@Email AND Password=@Password";
                            using (var command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@Email", testUser.Email);
                                command.Parameters.AddWithValue("@Password", testUser.Password);

                                _loginAttemptCounter.Increment();
                                int count = (int)command.ExecuteScalar();

                                if (count > 0)
                                    _successfulLoginCounter.Increment();
                                else
                                    _failedLoginCounter.Increment();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _errorCounter.Increment();
                        Console.WriteLine($"Error in ConcurrentAuthenticationBenchmark: {ex.Message}");
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
                    string query = "DELETE FROM users WHERE Email LIKE 'validuser%@test.com'";
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
