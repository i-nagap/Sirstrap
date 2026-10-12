import { Icon, type IconName } from "@/ui/components/Icon";

const ACTION =
  "inline-flex items-center gap-2 border px-6 py-3 font-body text-[0.82rem] font-semibold tracking-[0.03em] rounded-[5px] transition-all duration-300 whitespace-nowrap";

export interface ProductLinks {
  download: string;
  source: string;
}

function ExternalAction({ href, icon, label, className }: { href: string; icon: IconName; label: string; className: string }) {
  return (
    <a href={href} target="_blank" rel="noopener noreferrer" className={`${ACTION} ${className}`}>
      <Icon name={icon} />
      <span>{label}</span>
    </a>
  );
}

export function ProductActions({ links, accentBorder }: { links: ProductLinks; accentBorder: string }) {
  return (
    <>
      <ExternalAction href={links.download} icon="download" label="Download" className={`${accentBorder} text-foreground`} />
      <ExternalAction
        href={links.source}
        icon="source"
        label="Source"
        className="border-border/40 hover:border-border text-muted-foreground hover:text-foreground"
      />
    </>
  );
}
