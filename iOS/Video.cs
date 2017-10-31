using Zebble;
using System;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;

namespace Zebble
{
    public partial class Video
    {
        public static async Task<bool> DoCompress(string inputPath, string outputPath, OnError errorAction)
        {
            try
            {
                var asset = AVAsset.FromUrl(NSUrl.FromFilename(inputPath));

                var export = new AVAssetExportSession(asset, AVAssetExportSession.PresetLowQuality)
                {
                    OutputUrl = NSUrl.FromFilename(outputPath),
                    OutputFileType = AVFileType.Mpeg4,
                    ShouldOptimizeForNetworkUse = true
                };

                await export.ExportTaskAsync();
                return true;
            }
            catch (Exception ex)
            {
                Device.Log.Error(ex.Message);
                await errorAction.Apply(ex, "Failed to compress the video file.");
                return false;
            }
        }
    }
}