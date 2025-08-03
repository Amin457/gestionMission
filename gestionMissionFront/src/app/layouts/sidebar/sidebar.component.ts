import { CommonModule } from '@angular/common';
import { Component, HostListener, OnDestroy, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { isPlatformBrowser } from '@angular/common';
import { SidebarService } from '../../core/services/Sidebar.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule
  ],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  animations: [
    trigger('submenuAnimation', [
      state('hidden', style({ height: '0px', opacity: 0, overflow: 'hidden' })),
      state('visible', style({ height: '*', opacity: 1 })),
      transition('hidden <=> visible', [animate('0.25s ease-in-out')])
    ])
  ]
})
export class SidebarComponent implements OnInit, OnDestroy {
  isCollapsed = false;
  private subscription: Subscription = new Subscription();
  screenWidth = 1024;
  hoveredItem: any = null;
  isBrowser: boolean;
  popupPosition = { top: '0', left: '0' };
  showPopup = false;
  clickedItem: any = null;

  menuItems = [
    {
      icon: 'bx-home',
      name: 'Dashboard',
      link: '/dashboard',
      exact: true
    },
    {
      expanded: false,
      icon: 'bx-data',
      name: 'Master Data',
      children: [
        { name: 'Articles', link: '/data/articles' },
        { name: 'City Management', link: '/data/villes' },
        { name: 'Site Management', link: '/data/sites' },
        { name: 'Vehicle Management', link: '/data/vehicules' }
      ]
    },
    {
      expanded: false,
      icon: 'bx-briefcase',
      name: 'Mission Management',
      children: [
        { name: 'View & Filter Missions', link: '/missions/list' },
        { name: 'Driver Calendar', link: '/missions/calendar' },
        { name: 'Missions Costs', link: '/missions/costs' },
        { name: 'Incident Management', link: '/missions/incidents' }
      ]
    },
    {
      expanded: false,
      icon: 'bx-map',
      name: 'Driver trip',
      children: [
        { name: 'Trip Management', link: '/circuits/trajets' }
      ]
    },
    {
      expanded: false,
      icon: 'bx-user',
      name: 'User Management',
      children: [
        { name: 'Users', link: '/settings/users/users-management' },
        { name: 'Roles', link: '/settings/users/roles-management' },
      ]
    },
    {
      expanded: false,
      icon: 'bx-car',
      name: 'Vehicle Fleet',
      children: [
        { name: 'Vehicle Reservation', link: '/vehicles/reservation' }
      ]
    },
    {
      expanded: false,
      icon: 'bx-bell',
      name: 'Notifications',
      children: [
        { name: 'Notification History', link: '/settings/notifications/history' }
      ]
    },
    {
      icon: 'bx-log-out',
      name: 'Logout',
      link: '/logout'
    }
  ];
  

  constructor(
    private sidebarService: SidebarService,
    public router: Router,
    private authService: AuthService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
    if (this.isBrowser) {
      this.screenWidth = window.innerWidth;
    }
  }

  @HostListener('window:resize', ['$event'])
  onResize() {
    if (this.isBrowser) {
      this.screenWidth = window.innerWidth;
      if (this.screenWidth <= 768 && !this.isCollapsed) {
        this.sidebarService.toggleSidebar();
      }
    }
  }

  ngOnInit() {
    this.subscription = this.sidebarService.isCollapsed$.subscribe(
      status => {
        this.isCollapsed = status;
        if (this.isCollapsed) {
          this.closeAllSubmenus();
          this.showPopup = false;
        }
      }
    );

    this.setActiveMenuFromRoute(this.router.url);

    if (this.isBrowser && this.screenWidth <= 768) {
      this.sidebarService.toggleSidebar();
    }
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  toggleSubmenu(item: any, event?: MouseEvent) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }

    if (this.isCollapsed) {
      return;
    }

    if (!item.expanded) {
      this.closeAllSubmenus(item);
    }

    item.expanded = !item.expanded;
    this.clickedItem = item.expanded ? item : null;
  }

  closeAllSubmenus(exceptItem?: any) {
    this.menuItems.forEach(item => {
      if (item !== exceptItem && item.children) {
        item.expanded = false;
      }
    });
  }

  setActiveMenuFromRoute(url: string) {
    this.menuItems.forEach(item => {
      if (item.link === url) {
        return;
      }

      if (item.children) {
        const activeChild = item.children.find((child: any) => url.startsWith(child.link));
        if (activeChild) {
          item.expanded = true;
          this.clickedItem = item;
        }
      }
    });
  }

  onMouseEnter(item: any, event: MouseEvent) {
    if (this.isCollapsed && item.children) {
      this.hoveredItem = item;
      this.showPopup = true;

      const target = event.target as HTMLElement;
      const rect = target.getBoundingClientRect();
      this.popupPosition = {
        top: `${rect.top}px`,
        left: `${rect.right + 10}px`
      };
    }
  }

  onMouseLeave() {
    if (this.isCollapsed && !this.showPopup) {
      this.hoveredItem = null;
    }
  }

  onItemClick(item: any, event: MouseEvent) {
    // Handle logout click
    if (item.name === 'Logout') {
      event.preventDefault();
      event.stopPropagation();
      this.authService.logout();
      return;
    }

    if (this.isCollapsed && item.children) {
      event.preventDefault();
      event.stopPropagation();
      this.hoveredItem = item;
      this.showPopup = true;

      const target = event.target as HTMLElement;
      const rect = target.getBoundingClientRect();
      this.popupPosition = {
        top: `${rect.top}px`,
        left: `${rect.right + 10}px`
      };
    }
  }

  onPopupItemClick() {
    if (this.isCollapsed) {
      this.showPopup = true;
    }
  }

  closePopup() {
    this.showPopup = false;
  }
  hasActiveChild(item: any): boolean {
    return item.children?.some((child: any) => this.isChildActive(child));
  }

  isChildActive(child: any): boolean {
    return this.router.isActive(child.link, false);
  }

  setActiveChild(child: any) {
    this.menuItems.forEach(item => {
      if (item.children && !item.children.includes(child)) {
        item.expanded = false;
      }
    });
  }

  isActive(item: any): boolean {
    if (item.link) {
      return this.router.isActive(item.link, item.exact || false);
    }
    return false;
  }
}