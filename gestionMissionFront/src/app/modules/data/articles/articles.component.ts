import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { Article, ArticleFilter } from '../../../core/models/Article';
import { PagedResponse } from '../../../core/models/PagedResponse';
import { ArticleService } from '../../../core/services/article.service';
import { SharedModule } from '../../../shared/shared.module';
import { FileUpload } from 'primeng/fileupload';

@Component({
  selector: 'app-articles',
  standalone: true,
  templateUrl: './articles.component.html',
  styleUrls: ['./articles.component.scss'],
  imports: [SharedModule],
  providers: [MessageService, ConfirmationService, ArticleService]
})
export class ArticlesComponent implements OnInit {
  @ViewChild('fileUpload') fileUpload!: FileUpload;

  // Data properties
  articles: Article[] = [];
  totalRecords = 0;
  loading = false;
  displayDialog = false;
  displayImageDialog = false;
  selectedArticle: Article | null = null;
  selectedImageIndex = 0;

  // Form properties
  articleForm: FormGroup;
  filterForm: FormGroup;
  isEditing = false;
  currentPage: number = 1;
  pageSize: number = 10;

  // Filter properties
  showFilters = false;
  appliedFilters: ArticleFilter = {};
  filterCount = 0;

  // Image properties
  selectedPhotos: File[] = [];
  existingPhotos: string[] = [];
  photosToKeep: string[] = [];

  // UI properties
  viewMode: 'grid' | 'table' = 'grid';
  sortField = '';
  sortOrder = 1;

