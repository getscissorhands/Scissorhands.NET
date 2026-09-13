import { defineConfig } from "@playwright/test";

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
      use: { browserName, viewport },
    })),
  ),
});
