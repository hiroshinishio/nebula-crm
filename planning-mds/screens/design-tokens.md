# Nebula Design Tokens - Dark & Light Themes

**Version:** 2.1
**Last Updated:** 2026-02-21
**Applies To:** Frontend Developer

**Inspiration:** Dark, glassy analytics UI with neon magenta/violet gradients and soft ambient glow (reference: `planning-mds/screens/WhatsApp Image 2026-02-21 at 4.15.15 PM.jpeg`).

---

## Overview

This document defines the design tokens (colors, spacing, typography, shadows, etc.) for Nebula's **dual-theme UI** with dark mode and light mode support. Both themes feature gradient accent borders and subtle glow effects on hover.

**Design Philosophy:**
- Dark-first with light-mode parity
- Glassy surfaces with soft edge glow
- Neon magenta/violet gradients for key data
- Subdued body text, high-contrast data emphasis
- Rounded cards with layered depth
- Smooth theme transitions

---

## Color Palette

### Dark Theme Colors

```css
.dark {
  /* Backgrounds */
  --background: 228 22% 6%;             /* #0b0f18 - Graphite */
  --background-elevated: 228 20% 9%;    /* #121726 - Raised cards */
  --background-panel: 228 22% 12%;      /* #171d2e - Panels */

  /* Foreground (Text) */
  --foreground: 210 20% 96%;            /* #f1f3f7 - Primary text */
  --muted-foreground: 215 15% 70%;      /* #aeb6c7 - Secondary text */
  --subtle-foreground: 215 12% 55%;     /* #7c8597 - Tertiary text */

  /* UI Elements */
  --card: 228 20% 9%;
  --card-foreground: 210 20% 96%;

  --popover: 228 20% 9%;
  --popover-foreground: 210 20% 96%;

  /* Borders */
  --border: 228 16% 18%;                /* #262c3b */
  --input: 228 16% 18%;

  /* Primary */
  --primary: 268 83% 70%;               /* #8b5cf6 - Violet */
  --primary-foreground: 0 0% 100%;

  /* Secondary */
  --secondary: 228 16% 14%;
  --secondary-foreground: 210 20% 96%;

  /* Muted */
  --muted: 228 16% 14%;
  --muted-foreground: 215 15% 70%;

  /* Accent */
  --accent: 300 84% 60%;                /* #d946ef - Fuchsia */
  --accent-foreground: 0 0% 100%;

  /* Destructive */
  --destructive: 0 84% 60%;
  --destructive-foreground: 0 0% 98%;

  /* Focus Ring */
  --ring: 268 83% 70%;                  /* Violet focus */

  /* Radius */
  --radius: 0.75rem;                    /* 12px */
}
```

### Light Theme Colors

```css
.light {
  /* Backgrounds */
  --background: 225 30% 98%;            /* #f4f6fb - Cool white */
  --background-elevated: 225 22% 96%;   /* #edf0f7 - Raised cards */
  --background-panel: 225 20% 94%;      /* #e7ecf4 - Panels */

  /* Foreground (Text) */
  --foreground: 230 32% 12%;            /* #141824 - Primary text */
  --muted-foreground: 225 12% 45%;      /* #6b7280 - Secondary text */
  --subtle-foreground: 225 10% 62%;     /* #98a2b3 - Tertiary text */

  /* UI Elements */
  --card: 0 0% 100%;
  --card-foreground: 230 32% 12%;

  --popover: 0 0% 100%;
  --popover-foreground: 230 32% 12%;

  /* Borders */
  --border: 220 16% 86%;                /* #d6dbe7 */
  --input: 220 16% 86%;

  /* Primary */
  --primary: 268 83% 70%;               /* #8b5cf6 - Same violet */
  --primary-foreground: 0 0% 100%;

  /* Secondary */
  --secondary: 225 20% 92%;
  --secondary-foreground: 230 32% 12%;

  /* Muted */
  --muted: 225 20% 92%;
  --muted-foreground: 225 12% 45%;

  /* Accent */
  --accent: 300 84% 60%;                /* #d946ef - Same fuchsia */
  --accent-foreground: 0 0% 100%;

  /* Destructive */
  --destructive: 0 84% 60%;
  --destructive-foreground: 0 0% 98%;

  /* Focus Ring */
  --ring: 268 83% 70%;

  /* Radius */
  --radius: 0.75rem;
}
```

### Shared Accent Colors (Work in Both Themes)

