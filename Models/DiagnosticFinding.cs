namespace frm_pin_remover.Models
{
    internal sealed class DiagnosticFinding
    {
        public string Source { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public DiagnosticSeverity Severity { get; set; }
    }
}
