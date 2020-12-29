using System;
using System.Collections.Generic;
using UnityEngine;

// See: http://wwwimages.adobe.com/www.adobe.com/content/dam/Adobe/en/devnet/pdf/pdfs/PDF32000_2008.pdf
// Page 333
namespace Aseprite.Utils
{
    public static class Texture2DBlender
    {        
        public static float Multiply(float b, float s)
        {
            return b * s;
        }

        public static float Screen(float b, float s)
        {
            return b + s - (b * s);
        }

        public static float Overlay(float b, float s)
        {
            return HardLight(s, b);
        }

        public static float Darken(float b, float s)
        {
            return Mathf.Min(b, s);
        }

        public static float Lighten(float b, float s)
        {
            return Mathf.Max(b, s);
        }

        // Color Dodge & Color Burn:  http://wwwimages.adobe.com/www.adobe.com/content/dam/Adobe/en/devnet/pdf/pdfs/adobe_supplement_iso32000_1.pdf
        public static float ColorDodge(float b, float s)
        {
            if (b == 0)
                return 0;
            else if (b >= (1 - s))
                return 1;
            else
                return b / (1 - s);
        }

        public static float ColorBurn(float b, float s)
        {
            if (b == 1)
                return 1;
            else if ((1 - b) >= s)
                return 0;
            else
                return 1 - ((1 - b) / s);
        }

        public static float HardLight(float b, float s)
        {
            if (s <= 0.5)
                return Multiply(b, 2 * s);
            else
                return Screen(b, 2 * s - 1);
        }

        public static float SoftLight(float b, float s)
        {
            if (s <= 0.5)
                return b - (1 - 2 * s) * b * (1 - b);
            else
                return b + (2 * s - 1) * (SoftLightD(b) - b);
        }

        private static float SoftLightD(float x)
        {
            if (x <= 0.25)
                return ((16 * x - 12) * x + 4) * x;
            else
                return Mathf.Sqrt(x);
        }

        public static float Difference(float b, float s)
        {
            return Mathf.Abs(b - s);
        }

        public static float Exclusion(float b, float s)
        {
            return b + s - 2 * b * s;
        }


        public static Texture2D Normal(Texture2D baseLayer, Texture2D layer, float opacity)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    
                    Color c = new Color();
                    b.a = b.a * opacity;

                    c = ((1f - b.a) * a) + (b.a * b);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }

        public static Texture2D Multiply(Texture2D baseLayer, Texture2D layer, float opacity)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    Color c = new Color();

