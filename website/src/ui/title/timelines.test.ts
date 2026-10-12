import { describe, expect, it } from "vitest";
import { TITLE_TIMING } from "@/config/motion";
import { rapidTimeline, typewriterTimeline } from "@/ui/title/timelines";

const take = <T,>(frames: Iterator<T>, count: number) => Array.from({ length: count }, () => frames.next().value as T);

describe("title timelines", () => {
  it("erases then retypes the text in a loop", () => {
    const frames = take(typewriterTimeline(".UI", () => 1), 8);

    expect(frames).toEqual([
      [".UI", TITLE_TIMING.startDelay],
      [".U", 1],
      [".", 1],
      ["", TITLE_TIMING.holdEmpty],
      [".", 1],
      [".U", 1],
      [".UI", TITLE_TIMING.holdFull],
      [".U", 1],
    ]);
  });

  it("grows then shrinks the glyph count in a loop", () => {
    const frames = take(rapidTimeline(2, () => 1), 6);

    expect(frames).toEqual([
      [0, TITLE_TIMING.startDelay],
      [1, 1],
      [2, 1 + TITLE_TIMING.holdFull],
      [1, 1],
      [0, 1 + TITLE_TIMING.holdEmpty],
      [1, 1],
    ]);
  });

  it("stops after the rest frame when there is nothing to animate", () => {
    const empty = typewriterTimeline("");
    empty.next();
    expect(empty.next().done).toBe(true);
  });
});
