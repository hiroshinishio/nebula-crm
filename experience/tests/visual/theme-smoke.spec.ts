import { expect, test, type Page, type TestInfo } from '@playwright/test'

type Theme = 'dark' | 'light'

const THEMES: Theme[] = ['dark', 'light']

const PAGES = [
  { path: '/', title: 'Dashboard' },
  { path: '/brokers', title: 'Brokers' },
  { path: '/brokers/new', title: 'New Broker' },
] as const

test.beforeEach(async ({ page }) => {
  await page.emulateMedia({ reducedMotion: 'reduce' })
  await mockNebulaApis(page)
})

for (const theme of THEMES) {
  test.describe(`${theme} theme`, () => {
    for (const pageCase of PAGES) {
      test(`${pageCase.path} renders and captures screenshot`, async ({ page }, testInfo) => {
        await openAppPage(page, pageCase.path, theme)
        await expect(page.getByRole('heading', { name: pageCase.title })).toBeVisible()
        await attachPageScreenshot(page, testInfo, pageCase.path, theme)
      })
    }

    test('dashboard KPI card text remains readable', async ({ page }) => {
      await openAppPage(page, '/', theme)
      await expect(page.getByText('Active Brokers')).toBeVisible()

      const metrics = await page.evaluate(() => {
        const label = Array.from(document.querySelectorAll('p')).find(
          (el) => el.textContent?.trim() === 'Active Brokers',
        ) as HTMLElement | undefined

        if (!label) {
          throw new Error('KPI label "Active Brokers" not found')
        }

        const value = label.parentElement?.querySelector('p.mt-1') as HTMLElement | null
        const card = label.closest('.glass-card') as HTMLElement | null

        if (!value || !card) {
          throw new Error('KPI value/card not found')
        }

        return {
          labelColor: getComputedStyle(label).color,
          valueColor: getComputedStyle(value).color,
          cardBg: getComputedStyle(card).backgroundColor,
          bodyBg: getComputedStyle(document.body).backgroundColor,
        }
      })

      const cardBg = compositeOver(parseCssColor(metrics.cardBg), parseCssColor(metrics.bodyBg))
      const labelContrast = contrastRatio(parseCssColor(metrics.labelColor), cardBg)
      const valueContrast = contrastRatio(parseCssColor(metrics.valueColor), cardBg)

      // Label text is intentionally de-emphasized; keep it above a minimum contrast floor.
      expect(labelContrast).toBeGreaterThan(3)
      expect(valueContrast).toBeGreaterThan(4.5)
    })
  })
}

async function openAppPage(page: Page, path: string, theme: Theme) {
  await page.addInitScript((selectedTheme) => {
    localStorage.setItem('nebula-theme', selectedTheme)
  }, theme)

  await page.goto(path)
  await page.waitForLoadState('networkidle')

  await page.addStyleTag({
    content: `
      *, *::before, *::after {
        animation: none !important;
        transition: none !important;
        caret-color: transparent !important;
      }
    `,
  })
}

async function attachPageScreenshot(page: Page, testInfo: TestInfo, routePath: string, theme: Theme) {
  const slug = routePath === '/' ? 'dashboard' : routePath.replaceAll('/', '-').replace(/^-/, '')
  const screenshotPath = testInfo.outputPath(`${slug}-${theme}.png`)

  await page.screenshot({
    path: screenshotPath,
    fullPage: true,
    animations: 'disabled',
    caret: 'hide',
  })

  await testInfo.attach(`${slug}-${theme}`, {
    path: screenshotPath,
    contentType: 'image/png',
  })
}

