import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { ProcessService } from './services/process.service';
import {
  AplicacionCatalog,
  SubProceso,
  ProcessStartRequest,
  JobState,
  JobStatus,
  STATUS_INFO
} from './models/process.models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit, OnDestroy {
  private processService = inject(ProcessService);
  private pollSub?: Subscription;

  // ─── Catálogo de la API ───
  catalogoAplicaciones: AplicacionCatalog[] = [];
  subProcesosDisponibles: SubProceso[] = [];
  catalogoLoading = true;

  // ─── Selecciones del usuario ───
  aplicacionSeleccionada: string = '';

  // ─── Estado de la UI ───
  isSubmitting = false;
  jobId: string | null = null;
  jobState: JobState | null = null;
  errorMessage: string | null = null;

  // ─── Pasos del proceso (para timeline) ───
  readonly steps: { status: JobStatus; label: string; icon: string }[] = [
    { status: 'Iniciando',           label: 'Navegar',          icon: '🌐' },
    { status: 'LanzandoReproceso',   label: 'Fecha y Proceso',  icon: '📅' },
    { status: 'MonitoreandoTablero', label: 'Iniciar',          icon: '🚀' },
    { status: 'Completado',          label: 'Completado',       icon: '✅' }
  ];

  readonly statusInfo = STATUS_INFO;

  ngOnInit(): void {
    this.processService.getCatalog().subscribe({
      next: (apps) => {
        this.catalogoAplicaciones = apps;
        // Cargar sub-procesos de la primera/única aplicación
        if (apps.length > 0) {
          this.subProcesosDisponibles = apps[0].subProcesos;
          this.aplicacionSeleccionada = apps[0].nombre;
        }
        this.catalogoLoading = false;
      },
      error: () => {
        this.catalogoLoading = false;
      }
    });
  }

  onSubmit(): void {
    this.errorMessage = null;
    this.jobState = null;
    this.isSubmitting = true;

    const request: ProcessStartRequest = {
      nombreAplicacion: this.aplicacionSeleccionada
    };

    this.processService.startProcess(request).subscribe({
      next: (response) => {
        this.jobId = response.jobId;
        this.isSubmitting = false;
        this.startPolling(response.jobId);
      },
      error: (err) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.error || err.message || 'Error al iniciar el proceso';
      }
    });
  }

  private startPolling(jobId: string): void {
    this.pollSub?.unsubscribe();
    this.pollSub = this.processService.pollStatus(jobId).subscribe({
      next: (state) => { this.jobState = state; },
      error: (err) => {
        this.errorMessage = 'Error al consultar estado: ' + (err.message || 'desconocido');
      }
    });
  }

  resetForm(): void {
    this.pollSub?.unsubscribe();
    this.jobId = null;
    this.jobState = null;
    this.errorMessage = null;
    this.isSubmitting = false;
  }

  getCurrentStepIndex(): number {
    if (!this.jobState) return -1;
    return this.steps.findIndex(s => s.status === this.jobState!.status);
  }

  isStepCompleted(index: number): boolean {
    const current = this.getCurrentStepIndex();
    return current > index || this.jobState?.status === 'Completado';
  }

  isStepActive(index: number): boolean {
    return this.getCurrentStepIndex() === index;
  }

  getDuration(): string {
    if (!this.jobState) return '';
    const start = new Date(this.jobState.iniciadoEn);
    const end = this.jobState.finalizadoEn ? new Date(this.jobState.finalizadoEn) : new Date();
    const diff = Math.floor((end.getTime() - start.getTime()) / 1000);
    const min = Math.floor(diff / 60);
    const sec = diff % 60;
    return min > 0 ? `${min}m ${sec}s` : `${sec}s`;
  }

  getDurationMinutos(): string {
    if (!this.jobState) return '0.0m';
    const start = new Date(this.jobState.iniciadoEn);
    const end = this.jobState.finalizadoEn ? new Date(this.jobState.finalizadoEn) : new Date();
    const diffMs = end.getTime() - start.getTime();
    const min = diffMs / 60000;
    return min.toFixed(1) + 'm';
  }

  ngOnDestroy(): void {
    this.pollSub?.unsubscribe();
  }
}