                    c.r = (a.r) * (opacity * (1f - b.a * (1f - b.r)));
                    c.g = (a.g) * (opacity * (1f - b.a * (1f - b.g)));
                    c.b = (a.b) * (opacity * (1f - b.a * (1f - b.b)));
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }


        public static Texture2D Screen(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    Color c = a + b - (a * b);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }

        public static Texture2D Overlay(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);
                    
                    Color c = new Color();


                    if (a.r < 0.5)
                        c.r = 2f * a.r * b.r;
                    else
                        c.r = 1f - 2f * (1f - b.r) * (1f - a.r);

                    if (a.g < 0.5)
                        c.g = 2f * a.g * b.g;
                    else
                        c.g = 1f - 2f * (1f - b.g) * (1f - a.g);

                    if (a.b < 0.5)
                        c.b = 2f * a.b * b.b;
                    else
                        c.b = 1f - 2f * (1f - b.b) * (1f - a.b);

                    c = ((1f - b.a) * a) + (b.a * c);

                    c.a = a.a + b.a * (1f - a.a);

                    

                    newLayer.SetPixel(x, y, c);
                    
                }
            }

            newLayer.Apply();
    
            return newLayer;
        }

        public static Texture2D Darken(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    Color c = new Color();


                    c.r = Mathf.Min(a.r, b.r);
                    c.g = Mathf.Min(a.g, b.g);
                    c.b = Mathf.Min(a.b, b.b);

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }

        public static Texture2D Lighten(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    Color c = new Color();

                    c.r = Lighten(a.r, b.r);
                    c.g = Lighten(a.g, b.g);
                    c.b = Lighten(a.b, b.b);

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }

        public static Texture2D ColorDodge(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    Color c = new Color();


                    c.r = ColorDodge(a.r, b.r);
                    c.g = ColorDodge(a.g, b.g);
                    c.b = ColorDodge(a.b, b.b);

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }

        public static Texture2D ColorBurn(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    Color c = new Color();

                    c.r = ColorBurn(a.r, b.r);
                    c.g = ColorBurn(a.g, b.g);
                    c.b = ColorBurn(a.b, b.b);
                    
                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }

        public static Texture2D HardLight(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    Color c = new Color();

                    c.r = HardLight(a.r, b.r);
                    c.g = HardLight(a.g, b.g);
                    c.b = HardLight(a.b, b.b);

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }

        public static Texture2D SoftLight(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    Color c = new Color();

                    c.r = SoftLight(a.r, b.r);
                    c.g = SoftLight(a.g, b.g);
                    c.b = SoftLight(a.b, b.b);

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }

        public static Texture2D Difference(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    Color c = new Color();

                    c.r = Difference(a.r, b.r);
                    c.g = Difference(a.g, b.g);
                    c.b = Difference(a.b, b.b);

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }

        public static Texture2D Exclusion(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    Color c = new Color();

                    c.r = Exclusion(a.r, b.r);
                    c.g = Exclusion(a.g, b.g);
                    c.b = Exclusion(a.b, b.b);

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }



        public static Texture2D Hue(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    var s = Sat(a);
                    var l = Lum(a);

                    Color c = SetLum(SetSat(b, s), l);

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }

        public static Texture2D Saturation(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    var s = Sat(b);
                    var l = Lum(a);

                    Color c = SetLum(SetSat(a, s), l);

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }

        public static Texture2D Color(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    Color c = SetLum(b, Lum(a));

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }

        public static Texture2D Luminosity(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);



                    Color c = SetLum(a, Lum(b));

                    c = ((1f - b.a) * a) + (b.a * c); ;
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }


        public static Texture2D Addition(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    Color c = a + b;

                    c = ((1f - b.a) * a) + (b.a * c); ;
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }


        public static Texture2D Subtract(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    Color c = a - b;

                    c = ((1f - b.a) * a) + (b.a * c); ;
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }


        public static Texture2D Divide(Texture2D baseLayer, Texture2D layer)
        {
            Texture2D newLayer = new Texture2D(baseLayer.width, baseLayer.height);

            for (int x = 0; x < baseLayer.width; x++)
            {
                for (int y = 0; y < baseLayer.height; y++)
                {
                    Color a = baseLayer.GetPixel(x, y);
                    Color b = layer.GetPixel(x, y);

                    Color c = new Color(
                        BlendDivide(a.r, b.r),
                        BlendDivide(a.g, b.g),
                        BlendDivide(a.b, b.b)
                        );

                    c = ((1f - b.a) * a) + (b.a * c); ;
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            newLayer.Apply();

            return newLayer;
        }

        private static float BlendDivide(float b, float s)
        {
            if (b == 0)
                return 0;
            else if (b >= s)
                return 255;
            else
                return b / s;
        }


        private static double Lum(Color c)
        {
            return (0.3 * c.r) + (0.59 * c.g) + (0.11 * c.b);
        }

        private static Color ClipColor(Color c)
        {
            double l = Lum(c);
            float n = Math.Min(c.r, Math.Min(c.g, c.b));
            float x = Math.Max(c.r, Math.Max(c.g, c.b));


            if (n < 0)
            {
                c.r = (float)(l + (((c.r - l) * l) / (l - n)));
                c.g = (float)(l + (((c.g - l) * l) / (l - n)));
                c.b = (float)(l + (((c.b - l) * l) / (l - n)));
            }
            if (x > 1)
            {
                c.r = (float)(l + (((c.r - l) * (1 - l)) / (x - l)));
                c.g = (float)(l + (((c.g - l) * (1 - l)) / (x - l)));
                c.b = (float)(l + (((c.b - l) * (1 - l)) / (x - l)));
            }

            return c;
        }




        private static Color SetLum(Color c, double l)
        {
            double d = l - Lum(c);
            c.r = (float)(c.r + d);
            c.g = (float)(c.g + d);
            c.b = (float)(c.b + d);

            return ClipColor(c);
        }

        private static double Sat(Color c)
        {
            return Math.Max(c.r, Math.Max(c.g, c.b)) - Math.Min(c.r, Math.Min(c.g, c.b));
        }

        private static double DMax(double x, double y) { return (x > y) ? x : y; }
        private static double DMin(double x, double y) { return (x < y) ? x : y; }




        private static Color SetSat(Color c, double s)
        {
            char cMin = GetMinComponent(c);
            char cMid = GetMidComponent(c);
            char cMax = GetMaxComponent(c);

            double min = GetComponent(c, cMin);
            double mid = GetComponent(c, cMid);
            double max = GetComponent(c, cMax);


            if (max > min)
            {
                mid = ((mid - min) * s) / (max - min);
                c = SetComponent(c, cMid, (float)mid);
                max = s;
                c = SetComponent(c, cMax, (float)max);
            }
            else
            {
                mid = max = 0;
                c = SetComponent(c, cMax, (float)max);
                c = SetComponent(c, cMid, (float)mid);
            }

            min = 0;
            c = SetComponent(c, cMin, (float)min);

            return c;
        }




        private static float GetComponent(Color c, char component)
        {
            switch (component)
            {
                case 'r': return c.r;
                case 'g': return c.g;
                case 'b': return c.b;
            }

            return 0f;
        }


        private static Color SetComponent(Color c, char component, float value)
        {
            switch (component)
            {
                case 'r': c.r = value; break;
                case 'g': c.g = value; break;
                case 'b': c.b = value; break;
            }

            return c;
        }

        private static char GetMinComponent(Color c)
        {
            var r = new KeyValuePair<char, float>('r', c.r);
            var g = new KeyValuePair<char, float>('g', c.g);
            var b = new KeyValuePair<char, float>('b', c.b);

            return MIN(r, MIN(g, b)).Key;
        }

        private static char GetMidComponent(Color c)
        {
            var r = new KeyValuePair<char, float>('r', c.r);
            var g = new KeyValuePair<char, float>('g', c.g);
            var b = new KeyValuePair<char, float>('b', c.b);

            return MID(r, g, b).Key;
        }

        private static char GetMaxComponent(Color c)
        {
            var r = new KeyValuePair<char, float>('r', c.r);
            var g = new KeyValuePair<char, float>('g', c.g);
            var b = new KeyValuePair<char, float>('b', c.b);

            return MAX(r, MAX(g, b)).Key;
        }

        private static KeyValuePair<char, float> MIN(KeyValuePair<char, float> x, KeyValuePair<char, float> y)
        {
            return (x.Value < y.Value) ? x : y;
        }

        private static KeyValuePair<char, float> MAX(KeyValuePair<char, float> x, KeyValuePair<char, float> y)
        {
            return (x.Value > y.Value) ? x : y;
        }

        private static KeyValuePair<char, float> MID(KeyValuePair<char, float> x, KeyValuePair<char, float> y, KeyValuePair<char, float> z)
        {
            List<KeyValuePair<char, float>> components = new List<KeyValuePair<char, float>>();
            components.Add(x);
            components.Add(z);
            components.Add(y);
            

            components.Sort((c1, c2) => { return c1.Value.CompareTo(c2.Value); });

            return components[1];
            //return MAX(x, MIN(y, z));
        }
    }
}

