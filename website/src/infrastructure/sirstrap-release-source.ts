import type { Release, ReleaseSource } from "@/domain/ports";
import { browserFetch, type Fetch } from "@/infrastructure/codeberg-api";

interface SirstrapRelease {
  tagName: string;
  isDraft: boolean;
  assets: { fileName: string; downloadCount: number }[];
}

export class SirstrapReleaseSource implements ReleaseSource {
  constructor(
    private readonly url: string,
    private readonly fetcher: Fetch = browserFetch,
  ) {}

  async releases(): Promise<Release[]> {
    const response = await this.fetcher(this.url);
    if (!response.ok) return [];

    return ((await response.json()) as SirstrapRelease[])
      .filter(release => !release.isDraft)
      .map(release => ({
        tag: release.tagName,
        assets: release.assets.map(asset => ({ name: asset.fileName, downloadCount: asset.downloadCount })),
      }));
  }
}
