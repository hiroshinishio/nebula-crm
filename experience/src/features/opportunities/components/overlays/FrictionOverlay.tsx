import { cn } from '@/lib/utils';
import type { StageAnchor } from '../storyTimelineTypes';

interface FrictionOverlayProps {
  stageAnchors: StageAnchor[];
}

export function FrictionOverlay({ stageAnchors }: FrictionOverlayProps) {
  return (
    <div aria-label="Friction overlay" className="pointer-events-none absolute inset-0">
      {stageAnchors.map((anchor) => {
        const emphasis = anchor.emphasis ?? 'normal';
        return (
          <div
            key={`friction-${anchor.status}`}
            className="absolute -translate-x-1/2"
            style={{ left: anchor.x, top: anchor.y - 74 }}
          >
            <div className="rounded-full bg-surface-main/75 px-2 py-0.5 text-[10px] uppercase tracking-wide text-text-muted">
              {emphasis}
            </div>
            <p
              className={cn(
                'mt-1 text-center text-[11px] font-semibold text-text-secondary',
                emphasis !== 'normal' && `flow-emphasis-${emphasis}`,
              )}
            >
              {(anchor.avgDwellDays ?? 0).toFixed(1)}d
            </p>
          </div>
        );
      })}
    </div>
  );
}
