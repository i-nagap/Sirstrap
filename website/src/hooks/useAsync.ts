import { useEffect, useState } from "react";

export interface AsyncState<T> {
  value: T;
  loading: boolean;
}

export function useAsync<T>(load: () => Promise<T>, initial: T): AsyncState<T> {
  const [state, setState] = useState<AsyncState<T>>({ value: initial, loading: true });

  useEffect(() => {
    let cancelled = false;

    load().then(
      value => {
        if (!cancelled) setState({ value, loading: false });
      },
      () => {
        if (!cancelled) setState(previous => ({ ...previous, loading: false }));
      },
    );

    return () => {
      cancelled = true;
    };
  }, [load]);

  return state;
}
