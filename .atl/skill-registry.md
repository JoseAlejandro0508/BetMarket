# Skill Registry

**Delegator use only.** Any agent that launches sub-agents reads this registry to resolve compact rules, then injects them directly into sub-agent prompts. Sub-agents do NOT read this registry or individual SKILL.md files.

## User Skills

| Trigger | Skill | Path |
|---------|-------|------|
| Create/open PRs | branch-pr | C:/Users/Alejandro/.config/opencode/skills/branch-pr/SKILL.md |
| Write Go tests (incl. Bubbletea) | go-testing | C:/Users/Alejandro/.config/opencode/skills/go-testing/SKILL.md |
| Create GitHub issue | issue-creation | C:/Users/Alejandro/.config/opencode/skills/issue-creation/SKILL.md |
| Dual adversarial review | judgment-day | C:/Users/Alejandro/.config/opencode/skills/judgment-day/SKILL.md |
| Create new AI skills | skill-creator | C:/Users/Alejandro/.config/opencode/skills/skill-creator/SKILL.md |
| Responsive design adaptation | adapt | C:/Users/Alejandro/.agents/skills/adapt/SKILL.md |
| Browser automation tasks | agent-browser | C:/Users/Alejandro/.agents/skills/agent-browser/SKILL.md |
| .NET performance analysis | analyzing-dotnet-performance | C:/Users/Alejandro/.agents/skills/analyzing-dotnet-performance/SKILL.md |
| Add UI motion and transitions | animate | C:/Users/Alejandro/.agents/skills/animate/SKILL.md |
| Backend architecture patterns | architecture-patterns | C:/Users/Alejandro/.agents/skills/architecture-patterns/SKILL.md |
| ASP.NET Core controllers/DI/config | aspnet-core | C:/Users/Alejandro/.agents/skills/aspnet-core/SKILL.md |
| UX/accessibility/performance audit | audit | C:/Users/Alejandro/.agents/skills/audit/SKILL.md |
| Make design bolder | bolder | C:/Users/Alejandro/.agents/skills/bolder/SKILL.md |
| Improve UX copy and labels | clarify | C:/Users/Alejandro/.agents/skills/clarify/SKILL.md |
| Generate unit tests | code-testing-agent | C:/Users/Alejandro/.agents/skills/code-testing-agent/SKILL.md |
| Add strategic UI color | colorize | C:/Users/Alejandro/.agents/skills/colorize/SKILL.md |
| UX critique and scoring | critique | C:/Users/Alejandro/.agents/skills/critique/SKILL.md |
| Add delightful polish | delight | C:/Users/Alejandro/.agents/skills/delight/SKILL.md |
| Simplify and declutter UI | distill | C:/Users/Alejandro/.agents/skills/distill/SKILL.md |
| .NET backend patterns | dotnet-backend-patterns | C:/Users/Alejandro/.agents/skills/dotnet-backend-patterns/SKILL.md |
| Discover installable skills | find-skills | C:/Users/Alejandro/.agents/skills/find-skills/SKILL.md |
| High-quality frontend implementation | frontend-design | C:/Users/Alejandro/.agents/skills/frontend-design/SKILL.md |
| Premium frontend craft | impeccable | C:/Users/Alejandro/.agents/skills/impeccable/SKILL.md |
| Improve layout/spacing/hierarchy | layout | C:/Users/Alejandro/.agents/skills/layout/SKILL.md |
| Node.js backend architecture | nodejs-backend-patterns | C:/Users/Alejandro/.agents/skills/nodejs-backend-patterns/SKILL.md |
| UI performance optimization | optimize | C:/Users/Alejandro/.agents/skills/optimize/SKILL.md |
| Push UI to advanced interactions | overdrive | C:/Users/Alejandro/.agents/skills/overdrive/SKILL.md |
| Final visual polish pass | polish | C:/Users/Alejandro/.agents/skills/polish/SKILL.md |
| Tone down visual intensity | quieter | C:/Users/Alejandro/.agents/skills/quieter/SKILL.md |
| Plan UX/UI before coding | shape | C:/Users/Alejandro/.agents/skills/shape/SKILL.md |
| Stitch design generation/editing | stitch-design | C:/Users/Alejandro/.agents/skills/stitch-design/SKILL.md |
| Stitch MCP setup/troubleshooting | stitch-setup | C:/Users/Alejandro/.agents/skills/stitch-setup/SKILL.md |
| Typography refinement | typeset | C:/Users/Alejandro/.agents/skills/typeset/SKILL.md |
| React/Next.js performance patterns | vercel-react-best-practices | C:/Users/Alejandro/.agents/skills/vercel-react-best-practices/SKILL.md |
| Web guidelines compliance review | web-design-guidelines | C:/Users/Alejandro/.agents/skills/web-design-guidelines/SKILL.md |

