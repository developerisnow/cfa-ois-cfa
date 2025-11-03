# @ois/theme

Shared design tokens for OIS portals.

## Usage

### In Tailwind Config

```ts
import preset from '../_theme/tailwind-preset.js';

export default {
  presets: [preset],
  // ...
};
```

### In CSS

```css
@import '../../_theme/tokens.css';
```

## Tokens

- Colors (background, surface, primary, success, warning, danger, info, text, border, chart)
- Radiuses
- Shadows
- Spacing
- Z-index scale

See [docs/ui/design-system.md](../../docs/ui/design-system.md) for details.

