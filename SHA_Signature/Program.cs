using System;
using System.Diagnostics;
using System.Threading;

namespace SHA_Signature
{
    class Program
    {

        private static readonly int _cores = Environment.ProcessorCount * 2;
        private static TaskPool _readerTaskPool;
        private static Thread _reader;
        private static TaskPool _writerTaskPool;
        private static Thread _writer;
        private static Thread[] _handlers = new Thread[_cores];

        public static int blockSize;
        public static string fileName;

        static void Main(string[] args)
        {
            ParseArgs parametres = new ParseArgs(args);

            blockSize = parametres.blockSize;
            fileName = parametres.fileName;

            _readerTaskPool = new TaskPool(_cores);
            _writerTaskPool = new TaskPool(_cores);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Worker worker = new Worker();
            worker.Finish += _readerTaskPool.Finish;
            worker.Finish += _writerTaskPool.Finish;

            //Create threads
            _reader = new Thread(delegate () { worker.Reader(fileName, blockSize, ref _readerTaskPool); });

            for (int i = 0; i < _cores; i++)
            {
                _handlers[i] = new Thread(delegate () { worker.Handler(ref _readerTaskPool, ref _writerTaskPool); });
            }

            _writer = new Thread(delegate () { worker.Writer(fileName, ref _writerTaskPool); });

            //Start threads
            _reader.Start();

            foreach (Thread handler in _handlers)
            {
                handler.Start();
            }

            _writer.Start();

            //Join threads
            _writer.Join();

            foreach (Thread handler in _handlers)
            {
                handler.Join();
            }
            _reader.Join();

            //Finish Programm
            worker.Finish -= _writerTaskPool.Finish;
            worker.Finish -= _readerTaskPool.Finish;
            sw.Stop();
            var time = sw.Elapsed;
            Console.Write("Programm successfully completed! Time spent: {0:D1}h:{1:D2}m:{2:D2}s:{3:D3}ms", time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
            Console.ReadKey();
        }
    }
}
