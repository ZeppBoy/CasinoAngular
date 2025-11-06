import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { SlotComponent } from './components/games/slot/slot.component';
import { BlackjackComponent } from './components/games/blackjack/blackjack.component';
import { RouletteComponent } from './components/games/roulette/roulette.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'games/slot', component: SlotComponent, canActivate: [authGuard] },
  { path: 'games/blackjack', component: BlackjackComponent, canActivate: [authGuard] },
  { path: 'games/roulette', component: RouletteComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '/login' }
];
