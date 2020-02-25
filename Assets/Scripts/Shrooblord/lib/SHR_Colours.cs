using UnityEngine;

namespace Shrooblord.lib {
    public static class SHR_Colours {
        public static Color purple = new Color(0.9f, 0.05f, 0.9f);
        public static Color lime = new Color(0.016f, 1f, 0.174f);

        public static Color Invert(Color col_in) {
            float hue, sat, val;
            Color.RGBToHSV(col_in, out hue, out sat, out val);

            hue = (hue + 0.5f) % 1f; //find colour on opposite of colour wheel

            return Color.HSVToRGB(hue, sat, val);
        }
    }
}
