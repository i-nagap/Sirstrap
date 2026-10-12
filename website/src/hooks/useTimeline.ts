import { useEffect, useState } from "react";

export type Frame<T> = readonly [value: T, holdMs: number];
export type Timeline<T> = () => Iterator<Frame<T>>;

export function useTimeline<T>(rest: T, enabled: boolean, timeline: Timeline<T>): T {
  const [value, setValue] = useState(rest);

  useEffect(() => {
    if (!enabled) return;

    const frames = timeline();
    let timer: ReturnType<typeof setTimeout>;

    const play = () => {
      const step = frames.next();
      if (step.done) return;
      const [next, holdMs] = step.value;
      setValue(next);
      timer = setTimeout(play, holdMs);
    };
    play();

    return () => {
      clearTimeout(timer);
      setValue(rest);
    };
  }, [enabled, rest, timeline]);

  return enabled ? value : rest;
}
