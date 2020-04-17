# MetaBrainz.MusicBrainz.dotnet-mbdiscid [![Build Status](https://img.shields.io/appveyor/build/zastai/metabrainz-musicbrainz-dotnet-mbdiscid)](https://ci.appveyor.com/project/Zastai/metabrainz-musicbrainz-dotnet-mbdiscid) [![NuGet Version](https://img.shields.io/nuget/v/MetaBrainz.MusicBrainz.dotnet-mbdiscid)](https://www.nuget.org/packages/MetaBrainz.MusicBrainz.dotnet-mbdiscid)

A small program using `MetaBrainz.MusicBrainz.DiscId` to show information about an audio CD, including the MusicBrainz disc ID, CD-TEXT data, ISRC values, ...

Intended for use as a .NET global tool; install it via
```
dotnet tool install -g MetaBrainz.MusicBrainz.dotnet-mbdiscid
```
Once done, you can use `dotnet mbdiscid` to run it.

## Release Notes

### v1.0.1 (2020-04-17)

Fixed a build issue causing the XML documentation to be missing from the NuGet package.

### v1.0 (2020-03-22)

First release.

- Targets only .NET Core 2.1 and 3.1 (the current LTS releases), because it's intended to be used as a .NET global tool.
