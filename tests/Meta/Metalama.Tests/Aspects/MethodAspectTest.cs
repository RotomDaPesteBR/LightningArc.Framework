using LightningArc.Metalama;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using LightningArc.Metalama.Tests.Aspects;

namespace LightningArc.Metalama.Tests.Aspects;

public partial class MethodAspectTest
{
    [OverrideMethodWithLoggingAspect]
    public void Test() => Debug.WriteLine("Test");
}

