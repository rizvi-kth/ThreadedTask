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
            //ts.TestThreadStatic();
            //ts.TestThreadLocal();
            //ts.TestThreadPool();
            //ts.TestTaskContinueWith();
            ts.TestTaskCancelation();
            //ts.TestTaskParent();
            //ts.TestTaskFactory();
            //ts.TestTaskWaitAll();
            //ts.TestTaskWithLamdaInLoop();
            //ts.TestTaskWithLamdaInLoop2();
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
