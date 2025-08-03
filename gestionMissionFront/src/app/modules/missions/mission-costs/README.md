# MissionCosts API Implementation

## Overview
This document describes the complete implementation of the MissionCosts API in the Angular frontend application, matching the backend API specification.

## API Endpoints Implementation

### 1. Get All Mission Costs
**Service Method:** `getAllMissionCosts()`
**HTTP:** `GET /api/missioncosts`
**Usage:**
```typescript
this.missionCostService.getAllMissionCosts().subscribe({
  next: (costs) => {
    this.costs = costs;
    this.calculateTotalCost();
  },
  error: (error) => {
    // Handle error
  }
});
```

### 2. Get Mission Cost by ID
**Service Method:** `getMissionCostById(id: number)`
**HTTP:** `GET /api/missioncosts/{id}`
**Usage:**
```typescript
this.missionCostService.getMissionCostById(costId).subscribe({
  next: (cost) => {
    console.log('Cost details:', cost);
  },
  error: (error) => {
    // Handle error
  }
});
```

### 3. Get Mission Costs by Mission ID
**Service Method:** `getMissionCostsByMissionId(missionId: number)`
**HTTP:** `GET /api/missioncosts/mission/{missionId}`
**Usage:**
```typescript
this.missionCostService.getMissionCostsByMissionId(missionId).subscribe({
  next: (costs) => {
    console.log(`Found ${costs.length} costs for mission ${missionId}`);
  },
  error: (error) => {
    // Handle error
  }
});
```

### 4. Get Total Cost by Mission ID
**Service Method:** `getTotalCostByMissionId(missionId: number)`
**HTTP:** `GET /api/missioncosts/total/mission/{missionId}`
**Usage:**
```typescript
this.missionCostService.getTotalCostByMissionId(missionId).subscribe({
  next: (total) => {
    console.log(`Total cost: $${total.toFixed(2)}`);
  },
  error: (error) => {
    // Handle error
  }
});
```

### 5. Create Mission Cost (with photos)
**Service Method:** `createMissionCostWithPhotos(missionId, type, amount, date, receiptPhotos)`
**HTTP:** `POST /api/missioncosts`
**Content-Type:** `multipart/form-data`
**Usage:**
```typescript
const formData = new FormData();
formData.append('missionId', missionId.toString());
formData.append('type', type.toString());
formData.append('amount', amount.toString());
formData.append('date', date);

receiptPhotos.forEach(photo => {
  formData.append('receiptPhotos', photo);
});

this.missionCostService.createMissionCostWithPhotos(
  missionId,
  type,
  amount,
  date,
  receiptPhotos
).subscribe({
  next: (cost) => {
    console.log('Cost created:', cost);
  },
  error: (error) => {
    // Handle error
  }
});
```

### 6. Create Mission Cost (JSON only)
**Service Method:** `createMissionCost(request: CreateMissionCostRequest)`
**HTTP:** `POST /api/missioncosts/json`
**Content-Type:** `application/json`
**Usage:**
```typescript
const request: CreateMissionCostRequest = {
  missionId: 5,
  type: MissionCostType.Fuel,
  amount: 45.50,
  date: '2024-01-15T10:30:00'
};

this.missionCostService.createMissionCost(request).subscribe({
  next: (cost) => {
    console.log('Cost created:', cost);
  },
  error: (error) => {
    // Handle error
  }
});
```

### 7. Update Mission Cost (with photos)
**Service Method:** `updateMissionCostWithPhotos(id, missionId, type, amount, date, receiptPhotos)`
**HTTP:** `PUT /api/missioncosts/{id}`
**Content-Type:** `multipart/form-data`
**Usage:**
```typescript
this.missionCostService.updateMissionCostWithPhotos(
  costId,
  missionId,
  type,
  amount,
  date,
  receiptPhotos
).subscribe({
  next: () => {
    console.log('Cost updated successfully');
  },
  error: (error) => {
    // Handle error
  }
});
```

### 8. Update Mission Cost (JSON only)
**Service Method:** `updateMissionCost(id: number, request: UpdateMissionCostRequest)`
**HTTP:** `PUT /api/missioncosts/{id}/json`
**Content-Type:** `application/json`
**Usage:**
```typescript
const request: UpdateMissionCostRequest = {
  missionId: 5,
  type: MissionCostType.Fuel,
  amount: 50.00,
  date: '2024-01-15T10:30:00'
};

this.missionCostService.updateMissionCost(costId, request).subscribe({
  next: () => {
    console.log('Cost updated successfully');
  },
  error: (error) => {
    // Handle error
  }
});
```

### 9. Delete Mission Cost
**Service Method:** `deleteMissionCost(id: number)`
**HTTP:** `DELETE /api/missioncosts/{id}`
**Usage:**
```typescript
this.missionCostService.deleteMissionCost(costId).subscribe({
  next: () => {
    console.log('Cost deleted successfully');
  },
  error: (error) => {
    // Handle error
  }
});
```

## Data Models

### MissionCost Interface
```typescript
export interface MissionCost {
  costId: number;
  missionId: number;
  type: MissionCostType;
  amount: number;
  date: string;
  receiptPhotoUrls?: string[] | null;
}
```

### MissionCostType Enum
```typescript
export enum MissionCostType {
  Fuel = 0,
  Toll = 1,
  Maintenance = 2
}
```

