using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using frm_pin_remover.Core;
using frm_pin_remover.Models;

namespace frm_pin_remover.Services
{
    internal sealed class RegistryPolicyResult
    {
        public bool AllowDomainPINLogonExists { get; set; }
        public object AllowDomainPINLogonValue { get; set; }
        public bool PolicyManagerAuthenticationKeyExists { get; set; }
        public string[] PolicyManagerValueNames { get; set; } = Array.Empty<string>();
        public bool GpeditAvailable { get; set; }
        public bool GpResultMentionsPassport { get; set; }
        public string GpResultRawOutput { get; set; } = string.Empty;

        public bool HasGroupPolicyRestriction => AllowDomainPINLogonExists || GpResultMentionsPassport;
        public bool HasPolicyManagerOverride => PolicyManagerAuthenticationKeyExists && PolicyManagerValueNames.Length > 0;

        public DiagnosticFinding Finding { get; set; }
    }

    internal static class RegistryPolicyDiagnostics
    {
        public static RegistryPolicyResult Run()
        {
            var result = new RegistryPolicyResult();
            var detail = new StringBuilder();

            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(AppConstants.PolicySystemKeyPath))
                {
                    var value = key?.GetValue(AppConstants.PolicySystemValueName);
                    result.AllowDomainPINLogonExists = value != null;
                    result.AllowDomainPINLogonValue = value;
                    detail.AppendLine(value != null
                        ? $"HKLM\\{AppConstants.PolicySystemKeyPath}\\{AppConstants.PolicySystemValueName} = {value}"
                        : Localization.TF("svc.valueNotExist", $"HKLM\\{AppConstants.PolicySystemKeyPath}\\{AppConstants.PolicySystemValueName}"));
                }
            }
            catch (Exception ex)
            {
                detail.AppendLine(Localization.TF("svc.reg.readSystemPolicyError", ex.Message));
            }

            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(AppConstants.PolicyManagerAuthenticationKeyPath))
                {
                    result.PolicyManagerAuthenticationKeyExists = key != null;
                    result.PolicyManagerValueNames = key?.GetValueNames() ?? Array.Empty<string>();
                    detail.AppendLine(key != null
                        ? Localization.TF("svc.reg.policyManagerExists", $"HKLM\\{AppConstants.PolicyManagerAuthenticationKeyPath}", result.PolicyManagerValueNames.Length, string.Join(", ", result.PolicyManagerValueNames))
                        : Localization.TF("svc.valueNotExist", $"HKLM\\{AppConstants.PolicyManagerAuthenticationKeyPath}"));
                }
            }
            catch (Exception ex)
            {
                detail.AppendLine(Localization.TF("svc.reg.readPolicyManagerError", ex.Message));
            }

            try
            {
                string gpeditPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "gpedit.msc");
                result.GpeditAvailable = File.Exists(gpeditPath);

                if (result.GpeditAvailable)
                {
                    string output = RunProcessCapture("gpresult.exe", "/r /scope:computer", 15000);
                    result.GpResultRawOutput = output;
                    result.GpResultMentionsPassport = output.IndexOf("Passport", StringComparison.OrdinalIgnoreCase) >= 0
                        || output.IndexOf("Windows Hello", StringComparison.OrdinalIgnoreCase) >= 0;
                    detail.AppendLine(result.GpResultMentionsPassport
                        ? Localization.T("svc.reg.gpresultFound")
                        : Localization.T("svc.reg.gpresultNotFound"));
                }
                else
                {
                    detail.AppendLine(Localization.T("svc.reg.noGpedit"));
                }
            }
            catch (Exception ex)
            {
                detail.AppendLine(Localization.TF("svc.reg.gpresultRunError", ex.Message));
            }

            DiagnosticSeverity severity;
            string title;

            if (result.HasGroupPolicyRestriction)
            {
                severity = DiagnosticSeverity.Warning;
                title = Localization.T("svc.reg.titleGpDetected");
            }
            else if (result.HasPolicyManagerOverride)
            {
                severity = DiagnosticSeverity.Warning;
                title = Localization.T("svc.reg.titlePolicyManagerDetected");
            }
            else
            {
                severity = DiagnosticSeverity.Info;
                title = Localization.T("svc.reg.titleNoneFound");
            }

            result.Finding = new DiagnosticFinding
            {
                Source = Localization.T("svc.reg.source"),
                Title = title,
                Detail = detail.ToString().TrimEnd(),
                Severity = severity
            };

            return result;
        }

        private static string RunProcessCapture(string fileName, string arguments, int timeoutMs)
        {
            var psi = new ProcessStartInfo(fileName, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                string stdout = process.StandardOutput.ReadToEnd();
                if (!process.WaitForExit(timeoutMs))
                {
                    try { process.Kill(); } catch { /* best effort */ }
                    return stdout + Environment.NewLine + Localization.T("svc.reg.gpresultTimeout");
                }

                return stdout;
            }
        }
    }
}
