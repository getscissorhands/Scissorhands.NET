import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
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
  ["pages", "draft.md", "title: Draft primary\ndraft: true"],
  ["pages", "ko-kr/draft.md", "title: Ready translation of a draft"],
  ["pages", "ja-jp/orphan.md", "title: Orphan translation"],
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
execFileSync("dotnet", ["run", "-c", "Release", "--no-build", "--no-launch-profile", "--", "--build"], options);
