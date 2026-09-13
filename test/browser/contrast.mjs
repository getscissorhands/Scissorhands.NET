export function parseColor(value) {
  const match = /^rgba?\(([^)]+)\)$/.exec(value);
  if (!match) {
    throw new Error(`Unsupported computed color: ${value}`);
  }
  const parts = match[1].split(/[,\s/]+/).filter(Boolean).map(Number);
  if (parts.length === 3) {
    parts.push(1);
  }
  if (parts.length !== 4 || parts.some(value => !Number.isFinite(value))
      || parts.slice(0, 3).some(value => value < 0 || value > 255)
      || parts[3] < 0 || parts[3] > 1) {
    throw new Error(`Invalid computed color: ${value}`);
  }
  return parts;
}

export function composite(foreground, background) {
  return background.map((value, index) => foreground[index] * foreground[3] + value * (1 - foreground[3]));
}

export function resolveBackground(layers) {
  let background;
  for (const layer of layers) {
    if (layer.image !== "none" || Number(layer.opacity) !== 1) {
      throw new Error("Contrast measurement requires solid backgrounds without group opacity.");
    }
    const color = parseColor(layer.color);
    if (color[3] === 0) {
      continue;
    }
    if (!background && color[3] !== 1) {
      throw new Error("No opaque background beneath a translucent layer.");
    }
    background = background ? composite(color, background) : color.slice(0, 3);
  }
  if (!background) {
    throw new Error("No opaque rendered background was found.");
  }
  return background;
}

export function contrastRatio(foreground, background) {
  const luminance = color => {
    const linear = color.map(value => {
      if (!Number.isFinite(value) || value < 0 || value > 255) {
        throw new Error("RGB channels must be finite values between 0 and 255.");
      }
      const channel = value / 255;
      return channel <= 0.04045 ? channel / 12.92 : ((channel + 0.055) / 1.055) ** 2.4;
    });
    if (linear.length !== 3) {
      throw new Error("Contrast requires three RGB channels.");
    }
    return linear[0] * 0.2126 + linear[1] * 0.7152 + linear[2] * 0.0722;
  };
  const values = [luminance(foreground), luminance(background)].sort((a, b) => a - b);
  return (values[1] + 0.05) / (values[0] + 0.05);
}

export function measureColor(color, layers) {
  const background = resolveBackground(layers);
  const foreground = composite(parseColor(color), background);
  return { foreground, background, ratio: contrastRatio(foreground, background) };
}
