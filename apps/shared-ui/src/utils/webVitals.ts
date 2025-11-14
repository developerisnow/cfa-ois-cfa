// Lightweight web-vitals initializer with safe dynamic import
export function initWebVitals() {
  if (typeof window === 'undefined') return;
  // Avoid double init
  if ((window as any).__webVitalsInited) return;
  (window as any).__webVitalsInited = true;

  import('web-vitals')
    .then(({ onCLS, onFID, onLCP, onINP, onTTFB }) => {
      const report = (metric: any) => {
        try {
          // Emit as a custom event; consumers may collect and forward
          window.dispatchEvent(
            new CustomEvent('web-vitals', { detail: metric })
          );
          // Also log for debugging
          // eslint-disable-next-line no-console
          console.debug('[web-vitals]', metric.name, Math.round(metric.value));
        } catch {
          // ignore
        }
      };
      onCLS(report);
      onFID(report);
      onLCP(report);
      onINP?.(report as any);
      onTTFB(report);
    })
    .catch(() => {
      // web-vitals not installed; ignore
    });
}

