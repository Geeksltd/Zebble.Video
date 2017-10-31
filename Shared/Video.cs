using System.Threading.Tasks;

namespace Zebble
{
    public partial class Video
    {
        public static Task<bool> Compress(string inputPath, string outputPath, OnError errorAction = OnError.Ignore)
        {
            return DoCompress(inputPath, outputPath, errorAction);
        }
    }
}