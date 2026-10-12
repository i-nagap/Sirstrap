import { useState } from "react";

export interface PanelLayout {
  isMobile: boolean;
  active: boolean;
  dimmed: boolean;
  flex: number;
  opacity: number;
}

export interface PanelHandlers {
  onEnter: () => void;
  onLeave: () => void;
  onClick: () => void;
}

export interface PanelFocus {
  hasActive: boolean;
  layoutOf(index: number): PanelLayout;
  handlersOf(index: number): PanelHandlers;
}

export function usePanelFocus(isMobile: boolean): PanelFocus {
  const [active, setActive] = useState<number | null>(null);
  const hasActive = active !== null;

  return {
    hasActive,
    layoutOf: index => ({
      isMobile,
      active: active === index,
      dimmed: !isMobile && hasActive && active !== index,
      flex: !hasActive ? 1 : active === index ? 2 : 0.5,
      opacity: !hasActive ? 0.7 : active === index ? 1 : 0.5,
    }),
    handlersOf: index => ({
      onEnter: () => {
        if (!isMobile) setActive(index);
      },
      onLeave: () => {
        if (!isMobile) setActive(null);
      },
      onClick: () => {
        if (isMobile) setActive(previous => (previous === index ? null : index));
      },
    }),
  };
}
