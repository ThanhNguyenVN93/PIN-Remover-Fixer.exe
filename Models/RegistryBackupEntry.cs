using System;

namespace frm_pin_remover.Models
{
    [Serializable]
    internal sealed class RegistryBackupEntry
    {
        public string Hive { get; set; }
        public string KeyPath { get; set; }
        public string ValueName { get; set; }
        public bool ValueExisted { get; set; }
        public string ValueKind { get; set; }
        public string ValueData { get; set; }
        public DateTime TimestampUtc { get; set; }
    }
}
