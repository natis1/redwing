using System;
using System.Drawing;

namespace FlamegenTesting
{
    class RedwingFlame
    {
        //public Texture2D tex;

        public System.Drawing.Bitmap bmp;

        private readonly int TEXTURE_WIDTH = 400;
        private readonly int TEXTURE_HEIGHT = 400;
        private readonly int TEXTURE_WMIDPOINT = 200;
        private readonly int TEXTURE_HMIDPOINT = 200;

        public readonly Color[] flameIntensityCurve = { Color.FromArgb(255, 0, 0), Color.FromArgb(255, 76, 0), Color.FromArgb(255, 240, 0), Color.FromArgb(255, 255, 255) };


        static void Main(string[] args)
        {

            Console.WriteLine("ATTEMPTING TO BUILD IMAGE");
            RedwingFlame p = new RedwingFlame();
            p.GenerateFlameTextures();
            Console.WriteLine("BUILD IMAGE SUCCESS ATTEMPTING TO SAVE");

            
            try
            {
                // just some test output
                p.bmp.Save("/tmp/pseudofireball.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            }
            catch (Exception e)
            {
                Console.WriteLine(" CANNOT SAVE IMAGE BECAUSE ERROR " + e);
            }
        }


        public void GenerateFlameTextures()
        {
            System.Random rng = new System.Random();
            if (bmp == null)
            {
                bmp = new Bitmap(400, 400);
            }
            double[] radialIntensity400 = new double[360];
            double[] radialOpacity400 = new double[360];
            for (int i = 0; i < 360; i++)
            {
                radialIntensity400[i] = rng.NextDouble();
                radialOpacity400[i] = rng.NextDouble();

                // because c# sucks NextDouble can't return arbitrary numbers
                // so apply a transformation to map radialIntensity400 -> 1-1.5
                // and radialOpacity400 -> 0.6-1.0
                radialIntensity400[i] = (radialIntensity400[i] * 0.5) + 1.0;
                radialOpacity400[i] = (radialOpacity400[i] * 0.4) + 0.6;
            }

            for (int x = 0; x < 400; x++)
            {
                for (int y = 0; y < 400; y++)
                {
                    int angel = getNearestAngel(x, y);
                    
                    bmp.SetPixel(x, y, getFireColor(getDistance(x, y), radialIntensity400[angel], radialOpacity400[angel]));
                }
            }


        }

        public Color getFireColor(double distance, double intensity400, double opacity400)
        {
            double intensity = getRealIntensity(distance, intensity400);
            Color c = new Color();
            int a;
            int r;
            int g;
            int b;
            float realAlpha = (float)getRealOpacity(distance, opacity400);

            a = (byte)Math.Floor(realAlpha >= 1.0 ? 255 : realAlpha * 256.0) ;

            if (intensity < 1.0)
            {
                double intens1 = intensity;
                double intens2 = intensity - 1.0;
                c.R = (flameIntensityCurve[0].R * (float)intens2) + (flameIntensityCurve[1].r * (float)intens1);
                c.R = (flameIntensityCurve[0].R * (float)intens2) + (flameIntensityCurve[1].g * (float)intens1);
                c.b = (flameIntensityCurve[0].b * (float)intens2) + (flameIntensityCurve[1].b * (float)intens1);
            }
            else if (intensity < 2.0)
            {
                double intens1 = intensity - 1.0;
                double intens2 = intensity - 2.0;
                c.r = (flameIntensityCurve[1].r * (float)intens2) + (flameIntensityCurve[2].r * (float)intens1);
                c.g = (flameIntensityCurve[1].g * (float)intens2) + (flameIntensityCurve[2].g * (float)intens1);
                c.b = (flameIntensityCurve[1].b * (float)intens2) + (flameIntensityCurve[2].b * (float)intens1);
            }
            else
            {
                double intens1 = intensity - 2.0;
                double intens2 = intensity - 3.0;
                c.r = (flameIntensityCurve[2].r * (float)intens2) + (flameIntensityCurve[3].r * (float)intens1);
                c.g = (flameIntensityCurve[2].g * (float)intens2) + (flameIntensityCurve[3].g * (float)intens1);
                c.b = (flameIntensityCurve[2].b * (float)intens2) + (flameIntensityCurve[3].b * (float)intens1);
            }
            return c;
        }

        public double getRealOpacity(double distance, double opacity400)
        {
            double averageWeighting = distance / 400.0;
            double opactReal = opacity400 * averageWeighting + ((1 - opacity400) * 1.6);
            if (opactReal < 0.0)
            {
                opactReal = 0.0;
            }
            else if (opactReal >= 1.0)
            {
                opactReal = 1.0;
            }
            return opactReal;
        }

        public double getRealIntensity(double distance, double intensity400)
        {
            double averageWeighting = distance / 400.0;
            double intenReal = intensity400 * averageWeighting + ((1 - averageWeighting) * 4);
            if (intenReal < 0.0)
            {
                intenReal = 0.0;
            }
            else if (intenReal >= 3.0)
            {
                intenReal = 3.0;
            }
            return intenReal;
        }

        public double getDistance(int x, int y)
        {
            int relX = x - TEXTURE_WMIDPOINT;
            int relY = y - TEXTURE_HMIDPOINT;

            return Math.Sqrt((double)(relX * relX) + (double)(relY * relY));
        }



        public int getNearestAngel(int x, int y)
        {
            int angel = 0;

            int relX = x - TEXTURE_WMIDPOINT;
            int relY = y - TEXTURE_HMIDPOINT;

            if (relX == 0)
            {
                if (relY >= 0)
                {
                    return 90;
                }
                else
                {
                    return 270;
                }
            }
            else if (relX > 0)
            {
                angel = (int)Math.Atan2(relY, relX);
            }
            else
            {
                angel = (int)Math.Atan2(relY, relX) + 180;
            }
            while (angel < 0)
            {
                angel += 360;
            }
            angel %= 360;

            return angel;
        }
    }
}
