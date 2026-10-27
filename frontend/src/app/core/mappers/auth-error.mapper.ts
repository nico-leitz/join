import { HttpErrorResponse } from '@angular/common/http';

/**
 * Fallback message for unknown or unsupported authentication errors.
 */
const DEFAULT_AUTH_ERROR = 'Authentication failed. Please try again.';

/**
 * Maps a C# API authentication error to a safe UI message.
 * @param error - Unknown error returned by an authentication request.
 * @returns Matching user-facing message or the default error message.
 */
export function mapAuthErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (typeof error.error === 'string') return error.error;
    if (error.error?.message) return error.error.message;
    if (error.status === 401 || error.status === 403) return 'Invalid email or password.';
  }
  return error instanceof Error ? error.message : DEFAULT_AUTH_ERROR;
}