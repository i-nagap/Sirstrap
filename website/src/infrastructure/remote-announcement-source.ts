import type { Accounts } from "@/domain/accounts";
import type { AnnouncementSource } from "@/domain/ports";
import { browserFetch, type Fetch } from "@/infrastructure/codeberg-api";

export class RemoteAnnouncementSource implements AnnouncementSource {
  constructor(
    private readonly accounts: Accounts,
    private readonly urlFor: (account: string) => string,
    private readonly fetcher: Fetch = browserFetch,
  ) {}

  current(): Promise<string | null> {
    return this.accounts.firstResolved(async account => {
      const response = await this.fetcher(this.urlFor(account));
      if (!response.ok) return null;
      return (await response.text()).trim() || null;
    });
  }
}
