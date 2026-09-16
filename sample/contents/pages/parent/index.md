---
title: Parent
description: A parent page demonstrating hierarchical navigation.
show_in_navigation: true
tags:
  - sample
  - hierarchy
  - navigation
  - directory-index
---

# Parent

This `index.md` file is the landing page for its directory. With no explicit `slug`, its route is inferred as `parent`.

The navigation groups pages using their slugs. Expand Parent to find [Child](parent/child), the non-clickable Group label, and [Child 2](parent/child-2). Group has no page of its own; it appears because [Visible Grandchild](parent/group/visible-grandchild) is visible in navigation.

Reading order comes from source filenames, not titles or slugs. This index comes first within its directory, followed by `01-child.md`, the eligible pages in `02-group`, and `03-child-2.md`. The theme's previous/next links follow that sequence across directories.

Each page opts in with `show_in_navigation: true`. Hiding this page from navigation also hides its descendants, without removing any generated pages.
