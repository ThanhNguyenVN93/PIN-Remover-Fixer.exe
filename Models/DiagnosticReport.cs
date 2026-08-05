using System.Collections.Generic;

namespace frm_pin_remover.Models
{
    internal sealed class DiagnosticReport
    {
        public DiagnosticFinding RegistryPolicyFinding { get; set; }
        public DiagnosticFinding BitLockerFinding { get; set; }
        public DiagnosticFinding AccountFinding { get; set; }

        public bool BitLockerProtectionOn { get; set; }
        public int BitLockerSuspendCount { get; set; }

        public string RootCauseSummary { get; set; }
        public List<RecommendedAction> RecommendedActions { get; set; } = new List<RecommendedAction>();

        public IEnumerable<DiagnosticFinding> AllFindings
        {
            get
            {
                yield return RegistryPolicyFinding;
                yield return BitLockerFinding;
                yield return AccountFinding;
            }
        }
    }
}
