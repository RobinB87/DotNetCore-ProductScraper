using System.Threading;

namespace ProductScraper.Helpers
{
    public class Utils : IUtils
    {
        public void Sleep(int miliSecondsToWait)
        {
            Thread.Sleep(miliSecondsToWait);
        }
    }
}
