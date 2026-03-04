namespace EnkaDotNet.Utils.Common
{
    /// <summary>
    /// Math utility helpers for portability across target frameworks.
    /// </summary>
    public static class MathHelper
    {
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
