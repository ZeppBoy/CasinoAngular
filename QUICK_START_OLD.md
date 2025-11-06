# 🎰 Casino Application - Quick Start Guide

## ✅ Phase 1 Setup Complete!

All foundation components are ready. You can now start development immediately.

## 🚀 Running the Application

### Option 1: Run Both (Recommended)

**Terminal 1 - Backend:**
```bash
cd backend/CasinoAPI.API
dotnet run
```
✅ API will start at: https://localhost:5001  
✅ Swagger UI at: https://localhost:5001/swagger

**Terminal 2 - Frontend:**
```bash
cd frontend/casino-app
ng serve
```
✅ App will start at: http://localhost:4200

### Option 2: Backend Only
```bash
cd backend/CasinoAPI.API
dotnet run
```
Then visit https://localhost:5001/swagger to test the API

## 📦 What's Installed

### Backend
- ✅ ASP.NET Core 8.0 Web API
- ✅ Entity Framework Core with SQLite
- ✅ JWT Authentication (ready to implement)
- ✅ BCrypt for password hashing
- ✅ Swagger/OpenAPI documentation
- ✅ Moq for unit testing
- ✅ Database created with 4 tables (Users, Transactions, GameSessions, GameHistories)

### Frontend
- ✅ Angular 17+ with TypeScript
- ✅ SCSS styling
- ✅ Angular Router configured
- ✅ Ready for HTTP client integration

## 🗂️ Project Structure

```
CasinoAngular/
├── backend/
│   ├── CasinoAPI.API/          # Main API project
│   │   ├── casino.db           # SQLite database ✅
│   │   ├── Program.cs          # Configured with CORS, DbContext
│   │   └── appsettings.json    # JWT & DB settings
│   ├── CasinoAPI.Core/         # Domain models
│   │   └── Entities/           # User, Transaction, GameSession, GameHistory
│   ├── CasinoAPI.Infrastructure/
│   │   ├── Data/               # DbContext
│   │   └── Migrations/         # Initial migration applied ✅
│   └── CasinoAPI.Tests/        # xUnit tests
└── frontend/
    └── casino-app/             # Angular application
        └── src/app/            # Application code
```

## 🔍 Verify Installation

```bash
# Check backend builds
cd backend
dotnet build
# Expected: Build succeeded

# Check database exists
ls -la backend/CasinoAPI.API/casino.db
# Expected: File exists (65KB)

# Check frontend
cd frontend/casino-app
npm list --depth=0
# Expected: All packages installed
```

## 📋 Database Schema

The following tables are ready to use:

1. **Users** - User accounts & authentication
   - Username (unique)
   - Email (unique)  
   - PasswordHash/Salt
   - Balance (default: 1000.00)

2. **Transactions** - Financial history
   - Track deposits, withdrawals, bets, wins

3. **GameSessions** - Game session tracking
   - Track start/end times, total bets, winnings

4. **GameHistories** - Detailed game records
   - Individual game results with JSON data

## 🎯 Next Steps (Phase 2)

### Backend Tasks:
1. Create DTOs for authentication
2. Implement AuthenticationService with JWT
3. Create AuthController (register, login endpoints)
4. Write unit tests

### Frontend Tasks:
1. Create auth service
2. Build login/register components
3. Implement auth guard
4. Create JWT interceptor

## 📚 Documentation

- **README.md** - Full project documentation
- **PHASE1_COMPLETE.md** - Detailed completion report
- **brAngular.md** - Original requirements & plan

## 🔐 Configuration

**CORS:** Configured to accept requests from http://localhost:4200

**JWT Settings (appsettings.json):**
- Secret: "YourSuperSecretKeyForJWTTokenGeneration123!"
- Expiration: 60 minutes
- Issuer: CasinoAPI
- Audience: CasinoClient

**Database:** SQLite file at `backend/CasinoAPI.API/casino.db`

## ⚡ Quick Commands

```bash
# Build backend
cd backend && dotnet build

# Run tests
cd backend && dotnet test

# Run API
cd backend/CasinoAPI.API && dotnet run

# Run Angular app
cd frontend/casino-app && ng serve

# Add migration (when needed)
cd backend/CasinoAPI.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../CasinoAPI.API

# Update database
dotnet ef database update --startup-project ../CasinoAPI.API
```

## 🎮 Development Roadmap

- ✅ **Week 1:** Foundation Setup (COMPLETE)
- 🔄 **Week 2:** Authentication System (NEXT)
- 📅 **Week 3:** User Account Management
- 📅 **Week 4:** Slot Machine Game
- 📅 **Week 5-6:** Blackjack & Poker
- 📅 **Week 7:** Roulette
- 📅 **Week 8:** Polish & Deployment

---

**Status:** ✅ Ready for Phase 2 Development!

Happy coding! 🚀
