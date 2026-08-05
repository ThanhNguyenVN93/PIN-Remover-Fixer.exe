using System.Threading.Tasks;
using frm_pin_remover.Core;
using frm_pin_remover.Models;

namespace frm_pin_remover.Services
{
    internal static class DiagnosticsEngine
    {
        public static async Task<DiagnosticReport> RunAsync()
        {
            var registryTask = Task.Run(() => RegistryPolicyDiagnostics.Run());
            var bitLockerTask = Task.Run(() => BitLockerDiagnostics.Run());
            var accountTask = Task.Run(() => AccountDiagnostics.Run());

            await Task.WhenAll(registryTask, bitLockerTask, accountTask).ConfigureAwait(false);

            var registryResult = registryTask.Result;
            var bitLockerResult = bitLockerTask.Result;
            var accountResult = accountTask.Result;

            var report = new DiagnosticReport
            {
                RegistryPolicyFinding = registryResult.Finding,
                BitLockerFinding = bitLockerResult.Finding,
                AccountFinding = accountResult.Finding,
                BitLockerProtectionOn = bitLockerResult.ProtectionOn && bitLockerResult.QuerySucceeded,
                BitLockerSuspendCount = bitLockerResult.SuspendCount
            };

            if (registryResult.HasGroupPolicyRestriction)
            {
                report.RootCauseSummary = Localization.T("svc.engine.rootCauseGp");
                report.RecommendedActions.Add(new RecommendedAction
                {
                    Kind = ActionKind.FixRegistryPolicy,
                    Title = Localization.T("svc.engine.priority1TitleGp"),
                    Description = Localization.T("svc.engine.priority1DescGp")
                });
            }
            else if (registryResult.HasPolicyManagerOverride)
            {
                report.RootCauseSummary = Localization.T("svc.engine.rootCausePolicyManager");
                report.RecommendedActions.Add(new RecommendedAction
                {
                    Kind = ActionKind.FixRegistryPolicy,
                    Title = Localization.T("svc.engine.priority1TitlePm"),
                    Description = Localization.T("svc.engine.priority1DescPm")
                });
            }
            else
            {
                report.RootCauseSummary = Localization.T("svc.engine.rootCauseUnknown");
            }

            report.RecommendedActions.Add(new RecommendedAction
            {
                Kind = ActionKind.DeleteNgcFolder,
                Title = Localization.T("svc.engine.priority2Title"),
                Description = Localization.T("svc.engine.priority2Desc"),
                RequiresBitLockerOff = true
            });

            return report;
        }
    }
}
