import { ACCOUNTS, GITHUB_API_BASE, OWNER } from "@/config/site.config";

const ACCOUNT_PATTERN = new RegExp(`(?<=^|[/@.])(${ACCOUNTS.join("|")})(?=$|[/.])`, "g");

export const repoForAccount = (repo: string, account: string) => `${account}/${repo.slice(repo.indexOf("/") + 1)}`;

export const withAccount = (value: string, account: string) => value.replace(ACCOUNT_PATTERN, account);

export async function resolveAcrossAccounts<T>(attempt: (account: string) => Promise<T | null | undefined>): Promise<T | null> {
  for (const account of ACCOUNTS) {
    try {
      const result = await attempt(account);
      if (result !== null && result !== undefined) return result;
    } catch {
      continue;
    }
  }

  return null;
}

let reachable: Promise<string> | null = null;

export function reachableAccount(): Promise<string> {
  reachable ??= resolveAcrossAccounts(async account => {
    const response = await fetch(`${GITHUB_API_BASE}/users/${account}`);
    return response.ok ? account : null;
  }).then(account => account ?? OWNER);

  return reachable;
}
