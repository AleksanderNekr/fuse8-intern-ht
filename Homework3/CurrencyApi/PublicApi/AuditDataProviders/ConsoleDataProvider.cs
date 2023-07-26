using Audit.Core;
using Newtonsoft.Json;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.AuditDataProviders;

internal sealed class ConsoleDataProvider : AuditDataProvider
{
    public override object InsertEvent(AuditEvent auditEvent)
    {
        string json = JsonConvert.SerializeObject(auditEvent,
                                                  new JsonSerializerSettings
                                                  {
                                                      ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                                  });

        Console.WriteLine(json);

        return Guid.NewGuid();
    }
}
