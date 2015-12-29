# Scissorhands.NET
A markdown based static blog engine in .NET

# Dev setup
## OSX

1. Install .NET CLI

Go to dotnet.github.io and download/install .NET CLI

2. Update feed setting

Create a file `~/.config/Nuget/NuGet.config` and copy/paste below

``` xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="aspnet-contrib" value="https://www.myget.org/F/aspnet-contrib/api/v3/index.json" />
    <add key="nuget.org" value="https://www.nuget.org/api/v2/" />
  </packageSources>
  <disabledPackageSources />
  <activePackageSource>
    <add key="nuget.org" value="https://www.nuget.org/api/v2/" />
  </activePackageSource>
</configuration>
```

This file will be automatically picked up by .NET CLI

3. Restore project

Run `dotnet restore`

At the end of the process, the following messeage will be shown.

```
NuGet Config files used:
    /Users/{your username}/.config/NuGet/nuget.config

Feeds used:
    https://www.myget.org/F/aspnet-contrib/api/v3/flatcontainer/
    https://www.nuget.org/api/v2/
```