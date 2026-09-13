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

This `index.md` file is the landing page for its directory. With no explicit
`slug`, its route is inferred as `parent`.

The navigation groups pages using their slugs. Expand Parent to find
[Child](parent/child) and the non-clickable Group label.
Group has no page of its own; it appears because
[Visible Grandchild](parent/group/visible-grandchild) is visible in navigation.

Each page opts in with `show_in_navigation: true`. Hiding this page from navigation
also hides its descendants, without removing any generated pages.
