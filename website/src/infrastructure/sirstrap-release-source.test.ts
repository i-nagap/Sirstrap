import { describe, expect, it } from "vitest";
import { SirstrapReleaseSource } from "@/infrastructure/sirstrap-release-source";

const index = [
  { tagName: "v3", isDraft: true, changelog: "", assets: [{ fileName: "A.zip", downloadUrl: "", downloadCount: 9 }] },
  { tagName: "v2", isDraft: false, changelog: "", assets: [{ fileName: "A.zip", downloadUrl: "", downloadCount: 4 }] },
];

describe("SirstrapReleaseSource", () => {
  it("maps published releases and skips drafts", async () => {
    const source = new SirstrapReleaseSource("index", async () => new Response(JSON.stringify(index)));

    expect(await source.releases()).toEqual([{ tag: "v2", assets: [{ name: "A.zip", downloadCount: 4 }] }]);
  });

  it("returns nothing when the index is unreachable", async () => {
    expect(await new SirstrapReleaseSource("index", async () => new Response(null, { status: 403 })).releases()).toEqual([]);
  });
});
