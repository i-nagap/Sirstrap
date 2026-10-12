import type { Product } from "@/domain/product";
import type { Release } from "@/domain/ports";

export type AssetCounts = Record<string, number>;

export function countAssets(releases: Release[]): AssetCounts {
  const counts: AssetCounts = {};
  for (const release of releases) {
    for (const asset of release.assets) {
      counts[asset.name] = (counts[asset.name] ?? 0) + asset.downloadCount;
    }
  }
  return counts;
}

export function aggregateDownloads(products: Product[], mainCounts: AssetCounts): AssetCounts {
  const totals: AssetCounts = { ...mainCounts };

  const add = (asset: string, amount: number) => {
    if (amount > 0) totals[asset] = (totals[asset] ?? 0) + amount;
  };

  for (const product of products) {
    for (const variant of product.variants) {
      const count = mainCounts[variant];
      if (count) {
        add(product.asset, count);
        delete totals[variant];
      }
    }
  }

  return totals;
}

export function mostDownloadedAsset(products: Product[], downloads: AssetCounts): string | null {
  if (products.length === 0 || Object.keys(downloads).length === 0) return null;
  return products.reduce((best, product) =>
    (downloads[product.asset] ?? 0) > (downloads[best.asset] ?? 0) ? product : best,
  ).asset;
}
