export interface ReleaseAsset {
  name: string;
  downloadCount: number;
}

export interface Release {
  assets: ReleaseAsset[];
}

export interface Contributor {
  login: string;
  avatarUrl: string;
  htmlUrl: string;
}

export interface ReleaseSource {
  latestTag(repo: string): Promise<string | null>;
  allReleases(repo: string): Promise<Release[]>;
}

export interface ContributorSource {
  contributors(repo: string): Promise<Contributor[]>;
}

export interface AnnouncementSource {
  current(): Promise<string | null>;
}

export interface AccountProbe {
  reachable(): Promise<string>;
}
