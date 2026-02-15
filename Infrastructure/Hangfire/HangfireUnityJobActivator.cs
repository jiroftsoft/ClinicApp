using System;
using Hangfire;

namespace ClinicApp.Infrastructure.Hangfire
{
    /// <summary>
    /// فعال‌سازی Jobهای Hangfire از طریق Unity — در محیط Background فاقد HttpContext، مستقیم از Container حل وابستگی می‌شود.
    /// </summary>
    public class HangfireUnityJobActivator : JobActivator
    {
        private readonly Func<Type, object> _resolver;

        public HangfireUnityJobActivator(Func<Type, object> resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public override object ActivateJob(Type jobType)
        {
            return _resolver(jobType);
        }
    }
}
