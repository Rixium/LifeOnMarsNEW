using System;

namespace LoM.Util
{
    public class Randomizer
    {

        private static Random _random = new Random();

        public static float NextFloat(float max, float min)
        {
            var range = max - (double) min;
            var sample = _random.NextDouble();
            var scaled = sample * range + min;
            var f = (float) scaled;
            return f;
        }
    }
}