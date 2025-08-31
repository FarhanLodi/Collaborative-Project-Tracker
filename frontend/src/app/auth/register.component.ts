import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { CommonModule } from '@angular/common';
import { ToastService } from '../core/api/toast.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private toasts = inject(ToastService);
  loading = false;

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    fullName: ['', []],
    designation: ['', []]
  });

  submit() {
    if (this.form.invalid) return;
    const { email, password, fullName, designation } = this.form.value as { email: string; password: string; fullName?: string; designation?: string };
    this.loading = true;
    this.auth.register(email, password, fullName ?? '', designation ?? '').subscribe({
      next: () => {
        this.toasts.success('Registration successful');
        this.auth.login(email, password).subscribe({
          next: (res) => {
            this.auth.setToken(res.token);
            location.href = '/';
          },
          error: (err) => {
            const msg = err?.error?.message || 'Login failed';
            this.toasts.error(msg);
          }
        });
      },
      error: (err) => {
        const msg = err?.error?.message || 'Registration failed';
        this.toasts.error(msg);
      },
      complete: () => this.loading = false
    });
  }
}


