using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Win32;
using frm_pin_remover.Core;
using frm_pin_remover.Models;

namespace frm_pin_remover.Services
{
    internal sealed class RegistryFixResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    internal static class RegistryFixService
    {
        public static RegistryFixResult Apply()
        {
            try
            {
                var entries = new List<RegistryBackupEntry>();

                using (var key = Registry.LocalMachine.OpenSubKey(AppConstants.PolicySystemKeyPath, writable: false))
                {
                    var existingValue = key?.GetValue(AppConstants.PolicySystemValueName);
                    entries.Add(new RegistryBackupEntry
                    {
                        Hive = "HKLM",
                        KeyPath = AppConstants.PolicySystemKeyPath,
                        ValueName = AppConstants.PolicySystemValueName,
                        ValueExisted = existingValue != null,
                        ValueKind = existingValue != null ? key.GetValueKind(AppConstants.PolicySystemValueName).ToString() : RegistryValueKind.DWord.ToString(),
                        ValueData = existingValue?.ToString(),
                        TimestampUtc = DateTime.UtcNow
                    });
                }

                SaveBackup(entries);

                using (var key = Registry.LocalMachine.CreateSubKey(AppConstants.PolicySystemKeyPath, writable: true))
                {
                    key.SetValue(AppConstants.PolicySystemValueName, 1, RegistryValueKind.DWord);
                }

                AppLogger.Log(Localization.T("svc.log.actionFixRegistry"), Localization.TF("svc.reg.fixDetail", AppConstants.PolicySystemValueName), Localization.T("common.success"));
                return new RegistryFixResult { Success = true, Message = Localization.T("svc.reg.applyResultMessage") };
            }
            catch (Exception ex)
            {
                AppLogger.Log(Localization.T("svc.log.actionFixRegistry"), ex.Message, Localization.T("common.failed"));
                return new RegistryFixResult { Success = false, Message = ex.Message };
            }
        }

        public static RegistryFixResult Rollback()
        {
            try
            {
                var entries = LoadBackup();
                if (entries == null || entries.Count == 0)
                {
                    return new RegistryFixResult { Success = false, Message = Localization.T("svc.reg.noBackupMessage") };
                }

                foreach (var entry in entries)
                {
                    var hive = entry.Hive == "HKLM" ? Registry.LocalMachine : Registry.CurrentUser;
                    using (var key = hive.CreateSubKey(entry.KeyPath, writable: true))
                    {
                        if (entry.ValueExisted)
                        {
                            var kind = (RegistryValueKind)Enum.Parse(typeof(RegistryValueKind), entry.ValueKind);
                            object data = kind == RegistryValueKind.DWord ? (object)Convert.ToInt32(entry.ValueData) : entry.ValueData;
                            key.SetValue(entry.ValueName, data, kind);
                        }
                        else
                        {
                            key.DeleteValue(entry.ValueName, throwOnMissingValue: false);
                        }
                    }
                }

                AppLogger.Log(Localization.T("svc.log.actionRollbackRegistry"), Localization.TF("svc.reg.rollbackDetail", entries.Count), Localization.T("common.success"));
                return new RegistryFixResult { Success = true, Message = Localization.T("svc.reg.rollbackResultMessage") };
            }
            catch (Exception ex)
            {
                AppLogger.Log(Localization.T("svc.log.actionRollbackRegistry"), ex.Message, Localization.T("common.failed"));
                return new RegistryFixResult { Success = false, Message = ex.Message };
            }
        }

        public static bool HasBackup() => File.Exists(AppConstants.RegistryBackupFilePath);

        private static void SaveBackup(List<RegistryBackupEntry> entries)
        {
            string dir = Path.GetDirectoryName(AppConstants.RegistryBackupFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var serializer = new DataContractJsonSerializer(typeof(List<RegistryBackupEntry>));
            using (var stream = new FileStream(AppConstants.RegistryBackupFilePath, FileMode.Create, FileAccess.Write))
            {
                serializer.WriteObject(stream, entries);
            }
        }

        private static List<RegistryBackupEntry> LoadBackup()
        {
            if (!File.Exists(AppConstants.RegistryBackupFilePath))
            {
                return null;
            }

            var serializer = new DataContractJsonSerializer(typeof(List<RegistryBackupEntry>));
            using (var stream = new FileStream(AppConstants.RegistryBackupFilePath, FileMode.Open, FileAccess.Read))
            {
                return (List<RegistryBackupEntry>)serializer.ReadObject(stream);
            }
        }
    }
}
