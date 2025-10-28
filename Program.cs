using System.Runtime.InteropServices;
using System.Text.Json;

namespace BingDailyWallpaper
{
    internal static class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        [STAThread]
        static async Task<int> Main()
        {
            try
            {
                var appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BingDailyWallpaper");
                Directory.CreateDirectory(appFolder); // use the appdata folder to store images

                using var http = new HttpClient();
                var result = await FetchLatestImageBytesAsync(http);
                if (result == null)
                {
                    return 1;
                }

                var (bytes, fileName) = result.Value;
                var jpgPath = Path.Combine(appFolder, fileName);
                await File.WriteAllBytesAsync(jpgPath, bytes);

                var bmpPath = Path.Combine(appFolder, "wallpaper.bmp");
                using (var ms = new MemoryStream(bytes))
                using (var image = Image.FromStream(ms))
                {
                    image.Save(bmpPath, System.Drawing.Imaging.ImageFormat.Bmp);
                }

                bool ok = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, bmpPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
                return ok ? 0 : 2;
            }
            catch
            {
                return 3;
            }
        }

        private static async Task<(byte[] bytes, string fileName)?> FetchLatestImageBytesAsync(HttpClient httpClient)
        // function to fetch the latest Bing image
        {
            try
            {
                var api = $"https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=en-US"; // fetch metadata for today's image
                using var resp = await httpClient.GetAsync(api);
                if (!resp.IsSuccessStatusCode) return null;

                using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                var root = doc.RootElement;
                if (!root.TryGetProperty("images", out var images) || images.GetArrayLength() == 0) return null;
                var image = images[0];
                if (!image.TryGetProperty("url", out var urlElem)) return null;
                var url = urlElem.GetString();
                if (string.IsNullOrEmpty(url)) return null;
                var fullUrl = url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? url : "https://www.bing.com" + url; // fetch the image

                using var imgResp = await httpClient.GetAsync(fullUrl);
                if (!imgResp.IsSuccessStatusCode) return null;
                var bytes = await imgResp.Content.ReadAsByteArrayAsync();

                var uri = new Uri(fullUrl);
                var fileName = Path.GetFileName(uri.LocalPath);
                if (string.IsNullOrEmpty(fileName)) fileName = "bing.jpg";
                return (bytes, fileName);
            }
            catch
            {
                return null;
            }
        }
    }
}
