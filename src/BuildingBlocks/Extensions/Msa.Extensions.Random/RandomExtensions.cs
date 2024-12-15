namespace Msa.Extensions.Random
{
    public static class RandomExtensions
    {
        public static Task Delay(this System.Random random, int minMills = 0, int maxMills = 0)
        {
            if (minMills == 0 && maxMills == 0) return Task.CompletedTask;
            var randMills = random.Next(minMills, maxMills);
            return Task.Delay(randMills);
        }
    }
}