  constructor(
    private fb: FormBuilder,
    private articleService: ArticleService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {
    this.articleForm = this.createArticleForm();
    this.filterForm = this.createFilterForm();
  }

  ngOnInit(): void {
    this.loadArticles();
  }

  createArticleForm(): FormGroup {
    return this.fb.group({
      articleId: [0],
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(255)]],
      description: ['', [Validators.maxLength(500)]],
      quantity: [0, [Validators.required, Validators.min(0)]],
      weight: [0, [Validators.required, Validators.min(0)]],
      volume: [0, [Validators.required, Validators.min(0)]]
    });
  }

  createFilterForm(): FormGroup {
    return this.fb.group({
      name: [''],
      description: [''],
      minQuantity: [null],
      maxQuantity: [null],
      minWeight: [null],
      maxWeight: [null],
      minVolume: [null],
      maxVolume: [null]
    });
  }

  loadArticles(): void {
    this.loading = true;
    this.articleService.getPagedArticles(this.currentPage, this.pageSize, this.appliedFilters).subscribe({
      next: (response: PagedResponse<Article>) => {
        this.articles = response.data;
        this.totalRecords = response.totalRecords;
        this.loading = false;
      },
      error: err => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load articles' });
        this.loading = false;
        console.error(err);
      }
    });
  }

  onPageChange(event: any): void {
    this.currentPage = event.page + 1;
    this.pageSize = event.rows;
    this.loadArticles();
  }

  onSort(event: any): void {
    this.sortField = event.field;
    this.sortOrder = event.order;
    this.loadArticles();
  }

  applyFilters(): void {
    const filterValues = this.filterForm.value;
    this.appliedFilters = {};
    this.filterCount = 0;

    Object.keys(filterValues).forEach(key => {
      if (filterValues[key] !== null && filterValues[key] !== '' && filterValues[key] !== undefined) {
        this.appliedFilters[key as keyof ArticleFilter] = filterValues[key];
        this.filterCount++;
      }
    });

    this.currentPage = 1;
    this.loadArticles();
    this.showFilters = false;
  }

  clearFilters(): void {
    this.filterForm.reset();
    this.appliedFilters = {};
    this.filterCount = 0;
    this.currentPage = 1;
    this.loadArticles();
  }

  showAddEditDialog(article?: Article): void {
    this.resetForm();
    this.selectedPhotos = [];
    this.existingPhotos = [];
    this.photosToKeep = [];

    if (article) {
      this.isEditing = true;
      this.articleForm.patchValue(article);
      this.existingPhotos = article.photoUrls || [];
      this.photosToKeep = [...this.existingPhotos];
    }
    this.displayDialog = true;
  }

  onFileSelect(event: any): void {
    const files = event.files;
    if (files && files.length > 0) {
      // Validate file types and sizes
      const validFiles = Array.from(files).filter((file: any) => {
        const isValidType = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'].includes(file.type);
        const isValidSize = file.size <= 5 * 1024 * 1024; // 5MB limit
        return isValidType && isValidSize;
      }) as File[];

      if (validFiles.length !== files.length) {
        this.messageService.add({ 
          severity: 'warn', 
          summary: 'Warning', 
          detail: 'Some files were skipped due to invalid type or size (max 5MB)' 
        });
      }

      this.selectedPhotos = [...this.selectedPhotos, ...validFiles];
    }
  }

  removeSelectedPhoto(index: number): void {
    this.selectedPhotos.splice(index, 1);
  }

  removeExistingPhoto(url: string): void {
    this.photosToKeep = this.photosToKeep.filter(photo => photo !== url);
  }

  addExistingPhoto(url: string): void {
    if (!this.photosToKeep.includes(url)) {
      this.photosToKeep.push(url);
    }
  }

  onSubmit(): void {
    if (this.articleForm.invalid) {
      this.markFormTouched(this.articleForm);
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fix the errors' });
      return;
    }

    const article: Article = this.articleForm.value;
    this.loading = true;

    if (this.isEditing) {
      this.articleService.updateArticle(article.articleId, article, this.photosToKeep, this.selectedPhotos).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Article updated successfully' });
          this.displayDialog = false;
          this.loadArticles();
          this.loading = false;
        },
        error: err => {
          const errorMessage = err?.error?.Message || 'Operation failed';
          this.messageService.add({ severity: 'error', summary: 'Error', detail: errorMessage });
          this.loading = false;
          console.error(err);
        }
      });
    } else {
      this.articleService.createArticle(article, this.selectedPhotos).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Article created successfully' });
          this.displayDialog = false;
          this.loadArticles();
          this.loading = false;
        },
        error: err => {
          const errorMessage = err?.error?.Message || 'Operation failed';
          this.messageService.add({ severity: 'error', summary: 'Error', detail: errorMessage });
          this.loading = false;
          console.error(err);
        }
      });
    }
  }

  showImageGallery(article: Article, index: number = 0): void {
    this.selectedArticle = article;
    this.selectedImageIndex = index;
    this.displayImageDialog = true;
  }

  nextImage(): void {
    if (this.selectedArticle && this.selectedArticle.photoUrls) {
      this.selectedImageIndex = (this.selectedImageIndex + 1) % this.selectedArticle.photoUrls.length;
    }
  }

  previousImage(): void {
    if (this.selectedArticle && this.selectedArticle.photoUrls) {
      this.selectedImageIndex = this.selectedImageIndex === 0 
        ? this.selectedArticle.photoUrls.length - 1 
        : this.selectedImageIndex - 1;
    }
  }

  confirmDelete(article: Article): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete article "${article.name}"?`,
      header: 'Delete Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.deleteArticle(article.articleId)
    });
  }

  deleteArticle(id: number): void {
    this.loading = true;
    this.articleService.deleteArticle(id).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Article deleted successfully' });
        this.loadArticles();
      },
      error: err => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete article' });
        this.loading = false;
        console.error(err);
      }
    });
  }

  resetForm(): void {
    this.articleForm.reset({ articleId: 0, quantity: 0, weight: 0, volume: 0 });
    this.isEditing = false;
  }

  markFormTouched(form: FormGroup): void {
    Object.values(form.controls).forEach(control => control.markAsTouched());
  }

  getFieldError(field: string): string {
    const control = this.articleForm.get(field);
    if (control?.touched && control?.errors) {
      if (control.errors['required']) return 'This field is required';
      if (control.errors['minlength']) return `Minimum length is ${control.errors['minlength'].requiredLength}`;
      if (control.errors['maxlength']) return `Maximum length is ${control.errors['maxlength'].requiredLength}`;
      if (control.errors['min']) return `${field} must be non-negative`;
    }
    return '';
  }

  getImageUrl(photoUrl: string): string {
    return this.articleService.getImageUrl(photoUrl);
  }

  getQuantityBadgeClass(quantity: number): string {
    if (quantity === 0) return 'bg-red-100 text-red-800 border-red-200';
    if (quantity < 5) return 'bg-yellow-100 text-yellow-800 border-yellow-200';
    return 'bg-green-100 text-green-800 border-green-200';
  }

  getQuantityStatus(quantity: number): string {
    if (quantity === 0) return 'Out of Stock';
    if (quantity < 5) return 'Low Stock';
    return 'In Stock';
  }

  toggleViewMode(): void {
    this.viewMode = this.viewMode === 'grid' ? 'table' : 'grid';
  }

  get totalPhotos(): number {
    return this.selectedPhotos.length + this.photosToKeep.length;
  }

  getPhotoPreviewUrl(file: File): string {
    return URL.createObjectURL(file);
  }
}