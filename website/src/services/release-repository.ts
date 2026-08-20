import { GITHUB_API_BASE } from "@/config/site.config";
import { repoForAccount, resolveAcrossAccounts } from "@/services/github-accounts";

export interface ReleaseAsset {
  name: string;
  download_count: number;
}

export interface Release {
  assets?: ReleaseAsset[];
}

export interface ReleaseRepository {
  fetchLatestTag(repo: string): Promise<string | undefined>;
  fetchAllReleases(repo: string): Promise<Release[]>;
}

const NEXT_PAGE = /<([^>]+)>;\s*rel="next"/;

export class GithubReleaseRepository implements ReleaseRepository {
  constructor(private readonly apiBase: string = GITHUB_API_BASE) {}

  async fetchLatestTag(repo: string): Promise<string | undefined> {
    const tag = await resolveAcrossAccounts(async account => {
      const response = await fetch(`${this.apiBase}/repos/${repoForAccount(repo, account)}/releases/latest`);
      if (!response.ok) return null;
      const json = await response.json();
      return (json.tag_name as string | undefined) ?? null;
    });

    return tag ?? undefined;
  }

  async fetchAllReleases(repo: string): Promise<Release[]> {
    const releases = await resolveAcrossAccounts(async account => {
      const collected: Release[] = [];
      let url: string | null = `${this.apiBase}/repos/${repoForAccount(repo, account)}/releases?per_page=100`;

      while (url) {
        const response: Response = await fetch(url);
        if (!response.ok) return null;
        const page = await response.json();
        if (Array.isArray(page)) collected.push(...page);
        const next = (response.headers.get("Link") ?? "").match(NEXT_PAGE);
        url = next ? next[1] : null;
      }

      return collected.length > 0 ? collected : null;
    });

    return releases ?? [];
  }
}

export const githubReleaseRepository: ReleaseRepository = new GithubReleaseRepository();
