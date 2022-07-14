﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SqlSugar 
{
    internal class OneToManyNavgateExpressionN
    {
        #region  Constructor
        public SqlSugarProvider context;
        public string shorName;
        public EntityInfo entityInfo;
        public List<ExpressionItems> items;
        public string whereSql;
        public MethodCallExpressionResolve methodCallExpressionResolve;
        public OneToManyNavgateExpressionN(SqlSugarProvider context, MethodCallExpressionResolve methodCallExpressionResolve)
        {
            this.context = context;
            this.methodCallExpressionResolve= methodCallExpressionResolve;
    }
        #endregion

        internal bool IsNavgate(Expression expression)
        {
            var result = false;
            var exp = expression;
            if (exp is UnaryExpression) 
            {
                exp = (exp as UnaryExpression).Operand;
            }
            if (exp is MethodCallExpression) 
            {
                var memberExp=exp as MethodCallExpression;
                if (memberExp.Method.Name.IsIn("Any","Count") &&  memberExp.Arguments.Count>0 && memberExp.Arguments[0] is MemberExpression ) 
                {
                    result = ValiteOneManyCall(result, memberExp, memberExp.Arguments[0] as MemberExpression, memberExp.Arguments[0]);
                    if (memberExp.Arguments.Count > 1)
                    {
                        whereSql = GetWhereSql(memberExp);
                    }
                }
            }
            return result;
        }
        private bool ValiteOneManyCall(bool result,MethodCallExpression callExpression, MemberExpression memberExp, Expression childExpression)
        {
            if (childExpression != null && childExpression is MemberExpression)
            {
                var oldChildExpression = childExpression;
                var child2Expression = (childExpression as MemberExpression).Expression;
                if (child2Expression == null || (child2Expression is ConstantExpression))
                {
                    return false;
                }
                items = new List<ExpressionItems>();
                var childType = oldChildExpression.Type;
                if (!childType.FullName.IsCollectionsList()) 
                {
                    return false;
                }
                childType = childType.GetGenericArguments()[0];
                items.Add(new ExpressionItems() { Type = 3, Expression = callExpression, ParentEntityInfo = this.context.EntityMaintenance.GetEntityInfo(childType) });
                items.Add(new ExpressionItems() { Type = 2, Expression = oldChildExpression, ThisEntityInfo = this.context.EntityMaintenance.GetEntityInfo(childType), ParentEntityInfo = this.context.EntityMaintenance.GetEntityInfo(child2Expression.Type) });
                if (items.Any(it => it.Type == 2 && it.Nav == null))
                {
                    return false;
                }
                while (child2Expression != null)
                {
                    if (IsClass(child2Expression))
                    {
                        items.Add(new ExpressionItems() { Type = 2, Expression = child2Expression, ThisEntityInfo = this.context.EntityMaintenance.GetEntityInfo(child2Expression.Type), ParentEntityInfo = this.context.EntityMaintenance.GetEntityInfo(GetMemberExpression(child2Expression).Type) });
                        child2Expression = GetMemberExpression(child2Expression);

                    }
                    else if (IsParameter(child2Expression))
                    {
                        shorName = child2Expression.ToString();
                        entityInfo = this.context.EntityMaintenance.GetEntityInfo(child2Expression.Type);
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                if (!items.Any(it => it.Type == 2 && it.Nav == null))
                {
                    return true;
                }
            }
            return result;
        }
        public object GetSql()
        {
            MapperSql MapperSql = new MapperSql();
            var memberInfo = this.items.Where(it => it.Type == 3).First();
            var subInfos = this.items.Where(it => it.Type == 2).Reverse().ToList();
            var formInfo = subInfos.First();
            var joinInfos = subInfos.Skip(1).ToList();
            var i = 0;
            var masterShortName = formInfo.ThisEntityInfo.DbTableName + i;
            var queryable = this.context.Queryable<object>(masterShortName).AS(formInfo.ThisEntityInfo.DbTableName);
            i++;
            var lastShortName = "";
            var index = 0;
            foreach (var item in joinInfos)
            {
                var shortName = item.ThisEntityInfo.DbTableName + i;
                EntityColumnInfo pkColumn;
                EntityColumnInfo navColum;
                if (index == 0)
                {
                    pkColumn = item.ThisEntityInfo.Columns.FirstOrDefault(it => it.PropertyName == item.Nav.Name);
                    navColum = item.ParentEntityInfo.Columns.FirstOrDefault(it => it.IsPrimarykey);
                }
                else
                {
                    pkColumn = item.ThisEntityInfo.Columns.FirstOrDefault(it => it.IsPrimarykey);
                    navColum = item.ParentEntityInfo.Columns.FirstOrDefault(it => it.PropertyName == item.Nav.Name);
                }
                Check.ExceptionEasy(pkColumn == null, $"{item.ThisEntityInfo.EntityName} need PrimayKey", $"使用导航属性{item.ThisEntityInfo.EntityName} 缺少主键");
                var on = $" {shortName}.{queryable.SqlBuilder.GetTranslationColumnName(pkColumn.DbColumnName)}={formInfo.ThisEntityInfo.DbTableName + (i - 1)}.{queryable.SqlBuilder.GetTranslationColumnName(navColum.DbColumnName)}";
                queryable.AddJoinInfo(item.ThisEntityInfo.DbTableName, shortName, on, JoinType.Inner);
                ++i;
                index++;
                lastShortName = shortName;
                formInfo = item;
            }
            queryable.Select($" COUNT(1)");
            var last = subInfos.First();
            var FirstPkColumn = last.ThisEntityInfo.Columns.FirstOrDefault(it => it.IsPrimarykey);
            Check.ExceptionEasy(FirstPkColumn == null, $"{ last.ThisEntityInfo.EntityName} need PrimayKey", $"使用导航属性{ last.ThisEntityInfo.EntityName} 缺少主键");
            var PkColumn = last.ParentEntityInfo.Columns.FirstOrDefault(it => it.PropertyName == last.Nav.Name);
            Check.ExceptionEasy(PkColumn == null, $"{ last.ParentEntityInfo.EntityName} no found {last.Nav.Name}", $"{ last.ParentEntityInfo.EntityName} 不存在 {last.Nav.Name}");
            queryable.Where($" {this.shorName}.{ queryable.SqlBuilder.GetTranslationColumnName(PkColumn.DbColumnName)} = {masterShortName}.{queryable.SqlBuilder.GetTranslationColumnName(FirstPkColumn.DbColumnName)} ");
            queryable.WhereIF(this.whereSql.HasValue(), GetWhereSql1(this.whereSql,lastShortName, joinInfos, queryable.SqlBuilder));
            MapperSql.Sql = $"( {queryable.ToSql().Key} ) ";
            if ((memberInfo.Expression as MethodCallExpression).Method.Name == "Any")
            {
                MapperSql.Sql = $"( {MapperSql.Sql}>0 ) ";

            }
            return MapperSql;
        }


        #region Helper
        private string GetWhereSql1(string wheresql,string lastShortName, List<ExpressionItems> joinInfos,ISqlBuilder sqlBuilder)
        {
            var sql = wheresql;
            if (sql == null) return sql;
            joinInfos.Last().ThisEntityInfo.Columns.ForEach(it =>
            {
                this.whereSql = this.whereSql.Replace(sqlBuilder.GetTranslationColumnName(it.DbColumnName),
                    lastShortName+"." + sqlBuilder.GetTranslationColumnName(it.DbColumnName));

            });
            return sql;
        }

        private string GetWhereSql(MethodCallExpression memberExp)
        {
            var whereExp = memberExp.Arguments[1];
            var result = this.methodCallExpressionResolve.GetNewExpressionValue(whereExp);
            return result;
        }
        private static bool IsParameter(Expression child2Expression)
        {
            return child2Expression.Type.IsClass() && child2Expression is ParameterExpression;
        }

        private static Expression GetMemberExpression(Expression child2Expression)
        {
            return (child2Expression as MemberExpression).Expression;
        }

        private static bool IsClass(Expression child2Expression)
        {
            return child2Expression.Type.IsClass() && child2Expression is MemberExpression;
        }
        #endregion
    }
}
