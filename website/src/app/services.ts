import { createContext } from "react";
import { ACCOUNTS, CODEBERG_API, RELEASES_URL, announcementUrl } from "@/config/site";
import { Accounts } from "@/domain/accounts";
import type { AccountProbe, AnnouncementSource, ContributorSource, ReleaseSource } from "@/domain/ports";
import { CodebergApi } from "@/infrastructure/codeberg-api";
import { CodebergAccountProbe } from "@/infrastructure/codeberg-account-probe";
import { CodebergContributorSource } from "@/infrastructure/codeberg-contributor-source";
import { SirstrapReleaseSource } from "@/infrastructure/sirstrap-release-source";
import { RemoteAnnouncementSource } from "@/infrastructure/remote-announcement-source";

export interface Services {
  accounts: Accounts;
  releases: ReleaseSource;
  contributors: ContributorSource;
  announcements: AnnouncementSource;
  accountProbe: AccountProbe;
}

export function createServices(): Services {
  const accounts = new Accounts(ACCOUNTS);
  const api = new CodebergApi(accounts, CODEBERG_API);

  return {
    accounts,
    releases: new SirstrapReleaseSource(RELEASES_URL),
    contributors: new CodebergContributorSource(api),
    announcements: new RemoteAnnouncementSource(accounts, announcementUrl),
    accountProbe: new CodebergAccountProbe(accounts, api),
  };
}

export const ServicesContext = createContext<Services | null>(null);
