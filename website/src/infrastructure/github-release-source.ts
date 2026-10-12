import type { Release, ReleaseSource } from "@/domain/ports";
import type { GithubApi } from "@/infrastructure/github-api";

interface GithubRelease {
  tag_name?: string;
  assets?: { name: string; download_count: number }[];
}

export class GithubReleaseSource implements ReleaseSource {
  constructor(private readonly api: GithubApi) {}

  latestTag(repo: string): Promise<string | null> {
    return this.api.repoJson(repo, "/releases/latest", json => (json as GithubRelease).tag_name ?? null);
  }

  async allReleases(repo: string): Promise<Release[]> {
    const releases = await this.api.repoPages<GithubRelease>(repo, "/releases?per_page=100");

    return (releases ?? []).map(release => ({
      assets: (release.assets ?? []).map(asset => ({ name: asset.name, downloadCount: asset.download_count })),
    }));
  }
}
