# Astro Wall
A MacOS agent to fetch [Astronomy Picture of the Day](https://apod.nasa.gov/apod/astropix.html) and set it as wallpaper.
Written in C#.net as a Xamarin project. Published under the [MIT license](https://github.com/wiegell/AstroWall/blob/master/LICENSE).
<p align="center">
  <img src="https://wiegell.github.io/AstroWall/assets/LargeIcon.png" width="250" title="hover text">
</p>

## Download
From [public page](https://wiegell.github.io/AstroWall/) or to the right under releases.

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
- Porting to MAUI
- Porting to Windows/Linux
- Porting to Windows/Linux

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
Tries to follow the [application layers as described by Microsoft](https://learn.microsoft.com/en-us/xamarin/cross-platform/app-fundamentals/building-cross-platform-applications/architecture#typical-application-layers). The UI-layer and Application layer is somewhat intertwined, but the business layer should be almost platform independent.