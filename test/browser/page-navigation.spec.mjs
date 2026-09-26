import { test as base, expect } from "@playwright/test";
import { measureColor } from "./contrast.mjs";
import { startSampleServer } from "./sample-server.mjs";

const test = base.extend({
  site: [async ({}, use) => {
    const server = await startSampleServer();
    try {
      await use(server.origin);
    } finally {
      await server.close();
    }
  }, { scope: "worker" }],
  localeSite: [async ({}, use) => {
    const server = await startSampleServer({ prefix: "/docs" });
    try {
      await use(`${server.origin}/docs`);
    } finally {
      await server.close();
    }
  }, { scope: "worker" }],
  previewSite: [async ({}, use) => {
    const server = await startSampleServer({ prefix: "/docs", preview: true });
    try {
      await use(`${server.origin}/docs`);
    } finally {
      await server.close();
    }
  }, { scope: "worker" }],
});

test("preview shows draft and scheduled badges on documents and collection entries", async ({ page, previewSite }) => {
  for (const palette of ["light", "dark"]) {
    await page.goto(`${previewSite}/ko-kr/2099/01/01/scheduled-preview/`);
    await page.evaluate(value => { document.documentElement.dataset.theme = value; }, palette);
    const article = page.locator("article");
    await expect(article.getByText("초안", { exact: true })).toBeVisible();
    await expect(article.getByText("2099-01-01 공개 예정", { exact: true })).toBeVisible();
    await expect(page.locator("[data-localization-fallback]")).toHaveCount(0);
    const headingTop = await article.locator("h1").evaluate(element => element.getBoundingClientRect().top);
    const badgeBottom = await article.getByText("2099-01-01 공개 예정", { exact: true })
      .evaluate(element => element.getBoundingClientRect().bottom);
    expect(badgeBottom).toBeLessThanOrEqual(headingTop);
  }
  for (const route of ["", "tags/preview-only/"]) {
    await page.goto(`${previewSite}/${route}`);
    const post = page.locator(".post-list li").filter({ has: page.getByRole("link", { name: "Scheduled preview", exact: true }) });
    await expect(post.getByText("Draft", { exact: true })).toBeVisible();
    await expect(post.getByText("Scheduled on 2099-01-01", { exact: true })).toBeVisible();
  }
  const taggedPage = page.locator(".page-list li").filter({ has: page.getByRole("link", { name: "Draft primary", exact: true }) });
  await expect(taggedPage.getByText("Draft", { exact: true })).toBeVisible();
  await page.goto(`${previewSite}/draft/`);
  await expect(page.locator("article").getByText("Draft", { exact: true })).toBeVisible();
  await expect(page.locator(".site-header nav").getByRole("link", { name: "Draft primary", exact: true })).toBeVisible();
  await expect(page.locator(".page-navigation")).toBeVisible();
  await page.goto(`${previewSite}/ko-kr/draft/`);
  await expect(page.locator("article").getByText("초안", { exact: true })).toBeVisible();
  await expect(page.locator("[data-localization-fallback]")).toHaveCount(0);
  await page.goto(`${previewSite}/ja-jp/2099/01/01/scheduled-preview/`);
  await expect(page.locator("article").getByText("下書き", { exact: true })).toBeVisible();
  await expect(page.locator("article").getByText("2099-01-01に公開予定", { exact: true })).toBeVisible();
  await expect(page.locator("article")).toHaveAttribute("lang", "en-us");
  await expect(page.locator("[data-localization-fallback]")).toHaveText("このページは現在日本語翻訳を提供していません");
});

const sequence = [
  { route: "about", title: "About" },
  { route: "parent", title: "Parent" },
  { route: "parent/child", title: "Child" },
  { route: "parent/group/visible-grandchild", title: "Visible Grandchild" },
  { route: "parent/child-2", title: "Child 2" },
];

