import { useAsync } from "@/hooks/useAsync";
import { useServices } from "@/hooks/useServices";

export function useReachableAccount(): string {
  const { accountProbe, accounts } = useServices();
  return useAsync(accountProbe.reachable, accounts.primary).value;
}
