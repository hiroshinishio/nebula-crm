import type { OutcomeAnchor } from '../storyTimelineTypes';

interface OutcomesOverlayProps {
  outcomeAnchors: OutcomeAnchor[];
}

export function OutcomesOverlay({ outcomeAnchors }: OutcomesOverlayProps) {
  return (
    <div aria-label="Outcomes overlay" className="pointer-events-none absolute inset-0">
      {outcomeAnchors.map((anchor) => (
        <div
          key={`outcomes-${anchor.key}`}
          className="absolute -translate-y-1/2 rounded-full bg-surface-main/75 px-2 py-0.5 text-[10px] uppercase tracking-wide text-text-muted"
          style={{ left: anchor.x - 190, top: anchor.y }}
        >
          {anchor.percentOfTotal.toFixed(1)}% of exits
        </div>
      ))}
    </div>
  );
}
