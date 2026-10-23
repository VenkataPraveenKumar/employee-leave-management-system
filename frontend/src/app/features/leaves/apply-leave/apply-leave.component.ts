import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
  ValidationErrors
} from '@angular/forms';
import { Router } from '@angular/router';
import { LeaveService } from '../../../core/services/leave.service';
import { LeaveType, LeaveBalance } from '../../../core/models/leave.models';

/**
 * Custom validator ensuring the requested end date is not earlier than the start date.
 * @param control Abstract form group containing startDate and endDate controls.
 * @returns Validation error object if invalid; otherwise null.
 */
function dateRangeValidator(control: AbstractControl): ValidationErrors | null {
  const start = control.get('startDate')?.value;
  const end = control.get('endDate')?.value;
  if (start && end && new Date(start) > new Date(end)) {
    return { dateRangeInvalid: true };
  }
  return null;
}

/**
 * Component providing an interactive booking form for submitting employee leave applications.
 */
@Component({
  selector: 'app-apply-leave',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './apply-leave.component.html',
  styleUrls: ['./apply-leave.component.scss']
})
export class ApplyLeaveComponent implements OnInit {
  /** Reactive form tracking user input and field validations. */
  public leaveForm: FormGroup;
  /** Signal containing available leave types for the select dropdown. */
  public leaveTypes = [
    { value: LeaveType.Sick, label: 'Sick Leave' },
    { value: LeaveType.Casual, label: 'Casual Leave' },
    { value: LeaveType.Paid, label: 'Paid Leave' },
    { value: LeaveType.Unpaid, label: 'Unpaid Leave' }
  ];
  /** Signal holding current leave balances for quota reference. */
  public balances = signal<LeaveBalance[]>([]);
  /** Loading state tracking submission in-flight status. */
  public isSubmitting = signal<boolean>(false);
  /** Error message signal for submission or network failures. */
  public errorMessage = signal<string | null>(null);

  /**
   * Initializes dependencies and sets up the reactive form structure.
   * @param fb FormBuilder utility.
   * @param leaveService Service handling leave transactions.
   * @param router Angular router for navigation.
   */
  constructor(
    private fb: FormBuilder,
    private leaveService: LeaveService,
    private router: Router
  ) {
    const today = new Date().toISOString().split('T')[0];
    this.leaveForm = this.fb.group(
      {
        leaveType: [LeaveType.Casual, [Validators.required]],
        startDate: [today, [Validators.required]],
        endDate: [today, [Validators.required]],
        reason: ['', [Validators.required, Validators.maxLength(500)]]
      },
      { validators: dateRangeValidator }
    );
  }

  /**
   * Fetches active user balances on component initialization.
   */
  public ngOnInit(): void {
    this.leaveService.getMyBalances().subscribe({
      next: (data) => this.balances.set(data),
      error: () => this.errorMessage.set('Could not retrieve current leave balances.')
    });
  }

  /**
   * Submits the leave application to the backend API.
   */
  public onSubmit(): void {
    if (this.leaveForm.invalid) {
      this.leaveForm.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    this.isSubmitting.set(true);

    const formValue = this.leaveForm.value;
    this.leaveService
      .submitLeaveRequest({
        leaveType: Number(formValue.leaveType),
        startDate: formValue.startDate,
        endDate: formValue.endDate,
        reason: formValue.reason
      })
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.router.navigate(['/leaves/history']);
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.errorMessage.set(
            err.error?.detail || err.error?.message || 'Failed to submit leave request.'
          );
        }
      });
  }
}
