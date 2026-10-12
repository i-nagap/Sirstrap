import { repoOf, unique, type Product } from "@/domain/product";
import type { Contributor, ContributorSource } from "@/domain/ports";

export type ContributorsByRepo = Record<string, Contributor[]>;

export async function loadContributors(products: Product[], source: ContributorSource): Promise<ContributorsByRepo> {
  const repos = unique(products.map(repoOf));
  return Object.fromEntries(await Promise.all(repos.map(async repo => [repo, await source.contributors(repo)] as const)));
}
