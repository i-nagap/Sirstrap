import { useCallback, type FC } from "react";
import type { TitleAnimation } from "@/domain/product";
import { useTimeline } from "@/hooks/useTimeline";
import { rapidTimeline, typewriterTimeline } from "@/ui/title/timelines";

const HEADING = "font-display text-2xl font-extrabold tracking-[-0.035em] lg:text-3xl";
const TAIL = `${HEADING} inline-block`;

type Kind = TitleAnimation["kind"];
type AnimationOf<K extends Kind> = Extract<TitleAnimation, { kind: K }>;

interface TitleProps<A extends TitleAnimation = TitleAnimation> {
  animation: A;
  active: boolean;
  accentClassName: string;
}

const breakable = (text: string) =>
  text.split(".").flatMap((segment, index, segments) =>
    index < segments.length - 1 ? [segment, ".", <wbr key={index} />] : [segment],
  );

function Typewriter({ text, enabled, className }: { text: string; enabled: boolean; className: string }) {
  const timeline = useCallback(() => typewriterTimeline(text), [text]);
  const displayed = useTimeline(text, enabled, timeline);

  return (
    <span className={className}>
      {displayed}
      {enabled && <span className="title-caret">▌</span>}
    </span>
  );
}

function RapidGlyph({ glyph, max, enabled, className }: { glyph: string; max: number; enabled: boolean; className: string }) {
  const timeline = useCallback(() => rapidTimeline(max), [max]);
  const count = useTimeline(0, enabled, timeline);
  return <span className={className}>{glyph.repeat(count)}</span>;
}

const PlainTitle: FC<TitleProps<AnimationOf<"plain">>> = ({ animation, accentClassName }) => (
  <h2 className={`${HEADING} ${accentClassName}`}>{breakable(animation.head)}</h2>
);

const TypewriterTitle: FC<TitleProps<AnimationOf<"typewriter">>> = ({ animation, active, accentClassName }) => (
  <h2 className={`${HEADING} ${accentClassName}`}>
    {breakable(animation.head)}
    <Typewriter text={animation.tail} enabled={active} className={`${TAIL} ${accentClassName}`} />
  </h2>
);

const ShimmerTitle: FC<TitleProps<AnimationOf<"shimmer">>> = ({ animation, active, accentClassName }) => (
  <h2 className={`${HEADING} ${accentClassName}`}>
    {breakable(animation.head)}
    <span className={`${TAIL} ${active ? "title-shimmer" : accentClassName}`}>{animation.tail}</span>
  </h2>
);

const RapidTitle: FC<TitleProps<AnimationOf<"rapid">>> = ({ animation, active, accentClassName }) => (
  <h2 className={`${HEADING} ${accentClassName}`}>
    {breakable(animation.head)}
    <RapidGlyph glyph={animation.glyph} max={animation.max} enabled={active} className={`${HEADING} ${accentClassName}`} />
    <span className={`${TAIL} ${accentClassName}`}>{animation.tail}</span>
  </h2>
);

const RENDERERS: { [K in Kind]: FC<TitleProps<AnimationOf<K>>> } = {
  plain: PlainTitle,
  typewriter: TypewriterTitle,
  shimmer: ShimmerTitle,
  rapid: RapidTitle,
};

export function ProductTitle(props: TitleProps) {
  const Renderer = RENDERERS[props.animation.kind] as FC<TitleProps>;
  return <Renderer {...props} />;
}
