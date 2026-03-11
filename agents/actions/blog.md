# Action: Blog

## User Intent

Write development logs, technical articles, and blog posts about project progress, decisions, lessons learned, and interesting technical challenges.

## Agent Flow

```
Blogger
  ↓
[SELF-REVIEW GATE: Validate content quality and accuracy]
  ↓
[EDITORIAL GATE: User reviews post]
  ↓
Blog Complete
```

**Flow Type:** Single agent with editorial gate

---

## Runtime Execution Boundary

- The blog action runs entirely in the builder runtime. No application runtime containers are required.
- Code examples in blog posts should be verified against the actual codebase for accuracy, but the Blogger does not execute code.

---

## Execution Steps

### Step 1: Topic and Audience Planning

**Execution Instructions:**

1. **Activate Blogger agent** by reading `agents/blogger/SKILL.md`

2. **Read context based on topic:**
   - `planning-mds/BLUEPRINT.md` (project context)
   - `planning-mds/architecture/decisions/` (ADRs for decision posts)
   - `planning-mds/features/F{NNNN}-{slug}/` (feature context: stories, STATUS.md)
   - `planning-mds/architecture/SOLUTION-PATTERNS.md` (for pattern-focused posts)
   - Relevant code changes (git log, implementation files)
   - Review outputs (code review, security review reports)
   - Test results and coverage data

3. **Determine post type from user input:**
   - `devlog` — development progress update
   - `tutorial` — step-by-step how-to guide
   - `case-study` — deep dive into a problem and solution
   - `retrospective` — lessons learned reflection
   - `deep-dive` — detailed technical exploration
   - `decision` — explaining an architectural decision (often based on an ADR)

4. **Produce editorial brief:**
   ```markdown
   # Editorial Brief

   Topic: [topic from user]
   Type: [post type]
   Target Audience: [developers / general tech / management / team]
   Estimated Length: [word count]
   Key Points: [3-5 bullet points]
   Code Examples Needed: [count and description]
   Diagrams Needed: [count and description]
   ```

**Completion Criteria for Step 1:**
- [ ] Editorial brief produced
- [ ] Target audience identified
- [ ] Source material gathered

---

### Step 2: Content Creation

**Execution Instructions:**

1. **Write blog post following the editorial brief:**

   **Structure (all post types):**
   - **Title:** Clear, engaging, specific (not clickbait)
   - **Introduction:** Hook + what the reader will learn (2-3 sentences)
   - **Body:** Main content organized with headings
   - **Code Examples:** Tested against actual codebase, syntax-highlighted
   - **Visuals:** Diagrams or screenshots where they add clarity
   - **Conclusion:** Key takeaways (2-3 bullet points)
   - **Metadata:** Tags, categories, estimated reading time

   **Post Type Guidelines:**

   | Type | Length | Structure | Key Element |
   |------|--------|-----------|-------------|
   | DevLog | 800-1,200 words | What → Why → How → Challenges → Results | Progress narrative |
   | Tutorial | 1,500-2,500 words | Prerequisites → Steps → Explanation → Examples | Copy-pasteable steps |
   | Case Study | 1,500-2,000 words | Problem → Investigation → Solution → Results → Lessons | Before/after comparison |
   | Retrospective | 1,000-1,500 words | Context → What went well → Challenges → Lessons → Changes | Honest reflection |
   | Deep Dive | 1,500-2,500 words | Context → Concept → Implementation → Trade-offs → Conclusion | Technical depth |
   | Decision | 1,000-1,500 words | Context → Options → Decision → Rationale → Consequences | Decision rationale |

2. **Verify code examples:**
   - All code snippets match actual project code
   - Examples are complete enough to understand (not isolated fragments)
   - No secrets, credentials, or internal URLs in examples
   - Syntax highlighting specified for each code block

3. **Save blog post:**
   - File: `blog/{year}/{month}-{slug}.md` (or user-specified location)

**Completion Criteria for Step 2:**
- [ ] Blog post written to target length
- [ ] All sections complete
- [ ] Code examples verified against codebase

---

### Step 3: SELF-REVIEW GATE (Content Quality)

**Execution Instructions:**

Blogger validates post quality:

**Technical Accuracy:**
- [ ] Code examples match actual codebase
- [ ] Architecture descriptions match SOLUTION-PATTERNS.md
- [ ] Metrics and data are from actual project (not invented)
- [ ] No secrets, credentials, or internal URLs
- [ ] Technical assertions are accurate and verifiable

**Content Quality:**
- [ ] Title is clear, specific, and engaging
- [ ] Introduction hooks the reader and states the value proposition
- [ ] Content is well-structured with clear headings
- [ ] Each section advances the narrative
- [ ] Conclusion summarizes key takeaways
- [ ] No filler content or unnecessary repetition

**Readability:**
- [ ] Appropriate for target audience (not too basic or advanced)
- [ ] Jargon explained when first used
- [ ] Paragraphs are focused (one idea per paragraph)
- [ ] Code examples have surrounding explanation
- [ ] Post length matches target range for post type

**If any check fails:**
- Fix content quality issues
- Re-run self-review
- Repeat until passing

**Gate Criteria:**
- [ ] All technical accuracy checks pass
- [ ] All content quality checks pass
- [ ] All readability checks pass

---

### Step 4: EDITORIAL GATE (User Review)

**Execution Instructions:**

