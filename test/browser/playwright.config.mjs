import { mkdirSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { defineConfig } from "@playwright/test";

const directory = path.dirname(fileURLToPath(import.meta.url));
const firefoxData = path.join(directory, "artifacts", "firefox-runtime", "data");
const firefoxCache = path.join(directory, "artifacts", "firefox-runtime", "cache");
mkdirSync(firefoxData, { recursive: true, mode: 0o700 });
mkdirSync(firefoxCache, { recursive: true, mode: 0o700 });

const viewports = {
  desktop: { width: 1280, height: 800 },
  mobile: { width: 375, height: 812 },
};

export default defineConfig({
  testDir: ".",
  testMatch: "page-navigation.spec.mjs",
  fullyParallel: true,
  forbidOnly: Boolean(process.env.CI),
  retries: 0,
  workers: 2,
  timeout: 30_000,
  reporter: [
    ["list"],
    ["json", { outputFile: "test-results/results.json" }],
  ],
  use: {
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
  },
  projects: ["chromium", "firefox", "webkit"].flatMap(browserName =>
    Object.entries(viewports).map(([layout, viewport]) => ({
      name: `${browserName}-${layout}`,
      use: {
        browserName,
        viewport,
        ...(browserName === "firefox" ? {
          launchOptions: {
            // Firefox initializes these locations separately from Playwright's temporary profile.
            env: { ...process.env, MOZ_APP_DATA: firefoxData, MOZ_LOCAL_APP_DATA: firefoxCache },
          },
        } : {}),
      },
    })),
  ),
});
