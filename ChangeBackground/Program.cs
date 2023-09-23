using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;

namespace DesktopBackground
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SET_DESKTOP_BACKGROUND = 20;
        private const int UPDATE_INI_FILE = 0x01;
        private const int SEND_WINDOWS_INI_CHANGE = 0x02;

        private static Timer _timer;
        private static WebClient _client = new WebClient();
        private static string _category;

        static void Main(string[] args)
        {
            int intervalMinutes = 10;
            _category = "food";

            if (args.Length > 0 && !int.TryParse(args[0], out intervalMinutes))
                intervalMinutes = 120;

            if (args.Length > 1)
                _category = args[1];

            var timer = new Timer(ChangeBackground, null, TimeSpan.Zero, TimeSpan.FromSeconds(intervalMinutes));
            Thread.Sleep(Timeout.Infinite);
        }

        private static void ChangeBackground(object state)
        {
            var imageUrl = GetRandomImageUrl();
            var filePath = Path.Combine(Path.GetTempPath(), "wallpaper.jpg");
            _client.DownloadFile(imageUrl, filePath);

            using (var img = Image.Load(filePath))
            {
                var bmpFilePath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
                using (var stream = new FileStream(bmpFilePath, FileMode.Create))
                {
                    img.Save(stream, new BmpEncoder());
                }
                SystemParametersInfo(SET_DESKTOP_BACKGROUND, 0, bmpFilePath, UPDATE_INI_FILE | SEND_WINDOWS_INI_CHANGE);
            }
        }

        private static string GetRandomImageUrl()
        {
            return $"https://source.unsplash.com/category/{_category}/1920x1080";
        }
    }
}
