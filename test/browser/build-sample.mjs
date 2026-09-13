import { execFileSync } from "node:child_process";
import { cpSync, mkdirSync, rmSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const directory = path.dirname(fileURLToPath(import.meta.url));
const sample = path.resolve(directory, "..", "..", "samples", "ScissorHands.Sample");
const options = {
  cwd: sample,
  stdio: "inherit",
  env: {
    ...process.env,
    Site__BaseUrl: "/",
    Site__UseLocaleInUrl: "false",
    Site__Locale: "en-US",
    Site__Theme: "default",
    Logging__LogLevel__Default: "Warning",
  },
};

execFileSync("dotnet", ["build", "-c", "Release", "--no-restore", "--verbosity", "minimal"], options);
execFileSync("dotnet", ["run", "-c", "Release", "--no-build", "--no-launch-profile", "--", "--build"], {
  ...options,
  env: { ...options.env, Site__BaseUrl: "/docs/", Site__UseLocaleInUrl: "true", Site__Locale: "ko-KR" },
});
const prefixArtifact = path.join(directory, "artifacts", "prefix");
rmSync(prefixArtifact, { recursive: true, force: true });
mkdirSync(path.dirname(prefixArtifact), { recursive: true });
cpSync(path.join(sample, "dist"), prefixArtifact, { recursive: true });
execFileSync("dotnet", ["run", "-c", "Release", "--no-build", "--no-launch-profile", "--", "--build"], options);