## Compact Rules

### branch-pr
- Use issue-first workflow before opening PR.
- Inspect branch state, diff, log, and divergence from base.
- Summarize ALL commits included in PR, not only latest.
- Push branch with upstream tracking when needed.
- Create PR with structured summary and return PR URL.

### go-testing
- Use Go standard testing idioms first.
- For Bubbletea TUI, prefer deterministic teatest patterns.
- Keep tests isolated, fast, and table-driven when possible.
- Avoid brittle timing assumptions; synchronize explicitly.
- Validate behavior, not implementation details.

### issue-creation
- Follow issue-first policy and capture clear problem statement.
- Include reproducible context, expected vs actual behavior.
- Add scope, acceptance criteria, and risk notes.
- Keep issue title concise and searchable.
- Link related artifacts when available.

### judgment-day
- Run two independent blind reviews on same target.
- Synthesize findings and fix highest-severity items first.
- Re-run review cycle until both pass or max iterations reached.
- Escalate explicitly when unresolved after retry limit.
- Preserve evidence and rationale for each fix.

### skill-creator
- Follow Agent Skills spec structure exactly.
- Define clear trigger conditions and anti-triggers.
- Include executable rules, not generic advice.
- Keep examples minimal and production-relevant.
- Validate skill usability before finalizing.

### adapt
- Implement responsive behavior with explicit breakpoints.
- Prefer fluid layouts and scalable spacing tokens.
- Ensure touch targets are mobile-safe.
- Re-balance hierarchy per viewport, not only scale.
- Validate key flows on mobile/tablet/desktop.

### agent-browser
- Use browser automation for navigation, forms, and extraction.
- Execute deterministic steps and verify page state transitions.
- Capture screenshots/data only when useful to outcome.
- Handle auth/session flows explicitly.
- Report actionable results, not raw click logs.

### analyzing-dotnet-performance
- Prioritize high-impact allocation and async hot paths.
- Classify findings by severity and expected payoff.
- Focus on measurable anti-patterns across LINQ/strings/I/O.
- Provide concrete code-level remediations.
- Distinguish micro-optimizations from architecture bottlenecks.

### animate
- Add purposeful motion that improves comprehension.
- Keep transitions performant and subtle by default.
- Use timing/curves consistently across components.
- Avoid decorative animation that harms usability.
- Respect reduced-motion accessibility preferences.

### architecture-patterns
- Enforce clear layer boundaries and dependency direction.
- Keep domain model isolated from infrastructure concerns.
- Use ports/adapters for external integrations.
- Make tradeoffs explicit when selecting architecture style.
- Prevent cyclic dependencies aggressively.

### aspnet-core
- Configure services in Program.cs with explicit DI boundaries.
- Keep controllers thin; push logic to services/use-cases.
- Use options/config binding for external settings.
- Order middleware correctly (auth, routing, errors, etc.).
- Prefer consistent API contracts and validation handling.

### audit
- Evaluate accessibility, performance, responsiveness, and consistency.
- Score findings by severity (P0-P3) with clear impact.
- Provide prioritized remediation plan.
- Avoid vague critiques; tie each issue to evidence.
- Re-check fixes against original risk set.

### bolder
- Increase visual contrast and personality safely.
- Strengthen hierarchy before adding decorative complexity.
- Keep interaction clarity and readability intact.
- Apply boldness strategically to focal elements.
- Avoid reducing accessibility while amplifying style.

### clarify
- Rewrite copy for clarity and user intent.
- Prefer concise, action-oriented microcopy.
- Replace ambiguous errors with diagnostic guidance.
- Align labels with user mental models.
- Keep terminology consistent across flows.

### code-testing-agent
- Generate runnable tests aligned with project conventions.
- Cover happy path, edge cases, and failures.
- Keep tests deterministic and isolated.
- Avoid brittle mocks when integration seams are stable.
- Ensure outputs compile and pass in repo context.

### colorize
- Introduce color through semantic intent, not randomness.
- Preserve contrast and readability standards.
- Use accent colors to guide attention.
- Keep palette cohesive across states/components.
- Avoid over-saturation in dense interfaces.

### critique
- Assess UX via hierarchy, IA, clarity, and cognitive load.
- Provide quantified scoring with rationale.
- Identify anti-patterns and user friction points.
- Recommend specific, prioritized improvements.
- Balance aesthetics with task effectiveness.

