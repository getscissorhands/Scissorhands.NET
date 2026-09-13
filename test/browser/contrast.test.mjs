import assert from "node:assert/strict";
import { test } from "node:test";
import { composite, contrastRatio, measureColor, parseColor, resolveBackground } from "./contrast.mjs";

test("relative luminance has the expected black/white and equal-color limits", () => {
  assert.equal(contrastRatio([0, 0, 0], [255, 255, 255]), 21);
  assert.equal(contrastRatio([255, 255, 255], [0, 0, 0]), 21);
  assert.equal(contrastRatio([68, 68, 68], [68, 68, 68]), 1);
});

test("the original light link color falls below the approved text threshold", () => {
  const ratio = contrastRatio([0, 98, 255], [245, 240, 230]);
  assert.ok(ratio > 4.4 && ratio < 4.5);
});

test("transparent foreground and background layers are composited", () => {
  assert.deepEqual(parseColor("rgb(68, 68, 68)"), [68, 68, 68, 1]);
  assert.deepEqual(composite(parseColor("rgba(0, 0, 0, 0.5)"), [255, 255, 255]), [127.5, 127.5, 127.5]);
  const layers = [
    { color: "rgb(255, 255, 255)", image: "none", opacity: "1" },
    { color: "rgba(0, 0, 0, 0.5)", image: "none", opacity: "1" },
    { color: "rgba(0, 0, 0, 0)", image: "none", opacity: "1" },
  ];
  assert.deepEqual(resolveBackground(layers), [127.5, 127.5, 127.5]);
  assert.equal(measureColor("rgb(0, 0, 0)", layers).ratio, contrastRatio([0, 0, 0], [127.5, 127.5, 127.5]));
});

test("unsupported colors and backgrounds fail rather than claim a contrast pass", () => {
  assert.throws(() => parseColor("transparent"));
  assert.throws(() => parseColor("rgb(999, 0, 0)"));
  assert.throws(() => resolveBackground([{ color: "rgba(0, 0, 0, 0)", image: "none", opacity: "1" }]));
  assert.throws(() => resolveBackground([{ color: "rgb(255, 255, 255)", image: "linear-gradient(white, black)", opacity: "1" }]));
  assert.throws(() => resolveBackground([{ color: "rgb(255, 255, 255)", image: "none", opacity: "0.5" }]));
});
