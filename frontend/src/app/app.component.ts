import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/components/navbar/navbar.component';
import { ToastContainerComponent } from './shared/components/toast-container/toast-container.component';

/**
 * Root component that acts as the top-level shell, rendering the navigation bar,
 * active route container, and global toast notification stack.
 */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, ToastContainerComponent],
  template: `
    <app-toast-container></app-toast-container>
    <app-navbar></app-navbar>
    <main>
      <router-outlet></router-outlet>
    </main>
  `
})
export class AppComponent {
  /** Title identifier of the application. */
  public title = 'employee-leave-management-system-ui';
}
