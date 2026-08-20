export const ACCOUNTS = ["massimopaganigh", "i-nagap"];
export const OWNER = ACCOUNTS[0];
export const MAIN_REPO = `${OWNER}/Sirstrap`;
export const REPO = `https://github.com/${MAIN_REPO}`;
export const announcementUrlFor = (account: string) => `https://raw.githubusercontent.com/${account}/Sirstrap/main/announcements.txt`;
export const ANNOUNCEMENT_URL = announcementUrlFor(OWNER);
export const GITHUB_API_BASE = "https://api.github.com";
