import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { createTaskAssignmentRow } from '../mappers/task-payload.mapper';
import { Contact } from '../models/contact.model';
import { TaskAssignmentRow } from '../models/task-assignment.model';

/**
 * Provides persistence operations for task-to-contact assignments.
 */
@Injectable({
  providedIn: 'root',
})
export class TaskAssignmentRepository {
  /** Angular HTTP Client used for API requests. */
  private readonly http = inject(HttpClient);

  /** Base URLs for the API endpoints. */
  private readonly apiUrl = 'https://localhost:7190/api/task-assignments';
  private readonly tasksApiUrl = 'https://localhost:7190/api/tasks';

  /**
   * Retrieves the full contacts assigned to a task.
   * @param taskId Identifier of the task.
   * @returns Mapped contacts assigned to the task.
   */
  async getAssignedContacts(taskId: string): Promise<Contact[]> {
    return firstValueFrom(this.http.get<Contact[]>(`${this.tasksApiUrl}/${taskId}/contacts`));
  }

  /**
   * Retrieves the identifiers of contacts assigned to a task.
   * @param taskId Identifier of the task.
   * @returns Assigned contact identifiers.
   */
  async getAssignedContactIds(taskId: string): Promise<string[]> {
    const assignments = await firstValueFrom(
      this.http.get<TaskAssignmentRow[]>(`${this.apiUrl}?taskId=${taskId}`)
    );
    return assignments.map(a => a.contactId);
  }

  /**
   * Retrieves all task assignment rows required by the board.
   * @returns Assignment rows ordered by task and creation time.
   */
  async getAllAssignmentRows(): Promise<TaskAssignmentRow[]> {
    const assignments = await firstValueFrom(this.http.get<TaskAssignmentRow[]>(this.apiUrl));
    
    return assignments.sort((a, b) => {
      if (a.taskId !== b.taskId) {
        return a.taskId.localeCompare(b.taskId);
      }
      return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
    });
  }

  /**
   * Assigns a contact to a task.
   * @param taskId Identifier of the task.
   * @param contactId Identifier of the contact to assign.
   * @returns A promise that resolves after assignment.
   */
  async createTaskAssignment(taskId: string, contactId: string): Promise<void> {
    const row = createTaskAssignmentRow(taskId, contactId);
    await firstValueFrom(this.http.post(this.apiUrl, row));
  }

  /**
   * Assigns multiple contacts to a task.
   * @param taskId Identifier of the task.
   * @param contactIds Identifiers of the contacts to assign.
   * @returns A promise that resolves after assignment.
   */
  async createTaskAssignments(taskId: string, contactIds: string[]): Promise<void> {
    if (contactIds.length === 0) return;
    await Promise.all(contactIds.map(contactId => this.createTaskAssignment(taskId, contactId)));
  }

  /**
   * Removes a contact assignment from a task.
   * @param taskId Identifier of the task.
   * @param contactId Identifier of the contact to unassign.
   * @returns A promise that resolves after deletion.
   */
  async deleteTaskAssignment(taskId: string, contactId: string): Promise<void> {
    await firstValueFrom(this.http.delete(`${this.apiUrl}/${taskId}/${contactId}`));
  }

  /**
   * Removes multiple contact assignments from a task.
   * @param taskId Identifier of the task.
   * @param contactIds Identifiers of the contacts to unassign.
   * @returns A promise that resolves after deletion.
   */
  async deleteTaskAssignments(taskId: string, contactIds: string[]): Promise<void> {
    if (contactIds.length === 0) return;
    await Promise.all(contactIds.map(contactId => this.deleteTaskAssignment(taskId, contactId)));
  }
}