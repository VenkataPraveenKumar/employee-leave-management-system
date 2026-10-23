/**
 * Credentials payload submitted during user login.
 */
export interface LoginRequest {
  /** The registered employee work email address. */
  email: string;
  /** The raw password string. */
  password: string;
}

/**
 * Authentication response model emitted by the backend upon successful credential validation.
 */
export interface AuthResponse {
  /** Unique primary key identifier of the user. */
  id: number;
  /** Concatenated full name of the user. */
  fullName: string;
  /** Corporate email address of the user. */
  email: string;
  /** Assigned system authorization role ('Employee' | 'Manager' | 'Admin'). */
  role: string;
  /** Cryptographically signed JWT Bearer token. */
  token: string;
  /** UTC timestamp string when the token expires. */
  expiresAtUtc: string;
}

/**
 * In-memory and persisted user identity profile.
 */
export interface CurrentUser {
  /** Unique primary key identifier. */
  id: number;
  /** Concatenated full name. */
  fullName: string;
  /** User email address. */
  email: string;
  /** User authorization role. */
  role: string;
}