```css
:root {
  /* Accent colors (theme-independent) */
  --accent-violet: 268 83% 70%;         /* #8b5cf6 */
  --accent-fuchsia: 300 84% 60%;        /* #d946ef */
  --accent-purple: 276 80% 62%;         /* #9333ea */
  --accent-pink: 330 81% 60%;           /* #ec4899 */
  --accent-orange: 25 95% 53%;          /* #f97316 */

  /* RGB values for gradients and glows */
  --rgb-violet: 139 92 246;
  --rgb-fuchsia: 217 70 239;
  --rgb-purple: 147 51 234;
  --rgb-pink: 236 72 153;
  --rgb-orange: 249 115 22;

  /* Status colors */
  --success: 142 71% 45%;              /* #22c55e */
  --warning: 45 93% 47%;               /* #eab308 */
  --error: 0 84% 60%;                  /* #ef4444 */
  --info: 268 83% 70%;                 /* #8b5cf6 */
}
```

---

## Complete globals.css

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  :root {
    /* Accent colors (theme-independent) */
    --accent-violet: 268 83% 70%;
    --accent-fuchsia: 300 84% 60%;
    --accent-purple: 276 80% 62%;
    --accent-pink: 330 81% 60%;
    --accent-orange: 25 95% 53%;

    /* RGB values for gradients */
    --rgb-violet: 139 92 246;
    --rgb-fuchsia: 217 70 239;
    --rgb-purple: 147 51 234;
    --rgb-pink: 236 72 153;
    --rgb-orange: 249 115 22;

    --radius: 0.75rem;
  }

  /* Light Theme (Default) */
  .light {
    --background: 225 30% 98%;
    --background-elevated: 225 22% 96%;
    --background-panel: 225 20% 94%;
    --foreground: 230 32% 12%;

    --card: 0 0% 100%;
    --card-foreground: 230 32% 12%;

    --popover: 0 0% 100%;
    --popover-foreground: 230 32% 12%;

    --primary: 268 83% 70%;
    --primary-foreground: 0 0% 100%;

    --secondary: 225 20% 92%;
    --secondary-foreground: 230 32% 12%;

    --muted: 225 20% 92%;
    --muted-foreground: 225 12% 45%;

    --accent: 300 84% 60%;
    --accent-foreground: 0 0% 100%;

    --destructive: 0 84% 60%;
    --destructive-foreground: 0 0% 98%;

    --border: 220 16% 86%;
    --input: 220 16% 86%;
    --ring: 268 83% 70%;
  }

  /* Dark Theme */
  .dark {
    --background: 228 22% 6%;
    --background-elevated: 228 20% 9%;
    --background-panel: 228 22% 12%;
    --foreground: 210 20% 96%;

    --card: 228 20% 9%;
    --card-foreground: 210 20% 96%;

    --popover: 228 20% 9%;
    --popover-foreground: 210 20% 96%;

    --primary: 268 83% 70%;
    --primary-foreground: 0 0% 100%;

    --secondary: 228 16% 14%;
    --secondary-foreground: 210 20% 96%;

    --muted: 228 16% 14%;
    --muted-foreground: 215 15% 70%;

    --accent: 300 84% 60%;
    --accent-foreground: 0 0% 100%;

    --destructive: 0 84% 60%;
    --destructive-foreground: 0 0% 98%;

    --border: 228 16% 18%;
    --input: 228 16% 18%;
    --ring: 268 83% 70%;
  }

  * {
    @apply border-border;
  }

  body {
    @apply bg-background text-foreground;
    font-feature-settings: 'rlig' 1, 'calt' 1;
  }
}

