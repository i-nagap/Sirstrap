import { MAIN_REPO, REPO_URL } from "@/config/site";
import type { Accent } from "@/domain/accent";

export type AssetMatcher = { type: "exact"; asset: string } | { type: "prefix"; prefix: string };

export interface ExternalDownload {
  repo: string;
  match: AssetMatcher;
}

export type TitleAnimation =
  | { kind: "plain"; head: string }
  | { kind: "typewriter"; head: string; tail: string }
  | { kind: "shimmer"; head: string; tail: string }
  | { kind: "rapid"; head: string; tail: string; glyph: string; max: number };

export interface Screenshot {
  src: string;
  alt: string;
}

export interface Product {
  name: string;
  description: string;
  asset: string;
  variants: string[];
  externalDownloads: ExternalDownload[];
  recommended: boolean;
  icon: string;
  source: string;
  accent: Accent;
  title: TitleAnimation;
  core?: boolean;
  repo?: string;
  downloadUrl?: string;
  screenshots?: Screenshot[];
}

export const repoOf = (product: Product) => product.repo ?? MAIN_REPO;

export const downloadUrlOf = (product: Product) =>
  product.downloadUrl ?? `${REPO_URL}/releases/latest/download/${product.asset}`;

export const unique = <T,>(values: T[]): T[] => [...new Set(values)];

export function leadingCoreCount(products: Product[]): number {
  const index = products.findIndex(product => !product.core);
  return index === -1 ? products.length : index;
}
