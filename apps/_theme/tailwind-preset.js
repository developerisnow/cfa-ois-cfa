/** @type {import('tailwindcss').Config} */
module.exports = {
  theme: {
    extend: {
      colors: {
        // Background colors
        background: {
          DEFAULT: 'var(--color-background)',
          alt: 'var(--color-background-alt)',
        },
        // Surface colors
        surface: {
          DEFAULT: 'var(--color-surface)',
          alt: 'var(--color-surface-alt)',
          hover: 'var(--color-surface-hover)',
        },
        // Primary colors
        primary: {
          50: 'var(--color-primary-50)',
          100: 'var(--color-primary-100)',
          200: 'var(--color-primary-200)',
          300: 'var(--color-primary-300)',
          400: 'var(--color-primary-400)',
          500: 'var(--color-primary-500)',
          600: 'var(--color-primary-600)',
          700: 'var(--color-primary-700)',
          800: 'var(--color-primary-800)',
          900: 'var(--color-primary-900)',
          DEFAULT: 'var(--color-primary-500)',
        },
        // Semantic colors
        success: {
          50: 'var(--color-success-50)',
          500: 'var(--color-success-500)',
          600: 'var(--color-success-600)',
          700: 'var(--color-success-700)',
          DEFAULT: 'var(--color-success-500)',
        },
        warning: {
          50: 'var(--color-warning-50)',
          500: 'var(--color-warning-500)',
          600: 'var(--color-warning-600)',
          700: 'var(--color-warning-700)',
          DEFAULT: 'var(--color-warning-500)',
        },
        danger: {
          50: 'var(--color-danger-50)',
          500: 'var(--color-danger-500)',
          600: 'var(--color-danger-600)',
          700: 'var(--color-danger-700)',
          DEFAULT: 'var(--color-danger-500)',
        },
        info: {
          50: 'var(--color-info-50)',
          500: 'var(--color-info-500)',
          600: 'var(--color-info-600)',
          700: 'var(--color-info-700)',
          DEFAULT: 'var(--color-info-500)',
        },
        // Text colors
        text: {
          primary: 'var(--color-text-primary)',
          secondary: 'var(--color-text-secondary)',
          tertiary: 'var(--color-text-tertiary)',
          disabled: 'var(--color-text-disabled)',
        },
        // Border colors
        border: {
          DEFAULT: 'var(--color-border)',
          focus: 'var(--color-border-focus)',
        },
        // Chart palette (8 colors)
        chart: {
          1: 'var(--color-chart-1)',
          2: 'var(--color-chart-2)',
          3: 'var(--color-chart-3)',
          4: 'var(--color-chart-4)',
          5: 'var(--color-chart-5)',
          6: 'var(--color-chart-6)',
          7: 'var(--color-chart-7)',
          8: 'var(--color-chart-8)',
        },
      },
      borderRadius: {
        none: 'var(--radius-none)',
        sm: 'var(--radius-sm)',
        DEFAULT: 'var(--radius-md)',
        lg: 'var(--radius-lg)',
        xl: 'var(--radius-xl)',
        '2xl': 'var(--radius-2xl)',
        full: 'var(--radius-full)',
      },
      boxShadow: {
        sm: 'var(--shadow-sm)',
        DEFAULT: 'var(--shadow-md)',
        lg: 'var(--shadow-lg)',
        xl: 'var(--shadow-xl)',
        '2xl': 'var(--shadow-2xl)',
        inner: 'var(--shadow-inner)',
      },
      spacing: {
        // Custom spacing scale
        0.5: '0.125rem',
        1.5: '0.375rem',
        2.5: '0.625rem',
        3.5: '0.875rem',
      },
      zIndex: {
        0: '0',
        10: '10',
        20: '20',
        30: '30',
        40: '40',
        50: '50',
        60: '60',
        70: '70',
        80: '80',
        90: '90',
        100: '100',
        dropdown: '1000',
        sticky: '1020',
        fixed: '1030',
        modal: '1040',
        popover: '1050',
        tooltip: '1060',
      },
      fontFamily: {
        sans: ['var(--font-sans)', 'system-ui', 'sans-serif'],
        mono: ['var(--font-mono)', 'monospace'],
      },
    },
  },
};

