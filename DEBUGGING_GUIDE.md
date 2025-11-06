# 🎰 Casino App - Debugging Guide

## Current Status

**Backend**: ✅ Running on http://localhost:5015  
**Frontend**: ✅ Running on http://localhost:4200  
**Issue**: Blank page or redirect loops

---

## Step-by-Step Debugging

### 1. Open Browser Console

1. Open Chrome/Firefox
2. Navigate to: **http://localhost:4200**
3. Press **F12** to open DevTools
4. Go to **Console** tab

**What to look for**:
- Any red error messages
- Any warnings about failed HTTP requests
- Any messages starting with `[Auth Guard]`, `[Auth Interceptor]`, or `[Login]`

---

### 2. Check If Page Loads

**If you see a BLANK WHITE PAGE**:
- Check Console for errors
- Look for messages like "Cannot find module" or "Unexpected token"
- The app might not be building correctly

**If you see the LOGIN PAGE**:
- ✅ Good! The app is working
- Try logging in with:
  - **Username**: `testuser`
  - **Password**: `Test123!`

**If you see a different page**:
- Tell me what you see

---

### 3. Test Login Flow

1. Enter credentials:
   - Username: `testuser`
   - Password: `Test123!`

2. Click **Login**

3. Watch the Console for messages:
   ```
   [Login] Attempting login... {usernameOrEmail: 'testuser', password: 'Test123!'}
   [Login] Success! Response: {token: '...', user: {...}}
   [Login] Token stored: true
   [Auth Guard] Checking authentication for: /dashboard Result: true
   [Auth Guard] Access granted
   ```

4. Check **Network** tab:
   - Should see a POST request to `http://localhost:5015/api/auth/login`
   - Status should be **200 OK**
   - Response should contain `token`, `refreshToken`, and `user`

---

### 4. Check Local Storage

After successful login:

1. In DevTools, go to **Application** tab
2. Expand **Local Storage** > `http://localhost:4200`
3. You should see:
   - `token`: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...` (long string)
   - `refreshToken`: `L6VcEcqeuTD37xHI9esScuiEap4SvuA9...` (long string)
   - `currentUser`: `{"userId":1,"username":"testuser",...}` (JSON)

**If these are missing**:
- Login failed or AuthService didn't store them
- Check Console for errors

---

### 5. Test Dashboard Access

After login, you should see:
- A purple gradient background
- Navigation bar at the top with:
  - "🎰 Casino" on the left
  - Balance: $1,350.00
  - Username: testuser
  - Logout button
- Three game cards:
  - 🎰 Slot Machine
  - 🃏 Blackjack
  - 🎲 Roulette

**If you don't see this**:
- Check Console for errors
- Look for messages about failed navigation

---

### 6. Test Slot Machine Access

1. Click on **"Slot Machine"** card or "Play Now" button
2. Watch Console for:
   ```
   [Auth Guard] Checking authentication for: /games/slot Result: true
   [Auth Guard] Access granted
   ```

**If you get redirected back to login**:
- The auth guard thinks you're not authenticated
- Check Console for:
   ```
   [Auth Guard] Checking authentication for: /games/slot Result: false
   [Auth Guard] Access denied, redirecting to login
   ```
- This means token is missing from localStorage

**If you see the Slot Machine page**:
- ✅ Great! You should see:
  - A 3x3 grid of symbols
  - Bet amount input
  - SPIN button
  - Quick bet buttons ($10, $25, $50, $100)

---

## Common Issues & Fixes

### Issue 1: Blank White Page

**Cause**: Build or routing error

**Fix**:
1. Check Console for errors
2. Try restarting Angular dev server:
   ```bash
   # Stop server (Ctrl+C in terminal)
   cd /Users/viktorshershnov/AI/Projects/CasinoAngular/frontend/casino-app
   ng serve --port 4200
   ```

### Issue 2: "Cannot GET /dashboard" Error

**Cause**: Angular routing issue

**Fix**: Make sure you're accessing `http://localhost:4200` (not `http://localhost:4200/dashboard` directly)

### Issue 3: Login Fails with 401 or 500

**Cause**: Backend not running or wrong credentials

**Fix**:
1. Verify backend is running:
   ```bash
   curl http://localhost:5015/api/auth/login
   ```
2. Check username and password are correct
3. Try creating a new user via Register

### Issue 4: Redirect Loop (Login → Dashboard → Login)

**Cause**: Token not being stored or retrieved

**Fix**:
1. After login, check localStorage (step 4 above)
2. If token is there, check auth guard logs
3. If token is missing, check AuthService.handleAuthResponse()

### Issue 5: CORS Error

**Cause**: Backend rejecting frontend requests

**Symptoms**: Console shows error like:
```
Access to XMLHttpRequest at 'http://localhost:5015/api/auth/login' from origin 'http://localhost:4200' has been blocked by CORS policy
```

**Fix**: Backend CORS is already configured, but verify it's running

---

## Manual Backend Test (Backup Plan)

If frontend isn't working, test backend directly:

### Using Swagger UI:
1. Open: **http://localhost:5015/swagger**
2. Test endpoints manually

### Using curl:
```bash
# Test login
curl -X POST http://localhost:5015/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"usernameOrEmail":"testuser","password":"Test123!"}'

# Should return:
# {"token":"eyJ...","refreshToken":"L6V...","user":{...}}
```

---

## Quick Test Checklist

- [ ] Backend running on port 5015
- [ ] Frontend running on port 4200
- [ ] Browser at http://localhost:4200
- [ ] Console open (F12)
- [ ] No errors in Console
- [ ] Login page visible
- [ ] Login with testuser/Test123! succeeds
- [ ] Token stored in localStorage
- [ ] Dashboard loads after login
- [ ] Balance shows $1,350.00
- [ ] Clicking Slot Machine navigates to /games/slot
- [ ] Slot game page loads
- [ ] Can place bet and spin

---

## What to Report Back

Please copy and paste the following information:

1. **What you see on http://localhost:4200**:
   - [ ] Blank white page
   - [ ] Login page
   - [ ] Dashboard
   - [ ] Error page
   - [ ] Other: _______________

2. **Console errors** (copy/paste from Console tab):
   ```
   (paste here)
   ```

3. **Network requests** (from Network tab during login):
   - Request URL: _______________
   - Status Code: _______________
   - Response: _______________

4. **localStorage contents** (from Application tab):
   - token: _______________
   - refreshToken: _______________
   - currentUser: _______________

5. **What happens when you click "Slot Machine"**:
   - [ ] Navigates to slot game
   - [ ] Redirects to login
   - [ ] Nothing happens
   - [ ] Error message
   - [ ] Other: _______________

---

## Emergency Contact Info

If nothing works:
1. Take screenshots of Console and Network tabs
2. Copy all error messages
3. Send me the details above

I can then provide specific fixes based on what you're seeing.

---

## Test User Credentials

**For Testing**:
- Username: `testuser`
- Password: `Test123!`
- Balance: $1,350.00

**Or Register a New User**:
- Go to Register page
- Create new account
- Start with $1,000 balance

---

Good luck! 🎰🍀

