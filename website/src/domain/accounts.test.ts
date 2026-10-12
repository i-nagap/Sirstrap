import { describe, expect, it } from "vitest";
import { GithubAccounts } from "@/domain/accounts";

const accounts = new GithubAccounts(["first", "second"]);

describe("GithubAccounts", () => {
  it("rewrites every account occurrence in a url", () => {
    expect(accounts.withAccount("https://github.com/first/Sirstrap/tree/main/src", "second")).toBe("https://github.com/second/Sirstrap/tree/main/src");
    expect(accounts.withAccount("https://github.com/first.png", "second")).toBe("https://github.com/second.png");
    expect(accounts.withAccount("first/sirhurt.cleaner", "second")).toBe("second/sirhurt.cleaner");
    expect(accounts.withAccount("https://github.com/firstborn/x", "second")).toBe("https://github.com/firstborn/x");
  });

  it("keeps the repository name when swapping the account", () => {
    expect(accounts.repoFor("first/KneeSurgery", "second")).toBe("second/KneeSurgery");
  });

  it("recognizes owned logins case-insensitively", () => {
    expect(accounts.owns("FIRST")).toBe(true);
    expect(accounts.owns("someone")).toBe(false);
  });

  it("falls back to the next account when one throws or resolves to nothing", async () => {
    const tried: string[] = [];

    const resolved = await accounts.firstResolved(async account => {
      tried.push(account);
      if (account === "first") throw new Error("not found");
      return account;
    });

    expect(tried).toEqual(["first", "second"]);
    expect(resolved).toBe("second");
    expect(await accounts.firstResolved(async account => (account === "first" ? null : account))).toBe("second");
  });

  it("returns null when every account fails", async () => {
    expect(await accounts.firstResolved(async () => null)).toBeNull();
  });
});
