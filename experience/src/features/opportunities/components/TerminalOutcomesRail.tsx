import { Popover } from '@/components/ui/Popover';
import { OpportunityOutcomePopoverContent } from './OpportunityOutcomePopover';
import type { OutcomeAnchor } from './ConnectedFlow';
import { cn } from '@/lib/utils';

interface TerminalOutcomesRailProps {
  anchors: OutcomeAnchor[];
  periodDays: number;
}

function branchStyleLabel(branchStyle: OutcomeAnchor['branchStyle']): string {
  if (branchStyle === 'solid') return 'Positive';
  if (branchStyle === 'gray_dotted') return 'Passive';
  return 'Negative';
}

export function TerminalOutcomesRail({ anchors, periodDays }: TerminalOutcomesRailProps) {
  if (anchors.length === 0) {
    return null;
  }

  return (
    <aside aria-label="Terminal outcomes rail">
      {anchors.map((anchor) => (
        <div
          key={anchor.key}
          className="absolute -translate-y-1/2"
          style={{ left: anchor.x - 70, top: anchor.y }}
        >
          <Popover
            trigger={
              <button
                type="button"
                className={cn(
                  'w-[138px] rounded-xl bg-surface-main/65 px-3 py-2 text-left shadow-sm transition-colors hover:bg-surface-main/80 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-nebula-violet/50',
                )}
                aria-label={`${anchor.label} outcome, ${anchor.count} exits, ${anchor.percentOfTotal.toFixed(1)} percent of total`}
              >
                <p className="truncate text-xs font-semibold uppercase tracking-wide text-text-muted">
                  {anchor.label}
                </p>
                <div className="mt-1 flex items-center justify-between">
                  <span className="text-base font-semibold text-text-primary">{anchor.count}</span>
                  <span className="text-xs text-text-muted">
                    {anchor.percentOfTotal.toFixed(1)}%
                  </span>
                </div>
                <p className="mt-1 text-[11px] text-text-muted">{branchStyleLabel(anchor.branchStyle)}</p>
              </button>
            }
          >
            <OpportunityOutcomePopoverContent outcomeKey={anchor.key} periodDays={periodDays} />
          </Popover>
        </div>
      ))}
    </aside>
  );
}