@layer utilities {
  /* Gradient text */
  .gradient-text {
    @apply bg-gradient-to-r from-[hsl(var(--accent-violet))] to-[hsl(var(--accent-fuchsia))] bg-clip-text text-transparent;
  }

  .gradient-text-fuchsia {
    @apply bg-gradient-to-r from-[hsl(var(--accent-fuchsia))] to-[hsl(var(--accent-pink))] bg-clip-text text-transparent;
  }

  /* Terminal styles (dark mode only) */
  .dark .terminal-bg {
    @apply bg-[#141a26] font-mono text-sm;
  }

  .light .terminal-bg {
    @apply bg-[#eef1f7] font-mono text-sm border border-border;
  }

  /* Glass surfaces */
  .dark .glass-card {
    @apply bg-background/60 backdrop-blur-xl border border-white/5 shadow-[0_12px_40px_rgba(0,0,0,0.45)];
  }

  .light .glass-card {
    @apply bg-white/70 backdrop-blur-xl border border-black/5 shadow-[0_10px_28px_rgba(15,23,42,0.12)];
  }

  /* Glow effects - Intensity varies by theme */
  .dark .glow-violet-hover {
    @apply hover:shadow-[0_0_20px_rgba(139,92,246,0.4),0_0_40px_rgba(139,92,246,0.2)] transition-shadow duration-300;
  }

  .light .glow-violet-hover {
    @apply hover:shadow-[0_0_15px_rgba(139,92,246,0.2),0_0_30px_rgba(139,92,246,0.1)] transition-shadow duration-300;
  }

  .dark .glow-fuchsia-hover {
    @apply hover:shadow-[0_0_20px_rgba(217,70,239,0.4),0_0_40px_rgba(217,70,239,0.2)] transition-shadow duration-300;
  }

  .light .glow-fuchsia-hover {
    @apply hover:shadow-[0_0_15px_rgba(217,70,239,0.2),0_0_30px_rgba(217,70,239,0.1)] transition-shadow duration-300;
  }

  .dark .glow-purple-hover {
    @apply hover:shadow-[0_0_20px_rgba(147,51,234,0.4),0_0_40px_rgba(147,51,234,0.2)] transition-shadow duration-300;
  }

  .light .glow-purple-hover {
    @apply hover:shadow-[0_0_15px_rgba(147,51,234,0.2),0_0_30px_rgba(147,51,234,0.1)] transition-shadow duration-300;
  }

  /* Gradient border utilities */
  .gradient-border-violet-fuchsia::before {
    content: '';
    position: absolute;
    inset: -1px;
    border-radius: inherit;
    padding: 1px;
    background: linear-gradient(135deg, rgb(var(--rgb-violet)) 0%, rgb(var(--rgb-fuchsia)) 100%);
    -webkit-mask: linear-gradient(#fff 0 0) content-box, linear-gradient(#fff 0 0);
    -webkit-mask-composite: xor;
    mask-composite: exclude;
    opacity: 0.5;
    transition: opacity 0.3s;
  }

  .gradient-border-violet-fuchsia:hover::before {
    opacity: 1;
  }

  /* Scrollbar styles */
  .custom-scrollbar::-webkit-scrollbar {
    width: 8px;
    height: 8px;
  }

  .custom-scrollbar::-webkit-scrollbar-track {
    @apply bg-background;
  }

  .custom-scrollbar::-webkit-scrollbar-thumb {
    @apply bg-border rounded-full;
  }

  .custom-scrollbar::-webkit-scrollbar-thumb:hover {
    @apply bg-muted;
  }

  /* ────────────────────────────────────────────
     Infographic Canvas Utilities (F0012)
     Flat canvas layout — no panel borders, card
     wrappers, or divider lines. Zones are
     differentiated by spacing, typography, and
     color weight only.
     ──────────────────────────────────────────── */

  /* Base canvas section — full-bleed, borderless container.
     Use on every top-level content zone (nudge bar, KPI band,
     connected flow, chapter overlays, activity, my tasks). */
  .canvas-section {
    @apply w-full px-6 md:px-10 lg:px-16;
    border: none;
    box-shadow: none;
    border-radius: 0;
    background: transparent;
  }

  /* Tight spacing between logically related zones
     (e.g., story controls → KPI band → connected flow).
     Uses 12px (0.75rem) vertical gap. */
  .canvas-zone-tight {
    @apply py-3;
  }

  /* Standard spacing between sibling content zones.
     Uses 24px (1.5rem) vertical gap. */
  .canvas-zone-default {
    @apply py-6;
  }

  /* Break spacing between unrelated sections
     (e.g., story canvas → activity feed → my tasks).
     Uses 40px (2.5rem) vertical gap + subtle
     background-color shift for visual separation. */
  .canvas-zone-break {
    @apply py-10;
  }

  .dark .canvas-zone-break {
    background: hsl(228 22% 7%);  /* slightly lighter than --background */
  }

  .light .canvas-zone-break {
    background: hsl(225 30% 97%); /* slightly darker than --background */
  }

  /* Chapter overlay container — holds the active overlay
     visualization with crossfade transition. */
  .canvas-chapter-overlay {
    @apply relative min-h-[200px];
    transition: opacity 150ms ease-in-out;
  }

  /* Flow node emphasis states (Friction chapter) */
  .flow-emphasis-normal {
    @apply opacity-100;
  }

  .flow-emphasis-active {
    @apply ring-2 ring-accent-violet/50;
  }

  .dark .flow-emphasis-blocked {
    @apply ring-2 ring-accent-orange/60 bg-accent-orange/10;
  }

  .light .flow-emphasis-blocked {
    @apply ring-2 ring-accent-orange/40 bg-accent-orange/5;
  }

  .dark .flow-emphasis-bottleneck {
    @apply ring-2 ring-accent-fuchsia/60 bg-accent-fuchsia/10;
  }

  .light .flow-emphasis-bottleneck {
    @apply ring-2 ring-accent-fuchsia/40 bg-accent-fuchsia/5;
  }

  /* Smooth theme transitions */
  * {
    @apply transition-colors duration-200;
  }
}
```

### Canvas Spacing Tokens

The infographic canvas uses three spacing tiers to create visual hierarchy without borders or dividers:

| Token                  | Vertical padding | Usage                                                    |
|------------------------|------------------|----------------------------------------------------------|
| `canvas-zone-tight`    | 12px (0.75rem)   | Between related zones (controls → KPIs → flow)           |
| `canvas-zone-default`  | 24px (1.5rem)    | Between sibling content zones within the story canvas     |
| `canvas-zone-break`    | 40px (2.5rem)    | Between major sections (story canvas → activity → tasks) |

Horizontal padding scales with breakpoint via `canvas-section`: 24px mobile → 40px tablet → 64px desktop.

### Canvas Color Architecture

Zone separation uses background-color micro-shifts instead of borders:

```
Dark theme:
  --background:           hsl(228 22% 6%)   ← primary canvas
  canvas-zone-break bg:   hsl(228 22% 7%)   ← +1% lightness for break zones

Light theme:
  --background:           hsl(225 30% 98%)  ← primary canvas
  canvas-zone-break bg:   hsl(225 30% 97%)  ← -1% lightness for break zones
```

The 1% lightness delta is perceptible but not jarring — enough to signal a section boundary without introducing visual noise.

---

## Tailwind Configuration

```js
/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: ['class'],
  content: [
    './pages/**/*.{ts,tsx}',
    './components/**/*.{ts,tsx}',
    './app/**/*.{ts,tsx}',
    './src/**/*.{ts,tsx}',
  ],
  theme: {
    container: {
      center: true,
      padding: '2rem',
      screens: {
        '2xl': '1400px',
      },
    },
    extend: {
      colors: {
        border: 'hsl(var(--border))',
        input: 'hsl(var(--input))',
        ring: 'hsl(var(--ring))',
        background: 'hsl(var(--background))',
        foreground: 'hsl(var(--foreground))',
        primary: {
          DEFAULT: 'hsl(var(--primary))',
          foreground: 'hsl(var(--primary-foreground))',
        },
        secondary: {
          DEFAULT: 'hsl(var(--secondary))',
          foreground: 'hsl(var(--secondary-foreground))',
        },
        destructive: {
          DEFAULT: 'hsl(var(--destructive))',
          foreground: 'hsl(var(--destructive-foreground))',
        },
        muted: {
          DEFAULT: 'hsl(var(--muted))',
          foreground: 'hsl(var(--muted-foreground))',
        },
        accent: {
          DEFAULT: 'hsl(var(--accent))',
          foreground: 'hsl(var(--accent-foreground))',
        },
        popover: {
          DEFAULT: 'hsl(var(--popover))',
          foreground: 'hsl(var(--popover-foreground))',
        },
        card: {
          DEFAULT: 'hsl(var(--card))',
          foreground: 'hsl(var(--card-foreground))',
        },
        // Theme-independent accent colors
        'accent-violet': 'hsl(var(--accent-violet))',
        'accent-fuchsia': 'hsl(var(--accent-fuchsia))',
        'accent-purple': 'hsl(var(--accent-purple))',
        'accent-pink': 'hsl(var(--accent-pink))',
        'accent-orange': 'hsl(var(--accent-orange))',
      },
      borderRadius: {
        lg: 'var(--radius)',
        md: 'calc(var(--radius) - 2px)',
        sm: 'calc(var(--radius) - 4px)',
      },
      backgroundImage: {
        'gradient-violet-purple': 'linear-gradient(135deg, rgb(var(--rgb-violet)) 0%, rgb(var(--rgb-purple)) 100%)',
        'gradient-violet-fuchsia': 'linear-gradient(135deg, rgb(var(--rgb-violet)) 0%, rgb(var(--rgb-fuchsia)) 100%)',
        'gradient-purple-pink': 'linear-gradient(135deg, rgb(var(--rgb-purple)) 0%, rgb(var(--rgb-pink)) 100%)',
        'gradient-fuchsia-violet': 'linear-gradient(135deg, rgb(var(--rgb-fuchsia)) 0%, rgb(var(--rgb-violet)) 100%)',
      },
      keyframes: {
        'gradient-shift': {
          '0%, 100%': { backgroundPosition: '0% 50%' },
          '50%': { backgroundPosition: '100% 50%' },
        },
      },
      animation: {
        'gradient-shift': 'gradient-shift 3s ease infinite',
      },
      fontFamily: {
        sans: ['Sora', 'system-ui', 'sans-serif'],
        mono: ['JetBrains Mono', 'Fira Code', 'monospace'],
      },
    },
  },
  plugins: [require('tailwindcss-animate')],
};
```

---

## Theme Switcher Component

```tsx
// components/theme-switcher.tsx
import { Moon, Sun } from 'lucide-react';
import { useTheme } from 'next-themes';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

export function ThemeSwitcher() {
  const { setTheme, theme } = useTheme();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="relative">
          <Sun className="h-5 w-5 rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0" />
          <Moon className="absolute h-5 w-5 rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100" />
          <span className="sr-only">Toggle theme</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem onClick={() => setTheme('light')}>
          <Sun className="mr-2 h-4 w-4" />
          Light
        </DropdownMenuItem>
        <DropdownMenuItem onClick={() => setTheme('dark')}>
          <Moon className="mr-2 h-4 w-4" />
          Dark
        </DropdownMenuItem>
        <DropdownMenuItem onClick={() => setTheme('system')}>
          <span className="mr-2 h-4 w-4">💻</span>
          System
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
```

### Theme Provider Setup

```tsx
// app/providers.tsx
'use client';

import { ThemeProvider } from 'next-themes';

export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <ThemeProvider
      attribute="class"
      defaultTheme="system"
      enableSystem
      disableTransitionOnChange
    >
      {children}
    </ThemeProvider>
  );
}

// app/layout.tsx
import { Providers } from './providers';

export default function RootLayout({ children }) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body>
        <Providers>
          {children}
        </Providers>
      </body>
    </html>
  );
}
```

---

## Components with Gradient Glow Hover

### Terminal Card with Gradient Border & Glow

```tsx
// components/terminal-card.tsx
import { cn } from '@/lib/utils';

