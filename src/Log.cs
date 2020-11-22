using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HousesCalradia
{
    internal sealed class Log : LogBase
    {
        private const string BeginMultiLine = @"=======================================================================================================================\";
        private const string EndMultiLine   = @"=======================================================================================================================/";

        public readonly string Module;
        public readonly string LogDir;
        public readonly string LogFile;
        public readonly string LogPath;

        private TextWriter? Writer { get; set; }
        private bool LastWasMultiline { get; set; } = false;

        public override void Print(string line)
        {
            if (Writer is null)
                return;

            LastWasMultiline = false;
            Writer.WriteLine(line);
            Writer.Flush();
        }

        public override void Print(List<string> lines)
        {
            if (Writer is null || lines.Count == 0)
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

        public Log(bool truncate = false, string? logName = null)
        {
            var userDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Mount and Blade II Bannerlord");

            Module = GetType().FullName;
            LogDir = Path.Combine(userDir, "Logs");
            LogFile = logName is null ? $"{GetType().Namespace}.log" : $"{GetType().Namespace}.{logName}.log";
            LogPath = Path.Combine(LogDir, LogFile);

            Directory.CreateDirectory(LogDir);
            var existed = File.Exists(LogPath);

            try
            {
                Writer = TextWriter.Synchronized(new StreamWriter(LogPath, !truncate, Encoding.UTF8, 1 << 15));
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
    }
}
