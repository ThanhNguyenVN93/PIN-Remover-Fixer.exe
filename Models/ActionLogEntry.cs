using System;

namespace frm_pin_remover.Models
{
    internal sealed class ActionLogEntry
    {
        public DateTime TimestampUtc { get; set; }
        public string Action { get; set; }
        public string Detail { get; set; }
        public string Result { get; set; }
    }
}
