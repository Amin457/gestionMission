import { Component } from '@angular/core';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Observable } from 'rxjs';
import { TopbarComponent } from '../topbar/topbar.component';
import { SidebarService } from '../../core/services/Sidebar.service';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent,TopbarComponent],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.scss'
})
export class AdminLayoutComponent {

  isCollapsed$: Observable<boolean> | undefined;

  constructor(private sidebarService: SidebarService) {}

  ngOnInit() {
    this.isCollapsed$ = this.sidebarService.isCollapsed$;
  }
}