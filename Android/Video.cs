using Zebble;
using System;
using System.Threading.Tasks;
using XamarinAndroidFFmpeg;
using System.Collections.Generic;
using System.Windows.Input;

namespace Zebble
{
    public partial class Video
    {
        public static Task<bool> DoCompress(string inputPath, string outputPath, OnError errorAction)
        {
            var task = new TaskCompletionSource<bool>();
            try
            {
                //if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
                //{

                //    Xamarin.MP4Transcoder.Transcoder.For720pFormat().ConvertAsync(inputFile, ouputFile, f =>
                //    {
                //        onProgress?.Invoke((int)(f * (double)100), 100);
                //    });
                //    return ouputFile;

                //}
                CompressByFFMPEG(inputPath, outputPath, (u) =>
                {
                    task.SetResult(true);
                });
            }
            catch (Exception ex)
            {
                Device.Log.Error(ex.Message);
                errorAction.Apply(ex, "Failed to compress the video file.").GetAwaiter();
                task.SetResult(false);
            }
            return task.Task;
        }

        public static void CompressByFFMPEG(string inputPath, string outputPath, Action<string> callback)
        {
            Task.Run(() =>
            {
                var workingDirectory = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
                var ffmpeg = new FFMpeg(UIRuntime.CurrentActivity, workingDirectory);
                var vfTranspose = new TransposeVideoFilter(TransposeVideoFilter.NINETY_CLOCKWISE);
                var filters = new List<VideoFilter> { vfTranspose };

                var sourceClip = new Clip(System.IO.Path.Combine(workingDirectory, inputPath))
                {
                    videoFilter = VideoFilter.Build(filters)
                };

                var onComplete = new CommandExecuter((_) =>
                {
                    callback(outputPath);
                });

                var onMessage = new CommandExecuter((message) =>
                {
                    Device.Log.Warning(message);
                });

                var callbacks = new FFMpegCallbacks(onComplete, onMessage);

                string[] cmds = new string[] {
                    "-y",
                    "-i",
                    sourceClip.path,
                   "-strict", "experimental",
                            "-vcodec", "libx264",
                            "-preset", "ultrafast",
                            "-crf","30", "-acodec","aac", "-ar", "44100" ,
                            "-q:v", "20",
                      "-vf",sourceClip.videoFilter,
                     // "mp=eq2=1:1.68:0.3:1.25:1:0.96:1",

                    outputPath ,
                };

                ffmpeg.Execute(cmds, callbacks);
            });
        }

        public class CommandExecuter : ICommand
        {
            public delegate void ICommandOnExecute(object parameter = null);
            public delegate bool ICommandOnCanExecute(object parameter);

            private ICommandOnExecute _execute;
            private ICommandOnCanExecute _canExecute;

            public CommandExecuter(ICommandOnExecute onExecuteMethod)
            {
                _execute = onExecuteMethod;
            }

            public CommandExecuter(ICommandOnExecute onExecuteMethod, ICommandOnCanExecute onCanExecuteMethod)
            {
                _execute = onExecuteMethod;
                _canExecute = onCanExecuteMethod;
            }

            public event EventHandler CanExecuteChanged
            {
                add { throw new NotImplementedException(); }
                remove { throw new NotImplementedException(); }
            }

            public bool CanExecute(object parameter)
            {
                if (_canExecute == null && _execute != null)
                    return true;

                return _canExecute.Invoke(parameter);
            }

            public void Execute(object parameter)
            {
                if (_execute == null)
                    return;

                _execute.Invoke(parameter);
            }
        }
    }
}
