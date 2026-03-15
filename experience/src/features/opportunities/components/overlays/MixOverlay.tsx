import { ErrorFallback } from '@/components/ui/ErrorFallback';
import { Skeleton } from '@/components/ui/Skeleton';
import { opportunityHex } from '../../lib/opportunity-colors';
import type { OpportunityHierarchyDto, OpportunityHierarchyNodeDto } from '../../types';

interface MixOverlayProps {
  data?: OpportunityHierarchyDto;
  isLoading: boolean;
  isError: boolean;
  onRetry: () => void;
}

interface MixLeaf {
  id: string;
  label: string;
  count: number;
  color: string;
}

function flattenLeaves(node: OpportunityHierarchyNodeDto): OpportunityHierarchyNodeDto[] {
  if (!node.children || node.children.length === 0) return [node];
  return node.children.flatMap(flattenLeaves);
}

function startPoint(angle: number, radius: number) {
  const radians = (angle - 90) * (Math.PI / 180);
  return {
    x: 70 + radius * Math.cos(radians),
    y: 70 + radius * Math.sin(radians),
  };
}

export function MixOverlay({ data, isLoading, isError, onRetry }: MixOverlayProps) {
  const leaves: MixLeaf[] = data
    ? flattenLeaves(data.root)
        .filter((node) => node.count > 0 && node.levelType === 'status')
        .slice(0, 8)
        .map((node) => ({
          id: node.id,
          label: node.label,
          count: node.count,
          color: opportunityHex(node.colorGroup ?? 'intake'),
        }))
    : [];

  const total = leaves.reduce((sum, leaf) => sum + leaf.count, 0);
  let angleCursor = 0;
  const arcs = leaves.map((leaf) => {
    const angleSize = total > 0 ? (leaf.count / total) * 360 : 0;
    const startAngle = angleCursor;
    const endAngle = angleCursor + angleSize;
    angleCursor = endAngle;
    return { ...leaf, startAngle, endAngle };
  });

  return (
    <div aria-label="Mix overlay" className="pointer-events-none absolute inset-x-4 bottom-3">
      <div className="pointer-events-auto flex flex-col gap-3 rounded-xl bg-surface-main/70 p-3 lg:flex-row lg:items-center">
        {isLoading && <Skeleton className="h-24 w-full" />}

        {isError && (
          <ErrorFallback message="Unable to load mix overlay data" onRetry={onRetry} />
        )}

        {!isLoading && !isError && data && (
          <>
            <div className="flex-1">
              <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-text-muted">
                Composition blocks
              </p>
              <div className="flex h-14 overflow-hidden rounded-lg bg-surface-main/50">
                {leaves.map((leaf) => (
                  <div
                    key={leaf.id}
                    className="flex min-w-[2.5rem] items-end px-2 py-1 text-[10px] font-semibold text-white"
                    style={{ backgroundColor: leaf.color, width: `${(leaf.count / Math.max(total, 1)) * 100}%` }}
                    title={`${leaf.label}: ${leaf.count}`}
                  >
                    <span className="truncate">{leaf.label}</span>
                  </div>
                ))}
              </div>
            </div>

            <div className="mx-auto lg:mx-0">
              <svg viewBox="0 0 140 140" className="h-[90px] w-[90px]" role="img" aria-label="Mix radial inset">
                {arcs.map((arc) => {
                  const outerStart = startPoint(arc.startAngle, 50);
                  const outerEnd = startPoint(arc.endAngle, 50);
                  const innerStart = startPoint(arc.startAngle, 30);
                  const innerEnd = startPoint(arc.endAngle, 30);
                  const largeArc = arc.endAngle - arc.startAngle > 180 ? 1 : 0;

                  return (
                    <path
                      key={`arc-${arc.id}`}
                      d={[
                        `M ${outerStart.x} ${outerStart.y}`,
                        `A 50 50 0 ${largeArc} 1 ${outerEnd.x} ${outerEnd.y}`,
                        `L ${innerEnd.x} ${innerEnd.y}`,
                        `A 30 30 0 ${largeArc} 0 ${innerStart.x} ${innerStart.y}`,
                        'Z',
                      ].join(' ')}
                      fill={arc.color}
                      fillOpacity={0.9}
                    />
                  );
                })}
              </svg>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
