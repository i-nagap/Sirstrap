import { useSyncExternalStore } from "react";

const QUERY = "(max-width: 767px)";

const subscribe = (onChange: () => void) => {
  const media = window.matchMedia(QUERY);
  media.addEventListener("change", onChange);
  return () => media.removeEventListener("change", onChange);
};

const snapshot = () => window.matchMedia(QUERY).matches;

export function useIsMobile(): boolean {
  return useSyncExternalStore(subscribe, snapshot, () => false);
}