async function mockNebulaApis(page: Page) {
  await page.route(
    '**/realms/nebula/protocol/openid-connect/token',
    async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          access_token: 'test-token',
          expires_in: 3600,
          token_type: 'Bearer',
        }),
      })
    },
  )

  await page.route('**/dashboard/kpis', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        activeBrokers: 128,
        openSubmissions: 42,
        renewalRate: 0.83,
        avgTurnaroundDays: 4.6,
      }),
    })
  })

  await page.route('**/dashboard/nudges', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        nudges: [
          {
            nudgeType: 'OverdueTask',
            title: 'Follow up on missing loss runs',
            description: 'Broker A12 has an overdue task blocking submission review.',
            linkedEntityType: 'Task',
            linkedEntityId: 'task-1',
            linkedEntityName: 'Follow-up task',
            urgencyValue: 95,
            ctaLabel: 'Open Task',
          },
          {
            nudgeType: 'StaleSubmission',
            title: 'Submission idle for 9 days',
            description: 'Submission S-2048 has no recent activity.',
            linkedEntityType: 'Submission',
            linkedEntityId: 'sub-2048',
            linkedEntityName: 'Submission S-2048',
            urgencyValue: 72,
            ctaLabel: 'Open Submission',
          },
        ],
      }),
    })
  })

  await page.route('**/dashboard/opportunities/flow?*', async (route) => {
    const url = new URL(route.request().url())
    const entityType = url.searchParams.get('entityType')

    const body = entityType === 'renewal'
      ? {
          entityType: 'renewal',
          periodDays: 180,
          windowStartUtc: '2025-09-01T00:00:00Z',
          windowEndUtc: '2026-02-28T00:00:00Z',
          nodes: [
            { status: 'Created', label: 'Created', isTerminal: false, displayOrder: 1, colorGroup: 'intake', currentCount: 11, inflowCount: 0, outflowCount: 14 },
            { status: 'DataReview', label: 'Data Review', isTerminal: false, displayOrder: 3, colorGroup: 'triage', currentCount: 8, inflowCount: 10, outflowCount: 9 },
            { status: 'OutreachStarted', label: 'Outreach Started', isTerminal: false, displayOrder: 4, colorGroup: 'waiting', currentCount: 7, inflowCount: 11, outflowCount: 10 },
            { status: 'WaitingOnBroker', label: 'Waiting on Broker', isTerminal: false, displayOrder: 5, colorGroup: 'waiting', currentCount: 6, inflowCount: 7, outflowCount: 5 },
            { status: 'InReview', label: 'In Review', isTerminal: false, displayOrder: 6, colorGroup: 'review', currentCount: 7, inflowCount: 8, outflowCount: 8 },
            { status: 'Quoted', label: 'Quoted', isTerminal: false, displayOrder: 7, colorGroup: 'decision', currentCount: 5, inflowCount: 7, outflowCount: 6 },
            { status: 'Negotiation', label: 'Negotiation', isTerminal: false, displayOrder: 8, colorGroup: 'decision', currentCount: 4, inflowCount: 4, outflowCount: 4 },
            { status: 'BindRequested', label: 'Bind Requested', isTerminal: false, displayOrder: 9, colorGroup: 'decision', currentCount: 3, inflowCount: 4, outflowCount: 3 },
            { status: 'Bound', label: 'Bound', isTerminal: true, displayOrder: 10, colorGroup: 'decision', currentCount: 12, inflowCount: 9, outflowCount: 0 },
            { status: 'NotRenewed', label: 'Not Renewed', isTerminal: true, displayOrder: 11, colorGroup: 'decision', currentCount: 9, inflowCount: 6, outflowCount: 0 },
            { status: 'Lost', label: 'Lost', isTerminal: true, displayOrder: 12, colorGroup: 'decision', currentCount: 6, inflowCount: 4, outflowCount: 0 },
            { status: 'Lapsed', label: 'Lapsed', isTerminal: true, displayOrder: 13, colorGroup: 'decision', currentCount: 3, inflowCount: 2, outflowCount: 0 },
          ],
          links: [
            { sourceStatus: 'Created', targetStatus: 'DataReview', count: 10 },
            { sourceStatus: 'Created', targetStatus: 'OutreachStarted', count: 4 },
            { sourceStatus: 'DataReview', targetStatus: 'OutreachStarted', count: 5 },
            { sourceStatus: 'DataReview', targetStatus: 'WaitingOnBroker', count: 2 },
            { sourceStatus: 'DataReview', targetStatus: 'InReview', count: 2 },
            { sourceStatus: 'OutreachStarted', targetStatus: 'WaitingOnBroker', count: 5 },
            { sourceStatus: 'OutreachStarted', targetStatus: 'InReview', count: 3 },
            { sourceStatus: 'OutreachStarted', targetStatus: 'Quoted', count: 2 },
            { sourceStatus: 'WaitingOnBroker', targetStatus: 'InReview', count: 3 },
            { sourceStatus: 'WaitingOnBroker', targetStatus: 'Quoted', count: 2 },
            { sourceStatus: 'InReview', targetStatus: 'Quoted', count: 5 },
            { sourceStatus: 'InReview', targetStatus: 'Negotiation', count: 3 },
            { sourceStatus: 'Quoted', targetStatus: 'Negotiation', count: 2 },
            { sourceStatus: 'Quoted', targetStatus: 'BindRequested', count: 3 },
            { sourceStatus: 'Quoted', targetStatus: 'Bound', count: 1 },
            { sourceStatus: 'Negotiation', targetStatus: 'BindRequested', count: 2 },
            { sourceStatus: 'Negotiation', targetStatus: 'Lost', count: 2 },
            { sourceStatus: 'BindRequested', targetStatus: 'Bound', count: 3 },
            { sourceStatus: 'OutreachStarted', targetStatus: 'NotRenewed', count: 2 },
            { sourceStatus: 'WaitingOnBroker', targetStatus: 'Lapsed', count: 2 },
            { sourceStatus: 'Quoted', targetStatus: 'NotRenewed', count: 2 },
            { sourceStatus: 'DataReview', targetStatus: 'Lost', count: 2 },
          ],
        }
      : {
          entityType: 'submission',
          periodDays: 180,
          windowStartUtc: '2025-09-01T00:00:00Z',
          windowEndUtc: '2026-02-28T00:00:00Z',
          nodes: [
            { status: 'Received', label: 'Received', isTerminal: false, displayOrder: 1, colorGroup: 'intake', currentCount: 9, inflowCount: 0, outflowCount: 18 },
            { status: 'Triaging', label: 'Triaging', isTerminal: false, displayOrder: 2, colorGroup: 'triage', currentCount: 7, inflowCount: 14, outflowCount: 14 },
            { status: 'WaitingOnBroker', label: 'Waiting on Broker', isTerminal: false, displayOrder: 3, colorGroup: 'waiting', currentCount: 6, inflowCount: 7, outflowCount: 6 },
            { status: 'WaitingOnDocuments', label: 'Waiting on Documents', isTerminal: false, displayOrder: 4, colorGroup: 'waiting', currentCount: 5, inflowCount: 5, outflowCount: 4 },
            { status: 'ReadyForUWReview', label: 'Ready for UW Review', isTerminal: false, displayOrder: 5, colorGroup: 'review', currentCount: 4, inflowCount: 7, outflowCount: 5 },
            { status: 'InReview', label: 'In Review', isTerminal: false, displayOrder: 6, colorGroup: 'review', currentCount: 4, inflowCount: 7, outflowCount: 8 },
            { status: 'QuotePreparation', label: 'Quote Preparation', isTerminal: false, displayOrder: 7, colorGroup: 'decision', currentCount: 3, inflowCount: 5, outflowCount: 5 },
            { status: 'Quoted', label: 'Quoted', isTerminal: false, displayOrder: 8, colorGroup: 'decision', currentCount: 3, inflowCount: 7, outflowCount: 8 },
            { status: 'RequoteRequested', label: 'Requote Requested', isTerminal: false, displayOrder: 9, colorGroup: 'decision', currentCount: 2, inflowCount: 3, outflowCount: 3 },
            { status: 'BindRequested', label: 'Bind Requested', isTerminal: false, displayOrder: 10, colorGroup: 'decision', currentCount: 2, inflowCount: 4, outflowCount: 4 },
            { status: 'Binding', label: 'Binding', isTerminal: false, displayOrder: 11, colorGroup: 'decision', currentCount: 1, inflowCount: 2, outflowCount: 2 },
            { status: 'Bound', label: 'Bound', isTerminal: true, displayOrder: 12, colorGroup: 'decision', currentCount: 12, inflowCount: 6, outflowCount: 0 },
            { status: 'Declined', label: 'Declined', isTerminal: true, displayOrder: 13, colorGroup: 'decision', currentCount: 8, inflowCount: 5, outflowCount: 0 },
            { status: 'Withdrawn', label: 'Withdrawn', isTerminal: true, displayOrder: 14, colorGroup: 'decision', currentCount: 6, inflowCount: 4, outflowCount: 0 },
            { status: 'NotQuoted', label: 'Not Quoted', isTerminal: true, displayOrder: 15, colorGroup: 'decision', currentCount: 4, inflowCount: 3, outflowCount: 0 },
            { status: 'Lost', label: 'Lost', isTerminal: true, displayOrder: 16, colorGroup: 'decision', currentCount: 5, inflowCount: 4, outflowCount: 0 },
          ],
          links: [
            { sourceStatus: 'Received', targetStatus: 'Triaging', count: 14 },
            { sourceStatus: 'Received', targetStatus: 'Withdrawn', count: 2 },
            { sourceStatus: 'Received', targetStatus: 'NotQuoted', count: 2 },
            { sourceStatus: 'Triaging', targetStatus: 'WaitingOnBroker', count: 5 },
            { sourceStatus: 'Triaging', targetStatus: 'WaitingOnDocuments', count: 4 },
            { sourceStatus: 'Triaging', targetStatus: 'ReadyForUWReview', count: 5 },
            { sourceStatus: 'WaitingOnBroker', targetStatus: 'WaitingOnDocuments', count: 3 },
            { sourceStatus: 'WaitingOnBroker', targetStatus: 'ReadyForUWReview', count: 3 },
            { sourceStatus: 'WaitingOnDocuments', targetStatus: 'ReadyForUWReview', count: 4 },
            { sourceStatus: 'ReadyForUWReview', targetStatus: 'InReview', count: 5 },
            { sourceStatus: 'InReview', targetStatus: 'QuotePreparation', count: 4 },
            { sourceStatus: 'InReview', targetStatus: 'Declined', count: 3 },
            { sourceStatus: 'InReview', targetStatus: 'Quoted', count: 1 },
            { sourceStatus: 'QuotePreparation', targetStatus: 'Quoted', count: 5 },
            { sourceStatus: 'Quoted', targetStatus: 'RequoteRequested', count: 3 },
            { sourceStatus: 'Quoted', targetStatus: 'BindRequested', count: 4 },
            { sourceStatus: 'Quoted', targetStatus: 'Lost', count: 1 },
            { sourceStatus: 'RequoteRequested', targetStatus: 'QuotePreparation', count: 2 },
            { sourceStatus: 'RequoteRequested', targetStatus: 'Quoted', count: 1 },
            { sourceStatus: 'BindRequested', targetStatus: 'Binding', count: 2 },
            { sourceStatus: 'BindRequested', targetStatus: 'Bound', count: 2 },
            { sourceStatus: 'Binding', targetStatus: 'Bound', count: 2 },
            { sourceStatus: 'WaitingOnBroker', targetStatus: 'Withdrawn', count: 2 },
            { sourceStatus: 'QuotePreparation', targetStatus: 'NotQuoted', count: 1 },
            { sourceStatus: 'ReadyForUWReview', targetStatus: 'Declined', count: 2 },
          ],
        }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(body),
    })
  })

  await page.route('**/dashboard/opportunities', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        submissions: [
          { status: 'Received', count: 9, colorGroup: 'intake' },
          { status: 'Triaging', count: 7, colorGroup: 'triage' },
          { status: 'WaitingOnBroker', count: 4, colorGroup: 'waiting' },
          { status: 'InReview', count: 3, colorGroup: 'review' },
          { status: 'Quoted', count: 2, colorGroup: 'decision' },
        ],
        renewals: [
          { status: 'Created', count: 11, colorGroup: 'intake' },
          { status: 'DataReview', count: 8, colorGroup: 'triage' },
          { status: 'OutreachStarted', count: 7, colorGroup: 'waiting' },
          { status: 'InReview', count: 7, colorGroup: 'review' },
          { status: 'Quoted', count: 5, colorGroup: 'decision' },
        ],
      }),
    })
  })

  await page.route('**/my/tasks?*', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        tasks: [
          {
            id: 'task-1',
            title: 'Request updated application',
            status: 'InProgress',
            dueDate: '2026-03-05T12:00:00Z',
            linkedEntityType: 'Submission',
            linkedEntityId: 'sub-2048',
            linkedEntityName: 'Submission S-2048',
            isOverdue: false,
          },
          {
            id: 'task-2',
            title: 'Call broker for quote clarification',
            status: 'Open',
            dueDate: '2026-03-01T12:00:00Z',
            linkedEntityType: 'Broker',
            linkedEntityId: 'broker-1',
            linkedEntityName: 'Blue Horizon Brokerage',
            isOverdue: true,
          },
        ],
        totalCount: 2,
      }),
    })
  })

  await page.route('**/timeline/events?*', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([]),
    })
  })

  await page.route('**/brokers?*', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        data: [
          {
            id: 'broker-1',
            legalName: 'Blue Horizon Brokerage',
            licenseNumber: 'CA-120045',
            state: 'CA',
            status: 'Active',
            email: 'ops@bluehorizon.example',
            phone: '+1-415-555-0101',
            createdAt: '2026-01-01T12:00:00Z',
            updatedAt: '2026-02-20T12:00:00Z',
            rowVersion: 1,
          },
          {
            id: 'broker-2',
            legalName: 'Northline Risk Partners',
            licenseNumber: 'TX-882311',
            state: 'TX',
            status: 'Pending',
            email: 'team@northline.example',
            phone: '+1-512-555-0199',
            createdAt: '2026-01-15T12:00:00Z',
            updatedAt: '2026-02-21T12:00:00Z',
            rowVersion: 3,
          },
        ],
        page: 1,
        pageSize: 10,
        totalCount: 2,
        totalPages: 1,
      }),
    })
  })
}

