# Themes in ScissorHands.NET

Theme components are discovered automatically from the application's configured
`Site:Theme` slug. A theme should:

- Keep `MainLayout`, `IndexView`, `PostView`, `PageView`, and `NotFoundView` in one namespace.
- Use a namespace whose normalized suffix matches the slug. For example,
  `minimal-blog` maps to `ScissorHands.Theme.MinimalBlog`, and `theme-template`
  maps to `ScissorHands.Theme.Template`.
- Optionally provide `TagListView` and `TagView` in the same namespace. The
  built-in tag views are used when these components are omitted.
- Include the theme source under the consuming Razor project so the components
  are compiled into the application.

The consuming application does not need theme type aliases or `AddLayouts`:

```csharp
using ScissorHands.Web;

var app = new ScissorHandsApplicationBuilder(args).Build();
await app.RunAsync();
```

`AddLayouts` remains available as an explicit compatibility override for themes
that cannot follow the discovery convention.

For more details, visit the [Themes](https://getscissorhands.app/docs/themes/) page.
