import { useEffect, useState } from "react";
import { OWNER } from "@/config/site.config";
import { reachableAccount } from "@/services/github-accounts";

export function useReachableAccount(): string {
  const [account, setAccount] = useState(OWNER);

  useEffect(() => {
    let cancelled = false;

    reachableAccount().then(resolved => {
      if (!cancelled) setAccount(resolved);
    });

    return () => {
      cancelled = true;
    };
  }, []);

  return account;
}
