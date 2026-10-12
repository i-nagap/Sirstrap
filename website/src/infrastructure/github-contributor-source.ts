import type { Contributor, ContributorSource } from "@/domain/ports";
import type { GithubApi } from "@/infrastructure/github-api";

interface GithubContributor {
  login: string;
  avatar_url: string;
  html_url: string;
  type: string;
}

const isHuman = (contributor: GithubContributor) =>
  contributor.type === "User" && !contributor.login.toLowerCase().endsWith("[bot]");

export class GithubContributorSource implements ContributorSource {
  constructor(private readonly api: GithubApi) {}

  async contributors(repo: string): Promise<Contributor[]> {
    const contributors = await this.api.repoJson(repo, "/contributors?per_page=100", json => {
      if (!Array.isArray(json)) return null;

      const humans = (json as GithubContributor[]).filter(isHuman).map(contributor => ({
        login: contributor.login,
        avatarUrl: contributor.avatar_url,
        htmlUrl: contributor.html_url,
      }));

      return humans.length > 0 ? humans : null;
    });

    return contributors ?? [];
  }
}
