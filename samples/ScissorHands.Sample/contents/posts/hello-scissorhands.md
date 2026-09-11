---
title: Hello, ScissorHands
description: A sample post rendered from Markdown through the built-in Razor theme.
published: 2026-09-11
tags:
  - dotnet
  - static-site
---

# Hello, ScissorHands

This page demonstrates the complete generation pipeline:

1. YAML frontmatter is loaded and validated.
2. Markdown is converted to HTML.
3. The built-in Razor theme renders the final static page.
4. Tag pages and navigation links are generated automatically.

```csharp
var app = new ScissorHandsApplicationBuilder(args).Build();
await app.RunAsync();
```
