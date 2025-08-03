import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MissionService } from '../../../core/services/mission.service';
import { TaskMissionService } from '../../../core/services/TaskMission.service';
import { MissionCostService } from '../../../core/services/mission-cost.service';
import { UserService } from '../../../core/services/User.service';
import { SiteService } from '../../../core/services/site.service';
import { CityService } from '../../../core/services/city.service';
import { ArticleService } from '../../../core/services/article.service';
import { Mission, MissionGet } from '../../../core/models/Mission';
import { TaskGet } from '../../../core/models/Task';
import { MissionCost, MissionCostType } from '../../../core/models/MissionCost';
import { User } from '../../../core/models/User';
import { Site } from '../../../core/models/Site';
import { City } from '../../../core/models/City';
import { Article } from '../../../core/models/Article';
import { MissionStatus, MissionType } from '../../../core/enums/mission.enums';
import { TasksStatus } from '../../../core/enums/TasksStatus';
import { ChartModule } from 'primeng/chart';
import { TagModule } from 'primeng/tag';
import { ProgressBarModule } from 'primeng/progressbar';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-mission-sheet',
  standalone: true,
  templateUrl: './mission-sheet.component.html',
  styleUrls: ['./mission-sheet.component.scss'],
  imports: [
    CommonModule,
    ChartModule,
    TagModule,
    ProgressBarModule,
    DialogModule,
    ButtonModule,
    TableModule,
    CardModule,
    TooltipModule,
    ToastModule
  ],
  providers: [MessageService]
})
export class MissionSheetComponent implements OnInit, OnDestroy {
  @Input() missionId!: number;
  mission: MissionGet | null = null;
  requester: User | null = null;
  driver: User | null = null;
  tasks: TaskGet[] = [];
  costs: MissionCost[] = [];
  sites: { [id: number]: Site } = {};
  cities: { [id: number]: City } = {};
  taskArticles: { [taskId: number]: Article[] } = {};
  loading = true;
  error: string | null = null;
  showCostDialog = false;
  selectedCost: MissionCost | null = null;
  
  private destroy$ = new Subject<void>();

  MissionStatus = MissionStatus;
  MissionType = MissionType;
  TasksStatus = TasksStatus;
  MissionCostType = MissionCostType;

  // Enhanced visualizations
  costChartData: any;
  costChartOptions: any;
  taskStatusChartData: any;
  taskStatusChartOptions: any;
  timelineChartData: any;
  timelineChartOptions: any;

  // Statistics
  missionStats = {
    totalTasks: 0,
    completedTasks: 0,
    pendingTasks: 0,
    inProgressTasks: 0,
    totalCosts: 0,
    totalArticles: 0,
    totalWeight: 0,
    totalVolume: 0,
    averageCostPerTask: 0,
    estimatedDuration: 0
  };

  // Make missionCostService public for template access
  public missionCostService: MissionCostService;

