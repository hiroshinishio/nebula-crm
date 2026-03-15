import { Popover } from '@/components/ui/Popover';
import { opportunityHex } from '../lib/opportunity-colors';
import { OpportunityPopoverContent } from './OpportunityPopover';
import { TerminalOutcomesRail } from './TerminalOutcomesRail';
import { ChapterOverlayManager } from './ChapterOverlayManager';
import type {
  OpportunityAgingDto,
  OpportunityFlowDto,
  OpportunityFlowNodeDto,
  OpportunityHierarchyDto,
  OpportunityOutcomeDto,
} from '../types';
import type { StoryChapter } from './storyTypes';
import { cn } from '@/lib/utils';

export interface StageAnchor {
  status: string;
  label: string;
  x: number;
  y: number;
  avgDwellDays?: number | null;
  emphasis?: OpportunityFlowNodeDto['emphasis'];
}

export interface OutcomeAnchor {
  key: string;
  label: string;
  branchStyle: OpportunityOutcomeDto['branchStyle'];
  count: number;
  percentOfTotal: number;
  x: number;
  y: number;
}

interface ConnectedFlowProps {
  flow: OpportunityFlowDto;
  outcomes: OpportunityOutcomeDto[];
  chapter: StoryChapter;
  periodDays: number;
  outcomesLoading: boolean;
  outcomesError: boolean;
  onRetryOutcomes: () => void;
  agingData?: OpportunityAgingDto;
  agingLoading: boolean;
  agingError: boolean;
  onRetryAging: () => void;
  mixData?: OpportunityHierarchyDto;
  mixLoading: boolean;
  mixError: boolean;
  onRetryMix: () => void;
}

const STAGE_NODE_WIDTH = 116;
const STAGE_START_X = 92;
const STAGE_SPACING = 132;
const STAGE_Y = 146;

function buildLinkPath(source: StageAnchor, target: StageAnchor): string {
  const startX = source.x + STAGE_NODE_WIDTH / 2 - 6;
  const endX = target.x - STAGE_NODE_WIDTH / 2 + 6;
  const controlX = (startX + endX) / 2;
  return `M ${startX} ${source.y} C ${controlX} ${source.y}, ${controlX} ${target.y}, ${endX} ${target.y}`;
}

function buildOutcomePath(source: StageAnchor, target: OutcomeAnchor): string {
  const startX = source.x + STAGE_NODE_WIDTH / 2 - 6;
  const endX = target.x - 74;
  const controlX = startX + 72;
  return `M ${startX} ${source.y} C ${controlX} ${source.y}, ${controlX} ${target.y}, ${endX} ${target.y}`;
}

function branchStroke(branchStyle: OpportunityOutcomeDto['branchStyle']) {
  if (branchStyle === 'solid') {
    return { stroke: '#2f8f6a', strokeDasharray: undefined };
  }

  if (branchStyle === 'gray_dotted') {
    return { stroke: '#94a3b8', strokeDasharray: '3 6' };
  }

  return { stroke: '#c95c5c', strokeDasharray: '8 6' };
}

