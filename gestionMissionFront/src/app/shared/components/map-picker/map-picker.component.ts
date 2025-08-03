import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ViewChild, ElementRef, PLATFORM_ID, Inject, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import Map from 'ol/Map';
import View from 'ol/View';
import TileLayer from 'ol/layer/Tile';
import OSM from 'ol/source/OSM';
import { fromLonLat, transform } from 'ol/proj';
import { Feature } from 'ol';
import { Point } from 'ol/geom';
import { Vector as VectorLayer } from 'ol/layer';
import { Vector as VectorSource } from 'ol/source';
import { Style, Icon } from 'ol/style';
import { defaults as defaultControls } from 'ol/control';
import MapBrowserEvent from 'ol/MapBrowserEvent';
import { EventsKey } from 'ol/events';
import { unByKey } from 'ol/Observable';

@Component({
  selector: 'app-map-picker',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="map-container" [style.height.px]="mapHeight">
      <div #mapElement class="map"></div>
      <div class="map-controls">
        <div class="coordinate-inputs">
          <div class="input-group">
            <label>Latitude:</label>
            <input type="number" [(ngModel)]="latitude" (ngModelChange)="onCoordinateChange()" step="0.000001" />
          </div>
          <div class="input-group">
            <label>Longitude:</label>
            <input type="number" [(ngModel)]="longitude" (ngModelChange)="onCoordinateChange()" step="0.000001" />
          </div>
        </div>
        <div class="control-buttons">
          <button (click)="zoomIn()" class="control-button">+</button>
          <button (click)="zoomOut()" class="control-button">-</button>
          <button (click)="getCurrentLocation()" class="control-button">📍</button>
        </div>
      </div>
      <div *ngIf="isLoading" class="loading-overlay">
        <div class="loading-spinner"></div>
      </div>
      <div *ngIf="address" class="address-display">
        {{ address }}
      </div>
    </div>
  `,
  styles: [`
    .map-container {
      position: relative;
      width: 100%;
      border-radius: 8px;
      overflow: hidden;
    }
    .map {
      width: 100%;
      height: 100%;
    }
    .map-controls {
      position: absolute;
      top: 10px;
      right: 10px;
      z-index: 1000;
      background: white;
      padding: 10px;
      border-radius: 4px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    .coordinate-inputs {
      margin-bottom: 10px;
    }
    .input-group {
      display: flex;
      align-items: center;
      margin-bottom: 5px;
    }
    .input-group label {
      width: 80px;
      font-size: 12px;
    }
    .input-group input {
      width: 120px;
      padding: 4px;
      border: 1px solid #ddd;
      border-radius: 4px;
    }
    .control-buttons {
      display: flex;
      gap: 5px;
    }
    .control-button {
      width: 30px;
      height: 30px;
      border: none;
      border-radius: 4px;
      background: #f0f0f0;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 16px;
    }
    .control-button:hover {
      background: #e0e0e0;
    }
    .loading-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(255,255,255,0.8);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }
    .loading-spinner {
      width: 40px;
      height: 40px;
      border: 4px solid #f3f3f3;
      border-top: 4px solid #3498db;
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }
    .address-display {
      position: absolute;
      bottom: 10px;
      left: 10px;
      right: 10px;
      background: white;
      padding: 8px;
      border-radius: 4px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      font-size: 12px;
      text-align: center;
    }
    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
  `]
})
export class MapPickerComponent implements OnInit, OnDestroy, OnChanges {
  @ViewChild('mapElement') mapElement!: ElementRef;
  @Input() latitude: number = 36.8065;
  @Input() longitude: number = 10.1815;
  @Input() mapHeight: number = 400;
  @Output() coordinatesChange = new EventEmitter<{latitude: number, longitude: number}>();

  private map!: Map;
  private markerLayer!: VectorLayer<VectorSource>;
  private markerSource!: VectorSource;
  private markerFeature!: Feature;
  private eventKeys: EventsKey[] = [];
  private isMapInitialized = false;
  isLoading = true;
  address: string = '';

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {}

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      setTimeout(() => {
        this.initializeMap();
      }, 100);
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (this.isMapInitialized && (changes['latitude'] || changes['longitude'])) {
      this.updateMarkerPosition();
    }
  }

  ngOnDestroy() {
    if (this.map) {
      this.eventKeys.forEach(key => unByKey(key));
      this.map.setTarget(undefined);
    }
  }

  private initializeMap() {
    try {
      // Create marker source and layer
      this.markerSource = new VectorSource();
      this.markerLayer = new VectorLayer({
        source: this.markerSource,
        style: new Style({
          image: new Icon({
            src: 'assets/marker-icon.png',
            scale: 1
          })
        })
      });

      // Create map
      this.map = new Map({
        target: this.mapElement.nativeElement,
        layers: [
          new TileLayer({
            source: new OSM({
              attributions: []
            })
          }),
          this.markerLayer
        ],
        view: new View({
          center: fromLonLat([this.longitude, this.latitude]),
          zoom: 8
        }),
        controls: []
      });

      // Add marker
      this.addMarker();

      // Map events
      this.map.on('moveend', () => this.updateCoordinatesFromMap());
      const clickKey = this.map.on('click', (event: any) => {
        const coords = event.coordinate;
        const lonLat = transform(coords, 'EPSG:3857', 'EPSG:4326');
        this.longitude = lonLat[0];
        this.latitude = lonLat[1];
        this.updateMarkerPosition();
        this.updateCoordinates();
      });

      // Store the event key for cleanup
      this.eventKeys.push(clickKey);

      // Handle loading
      this.map.once('postrender', () => {
        this.isLoading = false;
        this.isMapInitialized = true;
      });

      // Handle cursor changes
      const mapElement = this.map.getTargetElement();
      mapElement.style.cursor = 'grab';

      this.map.on('pointermove', (event: any) => {
        if (event.dragging) {
          mapElement.style.cursor = 'grabbing';
        } else {
          mapElement.style.cursor = 'grab';
        }
      });

      // Handle pointer up using the map's target element
      mapElement.addEventListener('pointerup', () => {
        mapElement.style.cursor = 'grab';
      });

      // Set a timeout to ensure loading state is cleared even if postrender doesn't fire
      setTimeout(() => {
        this.isLoading = false;
        this.isMapInitialized = true;
      }, 5000);
    } catch (error) {
      console.error('Error initializing map:', error);
      this.isLoading = false;
    }
  }

  private addMarker() {
    const coordinates = fromLonLat([this.longitude, this.latitude]);
    this.markerFeature = new Feature({
      geometry: new Point(coordinates)
    });
    this.markerSource.addFeature(this.markerFeature);
  }

  private updateMarkerPosition() {
    if (this.markerFeature) {
      const coordinates = fromLonLat([this.longitude, this.latitude]);
      (this.markerFeature.getGeometry() as Point).setCoordinates(coordinates);
      this.map.getView().animate({
        center: coordinates,
        duration: 500
      });
    }
  }

  private updateCoordinatesFromMap() {
    const center = this.map.getView().getCenter();
    if (center) {
      const lonLat = transform(center, 'EPSG:3857', 'EPSG:4326');
      this.longitude = lonLat[0];
      this.latitude = lonLat[1];
      this.updateCoordinates();
    }
  }

  onCoordinateChange() {
    this.updateMarkerPosition();
    this.updateCoordinates();
  }

  private updateCoordinates() {
    this.coordinatesChange.emit({
      latitude: this.latitude,
      longitude: this.longitude
    });
  }

  zoomIn(event?: Event) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    const view = this.map.getView();
    const zoom = view.getZoom();
    if (zoom) {
      view.animate({
        zoom: zoom + 1,
        duration: 250
      });
    }
  }

  zoomOut(event?: Event) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    const view = this.map.getView();
    const zoom = view.getZoom();
    if (zoom) {
      view.animate({
        zoom: zoom - 1,
        duration: 250
      });
    }
  }

  getCurrentLocation(event?: Event) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    if (navigator.geolocation) {
      this.isLoading = true;
      navigator.geolocation.getCurrentPosition(
        (position) => {
          this.latitude = position.coords.latitude;
          this.longitude = position.coords.longitude;
          this.updateMarkerPosition();
          this.updateCoordinates();
          this.isLoading = false;
        },
        (error) => {
          console.error('Error getting location:', error);
          this.isLoading = false;
        }
      );
    }
  }
} 