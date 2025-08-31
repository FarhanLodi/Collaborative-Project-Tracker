import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export type TaskStatus = 'ToDo' | 'InProgress' | 'Done';

export interface TaskItem {
  id: string;
  projectId: string;
  title: string;
  description?: string;
  status: TaskStatus;
  assigneeId?: string;
  dueDate?: string;
}

@Injectable({ providedIn: 'root' })
export class TasksService {
  private http = inject(HttpClient);

  list(projectId: string) {
    return this.http.get<TaskItem[]>(`${environment.apiBaseUrl}/api/projects/${projectId}/tasks`);
  }

  create(projectId: string, payload: Partial<TaskItem>) {
    return this.http.post<TaskItem>(`${environment.apiBaseUrl}/api/projects/${projectId}/tasks`, payload);
  }

  update(projectId: string, taskId: string, payload: Partial<TaskItem>) {
    return this.http.put<TaskItem>(`${environment.apiBaseUrl}/api/projects/${projectId}/tasks/${taskId}`, payload);
  }

  updateStatus(projectId: string, taskId: string, status: TaskStatus) {
    return this.http.patch<TaskItem>(`${environment.apiBaseUrl}/api/projects/${projectId}/tasks/${taskId}/status`, { status });
  }

  delete(projectId: string, taskId: string) {
    return this.http.delete<void>(`${environment.apiBaseUrl}/api/projects/${projectId}/tasks/${taskId}`);
  }
}


