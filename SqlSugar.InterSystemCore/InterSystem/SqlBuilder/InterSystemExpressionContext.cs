using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SqlSugar.InterSystemCore
{
    public class InterSystemExpressionContext : ExpressionContext, ILambdaExpressions
    {
        public SqlSugarProvider Context { get; set; }

        public override string SqlTranslationLeft { get { return ""; } }
        public override string SqlTranslationRight { get { return ""; } }


        public void Resolve(Expression expression, ResolveExpressType resolveType)
        {
            this.ResolveType = resolveType;
            this.Expression = expression;
            BaseResolve resolve = new BaseResolve(new ExpressionParameter() { CurrentExpression = this.Expression, Context = this });
            resolve.Start();
        }
    }
}