test("sample publication examples appear only in preview with their individual statuses", async ({ page, previewSite, site, localeSite }) => {
  const examples = [
    { route: "2026/09/11/draft-post/", title: "Draft post", badge: "draft", label: "Draft" },
    { route: "2099/12/31/scheduled-post/", title: "Scheduled post", badge: "scheduled", label: "Scheduled on 2099-12-31" },
    { route: "draft-page/", title: "Draft page", badge: "draft", label: "Draft" },
  ];
  for (const example of examples) {
    expect((await page.goto(`${previewSite}/${example.route}`)).status()).toBe(200);
    const badges = page.locator("article [data-publication-badge]");
    await expect(badges).toHaveCount(1);
    await expect(badges).toHaveAttribute("data-publication-badge", example.badge);
    await expect(badges).toHaveText(example.label);
    await expect(badges).toBeVisible();
    expect((await page.request.get(`${site}/${example.route}`)).status()).toBe(404);
    expect((await page.request.get(`${localeSite}/${example.route}`)).status()).toBe(404);
    expect((await page.request.get(`${localeSite}/ko-kr/${example.route}`)).status()).toBe(404);
  }
  await expect(page.locator(".site-header nav").getByRole("link", { name: "Draft page", exact: true })).toBeVisible();
  await expect(page.locator(".page-navigation")).toBeVisible();
  await page.goto(`${previewSite}/`);
  for (const example of examples.slice(0, 2)) {
    const entry = page.locator(".post-list li").filter({ has: page.getByRole("link", { name: example.title, exact: true }) });
    await expect(entry.locator("[data-publication-badge]")).toHaveText(example.label);
  }
  await page.goto(`${previewSite}/tags/publication-examples/`);
  for (const example of examples) {
    const entry = page.locator("[data-publication-entry]").filter({ has: page.getByRole("link", { name: example.title, exact: true }) });
    await expect(entry.locator("[data-publication-badge]")).toHaveText(example.label);
  }
  expect((await page.request.get(`${site}/tags/publication-examples/`)).status()).toBe(404);
});

test("the generated reading sequence has working, labelled links and no endpoint placeholders", async ({ page, site }) => {
  for (const [index, current] of sequence.entries()) {
    const response = await page.goto(`${site}/${current.route}/`);
    expect(response.status()).toBe(200);
    const nav = page.getByRole("navigation", { name: "Page navigation", exact: true });
    await expect(nav).toBeVisible();
    for (const [relation, target] of [["prev", sequence[index - 1]], ["next", sequence[index + 1]]]) {
      const link = nav.locator(`a[rel="${relation}"]`);
      if (!target) {
        await expect(link).toHaveCount(0);
        continue;
      }
      await expect(link).toHaveAttribute("href", target.route);
      await expect(link).toHaveAttribute("tabindex", "0");
      await expect(link).toHaveAccessibleName(`${relation === "prev" ? "Previous" : "Next"} ${target.title}`);
      const targetResponse = await page.request.get(new URL(target.route, `${site}/`).href);
      expect(targetResponse.status()).toBe(200);
    }
  }
});

test("hidden and synthetic pages omit the pager", async ({ page, site }) => {
  for (const route of ["", "parent/group/hidden-grandchild/", "tags/", "404.html"]) {
    const response = await page.goto(`${site}/${route}`);
    expect(response.status()).toBe(200);
    await expect(page.locator(".page-navigation")).toHaveCount(0);
  }
});

test("locale-aware links resolve on correctly mounted subpath output", async ({ page, localeSite }) => {
  const response = await page.goto(`${localeSite}/ko-kr/parent/group/visible-grandchild/`);
  expect(response.status()).toBe(200);
  await expect(page.locator("html")).toHaveAttribute("lang", "ko-kr");
  await expect(page.locator("base")).toHaveAttribute("href", "/docs/");
  for (const [relation, route] of [["prev", "ko-kr/parent/child"], ["next", "ko-kr/parent/child-2"]]) {
    const link = page.locator(`.page-navigation a[rel="${relation}"]`);
    await expect(link).toHaveAttribute("href", route);
    const resolved = await link.evaluate(anchor => anchor.href);
    expect(resolved).toBe(`${localeSite}/${route}`);
    expect((await page.request.get(resolved)).status()).toBe(200);
  }
});

