# Astro Wall
A MacOS agent to fetch [Astronomy Picture of the Day](https://apod.nasa.gov/apod/astropix.html) and set it as wallpaper.
Written in C#.net as a Xamarin project. Published under the [MIT license](https://github.com/wiegell/AstroWall/blob/master/LICENSE).

## Download
From [public page](https://wiegell.github.io/AstroWall/) or to the right under releases.

## Build instructions:
- Install VS for mac
- Install Xcode
- Install script deps: `cd sripts && npm install`
- Install nuget packages
- Build in VS

## Current features:
- Fetch latest pictures and browse them
- Auto fetch the latest picture each day

## Roadmap:
- Filter pictures based on resolution
- Filter pictures based on description
- Embed description in picture
- Scaling options
- Random picture of all time

## Future thoughts
- Porting to MAUI
- Porting to Windows/Linux

## Source organization
Tries to follow the [application layers as described by Microsoft](https://learn.microsoft.com/en-us/xamarin/cross-platform/app-fundamentals/building-cross-platform-applications/architecture#typical-application-layers). The UI-layer and Application layer is somewhat intertwined, but the business layer should be almost platform independent.