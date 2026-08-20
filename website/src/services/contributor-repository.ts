import { GITHUB_API_BASE } from "@/config/site.config";
import { repoForAccount, resolveAcrossAccounts } from "@/services/github-accounts";

export interface Contributor {
  login: string;
  avatarUrl: string;
  htmlUrl: string;
}

export interface ContributorRepository {
  fetchContributors(repo: string): Promise<Contributor[]>;
}

interface GithubContributor {
  login: string;
  avatar_url: string;
  html_url: string;
  type: string;
}

export class GithubContributorRepository implements ContributorRepository {
  constructor(private readonly apiBase: string = GITHUB_API_BASE) {}

  async fetchContributors(repo: string): Promise<Contributor[]> {
    const contributors = await resolveAcrossAccounts(async account => {
      const response = await fetch(`${this.apiBase}/repos/${repoForAccount(repo, account)}/contributors?per_page=100`);
      if (!response.ok) return null;
      const json = await response.json();
      if (!Array.isArray(json)) return null;

      const mapped = (json as GithubContributor[])
        .filter(contributor => contributor.type === "User" && !contributor.login.toLowerCase().endsWith("[bot]"))
        .map(contributor => ({
          login: contributor.login,
          avatarUrl: contributor.avatar_url,
          htmlUrl: contributor.html_url,
        }));

      return mapped.length > 0 ? mapped : null;
    });

    return contributors ?? [];
  }
}

export const githubContributorRepository: ContributorRepository = new GithubContributorRepository();
