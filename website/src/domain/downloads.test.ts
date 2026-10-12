import { describe, expect, it } from "vitest";
import { aggregateDownloads, countAssets, mostDownloadedAsset } from "@/domain/downloads";
import type { Product } from "@/domain/product";

const product = (overrides: Partial<Product>): Product => ({
  name: "P",
  description: "",
  asset: "P.zip",
  variants: [],
  externalDownloads: [],
  recommended: false,
  icon: "",
  source: "",
  accent: "teal",
  title: { kind: "plain", head: "P" },
  ...overrides,
});

describe("downloads", () => {
  it("sums asset downloads across releases", () => {
    const releases = [
      { assets: [{ name: "a.zip", downloadCount: 2 }] },
      { assets: [{ name: "a.zip", downloadCount: 3 }, { name: "b.zip", downloadCount: 1 }] },
    ];

    expect(countAssets(releases)).toEqual({ "a.zip": 5, "b.zip": 1 });
  });

  it("folds variants and external downloads into the product asset", () => {
    const products = [
      product({
        asset: "A.zip",
        variants: ["A_fat.zip"],
        externalDownloads: [
          { repo: "o/ext", match: { type: "exact", asset: "A.exe" } },
          { repo: "o/pre", match: { type: "prefix", prefix: "A_" } },
        ],
      }),
    ];

    const totals = aggregateDownloads(products, { "A.zip": 10, "A_fat.zip": 5, "other.zip": 1 }, {
      "o/ext": { "A.exe": 7, "B.exe": 100 },
      "o/pre": { "A_1.dll": 2, "A_2.dll": 3 },
    });

    expect(totals).toEqual({ "A.zip": 27, "other.zip": 1 });
  });

  it("picks the most downloaded product asset", () => {
    const products = [product({ asset: "A.zip" }), product({ asset: "B.zip" })];

    expect(mostDownloadedAsset(products, { "A.zip": 1, "B.zip": 4 })).toBe("B.zip");
    expect(mostDownloadedAsset(products, {})).toBeNull();
  });
});
