using System;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Text;
using Microsoft.Win32;
using frm_pin_remover.Core;
using frm_pin_remover.Models;

namespace frm_pin_remover.Services
{
    internal sealed class AccountResult
    {
        public bool IsMicrosoftAccount { get; set; }
        public bool IsDomainJoined { get; set; }
        public bool IsAzureAdJoined { get; set; }
        public string CurrentUserName { get; set; }

        public DiagnosticFinding Finding { get; set; }
    }

    internal static class AccountDiagnostics
    {
        public static AccountResult Run()
        {
            var result = new AccountResult
            {
                CurrentUserName = Environment.UserDomainName + "\\" + Environment.UserName
            };

            var detail = new StringBuilder();

            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\IdentityCRL\UserExtendedProperties"))
                {
                    var subKeyNames = key?.GetSubKeyNames() ?? Array.Empty<string>();
                    result.IsMicrosoftAccount = subKeyNames.Length > 0;
                    detail.AppendLine(result.IsMicrosoftAccount
                        ? Localization.T("svc.acc.isMicrosoft")
                        : Localization.T("svc.acc.isLocal"));
                }
            }
            catch (Exception ex)
            {
                detail.AppendLine(Localization.TF("svc.acc.typeError", ex.Message));
            }

            try
            {
                Domain.GetComputerDomain();
                result.IsDomainJoined = true;
                detail.AppendLine(Localization.T("svc.acc.domainJoined"));
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                result.IsDomainJoined = false;
                detail.AppendLine(Localization.T("svc.acc.notDomainJoined"));
            }
            catch (Exception ex)
            {
                detail.AppendLine(Localization.TF("svc.acc.domainCheckError", ex.Message));
            }

            try
            {
                string output = RunProcessCapture("dsregcmd.exe", "/status", 10000);
                result.IsAzureAdJoined = ContainsYes(output, "AzureAdJoined");
                bool enterpriseJoined = ContainsYes(output, "EnterpriseJoined");
                detail.AppendLine(result.IsAzureAdJoined || enterpriseJoined
                    ? Localization.T("svc.acc.azureJoined")
                    : Localization.T("svc.acc.notAzureJoined"));
            }
            catch (Exception ex)
            {
                detail.AppendLine(Localization.TF("svc.acc.dsregcmdError", ex.Message));
            }

            DiagnosticSeverity severity = DiagnosticSeverity.Info;
            string title = Localization.T("svc.acc.titleLocalUnmanaged");

            if (result.IsDomainJoined || result.IsAzureAdJoined)
            {
                severity = DiagnosticSeverity.Warning;
                title = Localization.T("svc.acc.titleManaged");
            }
            else if (result.IsMicrosoftAccount)
            {
                title = Localization.T("svc.acc.titleMicrosoftUnmanaged");
            }

            result.Finding = new DiagnosticFinding
            {
                Source = Localization.T("svc.acc.source"),
                Title = title,
                Detail = detail.ToString().TrimEnd(),
                Severity = severity
            };

            return result;
        }

        private static bool ContainsYes(string output, string fieldName)
        {
            foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.IndexOf(fieldName, StringComparison.OrdinalIgnoreCase) >= 0
                    && line.IndexOf("YES", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
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
                    return stdout;
                }

                return stdout;
            }
        }
    }
}