### CreateMissionCostRequest Interface
```typescript
export interface CreateMissionCostRequest {
  missionId: number;
  type: MissionCostType;
  amount: number;
  date: string;
}
```

### UpdateMissionCostRequest Interface
```typescript
export interface UpdateMissionCostRequest {
  missionId: number;
  type: MissionCostType;
  amount: number;
  date: string;
}
```

## Validation Rules

### Form Validation
- **missionId**: Required, must be greater than 0
- **type**: Required, must be valid MissionCostType enum value
- **amount**: Required, must be greater than 0.01
- **date**: Required, cannot be in the future

### File Validation
- **Maximum files**: 10 photos per mission cost
- **File size**: Maximum 10MB per file
- **File types**: JPEG, PNG, GIF only
- **Content-Type**: `multipart/form-data` for photo uploads

## Image Handling

### Image URL Processing
The service includes a `getImageUrl()` method that properly handles both relative and absolute URLs:

```typescript
getImageUrl(photoUrl: string): string {
  if (!photoUrl) return '';
  if (photoUrl.startsWith('http')) return photoUrl;
  const baseUrl = environment.apiBaseUrl.replace('/api', '');
  return `${baseUrl}/${photoUrl}`;
}
```

### Photo Gallery Features
- **Full-screen modal** with dark theme
- **Navigation controls** (previous/next buttons)
- **Thumbnail navigation** for multiple photos
- **Photo counter** showing current position
- **Responsive design** for all screen sizes

## Error Handling

### Common Error Scenarios
1. **400 Bad Request**: Validation errors (amount ≤ 0, future date, etc.)
2. **404 Not Found**: Cost with specified ID not found
3. **415 Unsupported Media Type**: Wrong content type for photo uploads
4. **File validation errors**: Invalid file type, size, or count

### Error Response Format
```json
{
  "errors": [
    "Amount must be greater than 0",
    "Date cannot be in the future",
    "Maximum 10 receipt photos allowed"
  ]
}
```

## Component Features

### Main Features
- **CRUD operations** for mission costs
- **Photo upload** with drag-and-drop support
- **Photo gallery** with navigation
- **Real-time validation** with user feedback
- **Responsive design** for mobile and desktop
- **Statistics dashboard** showing totals and averages

### UI Components
- **Modern card-based layout** with gradient backgrounds
- **Interactive data table** with sorting and filtering
- **Professional dialogs** with form validation
- **Toast notifications** for user feedback
- **Confirmation dialogs** for destructive actions

## Usage Examples

### Creating a New Mission Cost
```typescript
// 1. Show the add dialog
this.showAddDialog();

// 2. Fill the form
this.costForm.patchValue({
  missionId: 5,
  type: MissionCostType.Fuel,
  amount: 45.50,
  date: new Date()
});

// 3. Add photos (optional)
this.uploadedFiles = [file1, file2];

// 4. Save
this.saveCost();
```

### Viewing Receipt Photos
```typescript
// View photos for a specific cost
this.viewPhotos(cost);

// Navigate through photos
this.nextPhoto();
this.previousPhoto();

// Close gallery
this.closePhotoGallery();
```

### Getting Mission Statistics
```typescript
// Get total cost for a mission
this.getTotalCostForMission(missionId);

// Get all costs for a mission
this.getCostsByMission(missionId);

// Get specific cost details
this.getCostById(costId);
```

## Environment Configuration

### API Base URL
The service uses the environment configuration for the API base URL:

```typescript
// environment.ts
export const environment = {
  production: false,
  apiBaseUrl: 'https://your-api-domain.com/api'
};
```

### Image URL Processing
Images are served from the same domain but without the `/api` prefix:

```typescript
// API URL: https://your-api-domain.com/api/missioncosts
// Image URL: https://your-api-domain.com/uploads/photo.jpg
```

## Testing

### Manual Testing Checklist
- [ ] Create mission cost without photos
- [ ] Create mission cost with photos
- [ ] Update mission cost (JSON only)
- [ ] Update mission cost with new photos
- [ ] Delete mission cost
- [ ] View receipt photos in gallery
- [ ] Navigate through multiple photos
- [ ] Validate form inputs
- [ ] Test file upload restrictions
- [ ] Test responsive design

### API Testing
All endpoints can be tested using tools like Postman or curl:

```bash
# Get all costs
curl -X GET "https://your-api-domain.com/api/missioncosts"

# Create cost with photos
curl -X POST "https://your-api-domain.com/api/missioncosts" \
  -F "missionId=5" \
  -F "type=0" \
  -F "amount=45.50" \
  -F "date=2024-01-15T10:30:00" \
  -F "receiptPhotos=@photo1.jpg" \
  -F "receiptPhotos=@photo2.jpg"
```

## Conclusion

This implementation provides a complete, production-ready solution for managing mission costs with photo uploads. It includes:

- ✅ **Full CRUD operations** matching the API specification
- ✅ **Robust validation** for all inputs and files
- ✅ **Professional UI** with modern design patterns
- ✅ **Comprehensive error handling** with user feedback
- ✅ **Photo gallery** with navigation and thumbnails
- ✅ **Responsive design** for all devices
- ✅ **Type safety** with TypeScript interfaces
- ✅ **Performance optimization** with proper image handling

The implementation is ready for production use and provides an excellent user experience for managing mission costs and receipt photos. 