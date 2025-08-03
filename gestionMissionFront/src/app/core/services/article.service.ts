import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Article, ArticleFilter } from '../models/Article';
import { PagedResponse } from '../models/PagedResponse';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ArticleService {
  private apiUrl = `${environment.apiBaseUrl}/Articles`;

  constructor(private http: HttpClient) {}

  getPagedArticles(pageNumber: number, pageSize: number, filter?: ArticleFilter): Observable<PagedResponse<Article>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (filter) {
      if (filter.name) params = params.set('filter.Name', filter.name);
      if (filter.description) params = params.set('filter.Description', filter.description);
      if (filter.minQuantity !== undefined) params = params.set('filter.MinQuantity', filter.minQuantity.toString());
      if (filter.maxQuantity !== undefined) params = params.set('filter.MaxQuantity', filter.maxQuantity.toString());
      if (filter.minWeight !== undefined) params = params.set('filter.MinWeight', filter.minWeight.toString());
      if (filter.maxWeight !== undefined) params = params.set('filter.MaxWeight', filter.maxWeight.toString());
      if (filter.minVolume !== undefined) params = params.set('filter.MinVolume', filter.minVolume.toString());
      if (filter.maxVolume !== undefined) params = params.set('filter.MaxVolume', filter.maxVolume.toString());
    }

    return this.http.get<PagedResponse<Article>>(`${this.apiUrl}/paged`, { params });
  }

  getAllArticles(): Observable<Article[]> {
    return this.http.get<Article[]>(this.apiUrl);
  }

  getArticle(id: number): Observable<Article> {
    return this.http.get<Article>(`${this.apiUrl}/${id}`);
  }

  createArticle(article: Article, photos?: File[]): Observable<Article> {
    const formData = new FormData();
    
    // Add article data
    formData.append('Name', article.name);
    formData.append('Description', article.description || '');
    formData.append('Quantity', article.quantity.toString());
    formData.append('Weight', article.weight.toString());
    formData.append('Volume', article.volume.toString());

    // Add photos if provided
    if (photos && photos.length > 0) {
      photos.forEach(photo => {
        formData.append('Photos', photo);
      });
    }

    return this.http.post<Article>(this.apiUrl, formData);
  }

  updateArticle(id: number, article: Article, keepPhotosUrls?: string[], newPhotos?: File[]): Observable<void> {
    const formData = new FormData();
    
    // Add article data
    formData.append('ArticleId', article.articleId.toString());
    formData.append('Name', article.name);
    formData.append('Description', article.description || '');
    formData.append('Quantity', article.quantity.toString());
    formData.append('Weight', article.weight.toString());
    formData.append('Volume', article.volume.toString());

    // Add existing photos to keep
    if (keepPhotosUrls && keepPhotosUrls.length > 0) {
      keepPhotosUrls.forEach(url => {
        formData.append('KeepPhotosUrls', url);
      });
    }

    // Add new photos
    if (newPhotos && newPhotos.length > 0) {
      newPhotos.forEach(photo => {
        formData.append('NewPhotos', photo);
      });
    }

    return this.http.put<void>(`${this.apiUrl}/${id}`, formData);
  }

  deleteArticle(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getImageUrl(photoUrl: string): string {
    if (!photoUrl) return '';
    if (photoUrl.startsWith('http')) return photoUrl;
    // Remove 'api' from the base URL for image paths
    const baseUrl = environment.apiBaseUrl.replace('/api', '');
    return `${baseUrl}/${photoUrl}`;
  }
}