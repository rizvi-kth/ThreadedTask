using System;
using System.ComponentModel;
using System.Linq;
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

        public static int _threadStaticfield= 0;
        public void TestThreadStatic()
        {
            // A thread has its own call stack that stores all the methods that are executed.
            // Local variables are stored on the call stack and are private to the thread.
            // A thread can also have its own data that’s not a local variable.
            // By marking a field with the ThreadStatic attribute, each thread gets its own copy of a field.

            Parallel.Invoke(() => increaseThreadStaticfield(10),
                            () => increaseThreadStaticfield(100),
                            () => increaseThreadStaticfield(1000),
                            () => increaseThreadStaticfield(10000),
                            () => increaseThreadStaticfield(100000),
                            () => increaseThreadStaticfield(1000000)
                );

        }

        public void increaseThreadStaticfield(int p)
        {
            int len = p.ToString().Length;
            string threadIdentifyer = "*".PadLeft(len,'*'); 

            for (int i = 0; i < 10; i++)
            {
                Console.Write("Thread# {0} ", threadIdentifyer);
                Console.WriteLine(" P: {0}", p++);
                
            }
        }

        public static ThreadLocal<int> _field = new ThreadLocal<int>(() =>
                                                    {
                                                        // Field value is initialized with current ThreadID
                                                        return Thread.CurrentThread.ManagedThreadId;
                                                    });
        public void TestThreadLocal()
        {
            // If you want to use local data in a thread and INITIALIZE it for each thread, 
            // you can use the ThreadLocal<T> class. 
            // Without a ThreadLocal<T> all the variables in the main thread 
            // is shared among children, so needs locking to avoid conflict. 

            // The following two thread has their own copy of the _field;
            // even it is a Main thread (static)variable.

            new Thread(() =>
            {
                for (int x = 0; x < 20; x++)
                {
                    Console.WriteLine("Thread A(_field.value:{1}): i:{0}", x, _field.Value++);
                }
            }).Start();
            new Thread(() =>
            {
                for (int x = 0; x < 20; x++)
                {
                    Console.WriteLine("Thread B(_field.value:{1}):           i:{0}", x, _field.Value++);
                }
            }).Start();
        }

        public void TestThreadPool()
        {
            const int UPPER = 10;
            for (int i = 0; i < UPPER; i++)
            {
                // With out state information
                ThreadPool.QueueUserWorkItem((state) => Console.WriteLine("Without state info S is :{0}", state ));
            }

            for (int i = 0; i < UPPER; i++)
            {
                // With state information
                ThreadPool.QueueUserWorkItem((state) => Console.WriteLine("With state info S is :{0}", (int)state), i);
            }

        }

        public void TestTask_ContinueWith()
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

            // If the parent does not wait here all the assignments will not complete properly.
            // <Try comment this line and observe!>
            parentTask.Wait();

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

        public void TestTaskWithLamdaInLoop()
        {
            // Create a task in a loop and pass in the loop counter in the task.
            // This produces an UNEXPECTED RESULT.
            // The main thread which is incrementing the value of i, will come to a rece condition 
            // with a new threade created in the loop.
            // A time-lag required to create a thread will fail the new thread in the race of accessing the value of i.
            Console.WriteLine("\nInvalid Task# for the 10 threads:\n");
            Task[] taskArray = new Task[10];
            for (int i = 0; i < taskArray.Length; i++)
            {
                taskArray[i] = Task.Factory.StartNew(() => {
                                                                var data = new CustomData()
                                                                {
                                                                    Name = i,
                                                                    CreationTime = DateTime.Now.Ticks,
                                                                    ThreadNum = Thread.CurrentThread.ManagedThreadId
                                                                };
                                                                Console.WriteLine("Task #{0} created at {1} on thread #{2}.", data.Name, data.CreationTime,data.ThreadNum);
                                                            });
            }
            Task.WaitAll(taskArray);


            // You can access the value on each iteration by providing a state object to a task through its constructor. 
            // The following example modifies the previous example by passsing the CustomData object 
            // as an argument to the StartNew() method which, in turn, is passed to the lambda expression.
            // This produces the EXPECTED RESULT.
            // Now the value of i(for-loop) will be captited when the Thread is declared, not when the thread starts.
            // This will stop race condition and solve the problem.
            Console.WriteLine("\nFixed Task# for the 10 threads:\n");
            Task[] taskArray2 = new Task[10];
            for (int i = 0; i < taskArray2.Length; i++)
            {
                taskArray2[i] = Task.Factory.StartNew((Object obj) => {
                                                                        var data = obj as CustomData;
                                                                        data.ThreadNum = Thread.CurrentThread.ManagedThreadId;
                                                                        Console.WriteLine("Task #{0} created at {1} on thread #{2}.", data.Name, data.CreationTime, data.ThreadNum);
                                                                    }, new CustomData(){Name = i,CreationTime = DateTime.Now.Ticks});
            }
            Task.WaitAll(taskArray2);

            // This state is passed as an argument to the task delegate, and 
            // it can be accessed from the task object by using the Task.AsyncState property.
            Console.WriteLine("\nThis state can be accessed with Task.AsyncState property even after the Tasks are completed :\n");
            foreach (var task in taskArray2)
            {
                var data = task.AsyncState as CustomData;
                if (data != null)
                    Console.WriteLine("Task #{0} created at {1}, ran on thread #{2}.",
                                      data.Name, data.CreationTime, data.ThreadNum);
            }



        }

        public void TestTaskWithLamdaInLoop2()
        {
            Action<int>[] jobs = new Action<int>[10];
            for(int p= 0; p < jobs.Length; p++)
            {
                jobs[p] = (indx)=> { Console.WriteLine("Job running # {0}", indx);};
            }

            
            Task[] tasks = new Task[10];
            for(int i=0;i < tasks.Length; i++)
            {
                // This produces the UNEXPECTED RESULT.
                //tasks[i] = new Task(()=> RunJob((indx) => { Console.WriteLine("Job running # {0}", indx); }, i)); 

                // This produces the EXPECTED RESULT.
                tasks[i] = new Task((obj) =>
                {
                    RunJob(
                          (ob) => { Console.WriteLine("Job running # {0}", (int)ob); }
                        , (int)obj);
                }, i);
                
                
                // ** With Tasks are in an array **
                
                // This produces the RUNTIME EXCEPTION. 
                //tasks[i] = new Task(() => RunJob(jobs[i], i)); 

                // This produces the EXPECTED RESULT. 
                //tasks[i] = new Task((obj) => RunJob(jobs[(int)obj], (int)obj), i); 


                tasks[i].Start();
                
            }
            
            Task.WaitAll(tasks);
        }

        public void RunJob(Action<int> act, int ix)
        {
            act(ix);
        }

        public void TestParallel()
        {
            //-- Creating and running tasks implicitly --
            //The Parallel class provides a convenient way to run any number of arbitrary statements concurrently.
            //The Parallel class has a couple of static methods—For, ForEach, and Invoke—that you can use to parallelize work.
            //Just pass in an Action delegate for each item of work.The easiest way to create these delegates is to use lambda
            //expressions.The lambda expression can either call a named method or provide the code inline.


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

        public void ParallelBreak()
        {
            //You can cancel the loop by using the ParallelLoopState object.
            //You have two options to do this: Break or Stop.
            //Break ensures that all iterations that are currently running will be finished. 
            //Stop just terminates everything.

            ParallelLoopResult result = Parallel.
              For(0, 1000, (int i, ParallelLoopState loopState) =>
                                                                  {
                                                                      if (i == 500)
                                                                      {
                                                                          Console.WriteLine("Breaking loop");
                                                                          loopState.Break();
                                                                        //Console.WriteLine("Stoping loop");
                                                                        //loopState.Stop();
                                                                    }
                                                                  });

            Console.WriteLine("IsCompleted:{0}", result.IsCompleted ? "true" : "false");
            Console.WriteLine("LowestBreakIteration:{0}", result.LowestBreakIteration.HasValue ? result.LowestBreakIteration.ToString() : "NULL");

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

    class CustomData
    {
        public long CreationTime;
        public int Name;
        public int ThreadNum;
    }
}