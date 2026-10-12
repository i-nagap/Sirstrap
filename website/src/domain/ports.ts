export interface ReleaseAsset {
  name: string;
  downloadCount: number;
}

export interface Release {
  tag: string;
  assets: ReleaseAsset[];
}

export interface Contributor {
  login: string;
  avatarUrl: string;
  htmlUrl: string;
}

export interface ReleaseSource {
  releases(): Promise<Release[]>;
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
