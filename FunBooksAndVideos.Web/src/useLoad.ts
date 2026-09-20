import { useEffect, useState } from "react";
import { asApiError, type ApiError } from "./api";

export interface Loaded<T> {
  data: T | undefined;
  error: ApiError | undefined;
  loading: boolean;
  reload: () => void;
}

// Loads data when the component mounts and again whenever `deps` change or reload() is called.
// The old data stays on screen while a reload is in flight, so lists do not flash empty after a change.
export function useLoad<T>(load: () => Promise<T>, deps: unknown[] = []): Loaded<T> {
  const [data, setData] = useState<T>();
  const [error, setError] = useState<ApiError>();
  const [loading, setLoading] = useState(true);
  const [reloads, setReloads] = useState(0);

  useEffect(() => {
    let stale = false; // set when a newer request has replaced this one
    setLoading(true);

    load()
      .then((result) => {
        if (stale) return;
        setData(result);
        setError(undefined);
      })
      .catch((e) => {
        if (!stale) setError(asApiError(e));
      })
      .finally(() => {
        if (!stale) setLoading(false);
      });

    return () => {
      stale = true;
    };
  }, [...deps, reloads]);

  return { data, error, loading, reload: () => setReloads((count) => count + 1) };
}
