import { MAIN_REPO } from "@/config/site";
import { aggregateDownloads, countAssets, type AssetCounts } from "@/domain/downloads";
import { repoOf, unique, type Product } from "@/domain/product";
import type { ReleaseSource } from "@/domain/ports";

export interface ReleaseStats {
  versions: Record<string, string>;
  downloads: AssetCounts;
}

export const EMPTY_RELEASE_STATS: ReleaseStats = { versions: {}, downloads: {} };

export async function loadReleaseStats(products: Product[], source: ReleaseSource): Promise<ReleaseStats> {
  const productRepos = unique(products.map(repoOf));
  const externalRepos = unique(products.flatMap(product => product.externalDownloads.map(external => external.repo)));

  const [tags, mainReleases, externalReleases] = await Promise.all([
    Promise.all(productRepos.map(async repo => [repo, await source.latestTag(repo)] as const)),
    source.allReleases(MAIN_REPO),
    Promise.all(externalRepos.map(async repo => [repo, countAssets(await source.allReleases(repo))] as const)),
  ]);

  const versions: Record<string, string> = {};
  for (const [repo, tag] of tags) {
    if (tag) versions[repo] = tag;
  }

  return {
    versions,
    downloads: aggregateDownloads(products, countAssets(mainReleases), Object.fromEntries(externalReleases)),
  };
}
