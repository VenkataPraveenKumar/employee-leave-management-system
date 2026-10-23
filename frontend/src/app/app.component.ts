import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/components/navbar/navbar.component';

/**
 * Root component that acts as the top-level shell and renders matched routes via RouterOutlet.
 */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent],
  template: `
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
