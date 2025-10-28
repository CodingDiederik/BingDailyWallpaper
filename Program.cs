using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

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
                    Console.Error.WriteLine("Failed to fetch image bytes.");
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
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unhandled error: " + ex);
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
                if (!resp.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Bing metadata request failed: {resp.StatusCode}");
                    return null;
                }

                using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                var root = doc.RootElement;
                if (!root.TryGetProperty("images", out var images) || images.GetArrayLength() == 0) return null;
                var image = images[0];

                var urlBase = image.GetProperty("urlbase").GetString();
                if (string.IsNullOrEmpty(urlBase)) return null;

                var match = Regex.Match(urlBase, @"OHR\.([^_]+)");
                var name = match.Success ? match.Groups[1].Value : null;
                if (string.IsNullOrEmpty(name)) return null;

                var fallbackUrl = $"https://cdn.bingwalls.com/bing-wallpapers/{name}_landscape.jpg";

                using var fallbackResp = await httpClient.GetAsync(fallbackUrl);
                if (!fallbackResp.IsSuccessStatusCode) return null;
                var fallbackBytes = await fallbackResp.Content.ReadAsByteArrayAsync();
                return (fallbackBytes, $"{name}.jpg");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error fetching image: " + ex);
                return null;
            }
        }
    }
}
