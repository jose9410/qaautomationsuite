import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, interval, switchMap, takeWhile } from 'rxjs';
import { AplicacionCatalog, ProcessStartRequest, StartResponse, JobState } from '../models/process.models';

/**
 * Servicio para comunicarse con la API de automatización.
 * Las URLs son relativas porque Angular se sirve desde el mismo host (monolito SPA).
 */
@Injectable({ providedIn: 'root' })
export class ProcessService {
  private http = inject(HttpClient);
  private readonly baseUrl = '/api/process';

  /** Carga el catálogo de aplicaciones y sub-procesos desde appsettings.json */
  getCatalog(): Observable<AplicacionCatalog[]> {
    return this.http.get<AplicacionCatalog[]>(`${this.baseUrl}/catalog`);
  }

  /** Inicia un proceso de automatización. Retorna el JobId inmediatamente. */
  startProcess(request: ProcessStartRequest): Observable<StartResponse> {
    return this.http.post<StartResponse>(`${this.baseUrl}/start`, request);
  }

  /** Consulta el estado actual de un job. */
  getStatus(jobId: string): Observable<JobState> {
    return this.http.get<JobState>(`${this.baseUrl}/status/${jobId}`);
  }

  /**
   * Polling reactivo: consulta el estado cada 2 segundos
   * hasta que el job esté en estado Completado o Error.
   */
  pollStatus(jobId: string): Observable<JobState> {
    return interval(2000).pipe(
      switchMap(() => this.getStatus(jobId)),
      takeWhile(
        job => job.status !== 'Completado' && job.status !== 'Error',
        true // incluir el último valor (Completado/Error)
      )
    );
  }
}
