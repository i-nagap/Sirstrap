import type { Accounts } from "@/domain/accounts";
import type { AccountProbe } from "@/domain/ports";
import type { CodebergApi } from "@/infrastructure/codeberg-api";

export class CodebergAccountProbe implements AccountProbe {
  private resolved: Promise<string> | null = null;

  constructor(
    private readonly accounts: Accounts,
    private readonly api: CodebergApi,
  ) {}

  reachable = (): Promise<string> => {
    this.resolved ??= this.accounts
      .firstResolved(async account => ((await this.api.userExists(account)) ? account : null))
      .then(account => account ?? this.accounts.primary);

    return this.resolved;
  };
}
