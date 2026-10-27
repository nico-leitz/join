import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { Contact, ContactRow, CreateContact, UpdateContact } from '../models/contact.model';

/**
 * Manages contact persistence, mapping, sorting and application state.
 */
@Injectable({
  providedIn: 'root',
})
export class ContactService {
  /** Angular HTTP Client used for API requests. */
  private readonly http = inject(HttpClient);

  /** Base URL for the contacts API endpoint. */
  private readonly apiUrl = 'https://localhost:7190/api/contacts';

  /** Currently selected contact or null when no contact is selected. */
  selectedContact = signal<Contact | null>(null);

  /** Complete sorted contact collection held in application state. */
  allContacts = signal<Contact[]>([]);

  /**
   * Retrieves all contacts from the database.
   * @returns Mapped contacts sorted by first and last name.
   */
  async getContacts(): Promise<Contact[]> {
    const data = await firstValueFrom(this.http.get<ContactRow[]>(this.apiUrl));
    return this.sortContacts(this.mapContactRows(data));
  }

  /**
   * Retrieves a single contact by its identifier.
   * @param id Identifier of the requested contact.
   * @returns Mapped contact or null when the contact does not exist.
   */
  async getContactById(id: string): Promise<Contact | null> {
    try {
      const data = await firstValueFrom(this.http.get<ContactRow>(`${this.apiUrl}/${id}`));
      return this.mapContactRow(data);
    } catch {
      return null;
    }
  }

  /**
   * Creates a contact and adds it to the local contact state.
   * @param contact Contact data to persist.
   * @returns Created and mapped contact.
   */
  async createContact(contact: CreateContact): Promise<Contact> {
    const badgeColor = await this.createUniqueBadgeColor();
    const payload = this.createInsertPayload(contact, badgeColor);
    
    const data = await firstValueFrom(this.http.post<ContactRow>(this.apiUrl, payload));

    const createdContact = this.mapContactRow(data);
    this.addContactToState(createdContact);
    this.selectedContact.set(createdContact);

    return createdContact;
  }

  /**
   * Updates a contact and synchronizes the local contact state.
   * @param id Identifier of the contact to update.
   * @param contact Contact fields to persist.
   * @returns Updated and mapped contact.
   */
  async updateContact(id: string, contact: UpdateContact): Promise<Contact> {
    const payload = this.createUpdatePayload(contact);
    
    const data = await firstValueFrom(this.http.put<ContactRow>(`${this.apiUrl}/${id}`, payload));

    const updatedContact = this.mapContactRow(data);
    this.updateContactInState(updatedContact);

    return updatedContact;
  }

  /**
   * Deletes a contact and removes it from the local contact state.
   * @param id Identifier of the contact to delete.
   * @returns A promise that resolves after deletion.
   */
  async deleteContact(id: string): Promise<void> {
    await firstValueFrom(this.http.delete(`${this.apiUrl}/${id}`));
    this.removeContactFromState(id);
  }

  /**
   * Creates uppercase initials from a contact name.
   * @param firstName Contact first name.
   * @param lastName Contact last name.
   * @returns Combined first characters of the trimmed names.
   */
  getInitials(firstName: string, lastName: string): string {
    const firstInitial = firstName.trim().charAt(0);
    const lastInitial = lastName.trim().charAt(0);

    return `${firstInitial}${lastInitial}`.toUpperCase();
  }

  /**
   * Creates a normalized database payload for a new contact.
   * @param contact Contact data to transform.
   * @param badgeColor Badge color assigned to the contact.
   * @returns Contact insert payload using database field names.
   */
  private createInsertPayload(contact: CreateContact, badgeColor: string): Partial<ContactRow> {
    return {
      firstName: contact.firstName.trim(),
      lastName: contact.lastName.trim(),
      email: contact.email.trim(),
      phone: contact.phone?.trim() || null,
      badgeColor: badgeColor,
    };
  }

  /**
   * Creates a database payload containing the provided contact updates.
   * @param contact Contact fields to update.
   * @returns Normalized contact update payload with a new update timestamp.
   */
  private createUpdatePayload(contact: UpdateContact): Partial<ContactRow> {
    const payload: Partial<ContactRow> = { updatedAt: new Date().toISOString() };
    if (contact.firstName !== undefined) payload.firstName = contact.firstName.trim();
    if (contact.lastName !== undefined) payload.lastName = contact.lastName.trim();
    if (contact.email !== undefined) payload.email = contact.email.trim();
    if (contact.phone !== undefined) payload.phone = contact.phone?.trim() || null;
    return payload;
  }

