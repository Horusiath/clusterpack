using System;
using System.Collections.Generic;
using System.Threading;

namespace ClusterPack
{
    /// <summary>
    /// Timer interface used to schedule actions to be invoked after
    /// some initial dealy or periodicaly.
    /// </summary>
    public interface ITimer : IDisposable
    {
        void After(TimeSpan delay, Action action);
        void Every(TimeSpan delay, TimeSpan interval, Action action);
        event EventHandler<TimerFailure> OnError;
    }

    /// <summary>
    /// Container with all informations about failing scheduled calls.
    /// </summary>
    public sealed class TimerFailure
    {
        /// <summary>
        /// Date provided by calling timer, at which a call has failed.
        /// </summary>
        public DateTime CallTime { get; }

        /// <summary>
        /// An exception that caused a failure.
        /// </summary>
        public Exception Cause { get; }
        
        /// <summary>
        /// An action, which failed to execute.
        /// </summary>
        public Action Callback { get; }

        public TimerFailure(DateTime callTime, Exception cause, Action callback)
        {
            CallTime = callTime;
            Cause = cause;
            Callback = callback;
        }
    }

    internal struct TimerCall : IComparable<TimerCall>
    {
        public static readonly TimerCall Min = At(DateTime.MinValue);
        public static TimerCall At(DateTime date) => new TimerCall(null, date);

        public Action Call { get; }
        public TimeSpan? Interval { get; }
        public DateTime NextCallDate { get; }

        public TimerCall(Action call, DateTime nextCallDate, TimeSpan? interval = null)
        {
            NextCallDate = nextCallDate;
            Call = call;
            Interval = interval;
        }

        public bool TryAdvance(out TimerCall next)
        {
            if (!Interval.HasValue)
            {
                next = Min;
                return false;
            }
            
            next = new TimerCall(Call, NextCallDate + Interval.Value, Interval);
            return true;
        }

        public int CompareTo(TimerCall other) => 
            NextCallDate.CompareTo(other.NextCallDate);
    }

    public abstract class AbstractTimer : ITimer
    {
        public event EventHandler<TimerFailure> OnError; 

        private bool disposed = false;
        private object syncLock = new object();
        private readonly SortedSet<TimerCall> calls = new SortedSet<TimerCall>();

        protected abstract void SafeDispose();

        public void Dispose()
        {
            if (disposed) return;
            lock (syncLock)
            {
                this.disposed = true;
                this.SafeDispose();
            }
        }

        public void After(TimeSpan delay, Action action) => 
            Schedule(action, delay);

        public void Every(TimeSpan delay, TimeSpan interval, Action action) => 
            Schedule(action, delay, interval);

        private void Schedule(Action action, TimeSpan delay, TimeSpan? interval = null)
        {
            if (disposed) throw new ObjectDisposedException("Timer has been disposed");

            var timerCall = new TimerCall(action, DateTime.UtcNow + delay, interval);
            lock (syncLock)
            {
                this.calls.Add(timerCall);
            }
        }

        protected void Callback(DateTime now)
        {
            var boundary = TimerCall.At(now);
            SortedSet<TimerCall> toCall;

            // get all calls scheduler before infinity and now remove 
            // them once from the set of awaiting calls then try to 
            // advance them and scheduler for a next call
            lock (syncLock)
            {
                toCall = this.calls.GetViewBetween(TimerCall.Min, boundary);
                this.calls.ExceptWith(toCall);
                foreach (var call in toCall)
                {
                    if (call.TryAdvance(out var nextCall))
                    {
                        this.calls.Add(nextCall);
                    }
                }
            }
            
            foreach (var call in toCall)
            {
                try
                {
                    call.Call();
                }
                catch (Exception cause)
                {
                    var failure = new TimerFailure(now, cause, call.Call);
                    this.OnError?.Invoke(this, failure);
                }
            }
        }
    }

    /// <summary>
    /// Default timer implementation.
    /// </summary>
    public class DefaultTimer : AbstractTimer
    {
        private readonly Timer inner;

        public DefaultTimer(TimeSpan frequency)
        {
            this.inner = new Timer(_ => this.Callback(DateTime.UtcNow), null, frequency, frequency);
        }

        protected override void SafeDispose()
        {
            this.inner.Dispose();
        }
    }

    /// <summary>
    /// Virtual timer allows to perform actions in distinction from a real 
    /// system timer.
    /// </summary>
    public class VirtualTimer : AbstractTimer
    {
        private DateTime currentTime;

        public DateTime Current
        {
            get { return currentTime; }

            set
            {
                if (this.currentTime > value)
                    throw new InvalidOperationException("Moving backwards in time is not allowed in virtual timer");

                this.Callback(value);
                this.currentTime = value;
            }
        }
        
        public VirtualTimer(DateTime initialTime)
        {
            this.currentTime = initialTime;
        }

        protected override void SafeDispose()
        {
        }
    }
}