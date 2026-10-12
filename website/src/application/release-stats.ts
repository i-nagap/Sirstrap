import { aggregateDownloads, countAssets, type AssetCounts } from "@/domain/downloads";
import type { Product } from "@/domain/product";
import type { ReleaseSource } from "@/domain/ports";

export interface ReleaseStats {
  version?: string;
  downloads: AssetCounts;
}

export const EMPTY_RELEASE_STATS: ReleaseStats = { downloads: {} };

export async function loadReleaseStats(products: Product[], source: ReleaseSource): Promise<ReleaseStats> {
  const releases = await source.releases();

  return {
    version: releases[0]?.tag,
    downloads: aggregateDownloads(products, countAssets(releases)),
  };
}
