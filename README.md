# ScissorHands.NET

A Blazor-based static site generator

## Prerequisites

- [.NET 10+ SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2026](https://visualstudio.microsoft.com/downloads/) or [VS Code](https://code.visualstudio.com/download) + [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

## Getting Started

1. Create a new console app.

    ```bash
    dotnet new console -n MyScissorHandsSite
    ```

1. Add the NuGet package.

    ```bash
    dotnet add package ScissorHands.Web --prerelease
    ```

   > Currently, ScissorHands is public preview. Therefore, add the `--prerelease` option.

1. Open `.csproj` file and change the SDK reference of the project to Web.

    ```xml
    👇👇👇 Remove 👇👇👇
    <Project Sdk="Microsoft.NET.Sdk">
    👆👆👆 Remove 👆👆👆
    
    👇👇👇 Add 👇👇👇
    <Project Sdk="Microsoft.NET.Sdk.Web">
    👆👆👆 Add 👆👆👆
    ...
    </Project>
    ```

1. Remove the `OutputType` property from the `PropertyGroup` node.

    ```xml
    <PropertyGroup>
      👇👇👇 Remove 👇👇👇
      <OutputType>Exe</OutputType>
      👆👆👆 Remove 👆👆👆
      <TargetFramework>net10.0</TargetFramework>
      ...
    </PropertyGroup>
    ```

1. Add the following properties to the `PropertyGroup` node.

    ```xml
    <PropertyGroup>
      ...
      <GenerateDocumentationFile>false</GenerateDocumentationFile>
      <EnableDefaultContentItems>false</EnableDefaultContentItems>
      ...
    </PropertyGroup>
    ```

   > **NOTE**: You may also need to add the following properties to the `PropertyGroup` node, if necessary.
   >
   > ```xml
   > <PropertyGroup>
   >   ...
   >   <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
   >   <GenerateTargetFrameworkAttribute>false</GenerateTargetFrameworkAttribute>
   >   ...
   > </PropertyGroup>
   > ```

1. Add the following `ItemGroup` node to build static contents.

    ```xml
      <ItemGroup>
        <Content Include="contents/**/*" CopyToOutputDirectory="PreserveNewest" />
        <Content Include="themes/**/*" CopyToOutputDirectory="PreserveNewest" />
      </ItemGroup>
    ...
    </Project>
    ```

1. Add your preferred theme to the `themes` directory. For example, if you want to use the [MinimalBlog](https://github.com/getscissorhands/MinimalBlog) theme, clone it under the `themes` directory.

    ```bash
    pushd themes
    git clone https://github.com/getscissorhands/MinimalBlog.git
    popd
    ```

1. (Optional) Add your preferred plugin packages. For example, here's how to add a [Google Analytics](https://analytics.google.com) plugin.

    ```bash
    dotnet add package ScissorHands.Plugin.GoogleAnalytics --prerelease
    ```

1. Open `Program.cs` and add the following codes.

    ```csharp
    using ScissorHands.Theme.MinimalBlog;
    using ScissorHands.Web;
    
    var app = await new ScissorHandsApplication<MainLayout, IndexView, PostView, PageView>(args)
                        .VerifyCommandArguments()
                        .InitializeAsync();
    await app.RunAsync();
    ```

1. Run the app for preview.

    ```bash
    dotnet run -- --preview
    ```

1. Run the app to build static contents.

    ```bash
    dotnet run -- --build
    ```

## Issues?

If you find any issues, please [report them](../../issues).