  ngOnInit() {
    // Get mission ID from route if not provided as input
    if (!this.missionId) {
      this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
        if (params['missionId']) {
          this.missionId = +params['missionId'];
          this.loadMissionSheet(this.missionId);
        }
      });
    } else {
      this.loadMissionSheet(this.missionId);
    }
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  async loadMissionSheet(missionId: number) {
    this.loading = true;
    this.error = null;
    
    try {
      // Fetch mission
      const mission = await this.missionService.getMissionById(missionId).toPromise();
      
      if (!mission) {
        throw new Error('Mission not found');
      }
      
      this.mission = mission as MissionGet;

      // Fetch requester and driver
      const [requester, driver] = await Promise.all([
        this.userService.getUserById(this.mission.requesterId).toPromise().catch(() => null),
        this.userService.getUserById(this.mission.driverId).toPromise().catch(() => null)
      ]);
      
      this.requester = requester || null;
      this.driver = driver || null;

      // Fetch tasks
      const tasks = await this.taskMissionService.getTasksByMissionId(missionId).toPromise();
      this.tasks = (tasks || []).map(task => ({
        ...task,
        siteName: this.getSiteName(task.siteId)
      })) as TaskGet[];

      // Fetch articles for each task
      if (this.tasks.length > 0) {
        const articlePromises = this.tasks.map(async (task) => {
          try {
            const articles = await this.taskMissionService.getArticlesByTaskId(task.taskId).toPromise();
            this.taskArticles[task.taskId] = articles || [];
            return articles;
          } catch (error) {
            this.taskArticles[task.taskId] = [];
            return [];
          }
        });
        await Promise.all(articlePromises);
      }

      // Fetch sites for tasks
      if (this.tasks.length > 0) {
        const siteIds = Array.from(new Set(this.tasks.map(t => t.siteId)));
        const sitePromises = siteIds.map(id => this.siteService.getSite(id).toPromise().catch(() => null));
        const sites = await Promise.all(sitePromises);
        sites.forEach(site => {
          if (site) this.sites[site.siteId] = site;
        });

        // Fetch cities for sites
        const cityIds = Array.from(new Set(sites.filter(s => s).map(s => s!.cityId)));
        const cityPromises = cityIds.map(id => this.cityService.getById(id).toPromise().catch(() => null));
        const cities = await Promise.all(cityPromises);
        cities.forEach(city => {
          if (city) this.cities[city.cityId] = city;
        });
      }

      // Fetch costs
      this.costs = await this.missionCostService.getMissionCostsByMissionId(missionId).toPromise() || [];

      // Calculate statistics
      this.calculateStatistics();

      // Prepare charts
      this.prepareCostChart();
      this.prepareTaskStatusChart();
      this.prepareTimelineChart();

      this.loading = false;
      
      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Mission sheet loaded successfully'
      });

    } catch (err: any) {
      this.error = err?.message || 'Failed to load mission sheet.';
      this.loading = false;
      
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: this.error || 'Unknown error occurred'
      });
    }
  }

  calculateStatistics() {
    this.missionStats.totalTasks = this.tasks.length;
    this.missionStats.completedTasks = this.tasks.filter(t => t.status === TasksStatus.Completed).length;
    this.missionStats.pendingTasks = this.tasks.filter(t => t.status === TasksStatus.Pending).length;
    this.missionStats.inProgressTasks = this.tasks.filter(t => t.status === TasksStatus.InProgress).length;
    this.missionStats.totalCosts = this.getTotalCost();
    
    // Calculate article statistics
    const allArticles = Object.values(this.taskArticles).flat();
    this.missionStats.totalArticles = allArticles.length;
    this.missionStats.totalWeight = allArticles.reduce((sum, article) => sum + (article.weight * article.quantity), 0);
    this.missionStats.totalVolume = allArticles.reduce((sum, article) => sum + (article.volume * article.quantity), 0);
    
    this.missionStats.averageCostPerTask = this.missionStats.totalTasks > 0 ? 
      this.missionStats.totalCosts / this.missionStats.totalTasks : 0;
  }

  prepareCostChart() {
    const typeLabels = Object.keys(MissionCostType).filter(k => isNaN(Number(k)));
    const typeValues = Object.values(MissionCostType).filter(v => typeof v === 'number') as number[];
    const data = typeValues.map(type => this.costs.filter(c => c.type === type).reduce((sum, c) => sum + c.amount, 0));
    
    this.costChartData = {
      labels: typeLabels,
      datasets: [{
        data,
        backgroundColor: ['#6366f1', '#f59e42', '#10b981', '#ef4444', '#8b5cf6'],
        borderWidth: 2,
        borderColor: '#ffffff'
      }]
    };
    
    this.costChartOptions = {
      plugins: {
        legend: { 
          position: 'bottom',
          labels: {
            usePointStyle: true,
            padding: 20
          }
        },
        tooltip: {
          callbacks: {
            label: (context: any) => {
              const total = context.dataset.data.reduce((a: number, b: number) => a + b, 0);
              const percentage = ((context.parsed * 100) / total).toFixed(1);
              return `${context.label}: ${context.parsed.toLocaleString()} (${percentage}%)`;
            }
          }
        }
      },
      responsive: true,
      maintainAspectRatio: false
    };
  }

  prepareTaskStatusChart() {
    const statusLabels = Object.keys(TasksStatus).filter(k => isNaN(Number(k)));
    const statusValues = Object.values(TasksStatus).filter(v => typeof v === 'number') as number[];
    const data = statusValues.map(status => this.tasks.filter(t => t.status === status).length);
    
    this.taskStatusChartData = {
      labels: statusLabels,
      datasets: [{
        data,
        backgroundColor: ['#fbbf24', '#3b82f6', '#10b981'],
        borderWidth: 2,
        borderColor: '#ffffff'
      }]
    };
    
    this.taskStatusChartOptions = {
      plugins: {
        legend: { 
          position: 'bottom',
          labels: {
            usePointStyle: true,
            padding: 20
          }
        }
      },
      responsive: true,
      maintainAspectRatio: false
    };
  }

  prepareTimelineChart() {
    if (this.tasks.length === 0) return;
    
    const sortedTasks = [...this.tasks].sort((a, b) => 
      new Date(a.assignmentDate).getTime() - new Date(b.assignmentDate).getTime()
    );
    
    this.timelineChartData = {
      labels: sortedTasks.map(task => this.getSiteName(task.siteId)),
      datasets: [{
        label: 'Task Timeline',
        data: sortedTasks.map((task, index) => ({
          x: index,
          y: task.status,
          task: task
        })),
        backgroundColor: '#6366f1',
        borderColor: '#4f46e5',
        borderWidth: 2,
        pointRadius: 6,
        pointHoverRadius: 8
      }]
    };
    
    this.timelineChartOptions = {
      plugins: {
        legend: { display: false },
        tooltip: {
          callbacks: {
            title: (context: any) => context[0].label,
            label: (context: any) => {
              const task = context.raw.task;
              return [
                `Status: ${this.getTaskStatusLabel(task.status)}`,
                `Assignment: ${new Date(task.assignmentDate).toLocaleDateString()}`,
                `Completion: ${new Date(task.completionDate).toLocaleDateString()}`
              ];
            }
          }
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            callback: (value: any) => this.getTaskStatusLabel(value)
          }
        }
      },
      responsive: true,
      maintainAspectRatio: false
    };
  }

  // Enhanced helper methods
  getStatusLabel(status: MissionStatus): string {
    return MissionStatus[status];
  }
  
  getTypeLabel(type: MissionType): string {
    return MissionType[type];
  }
  
  getTaskStatusLabel(status: TasksStatus): string {
    return TasksStatus[status];
  }
  
  getCostTypeLabel(type: MissionCostType): string {
    return MissionCostType[type];
  }
  
  getSiteName(siteId: number): string {
    return this.sites[siteId]?.name || 'Unknown Site';
  }
  
  getSiteAddress(siteId: number): string {
    const site = this.sites[siteId];
    if (!site) return 'Unknown Address';
    const city = this.cities[site.cityId]?.name || 'Unknown City';
    return `${site.address}, ${city}`;
  }
  
  getTaskProgress(): number {
    if (!this.tasks.length) return 0;
    const completed = this.tasks.filter(t => t.status === TasksStatus.Completed).length;
    return Math.round((completed / this.tasks.length) * 100);
  }
  
  getTotalCost(): number {
    return this.costs.reduce((sum, c) => sum + c.amount, 0);
  }

  getTaskArticles(taskId: number): Article[] {
    return this.taskArticles[taskId] || [];
  }

  getTaskArticleCount(taskId: number): number {
    return this.getTaskArticles(taskId).length;
  }

  getTaskTotalWeight(taskId: number): number {
    return this.getTaskArticles(taskId).reduce((sum, article) => sum + (article.weight * article.quantity), 0);
  }

  getTaskTotalVolume(taskId: number): number {
    return this.getTaskArticles(taskId).reduce((sum, article) => sum + (article.volume * article.quantity), 0);
  }

  // Track by functions for performance
  trackByTaskId(index: number, task: TaskGet): number {
    return task.taskId;
  }

  trackByCostId(index: number, cost: MissionCost): number {
    return cost.costId;
  }

  trackByArticleId(index: number, article: Article): number {
    return article.articleId;
  }

  // Export functions
  printSheet() {
    window.print();
  }

  exportToPDF() {
    // Implementation for PDF export
    this.messageService.add({
      severity: 'info',
      summary: 'Export',
      detail: 'PDF export functionality will be implemented'
    });
  }

  exportToCSV() {
    // Implementation for CSV export
    const csvData = this.generateCSVData();
    this.downloadCSV(csvData, `mission-${this.missionId}-sheet.csv`);
  }

  private generateCSVData(): string {
    const headers = ['Section', 'Field', 'Value'];
    const rows = [
      headers.join(','),
      `Mission,ID,${this.mission?.missionId}`,
      `Mission,Service,${this.mission?.service}`,
      `Mission,Type,${this.getTypeLabel(this.mission?.type!)}`,
      `Mission,Status,${this.getStatusLabel(this.mission?.status!)}`,
      `Mission,Requester,${this.requester?.firstName} ${this.requester?.lastName}`,
      `Mission,Driver,${this.driver?.firstName} ${this.driver?.lastName}`,
      `Statistics,Total Tasks,${this.missionStats.totalTasks}`,
      `Statistics,Completed Tasks,${this.missionStats.completedTasks}`,
      `Statistics,Total Cost,${this.missionStats.totalCosts}`,
      `Statistics,Total Articles,${this.missionStats.totalArticles}`,
      `Statistics,Total Weight,${this.missionStats.totalWeight}`,
      `Statistics,Total Volume,${this.missionStats.totalVolume}`
    ];
    
    return rows.join('\n');
  }

  private downloadCSV(csvData: string, filename: string) {
    const blob = new Blob([csvData], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    if (link.download !== undefined) {
      const url = URL.createObjectURL(blob);
      link.setAttribute('href', url);
      link.setAttribute('download', filename);
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  }

  // Dialog methods
  openCostDialog(cost: MissionCost) {
    this.selectedCost = cost;
    this.showCostDialog = true;
  }

  closeCostDialog() {
    this.showCostDialog = false;
    this.selectedCost = null;
  }

  // Navigation
  goBack() {
    this.router.navigate(['/missions/list']);
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private missionService: MissionService,
    private taskMissionService: TaskMissionService,
    missionCostService: MissionCostService,
    private userService: UserService,
    private siteService: SiteService,
    private cityService: CityService,
    private articleService: ArticleService,
    private messageService: MessageService
  ) {
    this.missionCostService = missionCostService;
  }
}