import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

/**
 * Standalone component handling user authentication, credential submission, and error display.
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  /** Reactive form instance managing email and password input state. */
  public loginForm: FormGroup;
  /** Signal holding error alert messages returned from the API. */
  public errorMessage = signal<string | null>(null);
  /** Signal tracking in-flight submission state to disable UI buttons and display loading indicators. */
  public isSubmitting = signal<boolean>(false);

  /**
   * Initializes form validation rules and injects authentication services.
   * @param fb FormBuilder utility for constructing reactive forms.
   * @param authService Service handling authentication transactions.
   * @param router Angular router for redirection upon successful login.
   */
  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  /**
   * Submits the reactive form, triggers the authentication service, and directs the user to the dashboard.
   */
  public onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    this.isSubmitting.set(true);

    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        const detail = err.error?.detail || err.error?.message || 'Authentication failed. Please verify credentials.';
        this.errorMessage.set(detail);
      }
    });
  }

  /**
   * Convenience helper to fill the form with demo employee credentials.
   */
  public fillDemoEmployee(): void {
    this.loginForm.patchValue({
      email: 'employee@leaveapp.com',
      password: 'Password@123'
    });
  }

  /**
   * Convenience helper to fill the form with demo manager credentials.
   */
  public fillDemoManager(): void {
    this.loginForm.patchValue({
      email: 'manager@leaveapp.com',
      password: 'Password@123'
    });
  }
}
