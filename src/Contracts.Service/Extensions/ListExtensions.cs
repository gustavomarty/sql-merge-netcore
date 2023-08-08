namespace Contracts.Service.Extensions
{
    public static class ListExtensions
    {
        private static readonly Random _rng = new();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while(n > 1)
            {
                n--;
                int k = _rng.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
    }
}
