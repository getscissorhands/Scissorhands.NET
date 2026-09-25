import assert from "node:assert/strict";
import { execFileSync, spawn } from "node:child_process";
import { cpSync, mkdirSync, readdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const directory = path.dirname(fileURLToPath(import.meta.url));
const sample = path.resolve(directory, "..", "..", "sample");
const options = {
  cwd: sample,
  stdio: "inherit",
  env: {
    ...process.env,
    Site__BaseUrl: "/",
    Site__Locale: "en-US",
    Site__Theme: "default",
    Logging__LogLevel__Default: "Warning",
  },
};

execFileSync("dotnet", ["build", "-c", "Release", "--no-restore", "--verbosity", "minimal"], options);
const localizedSource = path.join(directory, "artifacts", "locale-source");
rmSync(localizedSource, { recursive: true, force: true });
mkdirSync(localizedSource, { recursive: true });
cpSync(path.join(sample, "contents"), path.join(localizedSource, "contents"), { recursive: true });
cpSync(path.join(sample, "appsettings.json"), path.join(localizedSource, "appsettings.json"));
const fixtures = [
  ["posts", "english-post.md", "title: English post\nslug: welcome\npublished: 2026-09-16\ntags: [dotnet, english-only]"],
  ["posts", "ko-kr/english-post.md", "title: Korean post\nslug: welcome\npublished: 2026-09-16\ntags: [dotnet, korean-only]"],
  ["pages", "draft.md", "title: Draft primary\ndraft: true\nshow_in_navigation: true\ntags: [preview-only]"],
  ["pages", "ko-kr/draft.md", "title: Ready translation of a draft"],
  ["pages", "ja-jp/orphan.md", "title: Orphan translation"],
  ["posts", "scheduled-preview.md", "title: Scheduled preview\ndraft: true\npublished: 2099-01-01\ntags: [preview-only]"],
  ["posts", "ko-kr/scheduled-preview.md", "title: Korean scheduled preview\npublished: 2099-01-01\ntags: [preview-only]"],
];
for (const [kind, filename, metadata] of fixtures) {
  const destination = path.join(localizedSource, "contents", kind, filename);
  mkdirSync(path.dirname(destination), { recursive: true });
  writeFileSync(destination, `---\n${metadata}\n---\n# Locale fixture\n`);
}
function buildLocalized(baseUrl) {
  execFileSync("dotnet", [path.join(sample, "bin", "Release", "net10.0", "ScissorHands.Sample.dll"), "--build"], {
    ...options,
    cwd: localizedSource,
    env: {
      ...options.env,
      Site__BaseUrl: baseUrl,
      "Site__LocalizationFallbackMessages__ja-jp": "このページは現在日本語翻訳を提供していません",
    },
  });
}

function readArtifact(root, relative = "") {
  return readdirSync(path.join(root, relative), { withFileTypes: true }).flatMap(entry => {
    const filename = path.join(relative, entry.name);
    if (entry.isDirectory()) return readArtifact(root, filename);
    assert(entry.isFile(), `Unexpected artifact entry: ${filename}`);
    return [[filename, readFileSync(path.join(root, filename))]];
  });
}

buildLocalized("/docs");
const prefixArtifact = path.join(directory, "artifacts", "prefix");
rmSync(prefixArtifact, { recursive: true, force: true });
mkdirSync(path.dirname(prefixArtifact), { recursive: true });
cpSync(path.join(localizedSource, "dist"), prefixArtifact, { recursive: true });
buildLocalized("/docs/");
const expectedArtifact = new Map(readArtifact(prefixArtifact));
const actualArtifact = new Map(readArtifact(path.join(localizedSource, "dist")));
assert.deepEqual([...actualArtifact.keys()].sort(), [...expectedArtifact.keys()].sort());
for (const [filename, bytes] of expectedArtifact) {
  assert(bytes.equals(actualArtifact.get(filename)), `BaseUrl slash variants changed ${filename}`);
}
await buildPreview();
execFileSync("dotnet", ["run", "-c", "Release", "--no-build", "--no-launch-profile", "--", "--build"], options);

async function buildPreview() {
  const child = spawn("dotnet", [path.join(sample, "bin", "Release", "net10.0", "ScissorHands.Sample.dll"), "--preview"], {
    cwd: localizedSource,
    env: {
      ...options.env,
      Site__BaseUrl: "/docs/",
      ASPNETCORE_URLS: "http://127.0.0.1:0",
      "Logging__LogLevel__Microsoft.Hosting.Lifetime": "Information",
      "Site__LocalizationFallbackMessages__ja-jp": "Translation not available.",
    },
    stdio: ["ignore", "pipe", "pipe"],
  });
  let output = "";
  let stopped = false;
  let startupError;
  child.stdout.on("data", data => { output += data; });
  child.stderr.on("data", data => { output += data; });
  child.on("error", error => { startupError = error; });
  const exited = new Promise(resolve => child.once("close", () => { stopped = true; resolve(); }));
  try {
    let ready = false;
    for (let attempt = 0; attempt < 300; attempt++) {
      if (startupError) throw startupError;
      if (stopped) throw new Error(`Preview exited before it was ready:\n${output}`);
      const address = output.match(/Now listening on: (http:\/\/127\.0\.0\.1:\d+)/)?.[1];
      if (address) {
        const response = await fetch(`${address}/docs/2099/01/01/scheduled-preview/`);
        const script = await fetch(`${address}/docs/themes/default/assets/theme.js`);
        if (response.ok && script.ok && (await response.text()).includes("Scheduled on 2099-01-01")) {
          ready = true;
          break;
        }
      }
      await new Promise(resolve => setTimeout(resolve, 100));
    }
    assert(ready, `Preview did not become ready:\n${output}`);
    const artifact = path.join(directory, "artifacts", "preview");
    rmSync(artifact, { recursive: true, force: true });
    cpSync(path.join(localizedSource, "preview"), artifact, { recursive: true });
  } finally {
    if (!stopped) child.kill();
    await exited;
  }
}
