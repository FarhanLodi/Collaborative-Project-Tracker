import { Component, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ProjectsService } from '../core/api/projects.service';
import { TasksService, TaskItem, TaskStatus } from '../core/api/tasks.service';
import { CdkDropListGroup, CdkDropList, CdkDrag, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { FormsModule } from '@angular/forms';
import { RealtimeService } from '../core/realtime/realtime.service';
import { AuthService } from '../core/auth/auth.service';
import { AttachmentsService, Attachment } from '../core/api/attachments.service';
import { ToastService } from '../core/api/toast.service';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, CdkDropListGroup, CdkDropList, CdkDrag],
  templateUrl: './project-detail.component.html',
  styleUrls: ['./project-detail.component.scss']
})
export class ProjectDetailComponent {
  private route = inject(ActivatedRoute);
  private projects = inject(ProjectsService);
  private tasks = inject(TasksService);
  private realtime = inject(RealtimeService);
  private auth = inject(AuthService);
  private attachmentsApi = inject(AttachmentsService);
  private toasts = inject(ToastService);

  projectId = signal<string>('');
  project = signal<any | null>(null);
  allTasks = signal<TaskItem[]>([]);
  // New/edit modal state
  newTitle = '';
  assigneeForNew: string = '';
  newDescription = '';
  newStatus: TaskStatus = 'ToDo';
  newDueDate: string = '';
  newFiles: File[] = [];
  creatingTask = signal<boolean>(false);
  submittedCreate = signal<boolean>(false);
  uploadingAttachments = signal<boolean>(false);
  savingTask = signal<boolean>(false);
  submittedEdit = signal<boolean>(false);
  deletingTask = signal<boolean>(false);

  todo = signal<TaskItem[]>([]);
  inProgress = signal<TaskItem[]>([]);
  done = signal<TaskItem[]>([]);
  selected = signal<TaskItem | null>(null);
  // comments removed
  attachments = signal<Attachment[]>([]);
  newComment = '';

  // Filters
  filterAssigneeId = signal<string>('all');
  filterStatus = signal<'all' | TaskStatus>('all');

  // Modals
  showEditModal = signal<boolean>(false);
  showDeleteModal = signal<boolean>(false);
  showCreateModal = signal<boolean>(false);

  // Edit fields
  editTitle = '';
  editDescription = '';
  editAssigneeId: string | '' = '';
  editStatus: TaskStatus = 'ToDo';
  isOwner = signal<boolean>(false);

  constructor() {
    this.route.paramMap.subscribe(async p => {
      const id = p.get('id')!;
      this.projectId.set(id);
      this.load();
      const token = this.auth.token();
      if (token) {
        await this.realtime.connect(token);
        await this.realtime.joinProject(id);
      }
    });
    effect(() => {
      const evt = this.realtime.events();
      if (evt && evt.projectId === this.projectId()) {
        this.loadTasks();
      }
    });
  }

  private load() {
    this.projects.getById(this.projectId()).subscribe((p: any) => {
      this.project.set(p);
      const uid = this.auth.getUserId();
      this.isOwner.set(!!uid && p && p.ownerId === uid);
      this.loadTasks();
    });
  }

  private loadTasks() {
    this.tasks.list(this.projectId()).subscribe(list => {
      const uid = this.auth.getUserId();
      const owner = this.isOwner();
      const visible = owner ? list : list.filter(t => t.assigneeId === uid);
      this.allTasks.set(visible);
      const filtered = this.applyFilters(visible);
      this.todo.set(filtered.filter(t => t.status === 'ToDo'));
      this.inProgress.set(filtered.filter(t => t.status === 'InProgress'));
      this.done.set(filtered.filter(t => t.status === 'Done'));
    });
  }

  openCreate() {
    if (!this.isOwner()) return;
    this.newTitle = '';
    this.newDescription = '';
    this.assigneeForNew = '';
    this.newStatus = 'ToDo';
    this.newDueDate = '';
    this.newFiles = [];
    this.submittedCreate.set(false);
    this.showCreateModal.set(true);
  }

  createTask() {
    this.submittedCreate.set(true);
    const title = this.newTitle.trim();
    const hasAssignee = this.assigneeForNew !== undefined; // allow '' for Unassigned
    if (!title || !this.newStatus || !this.newDueDate || !hasAssignee) return;
    const payload: any = {
      title,
      description: this.newDescription || undefined,
      assigneeId: this.assigneeForNew || undefined,
      status: this.newStatus,
      dueDate: this.newDueDate || undefined
    };
    this.creatingTask.set(true);
    this.tasks.create(this.projectId(), payload).subscribe({
      next: (created) => {
        this.toasts.success('Task created');
        const files = this.newFiles;
        if (files && files.length > 0) {
          this.uploadingAttachments.set(true);
          let remaining = files.length;
          files.forEach(f => {
            this.attachmentsApi.upload(this.projectId(), created.id, f).subscribe({
              next: () => {},
              error: () => { remaining--; if (remaining <= 0) { this.afterCreateCleanup(); } },
              complete: () => { remaining--; if (remaining <= 0) { this.afterCreateCleanup(); } }
            });
          });
        } else {
          this.afterCreateCleanup();
        }
      },
      error: () => {},
      complete: () => this.creatingTask.set(false)
    });
  }

