import { transition } from "@/config/motion";
import { ACCENTS } from "@/domain/accent";
import type { Product } from "@/domain/product";
import type { PanelHandlers, PanelLayout } from "@/hooks/usePanelFocus";
import { CORE_LABEL } from "@/ui/components/CoreBracket";
import { ProductActions, type ProductLinks } from "@/ui/product/ProductActions";
import { ProductBadges, type ProductStats } from "@/ui/product/ProductBadges";
import { ProductByline, type ProductCredits } from "@/ui/product/ProductByline";
import { ProductScreenshots } from "@/ui/product/ProductScreenshots";
import { WavyBackground } from "@/ui/product/WavyBackground";
import { ProductTitle } from "@/ui/title/ProductTitle";

const PANEL_BACKGROUND = "hsl(var(--background))";

interface ProductPanelProps {
  product: Product;
  index: number;
  layout: PanelLayout;
  handlers: PanelHandlers;
  stats: ProductStats;
  credits: ProductCredits;
  links: ProductLinks;
}

export function ProductPanel({ product, index, layout, handlers, stats, credits, links }: ProductPanelProps) {
  const accent = ACCENTS[product.accent];
  const { isMobile, active, dimmed } = layout;

  return (
    <section
      onMouseEnter={handlers.onEnter}
      onMouseLeave={handlers.onLeave}
      onClick={handlers.onClick}
      style={{
        flex: isMobile ? undefined : layout.flex,
        opacity: isMobile ? 1 : layout.opacity,
        transition: transition(["flex", "opacity"]),
      }}
      className="relative flex flex-col min-h-[70vh] md:min-h-0 border-b border-border last:border-b-0 md:border-b-0 md:border-r md:last:border-r-0 bg-background px-8 py-12 lg:px-12 overflow-hidden"
    >
      <div className={`absolute inset-x-0 top-0 h-[2px] ${accent.line} opacity-60`} />

      <WavyBackground wave={accent.wave} active={active} />

      {!isMobile && (
        <div
          className="pointer-events-none absolute inset-y-0 right-0 w-16 z-10"
          style={{
            backgroundImage: `linear-gradient(to right, transparent, ${PANEL_BACKGROUND})`,
            opacity: dimmed ? 1 : 0,
            transition: transition(["opacity"]),
          }}
        />
      )}

      <div className="relative flex-1 min-h-0 overflow-hidden min-w-0 flex flex-col justify-center">
        <div
          className="min-w-0 flex flex-col gap-4 animate-in fade-in-0 slide-in-from-bottom-3 duration-500"
          style={{ animationDelay: `${index * 80}ms`, animationFillMode: "both" }}
        >
          <span className="block font-body text-xs font-medium uppercase tracking-[0.12em] text-muted-foreground whitespace-nowrap">
            {String(index + 1).padStart(2, "0")}
          </span>

          {isMobile && product.core && (
            <span className="-mt-2 inline-flex w-fit items-center gap-2 font-body text-[0.65rem] font-medium uppercase tracking-[0.16em] text-muted-foreground">
              <span className="h-2 w-3 border-l border-t border-border/70" />
              {CORE_LABEL}
            </span>
          )}

          <div className="grid gap-x-3" style={{ gridTemplateColumns: "auto minmax(0, 1fr)" }}>
            <img src={product.icon} alt="" className="h-8 w-8 lg:h-9 lg:w-9 shrink-0 row-span-2 self-center" />
            <ProductTitle animation={product.title} active={active} accentClassName={accent.text} />
            <ProductByline credits={credits} accent={accent} />
          </div>

          <ProductBadges stats={stats} recommended={product.recommended} />

          {product.description && (
            <p className="font-body text-[0.85rem] leading-[1.55] text-muted-foreground max-w-xs">{product.description}</p>
          )}

          {product.screenshots && <ProductScreenshots screenshots={product.screenshots} accent={product.accent} active={active} />}
        </div>
      </div>

      <div
        className="flex-shrink-0 flex flex-col gap-4 overflow-hidden"
        style={{
          maxHeight: active ? "200px" : "0",
          marginTop: active ? "2rem" : "0",
          opacity: active ? 1 : 0,
          transition: transition(["max-height", "margin-top", "opacity"]),
          pointerEvents: active ? "auto" : "none",
        }}
      >
        <ProductActions links={links} accentBorder={accent.border} />
      </div>
    </section>
  );
}
