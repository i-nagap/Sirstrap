import { TITLE_TIMING, jitter } from "@/config/motion";
import type { Frame } from "@/hooks/useTimeline";

export function* typewriterTimeline(text: string, pause = jitter): Generator<Frame<string>> {
  yield [text, TITLE_TIMING.startDelay];
  if (!text) return;

  while (true) {
    for (let length = text.length - 1; length >= 0; length--) {
      yield [text.slice(0, length), length === 0 ? TITLE_TIMING.holdEmpty : pause(TITLE_TIMING.erase)];
    }
    for (let length = 1; length <= text.length; length++) {
      yield [text.slice(0, length), length === text.length ? TITLE_TIMING.holdFull : pause(TITLE_TIMING.type)];
    }
  }
}

export function* rapidTimeline(max: number, pause = jitter): Generator<Frame<number>> {
  yield [0, TITLE_TIMING.startDelay];
  if (max <= 0) return;

  while (true) {
    for (let count = 1; count <= max; count++) {
      yield [count, pause(TITLE_TIMING.rapid) + (count === max ? TITLE_TIMING.holdFull : 0)];
    }
    for (let count = max - 1; count >= 0; count--) {
      yield [count, pause(TITLE_TIMING.erase) + (count === 0 ? TITLE_TIMING.holdEmpty : 0)];
    }
  }
}
