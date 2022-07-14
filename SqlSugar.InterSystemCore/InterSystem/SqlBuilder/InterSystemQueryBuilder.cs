using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SqlSugar.InterSystemCore
{
    public class InterSystemQueryBuilder: QueryBuilder
    {
        public override string PageTempalte 
        {
            get { return @"SELECT * FROM ({0}) T WHERE %VID BETWEEN {1} AND {2}"; }
        }

        public override string ExternalPageTempalte
        {
            get { return @"SELECT * FROM ({0}) T WHERE %VID BETWEEN {1} AND {2}"; }
        }

        public override ExpressionResult GetExpressionValue(Expression expression, ResolveExpressType resolveType)
        {
            ILambdaExpressions resolveExpress = this.LambdaExpressions;
            if (resolveType.IsIn(ResolveExpressType.FieldSingle, 
                ResolveExpressType.FieldMultiple, ResolveExpressType.SelectSingle, 
                ResolveExpressType.SelectMultiple) && (expression is LambdaExpression) && (expression as LambdaExpression).Body is BinaryExpression)
            {
                resolveType = resolveType.IsIn(ResolveExpressType.SelectSingle, ResolveExpressType.FieldSingle) ? ResolveExpressType.WhereSingle : ResolveExpressType.WhereMultiple;
            }
            this.LambdaExpressions.Clear();
            if (this.Context.CurrentConnectionConfig.MoreSettings != null)
            {
                resolveExpress.TableEnumIsString = this.Context.CurrentConnectionConfig.MoreSettings.TableEnumIsString;
                resolveExpress.PgSqlIsAutoToLower = this.Context.CurrentConnectionConfig.MoreSettings.PgSqlIsAutoToLower;
            }
            else
            {
                resolveExpress.PgSqlIsAutoToLower = true;
            }
            resolveExpress.SugarContext = new ExpressionOutParameter() { Context = this.Context };
            resolveExpress.RootExpression = expression;
            resolveExpress.JoinQueryInfos = Builder.QueryBuilder.JoinQueryInfos;
            resolveExpress.IsSingle = IsSingle() && resolveType != ResolveExpressType.WhereMultiple;
            resolveExpress.MappingColumns = Context.MappingColumns;
            resolveExpress.MappingTables = Context.MappingTables;
            resolveExpress.IgnoreComumnList = Context.IgnoreColumns;
            resolveExpress.SqlFuncServices = Context.CurrentConnectionConfig.ConfigureExternalServices == null ? null : Context.CurrentConnectionConfig.ConfigureExternalServices.SqlFuncServices;
            resolveExpress.InitMappingInfo = this.Context.InitMappingInfo;
            resolveExpress.RefreshMapping = () =>
            {
                resolveExpress.MappingColumns = Context.MappingColumns;
                resolveExpress.MappingTables = Context.MappingTables;
                resolveExpress.IgnoreComumnList = Context.IgnoreColumns;
                resolveExpress.SqlFuncServices = Context.CurrentConnectionConfig.ConfigureExternalServices == null ? null : Context.CurrentConnectionConfig.ConfigureExternalServices.SqlFuncServices;
            };
            resolveExpress.Resolve(expression, resolveType);
            this.Parameters.AddRange(resolveExpress.Parameters.Select(it => new SugarParameter(it.ParameterName, it.Value, it.DbType)));
            var result = resolveExpress.Result;
            var isSingleTableHasSubquery = IsSingle() && resolveExpress.SingleTableNameSubqueryShortName.HasValue();
            if (isSingleTableHasSubquery)
            {
                Check.Exception(!string.IsNullOrEmpty(this.TableShortName) && resolveExpress.SingleTableNameSubqueryShortName != this.TableShortName, "{0} and {1} need same name", resolveExpress.SingleTableNameSubqueryShortName, this.TableShortName);
                this.TableShortName = resolveExpress.SingleTableNameSubqueryShortName;
            }
            return result;
        }


    }
}