test("locale collections select translations or fallbacks without changing primary URLs", async ({ page, localeSite }) => {
  await page.goto(`${localeSite}/`);
  await expect(page.locator(".post-link")).toHaveText(["English post", "Hello, ScissorHands"]);
  await expect(page.locator(".site-title")).toHaveAttribute("href", ".");
  await expect(page.locator(".site-header nav a")).toHaveText(["Home", ...sequence.map(item => item.title), "Tags"]);
  await page.getByRole("link", { name: "Tags", exact: true }).click();
  await expect(page).toHaveURL(`${localeSite}/tags/`);
  await page.locator("main a[href='tags/dotnet']").click();
  await expect(page).toHaveURL(`${localeSite}/tags/dotnet/`);
  await expect(page.locator("main")).toContainText("English post");
  await expect(page.locator("main")).toContainText("Hello, ScissorHands");

  await page.goto(`${localeSite}/about/`);
  await expect(page.locator(".page-navigation-previous")).toHaveCount(0);
  await page.locator(".page-navigation-next").click();
  await expect(page).toHaveURL(`${localeSite}/parent/`);
  await page.getByRole("link", { name: "Home", exact: true }).click();
  await expect(page).toHaveURL(`${localeSite}/`);

  await page.goto(`${localeSite}/ko-kr/`);
  await expect(page.locator(".post-link")).toHaveText(["Korean post", "Hello, ScissorHands"]);
  await expect(page.locator(".site-header nav a")).toHaveText(["Home", "소개", ...sequence.slice(1).map(item => item.title), "Tags"]);
  await expect(page.locator("[data-localization-fallback]")).toHaveCount(0);
  await page.goto(`${localeSite}/ko-kr/about/`);
  await expect(page.locator("[data-localization-fallback]")).toHaveCount(0);
  await expect(page.locator("article")).toHaveAttribute("lang", "ko-kr");
  await expect(page.locator("link[rel=canonical]")).toHaveAttribute("href", "http://localhost:5000/docs/ko-kr/about/");
  await expect(page.locator("link[rel=alternate]")).toHaveCount(2);
});

test("primary pages and localized fallback work without JavaScript or locale redirects", async ({ browser, localeSite }, testInfo) => {
  const context = await browser.newContext({ javaScriptEnabled: false, viewport: testInfo.project.use.viewport });
  try {
    const page = await context.newPage();
    for (const route of ["", "tags/", "tags/dotnet/"]) {
      const response = await page.request.get(`${localeSite}/${route}`);
      expect(response.status()).toBe(200);
      const html = await response.text();
      expect(html).not.toContain('http-equiv="refresh"');
      await page.goto(`${localeSite}/${route}`);
      await expect(page).toHaveURL(`${localeSite}/${route}`);
      await expect(page.locator("html")).toHaveAttribute("lang", "en-us");
    }
    await page.goto(`${localeSite}/ko-kr/parent/`);
    await expect(page.getByRole("note")).toHaveText("이 페이지는 현재 한국어 번역을 제공하지 않습니다");
    await expect(page.getByRole("note")).toBeVisible();
    await expect(page.getByRole("note")).toHaveAttribute("lang", "ko-kr");
    await expect(page.locator("article")).toHaveAttribute("lang", "en-us");
    await expect(page.locator("link[rel=canonical]")).toHaveAttribute("href", "http://localhost:5000/docs/parent/");
    await expect(page.locator("link[rel=alternate]")).toHaveCount(1);
    await page.locator(".page-navigation-next").click();
    await expect(page).toHaveURL(`${localeSite}/ko-kr/parent/child/`);
  } finally {
    await context.close();
  }
});

