namespace frm_pin_remover.Models
{
    internal enum ActionKind
    {
        FixRegistryPolicy,
        DeleteNgcFolder
    }

    internal sealed class RecommendedAction
    {
        public ActionKind Kind { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool RequiresBitLockerOff { get; set; }
    }
}
