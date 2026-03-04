import { useQuery } from '@tanstack/react-query';
import { api } from '@/services/api';
import type { TimelineEventDto } from '@/contracts/timeline';

export function useTimelineEvents(entityType: string, limit: number) {
  return useQuery({
    queryKey: ['timeline', 'events', entityType, limit],
    queryFn: () =>
      api.get<TimelineEventDto[]>(`/timeline/events?entityType=${entityType}&limit=${limit}`),
  });
}
