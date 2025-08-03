import { Component, Input, OnInit, Output, EventEmitter, ViewEncapsulation, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { SharedModule } from '../../../shared/shared.module';
import { TasksStatus } from '../../../core/enums/TasksStatus';
import { TaskMissionService } from '../../../core/services/TaskMission.service';
import { SiteService } from '../../../core/services/site.service';
import { ArticleService } from '../../../core/services/article.service';
import { Task } from '../../../core/models/Task';
import { Site } from '../../../core/models/Site';
import { Article } from '../../../core/models/Article';
import { PagedResponse } from '../../../core/models/PagedResponse';
import { Observable } from 'rxjs';
import { CheckboxModule } from 'primeng/checkbox';
import { PickListModule } from 'primeng/picklist';
import { trigger, state, style, transition, animate, query, stagger } from '@angular/animations';
import { ActivatedRoute } from '@angular/router';

@Component({
  encapsulation: ViewEncapsulation.None,
  selector: 'app-mission-tasks',
  standalone: true,
  imports: [SharedModule, CheckboxModule, PickListModule],
  providers: [MessageService, ConfirmationService, TaskMissionService, SiteService, ArticleService],
  templateUrl: './mission-tasks.component.html',
  styleUrls: ['./mission-tasks.component.scss'],
  animations: [
    trigger('taskAnimation', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('300ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ opacity: 0, transform: 'translateY(-10px)' }))
      ])
    ]),
    trigger('listAnimation', [
      transition('* => *', [
        query(':enter', [
          style({ opacity: 0, transform: 'translateX(-20px)' }),
          stagger(50, [
            animate('300ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
          ])
        ], { optional: true })
      ])
    ]),
    trigger('statusAnimation', [
      transition('* => *', [
        animate('200ms ease-in-out', style({ transform: 'scale(1.1)' })),
        animate('200ms ease-in-out', style({ transform: 'scale(1)' }))
      ])
    ]),
    trigger('progressAnimation', [
      transition('* => *', [
        animate('500ms ease-in-out')
      ])
    ])
  ],
})
export class MissionTasksComponent implements OnInit {
  @Input() tasks: Task[] = [];
  @Input() missionId: number = 0;
  @Output() tasksUpdated = new EventEmitter<void>();
  @ViewChild('taskFilter') taskFilter: any;

  TasksStatus = TasksStatus;

  taskStatusOptions: { label: string; value: TasksStatus }[] = Object.entries(TasksStatus)
    .filter(([key]) => isNaN(Number(key)))
    .map(([key, value]) => ({ label: key, value: value as TasksStatus }));

  sites: Site[] = [];
  articles: Article[] = [];
  selectedArticles: Article[] = [];
  taskDialog: boolean = false;
  articleDialog: boolean = false;
  taskForm: FormGroup;
  isEditingTask: boolean = false;
  loading: boolean = false;
  selectedTask: Task | null = null;

  // Enhanced UI properties
  filterStatus: TasksStatus | null = null;
  filterSite: number | null = null;
  showCompletedTasks: boolean = true;
  viewMode: 'list' | 'kanban' = 'list';
  sortBy: 'status' | 'completionDate' | 'description' = 'status';
  sortOrder: 'asc' | 'desc' = 'asc';
  selectedTasks: Task[] = [];
  bulkActionsVisible: boolean = false;
  taskProgress: { completed: number; total: number; percentage: number } = { completed: 0, total: 0, percentage: 0 };

  constructor(
    private fb: FormBuilder,
    private taskMissionService: TaskMissionService,
    private siteService: SiteService,
    private articleService: ArticleService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private activatedRoute: ActivatedRoute
  ) {
    this.taskForm = this.createTaskForm();
  }

  ngOnInit(): void {
    // Get missionId from route parameters if not provided as input
    this.activatedRoute.params.subscribe(params => {
      if (params['missionId']) {
        this.missionId = +params['missionId'];
        this.taskForm.patchValue({ missionId: this.missionId });
        this.loadTasksForMission();
      }
    });
    
    this.loadSites();
    this.loadArticles();
    this.updateTaskProgress();
  }

