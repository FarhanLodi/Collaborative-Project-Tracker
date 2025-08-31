import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface Attachment {
  id: string;
  taskItemId: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  storagePath: string;
}

@Injectable({ providedIn: 'root' })
export class AttachmentsService {
  private http = inject(HttpClient);

  list(projectId: string, taskId: string) {
    return this.http.get<Attachment[]>(`${environment.apiBaseUrl}/api/projects/${projectId}/tasks/${taskId}/attachments`);
  }

  upload(projectId: string, taskId: string, file: File) {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<Attachment>(`${environment.apiBaseUrl}/api/projects/${projectId}/tasks/${taskId}/attachments`, form);
  }

  download(projectId: string, taskId: string, attachmentId: string) {
    return this.http.get(`${environment.apiBaseUrl}/api/projects/${projectId}/tasks/${taskId}/attachments/${attachmentId}`, { responseType: 'blob' });
  }
}


