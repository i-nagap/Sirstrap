import type { GithubAccounts } from "@/domain/accounts";
import type { AccountProbe } from "@/domain/ports";
import type { GithubApi } from "@/infrastructure/github-api";

export class GithubAccountProbe implements AccountProbe {
  private resolved: Promise<string> | null = null;

  constructor(
    private readonly accounts: GithubAccounts,
    private readonly api: GithubApi,
  ) {}

  reachable = (): Promise<string> => {
    this.resolved ??= this.accounts
      .firstResolved(async account => ((await this.api.userExists(account)) ? account : null))
      .then(account => account ?? this.accounts.primary);

    return this.resolved;
  };
}
