import type { Config } from "tailwindcss";

export default {
  content: ["./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      fontFamily: {
        display: ['"Syne"', "sans-serif"],
        body: ['"DM Sans"', "sans-serif"],
      },
      colors: {
        border: "hsl(var(--border))",
        background: "hsl(var(--background))",
        foreground: "hsl(var(--foreground))",
        primary: "hsl(var(--primary))",
        muted: {
          DEFAULT: "hsl(var(--muted))",
          foreground: "hsl(var(--muted-foreground))",
        },
        glow: {
          red: "hsl(var(--glow-red))",
          teal: "hsl(var(--glow-teal))",
          amber: "hsl(var(--glow-amber))",
          mint: "hsl(var(--glow-mint))",
          orange: "hsl(var(--glow-orange))",
        },
      },
      borderRadius: {
        lg: "var(--radius)",
      },
    },
  },
} satisfies Config;
