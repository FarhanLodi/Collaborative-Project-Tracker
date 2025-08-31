import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private connection?: signalR.HubConnection;
  events = signal<{ projectId: string; type: string; taskId: string } | null>(null);

  async connect(token: string) {
    if (this.connection) return;
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiBaseUrl}/hubs/tasks`, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();
    this.connection.on('tasksChanged', (evt) => this.events.set(evt));
    await this.connection.start();
  }

  async joinProject(projectId: string) {
    await this.connection?.invoke('JoinProject', projectId);
  }
}


