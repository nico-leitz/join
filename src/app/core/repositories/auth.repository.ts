import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { LoginCredentials, SignUpCredentials } from '../models/auth.model';

/**
 * Contains the registration fields accepted by the repository.
 */
type SignUpPayload = Pick<SignUpCredentials, 'fullName' | 'email' | 'password'>;

@Injectable({
  providedIn: 'root',
})
export class AuthRepository {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'https://localhost:7190/api/auth';

  /**
   * Registers a permanent user via the C# API.
   */
  async signUp(credentials: SignUpPayload): Promise<any> {
    const nameParts = credentials.fullName.trim().split(' ');
    const firstName = nameParts[0];
    const lastName = nameParts.length > 1 ? nameParts.slice(1).join(' ') : '';

    return firstValueFrom(
      this.http.post(`${this.apiUrl}/register`, {
        email: credentials.email.trim(),
        password: credentials.password,
        firstName: firstName,
        lastName: lastName
      })
    );
  }

  /**
   * Signs in a user via the C# API and receives the HttpOnly JWT Cookie.
   */
  async signIn(credentials: LoginCredentials): Promise<any> {
    return firstValueFrom(
      this.http.post(`${this.apiUrl}/login`, {
        email: credentials.email.trim(),
        password: credentials.password,
      })
    );
  }

  /**
   * Signs out the user by telling the C# API to delete the HttpOnly Cookie.
   */
  async signOut(): Promise<void> {
    await firstValueFrom(this.http.post(`${this.apiUrl}/logout`, {}));
  }

  /**
   * Sends a request to establish an anonymous guest session.
   */
  async guestSignIn(): Promise<void> {
    await firstValueFrom(this.http.post(`${this.apiUrl}/guest-login`, {}));
  }
}