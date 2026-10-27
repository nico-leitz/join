import { computed, Injectable, inject, OnDestroy, signal } from '@angular/core';
import { LoginCredentials, SignUpCredentials } from '../models/auth.model';
import { AuthRepository } from '../repositories/auth.repository';

/**
 * Defines the greeting variants that can be shown after summary navigation.
 */
type SummaryGreetingMode = 'user' | 'guest';

/**
 * Represents the authenticated user within the application state.
 */
export interface AppUser {
  id: string;
  email: string;
  fullName: string;
  isAnonymous: boolean;
}

/**
 * Manages authentication requests and exposes the current session state.
 */
@Injectable({
  providedIn: 'root',
})
export class AuthService implements OnDestroy {
  /** Repository used for backend authentication operations. */
  private readonly authRepository = inject(AuthRepository);

  /** Internal state containing the authenticated application user. */
  private readonly currentUserState = signal<AppUser | null>(null);

  /** Internal state indicating whether an authentication request is running. */
  private readonly loadingState = signal(false);

  /** Internal state indicating whether session restoration has completed. */
  private readonly initializedState = signal(false);

  /** Internal state containing the current user-facing authentication error. */
  private readonly errorState = signal<string | null>(null);

  /** Internal state containing a queued one-time summary greeting. */
  private readonly summaryGreetingModeState = signal<SummaryGreetingMode | null>(null);

  /** Shared initialization promise used to prevent duplicate restoration. */
  private initializationPromise: Promise<void> | null = null;

  /** Read-only signal containing the authenticated user or null. */
  readonly currentUser = this.currentUserState.asReadonly();

  /** Read-only signal indicating whether a request is running. */
  readonly isLoading = this.loadingState.asReadonly();

  /** Read-only signal indicating whether authentication was initialized. */
  readonly isInitialized = this.initializedState.asReadonly();

  /** Read-only signal containing the current authentication error message. */
  readonly errorMessage = this.errorState.asReadonly();

  /** Computed signal indicating whether a user is authenticated. */
  readonly isAuthenticated = computed(() => {
    return this.currentUserState() !== null;
  });

  /** Computed signal indicating whether the authenticated user is a guest. */
  readonly isGuest = computed(() => {
    return this.currentUserState()?.isAnonymous ?? false;
  });

  /**
   * Restores the persisted session.
   * @returns Shared promise that resolves when initialization is complete.
   */
  initialize(): Promise<void> {
    this.initializationPromise ??= this.initializeAuth();

    return this.initializationPromise;
  }

  /**
   * Registers a permanent user after privacy acceptance.
   * @param credentials Registration credentials and privacy acceptance state.
   * @returns True when registration creates an authenticated session.
   */
  async signUp(credentials: SignUpCredentials): Promise<boolean> {
    if (!credentials.privacyAccepted) {
      this.errorState.set('Please accept the Privacy Policy.');
      return false;
    }

    return this.runRequest(() => this.performSignUp(credentials), false);
  }

  /**
   * Signs in with email and password.
   * @param credentials Credentials used for the login request.
   * @returns True when an authenticated session was established.
   */
  signIn(credentials: LoginCredentials): Promise<boolean> {
    return this.runRequest(() => this.performSignIn(credentials), false);
  }

  /**
   * Creates and activates an anonymous guest session.
   * @returns True when an anonymous session was established.
   */
  signInAsGuest(): Promise<boolean> {
    return this.runRequest(() => this.performGuestSignIn(), false);
  }

  /**
   * Signs out the active user.
   * @returns True when the active session was signed out successfully.
   */
  signOut(): Promise<boolean> {
    return this.runRequest(() => this.performSignOut(), false);
  }

  /**
   * Clears the current authentication error.
   */
  clearError(): void {
    this.errorState.set(null);
  }

  /**
   * Queues a one-time mobile greeting for the next summary navigation.
   * @param mode Greeting variant to queue.
   */
  queueSummaryGreeting(mode: SummaryGreetingMode): void {
    this.summaryGreetingModeState.set(mode);
  }

  /**
   * Consumes and clears the queued summary greeting mode.
   * @returns Queued greeting mode or null when no greeting is pending.
   */
  consumeSummaryGreeting(): SummaryGreetingMode | null {
    const mode = this.summaryGreetingModeState();
    this.summaryGreetingModeState.set(null);

    return mode;
  }

  /**
   * Releases resources on destruction.
   */
  ngOnDestroy(): void {
    // Supabase listener removed
  }

  /**
   * Initializes session state exactly once.
   * @returns A promise that resolves after session restoration finishes.
   */
  private async initializeAuth(): Promise<void> {
    this.initializedState.set(true);
  }

  /**
   * Completes the registration operation.
   * @param credentials Validated registration credentials.
   * @returns True when the registration response contains an active session.
   * @throws The authentication error returned by the repository.
   */
  private async performSignUp(credentials: SignUpCredentials): Promise<boolean> {
    await this.authRepository.signUp(credentials);
    
    return this.performSignIn({ email: credentials.email, password: credentials.password });
  }

  /**
   * Completes the password sign-in operation.
   * @param credentials Credentials used for the login request.
   * @returns True when an authenticated session was applied.
   * @throws The authentication error returned by the repository.
   */
  private async performSignIn(credentials: LoginCredentials): Promise<boolean> {
    await this.authRepository.signIn(credentials);

    this.currentUserState.set({
      id: 'backend-user',
      email: credentials.email,
      fullName: credentials.email.split('@')[0], 
      isAnonymous: false
    });

    return true;
  }

  /**
   * Completes the anonymous sign-in operation.
   * @returns True when an authenticated guest session was applied.
   */
  private async performGuestSignIn(): Promise<boolean> {
    this.errorState.set('Guest login is no longer supported by the custom backend.');
    return false;
  }

  /**
   * Completes the sign-out operation.
   * @returns True when the repository signs out successfully.
   */
  private async performSignOut(): Promise<boolean> {
    await this.authRepository.signOut();
    this.currentUserState.set(null);

    return true;
  }

  /**
   * Executes one authentication request at a time.
   * @param request Authentication operation to execute.
   * @param fallback Value returned when the request is blocked or fails.
   * @returns Request result or the provided fallback value.
   */
  private async runRequest<T>(request: () => Promise<T>, fallback: T): Promise<T> {
    if (this.loadingState()) {
      return fallback;
    }

    this.startRequest();

    try {
      return await request();
    } catch (error: any) {
      const msg = error.error?.message || 'An error occurred during authentication.';
      this.errorState.set(msg);
      return fallback;
    } finally {
      this.loadingState.set(false);
    }
  }

  /**
   * Starts a clean authentication request.
   */
  private startRequest(): void {
    this.errorState.set(null);
    this.loadingState.set(true);
  }
}