  /**
   * Adds a contact to the local collection and restores sort order.
   * @param contact Contact to add.
   */
  private addContactToState(contact: Contact): void {
    this.allContacts.update((contacts) => {
      return this.sortContacts([...contacts, contact]);
    });
  }

  /**
   * Replaces an existing contact in all relevant local state.
   * @param updatedContact Persisted contact containing the latest values.
   */
  private updateContactInState(updatedContact: Contact): void {
    this.selectedContact.set(updatedContact);
    this.allContacts.update((contacts) => {
      return this.replaceContact(contacts, updatedContact);
    });
  }

  /**
   * Replaces a contact within a collection and sorts the result.
   * @param contacts Contact collection to update.
   * @param updatedContact Contact containing the replacement values.
   * @returns Sorted collection containing the updated contact.
   */
  private replaceContact(contacts: Contact[], updatedContact: Contact): Contact[] {
    const updatedContacts = contacts.map((contact) => {
      return contact.id === updatedContact.id ? updatedContact : contact;
    });

    return this.sortContacts(updatedContacts);
  }

  /**
   * Removes a contact from local state and clears its selection when required.
   * @param contactId Identifier of the contact to remove.
   */
  private removeContactFromState(contactId: string): void {
    this.allContacts.update((contacts) => {
      return contacts.filter((contact) => contact.id !== contactId);
    });

    if (this.selectedContact()?.id === contactId) {
      this.selectedContact.set(null);
    }
  }

  /**
   * Generates a badge color while attempting to avoid colors already in use.
   * @returns Newly generated HSL badge color.
   */
  private async createUniqueBadgeColor(): Promise<string> {
    const usedColors = await this.getUsedBadgeColors();

    for (let attempt = 0; attempt < 20; attempt++) {
      const badgeColor = this.createRandomBadgeColor();

      if (!usedColors.has(badgeColor)) {
        return badgeColor;
      }
    }

    return this.createRandomBadgeColor();
  }

  /**
   * Retrieves all badge colors currently stored for contacts.
   * @returns Set containing the stored badge colors.
   */
  private async getUsedBadgeColors(): Promise<Set<string>> {
    try {
      const contacts = await firstValueFrom(this.http.get<ContactRow[]>(this.apiUrl));
      return new Set(contacts.map((contact) => contact.badgeColor));
    } catch {
      return new Set();
    }
  }

  /**
   * Generates a badge color within the configured HSL ranges.
   * @returns Random HSL color string.
   */
  private createRandomBadgeColor(): string {
    const hue = this.getRandomNumber(0, 359);
    const saturation = this.getRandomNumber(65, 80);
    const lightness = this.getRandomNumber(38, 46);

    return `hsl(${hue} ${saturation}% ${lightness}%)`;
  }

  /**
   * Generates a random integer within an inclusive range.
   * @param minimum Lowest possible integer.
   * @param maximum Highest possible integer.
   * @returns Random integer between the provided boundaries.
   */
  private getRandomNumber(minimum: number, maximum: number): number {
    return Math.floor(Math.random() * (maximum - minimum + 1)) + minimum;
  }

  /**
   * Maps multiple database contact rows to application contacts.
   * @param contactRows Contact rows to transform.
   * @returns Mapped application contacts.
   */
  private mapContactRows(contactRows: ContactRow[]): Contact[] {
    return contactRows.map((contactRow) => {
      return this.mapContactRow(contactRow);
    });
  }

  /**
   * Maps a database contact row to the application contact model.
   * @param contactRow Contact row to transform.
   * @returns Mapped application contact.
   */
  private mapContactRow(contactRow: ContactRow): Contact {
    return {
      id: contactRow.id,
      authUserId: contactRow.authUserId || null,
      firstName: contactRow.firstName,
      lastName: contactRow.lastName,
      email: contactRow.email,
      phone: contactRow.phone,
      badgeColor: contactRow.badgeColor,
      createdAt: contactRow.createdAt,
      updatedAt: contactRow.updatedAt,
    };
  }

  /**
   * Returns a copy of a contact collection sorted by name.
   * @param contacts Contacts to sort.
   * @returns Sorted contact collection.
   */
  private sortContacts(contacts: Contact[]): Contact[] {
    return [...contacts].sort((firstContact, secondContact) => {
      return this.compareContacts(firstContact, secondContact);
    });
  }

  /**
   * Compares two contacts by first name and then by last name.
   * @param firstContact First contact to compare.
   * @param secondContact Second contact to compare.
   * @returns Locale comparison result used for sorting.
   */
  private compareContacts(firstContact: Contact, secondContact: Contact): number {
    return (
      firstContact.firstName.localeCompare(secondContact.firstName) ||
      firstContact.lastName.localeCompare(secondContact.lastName)
    );
  }
}