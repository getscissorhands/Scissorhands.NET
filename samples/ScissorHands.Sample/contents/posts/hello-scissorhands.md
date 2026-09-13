---
title: Hello, ScissorHands
description: A sample post rendered from Markdown through the built-in Razor theme.
published: 2026-09-11
tags:
  - dotnet
  - static-site
---

# Hello, ScissorHands

Publishing a small site should not require a running application server for every visit. ScissorHands.NET takes Markdown content, combines it with a Razor theme, and produces static files that a web host can serve directly.

This article follows that process from source content to a finished page. Along the way, it demonstrates the headings, lists, links, images, and code that make up a typical technical post.

## Start with readable content

Markdown keeps **content structure** separate from its visual styling. Headings describe the outline, paragraphs carry the explanation, and *emphasis* highlights a point without requiring custom HTML.

The YAML frontmatter above this article supplies its title, description, publication date, and tags. The body begins with a single top-level heading; subsequent sections use second- and third-level headings so readers can follow the outline.

Inline code is useful for filenames such as `hello-scissorhands.md`, configuration keys such as `Site.BaseUrl`, and short commands. Longer examples belong in fenced code blocks, where indentation and line breaks are preserved.

![A stylized hand holding scissors beside two sparkles.](images/sample.svg)

*This illustration is included in the sample's content assets, so it needs no external image service.*

## Follow the generation pipeline

When the engine builds the site, it performs a series of steps:

1. Load Markdown files, validate their frontmatter, resolve their routes, and skip drafts.
2. Run enabled pre-Markdown plugins, then convert Markdown to HTML.
3. Run post-Markdown plugins and render the content through the selected Razor theme.
4. Run post-HTML plugins on the rendered document.
5. Write the resulting HTML and copy content images and theme assets into the output directory.

This separation lets a theme focus on layout while plugins handle content transformations. The same Markdown can therefore be presented differently without rewriting the article itself.

### Keep the host small

The application entry point only needs to create the host and run it:

```csharp
using ScissorHands.Web;

var app = new ScissorHandsApplicationBuilder(args).Build();
await app.RunAsync();
```

The command-line argument selects the operating mode. Theme selection and other site settings live in configuration rather than in the article.

### Choose a mode for the task

| Mode | Directory | Purpose |
| --- | --- | --- |
| Preview | `preview/` | Review local edits |
| Build | `dist/` | Prepare published files |

Run `dotnet run --no-launch-profile -- --preview` while editing. Content changes regenerate the preview output; refresh the browser to see the result. Razor and C# changes still require recompilation.

When the site is ready to publish, use `dotnet run --no-launch-profile -- --build`. This generates static output without starting the preview server.

## Configure routes deliberately

A site's configuration provides the defaults that individual documents can build on. For example:

```json
{
  "Site": {
    "Title": "My ScissorHands Site",
    "Description": "Practical notes about building a small, readable static site with Markdown content and reusable Razor themes.",
    "Theme": "default",
    "SiteUrl": "https://example.com",
    "BaseUrl": "/",
    "UseDateInPostUrl": true
  }
}
```

Use `BaseUrl` when publishing below a subpath, such as `/project/`. Internal links and image paths should remain compatible with that base instead of assuming the site always lives at the domain root.

### Let directories describe page hierarchy

Posts are useful for dated articles, while pages provide stable sections. The sample's [Parent page](parent) and [Visible Grandchild](parent/group/visible-grandchild) demonstrate how page routes also shape navigation:

- Landing pages
    - `parent/index.md` infers the route `parent` without an explicit slug.
    - `parent/child.md` infers the route `parent/child`.
- Navigation
    - Existing pages opt in with `show_in_navigation: true`.
    - A missing intermediate page becomes a non-clickable group when it has a visible descendant.
    - Hiding an existing parent hides its entire navigation branch.
- Metadata
    - An explicit `slug` overrides the inferred route.
    - Tags connect related posts and pages independently of their navigation visibility.

> Hiding a navigation entry is a presentation choice, not access control. The generated page can still be opened through its URL or a tag listing.

## Review the rendered result

A successful build is only part of the review. Read a few paragraphs at both wide and narrow viewport sizes, expand a navigation group with the keyboard, and compare the light and dark themes.

Check that nested lists remain easy to follow, the table stays readable, and long code lines scroll within their block rather than widening the whole page. Images should fit their container, while links and tag labels should remain distinguishable from surrounding text.

---

## Publish a site you can maintain

Once the content and layout are ready, publish the contents of `dist` to a static host. Keep the Markdown sources alongside the application so future changes can be reviewed and regenerated in the same way.

For another example, visit the [Parent page](parent), explore the [static-site tag](tags/static-site), or browse the [ScissorHands.NET repository](https://github.com/getscissorhands/Scissorhands.NET).
