import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { ApplyLeaveComponent } from './features/leaves/apply-leave/apply-leave.component';
import { LeaveHistoryComponent } from './features/leaves/leave-history/leave-history.component';
import { PendingApprovalsComponent } from './features/approvals/pending-approvals/pending-approvals.component';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

/**
 * Top-level application route definitions with authentication and role-based guards.
 */
export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'leaves/apply', component: ApplyLeaveComponent, canActivate: [authGuard] },
  { path: 'leaves/history', component: LeaveHistoryComponent, canActivate: [authGuard] },
  { path: 'approvals', component: PendingApprovalsComponent, canActivate: [authGuard, roleGuard] },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: 'dashboard' }
];
