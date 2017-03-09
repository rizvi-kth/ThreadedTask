using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasks
{
    class Program
    {
        static void Main(string[] args)
        {
            var ts = new TestTask();
            //ts.TestTask_ContinueWith();
            //ts.TestTaskParent();
            //ts.TestTaskFactory();
            //ts.TestTaskWaitAll();
            ts.TestTaskWithLamdaInLoop();
            //ts.TestParallel();
            //ts.ParallelBreak();
            //ts.TestRaceCondition();
            //ts.TestLock();
            //ts.TestLockFreeLocal();


            Console.WriteLine(">> Press any key to exit.");
            Console.ReadKey();

        }
    }

   
}
