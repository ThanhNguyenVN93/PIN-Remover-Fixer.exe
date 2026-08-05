using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace frm_pin_remover.Core
{
    internal static class EmbeddedResources
    {
        public static Image LoadImage(string fileName)
        {
            var asm = Assembly.GetExecutingAssembly();
            string resourceName = Array.Find(asm.GetManifestResourceNames(),
                n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

            if (resourceName == null) return null;

            using (var resourceStream = asm.GetManifestResourceStream(resourceName))
            using (var buffer = new MemoryStream())
            {
                resourceStream.CopyTo(buffer);
                buffer.Position = 0;
                return new Bitmap(buffer);
            }
        }
    }
}
