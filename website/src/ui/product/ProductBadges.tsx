import type { ReactNode } from "react";
import { Icon } from "@/ui/components/Icon";

const BADGE =
  "inline-flex items-center gap-1 self-start border backdrop-blur-sm px-[0.55rem] py-[0.2rem] font-body text-[0.7rem] font-semibold uppercase tracking-[0.08em] rounded-[3px] whitespace-nowrap animate-in fade-in-0 zoom-in-95 duration-300";

const TONES = {
  neutral: "border-border/60 bg-background/80 text-muted-foreground",
  popular: "border-glow-amber/50 bg-glow-amber/15 text-glow-amber",
  recommended: "border-glow-teal/40 bg-glow-teal/10 text-glow-teal",
} as const;

function Badge({ tone, children }: { tone: keyof typeof TONES; children: ReactNode }) {
  return <span className={`${BADGE} ${TONES[tone]}`}>{children}</span>;
}

function SkeletonBadge({ className }: { className: string }) {
  return <span className={`h-[1.45rem] rounded-[3px] border border-border/50 bg-muted/40 animate-pulse ${className}`} />;
}

export interface ProductStats {
  downloads?: number;
  version?: string;
  mostPopular: boolean;
  loading: boolean;
}

export function ProductBadges({ stats, recommended }: { stats: ProductStats; recommended: boolean }) {
  return (
    <div className="flex flex-wrap items-center gap-2">
      {stats.loading ? (
        <>
          <SkeletonBadge className="w-16" />
          <SkeletonBadge className="w-28" />
        </>
      ) : (
        <>
          {stats.downloads != null && (
            <Badge tone="neutral">
              <Icon name="download" />
              {stats.downloads.toLocaleString()}
            </Badge>
          )}
          {stats.version && <Badge tone="neutral">{stats.version}</Badge>}
          {stats.mostPopular && (
            <Badge tone="popular">
              <Icon name="star" />
              Most popular
            </Badge>
          )}
        </>
      )}
      {recommended && (
        <Badge tone="recommended">
          <Icon name="check" />
          Recommended
        </Badge>
      )}
    </div>
  );
}