### delight
- Add lightweight moments of personality where meaningful.
- Focus on micro-interactions tied to user success.
- Keep delight secondary to speed and clarity.
- Avoid gimmicks that distract from core tasks.
- Preserve consistency with product tone.

### distill
- Remove non-essential UI elements first.
- Simplify flows by reducing decisions and noise.
- Strengthen primary action visibility.
- Keep only content with clear user value.
- Measure clarity gains after simplification.

### dotnet-backend-patterns
- Use async/await correctly to avoid blocking threads.
- Keep DI composition explicit and testable.
- Choose EF Core/Dapper based on query profile.
- Externalize configuration and secrets securely.
- Add tests around domain/business invariants.

### find-skills
- Start by clarifying desired capability/outcome.
- Map need to closest existing skill before proposing new one.
- Recommend install/use path with minimal friction.
- Highlight tradeoffs and skill boundaries.
- Escalate to skill creation only when necessary.

### frontend-design
- Deliver production-grade UI, not generic scaffolding.
- Establish strong visual system (spacing, type, color).
- Prioritize readability and interaction clarity.
- Keep components reusable and coherent.
- Polish states (hover, empty, loading, error).

### impeccable
- Use shape-then-build flow for high-quality outcomes.
- Extract reusable patterns into a design system.
- Optimize composition, rhythm, and craftsmanship.
- Avoid cookie-cutter AI aesthetics.
- Ship with consistent visual and interaction language.

### layout
- Fix spacing scale and alignment before styling tweaks.
- Create clear visual hierarchy with rhythm.
- Avoid overcrowded regions and uneven density.
- Use grid/flex structure intentionally.
- Validate scanability of key content blocks.

### nodejs-backend-patterns
- Standardize middleware, error handling, and auth flow.
- Keep routes thin; centralize domain/service logic.
- Design APIs with explicit contracts and validation.
- Handle async failures and observability from day one.
- Choose framework/storage patterns by scalability needs.

### optimize
- Target bottlenecks first (rendering, bundles, network).
- Reduce unnecessary re-renders and heavy effects.
- Optimize images/assets and loading strategy.
- Measure before/after to validate impact.
- Keep UX smooth under realistic device constraints.

### overdrive
- Use advanced effects only when value justifies complexity.
- Maintain 60fps budgets and fallback behavior.
- Pair ambitious visuals with robust interaction design.
- Guard against accessibility regressions.
- Provide progressive enhancement for weaker devices.

### polish
- Run final pass on alignment, spacing, and consistency.
- Standardize component states and edge-case visuals.
- Remove visual noise and micro-inconsistencies.
- Tighten copy and iconography alignment.
- Ship-ready means no obvious rough edges remain.

### quieter
- Reduce visual aggression while preserving hierarchy.
- Lower saturation/contrast where overstimulating.
- Keep focus indicators and affordances intact.
- Favor calm rhythm and whitespace balance.
- Ensure tone feels refined, not bland.

### shape
- Start with discovery before implementation.
- Define goals, constraints, and user outcomes explicitly.
- Produce a concrete design brief to guide build.
- Resolve ambiguity early through structured questions.
- Keep scope aligned with problem value.

### stitch-design
- Use Stitch as single entry point for design generation/editing.
- Build/maintain shared design system artifacts first.
- Enhance prompts with UX context and atmosphere cues.
- Iterate screens through directed edits and variants.
- Keep outputs consistent with project design language.

### stitch-setup
- Install stitch-kit and MCP server in prescribed order.
- Validate connectivity before attempting generation flows.
- Diagnose setup issues systematically (auth, config, runtime).
- Document working configuration for repeatability.
- Re-test after each fix to isolate root cause.

### typeset
- Improve readability with deliberate type hierarchy.
- Normalize font scales, line-height, and weight usage.
- Limit font combinations to maintain coherence.
- Ensure text contrast and spacing support scanning.
- Align typography with product tone and density.

### vercel-react-best-practices
- Prefer server-first patterns in Next.js where applicable.
- Minimize client-side JS and expensive hydration.
- Use efficient data fetching/caching primitives.
- Control bundle size through boundary and dependency hygiene.
- Optimize rendering paths and component granularity.

### web-design-guidelines
- Audit UI against accessibility and usability standards.
- Check semantics, contrast, keyboard flow, and feedback.
- Validate responsive behavior and content hierarchy.
- Flag anti-patterns with concrete remediation.
- Prioritize fixes by user impact and implementation effort.

## Project Conventions

No project convention files were found at root (`agents.md`, `AGENTS.md`, `CLAUDE.md`, `.cursorrules`, `GEMINI.md`, `copilot-instructions.md`).
