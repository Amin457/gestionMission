import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StepsModule } from 'primeng/steps';
import { MenuItem } from 'primeng/api';
import { Circuit, Route } from '../../../core/models/Circuit';
import { CardModule } from 'primeng/card';
import { PanelModule } from 'primeng/panel';
import { RippleModule } from 'primeng/ripple';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-circuit-details',
  standalone: true,
  imports: [CommonModule, StepsModule, CardModule, PanelModule, RippleModule, TooltipModule],
  templateUrl: './circuit-details.component.html',
  styleUrls: ['./circuit-details.component.scss']
})
export class CircuitDetailsComponent implements OnInit {
  @Input() circuit!: Circuit;

  steps: MenuItem[] = [];
  activeStepIndex: number = 0;

  ngOnInit(): void {
    if (this.circuit && this.circuit.routes) {
      this.initializeSteps();
    }
  }

  initializeSteps(): void {
    this.steps = this.circuit.routes.sort((a, b) => a.ordre - b.ordre).map((route, index) => ({
      label: `${route.departureSiteName} -> ${route.arrivalSiteName}`,
      tooltip: `Distance: ${route.distanceKm.toFixed(2)} km`,
      icon: index === 0 ? 'pi pi-map-marker' : 'pi pi-arrow-right',
      command: () => {
        this.activeStepIndex = index;
      },
      ariaLabel: `Step ${index + 1}: ${route.departureSiteName} to ${route.arrivalSiteName}`
    }));
  }

  get currentRoute(): Route {
    return this.circuit.routes.sort((a, b) => a.ordre - b.ordre)[this.activeStepIndex];
  }
}