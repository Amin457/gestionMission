import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Task } from '../models/Task';
import { TasksStatus } from '../enums/TasksStatus';
import { Article } from '../models/Article';


@Injectable({
  providedIn: 'root'
})
export class TaskMissionService {
  private apiUrl = `${environment.apiBaseUrl}/TaskMissions`;

  constructor(private http: HttpClient) {}

  getAllTasks(): Observable<Task[]> {
    return this.http.get<Task[]>(this.apiUrl)
  }

  getTaskById(id: number): Observable<Task> {
    return this.http.get<Task>(`${this.apiUrl}/${id}`)
  }

  getTasksByMissionId(missionId: number): Observable<Task[]> {
    return this.http.get<Task[]>(`${this.apiUrl}/mission/${missionId}`)
  }

  getTasksByStatus(status: TasksStatus): Observable<Task[]> {
    return this.http.get<Task[]>(`${this.apiUrl}/status/${TasksStatus[status].toLowerCase()}`)
  }

  getTaskCountByMissionId(missionId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/count/mission/${missionId}`)
  }

  createTask(task: Task): Observable<Task> {
    return this.http.post<Task>(this.apiUrl, task)
  }

  updateTask(task: Task): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${task.taskId}`, task)
  }

  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`)
  }

  assignArticlesToTask(taskId: number, articleIds: number[]): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${taskId}/articles`, articleIds)
  }

  removeArticlesFromTask(taskId: number, articleIds: number[]): Observable<void> {
    return this.http.request<void>('delete', `${this.apiUrl}/${taskId}/articles`, { body: articleIds })
  }

  getArticlesByTaskId(taskId: number): Observable<Article[]> {
    return this.http.get<Article[]>(`${this.apiUrl}/${taskId}/articles`)
  }
}