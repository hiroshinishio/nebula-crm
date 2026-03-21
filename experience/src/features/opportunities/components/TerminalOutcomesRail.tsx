import { Popover } from '@/components/ui/Popover';
import { cn } from '@/lib/utils';
import { OpportunityOutcomePopoverContent } from './OpportunityOutcomePopover';
import type { StoryChapter } from './storyTypes';
import type { OutcomeAnchor } from './storyTimelineTypes';

interface TerminalOutcomesRailProps {
  anchors: OutcomeAnchor[];
  periodDays: number;
  chapter: StoryChapter;
  allOutcomesZero: boolean;
}

function branchStyleLabel(branchStyle: OutcomeAnchor['branchStyle']): string {
  if (branchStyle === 'solid') return 'Positive';
  if (branchStyle === 'gray_dotted') return 'Passive';
  return 'Negative';
}

export function TerminalOutcomesRail({
  anchors,
  periodDays,
  chapter,
  allOutcomesZero,
}: TerminalOutcomesRailProps) {
  if (anchors.length === 0) {
    return null;
  }

  return (
    <aside aria-label="Terminal outcome branches">
      {anchors.map((anchor) => (
        <div
          key={anchor.key}
          className="absolute -translate-x-1/2 -translate-y-1/2"
          style={{ left: anchor.x, top: anchor.y }}
        >
          <Popover
            contentAriaLabel={`${anchor.label} outcome details, ${anchor.count} exits, ${anchor.percentOfTotal.toFixed(1)} percent`}
            trigger={
              <button
                type="button"
                className={cn(
                  'story-focus-ring w-[156px] rounded-xl bg-surface-main/65 px-3 py-2 text-left shadow-sm transition-colors hover:bg-surface-main/80',
                  chapter === 'outcomes' && !allOutcomesZero && 'story-active-ring',
                  chapter === 'outcomes' && allOutcomesZero && 'opacity-65',
                )}
                aria-label={`${anchor.label} outcome, ${anchor.count} exits, ${anchor.percentOfTotal.toFixed(1)} percent of total`}
              >
                <p className="truncate text-xs font-semibold uppercase tracking-wide text-text-muted">
                  {anchor.label}
                </p>
                <div className="mt-1 flex items-center justify-between">
                  <span
                    className={cn(
                      'text-base font-semibold text-text-primary',
                      chapter === 'outcomes' && !allOutcomesZero && 'text-lg',
                    )}
                  >
                    {anchor.count}
                  </span>
                  <span
                    className={cn(
                      'text-xs text-text-muted',
                      chapter === 'outcomes' && !allOutcomesZero && 'text-sm font-semibold text-text-primary',
                    )}
                  >
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
