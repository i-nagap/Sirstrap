export class Accounts {
  private readonly pattern: RegExp;
  private readonly lowered: string[];

  constructor(readonly all: readonly string[]) {
    this.pattern = new RegExp(`(?<=^|codeberg\\.org/)(${all.join("|")})(?=$|[/.])`, "g");
    this.lowered = all.map(account => account.toLowerCase());
  }

  get primary(): string {
    return this.all[0];
  }

  owns(login: string): boolean {
    return this.lowered.includes(login.toLowerCase());
  }

  withAccount(value: string, account: string): string {
    return value.replace(this.pattern, account);
  }

  repoFor(repo: string, account: string): string {
    return `${account}/${repo.slice(repo.indexOf("/") + 1)}`;
  }

  async firstResolved<T>(attempt: (account: string) => Promise<T | null | undefined>): Promise<T | null> {
    for (const account of this.all) {
      try {
        const result = await attempt(account);
        if (result != null) return result;
      } catch {
        continue;
      }
    }

    return null;
  }
}
