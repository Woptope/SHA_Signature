using System;

namespace SHA_Signature
{
	/// <summary>
	/// Write logs to console
	/// </summary>
	class Logger
    {
		public static void Log(Exception e, string comment)
		{
			Console.WriteLine(comment);
			Console.WriteLine($"Message: {e.Message}");
			Console.WriteLine("Stack trace:");
			Console.WriteLine(e.StackTrace);
		}

		public static void Log(Exception e)
		{
			Console.WriteLine($"Message: {e.Message}");
			Console.WriteLine("Stack trace:");
			Console.WriteLine(e.StackTrace);
		}

	}
}
