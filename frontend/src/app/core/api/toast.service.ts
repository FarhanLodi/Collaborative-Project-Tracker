import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info';

export interface Toast {
  id: number;
  type: ToastType;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  toasts = signal<Toast[]>([]);

  show(type: ToastType, message: string, timeoutMs = 3000) {
    const toast: Toast = { id: this.nextId++, type, message };
    this.toasts.update(list => [...list, toast]);
    setTimeout(() => this.dismiss(toast.id), timeoutMs);
  }

  success(message: string) { this.show('success', message); }
  error(message: string) { this.show('error', message, 5000); }
  info(message: string) { this.show('info', message); }

  dismiss(id: number) {
    this.toasts.update(list => list.filter(t => t.id !== id));
  }
}


