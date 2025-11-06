# Casino Application Development Plan

## 1. Project Overview

### 1.1 Application Description
A web-based casino application featuring four classic games (Slot Machine, Blackjack, Poker, and Roulette) with smooth animations, user account management, and real-time balance tracking.

### 1.2 Key Features
- Four fully functional casino games with animations
- User registration and authentication
- Account balance management
- Transaction history
- Responsive web design
- Secure API communication

---

## 2. System Architecture

### 2.1 Architecture Pattern
**Three-tier architecture:**
- **Presentation Layer**: Angular web client
- **Business Logic Layer**: ASP.NET Web API
- **Data Access Layer**: Entity Framework Core with SQLite

### 2.2 Technology Stack
- **Frontend**: Angular 17+ (TypeScript, RxJS, Angular Animations)
- **Backend**: ASP.NET Core 8.0 Web API (C#)
- **Database**: SQLite with Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Testing**: xUnit, Moq, Jasmine/Karma
- **API Documentation**: Swagger/OpenAPI

---

## 3. Database Design

### 3.1 Database Schema

```sql
-- Users Table
Users
- UserId (PK, INT, Identity)
- Username (NVARCHAR(50), Unique, NOT NULL)
- Email (NVARCHAR(100), Unique, NOT NULL)
- PasswordHash (NVARCHAR(255), NOT NULL)
- PasswordSalt (NVARCHAR(255), NOT NULL)
- Balance (DECIMAL(18,2), DEFAULT 1000.00)
- CreatedDate (DATETIME, NOT NULL)
- LastLoginDate (DATETIME)
- IsActive (BIT, DEFAULT 1)

-- Transactions Table
Transactions
- TransactionId (PK, INT, Identity)
- UserId (FK, INT, NOT NULL)
- TransactionType (NVARCHAR(20), NOT NULL) -- Deposit, Withdrawal, Bet, Win
- Amount (DECIMAL(18,2), NOT NULL)
- BalanceBefore (DECIMAL(18,2), NOT NULL)
- BalanceAfter (DECIMAL(18,2), NOT NULL)
- GameType (NVARCHAR(20)) -- SlotMachine, Blackjack, Poker, Roulette
- Description (NVARCHAR(255))
- CreatedDate (DATETIME, NOT NULL)

-- GameSessions Table
GameSessions
- SessionId (PK, INT, Identity)
- UserId (FK, INT, NOT NULL)
- GameType (NVARCHAR(20), NOT NULL)
- StartTime (DATETIME, NOT NULL)
- EndTime (DATETIME)
- TotalBets (INT, DEFAULT 0)
- TotalWinnings (DECIMAL(18,2), DEFAULT 0)
- TotalLosses (DECIMAL(18,2), DEFAULT 0)

-- GameHistory Table
GameHistory
- GameHistoryId (PK, INT, Identity)
- SessionId (FK, INT, NOT NULL)
- UserId (FK, INT, NOT NULL)
- GameType (NVARCHAR(20), NOT NULL)
- BetAmount (DECIMAL(18,2), NOT NULL)
- WinAmount (DECIMAL(18,2), DEFAULT 0)
- GameData (NVARCHAR(MAX)) -- JSON data for game-specific details
- PlayedDate (DATETIME, NOT NULL)
```

---

## 4. Backend API Design

### 4.1 Project Structure
```
CasinoAPI/
├── CasinoAPI.API/              # Web API project
│   ├── Controllers/
│   ├── Middleware/
│   ├── Program.cs
│   └── appsettings.json
├── CasinoAPI.Core/             # Business logic & domain models
│   ├── Entities/
│   ├── Interfaces/
│   ├── Services/
│   └── DTOs/
├── CasinoAPI.Infrastructure/   # Data access & external services
│   ├── Data/
│   ├── Repositories/
│   └── Migrations/
└── CasinoAPI.Tests/            # Unit tests
    ├── Controllers/
    ├── Services/
    └── Repositories/
```

### 4.2 API Endpoints

#### Authentication Endpoints
```
POST   /api/auth/register       - Register new user
POST   /api/auth/login          - User login (returns JWT)
POST   /api/auth/refresh        - Refresh JWT token
POST   /api/auth/logout         - User logout
```

#### User Account Endpoints
```
GET    /api/users/profile       - Get user profile
PUT    /api/users/profile       - Update user profile
GET    /api/users/balance       - Get current balance
POST   /api/users/deposit       - Add funds to account
POST   /api/users/withdraw      - Withdraw funds
GET    /api/users/transactions  - Get transaction history (paginated)
```

#### Game Endpoints
```
POST   /api/games/slot/spin           - Play slot machine
POST   /api/games/blackjack/start     - Start blackjack game
POST   /api/games/blackjack/hit       - Hit in blackjack
POST   /api/games/blackjack/stand     - Stand in blackjack
POST   /api/games/blackjack/double    - Double down
POST   /api/games/poker/start         - Start poker game
POST   /api/games/poker/bet           - Place poker bet
POST   /api/games/poker/fold          - Fold poker hand
POST   /api/games/roulette/spin       - Spin roulette wheel
GET    /api/games/history             - Get game history
GET    /api/games/sessions            - Get game sessions
```

### 4.3 Core Services

#### IAuthenticationService
- RegisterUser(RegisterDto)
- Login(LoginDto)
- ValidateToken(string token)
- GenerateJwtToken(User user)

#### IUserService
- GetUserProfile(int userId)
- UpdateUserProfile(int userId, UpdateProfileDto)
- GetBalance(int userId)
- AddFunds(int userId, decimal amount)
- WithdrawFunds(int userId, decimal amount)

#### ITransactionService
- CreateTransaction(Transaction transaction)
- GetUserTransactions(int userId, pagination)
- GetTransactionById(int transactionId)

#### ISlotMachineService
- Spin(int userId, decimal betAmount)
- CalculatePayout(string[] symbols)

#### IBlackjackService
- StartGame(int userId, decimal betAmount)
- Hit(int sessionId)
- Stand(int sessionId)
- DoubleDown(int sessionId)
- CalculateHandValue(List<Card> cards)

#### IPokerService
- StartGame(int userId, decimal betAmount)
- PlaceBet(int sessionId, decimal amount)
- Fold(int sessionId)
- EvaluateHand(List<Card> cards)

#### IRouletteService
- SpinWheel(int userId, decimal betAmount, BetType betType, int[] numbers)
- CalculatePayout(int winningNumber, Bet bet)

---

## 5. Frontend Design

### 5.1 Angular Project Structure
```
casino-client/
├── src/
│   ├── app/
│   │   ├── core/                    # Singleton services & guards
│   │   │   ├── guards/
│   │   │   ├── interceptors/
│   │   │   ├── services/
│   │   │   └── models/
│   │   ├── shared/                  # Shared components & utilities
│   │   │   ├── components/
│   │   │   ├── directives/
│   │   │   ├── pipes/
│   │   │   └── animations/
│   │   ├── features/
│   │   │   ├── auth/                # Login, Register
│   │   │   ├── dashboard/           # User dashboard
│   │   │   ├── games/
│   │   │   │   ├── slot-machine/
│   │   │   │   ├── blackjack/
│   │   │   │   ├── poker/
│   │   │   │   └── roulette/
│   │   │   ├── account/             # Profile, Balance, Transactions
│   │   │   └── history/             # Game history
│   │   ├── app.component.ts
│   │   ├── app.routes.ts
│   │   └── app.config.ts
│   ├── assets/
│   │   ├── images/
│   │   ├── sounds/
│   │   └── animations/
│   └── styles/
```

### 5.2 Key Angular Components

#### Slot Machine Component
**Animations:**
- Reel spinning animation (staggered start/stop)
- Symbol blur effect during spin
- Win line highlighting
- Coin explosion on big wins

**Features:**
- 3x3 or 5x3 reel grid
- Adjustable bet amount
- Auto-spin functionality
- Win calculation display

#### Blackjack Component
**Animations:**
- Card dealing animation (slide from deck)
- Card flip animation
- Chip stacking animation
- Dealer card reveal

**Features:**
- Player and dealer hands
- Hit/Stand/Double buttons
- Split functionality
- Real-time hand value calculation

#### Poker Component
**Animations:**
- Card shuffle animation
- Card distribution
- Chip betting animation
- Winning hand highlight

**Features:**
- 5-card draw poker
- Betting rounds
- Hand evaluation display
- Fold/Call/Raise actions

#### Roulette Component
**Animations:**
- Wheel spinning (3D CSS or Canvas)
- Ball rolling animation
- Winning number highlight
- Chip placement animation

**Features:**
- Interactive betting table
- Multiple bet types (single, split, corner, etc.)
- Bet history
- Statistics display

### 5.3 Angular Services

```typescript
// AuthService
- login(credentials)
- register(userData)
- logout()
- isAuthenticated()
- getToken()

// UserService
- getProfile()
- updateProfile()
- getBalance()
- deposit()
- withdraw()

// GameService
- playSlot(betAmount)
- startBlackjack(betAmount)
- blackjackAction(action)
- playPoker(betAmount)
- spinRoulette(bet)

// TransactionService
- getTransactions(pagination)
- getTransactionDetails(id)

// AnimationService
- triggerCardAnimation()
- triggerSlotSpin()
- triggerWheelSpin()
```

### 5.4 Animation Implementation

Using Angular Animations API:
```typescript
// Slot machine reel spin
trigger('reelSpin', [
  state('spinning', style({ transform: 'translateY(-{{distance}}px)' })),
  transition('idle => spinning', animate('{{duration}}ms cubic-bezier(0.25, 0.8, 0.25, 1)'))
])

// Card dealing
trigger('cardDeal', [
  transition(':enter', [
    style({ opacity: 0, transform: 'translateX(-100px)' }),
    animate('300ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
  ])
])

// Roulette wheel spin
trigger('wheelSpin', [
  transition('idle => spinning', [
    animate('{{duration}}ms cubic-bezier(0.33, 1, 0.68, 1)', 
      style({ transform: 'rotate({{rotation}}deg)' }))
  ])
])
```

---

## 6. Testing Strategy

### 6.1 Backend Unit Tests (xUnit + Moq)

**Test Coverage Target: 80%+**

#### Controller Tests
```csharp
// Example: AuthControllerTests
- Register_ValidUser_ReturnsOk()
- Register_DuplicateUsername_ReturnsBadRequest()
- Login_ValidCredentials_ReturnsToken()
- Login_InvalidCredentials_ReturnsUnauthorized()

// Example: GamesControllerTests
- SlotSpin_SufficientBalance_ReturnsResult()
- SlotSpin_InsufficientBalance_ReturnsBadRequest()
- BlackjackHit_ValidSession_ReturnsCard()
- RouletteSpinl_ValidBet_ReturnsResult()
```

#### Service Tests
```csharp
// Example: UserServiceTests
- GetBalance_ExistingUser_ReturnsCorrectBalance()
- AddFunds_ValidAmount_UpdatesBalance()
- WithdrawFunds_SufficientBalance_UpdatesBalance()
- WithdrawFunds_InsufficientBalance_ThrowsException()

// Example: SlotMachineServiceTests
- CalculatePayout_ThreeMatching_ReturnsCorrectAmount()
- CalculatePayout_NoMatch_ReturnsZero()
- Spin_UpdatesUserBalance()

// Example: BlackjackServiceTests
- CalculateHandValue_AceAsEleven_ReturnsCorrectValue()
- CalculateHandValue_AceAsOne_ReturnsCorrectValue()
- DetermineWinner_PlayerBlackjack_ReturnsPlayer()
```

#### Repository Tests
```csharp
// Example: UserRepositoryTests
- GetById_ExistingUser_ReturnsUser()
- Create_ValidUser_AddsToDatabase()
- Update_ExistingUser_UpdatesDatabase()

// Example: TransactionRepositoryTests
- Create_ValidTransaction_AddsToDatabase()
- GetByUserId_ReturnsUserTransactions()
```

### 6.2 Frontend Unit Tests (Jasmine/Karma)

**Test Coverage Target: 70%+**

#### Component Tests
```typescript
// Example: SlotMachineComponentTests
- should create component
- should disable spin button when balance insufficient
- should call gameService.playSlot on spin
- should display win amount after spin
- should update balance after spin

// Example: BlackjackComponentTests
- should deal initial cards on start
- should enable hit/stand buttons after deal
- should disable buttons after stand
- should calculate hand value correctly
```

#### Service Tests
```typescript
// Example: AuthServiceTests
- should store token on successful login
- should remove token on logout
- should return true for valid token
- should navigate to login on expired token

// Example: GameServiceTests
- should call API with correct bet amount
- should handle API errors gracefully
- should update balance observable
```

### 6.3 Integration Tests
```csharp
// API Integration Tests
- FullGameFlow_SlotMachine_UpdatesBalanceCorrectly()
- FullGameFlow_Blackjack_HandlesMultipleRounds()
- AuthenticationFlow_LoginToGamePlay_WorksEndToEnd()
- TransactionFlow_DepositAndWithdraw_UpdatesBalanceCorrectly()
```

### 6.4 E2E Tests (Optional - Playwright/Cypress)
```typescript
- User registration and login flow
- Complete slot machine game
- Complete blackjack game
- Deposit and withdrawal flow
- Transaction history verification
```

---

## 7. Implementation Phases

### Phase 1: Foundation (Weeks 1-2)
**Backend:**
- Setup solution structure
- Configure SQLite and Entity Framework
- Implement database schema and migrations
- Create User entity and repository
- Implement JWT authentication
- Basic API endpoints (Auth, User)

**Frontend:**
- Setup Angular project
- Configure routing
- Implement authentication module
- Create login/register components
- Setup HTTP interceptor for JWT
- Create navigation layout

**Testing:**
- Setup test projects
- Write tests for authentication

### Phase 2: User Account Management (Week 3)
**Backend:**
- Implement balance management
- Create transaction service
- Implement deposit/withdraw endpoints
- Transaction history API

**Frontend:**
- User dashboard component
- Balance display component
- Transaction history component
- Deposit/withdraw forms

**Testing:**
- Unit tests for user services
- Integration tests for transactions

### Phase 3: Slot Machine Game (Week 4)
**Backend:**
- Implement slot machine logic
- Create game session tracking
- Payout calculation algorithm

**Frontend:**
- Slot machine component
- Reel spinning animations
- Win animations
- Bet controls

**Testing:**
- Unit tests for slot logic
- Component tests for UI

### Phase 4: Blackjack Game (Week 5)
**Backend:**
- Implement card deck logic
- Blackjack game rules
- Hit/Stand/Double logic
- Dealer AI

**Frontend:**
- Blackjack component
- Card dealing animations
- Game controls
- Hand value display

**Testing:**
- Unit tests for game logic
- Card value calculation tests

### Phase 5: Poker Game (Week 6)
**Backend:**
- Poker hand evaluation
- Betting rounds logic
- Payout calculation

**Frontend:**
- Poker component
- Card exchange interface
- Betting interface
- Hand evaluation display

**Testing:**
- Hand evaluation tests
- Betting logic tests

### Phase 6: Roulette Game (Week 7)
**Backend:**
- Roulette betting logic
- Wheel spin simulation
- Payout calculations for all bet types

**Frontend:**
- Roulette component
- Wheel spinning animation
- Interactive betting table
- Win/loss display

**Testing:**
- Payout calculation tests
- Bet validation tests

### Phase 7: Polish & Optimization (Week 8)
- Performance optimization
- Animation refinement
- Error handling improvements
- Security audit
- Complete test coverage
- Documentation
- Bug fixes

---

## 8. Security Considerations

### 8.1 Backend Security
- **Password Security**: BCrypt hashing with salt
- **JWT**: Short expiration times, refresh token mechanism
- **Input Validation**: Data annotations and FluentValidation
- **SQL Injection**: Parameterized queries via EF Core
- **Rate Limiting**: Prevent API abuse
- **CORS**: Configure allowed origins
- **HTTPS**: Enforce SSL/TLS

### 8.2 Frontend Security
- **XSS Prevention**: Angular's built-in sanitization
- **Token Storage**: HttpOnly cookies or secure storage
- **Input Sanitization**: Validate all user inputs
- **Route Guards**: Protect authenticated routes

### 8.3 Game Integrity
- **Server-side validation**: All game logic on backend
- **Random Number Generation**: Cryptographically secure RNG
- **Balance verification**: Check before and after each game
- **Audit trail**: Log all game transactions

---

## 9. Performance Optimization

### 9.1 Backend
- Database indexing (UserId, Email, Username)
- Caching for user sessions (Memory Cache)
- Connection pooling for SQLite
- Async/await for all I/O operations
- Pagination for large result sets

### 9.2 Frontend
- Lazy loading for game modules
- OnPush change detection strategy
- Virtual scrolling for transaction history
- Image optimization and sprites
- CSS animations over JavaScript when possible
- Service Worker for offline capabilities (optional)

---

## 10. Deployment Strategy

### 10.1 Backend Deployment
- **Environment**: IIS, Azure App Service, or Docker
- **Database**: SQLite file with backups
- **Configuration**: Environment-specific appsettings
- **Logging**: Serilog to file/database

### 10.2 Frontend Deployment
- Build for production (`ng build --configuration production`)
- Deploy to Azure Static Web Apps, AWS S3, or nginx
- Enable gzip compression
- Configure CDN for static assets

---

## 11. Monitoring & Logging

### 11.1 Application Logging
- Structured logging with Serilog
- Log levels: Debug, Info, Warning, Error
- Log all game transactions
- Track API performance

### 11.2 Monitoring
- Application Insights (Azure) or similar
- Track user activity
- Monitor API response times
- Alert on errors and exceptions

---

## 12. Documentation Requirements

### 12.1 Technical Documentation
- API documentation (Swagger)
- Database schema documentation
- Architecture diagrams
- Setup and deployment guide

### 12.2 User Documentation
- Game rules and instructions
- Account management guide
- FAQ section

---

## 13. Risk Management

### 13.1 Technical Risks
- **Risk**: Complex animation performance issues
  - **Mitigation**: Use CSS animations, test on various devices
  
- **Risk**: Race conditions in balance updates
  - **Mitigation**: Database transactions, optimistic locking

- **Risk**: Random number generation not truly random
  - **Mitigation**: Use RNGCryptoServiceProvider

### 13.2 Business Risks
- **Risk**: Gambling regulations compliance
  - **Mitigation**: Legal review, implement responsible gaming features

---

## 14. Future Enhancements

- Multiplayer poker rooms
- Progressive jackpots
- Social features (leaderboards, achievements)
- Mobile app (iOS/Android)
- Additional games (craps, baccarat)
- Live dealer games
- Payment gateway integration
- Multi-currency support

---

## 15. Success Metrics

- 80%+ unit test coverage
- API response time < 200ms
- Zero critical security vulnerabilities
- All games functional with smooth animations
- Complete user authentication and account management
- Successful game transaction handling

---

This comprehensive plan provides a roadmap for developing a fully functional casino application with the specified tech stack and features. The modular approach ensures each component can be developed, tested, and integrated systematically.