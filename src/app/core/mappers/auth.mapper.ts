import { AuthUser } from '../models/auth.model';

/**
 * Expected shape of the user object returned by the C# API.
 */
export interface ApiAuthUserDto {
  id: string;
  email?: string | null;
  fullName?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  isAnonymous?: boolean | null;
}

/**
 * Contains the separated parts of a normalized full name.
 */
interface NameParts {
  /** First name or its fallback value. */
  firstName: string;

  /** Last name or its fallback value. */
  lastName: string;
}

/**
 * Represents the user metadata stored during registration.
 */
export interface AuthUserMetadata {
  /** User's first name. */
  firstName: string;

  /** User's last name. */
  lastName: string;

  /** User's normalized full name. */
  fullName: string;
}

/**
 * Creates normalized user metadata for a sign-up request.
 * @param fullName - Full name entered during registration.
 * @returns Metadata containing normalized full and separated names.
 */
export function createAuthUserMetadata(fullName: string): AuthUserMetadata {
  const normalizedName = normalizeFullName(fullName);
  const nameParts = splitFullName(normalizedName);

  return {
    firstName: nameParts.firstName,
    lastName: nameParts.lastName,
    fullName: normalizedName,
  };
}

/**
 * Maps a C# API user to the application's authentication user model.
 * @param user - API user to transform.
 * @returns Application authentication user.
 */
export function mapAuthUser(user: ApiAuthUserDto): AuthUser {
  if (!user) {
    throw new Error('Cannot map an empty user object.');
  }

  return {
    id: user.id,
    email: user.email ?? null,
    fullName: user.fullName ?? resolveNameFallback(user),
    isAnonymous: user.isAnonymous ?? false,
  };
}

/**
 * Trims a full name and reduces consecutive whitespace.
 * @param fullName - Full name to normalize.
 * @returns Normalized full name.
 */
function normalizeFullName(fullName: string): string {
  return fullName.trim().replace(/\s+/g, ' ');
}

/**
 * Splits a normalized full name into contact-compatible name values.
 * @param fullName - Normalized full name to split.
 * @returns Separated first and last names with safe fallback values.
 */
function splitFullName(fullName: string): NameParts {
  const [firstName, ...lastNameParts] = fullName.split(' ');

  return {
    firstName: firstName || 'User',
    lastName: lastNameParts.join(' ') || 'Unknown',
  };
}

/**
 * Combines separate name values or returns a fallback.
 * @param user - API user containing the metadata.
 * @returns Combined name or the appropriate fallback name.
 */
function resolveNameFallback(user: ApiAuthUserDto): string {
  const firstName = typeof user.firstName === 'string' ? user.firstName.trim() : '';
  const lastName = typeof user.lastName === 'string' ? user.lastName.trim() : '';
  const resolvedName = normalizeFullName(`${firstName} ${lastName}`);

  return resolvedName || (user.isAnonymous ? 'Guest' : 'User');
}