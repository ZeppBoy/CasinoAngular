# Casino Application – Development Plan (Reconstructed)

Date: 2025-11-06
Scope: Full‑stack casino app with Authentication, Account Management, and Games (Slot, Blackjack, Roulette; Poker optional) per brAngular.md.

1) Overview
- Web client: Angular 17+ (TypeScript, SCSS, Router, Animations, HttpClient).
- API: ASP.NET Core 8 Web API + EF Core (SQLite), JWT Auth, Swagger.
- Data: Users, Transactions, GameSessions, GameHistory.

2) Architecture
- Angular client → JWT → ASP.NET Core API → EF Core (SQLite).
- Secure CORS (http://localhost:4200), HTTPS, input validation, server‑side game logic.

3) Database (high level)
- Users: Username, Email, PasswordHash/Salt, Balance (default 1000), timestamps, IsActive.
- Transactions: Type (Deposit/Withdrawal/Bet/Win), Amount, Before/After, GameType, CreatedDate.
- GameSessions: UserId, GameType, Start/End, Totals.
- GameHistory: SessionId, UserId, GameType, BetAmount, WinAmount, GameData JSON, PlayedDate.

4) Phases & Deliverables
- Phase 1 (Complete): Solution setup, EF Core + SQLite, entities, initial migration, Swagger, Angular workspace scaffold.
- Phase 2: Authentication
  • Backend: DTOs (Register, Login, Token, Profile), AuthenticationService (BCrypt + JWT), AuthController (/register, /login, /refresh, /logout), JWT middleware, tests.
  • Frontend: AuthService, Login/Register components, AuthGuard, JWT interceptor, routes.
- Phase 3: User Account Management
  • Backend: UserService, TransactionService, Users/Transactions controllers (profile, balance, deposit, withdraw, history), tests.
  • Frontend: Dashboard, balance widget, deposit/withdraw forms, transaction table (pagination).
- Phase 4: Slot Machine
  • Backend: ISlotMachineService (Spin, payout), RNG, GamesController /games/slot/spin, transactions, tests.
  • Frontend: SlotMachineComponent (reels, bet controls), animations (spin, win lines), sounds.
- Phase 5: Blackjack
  • Backend: IBlackjackService (start/hit/stand/double, dealer AI), DTOs, tests.
  • Frontend: BlackjackComponent (hands, actions), dealing/flip/chips animations.
- Phase 6: Poker (optional/skip-able)
  • Backend: Hand evaluation + payouts, DTOs, tests.
  • Frontend: PokerComponent, betting UI, animations.
- Phase 7: Roulette
  • Backend: IRouletteService (spin, payouts, validation), DTOs, tests.
  • Frontend: RouletteComponent (wheel, table, chips), animations.
- Phase 8: Polish & Deployment
  • Perf (caching, indexes), security audit, full docs, CI/CD, prod builds.

5) API (planned)
- Auth: POST /api/auth/{register|login|refresh|logout}
- Users: GET/PUT /api/users/profile, GET /api/users/balance, POST /api/users/{deposit|withdraw}
- Transactions: GET /api/users/transactions?page=&pageSize=
- Games: POST /api/games/slot/spin, POST /api/games/blackjack/{start|hit|stand|double}, POST /api/games/roulette/spin

6) Current Status (from repo docs)
- Phase 1 complete; controllers present (Auth, Users, Transactions, Games); Program.cs config includes JWT, services, CORS; DbContext and entities exist.
- Phase 2–5 appear implemented backend‑side; frontend pending.
- Known frontend issue: Missing @angular-devkit/build-angular:dev-server builder.

7) Next Actions
- Frontend fix: In Angular project directory run: npm i -D @angular-devkit/build-angular @angular-devkit/architect && npx ng update @angular/cli @angular/core --force (if versions mismatch).
- Verify angular.json builder: "builder": "@angular-devkit/build-angular:application" (Angular 17+) and dev-server target exists.
- Implement auth UI and game components; wire API calls; add JWT interceptor and guards.

8) Definition of Done
- Auth flows work (register/login/refresh), protected routes enforced.
- Account management (balance, deposit/withdraw, transactions) functional.
- Slot, Blackjack, Roulette playable with correct payouts and animations.
- Tests passing (backend ≥80%, frontend ≥70%); Swagger docs complete; prod build & deploy succeed.
