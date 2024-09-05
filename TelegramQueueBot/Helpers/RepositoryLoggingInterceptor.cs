using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace TelegramQueueBot.Helpers
{
    public class RepositoryLoggingInterceptor : IInterceptor
    {
        private readonly ILogger<RepositoryLoggingInterceptor> _logger;

        public RepositoryLoggingInterceptor(ILogger<RepositoryLoggingInterceptor> logger)
        {
            _logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            // Get the hash of the object calling the method (the target instance)
            var instance = invocation.InvocationTarget;

            // Log the hash of the calling instance

            // Proceed with the original method execution
            invocation.Proceed();
        }
    }
}
