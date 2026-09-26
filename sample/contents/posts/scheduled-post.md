---
title: Scheduled post
description: An article scheduled for December 31, 2099.
published: 2099-12-31
tags:
  - sample
  - publication-examples
---

# Scheduled post

This article is not a draft, but its `published` date is `2099-12-31`.
Preview includes it with a scheduled badge on the article, homepage, and tag
listings. With the sample's English messages, the badge reads
**Scheduled on 2099-12-31**.

Production builds skip this post until midnight at the start of December 31,
2099 in `Site.TimeZone`, which defaults to UTC. The source file remains available
for editing.

The date alone does not update a deployed site. Run a build and deployment at or
after the publication time to make the article available.
