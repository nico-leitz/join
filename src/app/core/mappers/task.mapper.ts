import { Contact } from '../models/contact.model';
import { Subtask } from '../models/subtask.model';
import { Task, TaskPriority, TaskCategory, TaskStatus } from '../models/task.model';

export interface CsTaskDto {
  id: string;
  title: string;
  description: string;
  dueDate: string | null;
  priority: TaskPriority; 
  category: TaskCategory;
  status: TaskStatus;
  sortOrder: number;
  authUserId: string;
  createdAt: string;
  updatedAt: string;
}

export interface CsSubtaskDto {
  id: string;
  taskId: string;
  title: string;
  isCompleted: boolean;
  sortOrder: number;
  createdAt: string;
  updatedAt: string;
}

export interface CsContactDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  badgeColor: string;
  authUserId: string;
  createdAt: string;
  updatedAt: string;
}

export interface TaskContactRelationDto {
  contacts?: CsContactDto | null;
  contact?: CsContactDto | null;
}

export function mapTaskRows(taskRows: CsTaskDto[]): Task[] {
  if (!taskRows) return [];
  return taskRows.map(mapTaskRow);
}

export function mapTaskRow(taskRow: CsTaskDto): Task {
  return {
    id: taskRow.id,
    title: taskRow.title,
    description: taskRow.description,
    dueDate: taskRow.dueDate ? taskRow.dueDate.split('T')[0] : '',
    priority: taskRow.priority,
    category: taskRow.category,
    status: taskRow.status,
    sortOrder: taskRow.sortOrder ?? 1,
    createdAt: taskRow.createdAt,
    updatedAt: taskRow.updatedAt,
  };
}

export function mapSubtaskRows(subtaskRows: CsSubtaskDto[]): Subtask[] {
  if (!subtaskRows) return [];
  return subtaskRows.map(mapSubtaskRow);
}

export function mapSubtaskRow(subtaskRow: CsSubtaskDto): Subtask {
  return {
    id: subtaskRow.id,
    taskId: subtaskRow.taskId,
    title: subtaskRow.title,
    isCompleted: subtaskRow.isCompleted ?? false,
    sortOrder: subtaskRow.sortOrder ?? 1,
    createdAt: subtaskRow.createdAt,
    updatedAt: subtaskRow.updatedAt,
  };
}

export function mapContactRelations(relations: TaskContactRelationDto[]): Contact[] {
  if (!relations) return [];
  return relations
    .map((relation) => relation.contacts ?? relation.contact)
    .filter((contact): contact is CsContactDto => contact !== null && contact !== undefined)
    .map(mapContactRow);
}

export function mapContactRow(contactRow: CsContactDto): Contact {
  return {
    id: contactRow.id,
    firstName: contactRow.firstName,
    lastName: contactRow.lastName,
    email: contactRow.email,
    phone: contactRow.phone,
    badgeColor: contactRow.badgeColor,
    authUserId: contactRow.authUserId,
    createdAt: contactRow.createdAt,
    updatedAt: contactRow.updatedAt,
  };
}