# BingDailyWallpaper

Small Windows application that downloads Bing's daily wallpaper and sets it as the desktop background.

Requirements
- .NET 8 SDK
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

