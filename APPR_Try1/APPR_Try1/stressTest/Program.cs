using NBench;
using System.Reflection;

namespace stressTest 
{
    internal class Program
    { 
    static int Main(string[] args) 
        {
            return NBenchRunner.Run<Program>();
        }
    }
}