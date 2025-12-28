# ScissorHands.NET

A Blazor-based static site generator

## Prerequisites

- [.NET 10+ SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2026](https://visualstudio.microsoft.com/downloads/) or [VS Code](https://code.visualstudio.com/download) + [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

## Getting Started

1. Create a new empty web app.

    ```bash
    dotnet new web -n MyScissorHandsApp
    ```

1. Add the NuGet package.

    ```bash
    dotnet add ./MyScissorHandsApp package ScissorHands.Web --prerelease
    ```

   > Currently, ScissorHands is public preview. Therefore, add the `--prerelease` option.

1. Open `Program.cs` and add the following codes.

    ```csharp
    using ScissorHands.Web;

    var app = await new ScissorHandsApplication<MainLayout, IndexView, PostView, PageView>(args)
                        .VerifyCommandArguments()
                        .BuildAsync();
    await app.RunAsync();
    ```

1. Build the app.

    ```bash
    dotnet build
    ```

1. Run the app for preview.

    ```bash
    dotnet run -- --preview
    ```

1. Run the app to build static contents.

    ```bash
    dotnet run -- --build
    ```

> **NOTE**: For more details to run a ScissorHands.NET app, visit the [Quickstart](https://getscissorhands.app/docs/quickstart/) page.

## Issues?

If you find any issues, please [report them](../../issues).
