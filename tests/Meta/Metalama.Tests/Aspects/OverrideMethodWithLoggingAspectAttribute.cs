using LightningArc.Metalama;
using Metalama.Extensions.DependencyInjection;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Microsoft.Extensions.Logging;

namespace LightningArc.Metalama.Tests.Aspects;

public class OverrideMethodWithLoggingAspectAttribute : OverrideMethodAspect
{
    const string defaultLoggerName = "_logger";
    protected readonly bool _requiresLogger;
    protected bool _hasLogger;

    public OverrideMethodWithLoggingAspectAttribute()
        : this(true) { }

    public OverrideMethodWithLoggingAspectAttribute(bool requiresLogger)
    {
        _requiresLogger = requiresLogger;
    }

    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        IntroduceLogger(builder);
        builder.Override(nameof(OverrideMethod));
    }

    public override dynamic? OverrideMethod()
    {
        dynamic? logger = meta.This._logger;

        return meta.Proceed();
    }

    [CompileTime]
    protected IAspectBuilder<IMethod> IntroduceLogger(IAspectBuilder<IMethod> builder)
    {
        INamedType parentType = builder.Target.DeclaringType;

        if (parentType.Fields.Any(f => f.Name == defaultLoggerName))
        {
            _hasLogger = true;
        }

        if (!_hasLogger && _requiresLogger)
        {
            INamedType targetType = builder.Target.DeclaringType;

            var target = builder.With(targetType);

            INamedType loggerType = (
                NamedTypeFactory.GetType(typeof(ILogger<>))
            ).MakeGenericInstance(targetType);

            target.IntroduceDependency(loggerType, new() { MemberName = defaultLoggerName });

            _hasLogger = true;
        }

        return builder;
    }
}
