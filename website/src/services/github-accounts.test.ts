import { describe, it, expect } from "vitest";
import { ACCOUNTS } from "@/config/site.config";
import { repoForAccount, resolveAcrossAccounts, withAccount } from "@/services/github-accounts";

const [first, second] = ACCOUNTS;

describe("github-accounts", () => {
  it("rewrites every account occurrence in a url", () => {
    expect(withAccount(`https://github.com/${first}/Sirstrap/tree/main/src`, second)).toBe(`https://github.com/${second}/Sirstrap/tree/main/src`);
    expect(withAccount(`https://github.com/${first}.png`, second)).toBe(`https://github.com/${second}.png`);
    expect(withAccount(`${first}/sirhurt.cleaner`, second)).toBe(`${second}/sirhurt.cleaner`);
  });

  it("keeps the repository name when swapping the account", () => {
    expect(repoForAccount(`${first}/KneeSurgery`, second)).toBe(`${second}/KneeSurgery`);
  });

  it("falls back to the next account when the first one fails", async () => {
    const tried: string[] = [];

    const resolved = await resolveAcrossAccounts(async account => {
      tried.push(account);
      if (account === first) throw new Error("not found");
      return account;
    });

    expect(tried).toEqual([first, second]);
    expect(resolved).toBe(second);
  });

  it("falls back when an account resolves to nothing", async () => {
    const resolved = await resolveAcrossAccounts(async account => (account === first ? null : account));

    expect(resolved).toBe(second);
  });

  it("returns null when every account fails", async () => {
    expect(await resolveAcrossAccounts(async () => null)).toBeNull();
  });
});
