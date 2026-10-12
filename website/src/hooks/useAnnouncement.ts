import { useCallback } from "react";
import { useAsync } from "@/hooks/useAsync";
import { useServices } from "@/hooks/useServices";

export function useAnnouncement(): string | null {
  const { announcements } = useServices();
  const load = useCallback(() => announcements.current(), [announcements]);
  return useAsync(load, null).value;
}
