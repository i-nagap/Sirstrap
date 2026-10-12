const LINE_COUNT = 18;

export function drawWaves(
  ctx: CanvasRenderingContext2D,
  time: number,
  opacity: number,
  [hue, saturation, lightness]: readonly [number, number, number],
  pixelRatio: number,
) {
  const { width, height } = ctx.canvas;

  for (let line = 0; line < LINE_COUNT; line++) {
    const progress = line / (LINE_COUNT - 1);
    const baseY = height * 0.1 + progress * height * 0.8;
    const phase = line * 0.7 + time;
    const amplitude = height * (0.04 + progress * 0.03);

    ctx.beginPath();
    ctx.moveTo(-10, baseY);

    for (let x = 0; x <= width + 10; x += 4) {
      const xNorm = x / width;
      ctx.lineTo(
        x,
        baseY + Math.sin(xNorm * 3 + phase) * amplitude + Math.sin(xNorm * 1.5 + phase * 0.7 + line * 0.3) * amplitude * 0.6,
      );
    }

    ctx.strokeStyle = `hsla(${hue}, ${saturation}%, ${lightness}%, ${(0.08 + progress * 0.08) * opacity})`;
    ctx.lineWidth = 1.5 * pixelRatio;
    ctx.stroke();
  }
}
