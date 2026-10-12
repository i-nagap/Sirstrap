import { useCallback } from "react";
import { EMPTY_RELEASE_STATS, loadReleaseStats, type ReleaseStats } from "@/application/release-stats";
import type { Product } from "@/domain/product";
import { useAsync, type AsyncState } from "@/hooks/useAsync";
import { useServices } from "@/hooks/useServices";

export function useReleaseStats(products: Product[]): AsyncState<ReleaseStats> {
  const { releases } = useServices();
  const load = useCallback(() => loadReleaseStats(products, releases), [products, releases]);
  return useAsync(load, EMPTY_RELEASE_STATS);
}
