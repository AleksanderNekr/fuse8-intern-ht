using Audit.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.AuditDataProviders;

internal sealed class LoggerDataProvider : AuditDataProvider
{
    private readonly ILogger<LoggerDataProvider> _logger;

    public LoggerDataProvider(ILogger<LoggerDataProvider> logger)
    {
        _logger = logger;
    }

    public override object InsertEvent(AuditEvent auditEvent)
    {
        string json = JsonConvert.SerializeObject(auditEvent,
                                                  new JsonSerializerSettings
                                                  {
                                                      ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                                  });

        _logger.LogInformation("Audit event: {Json}", json);

        return Guid.NewGuid();
    }
}
