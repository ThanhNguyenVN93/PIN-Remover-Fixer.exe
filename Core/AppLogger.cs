using System;
using System.Collections.Generic;
using System.IO;
using frm_pin_remover.Models;

namespace frm_pin_remover.Core
{
    internal static class AppLogger
    {
        private static readonly object SyncRoot = new object();
        private static readonly List<ActionLogEntry> Entries = new List<ActionLogEntry>();

        public static event Action<ActionLogEntry> EntryAdded;

        public static IReadOnlyList<ActionLogEntry> AllEntries
        {
            get
            {
                lock (SyncRoot)
                {
                    return Entries.ToArray();
                }
            }
        }

        public static void Log(string action, string detail, string result)
        {
            var entry = new ActionLogEntry
            {
                TimestampUtc = DateTime.UtcNow,
                Action = action,
                Detail = detail,
                Result = result
            };

            lock (SyncRoot)
            {
                Entries.Add(entry);
                AppendToFile(entry);
            }

            EntryAdded?.Invoke(entry);
        }

        private static void AppendToFile(ActionLogEntry entry)
        {
            try
            {
                string dir = Path.GetDirectoryName(AppConstants.LogFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string line = $"{entry.TimestampUtc:yyyy-MM-dd HH:mm:ss}Z\t{entry.Action}\t{entry.Detail}\t{entry.Result}";
                File.AppendAllLines(AppConstants.LogFilePath, new[] { line });
            }
            catch
            {
                // Logging must never crash the app; file access issues are non-fatal.
            }
        }
    }
}
