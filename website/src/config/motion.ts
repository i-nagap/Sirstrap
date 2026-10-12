export const EASING = "cubic-bezier(0.4, 0, 0.2, 1)";
export const DURATION = 500;

export const transition = (properties: string[], duration = DURATION) =>
  properties.map(property => `${property} ${duration}ms ${EASING}`).join(", ");

export interface Jitter {
  min: number;
  jitter: number;
}

export const TITLE_TIMING = {
  startDelay: 5000,
  holdFull: 5000,
  holdEmpty: 2500,
  type: { min: 125, jitter: 25 },
  rapid: { min: 30, jitter: 25 },
  erase: { min: 25, jitter: 25 },
} as const;

export const jitter = ({ min, jitter }: Jitter) => min + Math.random() * jitter;