interface TerminalCardProps {
  title?: string;
  icon?: React.ReactNode;
  children: React.ReactNode;
  variant?: 'violet-purple' | 'violet-fuchsia' | 'purple-pink' | 'fuchsia-violet';
  className?: string;
}

export function TerminalCard({
  title,
  icon,
  children,
  variant = 'violet-purple',
  className,
}: TerminalCardProps) {
  const gradients = {
    'violet-purple': 'from-accent-violet to-accent-purple',
    'violet-fuchsia': 'from-accent-violet to-accent-fuchsia',
    'purple-pink': 'from-accent-purple to-accent-pink',
    'fuchsia-violet': 'from-accent-fuchsia via-accent-purple to-accent-violet',
  };

  const glows = {
    'violet-purple': 'glow-violet-hover',
    'violet-fuchsia': 'glow-fuchsia-hover',
    'purple-pink': 'glow-purple-hover',
    'fuchsia-violet': 'glow-fuchsia-hover',
  };

  return (
    <div className={cn('relative group', className)}>
      {/* Gradient border - subtle by default, brighter on hover */}
      <div
        className={cn(
          'absolute -inset-[1px] rounded-2xl bg-gradient-to-br opacity-0 group-hover:opacity-75 blur-[2px] transition-opacity duration-300',
          gradients[variant]
        )}
      />

      {/* Glow effect on hover */}
      <div
        className={cn(
          'absolute -inset-[1px] rounded-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-300',
          glows[variant]
        )}
      />

      {/* Card content */}
      <div className="relative rounded-2xl bg-card border border-border overflow-hidden">
        {/* Terminal header */}
        <div className="flex items-center gap-3 px-4 py-3 border-b border-border bg-background/50">
          <div className="flex gap-1.5">
            <div className="h-3 w-3 rounded-full bg-red-500/20 border border-red-500/50" />
            <div className="h-3 w-3 rounded-full bg-yellow-500/20 border border-yellow-500/50" />
            <div className="h-3 w-3 rounded-full bg-green-500/20 border border-green-500/50" />
          </div>
          {icon && title && (
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              {icon}
              <span>{title}</span>
            </div>
          )}
        </div>

        {/* Content */}
        <div className="p-4 terminal-bg">
          {children}
        </div>
      </div>
    </div>
  );
}
```

### Gradient Card with Hover Glow

```tsx
// components/gradient-card.tsx
import { cn } from '@/lib/utils';

