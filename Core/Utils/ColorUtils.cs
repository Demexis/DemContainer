using System.Diagnostics.Contracts;
using System.Text;
using UnityEngine;

namespace DemContainer {
    internal static class ColorUtils {
        private static readonly StringBuilder sB = new();

        [Pure]
        public static string Colorize(object obj, Color color) {
            lock (sB) {
                sB.Clear();
                sB.Append("<color=#");
                sB.Append(ColorUtility.ToHtmlStringRGBA(color));
                sB.Append(">");
                sB.Append(obj);
                sB.Append("</color>");
                return sB.ToString();
            }
        }
    }
}