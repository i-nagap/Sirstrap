import { useEffect, useRef } from "react";
import { drawWaves } from "@/ui/product/waves";

const SPEED = 0.0006;
const FADE_SPEED = 0.06;
const INVISIBLE = 0.005;

interface WavyBackgroundProps {
  wave: readonly [number, number, number];
  active: boolean;
}

export function WavyBackground({ wave, active }: WavyBackgroundProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const opacity = useRef(0);
  const time = useRef(0);

  useEffect(() => {
    const canvas = canvasRef.current;
    const parent = canvas?.parentElement;
    const ctx = canvas?.getContext("2d");
    if (!canvas || !parent || !ctx) return;

    const resize = () => {
      const rect = parent.getBoundingClientRect();
      canvas.width = rect.width * devicePixelRatio;
      canvas.height = rect.height * devicePixelRatio;
    };
    resize();

    const observer = new ResizeObserver(resize);
    observer.observe(parent);

    let frame = 0;
    let last = performance.now();

    const tick = (now: number) => {
      time.current += (now - last) * SPEED;
      last = now;
      opacity.current += ((active ? 1 : 0) - opacity.current) * FADE_SPEED;

      ctx.clearRect(0, 0, canvas.width, canvas.height);
      if (opacity.current >= INVISIBLE) drawWaves(ctx, time.current, opacity.current, wave, devicePixelRatio);

      frame = requestAnimationFrame(tick);
    };
    frame = requestAnimationFrame(tick);

    return () => {
      cancelAnimationFrame(frame);
      observer.disconnect();
    };
  }, [wave, active]);

  return <canvas ref={canvasRef} className="pointer-events-none absolute inset-0 z-0 h-full w-full" />;
}