type Rgba = { r: number; g: number; b: number; a: number }

function parseCssColor(input: string): Rgba {
  const normalized = input.trim()
  const match = normalized.match(/^rgba?\(([^)]+)\)$/i)
  if (!match) {
    throw new Error(`Unsupported CSS color: ${input}`)
  }

  const [r, g, b, a = '1'] = match[1].split(',').map((part) => part.trim())
  return {
    r: Number(r),
    g: Number(g),
    b: Number(b),
    a: Number(a),
  }
}

function compositeOver(fg: Rgba, bg: Rgba): Rgba {
  if (fg.a >= 1) return { ...fg, a: 1 }

  const a = fg.a + bg.a * (1 - fg.a)
  if (a === 0) return { r: 0, g: 0, b: 0, a: 0 }

  return {
    r: Math.round((fg.r * fg.a + bg.r * bg.a * (1 - fg.a)) / a),
    g: Math.round((fg.g * fg.a + bg.g * bg.a * (1 - fg.a)) / a),
    b: Math.round((fg.b * fg.a + bg.b * bg.a * (1 - fg.a)) / a),
    a,
  }
}

function contrastRatio(foreground: Rgba, background: Rgba) {
  const l1 = relativeLuminance(foreground)
  const l2 = relativeLuminance(background)
  const [lighter, darker] = l1 >= l2 ? [l1, l2] : [l2, l1]
  return (lighter + 0.05) / (darker + 0.05)
}

function relativeLuminance(color: Rgba) {
  const channels = [color.r, color.g, color.b].map((value) => {
    const srgb = value / 255
    return srgb <= 0.03928 ? srgb / 12.92 : ((srgb + 0.055) / 1.055) ** 2.4
  })

  return 0.2126 * channels[0] + 0.7152 * channels[1] + 0.0722 * channels[2]
}
