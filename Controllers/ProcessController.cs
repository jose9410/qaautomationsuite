using qaautomationsuite.Models;
using qaautomationsuite.Services;
using Microsoft.AspNetCore.Mvc;

namespace qaautomationsuite.Controllers;

/// <summary>
/// API REST para gestionar procesos de automatización.
/// POST /api/process/start       → Inicia proceso, retorna JobId inmediatamente
/// GET  /api/process/status/{id} → Consulta estado del job (polling)
/// GET  /api/process/jobs        → Lista todos los jobs (debug)
/// GET  /api/process/catalog     → Devuelve el catálogo de aplicaciones y sub-procesos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProcessController : ControllerBase
{
    private readonly JobManager _jobManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProcessController> _logger;
    private readonly IConfiguration _configuration;

    public ProcessController(
        JobManager jobManager,
        IServiceProvider serviceProvider,
        ILogger<ProcessController> logger,
        IConfiguration configuration)
    {
        _jobManager = jobManager;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Inicia un proceso de automatización en background.
    /// Responde inmediatamente con un JobId para evitar timeouts HTTP.
    /// </summary>
    [HttpPost("start")]
    public IActionResult Start([FromBody] ProcessStartRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NombreAplicacion))
            return BadRequest(new { Error = "NombreAplicacion es requerido" });

        var job = _jobManager.CreateJob(request.NombreAplicacion);

        _logger.LogInformation(
            "Job {JobId} creado para la Aplicación: {App}",
            job.JobId, request.NombreAplicacion);

        // ─── Fire-and-forget controlado ───
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var playwrightService = scope.ServiceProvider
                    .GetRequiredService<PlaywrightService>();

                await playwrightService.ExecuteProcessAsync(request, job.JobId);

                _logger.LogInformation("✅ Job {JobId} completado exitosamente", job.JobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Job {JobId} falló: {Message}", job.JobId, ex.Message);

                _jobManager.UpdateJob(job.JobId, j =>
                {
                    j.Status = JobStatus.Error;
                    j.Mensaje = "Error en el proceso de automatización";
                    j.ErrorDetalle = ex.Message;
                    j.FinalizadoEn = DateTime.Now;
                });
            }
        });

        return Ok(new
        {
            job.JobId,
            Message = "Proceso iniciado en background",
            Status = job.Status.ToString()
        });
    }

    /// <summary>
    /// Consulta el estado actual de un job por su ID.
    /// El frontend hace polling a este endpoint cada 2 segundos.
    /// </summary>
    [HttpGet("status/{jobId}")]
    public IActionResult GetStatus(string jobId)
    {
        var job = _jobManager.GetJob(jobId);
        if (job == null)
            return NotFound(new { Error = $"Job '{jobId}' no encontrado" });

        return Ok(job);
    }

    /// <summary>
    /// Lista todos los jobs registrados (útil para debug y administración).
    /// </summary>
    [HttpGet("jobs")]
    public IActionResult GetAllJobs()
    {
        return Ok(_jobManager.GetAllJobs());
    }

    /// <summary>
    /// Retorna el catálogo de aplicaciones y sub-procesos desde appsettings.json.
    /// GET /api/process/catalog
    /// Formato: [{ nombre, urlEntrada, subProcesos: [{ nombre, processId }] }]
    /// </summary>
    [HttpGet("catalog")]
    public IActionResult GetCatalog()
    {
        var aplicaciones = _configuration
            .GetSection("Aplicaciones")
            .Get<List<AplicacionConfig>>() ?? new List<AplicacionConfig>();

        // Devolver solo lo que necesita el frontend (sin selectores internos)
        var dto = aplicaciones.Select(a => new
        {
            nombre = a.Nombre,
            subProcesos = a.SubProcesos.Select(p => new
            {
                nombre = p.Nombre,
                processId = p.ProcessId
            }).ToList()
        }).ToList();

        return Ok(dto);
    }
}
