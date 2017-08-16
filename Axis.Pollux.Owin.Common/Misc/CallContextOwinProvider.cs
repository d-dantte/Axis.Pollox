﻿using Axis.Luna.Operation;
using Microsoft.Owin;
using Owin;
using System.Runtime.Remoting.Messaging;

namespace Axis.Pollux.Owin.Services.Misc
{
    public class CallContextOwinProvider: IOwinContextProvider
    {
        public IOperation<IOwinContext> Owin()
        => LazyOp.Try(() => CallContext.LogicalGetData(CallContextOwinProviderExtension.CallContextOwinKey) as IOwinContext);
    }

    public static class CallContextOwinProviderExtension
    {
        public static readonly string CallContextOwinKey = "Axis.Pollux.Owin.Services.CallContext.OwinContext";

        public static IAppBuilder UseCallContextOwinProvider(this IAppBuilder app)
        => app.Use(async (context, next) =>
        {
            try
            {
                CallContext.LogicalSetData(CallContextOwinKey, context);
                await next();
            }
            finally
            {
                CallContext.FreeNamedDataSlot(CallContextOwinKey);
            }
        });
    }
}
