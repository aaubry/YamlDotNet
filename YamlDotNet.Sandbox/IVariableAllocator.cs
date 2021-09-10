using System;
using System.Linq.Expressions;
using YamlDotNet.Helpers;

namespace YamlDotNet.Sandbox
{
    public interface IVariableAllocator
    {
        public ParamExpr<TValue> Allocate<TValue>(object key, Func<Expression<TValue>> initializerFactory);
    }
}
