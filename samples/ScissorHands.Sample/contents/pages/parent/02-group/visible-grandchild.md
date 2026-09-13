---
title: Visible Grandchild
description: A visible grandchild under a missing-parent group.
slug: parent/group/visible-grandchild
show_in_navigation: true
tags:
  - sample
  - hierarchy
  - navigation
  - visibility
---

# Visible Grandchild

This page appears below [Parent](parent) and the non-clickable Group label. There is no `index.md` in the group directory, so the label has no link.

The source directory `02-group` places this page between [Child](parent/child) and [Child 2](parent/child-2) in the reading sequence. Its explicit slug keeps the public URL and the Group label independent of that numeric prefix.

This page keeps Group visible while its sibling, [Hidden Grandchild](parent/group/hidden-grandchild), is hidden from navigation. Hide this page too and Group disappears. Hiding Parent also hides the entire branch.
