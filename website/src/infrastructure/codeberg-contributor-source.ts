import type { Contributor, ContributorSource } from "@/domain/ports";
import type { CodebergApi } from "@/infrastructure/codeberg-api";

interface CodebergCommit {
  author: { login: string; avatar_url: string; html_url: string } | null;
}

export class CodebergContributorSource implements ContributorSource {
  constructor(private readonly api: CodebergApi) {}

  async contributors(repo: string): Promise<Contributor[]> {
    const commits = await this.api.repoPages<CodebergCommit>(repo, "/commits?limit=50&stat=false&verification=false&files=false");
    const byLogin = new Map<string, Contributor & { commits: number }>();

    for (const { author } of commits ?? []) {
      if (!author) continue;
      const known = byLogin.get(author.login) ?? { login: author.login, avatarUrl: author.avatar_url, htmlUrl: author.html_url, commits: 0 };
      known.commits++;
      byLogin.set(author.login, known);
    }

    return [...byLogin.values()].sort((a, b) => b.commits - a.commits).map(({ login, avatarUrl, htmlUrl }) => ({ login, avatarUrl, htmlUrl }));
  }
}
