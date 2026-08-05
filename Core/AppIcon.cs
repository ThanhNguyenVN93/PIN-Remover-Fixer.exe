using System;
using System.Drawing;
using System.Windows.Forms;

namespace frm_pin_remover.Core
{
    internal static class AppIcon
    {
        private static readonly Lazy<Icon> LazyIcon = new Lazy<Icon>(Load);

        public static Icon Current => LazyIcon.Value;

        private static Icon Load()
        {
            try
            {
                return Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Shield;
            }
            catch
            {
                return SystemIcons.Shield;
            }
        }
    }
}
