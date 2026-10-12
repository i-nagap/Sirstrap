import { products } from "@/data/products";
import { mostDownloadedAsset } from "@/domain/downloads";
import { downloadUrlOf, leadingCoreCount, repoOf } from "@/domain/product";
import { useAnnouncement } from "@/hooks/useAnnouncement";
import { useContributors } from "@/hooks/useContributors";
import { useIsMobile } from "@/hooks/useIsMobile";
import { usePanelFocus } from "@/hooks/usePanelFocus";
import { useReachableAccount } from "@/hooks/useReachableAccount";
import { useReleaseStats } from "@/hooks/useReleaseStats";
import { useServices } from "@/hooks/useServices";
import { AnnouncementBanner } from "@/ui/components/AnnouncementBanner";
import { CoreBracket } from "@/ui/components/CoreBracket";
import { ProductPanel } from "@/ui/product/ProductPanel";

const CORE_COUNT = leadingCoreCount(products);

export function HomePage() {
  const { accounts } = useServices();
  const isMobile = useIsMobile();
  const focus = usePanelFocus(isMobile);
  const announcement = useAnnouncement();
  const author = useReachableAccount();
  const releases = useReleaseStats(products);
  const contributors = useContributors(products);
  const mostPopular = mostDownloadedAsset(products, releases.value.downloads);

  return (
    <div className="relative flex flex-col md:flex-row min-h-screen md:h-screen w-screen overflow-y-auto md:overflow-hidden">
      {announcement && <AnnouncementBanner message={announcement} />}
      <CoreBracket count={CORE_COUNT} total={products.length} dimmed={focus.hasActive} />
      {products.map((product, index) => {
        const repo = repoOf(product);

        return (
          <ProductPanel
            key={product.name}
            product={product}
            index={index}
            layout={focus.layoutOf(index)}
            handlers={focus.handlersOf(index)}
            stats={{
              downloads: releases.value.downloads[product.asset],
              version: releases.value.versions[repo],
              mostPopular: mostPopular === product.asset,
              loading: releases.loading,
            }}
            credits={{ author, contributors: contributors.value[repo] ?? [], loading: contributors.loading }}
            links={{
              download: accounts.withAccount(downloadUrlOf(product), author),
              source: accounts.withAccount(product.source, author),
            }}
          />
        );
      })}
    </div>
  );
}