interface GradientCardProps {
  children: React.ReactNode;
  variant?: 'violet-purple' | 'violet-fuchsia' | 'purple-pink';
  className?: string;
  withGlow?: boolean;
}

export function GradientCard({
  children,
  variant = 'violet-purple',
  className,
  withGlow = true,
}: GradientCardProps) {
  const gradients = {
    'violet-purple': 'from-accent-violet via-accent-purple to-accent-pink',
    'violet-fuchsia': 'from-accent-violet to-accent-fuchsia',
    'purple-pink': 'from-accent-purple to-accent-pink',
  };

  const glows = {
    'violet-purple': 'glow-violet-hover',
    'violet-fuchsia': 'glow-fuchsia-hover',
    'purple-pink': 'glow-purple-hover',
  };

  return (
    <div className={cn('relative group', className)}>
      {/* Gradient border - animated on hover */}
      <div
        className={cn(
          'absolute -inset-[1px] rounded-xl bg-gradient-to-r opacity-50 group-hover:opacity-100 blur-sm transition-opacity duration-300',
          'animate-gradient-shift bg-[length:200%_200%]',
          gradients[variant]
        )}
      />

      {/* Glow effect */}
      {withGlow && (
        <div
          className={cn(
            'absolute -inset-[1px] rounded-xl opacity-0 group-hover:opacity-100 transition-opacity duration-300',
            glows[variant]
          )}
        />
      )}

      {/* Card content */}
      <div className="relative rounded-xl bg-card border border-border p-6">
        {children}
      </div>
    </div>
  );
}
```

### Input with Gradient Focus & Subtle Glow

```tsx
// components/ui/input.tsx
import * as React from 'react';
import { cn } from '@/lib/utils';

