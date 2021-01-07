using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;

namespace HousesCalradia
{
    internal sealed class Log : LogBase
    {
        private const string BeginMultiLine = @"=======================================================================================================================\";
        private const string EndMultiLine = @"=======================================================================================================================/";

        public readonly string LogDir;
        public readonly string LogFile;
        public readonly string LogPath;

        private TextWriter Writer { get; }
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

            LogDir = Path.Combine(userDir, "Configs", "ModLogs");
            LogFile = logName is null ? $"{GetType().Namespace}.log" : $"{GetType().Namespace}.{logName}.log";
            LogPath = Path.Combine(LogDir, LogFile);

            try
            {
                Directory.CreateDirectory(LogDir);
            }
            catch (Exception e)
            {
                DumpConstructionException("Failed to create log directory(s)!", e, LogDir, isFolder: true);
                throw;
            }

            var existed = File.Exists(LogPath);

            try
            {
                Writer = TextWriter.Synchronized(new StreamWriter(LogPath, !truncate, Encoding.UTF8, 1 << 15));
            }
            catch (Exception e)
            {
                DumpConstructionException("Failed to open log file for writing!", e, LogPath, truncate);
                throw;
            }

            Writer.NewLine = "\n";

            var msg = new List<string>
            {
                $"{GetType().FullName} created at: {DateTimeOffset.Now:yyyy/MM/dd H:mm zzz}",
            };

            if (existed && !truncate)
            {
                Writer.WriteLine("\n");
                msg.Add("NOTE: Any prior log messages in this file may have no relation to this session.");
            }

            Print(msg);
        }

        private void DumpConstructionException(string msg, Exception exception, string path, bool isFolder = false, bool? truncateMode = null)
        {
            Console.WriteLine($"================================  EXCEPTION  ================================");
            Console.WriteLine($"{GetType().FullName}: {msg}");
            Console.WriteLine($"Path: {path}");

            if (truncateMode is not null)
                Console.WriteLine($"Truncate? {truncateMode}");

            if (isFolder ? Directory.Exists(path) : File.Exists(path))
                Console.WriteLine("Path preexists? Yes");

            Console.WriteLine($"Exception Information:");
            Console.WriteLine($"{exception}");
            Console.WriteLine($"=============================================================================");
        }
    }
}
