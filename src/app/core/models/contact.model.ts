/**
 * Represents a contact row returned by the database.
 */
/**
 * Represents a contact row returned by the C# database.
 */
export interface ContactRow {
  /** Unique contact identifier. */
  id: string;

  /** Contact first name stored in the database. */
  firstName: string;

  /** Contact last name stored in the database. */
  lastName: string;

  /** Contact email address. */
  email: string;

  /** Contact phone number or null when none is provided. */
  phone: string | null;

  /** Color used for the contact badge. */
  badgeColor: string;

  /** Related user identifier or null for unlinked contacts. */
  authUserId: string | null;

  /** ISO timestamp indicating when the contact was created. */
  createdAt: string;

  /** ISO timestamp indicating when the contact was last updated. */
  updatedAt: string;
}
/**
 * Represents a contact within the application.
 */
export interface Contact {
  /** Unique contact identifier. */
  id: string;

  /** Contact first name. */
  firstName: string;

  /** Contact last name. */
  lastName: string;

  /** Contact email address. */
  email: string;

  /** Contact phone number or null when none is provided. */
  phone: string | null;

  /** Color used for the contact badge. */
  badgeColor: string;

  /** Related Supabase user identifier or null for unlinked contacts. */
  authUserId: string | null;

  /** ISO timestamp indicating when the contact was created. */
  createdAt: string;

  /** ISO timestamp indicating when the contact was last updated. */
  updatedAt: string;
}

/**
 * Contains the data required to create a contact.
 */
export interface CreateContact {
  /** Contact first name. */
  firstName: string;

  /** Contact last name. */
  lastName: string;

  /** Contact email address. */
  email: string;

  /** Optional contact phone number. */
  phone?: string | null;
}

/**
 * Contains the contact fields that can be updated.
 */
export interface UpdateContact {
  /** Updated contact first name. */
  firstName?: string;

  /** Updated contact last name. */
  lastName?: string;

  /** Updated contact email address. */
  email?: string;

  /** Updated contact phone number. */
  phone?: string | null;
}