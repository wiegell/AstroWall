# AstroWall
A MacOS agent to fetch [Astronomy Picture of the Day](https://apod.nasa.gov/apod/astropix.html) and set it as wallpaper.
Written in C#.net as a Xamarin project. Published under the [MIT license](https://github.com/wiegell/AstroWall/blob/master/LICENSE).
![Animation of working app](https://wiegell.github.io/AstroWall/assets/ani.gif)

## Download
[Direct link](https://github.com/wiegell/AstroWall/releases/latest/download/Astro.dmg) or to the right under releases.

## Current features:
- Auto fetch the latest picture each day
- Fetch latest pictures and browse them
- Embed description in picture

## Roadmap:
- Filter pictures based on resolution
- Filter pictures based on description
- Scaling options
- Random picture of all time

## Future thoughts
- Porting to Windows
- Porting to MAUI
- Generalizing to other wallpaper download sources 

## FAQ
### Why can't i open the app?
Right click, then open

### How come macOS is warning me about malware?
Because i didn't pay Apple for a developers license

### What't the catch
None, the project has been made to try out C# skills

## Build instructions:
- Install VS for mac
- Install Xcode
- Install script deps: `cd scripts && npm install`
- Install nuget packages
- Build in VS
## Source organization
Tries to follow the [application layers as described by Microsoft](https://learn.microsoft.com/en-us/xamarin/cross-platform/app-fundamentals/building-cross-platform-applications/architecture#typical-application-layers). The UI-layer and Application layer is intertwined, but the business layer is almost platform independent.