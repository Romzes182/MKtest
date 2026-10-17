using System;
using System.Collections.Generic;

namespace MKtest.Services.Logging
{
    /// <summary>
    /// Потокобезопасная история последних записей без бесконечного роста памяти.
    /// </summary>
    public sealed class BoundedLogHistory
    {
        private readonly Queue<string> _messages = new();
        private readonly object _sync = new();
        private readonly int _capacity;

        public BoundedLogHistory(int capacity = 1000)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _capacity = capacity;
        }

        public void Add(string message)
        {
            lock (_sync)
            {
                if (_messages.Count >= _capacity)
                    _messages.Dequeue();
                _messages.Enqueue(message);
            }
        }

        public IReadOnlyList<string> Snapshot()
        {
            lock (_sync)
                return _messages.ToArray();
        }
    }
}
