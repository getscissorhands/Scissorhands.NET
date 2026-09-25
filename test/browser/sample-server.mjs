import http from "node:http";
import { readFile, realpath, stat } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const contentTypes = {
  ".html": "text/html; charset=utf-8",
  ".css": "text/css; charset=utf-8",
  ".js": "text/javascript; charset=utf-8",
  ".svg": "image/svg+xml",
  ".ico": "image/x-icon",
  ".png": "image/png",
  ".jpg": "image/jpeg",
};

export async function startSampleServer({ prefix = "", preview = false } = {}) {
  const directory = path.dirname(fileURLToPath(import.meta.url));
  const root = await realpath(preview
    ? path.join(directory, "artifacts", "preview")
    : prefix
    ? path.join(directory, "artifacts", "prefix")
    : path.resolve(directory, "..", "..", "sample", "dist"));
  const isWithinRoot = candidate => {
    const relative = path.relative(root, candidate);
    return relative !== ".." && !relative.startsWith(`..${path.sep}`) && !path.isAbsolute(relative);
  };
  const server = http.createServer(async (request, response) => {
    try {
      const url = new URL(request.url, "http://localhost");
      if (prefix && url.pathname !== prefix && !url.pathname.startsWith(`${prefix}/`)) {
        response.writeHead(404).end();
        return;
      }
      const pathname = url.pathname.slice(prefix.length);
      let filename = path.resolve(root, `.${decodeURIComponent(pathname)}`);
      if (!isWithinRoot(filename)) {
        response.writeHead(403).end();
        return;
      }
      filename = await realpath(filename);
      if (!isWithinRoot(filename)) {
        response.writeHead(403).end();
        return;
      }
      if ((await stat(filename)).isDirectory()) {
        if (!url.pathname.endsWith("/")) {
          response.writeHead(301, { Location: `${url.pathname}/${url.search}` }).end();
          return;
        }
        filename = path.join(filename, "index.html");
      }
      filename = await realpath(filename);
      if (!isWithinRoot(filename)) {
        response.writeHead(403).end();
        return;
      }
      const body = await readFile(filename);
      response.writeHead(200, {
        "Content-Type": contentTypes[path.extname(filename)] ?? "application/octet-stream",
        "Cache-Control": "no-store",
      });
      response.end(body);
    } catch (error) {
      if (error.code === "ENOENT" || error.code === "ENOTDIR") {
        response.writeHead(404).end();
      } else {
        console.error("Sample test server failed:", error);
        response.writeHead(500).end();
      }
    }
  });
  await new Promise((resolve, reject) => {
    server.once("error", reject);
    server.listen(0, "127.0.0.1", resolve);
  });
  return {
    origin: `http://127.0.0.1:${server.address().port}`,
    close: () => new Promise((resolve, reject) => {
      server.close(error => error ? reject(error) : resolve());
      server.closeAllConnections();
    }),
  };
}
