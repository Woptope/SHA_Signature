using System;
using System.IO;

namespace SHA_Signature
{
    /// <summary>
    /// Check input parametres
    /// </summary>
    class ParseArgs
    {
        public readonly int blockSize;
        public readonly string fileName;
        public ParseArgs(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("ERROR: допустимое количество параметров - 2", nameof(args));
            }

            if (!File.Exists(args[0]))
            {
                throw new FileNotFoundException("ERROR: указан неверный путь к файлу", args[0]);
            }

            fileName = args[0];

            if (!Int32.TryParse(args[1], out blockSize))
            {
                throw new ArgumentOutOfRangeException(args[1], "ERROR: недопустимая длина блока");
            }
            else
            {
                if (blockSize <= 0)
                {
                    throw new ArgumentException("ERROR: недопустимая длина блока", nameof(blockSize));
                }
            }

        }
    }
}