test("configured locales work without translations but primary drafts and orphan translations stay unpublished", async ({ page, localeSite }) => {
  await page.goto(`${localeSite}/ja-jp/`);
  await expect(page.locator("html")).toHaveAttribute("lang", "ja-jp");
  await expect(page.locator(".post-link")).toHaveText(["English post", "Hello, ScissorHands"]);
  await expect(page.locator(".site-header nav a")).toHaveText(["Home", ...sequence.map(item => item.title), "Tags"]);
  for (const route of ["draft/", "ko-kr/draft/", "ja-jp/orphan/", "de-de/", "tags/korean-only/"]) {
    expect((await page.request.get(`${localeSite}/${route}`)).status()).toBe(404);
  }
  for (const route of ["tags/english-only/", "ko-kr/tags/korean-only/", "ja-jp/tags/english-only/", "images/sample.svg", "themes/default/assets/theme.css"]) {
    expect((await page.request.get(`${localeSite}/${route}`)).status()).toBe(200);
  }
});

for (const prefixed of [false, true]) {
  test(`language and authored-link navigation works without JavaScript at ${prefixed ? "a subpath" : "root"}`, async ({ browser, site, localeSite }, testInfo) => {
    const origin = prefixed ? localeSite : site;
    const context = await browser.newContext({ javaScriptEnabled: false, viewport: testInfo.project.use.viewport });
    try {
      const page = await context.newPage();
      await page.goto(`${origin}/about/`);
      const switcher = page.getByRole("navigation", { name: "Language", exact: true });
      await expect(switcher).toBeVisible();
      await expect(switcher.locator("a[lang='en-us']")).toHaveText("English");
      await expect(switcher.locator("a[lang='ko-kr']")).toHaveText("한국어");
      await switcher.locator("a[lang='ko-kr']").click();
      await expect(page).toHaveURL(`${origin}/ko-kr/about/`);
      await expect(switcher.locator("a[aria-current='true']")).toHaveAttribute("lang", "ko-kr");
      await expect(page.getByRole("note")).toHaveCount(0);
      await expect(page.locator("article a[href='images/sample.svg']")).toHaveCount(1);
      await page.locator("article a[href='ko-kr/parent/?from=about#parent']").click();
      await expect(page).toHaveURL(`${origin}/ko-kr/parent/?from=about#parent`);
      await expect(page.getByRole("note")).toBeVisible();
      await expect(switcher.locator("a[aria-current='true']")).toHaveAttribute("lang", "ko-kr");
      await page.locator("article a[href='ko-kr/parent/child']").click();
      await expect(page).toHaveURL(`${origin}/ko-kr/parent/child/`);
      await switcher.locator("a[lang='en-us']").click();
      await expect(page).toHaveURL(`${origin}/parent/child/`);
      await page.goto(`${origin}/ko-kr/about/`);
      await page.locator("article a[data-localize='false']").click();
      await expect(page).toHaveURL(`${origin}/parent/?from=about#parent`);
    } finally {
      await context.close();
    }
  });
}

test("generated pages switch to available counterparts or locale homepages", async ({ browser, localeSite }, testInfo) => {
  const context = await browser.newContext({ javaScriptEnabled: false, viewport: testInfo.project.use.viewport });
  try {
    const page = await context.newPage();
    for (const [route, locale, target] of [
      ["", "ko-kr", "ko-kr/"],
      ["tags/", "ko-kr", "ko-kr/tags/"],
      ["tags/dotnet/", "ko-kr", "ko-kr/tags/dotnet/"],
      ["tags/english-only/", "ko-kr", "ko-kr/"],
      ["ko-kr/tags/korean-only/", "en-us", ""],
      ["404.html", "ja-jp", "ja-jp/"],
    ]) {
      await page.goto(`${localeSite}/${route}`);
      await expect(page.getByRole("note")).toHaveCount(0);
      await page.getByRole("navigation", { name: "Language", exact: true }).locator(`a[lang="${locale}"]`).click();
      await expect(page).toHaveURL(`${localeSite}/${target}`);
      await expect(page.getByRole("note")).toHaveCount(0);
    }
    expect((await page.request.get(`${localeSite}/ja-jp/404.html`)).status()).toBe(404);
  } finally {
    await context.close();
  }
});

