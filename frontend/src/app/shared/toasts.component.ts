import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../core/api/toast.service';

@Component({
  selector: 'app-toasts',
  standalone: true,
  imports: [CommonModule],
  template: `
  <div class="toast-container position-fixed bottom-0 end-0 p-3" style="z-index:1080;">
    <div *ngFor="let t of toastSvc.toasts()" class="toast show mb-2 border-0" [ngClass]="{
      'text-bg-success': t.type === 'success',
      'text-bg-danger': t.type === 'error',
      'text-bg-info': t.type === 'info'
    }">
      <div class="d-flex align-items-center px-3 py-2">
        <div class="me-3">{{ t.message }}</div>
        <button type="button" class="btn-close btn-close-white ms-auto" (click)="toastSvc.dismiss(t.id)"></button>
      </div>
    </div>
  </div>
  `
})
export class ToastsComponent {
  toastSvc = inject(ToastService);
}


