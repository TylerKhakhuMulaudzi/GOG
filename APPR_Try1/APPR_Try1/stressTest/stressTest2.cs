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
    public class DisasterReportStressTest
    {
        private Counter _syncOperationsCounter;
        private Counter _asyncOperationsCounter;
        private List<ReportModels> _testReports;
        private const string SyncOperationsCounterName = "SyncOperationsCounter";
        private const string AsyncOperationsCounterName = "AsyncOperationsCounter";

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            try
            {
                _syncOperationsCounter = context.GetCounter(SyncOperationsCounterName);
                _asyncOperationsCounter = context.GetCounter(AsyncOperationsCounterName);

                _testReports = GenerateTestReports() ?? throw new InvalidOperationException("Test reports generation failed.");

                context.Trace.Info("Setup completed successfully.");
            }
            catch (Exception ex)
            {
                context.Trace.Error(ex.Message);
                throw;
            }
        }

        [PerfBenchmark(
            NumberOfIterations = 3,
            RunMode = RunMode.Throughput,
            TestMode = TestMode.Measurement)]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 500)]
        public void SyncReportSubmissionTest(BenchmarkContext context)
        {
            try
            {
                if (_testReports == null)
                    throw new InvalidOperationException("Test reports are not initialized.");

                foreach (var report in _testReports.Take(50))
                {
                    _syncOperationsCounter.Increment();
                }
            }
            catch (Exception ex)
            {
                context.Trace.Error(ex.Message);
                throw;
            }
        }

        [PerfBenchmark(
            NumberOfIterations = 3,
            RunMode = RunMode.Throughput,
            TestMode = TestMode.Measurement)]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 500)]
        public async Task AsyncReportSubmissionTest(BenchmarkContext context)
        {
            try
            {
                if (_testReports == null)
                    throw new InvalidOperationException("Test reports are not initialized.");

                foreach (var report in _testReports.Take(50))
                {
                    await Task.Run(() => _asyncOperationsCounter.Increment());
                }
            }
            catch (Exception ex)
            {
                context.Trace.Error(ex.Message);
                throw;
            }
        }

        [PerfCleanup]
        public void Cleanup(BenchmarkContext context) 
        {
            try
            {
                context.Trace.Info("Cleanup completed.");
            }
            catch (Exception ex)
            {
                context.Trace.Error(ex.Message);
            }
        }

        private List<ReportModels> GenerateTestReports()
        {
            return new List<ReportModels> { new ReportModels(), new ReportModels() };
        }
    }
}
