using System;
using System.Diagnostics;
using frm_pin_remover.Core;

namespace frm_pin_remover.Services
{
    internal sealed class BitLockerActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    internal static class BitLockerActionService
    {
        public static string BuildSuspendCommandText()
        {
            string drive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
            return $"manage-bde -protectors -disable {drive} -RebootCount 1";
        }

        public static BitLockerActionResult ResumeNow()
        {
            string drive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";

            try
            {
                var psi = new ProcessStartInfo("manage-bde.exe", $"-protectors -enable {drive}")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();
                    process.WaitForExit(30000);

                    bool success = process.ExitCode == 0;
                    AppLogger.Log(Localization.T("svc.log.actionResumeBitlocker"), $"manage-bde -protectors -enable {drive}", success ? Localization.T("common.success") : Localization.TF("svc.bla.failedDetail", stderr));
                    return new BitLockerActionResult
                    {
                        Success = success,
                        Message = success ? Localization.T("svc.bla.resumeSuccessMessage") : (string.IsNullOrWhiteSpace(stderr) ? stdout : stderr)
                    };
                }
            }
            catch (Exception ex)
            {
                AppLogger.Log(Localization.T("svc.log.actionResumeBitlocker"), ex.Message, Localization.T("common.failed"));
                return new BitLockerActionResult { Success = false, Message = ex.Message };
            }
        }
    }
}
