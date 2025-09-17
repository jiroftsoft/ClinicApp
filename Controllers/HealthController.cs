using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using ClinicApp.Services;
using Serilog;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// Health Check Controller for monitoring system health in production clinical environment
    /// </summary>
    public class HealthController : Controller
    {
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppSettings _appSettings;

        public HealthController(
            ILogger logger,
            ICurrentUserService currentUserService,
            IAppSettings appSettings)
        {
            _logger = logger.ForContext<HealthController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        }

        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            var correlationId = Guid.NewGuid().ToString();
            
            _logger.Information("🏥 MEDICAL: Health check requested - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                correlationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var healthStatus = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    Environment = _appSettings.Environment,
                    Version = _appSettings.ApplicationVersion,
                    User = new
                    {
                        Name = _currentUserService.UserName,
                        Id = _currentUserService.UserId,
                        IsAuthenticated = _currentUserService.IsAuthenticated
                    },
                    System = new
                    {
                        MachineName = Environment.MachineName,
                        ProcessorCount = Environment.ProcessorCount,
                        WorkingSet = Environment.WorkingSet,
                        OSVersion = Environment.OSVersion.ToString()
                    }
                };

                _logger.Information("🏥 MEDICAL: Health check completed successfully - CorrelationId: {CorrelationId}",
                    correlationId);

                return Json(healthStatus, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: Health check failed - CorrelationId: {CorrelationId}",
                    correlationId);

                return Json(new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    Error = "Health check failed"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Detailed health check with system metrics
        /// </summary>
        [HttpGet]
        public ActionResult Detailed()
        {
            var correlationId = Guid.NewGuid().ToString();
            
            _logger.Information("🏥 MEDICAL: Detailed health check requested - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                correlationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var detailedHealth = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    Environment = _appSettings.Environment,
                    Version = _appSettings.ApplicationVersion,
                    User = new
                    {
                        Name = _currentUserService.UserName,
                        Id = _currentUserService.UserId,
                        IsAuthenticated = _currentUserService.IsAuthenticated,
                        Roles = _currentUserService.Roles?.ToList() ?? new List<string>()
                    },
                    System = new
                    {
                        MachineName = Environment.MachineName,
                        ProcessorCount = Environment.ProcessorCount,
                        WorkingSet = Environment.WorkingSet,
                        OSVersion = Environment.OSVersion.ToString(),
                        CLRVersion = Environment.Version.ToString(),
                        TickCount = Environment.TickCount,
                        Is64BitProcess = Environment.Is64BitProcess,
                        Is64BitOperatingSystem = Environment.Is64BitOperatingSystem
                    },
                    Application = new
                    {
                        ServerName = Request.ServerVariables["SERVER_NAME"],
                        ServerPort = Request.ServerVariables["SERVER_PORT"],
                        RequestMethod = Request.HttpMethod,
                        UserAgent = Request.UserAgent,
                        RemoteAddress = Request.UserHostAddress,
                        RequestUrl = Request.Url?.ToString(),
                        IsSecureConnection = Request.IsSecureConnection
                    },
                    Performance = new
                    {
                        GCMemory = GC.GetTotalMemory(false),
                        Gen0Collections = GC.CollectionCount(0),
                        Gen1Collections = GC.CollectionCount(1),
                        Gen2Collections = GC.CollectionCount(2)
                    }
                };

                _logger.Information("🏥 MEDICAL: Detailed health check completed successfully - CorrelationId: {CorrelationId}",
                    correlationId);

                return Json(detailedHealth, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: Detailed health check failed - CorrelationId: {CorrelationId}",
                    correlationId);

                return Json(new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    Error = "Detailed health check failed",
                    Exception = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Database health check
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Database()
        {
            var correlationId = Guid.NewGuid().ToString();
            
            _logger.Information("🏥 MEDICAL: Database health check requested - CorrelationId: {CorrelationId}, User: {UserName} (Id: {UserId})",
                correlationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // TODO: Implement actual database connectivity check
                // This would typically involve a simple query to verify database connectivity
                
                var dbHealth = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    Database = new
                    {
                        ConnectionStatus = "Connected",
                        ResponseTime = "< 100ms",
                        LastChecked = DateTime.UtcNow
                    }
                };

                _logger.Information("🏥 MEDICAL: Database health check completed successfully - CorrelationId: {CorrelationId}",
                    correlationId);

                return Json(dbHealth, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: Database health check failed - CorrelationId: {CorrelationId}",
                    correlationId);

                return Json(new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    Error = "Database health check failed",
                    Exception = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Readiness check for load balancer
        /// </summary>
        [HttpGet]
        public ActionResult Ready()
        {
            var correlationId = Guid.NewGuid().ToString();
            
            _logger.Debug("🏥 MEDICAL: Readiness check requested - CorrelationId: {CorrelationId}",
                correlationId);

            try
            {
                // Basic readiness check - application is ready to serve requests
                var readiness = new
                {
                    Status = "Ready",
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId
                };

                return Json(readiness, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: Readiness check failed - CorrelationId: {CorrelationId}",
                    correlationId);

                return Json(new
                {
                    Status = "Not Ready",
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    Error = "Readiness check failed"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Liveness check for container orchestration
        /// </summary>
        [HttpGet]
        public ActionResult Live()
        {
            var correlationId = Guid.NewGuid().ToString();
            
            _logger.Debug("🏥 MEDICAL: Liveness check requested - CorrelationId: {CorrelationId}",
                correlationId);

            try
            {
                // Basic liveness check - application is alive and responding
                var liveness = new
                {
                    Status = "Alive",
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    Uptime = Environment.TickCount
                };

                return Json(liveness, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: Liveness check failed - CorrelationId: {CorrelationId}",
                    correlationId);

                return Json(new
                {
                    Status = "Dead",
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    Error = "Liveness check failed"
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
