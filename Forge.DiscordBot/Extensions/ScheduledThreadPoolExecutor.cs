using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Forge.DiscordBot.Extensions
{

    namespace BattleCupBot.Extensions
    {
        public class ScheduledThreadPoolExecutor
        {
            public int ThreadCount => _threads.Length;
            public EventHandler<Exception> OnException;

            private readonly ManualResetEvent _waiter;
            private readonly Thread[] _threads;
            private readonly SortedSet<Tuple<DateTime, Action>> _queue;

            public ScheduledThreadPoolExecutor(int threadCount)
            {
                _waiter = new ManualResetEvent(false);
                _queue = new SortedSet<Tuple<DateTime, Action>>();
                OnException += (o, e) => { };
                _threads = Enumerable.Range(0, threadCount).Select(i => new Thread(ProcessLoop)).ToArray();
                foreach (var thread in _threads)
                {
                    thread.Start();
                }
            }

            public SortedSet<Tuple<DateTime, Action>> GetTuples()
            {
                lock (_waiter)
                {
                    return _queue;
                }
            }

            public void ScheduleWithFixedDelay(Action action, TimeSpan initialDelay, TimeSpan delay)
            {
                Schedule(() =>
                {
                    action();
                    ScheduleWithFixedDelay(action, delay, delay);
                }, DateTime.Now + initialDelay);
            }

            public void Schedule(Action action, TimeSpan initialDelay)
            {
                Schedule(action, DateTime.Now + initialDelay);
            }

            private void Schedule(Action action, DateTime time)
            {
                lock (_waiter)
                {
                    _queue.Add(Tuple.Create(time, action));
                }
                _waiter.Set();
            }

            public void ScheduleAtFixedRate(Action action, TimeSpan initialDelay, TimeSpan delay)
            {
                DateTime scheduleTime = DateTime.Now + initialDelay;

                void RegisterTask()
                {
                    Schedule(() =>
                    {
                        action();
                        scheduleTime += delay;
                        RegisterTask();
                    }, scheduleTime);
                }

                RegisterTask();
            }

            private void ProcessLoop()
            {
                while (true)
                {
                    TimeSpan sleepingTime = TimeSpan.MaxValue;
                    bool needToSleep = true;
                    Action task = null;

                    try
                    {
                        lock (_waiter)
                        {
                            if (_queue.Any())
                            {
                                if (_queue.First().Item1 < DateTime.Now)
                                {
                                    task = _queue.First().Item2;
                                    _queue.Remove(_queue.First());
                                    needToSleep = false;
                                }
                                else
                                {
                                    sleepingTime = _queue.First().Item1 - DateTime.Now;
                                }
                            }
                        }

                        if (needToSleep)
                        {
                            _waiter.WaitOne((int)sleepingTime.TotalMilliseconds);
                        }
                        else
                        {
                            task();
                        }
                    }
                    catch (Exception e)
                    {
                        OnException(task, e);
                    }
                }
            }
        }
    }

}
