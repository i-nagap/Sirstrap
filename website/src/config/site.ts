export const ACCOUNTS: readonly string[] = ["Sirstrap"];
export const OWNER = ACCOUNTS[0];
export const MAIN_REPO = `${OWNER}/Sirstrap`;
export const REPO_URL = `https://codeberg.org/${MAIN_REPO}`;
export const CODEBERG_API = "https://codeberg.org/api/v1";
export const RELEASES_URL = "https://sirstrap.com/releases/";
export const AUTHOR_NAME = "ギャップ";
export const announcementUrl = (account: string) => `https://codeberg.org/${account}/Sirstrap/raw/branch/main/announcements.txt`;
