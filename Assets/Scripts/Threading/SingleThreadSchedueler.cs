using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

//Custom task scheduler that provides inline task execution on a separate thread.
public class SingleThreadSchedueler : TaskScheduler
{
    [ThreadStatic]

    private readonly LinkedList<Task> _tasks = new LinkedList<Task>();

    private int _delegatesQueuedOrRunning = 0;

    protected sealed override void QueueTask(Task task)
    {
        lock (_tasks)
        {
            _tasks.AddLast(task);
            if (_delegatesQueuedOrRunning < 1)
            {
                ++_delegatesQueuedOrRunning;
                NotifyThreadPoolOfPendingWork();
            }
        }
    }

    private void NotifyThreadPoolOfPendingWork()
    {
        ThreadPool.UnsafeQueueUserWorkItem(_ =>
        {
            try
            {
                while (true)
                {
                    Task item;
                    lock (_tasks)
                    {
                        if (_tasks.Count == 0)
                        {
                            --_delegatesQueuedOrRunning;
                            break;
                        }

                        item = _tasks.First.Value;
                        _tasks.RemoveFirst();
                    }

                    base.TryExecuteTask(item);
                }
            }
            finally { }
        }, null);
    }

    protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
         return false;
    }

    protected sealed override bool TryDequeue(Task task)
    {
        lock (_tasks) return _tasks.Remove(task);
    }

    protected sealed override IEnumerable<Task> GetScheduledTasks()
    {
        bool lockTaken = false;
        try
        {
            Monitor.TryEnter(_tasks, ref lockTaken);
            if (lockTaken) return _tasks;
            else throw new NotSupportedException();
        }
        finally
        {
            if (lockTaken) Monitor.Exit(_tasks);
        }
    }

}
