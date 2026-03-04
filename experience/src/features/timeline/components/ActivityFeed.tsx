import { useTimelineEvents } from '../hooks/useTimelineEvents';
import { Card, CardHeader, CardTitle } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';
import { ErrorFallback } from '@/components/ui/ErrorFallback';
import { ActivityFeedItem } from './ActivityFeedItem';

export function ActivityFeed() {
  const { data, isLoading, isError, refetch } = useTimelineEvents('Broker', 12);

  return (
    <Card className="flex h-full min-h-0 flex-col overflow-hidden p-0">
      <CardHeader className="mb-0 border-b border-border-muted/60 px-4 py-3">
        <CardTitle>Activity</CardTitle>
        <span className="text-xs uppercase tracking-wider text-text-muted">Timeline</span>
      </CardHeader>

      {isLoading && (
        <div className="flex-1 space-y-2 px-4 py-3">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-8 w-full" />
          ))}
        </div>
      )}

      {isError && (
        <div className="flex-1 px-4 py-4">
          <ErrorFallback
            message="Unable to load activity feed"
            onRetry={() => refetch()}
          />
        </div>
      )}

      {data && (
        <>
          {data.length === 0 ? (
            <p className="flex-1 px-4 py-6 text-center text-sm text-text-muted">
              No recent broker activity.
            </p>
          ) : (
            <div className="timeline-scrollbar min-h-0 flex-1 overflow-y-scroll px-3 py-2">
              {data.map((event, index) => (
                <ActivityFeedItem
                  key={event.id}
                  event={event}
                  isLast={index === data.length - 1}
                />
              ))}
            </div>
          )}
        </>
      )}
    </Card>
  );
}
