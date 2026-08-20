import { announcementUrlFor } from "@/config/site.config";
import { resolveAcrossAccounts } from "@/services/github-accounts";

export interface AnnouncementRepository {
  fetch(): Promise<string | null>;
}

export class RemoteAnnouncementRepository implements AnnouncementRepository {
  constructor(private readonly urlFor: (account: string) => string = announcementUrlFor) {}

  async fetch(): Promise<string | null> {
    return resolveAcrossAccounts(async account => {
      const response = await fetch(this.urlFor(account));
      if (!response.ok) return null;
      const text = (await response.text()).trim();
      return text ? text : null;
    });
  }
}

export const announcementRepository: AnnouncementRepository = new RemoteAnnouncementRepository();
