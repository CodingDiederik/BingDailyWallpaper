# BingDailyWallpaper (Windows tray app)

Small Windows tray application that downloads Bing's daily wallpaper and sets it as the desktop background.

Requirements
- .NET 8 SDK (or .NET 7 if you change TargetFramework)
- Windows (uses WinForms + SystemParametersInfo)

Build & run (development)

```powershell
dotnet build
dotnet run --project .\BingDailyWallpaper.csproj
```

Publish single-file EXE 

```powershell
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\publish
```

Usage
- Run the app. It will show a tray icon.
- Right-click or double-click the tray icon to "Check now" (download & set immediately).
- "Open folder" opens the app data folder where images are stored.
- The app checks once immediately at startup and then every 24 hours by default.

Notes
- The app uses Bing's public HPImageArchive JSON API to find the current image URL.
- The downloaded image is saved in `%LOCALAPPDATA%\BingDailyWallpaper` and converted to BMP for the SystemParametersInfo API.
- To run at startup, add a shortcut to the published EXE in your Startup folder or use Task Scheduler.

License
 Copyright (c) 2025 CodingDiederik

 Permission is hereby granted, free of charge, to any person
 obtaining a copy of this software and associated documentation
 files (the "Software"), to deal in the Software without
 restriction, including without limitation the rights to use,
 copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the
 Software is furnished to do so, subject to the following
 conditions:

 The above copyright notice and this permission notice shall be
 included in all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 OTHER DEALINGS IN THE SOFTWARE.
