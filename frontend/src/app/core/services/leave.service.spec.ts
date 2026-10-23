import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { LeaveService } from './leave.service';
import { environment } from '../../../environments/environment';
import { LeaveBalance, LeaveType } from '../models/leave.models';

/**
 * Unit test suite for LeaveService verifying API resource endpoints and HTTP verbs.
 */
describe('LeaveService', () => {
  let service: LeaveService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        LeaveService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(LeaveService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should retrieve current user leave balances via GET', () => {
    const mockBalances: LeaveBalance[] = [
      { id: 1, leaveType: 'Paid', totalDays: 20, usedDays: 5, remainingDays: 15, year: 2026 }
    ];

    service.getMyBalances().subscribe((balances) => {
      expect(balances.length).toBe(1);
      expect(balances[0].remainingDays).toBe(15);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/leaves/balances`);
    expect(req.request.method).toBe('GET');
    req.flush(mockBalances);
  });

  it('should post a new leave application via POST', () => {
    const payload = {
      leaveType: LeaveType.Sick,
      startDate: '2026-11-01',
      endDate: '2026-11-03',
      reason: 'Medical checkup'
    };

    service.submitLeaveRequest(payload).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/leaves/request`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });
});
