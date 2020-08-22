using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HousesCalradia
{
	public class Log : LogBase
	{
		private const string BeginMultiLine      = @"=======================================================================================================================\";
		private const string EndMultiLine        = @"=======================================================================================================================/";

		public readonly string Module;
		public readonly string LogDir;
		public readonly string LogFile;
		public readonly string LogPath;

		protected TextWriter Writer { get; set; }
		protected bool LastWasMultiline { get; set; } = false;

		public override void Print(string line)
		{
			if (Writer == null)
				return;

			LastWasMultiline = false;
			Writer.WriteLine(line);
			Writer.Flush();
		}

		public override void Print(List<string> lines)
		{
			if (Writer == null || lines.Count == 0)
				return;

			if (lines.Count == 1)
			{
				Print(lines[0]);
				return;
			}

			if (!LastWasMultiline)
				Writer.WriteLine(BeginMultiLine);

			LastWasMultiline = true;

			foreach (string line in lines)
				Writer.WriteLine(line);

			Writer.WriteLine(EndMultiLine);
			Writer.Flush();
		}

		public Log(bool truncate = false, string logName = null)
		{
			var userDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Mount and Blade II Bannerlord");

			Module = $"{this.GetType().Namespace}.{this.GetType().Name}";

			LogDir = Path.Combine(userDir, "Logs");

			if (logName == null)
				LogFile = $"{this.GetType().Namespace}.log";
			else
				LogFile = $"{this.GetType().Namespace}.{logName}.log";

			LogPath = Path.Combine(LogDir, LogFile);
			Directory.CreateDirectory(LogDir);
			var existed = File.Exists(LogPath);

			try
			{
				// Give it a 64KiB buffer so that it will essentially never block on interim WriteLine calls:
				Writer = TextWriter.Synchronized( new StreamWriter(LogPath, !truncate, Encoding.UTF8, 1 << 16) );
			}
			catch (Exception e)
			{
				Console.WriteLine($"================================  EXCEPTION  ================================");
				Console.WriteLine($"{Module}: Failed to create StreamWriter!");
				Console.WriteLine($"Path: {LogPath}");
				Console.WriteLine($"Truncate: {truncate}");
				Console.WriteLine($"Preexisting Path: {existed}");
				Console.WriteLine($"Exception Information:");
				Console.WriteLine($"{e}");
				Console.WriteLine($"=============================================================================");
				throw;
			}

			Writer.NewLine = "\n";

			var msg = new List<string>
			{
				$"{Module} created at: {DateTimeOffset.Now:yyyy/MM/dd H:mm zzz}",
			};

			if (existed && !truncate)
			{
				Writer.WriteLine();
				Writer.WriteLine();
				msg.Add("NOTE: Any prior log messages in this file may have no relation to this session.");
			}

			Print(msg);
		}

		~Log()
		{
			try
			{
				Writer.Dispose();
			}
			catch (Exception)
			{
				// at least we tried.
			}
		}
	}
}