test("keyboard users can follow the language switcher", async ({ page, site }) => {
  await page.goto(`${site}/parent/`);
  const nav = page.getByRole("navigation", { name: "Language", exact: true });
  await nav.locator("a[lang='en-us']").focus();
  await page.keyboard.press("Tab");
  await expect(nav.locator("a[lang='ko-kr']")).toBeFocused();
  await page.keyboard.press("Enter");
  await expect(page).toHaveURL(`${site}/ko-kr/parent/`);
});

for (const theme of ["light", "dark"]) {
  test(`${theme} language switcher meets contrast and layout thresholds`, async ({ page, localeSite }) => {
    await page.goto(`${localeSite}/ko-kr/about/`);
    await page.addStyleTag({ content: "*, *::before, *::after { transition: none !important; }" });
    await page.evaluate(value => { document.documentElement.dataset.theme = value; }, theme);
    const links = page.locator(".language-switcher a");
    for (const link of await links.all()) {
      await link.focus();
      const rendered = await link.evaluate(element => {
        const layers = [];
        for (let current = element; current; current = current.parentElement) {
          const style = getComputedStyle(current);
          layers.unshift({ color: style.backgroundColor, image: style.backgroundImage, opacity: style.opacity });
        }
        const style = getComputedStyle(element);
        const box = element.getBoundingClientRect();
        return {
          color: style.color, layers, outline: style.outlineColor, outlineStyle: style.outlineStyle,
          fits: box.left >= 0 && box.right <= innerWidth + 1,
          overflow: document.documentElement.scrollWidth > innerWidth,
        };
      });
      expect(rendered.fits).toBe(true);
      expect(rendered.overflow).toBe(false);
      expect(measureColor(rendered.color, rendered.layers).ratio).toBeGreaterThanOrEqual(4.5);
      expect(rendered.outlineStyle).toBe("solid");
      expect(measureColor(rendered.outline, rendered.layers.slice(0, -1)).ratio).toBeGreaterThanOrEqual(3);
    }
  });
}

for (const theme of ["light", "dark"]) {
  test(`${theme} fallback notice is visible, readable, and above the article`, async ({ page, localeSite }) => {
    await page.goto(`${localeSite}/ko-kr/parent/`);
    await page.evaluate(value => { document.documentElement.dataset.theme = value; }, theme);
    const banner = page.getByRole("note");
    await expect(banner).toBeVisible();
    const rendered = await banner.evaluate(element => {
      const layers = [];
      for (let current = element; current; current = current.parentElement) {
        const style = getComputedStyle(current);
        layers.unshift({ color: style.backgroundColor, image: style.backgroundImage, opacity: style.opacity });
      }
      const box = element.getBoundingClientRect();
      return {
        color: getComputedStyle(element).color, layers,
        beforeArticle: !!(element.compareDocumentPosition(document.querySelector("article")) & Node.DOCUMENT_POSITION_FOLLOWING),
        fits: box.left >= 0 && box.right <= innerWidth + 1,
      };
    });
    expect(rendered.beforeArticle).toBe(true);
    expect(rendered.fits).toBe(true);
    expect(measureColor(rendered.color, rendered.layers).ratio).toBeGreaterThanOrEqual(4.5);
  });
}

test("keyboard users can move between the links and follow Next", async ({ page, site }) => {
  await page.goto(`${site}/parent/group/visible-grandchild/`);
  await page.locator(".page-navigation-previous").focus();
  await page.keyboard.press("Tab");
  await expect(page.locator(".page-navigation-next")).toBeFocused();
  await expect(page.locator(".page-navigation-next")).toHaveCSS("outline-style", "solid");
  await page.keyboard.press("Enter");
  await expect(page).toHaveURL(`${site}/parent/child-2/`);
});

