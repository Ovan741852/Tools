using System;
using System.Diagnostics;

namespace UNONE.Foundation.Utilities
{
    public class FiniteState<T> where T : struct, IConvertible
    {
        public FiniteState(T value)
        {
            Value = value;
            _stopwatch = new Stopwatch();
        }

        private T _next;
        private Stopwatch _stopwatch;

        public T Value { get; private set; }
        public T Prev { get; private set; }
        public bool IsTrasiting { get; private set; }
        public bool IsFirstFrame { get; private set; }
        public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;
        public float ElapsedSeconds => _stopwatch.ElapsedMilliseconds * 0.001f;

        public T MoveNext()
        {
            IsFirstFrame = false;

            if (IsTrasiting)
            {
                IsTrasiting = false;
                IsFirstFrame = true;
                Prev = Value;
                Value = _next;
                _stopwatch.Reset();
            }

            return Value;
        }

        public void Transit(T next)
        {
            if (Value.Equals(next))
                return;
            _next = next;
            IsTrasiting = true;
        }
    }
}