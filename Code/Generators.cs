namespace DailyCheck
{
    public class AscendingSQRT(int steps)
    {
        public IEnumerator<double> GetEnumerator()
        {
            for (double i = 1; i <= steps; i++)
                yield return Math.Sqrt(i / steps);
        }
    }

    public class DescendingSQRT(int steps)
    {
        public IEnumerator<double> GetEnumerator()
        {
            for (double i = 1; i <= steps; i++)
                yield return Math.Sqrt(1 - i / steps);
        }
    }
}