export interface InputProps
  extends React.InputHTMLAttributes<HTMLInputElement> {
  withGradientFocus?: boolean;
}

const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ className, type, withGradientFocus = true, ...props }, ref) => {
    if (!withGradientFocus) {
      return (
        <input
          type={type}
          className={cn(
            'flex h-10 w-full rounded-lg border border-input',
            'bg-background px-3 py-2 text-sm ring-offset-background',
            'file:border-0 file:bg-transparent file:text-sm file:font-medium',
            'placeholder:text-muted-foreground',
            'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
            'disabled:cursor-not-allowed disabled:opacity-50',
            className
          )}
          ref={ref}
          {...props}
        />
      );
    }

    return (
      <div className="relative group">
        {/* Gradient glow on focus */}
        <div className="absolute -inset-[1px] rounded-lg bg-gradient-to-r from-accent-violet to-accent-fuchsia opacity-0 group-focus-within:opacity-100 transition-opacity duration-300 blur-sm" />

        <input
          type={type}
          className={cn(
            'relative flex h-10 w-full rounded-lg border border-input',
            'bg-background px-3 py-2 text-sm ring-offset-background',
            'file:border-0 file:bg-transparent file:text-sm file:font-medium',
            'placeholder:text-muted-foreground',
            'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-accent-violet focus-visible:ring-offset-2',
            'focus-visible:border-accent-violet/50',
            'disabled:cursor-not-allowed disabled:opacity-50',
            'transition-colors',
            className
          )}
          ref={ref}
          {...props}
        />
      </div>
    );
  }
);
Input.displayName = 'Input';

export { Input };
```

### Button with Gradient & Glow

```tsx
// components/ui/button.tsx (extended variants)
import { cva, type VariantProps } from 'class-variance-authority';

const buttonVariants = cva(
  'inline-flex items-center justify-center rounded-lg text-sm font-medium transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50',
  {
    variants: {
      variant: {
        default: 'bg-primary text-primary-foreground hover:bg-primary/90',
        destructive: 'bg-destructive text-destructive-foreground hover:bg-destructive/90',
        outline: 'border border-input bg-background hover:bg-accent hover:text-accent-foreground',
        secondary: 'bg-secondary text-secondary-foreground hover:bg-secondary/80',
        ghost: 'hover:bg-accent hover:text-accent-foreground',
        link: 'text-primary underline-offset-4 hover:underline',

        // Gradient variants with glow on hover
        gradient: 'bg-gradient-to-r from-accent-violet to-accent-fuchsia text-white glow-violet-hover',
        'gradient-fuchsia': 'bg-gradient-to-r from-accent-fuchsia to-accent-pink text-white glow-fuchsia-hover',
        'gradient-purple': 'bg-gradient-to-r from-accent-purple to-accent-pink text-white glow-purple-hover',
      },
      size: {
        default: 'h-10 px-4 py-2',
        sm: 'h-9 rounded-md px-3',
        lg: 'h-11 rounded-lg px-8',
        icon: 'h-10 w-10',
      },
    },
    defaultVariants: {
      variant: 'default',
      size: 'default',
    },
  }
);

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean;
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, asChild = false, ...props }, ref) => {
    const Comp = asChild ? Slot : 'button';
    return (
      <Comp
        className={cn(buttonVariants({ variant, size, className }))}
        ref={ref}
        {...props}
      />
    );
  }
);
Button.displayName = 'Button';