test("navigation works with JavaScript disabled", async ({ browser, site }, testInfo) => {
  const context = await browser.newContext({
    javaScriptEnabled: false,
    viewport: testInfo.project.use.viewport,
  });
  try {
    const page = await context.newPage();
    const response = await page.goto(`${site}/parent/group/visible-grandchild/`);
    expect(response.status()).toBe(200);
    await expect(page.locator(".page-navigation-previous")).toBeVisible();
    await page.locator(".page-navigation-next").click();
    await expect(page).toHaveURL(`${site}/parent/child-2/`);
    await expect(page.locator(".page-navigation-next")).toHaveCount(0);
  } finally {
    await context.close();
  }
});

for (const theme of ["light", "dark"]) {
  test(`${theme} pager satisfies rendered-state contrast and layout checks`, async ({ page, browser, site }, testInfo) => {
    await page.goto(`${site}/parent/group/visible-grandchild/`);
    await page.addStyleTag({ content: "*, *::before, *::after { transition: none !important; animation: none !important; }" });
    await page.evaluate(value => { document.documentElement.dataset.theme = value; }, theme);
    await page.getByRole("navigation", { name: "Page navigation", exact: true }).scrollIntoViewIfNeeded();

    const measurements = [];
    for (const selector of [".page-navigation-previous", ".page-navigation-next"]) {
      const link = page.locator(selector);
      for (const state of ["normal", "hover", "focus"]) {
        await page.mouse.move(0, 0);
        await page.locator(".page-navigation a").evaluateAll(links => links.forEach(link => link.blur()));
        if (state === "hover") {
          await link.hover();
        } else if (state === "focus") {
          await link.focus();
          await page.keyboard.press("Tab");
          await page.keyboard.press("Shift+Tab");
          await expect(link).toBeFocused();
        }
        const rendered = await link.evaluate(link => {
          const layers = element => {
            const result = [];
            for (let current = element; current; current = current.parentElement) {
              const style = getComputedStyle(current);
              result.unshift({ color: style.backgroundColor, image: style.backgroundImage, opacity: style.opacity });
            }
            return result;
          };
          const texts = [];
          const walker = document.createTreeWalker(link, NodeFilter.SHOW_TEXT);
          for (let text = walker.nextNode(); text; text = walker.nextNode()) {
            if (text.textContent.trim()) {
              texts.push({
                text: text.textContent.trim(),
                color: getComputedStyle(text.parentElement).color,
                layers: layers(text.parentElement),
              });
            }
          }
          const style = getComputedStyle(link);
          const box = link.getBoundingClientRect();
          return {
            texts,
            focus: {
              color: style.outlineColor,
              width: parseFloat(style.outlineWidth),
              style: style.outlineStyle,
              layers: layers(parseFloat(style.outlineOffset) < 0 ? link : link.parentElement),
            },
            withinViewport: box.left >= 0 && box.right <= innerWidth + 1,
            horizontalOverflow: document.documentElement.scrollWidth > innerWidth,
          };
        });
        expect.soft(rendered.withinViewport, `${selector} ${state} must fit horizontally`).toBe(true);
        expect.soft(rendered.horizontalOverflow, `${selector} ${state} must not overflow`).toBe(false);
        expect(rendered.texts.length).toBeGreaterThan(0);
        for (const text of rendered.texts) {
          const result = measureColor(text.color, text.layers);
          measurements.push({ selector, state, text: text.text, ...result });
          expect.soft(result.ratio, `${theme} ${selector} ${state} text "${text.text}" contrast`).toBeGreaterThanOrEqual(4.5);
        }
        if (state === "focus") {
          const result = measureColor(rendered.focus.color, rendered.focus.layers);
          measurements.push({ selector, state, indicator: true, ...result });
          expect.soft(rendered.focus.style).toBe("solid");
          expect.soft(rendered.focus.width).toBeGreaterThan(0);
          expect.soft(result.ratio, `${theme} ${selector} focus-indicator contrast`).toBeGreaterThanOrEqual(3);
        }
      }
    }
    await testInfo.attach("rendered-contrast", {
      body: JSON.stringify({
        browser: browser.version(),
        project: testInfo.project.name,
        platform: process.platform,
        viewport: testInfo.project.use.viewport,
        theme,
        measurements,
      }, null, 2),
      contentType: "application/json",
    });
  });
}