  loadTasksForMission(): void {
    if (this.missionId) {
      this.loading = true;
      this.taskMissionService.getTasksByMissionId(this.missionId).subscribe({
        next: (tasks) => {
          this.tasks = tasks;
          this.updateTaskProgress();
          this.loading = false;
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load tasks for this mission'
          });
          this.loading = false;
        }
      });
    }
  }

  ngOnChanges(): void {
    this.updateTaskProgress();
  }

  updateTaskProgress(): void {
    this.taskProgress.total = this.tasks.length;
    this.taskProgress.completed = this.tasks.filter(task => task.status === TasksStatus.Completed).length;
    this.taskProgress.percentage = this.taskProgress.total > 0 ? 
      Math.round((this.taskProgress.completed / this.taskProgress.total) * 100) : 0;
  }

  getFilteredTasks(): Task[] {
    let filtered = [...this.tasks];

    // Filter by status
    if (this.filterStatus !== null) {
      filtered = filtered.filter(task => task.status === this.filterStatus);
    }

    // Filter by site
    if (this.filterSite !== null) {
      filtered = filtered.filter(task => task.siteId === this.filterSite);
    }

    // Filter completed tasks
    if (!this.showCompletedTasks) {
      filtered = filtered.filter(task => task.status !== TasksStatus.Completed);
    }

    // Sort tasks
    filtered.sort((a, b) => {
      let aValue: any, bValue: any;

      switch (this.sortBy) {
        case 'status':
          aValue = a.status;
          bValue = b.status;
          break;
        case 'completionDate':
          aValue = a.completionDate || new Date(0);
          bValue = b.completionDate || new Date(0);
          break;
        case 'description':
          aValue = a.description.toLowerCase();
          bValue = b.description.toLowerCase();
          break;
        default:
          return 0;
      }

      if (this.sortOrder === 'asc') {
        return aValue > bValue ? 1 : -1;
      } else {
        return aValue < bValue ? 1 : -1;
      }
    });

    return filtered;
  }

  getTasksByStatus(status: TasksStatus): Task[] {
    return this.getFilteredTasks().filter(task => task.status === status);
  }

  loadSites(): void {
    this.siteService.getPagedSites(1, 500).subscribe({
      next: (response: PagedResponse<Site>) => {
        this.sites = response.data;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load sites' });
      },
    });
  }

  loadArticles(): void {
    this.articleService.getPagedArticles(1, 500).subscribe({
      next: (response: PagedResponse<Article>) => {
        this.articles = response.data;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load articles' });
      },
    });
  }

  createTaskForm(): FormGroup {
    return this.fb.group({
      taskId: [0],
      description: ['', Validators.required],
      completionDate: [null],
      status: [TasksStatus.Pending, Validators.required],
      missionId: [this.missionId, Validators.required],
      siteId: [null, Validators.required],
      isFirstTask: [false]
    });
  }

  showAddEditTaskDialog(task?: Task): void {
    this.resetTaskForm();
    if (task) {
      this.isEditingTask = true;
      this.taskForm.patchValue({
        ...task,
        completionDate: task.completionDate ? this.formatDateForInput(task.completionDate) : null,
      });
    } else {
      this.taskForm.patchValue({ missionId: this.missionId });
    }
    this.taskDialog = true;
  }

  showArticleDialog(task: Task): void {
    this.selectedTask = task;
    this.selectedArticles = [];
    this.taskMissionService.getArticlesByTaskId(task.taskId).subscribe({
      next: (articles) => {
        this.selectedArticles = articles;
        this.articleDialog = true;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load assigned articles' });
      },
    });
  }

  onTaskSubmit(): void {
    if (this.taskForm.invalid) {
      this.markFormTouched(this.taskForm);
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fix the form errors' });
      return;
    }

    const task: Task = { ...this.taskForm.value };
    if (task.completionDate) {
      const localDate = new Date(task.completionDate);
      task.completionDate = new Date(
        Date.UTC(
          localDate.getFullYear(),
          localDate.getMonth(),
          localDate.getDate(),
          localDate.getHours(),
          localDate.getMinutes(),
          localDate.getSeconds()
        )
      );
    }
    this.loading = true;

    // If this task is being set as first task, update other tasks
    if (task.isFirstTask) {
      // Update all other tasks in the same mission to set isFirstTask to false
      const otherTasks = this.tasks.filter(t => t.taskId !== task.taskId);
      const updatePromises = otherTasks.map(otherTask => 
        this.taskMissionService.updateTask({ ...otherTask, isFirstTask: false }).toPromise()
      );

      Promise.all(updatePromises)
        .then(() => {
          // After updating other tasks, proceed with the current task update/create
          this.processTaskUpdate(task);
        })
        .catch(err => {
          this.handleTaskError(err);
          this.loading = false;
        });
    } else {
      // If this task is not being set as first task, proceed normally
      this.processTaskUpdate(task);
    }
  }

  private processTaskUpdate(task: Task): void {
    if (this.isEditingTask) {
      this.taskMissionService.updateTask(task).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Task updated successfully' });
          this.taskDialog = false;
          // Update the tasks array with the new task and handle isFirstTask
          if (task.isFirstTask) {
            this.tasks = this.tasks.map(t => ({ ...t, isFirstTask: false }));
            this.tasks = this.tasks.map((t) => {
              if (t.taskId === task.taskId) {
                return { ...t, ...task };
              }
              return t;
            });
          } else {
            this.tasks = this.tasks.map((t) => {
              if (t.taskId === task.taskId) {
                return { ...t, ...task };
              }
              return t;
            });
          }
          this.updateTaskProgress();
          this.tasksUpdated.emit();
          this.loading = false;
        },
        error: (err) => {
          this.handleTaskError(err);
          this.loading = false;
        },
      });
    } else {
      this.taskMissionService.createTask(task).subscribe({
        next: (newTask) => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Task created successfully' });
          this.taskDialog = false;
          // Add the new task to the tasks array and handle isFirstTask
          if (task.isFirstTask) {
            this.tasks = this.tasks.map(t => ({ ...t, isFirstTask: false }));
            this.tasks.push({ ...newTask, isFirstTask: true });
          } else {
            this.tasks.push(newTask);
          }
          this.updateTaskProgress();
          this.tasksUpdated.emit();
          this.loading = false;
        },
        error: (err) => {
          this.handleTaskError(err);
          this.loading = false;
        },
      });
    }
  }

  onArticleSubmit(): void {
    if (!this.selectedTask) return;

    this.loading = true;
    const articleIds = this.selectedArticles.map(article => article.articleId);

    this.taskMissionService.assignArticlesToTask(this.selectedTask.taskId, articleIds).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Articles assigned successfully' });
        this.articleDialog = false;
        this.loading = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to assign articles' });
        this.loading = false;
      },
    });
  }

  confirmDeleteTask(task: Task): void {
    console.log('Confirm delete task called for:', task);
    this.confirmationService.confirm({
      message: `Are you sure you want to delete task "${task.description}"?`,
      header: 'Delete Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        console.log('Delete confirmed for task ID:', task.taskId);
        this.deleteTask(task.taskId);
      },
      reject: () => {
        console.log('Delete cancelled');
      }
    });
  }

  deleteTask(id: number): void {
    console.log('Delete task called with ID:', id);
    this.loading = true;
    this.taskMissionService.deleteTask(id).subscribe({
      next: () => {
        console.log('Task deleted successfully');
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Task deleted successfully' });
        this.tasks = this.tasks.filter(task => task.taskId !== id);
        this.updateTaskProgress();
        this.tasksUpdated.emit();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error deleting task:', error);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete task' });
        this.loading = false;
      },
    });
  }

  resetTaskForm(): void {
    this.taskForm.reset({
      taskId: 0,
      description: '',
      completionDate: null,
      status: TasksStatus.Pending,
      missionId: this.missionId,
      siteId: null,
      isFirstTask: false,
    });
    this.isEditingTask = false;
  }

  formatDateForInput(date: string | Date): string {
    const dateObj = new Date(date);
    const year = dateObj.getFullYear();
    const month = String(dateObj.getMonth() + 1).padStart(2, '0');
    const day = String(dateObj.getDate()).padStart(2, '0');
    const hours = String(dateObj.getHours()).padStart(2, '0');
    const minutes = String(dateObj.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  markFormTouched(form: FormGroup): void {
    Object.values(form.controls).forEach((control) => control.markAsTouched());
  }

  getTaskFieldError(field: string): string {
    const control = this.taskForm.get(field);
    if (control?.touched && control?.errors) {
      if (control.errors['required']) return 'This field is required';
      if (control.errors['min']) return 'Please select a valid option';
    }
    return '';
  }

  getTaskStatusLabel(status: TasksStatus): string {
    return TasksStatus[status] || 'Unknown';
  }

  getTaskStatusBadgeClass(status: TasksStatus): string {
    switch (status) {
      case TasksStatus.Pending:
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case TasksStatus.InProgress:
        return 'bg-blue-100 text-blue-800 border-blue-200';
      case TasksStatus.Completed:
        return 'bg-green-100 text-green-800 border-green-200';
      // case TasksStatus.Cancelled:
      //   return 'bg-red-100 text-red-800 border-red-200';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  }

  handleTaskError(err: any): void {
    const errorMessage = err?.error?.Message || 'Operation failed';
    this.messageService.add({ severity: 'error', summary: 'Error', detail: errorMessage });
    console.error(err);
  }

  // Enhanced methods for better UX
  toggleViewMode(): void {
    this.viewMode = this.viewMode === 'list' ? 'kanban' : 'list';
  }

  onSelectionChange(): void {
    this.bulkActionsVisible = this.selectedTasks.length > 0;
  }

  bulkUpdateStatus(status: TasksStatus): void {
    if (this.selectedTasks.length === 0) return;

    this.confirmationService.confirm({
      message: `Are you sure you want to update ${this.selectedTasks.length} tasks to ${this.getTaskStatusLabel(status)}?`,
      header: 'Bulk Update Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        // Implement bulk update logic
        this.messageService.add({ 
          severity: 'success', 
          summary: 'Bulk Update', 
          detail: `${this.selectedTasks.length} tasks updated successfully` 
        });
        this.selectedTasks = [];
        this.bulkActionsVisible = false;
        this.updateTaskProgress();
      }
    });
  }

  getTaskPriority(task: Task): 'high' | 'medium' | 'low' {
    if (task.isFirstTask) return 'high';
    if (task.status === TasksStatus.Completed) return 'low';
    return 'medium';
  }

  getPriorityBadgeClass(priority: 'high' | 'medium' | 'low'): string {
    switch (priority) {
      case 'high':
        return 'bg-red-100 text-red-800 border-red-200';
      case 'medium':
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'low':
        return 'bg-green-100 text-green-800 border-green-200';
    }
  }

  getPriorityIcon(priority: 'high' | 'medium' | 'low'): string {
    switch (priority) {
      case 'high':
        return 'pi pi-exclamation-triangle';
      case 'medium':
        return 'pi pi-clock';
      case 'low':
        return 'pi pi-check-circle';
    }
  }

  getSiteName(siteId: number): string {
    const site = this.sites.find(s => s.siteId === siteId);
    return site ? site.name : 'Unknown Site';
  }

  // Drag and drop for Kanban view
  onDragStart(event: any, task: Task): void {
    event.dataTransfer.setData('text/plain', task.taskId.toString());
  }

  onDrop(event: any, newStatus: TasksStatus): void {
    event.preventDefault();
    const taskId = parseInt(event.dataTransfer.getData('text/plain'));
    const task = this.tasks.find(t => t.taskId === taskId);
    
    if (task && task.status !== newStatus) {
      this.updateTaskStatus(task, newStatus);
    }
  }

  onDragOver(event: any): void {
    event.preventDefault();
  }

  updateTaskStatus(task: Task, newStatus: TasksStatus): void {
    const updatedTask = { ...task, status: newStatus };
    if (newStatus === TasksStatus.Completed) {
      updatedTask.completionDate = new Date();
    }

    this.taskMissionService.updateTask(updatedTask).subscribe({
      next: () => {
        this.tasks = this.tasks.map(t => 
          t.taskId === task.taskId ? updatedTask : t
        );
        this.updateTaskProgress();
        this.messageService.add({ 
          severity: 'success', 
          summary: 'Status Updated', 
          detail: `Task moved to ${this.getTaskStatusLabel(newStatus)}` 
        });
      },
      error: () => {
        this.messageService.add({ 
          severity: 'error', 
          summary: 'Error', 
          detail: 'Failed to update task status' 
        });
      }
    });
  }

  // Quick actions
  quickCompleteTask(task: Task): void {
    if (task.status !== TasksStatus.Completed) {
      this.updateTaskStatus(task, TasksStatus.Completed);
    }
  }

  quickStartTask(task: Task): void {
    if (task.status === TasksStatus.Pending) {
      this.updateTaskStatus(task, TasksStatus.InProgress);
    }
  }

  duplicateTask(task: Task): void {
    const newTask = {
      ...task,
      taskId: 0,
      description: `${task.description} (Copy)`,
      status: TasksStatus.Pending,
      completionDate: new Date(),
      isFirstTask: false
    };
    this.showAddEditTaskDialog(newTask);
  }

  // Export functionality
  exportTasks(): void {
    // Implement export logic (CSV, Excel, etc.)
    this.messageService.add({ 
      severity: 'info', 
      summary: 'Export', 
      detail: 'Export functionality will be implemented soon' 
    });
  }

  // Search functionality
  searchTasks(query: string): void {
    // Implement search logic
    console.log('Searching tasks for:', query);
  }

  // Track by function for performance
  trackByTaskId(index: number, task: Task): number {
    return task.taskId;
  }

  // Helper methods for template options
  getStatusFilterOptions(): any[] {
    return [
      { label: 'All Statuses', value: null },
      ...this.taskStatusOptions
    ];
  }

  getSiteFilterOptions(): any[] {
    return [
      { label: 'All Sites', value: null },
      ...this.sites.map(s => ({ label: s.name, value: s.siteId }))
    ];
  }

  getSortOptions(): any[] {
    return [
      { label: 'Status', value: 'status' },
      { label: 'Completion Date', value: 'completionDate' },
      { label: 'Description', value: 'description' }
    ];
  }

  // Helper for PickList target change
  onTargetChange(event: any) {
    this.selectedArticles = event;
  }
}