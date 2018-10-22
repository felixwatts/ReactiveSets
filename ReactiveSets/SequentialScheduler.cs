using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

public class SequentialScheduler : IScheduler, IDisposable
{
    private readonly ConcurrentQueue<Action> _taskQueue;
    private readonly CancellationTokenSource _canceler;

    public DateTimeOffset Now => DateTimeOffset.Now;

    public SequentialScheduler()
    {
        _canceler = new CancellationTokenSource();
        _taskQueue = new ConcurrentQueue<Action>();
        Task.Factory.StartNew(WorkLoop, TaskCreationOptions.LongRunning, _canceler.Token);
    }

    public void Dispose()
    {
        _canceler.Cancel();
    }

    public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
    {
        _taskQueue.Enqueue(() => action(this, state));
        return Disposable.Empty;
    }

    public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        return Schedule(state, action);
    }

    public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        return Schedule(state, action);
    }

    private void WorkLoop(object _)
    {
        try
        {
            while(!_canceler.IsCancellationRequested)
            {
                if(_taskQueue.TryDequeue(out var task))
                {
                    task();
                }
            }
        }
        catch(TaskCanceledException)
        {
            // ignore
        }
    }
}