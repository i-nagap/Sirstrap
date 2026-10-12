export type Accent = "red" | "teal" | "amber" | "mint" | "orange";

export interface AccentStyle {
  line: string;
  text: string;
  border: string;
  groupText: string;
  groupBorder: string;
  wave: readonly [hue: number, saturation: number, lightness: number];
}

export const ACCENTS: Record<Accent, AccentStyle> = {
  red: {
    line: "bg-glow-red",
    text: "text-glow-red",
    border: "border-glow-red/40 hover:border-glow-red",
    groupText: "group-hover:text-glow-red",
    groupBorder: "group-hover:border-glow-red group-hover:shadow-[0_0_16px_-4px_hsl(var(--glow-red))]",
    wave: [5, 81, 48],
  },
  teal: {
    line: "bg-glow-teal",
    text: "text-glow-teal",
    border: "border-glow-teal/40 hover:border-glow-teal",
    groupText: "group-hover:text-glow-teal",
    groupBorder: "group-hover:border-glow-teal group-hover:shadow-[0_0_16px_-4px_hsl(var(--glow-teal))]",
    wave: [181, 80, 44],
  },
  amber: {
    line: "bg-glow-amber",
    text: "text-glow-amber",
    border: "border-glow-amber/40 hover:border-glow-amber",
    groupText: "group-hover:text-glow-amber",
    groupBorder: "group-hover:border-glow-amber group-hover:shadow-[0_0_16px_-4px_hsl(var(--glow-amber))]",
    wave: [39, 100, 52],
  },
  mint: {
    line: "bg-glow-mint",
    text: "text-glow-mint",
    border: "border-glow-mint/40 hover:border-glow-mint",
    groupText: "group-hover:text-glow-mint",
    groupBorder: "group-hover:border-glow-mint group-hover:shadow-[0_0_16px_-4px_hsl(var(--glow-mint))]",
    wave: [160, 45, 72],
  },
  orange: {
    line: "bg-glow-orange",
    text: "text-glow-orange",
    border: "border-glow-orange/40 hover:border-glow-orange",
    groupText: "group-hover:text-glow-orange",
    groupBorder: "group-hover:border-glow-orange group-hover:shadow-[0_0_16px_-4px_hsl(var(--glow-orange))]",
    wave: [30, 96, 50],
  },
};