1. **Present blog post summary to user:**
   ```
   ═══════════════════════════════════════════════════════════
   Blog Post Ready for Review
   ═══════════════════════════════════════════════════════════

   Title: [post title]
   Type: [post type]
   Length: [word count] words (~[reading time] min read)
   Audience: [target audience]

   Sections:
     - [Section 1 heading]
     - [Section 2 heading]
     - [Section 3 heading]
     - ...

   Code Examples: [count]
   Diagrams: [count]

   File: [file path]

   ═══════════════════════════════════════════════════════════
   Please review the post at the file path above.
   ═══════════════════════════════════════════════════════════
   ```

2. **Present review options:**
   ```
   Blog Post Review:
   - "approve" — Post is ready to publish
   - "request changes" — Specify what needs to change
   - "reject" — Major issues, needs rewrite
   ```

3. **Handle user response:**
   - **If "approve":**
     - Proceed to Step 5 (Blog Complete)

   - **If "request changes":**
     - Ask: "What changes are needed?"
     - Capture feedback
     - Apply changes to post
     - Return to Step 3 (re-run self-review)

   - **If "reject":**
     - Ask: "What are the major issues?"
     - Capture feedback
     - Return to Step 2 (rewrite with feedback)

**Gate Criteria:**
- [ ] User has reviewed blog post
- [ ] User has made explicit decision
- [ ] Any requested changes have been applied

---

### Step 5: Blog Complete

**Execution Instructions:**

Present completion summary:

```
═══════════════════════════════════════════════════════════
Blog Action Complete! ✓
═══════════════════════════════════════════════════════════

Post: [title]
Type: [post type]
Length: [word count] words
File: [file path]

Quality:
  ✓ Technical accuracy verified
  ✓ Content quality checked
  ✓ Code examples verified against codebase
  ✓ Readability validated

User Decision: APPROVED

═══════════════════════════════════════════════════════════
Next Steps:
═══════════════════════════════════════════════════════════

1. Publish to target platform
2. Share on social media / team channels
3. Engage with comments and feedback

Publishing Destinations:
  - Internal: Team wiki, Confluence, Notion
  - Public: Company blog, dev.to, Medium, Hashnode
  - Social: LinkedIn, Twitter/X (thread format)
  - Repo: blog/ directory (version controlled)

Blog post ready to publish! ✓
═══════════════════════════════════════════════════════════
```

---

## Validation Criteria

**Overall Blog Action Success:**
- [ ] Editorial brief produced
- [ ] Blog post written to target length and type
- [ ] Self-review gate passed (accuracy, quality, readability)
- [ ] User reviewed and approved post
- [ ] Code examples verified against actual codebase
- [ ] No secrets or credentials in content
- [ ] Post saved to specified location

---

## Prerequisites

Before running blog action:
- [ ] Something worth writing about (feature completed, decision made, problem solved, milestone reached)
- [ ] Context available (code, architecture docs, ADRs, review reports)
- [ ] Optional: Target audience identified by user

---

## Example Usage

### Scenario 1: Feature DevLog
```
User: "Write a devlog about building the customer management feature"

Agent Runtime: [Reads blog.md]
Agent Runtime: "I'll create a devlog about customer management..."

[Step 1: Planning]
  - Topic: Customer management feature
  - Type: devlog
  - Audience: developers and stakeholders
  - Key points: clean architecture, ABAC authorization, vertical slicing

[Step 2: Content creation]
  - Title: "Building Customer Management: Clean Architecture in Practice"
  - 1,100 words
  - 5 code examples (entity, endpoint, authorization policy, test, component)
  - 1 architecture diagram

[Step 3: Self-review — all checks pass]

[Step 4: Editorial gate]
Agent Runtime: "Blog post ready for review.
  Title: 'Building Customer Management: Clean Architecture in Practice'
  Length: 1,100 words (~5 min read)
  File: blog/2026/03-customer-management-feature.md

  Options: approve / request changes / reject"

User: "request changes — add a section about the challenges
  we faced with authorization policy syntax"

Agent Runtime: "Adding challenges section..."
[Updates post, re-runs self-review]

Agent Runtime: "Updated to 1,350 words. Review again?"

User: "approve"

Agent Runtime: "Blog post complete! Ready to publish."
```

### Scenario 2: Architecture Decision Post
```
User: "Write about our decision to switch from Keycloak to authentik"

[Step 1: Planning]
  - Reads ADR-006
  - Type: decision
  - Audience: technical professionals

[Step 2: Content creation]
  - Title: "Why We Migrated from Keycloak to authentik"
  - 1,300 words
  - Covers: evaluation criteria, migration challenges,
    blueprint gotchas, results

[Steps 3-4: Review and approval]

Agent Runtime: "Decision post complete! Saved to
  blog/2026/03-keycloak-to-authentik-migration.md"
```

### Scenario 3: Technical Deep Dive
```
User: "Write a deep dive on our ABAC authorization implementation"

[Step 1: Planning]
  - Type: deep-dive
  - Audience: .NET developers interested in authorization

[Step 2: Content creation]
  - Title: "ABAC with Casbin: Fine-Grained Authorization in .NET"
  - 2,100 words, 8 code examples, 2 diagrams

[Steps 3-4: Review and approval]

User: "approve"

Agent Runtime: "Deep dive complete!"
```

---

## Related Actions

- **After:** Any action — blog about progress or learnings
- **With:** [document action](./document.md) — docs for reference, blogs for narrative
- **Continuous:** Blog throughout the project lifecycle

---

## Notes

- Blog regularly (after each feature or milestone) for maximum value
- Don't wait for perfection — publish and iterate
- Use blogs to document decisions and reasoning (complements ADRs)
- Blogs are excellent onboarding material for new team members
- Be honest about challenges and failures (they are valuable)
- Technical blogs can become documentation later
- Keep a blog backlog of interesting topics as you work
