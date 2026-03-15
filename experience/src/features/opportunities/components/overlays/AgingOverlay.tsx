import { ErrorFallback } from '@/components/ui/ErrorFallback';
import { Skeleton } from '@/components/ui/Skeleton';
import { opportunityHex } from '../../lib/opportunity-colors';
import type { OpportunityAgingDto } from '../../types';

interface AgingOverlayProps {
  data?: OpportunityAgingDto;
  isLoading: boolean;
  isError: boolean;
  onRetry: () => void;
}

function alphaHex(intensity: number): string {
  const bounded = Math.max(0, Math.min(intensity, 1));
  const value = Math.round((0.2 + bounded * 0.65) * 255);
  return value.toString(16).padStart(2, '0');
}

export function AgingOverlay({ data, isLoading, isError, onRetry }: AgingOverlayProps) {
  return (
    <div aria-label="Aging overlay" className="pointer-events-none absolute inset-x-4 bottom-3">
      <div className="pointer-events-auto rounded-xl bg-surface-main/70 p-3">
        {isLoading && <Skeleton className="h-24 w-full" />}

        {isError && (
          <ErrorFallback message="Unable to load aging overlay data" onRetry={onRetry} />
        )}

        {!isLoading && !isError && data && (
          <div className="space-y-2">
            <p className="text-xs font-semibold uppercase tracking-wide text-text-muted">
              Aging intensity
            </p>
            {data.statuses
              .filter((status) => status.total > 0)
              .slice(0, 6)
              .map((status) => {
                const maxBucket = Math.max(...status.buckets.map((bucket) => bucket.count), 1);
                const tone = opportunityHex(status.colorGroup);
                return (
                  <div key={status.status} className="grid grid-cols-[9rem_1fr_auto] items-center gap-2 text-xs">
                    <span className="truncate text-text-secondary">{status.label}</span>
                    <div className="grid grid-cols-5 gap-1">
                      {status.buckets.map((bucket) => (
                        <span
                          key={bucket.key}
                          className="h-3 rounded"
                          style={{
                            backgroundColor: `${tone}${alphaHex(bucket.count / maxBucket)}`,
                          }}
                          title={`${status.label} ${bucket.label}: ${bucket.count}`}
                        />
                      ))}
                    </div>
                    <span className="tabular-nums text-text-muted">{status.total}</span>
                  </div>
                );
              })}
          </div>
        )}
      </div>
    </div>
  );
}
