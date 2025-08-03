import { CommonModule } from '@angular/common';
import { Component, ViewChild } from '@angular/core';
import { BrowserAnimationsModule, NoopAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule, RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { RippleModule } from 'primeng/ripple';
import { Sidebar, SidebarModule } from 'primeng/sidebar';
import { StyleClassModule } from 'primeng/styleclass';
import { AvatarModule } from 'primeng/avatar';
import { PrimeNGConfig } from 'primeng/api';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,  // CommonModule is the right one for standalone components
    RouterModule ,  // For routing
  ],  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})

export class AppComponent {
  title = 'gestionMissionFront';
  constructor(private primengConfig: PrimeNGConfig) {}

ngOnInit() {
  this.primengConfig.ripple = true;

  // Optional: configurer les traductions
  this.primengConfig.setTranslation({
    weak: 'Faible',
    medium: 'Moyen',
    strong: 'Fort',
    passwordPrompt: 'Saisissez un mot de passe'
  });
}
}
