using System;
using System.IO;
using System.Security.Cryptography;

namespace SHA_Signature
{
    /// <summary>
    /// Work with file(read, process and write to console)
    /// </summary>
    class Worker
    {
        public delegate void FinishEventHandler();
        public event FinishEventHandler Finish;

        private long _blockCount;


        public void Reader(string source, int blockSize, ref TaskPool readerTaskPool)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(source);

                _blockCount = fileInfo.Length / blockSize;

                if (fileInfo.Length % blockSize > 0)
                {
                    _blockCount++;
                }
            }
            catch (Exception e)
            {
                Finish();
                Logger.Log(e);
                return;
            }

            try
            {
                using (BinaryReader br = new BinaryReader(new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.None)))
                {
                    for (int blockNumber = 0; blockNumber < _blockCount; blockNumber++)
                    {
                        byte[] blockValue = br.ReadBytes(blockSize);

                        if (blockValue == null)
                        {
                            throw new ArgumentNullException("blockValue", "ERROR: некорректное значение блока");
                        }

                        if (!readerTaskPool.TrySet(blockNumber, blockValue))
                        {
                            return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Finish();
                Logger.Log(e);
                return;
            }
        }

        public void Handler(ref TaskPool readerTaskPool, ref TaskPool writerTaskPool)
        {
            int blockNumber = -1;
            byte[] blockValue = null;

            while (true)
            {
                try
                {
                    if (!readerTaskPool.TryGet(out blockNumber, out blockValue))
                    {
                        return;
                    }

                    if (blockValue == null)
                    {
                        break;
                    }


                    using var sha256 = SHA256.Create();
                    var hashBlock = sha256.ComputeHash(blockValue);

                    if (!writerTaskPool.TrySet(blockNumber, hashBlock))
                    {
                        return;
                    }


                }
                catch (Exception e)
                {
                    Finish();
                    Logger.Log(e);
                    return;
                }
            }
        }

        public void Writer(string destination, ref TaskPool writerTaskPool)
        {
            int counter = 0;

            int blockNumber = -1;
            byte[] blockValue = null;

            try
            {
                while (true)
                {
                    if (!writerTaskPool.TryGet(out blockNumber, out blockValue))
                    {
                        return;
                    }

                    if (blockValue == null)
                    {
                        break;
                    }

                    try
                    {
                        var output = BitConverter.ToString(blockValue).Replace("-", "");
                        Console.WriteLine(blockNumber + ")  " + output);
                    }
                    catch (Exception e)
                    {
                        Finish();
                        Logger.Log(e);
                        return;
                    }

                    counter++;

                    if (counter == _blockCount)
                    {
                        Finish();
                    }
                }
            }
            catch (Exception e)
            {
                Finish();
                Logger.Log(e);
            }

        }
    }
}
