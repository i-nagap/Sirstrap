import type { MouseEvent, SyntheticEvent } from "react";
import { AUTHOR_NAME } from "@/config/site";
import type { AccentStyle } from "@/domain/accent";
import type { Contributor } from "@/domain/ports";
import { useServices } from "@/hooks/useServices";

const MAX_AVATARS = 8;
const SKELETON_COUNT = 3;
const AVATAR = "h-5 w-5 rounded-full border-2 border-background bg-background object-cover";

const avatarOf = (account: string) => `https://github.com/${account}.png`;

const stopPropagation = (event: MouseEvent) => event.stopPropagation();

export interface ProductCredits {
  author: string;
  contributors: Contributor[];
  loading: boolean;
}

export function ProductByline({ credits, accent }: { credits: ProductCredits; accent: AccentStyle }) {
  const { accounts } = useServices();
  const { author, contributors, loading } = credits;
  const authorUrl = `https://github.com/${author}`;
  const others = contributors.filter(contributor => !accounts.owns(contributor.login)).slice(0, MAX_AVATARS - 1);
  const trailing = others.length + (loading ? SKELETON_COUNT : 0);

  const fallbackAvatar = (event: SyntheticEvent<HTMLImageElement>) => {
    const avatars = accounts.all.map(avatarOf);
    const next = avatars[avatars.indexOf(event.currentTarget.src) + 1];
    if (next) event.currentTarget.src = next;
  };

  return (
    <div className="group flex w-fit items-center gap-2" onClick={stopPropagation}>
      <a
        href={authorUrl}
        target="_blank"
        rel="noopener noreferrer"
        className="font-body text-[0.85rem] leading-[1.55] text-muted-foreground transition-colors duration-200 group-hover:text-foreground"
      >
        made by <span className={`transition-colors duration-200 ${accent.groupText}`}>{AUTHOR_NAME}</span>
      </a>
      <div className="flex items-center">
        <a
          href={authorUrl}
          target="_blank"
          rel="noopener noreferrer"
          aria-label={author}
          style={{ zIndex: trailing + 1 }}
          className="relative transition-all duration-300 ease-out hover:!z-20 hover:-translate-y-1"
        >
          <img
            src={avatarOf(author)}
            alt={author}
            onError={fallbackAvatar}
            className={`${AVATAR} transition-all duration-300 group-hover:scale-110 ${accent.groupBorder}`}
          />
        </a>

        {loading
          ? Array.from({ length: SKELETON_COUNT }, (_, index) => (
              <span
                key={index}
                style={{ zIndex: trailing - index, animationDelay: `${index * 150}ms` }}
                className={`relative -ml-2 ${AVATAR} bg-muted/60 animate-pulse`}
              />
            ))
          : others.map((contributor, index) => (
              <a
                key={contributor.login}
                href={contributor.htmlUrl}
                target="_blank"
                rel="noopener noreferrer"
                aria-label={contributor.login}
                style={{ zIndex: trailing - index, transitionDelay: `${index * 40}ms` }}
                className="relative -ml-2 transition-all duration-300 ease-out hover:!z-20 hover:-translate-y-1 group-hover:-ml-1"
              >
                <img
                  src={contributor.avatarUrl}
                  alt={contributor.login}
                  loading="lazy"
                  className={`${AVATAR} transition-all duration-300 group-hover:border-border`}
                />
              </a>
            ))}
      </div>
    </div>
  );
}
