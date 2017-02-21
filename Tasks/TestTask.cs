using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Tasks
{
    internal class TestTask
    {
        public TestTask()
        {

        }

        public void TestTaskSimple()
        {
            Task<int> t1 = Task.Run(() =>
            {
                Thread.Sleep(3000);
                Console.WriteLine("Returning within the Thread...");
                //throw new Exception("Error");
                return 50;
            });

            t1.ContinueWith((t) =>
            {
                Console.WriteLine("ContinueWith: canceled!");
            }, TaskContinuationOptions.OnlyOnCanceled);

            t1.ContinueWith((t) =>
            {
                Console.WriteLine("ContinueWith: faulted!");
            }, TaskContinuationOptions.OnlyOnFaulted);

            t1.ContinueWith((t) =>
            {
                Console.WriteLine("ContinueWith: completed successfully! Value:{0}", t.Result);
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            //Console.WriteLine(t1.Result);
            t1.Wait();
        }

        public void TestTaskParent()
        {
            Task<Int32[]> parentTask = Task.Run(() =>
            {
                var results = new Int32[3];
                new Task(()=> { results[0] = 1; Thread.Sleep(1000); }, TaskCreationOptions.AttachedToParent ).Start();
                new Task(()=> { results[1] = 2; Thread.Sleep(1000); }, TaskCreationOptions.AttachedToParent ).Start();
                new Task(()=> { results[2] = 3; Thread.Sleep(1000); }, TaskCreationOptions.AttachedToParent ).Start();
                
                return results;
            });

            var finalTask = parentTask.ContinueWith((parent) =>
            {
                foreach (var i in parent.Result)
                {
                    Console.WriteLine("Value: {0}", i);
                }
            });

            // The finalTask runs only after the parentTask is finished, and the parent Task finishes when all three children are finished.
            finalTask.Wait();
        }

        public void TestTaskFactory()
        {
            Task<Int32[]> parentTask = Task.Run(() =>
            {
                var results = new Int32[3];
                // A TaskFactory is created with a certain configuration and can then be used to create Tasks with that configuration.
                TaskFactory tf = new TaskFactory(TaskCreationOptions.AttachedToParent, TaskContinuationOptions.None);
                tf.StartNew(() => { results[0] = 1; Thread.Sleep(1000); });
                tf.StartNew(() => { results[1] = 2; Thread.Sleep(1000); });
                tf.StartNew(() => { results[2] = 3; Thread.Sleep(1000); });

                return results; 
            });

            var finalTask = parentTask.ContinueWith((parent) =>
            {
                foreach (var i in parent.Result)
                {
                    Console.WriteLine("Value: {0}", i);
                }
            },TaskContinuationOptions.OnlyOnRanToCompletion);

            // The finalTask runs only after the parentTask is finished, and the parent Task finishes when all three children are finished.
            finalTask.Wait();
        }

        public void TestTaskWaitAll()
        {
            Task<Int32[]> parentTask = Task.Run(() =>
            {
                var results = new Int32[3];
                Task[] tf = new Task[3];
                tf[0] = Task.Run(() => { results[0] = 1; Thread.Sleep(1000); });
                tf[1] = Task.Run(() => { results[1] = 2; Thread.Sleep(2000); });
                tf[2] = Task.Run(() => { results[2] = 3; Thread.Sleep(3000); });

                // Method WaitAll() to wait for multiple Tasks to finish before continuing execution.
                Task.WaitAll(tf);

                return results;
            });

            var finalTask = parentTask.ContinueWith((parent) =>
            {
                foreach (var i in parent.Result)
                {
                    Console.WriteLine("Value: {0}", i);
                }
            });

            //finalTask.Wait();
        }

        public void TestParallel()
        {
            // Comparison Parallel For VS Normal For

            // PARALLEL FOR
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Parallel.For(0, 100000, (i) =>
            {
                if (isPrime(i))
                {
                    //Console.WriteLine("{0} is Prime.", i);
                }
                else
                {
                    //Console.WriteLine("{0} is not Prime.", i);
                }
            });
            watch.Stop();
            Console.WriteLine("Time took {0} Milliseconds", watch.ElapsedMilliseconds);


            // NORMAL FOR
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //for (int j = 0; j < 100000; j++)
            //{
            //    if (isPrime(j))
            //    {
            //        Console.WriteLine("{0} is Prime.", j);
            //    }
            //    else
            //    {
            //        Console.WriteLine("{0} is not Prime.", j);
            //    }
            //}
            //watch.Stop();
            //Console.WriteLine("Time took {0} Milliseconds", watch.ElapsedMilliseconds);



        }

        public static bool isPrime(int number)
        {
            if (number == 1) return false;
            if (number == 2) return true;

            var boundary = (int)Math.Floor(Math.Sqrt(number));

            for (int i = 2; i <= boundary; ++i)
            {
                if (number % i == 0) return false;
            }

            return true;
        }

        public void TestRaceCondition()
        {

            int n = 0;
            Task[] tasks = new Task[2];
            tasks [0] = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    n++;
                }
            });
            tasks[1] = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    n--;
                }
            });

            Task.WaitAll(tasks);
            // Value of n is different in every execution. 
            // This is happened for race condition.
            // Implement lock to avoid race condition.
            Console.WriteLine("The value of n:{0}",n);
        }


        public void TestLock()
        {
            int n = 0;
            var l = new object();
            Task[] tasks = new Task[2];
            tasks[0] = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    lock (l) { n++; }
                }
            });
            tasks[1] = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    // Keep the locking region as small as possible.
                    // Other threads will have to wait for long to get access to n - thus hindering performance.
                    // If you use multiple locks then preserve the order in all the threads
                    lock (l) { n--; }
                }
            });

            Task.WaitAll(tasks);
            // Value of n is same in every execution. 
            // This is happened for eleminating race condition.
            Console.WriteLine("The value of n:{0}", n);
        }

        public void TestLockFreeLocal()
        {
            //int n = 0;
            Task<int>[] tasks = new Task<int>[2];
            tasks[0] = Task.Factory.StartNew(() =>
            {
                //Implement local variable for each thread
                int n1Local = 0;
                for (int i = 0; i < 100000; i++)
                {
                    n1Local++;
                }
                return n1Local; 
            });

            tasks[1] = Task.Factory.StartNew(() =>
            {
                //Implement local variable for each thread
                int n2Local = 0;
                for (int i = 0; i < 100000; i++)
                {
                    n2Local--;
                }
                return n2Local;
            });

            // Return local value from the tasks and agregate the results.
            var result = tasks[0].Result + tasks[1].Result;

            // Used LOCAL VARIABLES to avoid locking and race condition.
            // Not all use case can be implemented but it is better than locking solution.
            // Better in performance than locking solutioin.
            Console.WriteLine("The value of n:{0}", result);

        }
    }
}