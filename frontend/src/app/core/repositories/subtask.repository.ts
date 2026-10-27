import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  createSubtaskInsertPayload,
  createSubtaskUpdatePayload,
} from '../mappers/task-payload.mapper';
import { CreateSubtask, SubtaskRow, UpdateSubtask } from '../models/subtask.model';

/**
 * Provides persistence operations for task subtasks.
 */
@Injectable({
  providedIn: 'root',
})
export class SubtaskRepository {
  /** Angular HTTP Client used for API requests. */
  private readonly http = inject(HttpClient);

  /** Base URL for the subtasks API endpoint. */
  private readonly apiUrl = `${environment.apiUrl}/subtasks`;

  /**
   * Retrieves all subtasks belonging to a task.
   * @param taskId - Identifier of the parent task.
   * @returns Subtask rows ordered by position and creation time.
   */
  async getSubtaskRows(taskId: string): Promise<SubtaskRow[]> {
    const subtasks = await firstValueFrom(
      this.http.get<SubtaskRow[]>(`${this.apiUrl}?taskId=${taskId}`)
    );

    return subtasks.sort((a, b) => {
      if (a.sortOrder !== b.sortOrder) {
        return a.sortOrder - b.sortOrder;
      }
      return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
    });
  }

  /**
   * Retrieves all subtask rows required by the board.
   * @returns Subtask rows ordered by task, position and creation time.
   */
  async getAllSubtaskRows(): Promise<SubtaskRow[]> {
    const subtasks = await firstValueFrom(this.http.get<SubtaskRow[]>(this.apiUrl));

    return subtasks.sort((a, b) => {
      if (a.taskId !== b.taskId) {
        return a.taskId.localeCompare(b.taskId);
      }
      if (a.sortOrder !== b.sortOrder) {
        return a.sortOrder - b.sortOrder;
      }
      return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
    });
  }

  /**
   * Creates a subtask in the database.
   * @param subtask - Subtask data to persist.
   * @returns Created subtask row.
   */
  async createSubtask(subtask: CreateSubtask): Promise<SubtaskRow> {
    const payload = createSubtaskInsertPayload(subtask);
    return firstValueFrom(this.http.post<SubtaskRow>(this.apiUrl, payload));
  }

  /**
   * Updates a subtask in the database.
   * @param id - Identifier of the subtask to update.
   * @param subtask - Subtask fields to persist.
   * @returns Updated subtask row.
   */
  async updateSubtask(id: string, subtask: UpdateSubtask): Promise<SubtaskRow> {
    const payload = createSubtaskUpdatePayload(subtask);
    return firstValueFrom(this.http.put<SubtaskRow>(`${this.apiUrl}/${id}`, payload));
  }

  /**
   * Updates a subtask while restricting the request to its parent task.
   * @param taskId - Identifier of the parent task.
   * @param id - Identifier of the subtask to update.
   * @param subtask - Subtask fields to persist.
   * @returns A promise that resolves after the update.
   */
  async updateTaskSubtask(taskId: string, id: string, subtask: UpdateSubtask): Promise<void> {
    await this.updateSubtask(id, subtask);
  }

  /**
   * Deletes a subtask from the database.
   * @param id - Identifier of the subtask to delete.
   * @returns A promise that resolves after deletion.
   */
  async deleteSubtask(id: string): Promise<void> {
    await firstValueFrom(this.http.delete(`${this.apiUrl}/${id}`));
  }

  /**
   * Deletes selected subtasks belonging to a task.
   * @param taskId - Identifier of the parent task.
   * @param subtaskIds - Identifiers of the subtasks to delete.
   * @returns A promise that resolves after deletion.
   */
  async deleteTaskSubtasks(taskId: string, subtaskIds: string[]): Promise<void> {
    if (subtaskIds.length === 0) {
      return;
    }
    await Promise.all(subtaskIds.map((id) => this.deleteSubtask(id)));
  }
}