export { Button, buttonVariants };
```

### Icon Badge with Glow

```tsx
// components/icon-badge.tsx
import { cn } from '@/lib/utils';
import { LucideIcon } from 'lucide-react';

interface IconBadgeProps {
  icon: LucideIcon;
  variant?: 'violet' | 'fuchsia' | 'purple' | 'orange';
  size?: 'sm' | 'md' | 'lg';
  withGlow?: boolean;
}

export function IconBadge({
  icon: Icon,
  variant = 'violet',
  size = 'md',
  withGlow = true,
}: IconBadgeProps) {
  const sizes = {
    sm: 'h-8 w-8',
    md: 'h-10 w-10',
    lg: 'h-12 w-12',
  };

  const iconSizes = {
    sm: 'h-4 w-4',
    md: 'h-5 w-5',
    lg: 'h-6 w-6',
  };

  const variants = {
    violet: 'bg-accent-violet/10 border-accent-violet/30 text-accent-violet',
    fuchsia: 'bg-accent-fuchsia/10 border-accent-fuchsia/30 text-accent-fuchsia',
    purple: 'bg-accent-purple/10 border-accent-purple/30 text-accent-purple',
    orange: 'bg-accent-orange/10 border-accent-orange/30 text-accent-orange',
  };

  const glows = {
    violet: 'glow-violet-hover',
    fuchsia: 'glow-fuchsia-hover',
    purple: 'glow-purple-hover',
    orange: 'hover:shadow-[0_0_15px_rgba(249,115,22,0.3)]',
  };

  return (
    <div
      className={cn(
        'rounded-full border flex items-center justify-center transition-all',
        sizes[size],
        variants[variant],
        withGlow && glows[variant]
      )}
    >
      <Icon className={iconSizes[size]} />
    </div>
  );
}
```

---

## Example Usage

### Dashboard with Theme Support

```tsx
import { TerminalCard } from '@/components/terminal-card';
import { GradientCard } from '@/components/gradient-card';
import { Button } from '@/components/ui/button';
import { ThemeSwitcher } from '@/components/theme-switcher';
import { IconBadge } from '@/components/icon-badge';
import { Package, FileText, BookOpen } from 'lucide-react';

export default function Dashboard() {
  return (
    <div className="min-h-screen bg-background p-6">
      <div className="mx-auto max-w-7xl space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <h1 className="text-3xl font-bold gradient-text">
            Nebula Dashboard
          </h1>
          <div className="flex items-center gap-3">
            <Button variant="gradient">Create Submission</Button>
            <ThemeSwitcher />
          </div>
        </div>

        {/* Terminal Cards Grid */}
        <div className="grid gap-6 md:grid-cols-3">
          <TerminalCard
            title="apps/web"
            icon={<IconBadge icon={Package} variant="violet" size="sm" withGlow={false} />}
            variant="violet-purple"
          >
            <div className="space-y-2 text-foreground/90">
              <div className="text-sm">npm run lint && npm run build</div>
              <div className="text-xs text-muted-foreground">Done in 110.2s</div>
            </div>
          </TerminalCard>

          <TerminalCard
            title="packages/shared"
            icon={<IconBadge icon={FileText} variant="fuchsia" size="sm" withGlow={false} />}
            variant="violet-fuchsia"
          >
            <div className="space-y-2 text-foreground/90">
              <div className="text-sm">npm run test && npm run build</div>
              <div className="text-xs text-muted-foreground">Done in 90.8s</div>
            </div>
          </TerminalCard>

          <TerminalCard
            title="apps/docs"
            icon={<IconBadge icon={BookOpen} variant="purple" size="sm" withGlow={false} />}
            variant="purple-pink"
          >
            <div className="space-y-2 text-foreground/90">
              <div className="text-sm">npm run deploy</div>
              <div className="text-xs text-muted-foreground">Done in 140.2s</div>
            </div>
          </TerminalCard>
        </div>

        {/* Content Cards */}
        <div className="grid gap-6 md:grid-cols-2">
          <GradientCard variant="violet-purple">
            <h3 className="text-lg font-semibold mb-2">Recent Submissions</h3>
            <p className="text-sm text-muted-foreground">
              Track and manage insurance submissions with real-time updates.
            </p>
          </GradientCard>

          <GradientCard variant="violet-fuchsia">
            <h3 className="text-lg font-semibold mb-2">Broker Network</h3>
            <p className="text-sm text-muted-foreground">
              Manage relationships with brokers and MGAs across all regions.
            </p>
          </GradientCard>
        </div>
      </div>
    </div>
  );
}
```

### Form with Gradient Focus States

```tsx
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { GradientCard } from '@/components/gradient-card';

