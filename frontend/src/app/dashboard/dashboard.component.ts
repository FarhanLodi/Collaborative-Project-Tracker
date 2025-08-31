import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToastService } from '../core/api/toast.service';
import { environment } from '../../environments/environment';
import { AuthService } from '../core/auth/auth.service';

interface Project {
  id: string;
  name: string;
  description?: string;
  inviteCode: string;
  ownerId?: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  private http = inject(HttpClient);
  private toast = inject(ToastService);
  auth = inject(AuthService);
  private loadingCreate = signal<boolean>(false);
  private loadingJoin = signal<boolean>(false);
  submittedProjectCreate = signal<boolean>(false);
  // Expose as readonly for template via functions

  projects = signal<Project[]>([]);
  newProjectName = '';
  inviteCode = '';
  newProjectDescription = '';
  newProjectDeadline = '';
  showCreateModal = signal<boolean>(false);

  constructor() {
    this.load();
  }

  load() {
    this.http.get<Project[]>(`${environment.apiBaseUrl}/api/projects`).subscribe(res => this.projects.set(res));
  }

  createProject() {
    this.submittedProjectCreate.set(true);
    const name = this.newProjectName?.trim();
    if (!name || !this.newProjectDeadline) return;
    const payload: any = { name };
    if (this.newProjectDescription.trim()) payload.description = this.newProjectDescription.trim();
    if (this.newProjectDeadline) payload.deadline = this.newProjectDeadline;
    this.loadingCreate.set(true);
    this.http.post<Project>(`${environment.apiBaseUrl}/api/projects`, payload).subscribe({
      next: () => {
        this.toast.success('Project created');
        this.newProjectName = '';
        this.newProjectDescription = '';
        this.newProjectDeadline = '';
        this.showCreateModal.set(false);
        this.submittedProjectCreate.set(false);
        this.load();
      },
      error: () => {},
      complete: () => this.loadingCreate.set(false)
    });
  }

  joinProject() {
    const code = this.inviteCode?.trim();
    if (!code) return;
    this.loadingJoin.set(true);
    this.http.post<Project>(`${environment.apiBaseUrl}/api/projects/join`, { inviteCode: code }).subscribe({
      next: () => {
        this.toast.success('Joined project');
        this.inviteCode = '';
        this.load();
      },
      error: () => {},
      complete: () => this.loadingJoin.set(false)
    });
  }

  deleteProject(projectId: string) {
    // Only owners can delete; backend enforces too
    this.http.delete(`${environment.apiBaseUrl}/api/projects/${projectId}`).subscribe({
      next: () => {
        this.toast.success('Project deleted');
        this.load();
      },
      error: () => {}
    });
  }

  isOwnerProject(p: Project): boolean {
    const uid = this.auth.getUserId();
    const pid = p.ownerId;
    return !!uid && !!pid && pid.toLowerCase() === uid.toLowerCase();
  }

  copyInvite(inviteCode: string) {
    navigator.clipboard.writeText(inviteCode).then(() => this.toast.success('Invite code copied'));
  }

  isCreating() { return this.loadingCreate(); }
  isJoining() { return this.loadingJoin(); }
}


