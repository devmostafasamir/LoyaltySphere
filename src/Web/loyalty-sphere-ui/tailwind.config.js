/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      // =============================================
      // CINEMATIC COLOR PALETTE
      // Primary: Deep Crimson Red  |  Accent: Loyalty Gold
      // =============================================
      colors: {
        crimson: {
          50:  '#fff1f2',
          100: '#ffe4e6',
          200: '#fecdd3',
          300: '#fda4af',
          400: '#fb7185',
          500: '#f43f5e',
          600: '#e11d48',
          700: '#be123c',
          800: '#9f1239', // Primary brand color
          900: '#881337',
          950: '#4c0519',
        },
        gold: {
          50:  '#fffbeb',
          100: '#fef3c7',
          200: '#fde68a',
          300: '#fcd34d',
          400: '#fbbf24',
          500: '#f59e0b', // Primary accent
          600: '#d97706',
          700: '#b45309',
          800: '#92400e',
          900: '#78350f',
        },
      },

      // =============================================
      // TYPOGRAPHY
      // =============================================
      fontFamily: {
        sans:    ['Inter', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'sans-serif'],
        display: ['Poppins', 'Inter', 'sans-serif'],
        mono:    ['JetBrains Mono', 'Fira Code', 'monospace'],
      },

      // =============================================
      // CINEMATIC SHADOW SYSTEM
      // =============================================
      boxShadow: {
        'glow':         '0 0 20px rgba(159, 18, 57, 0.4), 0 0 40px rgba(159, 18, 57, 0.2)',
        'glow-gold':    '0 0 20px rgba(245, 158, 11, 0.5), 0 0 40px rgba(245, 158, 11, 0.3)',
        'glow-sm':      '0 0 10px rgba(159, 18, 57, 0.3)',
        'glow-lg':      '0 0 40px rgba(159, 18, 57, 0.5), 0 0 80px rgba(159, 18, 57, 0.2)',
        'card':         '0 4px 24px rgba(0, 0, 0, 0.4)',
        'card-hover':   '0 8px 40px rgba(0, 0, 0, 0.6), 0 0 20px rgba(159, 18, 57, 0.1)',
        'cinematic':    '0 25px 50px -12px rgba(0, 0, 0, 0.8)',
      },

      // =============================================
      // ANIMATIONS & KEYFRAMES
      // =============================================
      animation: {
        // Rewards & celebrations
        'reward-pop':     'rewardPop 0.6s cubic-bezier(0.175, 0.885, 0.32, 1.275)',
        'sparkle':        'sparkle 1.5s ease-in-out infinite',
        'scale-in':       'scaleIn 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275)',
        'float-up':       'floatUp 3s ease-out forwards',

        // Status & UI
        'fade-in':        'fadeIn 0.3s ease-out',
        'slide-in-right': 'slideInRight 0.35s cubic-bezier(0.4, 0, 0.2, 1)',
        'slide-in-up':    'slideInUp 0.35s cubic-bezier(0.4, 0, 0.2, 1)',
        'slide-out-right':'slideOutRight 0.3s ease-in forwards',

        // Loading & shimmer
        'shimmer':        'shimmer 2.5s infinite linear',
        'pulse-glow':     'pulseGlow 2s ease-in-out infinite',

        // Points counter
        'count-up':       'countUp 0.8s ease-out',
      },

      keyframes: {
        rewardPop: {
          '0%':   { opacity: '0', transform: 'scale(0.3) rotate(-15deg)' },
          '60%':  { opacity: '1', transform: 'scale(1.15) rotate(5deg)' },
          '80%':  { transform: 'scale(0.95) rotate(-2deg)' },
          '100%': { opacity: '1', transform: 'scale(1) rotate(0deg)' },
        },
        sparkle: {
          '0%, 100%': { transform: 'scale(1) rotate(0deg)',    opacity: '1' },
          '25%':      { transform: 'scale(1.2) rotate(15deg)', opacity: '0.8' },
          '75%':      { transform: 'scale(0.9) rotate(-15deg)',opacity: '0.9' },
        },
        scaleIn: {
          '0%':   { opacity: '0', transform: 'scale(0.8)' },
          '100%': { opacity: '1', transform: 'scale(1)' },
        },
        floatUp: {
          '0%':   { opacity: '1', transform: 'translateY(0)' },
          '100%': { opacity: '0', transform: 'translateY(-80px)' },
        },
        fadeIn: {
          from: { opacity: '0' },
          to:   { opacity: '1' },
        },
        slideInRight: {
          from: { transform: 'translateX(100%)', opacity: '0' },
          to:   { transform: 'translateX(0)',    opacity: '1' },
        },
        slideInUp: {
          from: { transform: 'translateY(20px)', opacity: '0' },
          to:   { transform: 'translateY(0)',    opacity: '1' },
        },
        slideOutRight: {
          from: { transform: 'translateX(0)',    opacity: '1' },
          to:   { transform: 'translateX(110%)', opacity: '0' },
        },
        shimmer: {
          '0%':   { backgroundPosition: '-1000px 0' },
          '100%': { backgroundPosition: '1000px 0' },
        },
        pulseGlow: {
          '0%, 100%': { boxShadow: '0 0 10px rgba(159, 18, 57, 0.3)' },
          '50%':      { boxShadow: '0 0 30px rgba(159, 18, 57, 0.7), 0 0 60px rgba(159, 18, 57, 0.2)' },
        },
        countUp: {
          from: { opacity: '0', transform: 'translateY(10px) scale(0.95)' },
          to:   { opacity: '1', transform: 'translateY(0) scale(1)' },
        },
      },

      // =============================================
      // SPACING SCALE (custom additions)
      // =============================================
      spacing: {
        '18': '4.5rem',
        '88': '22rem',
        '128': '32rem',
      },

      // =============================================
      // BORDER RADIUS
      // =============================================
      borderRadius: {
        '4xl': '2rem',
        '5xl': '2.5rem',
      },

      // =============================================
      // BACKDROP BLUR
      // =============================================
      backdropBlur: {
        xs: '2px',
      },
    },
  },
  plugins: [],
};
