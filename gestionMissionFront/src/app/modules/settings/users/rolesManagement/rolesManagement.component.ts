import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';

import { Role } from '../../../../core/models/Role';
import { RoleService } from '../../../../core/services/Role.service';
import { PagedResponse } from '../../../../core/models/PagedResponse';
import { SharedModule } from '../../../../shared/shared.module';

@Component({
  selector: 'app-roles-management',
  templateUrl: './rolesManagement.component.html',
  styleUrls: ['./rolesManagement.component.scss'],
  standalone: true,
  imports: [
    SharedModule 
 ],
  providers: [MessageService, ConfirmationService, RoleService]
})
export class RolesManagementComponent implements OnInit {
  roles: Role[] = [];
  totalRecords: number = 0;
  loading: boolean = false;
  displayDialog: boolean = false;
  roleForm: FormGroup;
  isEditing: boolean = false;
  currentPage: number = 1;
  pageSize: number = 10;

  constructor(
    private roleService: RoleService,
    private fb: FormBuilder,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {
    this.roleForm = this.createRoleForm();
  }

  ngOnInit(): void {
    this.loadRoles();
  }

  createRoleForm(): FormGroup {
    return this.fb.group({
      roleId: [0],
      name: ['', [Validators.required, Validators.minLength(2),Validators.maxLength(15)]],
      code: ['', [Validators.required,Validators.maxLength(15)]],
      libelle: ['', [Validators.required,Validators.maxLength(15)]]
    });
  }

  loadRoles(): void {
    this.loading = true;
    this.roleService.getPagedRoles(this.currentPage, this.pageSize).subscribe({
      next: (response: PagedResponse<Role>) => {
        this.roles = response.data;
        this.totalRecords = response.totalRecords;
        this.loading = false;
      },
      error: (error) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load roles' });
        this.loading = false;
        console.error('Error loading roles:', error);
      }
    });
  }

  onPageChange(event: any): void {
    this.currentPage = event.page + 1;
    this.pageSize = event.rows;
    this.loadRoles();
  }

  showAddEditDialog(role?: Role): void {
    this.resetForm();

    if (role) {
      this.isEditing = true;
      this.roleForm.patchValue({ ...role });
    }

    this.displayDialog = true;
  }

  onSubmitRole(): void {
    if (this.roleForm.invalid) {
      this.markFormGroupTouched(this.roleForm);
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fix the form errors' });
      return;
    }

    const role: Role = this.roleForm.value;
    this.loading = true;

    if (this.isEditing) {
      this.roleService.updateRole(role.roleId, role).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Role updated successfully' });
          this.displayDialog = false;
          this.loadRoles();
          this.loading = false;
        },
        error: (error) => {
          const errorMessage = error?.error?.Message || 'Failed to update role';
          this.messageService.add({ severity: 'error', summary: 'Error', detail: errorMessage });
          console.error('Error updating role:', error);
          this.loading = false;
        }
      });
    } else {
      this.roleService.createRole(role).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Role created successfully' });
          this.displayDialog = false;
          this.loadRoles();
          this.loading = false;
        },
        error: (error) => {
          const errorMessage = error?.error?.Message || 'Failed to create role';
          this.messageService.add({ severity: 'error', summary: 'Error', detail: errorMessage });
          console.error('Error creating role:', error);
          this.loading = false;
        }
      });
    }
  }

  confirmDelete(role: Role): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete role "${role.name}"?`,
      header: 'Delete Confirmation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.deleteRole(role.roleId);
      }
    });
  }

  deleteRole(roleId: number): void {
    this.loading = true;
    this.roleService.deleteRole(roleId).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Role deleted successfully' });
        this.loadRoles();
      },
      error: (error) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete role' });
        console.error('Error deleting role:', error);
        this.loading = false;
      }
    });
  }

  resetForm(): void {
    this.roleForm.reset({
      roleId: 0
    });
    this.isEditing = false;
  }

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
    const control = this.roleForm.get(fieldName);
    if (control?.touched && control?.errors) {
      if (control.errors['required']) {
        return 'This field is required';
      }
      if (control.errors['minlength']) {
        return `Minimum length is ${control.errors['minlength'].requiredLength} characters`;
      }
      if (control.errors['maxlength']) {
        return `Maximum length is ${control.errors['maxlength'].requiredLength} characters`;
      }
    }
    return '';
  }
}
