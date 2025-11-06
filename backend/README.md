# Casino Application - Phase 1 Setup Complete

## Project Overview
A full-stack casino application featuring Slot Machine, Blackjack, Poker, and Roulette games with user authentication and balance management.

## Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0 Web API
- **Database**: SQLite with Entity Framework Core 8.0
- **Authentication**: JWT (JSON Web Tokens)
- **Password Hashing**: BCrypt.Net
- **API Documentation**: Swagger/OpenAPI
- **Testing**: xUnit, Moq

### Frontend
- **Framework**: Angular 17+
- **Language**: TypeScript
- **Styling**: SCSS
- **HTTP Client**: Angular HttpClient
- **Routing**: Angular Router

## Project Structure

```
CasinoAngular/
├── backend/
│   ├── CasinoAPI.API/              # Web API project
│   │   ├── Controllers/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── CasinoAPI.Core/             # Business logic & domain models
│   │   └── Entities/
│   │       ├── User.cs
│   │       ├── Transaction.cs
│   │       ├── GameSession.cs
│   │       └── GameHistory.cs
│   ├── CasinoAPI.Infrastructure/   # Data access & external services
│   │   ├── Data/
│   │   │   └── CasinoDbContext.cs
│   │   └── Migrations/
│   └── CasinoAPI.Tests/            # Unit tests
└── frontend/
    └── casino-app/                 # Angular application
        ├── src/
        │   ├── app/
        │   ├── assets/
        │   └── environments/
        └── angular.json
```

## Database Schema

### Users Table
- UserId (PK)
- Username (Unique)
- Email (Unique)
- PasswordHash
- PasswordSalt
- Balance (default: 1000.00)
- CreatedDate
- LastLoginDate
- IsActive

### Transactions Table
- TransactionId (PK)
- UserId (FK)
- TransactionType (Deposit, Withdrawal, Bet, Win)
- Amount
- BalanceBefore
- BalanceAfter
- GameType
- Description
- CreatedDate

### GameSessions Table
- SessionId (PK)
- UserId (FK)
- GameType
- StartTime
- EndTime
- TotalBets
- TotalWinnings
- TotalLosses

### GameHistory Table
- GameHistoryId (PK)
- SessionId (FK)
- UserId (FK)
- GameType
- BetAmount
- WinAmount
- GameData (JSON)
- PlayedDate

## Setup Instructions

### Backend Setup

1. **Navigate to backend directory:**
   ```bash
   cd backend
   ```

2. **Restore packages:**
   ```bash
   dotnet restore
   ```

3. **Apply migrations and create database:**
   ```bash
   cd CasinoAPI.Infrastructure
   dotnet ef database update --startup-project ../CasinoAPI.API
   ```

4. **Run the API:**
   ```bash
   cd ../CasinoAPI.API
   dotnet run
   ```
   The API will be available at: `https://localhost:7xxx` (check console output)
   Swagger UI: `https://localhost:7xxx/swagger`

### Frontend Setup

1. **Navigate to frontend directory:**
   ```bash
   cd frontend/casino-app
   ```

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Run the development server:**
   ```bash
   ng serve
   ```
   The app will be available at: `http://localhost:4200`

## Configuration

### Backend Configuration (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=casino.db"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration123!",
    "Issuer": "CasinoAPI",
    "Audience": "CasinoClient",
    "ExpirationMinutes": 60
  }
}
```

### CORS Configuration
The backend is configured to accept requests from `http://localhost:4200` (Angular dev server).

## Current Status - Phase 1 Complete ✅

### Completed Tasks:
- [x] Backend solution structure created
- [x] Database entities defined (User, Transaction, GameSession, GameHistory)
- [x] DbContext configured with relationships and indexes
- [x] Initial migration created
- [x] NuGet packages installed:
  - Entity Framework Core (SQLite)
  - JWT Authentication
  - BCrypt for password hashing
  - Swagger/OpenAPI
  - Moq for testing
- [x] Program.cs configured with:
  - DbContext registration
  - CORS policy for Angular app
  - Controller routing
  - Swagger/OpenAPI
- [x] Angular project created with:
  - Routing enabled
  - SCSS styling
  - TypeScript configuration

## Next Steps - Phase 2: Authentication System

### Backend Tasks:
1. Create DTOs (Data Transfer Objects):
   - RegisterDto
   - LoginDto
   - UserProfileDto
   - TokenDto
2. Implement IAuthenticationService
3. Create AuthController with endpoints:
   - POST /api/auth/register
   - POST /api/auth/login
   - POST /api/auth/refresh
   - POST /api/auth/logout
4. Configure JWT authentication in Program.cs
5. Write unit tests for authentication

### Frontend Tasks:
1. Create authentication service
2. Build login component
3. Build register component
4. Implement auth guard
5. Create HTTP interceptor for JWT tokens
6. Set up routing with guards

## Running Tests

### Backend Tests:
```bash
cd backend
dotnet test
```

## API Endpoints (Planned)

### Authentication
- POST `/api/auth/register` - Register new user
- POST `/api/auth/login` - User login
- POST `/api/auth/refresh` - Refresh JWT token
- POST `/api/auth/logout` - User logout

### User Account
- GET `/api/users/profile` - Get user profile
- PUT `/api/users/profile` - Update profile
- GET `/api/users/balance` - Get balance
- POST `/api/users/deposit` - Add funds
- POST `/api/users/withdraw` - Withdraw funds
- GET `/api/users/transactions` - Transaction history

### Games
- POST `/api/games/slot/spin` - Play slot machine
- POST `/api/games/blackjack/start` - Start blackjack
- POST `/api/games/poker/start` - Start poker
- POST `/api/games/roulette/spin` - Spin roulette
- GET `/api/games/history` - Game history

## Development Timeline

- **Week 1**: ✅ Foundation Setup (COMPLETED)
- **Week 2**: Authentication System (IN PROGRESS)
- **Week 3**: User Account Management
- **Week 4**: Slot Machine Game
- **Week 5-6**: Blackjack & Poker Games
- **Week 7**: Roulette Game
- **Week 8**: Polish & Deployment

## Security Features

- Password hashing with BCrypt
- JWT token-based authentication
- HTTPS enforcement
- CORS protection
- SQL injection prevention (EF Core parameterized queries)
- Input validation
- Server-side game logic validation

## Contributing

This is a development project following the casino application specification from brAngular.md.

## License

Proprietary - Casino Application Development Project