export function BrokerForm() {
  return (
    <GradientCard variant="violet-purple">
      <h2 className="text-xl font-semibold mb-6">Create Broker</h2>

      <form className="space-y-6">
        <div className="space-y-2">
          <Label htmlFor="name">Broker Name</Label>
          <Input
            id="name"
            placeholder="ABC Insurance Brokers"
            withGradientFocus
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="license">License Number</Label>
          <Input
            id="license"
            placeholder="CA0123456"
            className="font-mono"
            withGradientFocus
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="email">Email</Label>
          <Input
            id="email"
            type="email"
            placeholder="contact@abc.com"
            withGradientFocus
          />
        </div>

        <div className="flex gap-3">
          <Button variant="outline" className="flex-1">
            Cancel
          </Button>
          <Button variant="gradient" className="flex-1">
            Create Broker
          </Button>
        </div>
      </form>
    </GradientCard>
  );
}
```

---

## Theme-Specific Adjustments

### Glow Intensity by Theme

The glow effects are **more intense in dark mode** (better visibility) and **subtle in light mode** (prevent overwhelming the UI):

```css
/* Dark mode - brighter glow */
.dark .glow-violet-hover {
  @apply hover:shadow-[0_0_20px_rgba(139,92,246,0.4),0_0_40px_rgba(139,92,246,0.2)];
}

/* Light mode - subtle glow */
.light .glow-violet-hover {
  @apply hover:shadow-[0_0_15px_rgba(139,92,246,0.2),0_0_30px_rgba(139,92,246,0.1)];
}
```

### Border Contrast

```tsx
// Dark mode uses subtle borders
.dark {
  --border: 228 16% 18%;  /* Deep graphite */
}

// Light mode uses visible but not harsh borders
.light {
  --border: 220 16% 86%;  /* Soft cool gray */
}
```

---

## Best Practices

### 1. Use Glow Effects Sparingly

✅ **GOOD:** Cards, buttons, important CTAs
```tsx
<Button variant="gradient">Primary Action</Button>
<GradientCard>Important content</GradientCard>
```

❌ **BAD:** Every element
```tsx
<div className="glow-violet-hover">
  <p className="glow-fuchsia-hover">Too much!</p>
</div>
```

### 2. Match Glow to Gradient

```tsx
// ✅ GOOD - Coordinated colors
<TerminalCard variant="violet-purple"> {/* Uses violet glow */}

// ❌ BAD - Mismatched
<TerminalCard variant="violet-purple" className="glow-fuchsia-hover">
```

### 3. Ensure Readability in Both Themes

Always test text contrast:
- Dark mode: Light text on dark backgrounds
- Light mode: Dark text on light backgrounds

```tsx
// ✅ GOOD - Uses theme-aware text colors
<p className="text-foreground">Always readable</p>
<p className="text-muted-foreground">Secondary text</p>

// ❌ BAD - Hard-coded colors
<p className="text-white">Only readable in dark mode</p>
```

### 4. Smooth Theme Transitions

The global transition is set in `globals.css`:

```css
* {
  @apply transition-colors duration-200;
}
```

This ensures all color changes animate smoothly when switching themes.

---

## Accessibility

### Color Contrast

Target WCAG AA for all text/background combinations. Verify after applying tokens:
- Foreground vs background
- Muted text vs background
- CTA text vs gradient backgrounds

### Focus Indicators

All interactive elements have visible focus rings:

```tsx
<Button className="focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2" />
```

### Theme Preference Respect

Respect user's system theme preference:

```tsx
<ThemeProvider defaultTheme="system" enableSystem>
```

---

## Installation Checklist

- [ ] Install dependencies: `npm install next-themes tailwindcss-animate`
- [ ] Copy color variables to `globals.css`
- [ ] Update `tailwind.config.js` with extended theme
- [ ] Add `ThemeProvider` to root layout
- [ ] Create `ThemeSwitcher` component
- [ ] Test both light and dark modes
- [ ] Verify glow effects work in both themes
- [ ] Check color contrast with accessibility tools
- [ ] Test theme switching animation
- [ ] Ensure all components respect theme

---

## References

- [next-themes Documentation](https://github.com/pacocoursey/next-themes)
- [Tailwind CSS Dark Mode](https://tailwindcss.com/docs/dark-mode)
- [shadcn/ui Theming](https://ui.shadcn.com/docs/theming)
- [WCAG Contrast Checker](https://webaim.org/resources/contrastchecker/)
