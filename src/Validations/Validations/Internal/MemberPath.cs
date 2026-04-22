using System.Linq.Expressions;

namespace LightningArc.Validations.Internal;

internal static class MemberPath
{
    public static string GetPath(LambdaExpression expression)
    {
        Stack<string> segments = new();
        Expression? current = expression.Body;

        while (current is MemberExpression member)
        {
            segments.Push(member.Member.Name);
            current = member.Expression;
        }

        return string.Join(".", segments);
    }
}
