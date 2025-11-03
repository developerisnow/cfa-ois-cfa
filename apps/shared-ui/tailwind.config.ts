import type { Config } from 'tailwindcss';
import preset from '../_theme/tailwind-preset.js';

const config: Config = {
  presets: [preset],
  content: [
    './src/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {},
  },
  plugins: [],
};

export default config;

