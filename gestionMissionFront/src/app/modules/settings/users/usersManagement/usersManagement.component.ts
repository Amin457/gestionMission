import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { PagedResponse } from '../../../../core/models/PagedResponse';
import { User } from '../../../../core/models/User';
import { UserService } from '../../../../core/services/User.service';
import { RoleService } from '../../../../core/services/Role.service';
import { Role } from '../../../../core/models/Role';
import { SharedModule } from '../../../../shared/shared.module';
import { DriverStatus, DriverStatusLabel } from '../../../../core/enums/DriverStatus';

@Component({
  selector: 'app-users-management',
  templateUrl: './usersManagement.component.html',
  styleUrls: ['./usersManagement.component.scss'],
  standalone: true,
  imports: [
    SharedModule
  ],
  providers: [MessageService, ConfirmationService, UserService, RoleService]
})
export class UsersManagementComponent implements OnInit {
  DriverStatus = DriverStatus;
  driverStatusOptions = Object.entries(DriverStatusLabel).map(([value, label]) => ({
    label,
    value: Number(value)
  }));
  
  getDriverStatusLabel(status: number | null | undefined): string {
    return DriverStatusLabel[status as DriverStatus] ?? 'N/A';
  }

  users: User[] = [];
  totalRecords: number = 0;
  loading: boolean = false;
  displayDialog: boolean = false;
  userForm: FormGroup;
  isEditing: boolean = false;
  currentPage: number = 1;
  pageSize: number = 10;

  roleOptions: Role[] = [];

  constructor(
    private userService: UserService,
    private fb: FormBuilder,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private roleService: RoleService
  ) {
    this.userForm = this.createUserForm();
  }

  ngOnInit(): void {
    this.loadUsers();
    this.loadRoles();
  }
  loadRoles() {
    this.roleService.getAllRoles().subscribe({
      next: (response: Role[]) => {
        this.roleOptions = response;
      },
      error: (error) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load roles' });
        console.error('Error loading roles:', error);
      }
    });
  }

  createUserForm(): FormGroup {
    return this.fb.group({
      userId: [0],
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      phone: ['', [Validators.required, Validators.pattern('^[0-9]{8,15}$')]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', this.isEditing ? null : Validators.required],
      role: ['Admin', Validators.required],
      currentDriverStatus: [null]
    });
  }

  loadUsers(): void {
    this.loading = true;
    this.userService.getPagedUsers(this.currentPage, this.pageSize).subscribe({
      next: (response: PagedResponse<User>) => {
        this.users = response.data;
        this.totalRecords = response.totalRecords;
        this.loading = false;
      },

      error: (error) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load users' });
        this.loading = false;
      }
    });
  }

  onPageChange(event: any): void {
    this.currentPage = event.page + 1;
    this.pageSize = event.rows;
    this.loadUsers();
  }

  showAddEditDialog(user?: User): void {
    this.resetForm();

    if (user) {
      this.isEditing = true;
      // Clone the user object to avoid modifying the original
      const userToEdit = { ...user };
      // We don't want to display the password hash
      userToEdit.password = '';
      this.userForm.patchValue(userToEdit);

      // Make password optional when editing
      this.userForm.get('password')?.setValidators(null);
      this.userForm.get('password')?.updateValueAndValidity();
    }

    this.displayDialog = true;
  }

  onSubmitUser(): void {
    if (this.userForm.invalid) {
      this.markFormGroupTouched(this.userForm);
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fix the form errors' });
      return;
    }

    this.loading = true;
    const user: User = this.userForm.value;
    user.passwordHash = user.password;
    console.log(this.userForm.value);
    if (this.isEditing) {
      if (!user.password) {
        delete user.password;
      }

      this.userService.updateUser(user).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'User updated successfully' });
          this.displayDialog = false;
          this.loadUsers();
          this.loading = false;
        },
        error: (error) => {
          const errorMessage = error?.error?.Message || 'Failed to update user';
          this.messageService.add({ severity: 'error', summary: 'Error', detail: errorMessage });
          console.error('Error updating user:', error);
          this.loading = false;
        }
      });
    } else {
      this.userService.createUser(user).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'User created successfully' });
          this.displayDialog = false;
          this.loadUsers();
          this.loading = false;
        },
        error: (error) => {
          const errorMessage = error?.error?.Message || 'Failed to create user';
          this.messageService.add({ severity: 'error', summary: 'Error', detail: errorMessage });
          console.error('Error creating user:', error);
          this.loading = false;
        }
      });
    }
  }

  confirmDelete(user: User): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete user ${user.firstName} ${user.lastName}?`,
      header: 'Delete Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.deleteUser(user.userId);
      }
    });
  }

  deleteUser(userId: number): void {
    this.loading = true;
    this.userService.deleteUser(userId).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'User deleted successfully' });
        this.loadUsers();
      },
      error: (error) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete user' });
        console.error('Error deleting user:', error);
        this.loading = false;
      }
    });
  }

  resetForm(): void {
    this.userForm.reset({
      userId: 0,
      role: 'User'
    });
    this.isEditing = false;
    // Reset password validator to required for new users
    this.userForm.get('password')?.setValidators(Validators.required);
    this.userForm.get('password')?.updateValueAndValidity();
  }

  // Helper method to mark all controls in a form group as touched
  markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  getFieldError(fieldName: string): string {
    const control = this.userForm.get(fieldName);
    if (control?.touched && control?.errors) {
      if (control.errors['required']) {
        return 'This field is required';
      }
      if (control.errors['minlength']) {
        return `Minimum length is ${control.errors['minlength'].requiredLength} characters`;
      }
      if (control.errors['email']) {
        return 'Please enter a valid email address';
      }
      if (control.errors['pattern']) {
        return 'Please enter a valid phone number';
      }
    }
    return '';
  }

}