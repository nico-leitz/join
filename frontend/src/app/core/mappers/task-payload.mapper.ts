import { CreateSubtask, SubtaskRow, UpdateSubtask } from '../models/subtask.model';
import { TaskAssignmentRow } from '../models/task-assignment.model';
import { CreateTask, TaskRow, UpdateTask } from '../models/task.model';

/**
 * Creates a database payload for inserting a task.
 * @param task Task data to transform.
 * @returns Normalized task insert payload.
 */
export function createTaskInsertPayload(task: CreateTask): any {
  return {
    title: task.title.trim(),
    description: task.description?.trim() ?? '',
    dueDate: task.dueDate,
    category: task.category,
    ...(task.priority !== undefined && { priority: task.priority }),
    ...(task.status !== undefined && { status: task.status }),
    ...(task.sortOrder !== undefined && { sortOrder: task.sortOrder }),
  };
}

/**
 * Creates a database payload containing the provided task updates.
 * @param task Task fields to update.
 * @returns Normalized task update payload with a new update timestamp.
 */
export function createTaskUpdatePayload(task: UpdateTask): any {
  return {
    ...(task.title !== undefined && { title: task.title.trim() }),
    ...(task.description !== undefined && { description: task.description.trim() }),
    ...(task.dueDate !== undefined && { dueDate: task.dueDate }),
    ...(task.priority !== undefined && { priority: task.priority }),
    ...(task.category !== undefined && { category: task.category }),
    ...(task.status !== undefined && { status: task.status }),
    ...(task.sortOrder !== undefined && { sortOrder: task.sortOrder }),
    updatedAt: new Date().toISOString(),
  };
}

/**
 * Creates a database payload for inserting a subtask.
 * @param subtask Subtask data to transform.
 * @returns Normalized subtask insert payload.
 */
export function createSubtaskInsertPayload(
  subtask: CreateSubtask,
): any {
  return {
    taskId: subtask.taskId,
    title: subtask.title.trim(),
    ...(subtask.sortOrder !== undefined && {
      sortOrder: subtask.sortOrder,
    }),
  };
}

/**
 * Creates a database payload containing the provided subtask updates.
 * @param subtask Subtask fields to update.
 * @returns Normalized subtask update payload with a new update timestamp.
 */
export function createSubtaskUpdatePayload(
  subtask: UpdateSubtask,
): any {
  return {
    ...(subtask.title !== undefined && {
      title: subtask.title.trim(),
    }),
    ...(subtask.isCompleted !== undefined && {
      isCompleted: subtask.isCompleted,
    }),
    ...(subtask.sortOrder !== undefined && {
      sortOrder: subtask.sortOrder,
    }),
    updatedAt: new Date().toISOString(),
  };
}

/**
 * Creates a database row linking a task with a contact.
 * @param taskId Identifier of the task.
 * @param contactId Identifier of the assigned contact.
 * @returns Task assignment insert payload.
 */
export function createTaskAssignmentRow(
  taskId: string,
  contactId: string,
): Partial<TaskAssignmentRow> {
  return {
    taskId: taskId,
    contactId: contactId,
  };
}

/**
 * Creates task assignment rows for multiple contacts.
 * @param taskId Identifier of the task.
 * @param contactIds Identifiers of the contacts to assign.
 * @returns Task assignment insert payloads.
 */
export function createTaskAssignmentRows(
  taskId: string,
  contactIds: string[],
): Partial<TaskAssignmentRow>[] {
  return contactIds.map((contactId) => {
    return createTaskAssignmentRow(taskId, contactId);
  });
}