import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginCredentials, SignUpCredentials } from '../models/auth.model';
import { ApiAuthUserDto } from '../mappers/auth.mapper';

/**
 * Contains the registration fields accepted by the repository.
 */
type SignUpPayload = Pick<SignUpCredentials, 'fullName' | 'email' | 'password'>;

/**
 * Handles HTTP requests for authentication against the C# backend.
 */
@Injectable({
  providedIn: 'root',
})
export class AuthRepository {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/auth`;

  /**
   * Retrieves the currently authenticated user based on the HttpOnly cookie.
   * @returns A promise resolving to the user data from the API.
   */
  async getCurrentUser(): Promise<ApiAuthUserDto> {
    return firstValueFrom(this.http.get<ApiAuthUserDto>(`${this.apiUrl}/me`));
  }

  /**
   * Registers a permanent user via the C# API.
   * @param credentials - The user's registration payload.
   * @returns A promise that resolves when registration is successful.
   */
  async signUp(credentials: SignUpPayload): Promise<void> {
    const nameParts = credentials.fullName.trim().split(' ');
    const firstName = nameParts[0];
    const lastName = nameParts.length > 1 ? nameParts.slice(1).join(' ') : '';

    await firstValueFrom(
      this.http.post<void>(`${this.apiUrl}/register`, {
        email: credentials.email.trim(),
        password: credentials.password,
        firstName: firstName,
        lastName: lastName
      })
    );
  }

  /**
   * Signs in a user via the C# API and receives the HttpOnly JWT Cookie.
   * @param credentials - The user's login credentials.
   * @returns A promise that resolves when the sign-in is successful.
   */
  async signIn(credentials: LoginCredentials): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${this.apiUrl}/login`, {
        email: credentials.email.trim(),
        password: credentials.password,
      })
    );
  }

  /**
   * Signs out the user by telling the C# API to delete the HttpOnly Cookie.
   * @returns A promise that resolves when the sign-out is successful.
   */
  async signOut(): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${this.apiUrl}/logout`, {}));
  }

  /**
   * Sends a request to establish an anonymous guest session.
   * @returns A promise that resolves when the guest session is established.
   */
  async guestSignIn(): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${this.apiUrl}/guest-login`, {}));
  }
}