  private afterCreateCleanup() {
    this.uploadingAttachments.set(false);
    this.newFiles = [];
    this.showCreateModal.set(false);
    this.loadTasks();
  }

  onNewFiles(e: Event) {
    const input = e.target as HTMLInputElement;
    const files = input.files ? Array.from(input.files) : [];
    this.newFiles = files;
  }

  drop(event: CdkDragDrop<TaskItem[]>, newStatus: TaskStatus) {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      const item = event.previousContainer.data[event.previousIndex];
      transferArrayItem(event.previousContainer.data, event.container.data, event.previousIndex, event.currentIndex);
      // Optimistic update already applied in UI; persist status only
      this.tasks.updateStatus(this.projectId(), item.id, newStatus).subscribe({
        error: () => {
          // revert on error by reloading tasks
          this.loadTasks();
        }
      });
    }
  }

  select(t: TaskItem) {
    this.selected.set(t);
    this.editTitle = t.title;
    this.editDescription = t.description || '';
    this.editAssigneeId = t.assigneeId || '';
    this.editStatus = t.status;
    this.loadDetails();
  }

  openEdit(t: TaskItem) {
    this.select(t);
    this.submittedEdit.set(false);
    this.showEditModal.set(true);
  }

  openDelete(t: TaskItem) {
    this.selected.set(t);
    this.showDeleteModal.set(true);
  }

  closeModals() {
    this.showEditModal.set(false);
    this.showDeleteModal.set(false);
  }

  private loadDetails() {
    const s = this.selected();
    if (!s) return;
    // Details panel removed; keep API calls only if modals need them in the future.
    // comments removed
    this.attachments.set([]);
  }

  getAssigneeName(t: TaskItem): string {
    const p: any = this.project();
    const members = (p && p.members) ? p.members : [];
    const match = members.find((m: any) => m.userId === t.assigneeId);
    return (match && match.user && (match.user.fullName || match.user.email)) || 'Unassigned';
  }

  onFiltersChange() {
    const list = this.allTasks();
    const filtered = this.applyFilters(list);
    this.todo.set(filtered.filter(t => t.status === 'ToDo'));
    this.inProgress.set(filtered.filter(t => t.status === 'InProgress'));
    this.done.set(filtered.filter(t => t.status === 'Done'));
  }

  private applyFilters(list: TaskItem[]): TaskItem[] {
    const assigneeId = this.filterAssigneeId();
    const status = this.filterStatus();
    return list.filter(t => {
      const assigneeOk = assigneeId === 'all' ? true : assigneeId === 'unassigned' ? !t.assigneeId : t.assigneeId === assigneeId;
      const statusOk = status === 'all' ? true : t.status === status;
      return assigneeOk && statusOk;
    });
  }

  saveTask() {
    const s = this.selected();
    if (!s) return;
    if (!this.isOwner()) return;
    this.submittedEdit.set(true);
    if (!this.editTitle.trim() || !this.editStatus) return;
    this.savingTask.set(true);
    this.tasks.update(this.projectId(), s.id, {
      title: this.editTitle,
      description: this.editDescription,
      assigneeId: this.editAssigneeId || undefined,
      status: this.editStatus
    } as any).subscribe({
      next: (updated) => {
        this.selected.set(updated);
        this.loadTasks();
        this.loadDetails();
        this.toasts.success('Task updated');
      },
      error: () => {},
      complete: () => this.savingTask.set(false)
    });
  }

  saveFromModal() {
    this.saveTask();
    this.showEditModal.set(false);
  }

  deleteTask() {
    const s = this.selected();
    if (!s) return;
    if (!this.isOwner()) return;
    this.deletingTask.set(true);
    this.tasks.delete(this.projectId(), s.id).subscribe({
      next: () => {
        this.selected.set(null);
        this.loadTasks();
        this.toasts.success('Task deleted');
      },
      error: () => {},
      complete: () => this.deletingTask.set(false)
    });
  }

  confirmDelete() {
    this.deleteTask();
    this.showDeleteModal.set(false);
  }

  addComment() {
    // Details panel removed
  }

  onFile(e: Event) {
    // Details panel removed
  }

  download(a: Attachment) {
    // Details panel removed
  }
}


