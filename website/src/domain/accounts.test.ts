import { describe, expect, it } from "vitest";
import { Accounts } from "@/domain/accounts";

const accounts = new Accounts(["first", "second"]);

describe("Accounts", () => {
  it("rewrites the owner in a url", () => {
    expect(accounts.withAccount("https://codeberg.org/first/Sirstrap/tree/main/src", "second")).toBe("https://codeberg.org/second/Sirstrap/tree/main/src");
    expect(accounts.withAccount("https://codeberg.org/first.png", "second")).toBe("https://codeberg.org/second.png");
    expect(accounts.withAccount("first/sirhurt.cleaner", "second")).toBe("second/sirhurt.cleaner");
    expect(accounts.withAccount("https://codeberg.org/firstborn/x", "second")).toBe("https://codeberg.org/firstborn/x");
    expect(accounts.withAccount("https://codeberg.org/first/first/src/branch/main/src/first.CLI", "second")).toBe("https://codeberg.org/second/first/src/branch/main/src/first.CLI");
  });

  it("keeps the repository name when swapping the account", () => {
    expect(accounts.repoFor("first/Sirstrap", "second")).toBe("second/Sirstrap");
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
