using System.Collections.Generic;
using System.Threading;


namespace SHA_Signature
{
    /// <summary>
	/// Organize Queue for threads
	/// </summary>
    class TaskPool
    {
        private Queue<KeyValuePair<int, byte[]>> _taskPool;

        private readonly int _cores;

        private bool _isFinished = false;

        public TaskPool(int capacity)
        {
            _taskPool = new Queue<KeyValuePair<int, byte[]>>(capacity);
            _cores = capacity;
        }

        /// <summary>
        /// Put data to the Queue
        /// </summary>
        public bool TrySet(int blockNumber, byte[] blockValue)
        {
            lock (_taskPool)
            {
                while (_taskPool.Count >= _cores)
                {
                    if (_isFinished)
                    {
                        return false;
                    }

                    Monitor.Wait(_taskPool);
                }

                if (_isFinished)
                {
                    return false;
                }

                _taskPool.Enqueue(new KeyValuePair<int, byte[]>(blockNumber, blockValue));

                Monitor.Pulse(_taskPool);
                return true;
            }
        }

        /// <summary>
        /// Get data from the Queue
        /// </summary>
        public bool TryGet(out int blockNumber, out byte[] blockValue)
        {
            lock (_taskPool)
            {
                while (_taskPool.Count == 0)
                {
                    if (_isFinished)
                    {
                        blockNumber = -1;
                        blockValue = null;

                        return false;
                    }

                    Monitor.Wait(_taskPool);
                }

                if (_isFinished)
                {
                    blockNumber = -1;
                    blockValue = null;

                    return false;
                }

                KeyValuePair<int, byte[]> block = _taskPool.Dequeue();

                blockNumber = block.Key;
                blockValue = block.Value;

                Monitor.Pulse(_taskPool);
                return true;
            }
        }

        /// <summary>
        /// Terminate a task
        /// </summary>
        public void Finish()
        {
            lock (_taskPool)
            {
                _isFinished = true;

                Monitor.PulseAll(_taskPool);
            }
        }
    }
}