export function ConnectedFlow({
  flow,
  outcomes,
  chapter,
  periodDays,
  outcomesLoading,
  outcomesError,
  onRetryOutcomes,
  agingData,
  agingLoading,
  agingError,
  onRetryAging,
  mixData,
  mixLoading,
  mixError,
  onRetryMix,
}: ConnectedFlowProps) {
  const stageNodes = flow.nodes
    .filter((node) => !node.isTerminal)
    .sort((left, right) => left.displayOrder - right.displayOrder);

  if (stageNodes.length === 0) {
    return (
      <div className="py-8 text-center text-sm text-text-muted">
        No opportunity flow data for the selected period.
      </div>
    );
  }

  const stageAnchors: StageAnchor[] = stageNodes.map((node, index) => ({
    status: node.status,
    label: node.label,
    x: STAGE_START_X + index * STAGE_SPACING,
    y: STAGE_Y,
    avgDwellDays: node.avgDwellDays,
    emphasis: node.emphasis,
  }));

  const rightmostStage = stageAnchors[stageAnchors.length - 1];
  const outcomeStartY = 74;
  const outcomeGap = outcomes.length > 1 ? 56 : 0;
  const outcomeX = rightmostStage.x + 252;
  const outcomeAnchors: OutcomeAnchor[] = outcomes.map((outcome, index) => ({
    key: outcome.key,
    label: outcome.label,
    branchStyle: outcome.branchStyle,
    count: outcome.count,
    percentOfTotal: outcome.percentOfTotal,
    x: outcomeX,
    y: outcomeStartY + index * outcomeGap,
  }));

  const canvasWidth = outcomeX + 180;
  const canvasHeight = Math.max(320, outcomeAnchors.length > 0 ? outcomeAnchors[outcomeAnchors.length - 1].y + 86 : 320);
  const stageByStatus = new Map(stageAnchors.map((anchor) => [anchor.status, anchor]));
  const maxLinkCount = Math.max(1, ...flow.links.map((link) => link.count));

  const stageLinks = flow.links
    .map((link) => ({
      link,
      source: stageByStatus.get(link.sourceStatus),
      target: stageByStatus.get(link.targetStatus),
    }))
    .filter((entry): entry is { link: OpportunityFlowDto['links'][number]; source: StageAnchor; target: StageAnchor } =>
      Boolean(entry.source && entry.target),
    );

  return (
    <div className="canvas-chapter-overlay relative overflow-x-auto">
      <div className="relative min-h-[320px] min-w-[900px]" style={{ width: canvasWidth, height: canvasHeight }}>
        <svg
          aria-hidden="true"
          className="absolute inset-0 h-full w-full"
          viewBox={`0 0 ${canvasWidth} ${canvasHeight}`}
        >
          {stageLinks.map(({ link, source, target }) => (
            <path
              key={`${link.sourceStatus}-${link.targetStatus}`}
              d={buildLinkPath(source, target)}
              fill="none"
              stroke={opportunityHex(
                stageNodes.find((node) => node.status === link.sourceStatus)?.colorGroup ?? 'intake',
              )}
              strokeOpacity={chapter === 'outcomes' ? 0.24 : 0.48}
              strokeWidth={2 + (link.count / maxLinkCount) * 8}
              strokeLinecap="round"
            />
          ))}

          {outcomeAnchors.map((outcomeAnchor) => {
            const stroke = branchStroke(outcomeAnchor.branchStyle);
            return (
              <path
                key={`branch-${outcomeAnchor.key}`}
                d={buildOutcomePath(rightmostStage, outcomeAnchor)}
                fill="none"
                stroke={stroke.stroke}
                strokeDasharray={stroke.strokeDasharray}
                strokeOpacity={chapter === 'outcomes' ? 1 : 0.62}
                strokeWidth={2.5}
                strokeLinecap="round"
              />
            );
          })}
        </svg>

        {stageAnchors.map((anchor) => {
          const sourceNode = stageNodes.find((node) => node.status === anchor.status);
          const emphasisClass = chapter === 'friction'
            ? `flow-emphasis-${sourceNode?.emphasis ?? 'normal'}`
            : 'flow-emphasis-normal';

          return (
            <div
              key={anchor.status}
              className="absolute -translate-x-1/2 -translate-y-1/2"
              style={{ left: anchor.x, top: anchor.y }}
            >
              <Popover
                trigger={
                  <button
                    type="button"
                    className={cn(
                      'min-h-11 w-[116px] rounded-xl bg-surface-main/65 px-3 py-2 text-left shadow-sm transition-colors hover:bg-surface-main/80 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-nebula-violet/50',
                      emphasisClass,
                    )}
                    aria-label={`${anchor.label} stage, ${sourceNode?.currentCount ?? 0} opportunities`}
                  >
                    <p className="truncate text-xs font-semibold uppercase tracking-wide text-text-muted">
                      {anchor.label}
                    </p>
                    <p className="mt-1 text-lg font-semibold text-text-primary">
                      {sourceNode?.currentCount ?? 0}
                    </p>
                  </button>
                }
              >
                <OpportunityPopoverContent entityType="submission" status={anchor.status} />
              </Popover>
            </div>
          );
        })}

        <TerminalOutcomesRail anchors={outcomeAnchors} periodDays={periodDays} />

        <ChapterOverlayManager
          chapter={chapter}
          stageAnchors={stageAnchors}
          outcomeAnchors={outcomeAnchors}
          agingData={agingData}
          agingLoading={agingLoading}
          agingError={agingError}
          onRetryAging={onRetryAging}
          mixData={mixData}
          mixLoading={mixLoading}
          mixError={mixError}
          onRetryMix={onRetryMix}
          outcomesLoading={outcomesLoading}
          outcomesError={outcomesError}
          onRetryOutcomes={onRetryOutcomes}
        />
      </div>
    </div>
  );
}
