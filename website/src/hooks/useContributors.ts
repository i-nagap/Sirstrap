import { useCallback } from "react";
import { loadContributors, type ContributorsByRepo } from "@/application/contributors";
import type { Product } from "@/domain/product";
import { useAsync, type AsyncState } from "@/hooks/useAsync";
import { useServices } from "@/hooks/useServices";

const NONE: ContributorsByRepo = {};

export function useContributors(products: Product[]): AsyncState<ContributorsByRepo> {
  const { contributors } = useServices();
  const load = useCallback(() => loadContributors(products, contributors), [products, contributors]);
  return useAsync(load, NONE);
}
