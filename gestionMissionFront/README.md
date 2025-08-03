# Mission Management Frontend

A modern Angular application for managing missions, vehicles, articles, and related resources with advanced filtering, image management, and responsive design.

## 🚀 Features

### Enhanced Components
- **Vehicles Management**: Complete vehicle fleet management with photo support
- **Articles Management**: Inventory management with advanced filtering
- **Modern UI/UX**: Responsive design with grid and table views
- **Image Management**: Photo upload, gallery, and management
- **Advanced Filtering**: Multi-criteria filtering with real-time search
- **Responsive Design**: Mobile-first approach with adaptive layouts

## 📋 Table of Contents

1. [Getting Started](#getting-started)
2. [Components Documentation](#components-documentation)
3. [API Integration](#api-integration)
4. [Features Overview](#features-overview)
5. [Technical Details](#technical-details)
6. [Usage Examples](#usage-examples)

## 🛠 Getting Started

### Prerequisites
- Node.js (v16 or higher)
- Angular CLI (v15 or higher)
- PrimeNG components

### Installation
```bash
npm install
```

### Development Server
```bash
ng serve
```

### Build for Production
```bash
ng build
```

## 📚 Components Documentation

### 🚗 Vehicles Component

#### Overview
The Vehicles component provides comprehensive vehicle fleet management with advanced filtering, image handling, and modern UI/UX.

#### Features
- **Grid & Table Views**: Toggle between card-based grid and traditional table views
- **Advanced Filtering**: Filter by type, availability, license plate, and capacity
- **Image Management**: Upload, preview, and manage vehicle photos
- **Responsive Design**: Mobile-first approach with adaptive layouts
- **Real-time Search**: Instant filtering with pagination support

#### Key Methods

##### Filtering
```typescript
applyFilters(): void
clearFilters(): void
```

##### Image Management
```typescript
onFileSelect(event: any): void
showImageGallery(vehicle: Vehicle, index: number): void
getImageUrl(photoUrl: string): string
```

##### CRUD Operations
```typescript
showAddEditDialog(vehicle?: Vehicle): void
onSubmit(): void
confirmDelete(vehicle: Vehicle): void
```

#### Usage Example
```typescript
// Load vehicles with filters
this.vehicleService.getPagedVehicles(1, 10, {
  type: VehicleType.Truck,
  availability: true,
  minCapacity: 1000
}).subscribe(response => {
  this.vehicles = response.data;
});
```

### 📦 Articles Component

#### Overview
The Articles component provides inventory management with photo support, advanced filtering, and stock status indicators.

#### Features
- **Inventory Management**: Complete article lifecycle management
- **Stock Status**: Visual indicators for stock levels (In Stock, Low Stock, Out of Stock)
- **Advanced Filtering**: Filter by name, description, quantity, weight, and volume
- **Image Gallery**: Full-screen photo viewer with navigation
- **Responsive Grid**: Adaptive layout for different screen sizes

#### Key Methods

##### Stock Management
```typescript
getQuantityStatus(quantity: number): string
getQuantityBadgeClass(quantity: number): string
```

##### Filtering
```typescript
applyFilters(): void
clearFilters(): void
```

##### Image Handling
```typescript
onFileSelect(event: any): void
showImageGallery(article: Article, index: number): void
```

#### Usage Example
```typescript
// Load articles with filters
this.articleService.getPagedArticles(1, 10, {
  name: 'Tool',
  minQuantity: 5,
  maxWeight: 20.0
}).subscribe(response => {
  this.articles = response.data;
});
```

## 🔌 API Integration

### Base Configuration
```typescript
// environment.ts
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5000/api'
};
```

### Service Structure
Both components follow a consistent service pattern:

```typescript
@Injectable({
  providedIn: 'root'
})
export class BaseService {
  private apiUrl = `${environment.apiBaseUrl}/[Resource]`;

  // CRUD Operations
  getPaged(page: number, size: number, filter?: Filter): Observable<PagedResponse<T>>
  create(item: T, photos?: File[]): Observable<T>
  update(id: number, item: T, keepPhotos?: string[], newPhotos?: File[]): Observable<void>
  delete(id: number): Observable<void>
  
  // Image Handling
  getImageUrl(photoUrl: string): string
}
```

### API Endpoints

#### Vehicles API
- `GET /api/vehicles/paged` - Get paginated vehicles with filtering
- `POST /api/vehicles` - Create vehicle with photos
- `PUT /api/vehicles/{id}` - Update vehicle with photo management
- `DELETE /api/vehicles/{id}` - Delete vehicle

#### Articles API
- `GET /api/articles/paged` - Get paginated articles with filtering
- `POST /api/articles` - Create article with photos
- `PUT /api/articles/{id}` - Update article with photo management
- `DELETE /api/articles/{id}` - Delete article

## 🎨 Features Overview

### 🖼 Image Management
- **Upload Support**: Drag-and-drop file upload with validation
- **Multiple Formats**: JPG, PNG, GIF, WebP support
- **Size Limits**: 5MB per file, maximum 10 photos per item
- **Gallery View**: Full-screen image viewer with navigation
- **Thumbnail Navigation**: Easy photo browsing with thumbnails

### 🔍 Advanced Filtering
- **Multi-criteria**: Filter by multiple properties simultaneously
- **Real-time**: Instant filtering with pagination
- **Range Filters**: Min/max values for numeric fields
- **Text Search**: Partial matching for text fields
- **Filter Count**: Visual indicator of active filters

### 📱 Responsive Design
- **Mobile First**: Optimized for mobile devices
- **Adaptive Grid**: Responsive grid layouts
- **Touch Friendly**: Touch-optimized interactions
- **Breakpoint Support**: Multiple responsive breakpoints

### 🎯 UI/UX Features
- **Loading States**: Skeleton loading and progress indicators
- **Error Handling**: Comprehensive error messages
- **Form Validation**: Real-time validation with error display
- **Confirmation Dialogs**: Safe delete operations
- **Toast Notifications**: User feedback for actions

## 🔧 Technical Details

### Architecture
- **Component-based**: Modular, reusable components
- **Service Layer**: Centralized business logic
- **Type Safety**: Full TypeScript support
- **Reactive Forms**: Form validation and handling

### Dependencies
```json
{
  "primeng": "^15.0.0",
  "tailwindcss": "^3.0.0",
  "@angular/forms": "^15.0.0",
  "@angular/common": "^15.0.0"
}
```

### Styling
- **Tailwind CSS**: Utility-first CSS framework
- **PrimeNG**: Component library for Angular
- **Custom SCSS**: Component-specific styles
- **Responsive Design**: Mobile-first approach

### State Management
- **Component State**: Local component state management
- **Service State**: Centralized data management
- **Form State**: Reactive forms with validation
- **UI State**: Loading, error, and success states

## 📖 Usage Examples

### Basic Vehicle Management
```typescript
// Create a new vehicle
const vehicle: Vehicle = {
  type: VehicleType.Truck,
  licensePlate: 'ABC123',
  availability: true,
  maxCapacity: 5000
};

this.vehicleService.createVehicle(vehicle, photos).subscribe(
  response => console.log('Vehicle created:', response)
);
```

### Advanced Filtering
```typescript
// Apply complex filters
const filters: VehicleFilter = {
  type: VehicleType.Truck,
  availability: true,
  minCapacity: 1000,
  maxCapacity: 10000,
  licensePlate: 'ABC'
};

this.vehicleService.getPagedVehicles(1, 10, filters).subscribe(
  response => this.vehicles = response.data
);
```

### Image Management
```typescript
// Handle file upload
onFileSelect(event: any): void {
  const files = event.files;
  const validFiles = Array.from(files).filter(file => 
    ['image/jpeg', 'image/png'].includes(file.type) && 
    file.size <= 5 * 1024 * 1024
  );
  this.selectedPhotos = [...this.selectedPhotos, ...validFiles];
}
```

## 🚀 Performance Optimizations

### Lazy Loading
- Component-based lazy loading
- Image lazy loading for galleries
- On-demand data loading

### Caching
- Service-level caching
- Image URL caching
- Filter state persistence

### Optimization
- Virtual scrolling for large datasets
- Image compression and optimization
- Debounced search inputs

## 🔒 Security Considerations

### File Upload Security
- File type validation
- File size limits
- Malware scanning (backend)
- Secure file storage

### Data Validation
- Client-side validation
- Server-side validation
- Input sanitization
- XSS prevention

## 📝 Contributing

### Development Guidelines
1. Follow Angular style guide
2. Use TypeScript strict mode
3. Implement proper error handling
4. Write unit tests for components
5. Maintain responsive design

### Code Quality
- ESLint configuration
- Prettier formatting
- Husky pre-commit hooks
- Automated testing

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation

---

**Built with ❤️ using Angular, PrimeNG, and Tailwind CSS**
