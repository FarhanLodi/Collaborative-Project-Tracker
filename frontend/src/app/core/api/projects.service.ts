import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface Project {
  id: string;
  name: string;
  description?: string;
  inviteCode: string;
  ownerId?: string;
}

@Injectable({ providedIn: 'root' })
export class ProjectsService {
  private http = inject(HttpClient);

  getMyProjects() {
    return this.http.get<Project[]>(`${environment.apiBaseUrl}/api/projects`);
  }

  create(name: string, description?: string, deadline?: string) {
    return this.http.post<Project>(`${environment.apiBaseUrl}/api/projects`, { name, description, deadline });
  }

  getById(id: string) {
    return this.http.get<Project>(`${environment.apiBaseUrl}/api/projects/${id}`);
  }

  join(inviteCode: string) {
    return this.http.post<Project>(`${environment.apiBaseUrl}/api/projects/join`, { inviteCode });
  }
}


