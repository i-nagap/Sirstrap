import { createContext } from "react";
import { ACCOUNTS, GITHUB_API, announcementUrl } from "@/config/site";
import { GithubAccounts } from "@/domain/accounts";
import type { AccountProbe, AnnouncementSource, ContributorSource, ReleaseSource } from "@/domain/ports";
import { GithubApi } from "@/infrastructure/github-api";
import { GithubAccountProbe } from "@/infrastructure/github-account-probe";
import { GithubContributorSource } from "@/infrastructure/github-contributor-source";
import { GithubReleaseSource } from "@/infrastructure/github-release-source";
import { RemoteAnnouncementSource } from "@/infrastructure/remote-announcement-source";

export interface Services {
  accounts: GithubAccounts;
  releases: ReleaseSource;
  contributors: ContributorSource;
  announcements: AnnouncementSource;
  accountProbe: AccountProbe;
}

export function createServices(): Services {
  const accounts = new GithubAccounts(ACCOUNTS);
  const api = new GithubApi(accounts, GITHUB_API);

  return {
    accounts,
    releases: new GithubReleaseSource(api),
    contributors: new GithubContributorSource(api),
    announcements: new RemoteAnnouncementSource(accounts, announcementUrl),
    accountProbe: new GithubAccountProbe(accounts, api),
  };
}

export const ServicesContext = createContext<Services | null>(null);
