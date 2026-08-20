import { ACCOUNTS } from "@/config/site.config";

export const repoForAccount = (repo: string, account: string) => `${account}/${repo.slice(repo.indexOf("/") + 1)}`;

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
