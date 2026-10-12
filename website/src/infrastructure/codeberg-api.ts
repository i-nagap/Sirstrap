import type { Accounts } from "@/domain/accounts";

export type Fetch = (input: string) => Promise<Response>;

export const browserFetch: Fetch = input => fetch(input);

const NEXT_PAGE = /<([^>]+)>;\s*rel="next"/;

export class CodebergApi {
  constructor(
    private readonly accounts: Accounts,
    private readonly baseUrl: string,
    private readonly fetcher: Fetch = browserFetch,
  ) {}

  repoPages<T>(repo: string, path: string): Promise<T[] | null> {
    return this.accounts.firstResolved(async account => {
      const collected: T[] = [];
      let url: string | null = this.repoUrl(repo, account, path);

      while (url) {
        const response: Response = await this.fetcher(url);
        if (!response.ok) return null;
        const page: unknown = await response.json();
        if (Array.isArray(page)) collected.push(...page);
        url = (response.headers.get("Link") ?? "").match(NEXT_PAGE)?.[1] ?? null;
      }

      return collected.length > 0 ? collected : null;
    });
  }

  async userExists(account: string): Promise<boolean> {
    return (await this.fetcher(`${this.baseUrl}/users/${account}`)).ok;
  }

  private repoUrl(repo: string, account: string, path: string): string {
    return `${this.baseUrl}/repos/${this.accounts.repoFor(repo, account)}${path}`;
  }
}
