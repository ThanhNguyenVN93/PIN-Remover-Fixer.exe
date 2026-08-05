using System;
using System.Management;
using frm_pin_remover.Core;
using frm_pin_remover.Models;

namespace frm_pin_remover.Services
{
    internal sealed class BitLockerResult
    {
        public bool QuerySucceeded { get; set; }
        public bool ProtectionOn { get; set; }
        public int SuspendCount { get; set; }
        public string DriveLetter { get; set; }
        public string ErrorMessage { get; set; }

        public DiagnosticFinding Finding { get; set; }
    }

    internal static class BitLockerDiagnostics
    {
        private const string Namespace = @"root\CIMV2\Security\MicrosoftVolumeEncryption";

        public static BitLockerResult Run()
        {
            var result = new BitLockerResult
            {
                DriveLetter = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:"
            };

            try
            {
                var scope = new ManagementScope(Namespace);
                scope.Connect();

                string query = $"SELECT * FROM Win32_EncryptableVolume WHERE DriveLetter = '{result.DriveLetter}'";
                using (var searcher = new ManagementObjectSearcher(scope, new ObjectQuery(query)))
                using (var results = searcher.Get())
                {
                    ManagementObject volume = null;
                    foreach (ManagementObject item in results)
                    {
                        volume = item;
                        break;
                    }

                    if (volume == null)
                    {
                        result.QuerySucceeded = false;
                        result.ErrorMessage = Localization.T("svc.bl.volumeNotFound");
                    }
                    else
                    {
                        using (volume)
                        {
                            uint protectionStatus = Convert.ToUInt32(volume["ProtectionStatus"]);
                            result.ProtectionOn = protectionStatus == 1;
                            result.QuerySucceeded = true;

                            if (result.ProtectionOn)
                            {
                                var inParams = volume.GetMethodParameters("GetSuspendCount");
                                var outParams = volume.InvokeMethod("GetSuspendCount", inParams, null);
                                result.SuspendCount = Convert.ToInt32(outParams["SuspendCount"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.QuerySucceeded = false;
                result.ErrorMessage = ex.Message;
            }

            result.Finding = BuildFinding(result);
            return result;
        }

        private static DiagnosticFinding BuildFinding(BitLockerResult result)
        {
            if (!result.QuerySucceeded)
            {
                return new DiagnosticFinding
                {
                    Source = Localization.T("svc.bl.source"),
                    Title = Localization.T("svc.bl.titleUnknown"),
                    Detail = result.ErrorMessage ?? Localization.T("svc.bl.unknownReason"),
                    Severity = DiagnosticSeverity.Info
                };
            }

            if (result.ProtectionOn)
            {
                return new DiagnosticFinding
                {
                    Source = Localization.T("svc.bl.source"),
                    Title = Localization.TF("svc.bl.titleOn", result.DriveLetter),
                    Detail = Localization.TF("svc.bl.detailOn", result.SuspendCount),
                    Severity = DiagnosticSeverity.Warning
                };
            }

            return new DiagnosticFinding
            {
                Source = Localization.T("svc.bl.source"),
                Title = Localization.TF("svc.bl.titleOff", result.DriveLetter),
                Detail = Localization.T("svc.bl.detailOff"),
                Severity = DiagnosticSeverity.Info
            };
        }
    }
}
