using Dapper;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Text;
using static Dommel.DommelMapper;

namespace Small.Tools.DataBase
{
    public class SqlCustomExpression<TEntity> : SqlExpression<TEntity>
    {
        private readonly DynamicParameters _parameters = new DynamicParameters();
        private readonly StringBuilder _whereBuilder = new StringBuilder();
        private int _parameterIndex;

        /// <summary>
        /// 访问表达式.
        /// </summary>
        /// <param name="expression">表达式.</param>
        /// <returns>结果.</returns>
        protected override object VisitExpression(Expression expression)
        {
            ExpressionType nodeType = expression.NodeType;
            switch (nodeType)
            {
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)expression);

                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return VisitBinary((BinaryExpression)expression);

                case ExpressionType.Convert:
                case ExpressionType.Not:
                    return VisitUnary((UnaryExpression)expression);

                case ExpressionType.New:
                    return VisitNew((NewExpression)expression);

                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)expression);

                case ExpressionType.Constant:
                    return VisitConstantExpression((ConstantExpression)expression);
                case ExpressionType.Call:
                    return VisitCallExpression((MethodCallExpression)expression);
                case ExpressionType.Invoke:
                    return VisitExpression(((InvocationExpression)expression).Expression);
            }
            return expression;
        }

        /// <summary>
        /// Processes a lambda expression.
        /// </summary>
        /// <param name="epxression">The lambda expression.</param>
        /// <returns>The result of the processing.</returns>
        protected override object VisitLambda(LambdaExpression epxression)
        {
            if (epxression.Body.NodeType == ExpressionType.MemberAccess)
            {
                var member = epxression.Body as MemberExpression;
                if (member?.Expression != null)
                {
                    return $"{VisitMemberAccess(member)} = '1'";
                }
            }

            return VisitExpression(epxression.Body);
        }

        /// <summary>
        /// Processes a member access expression.
        /// </summary>
        /// <param name="expression">The member access expression.</param>
        /// <returns>The result of the processing.</returns>
        protected override object VisitMemberAccess(MemberExpression expression)
        {
            if (expression.Expression?.NodeType == ExpressionType.Parameter)
            {
                return MemberToColumn(expression);
            }

            var member = Expression.Convert(expression, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            return getter();
        }

        /// <summary>
        /// Builds a SQL expression for the specified filter expression.
        /// </summary>
        /// <param name="expression">The filter expression on the entity.</param>
        /// <returns>The current <see cref="SqlExpression{TEntity}"/> instance.</returns>
        public override SqlExpression<TEntity> Where(Expression<Func<TEntity, bool>> expression)
        {
            if (expression != null)
            {
                if (_whereBuilder.Length == 0)
                {
                    // Start a new where expression 开始条件
                    AppendToWhere(null, expression);
                }
                else
                {
                    // Append a where expression with the 'and' operator 在开始条件后面追加
                    AppendToWhere("and", expression);
                }
            }

            return (SqlCustomExpression<TEntity>)this;
        }

        /// <summary>
        /// 获取生成的“SQL”
        /// </summary>
        /// <returns>SQL</returns>
        public new string ToSql() { return this._whereBuilder.ToString(); }

        /// <summary>
        /// 获取生成的“SQL”
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <returns>SQL</returns>
        public new string ToSql(out DynamicParameters parameters)
        {
            parameters = this._parameters;
            return this._whereBuilder.ToString();
        }

        /// <summary>
        /// Processes a new expression.
        /// </summary>
        /// <param name="expression">The new expression.</param>
        /// <returns>The result of the processing.</returns>
        protected override object VisitNew(NewExpression expression)
        {
            var member = Expression.Convert(expression, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            return getter();
        }

        /// <summary>
        /// 处理判断字符.
        /// </summary>
        /// <param name="expression">表达式.</param>
        /// <returns>处理结果.</returns>
        protected override object VisitUnary(UnaryExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    var o = VisitExpression(expression.Operand);
                    if (!(o is string))
                    {
                        return !(bool)o;
                    }

                    if (expression.Operand is MemberExpression)
                    {
                        o = $"{o} = '1'";
                    }

                    return $"not ({o})";
                case ExpressionType.Convert:
                    if (expression.Method != null)
                    {
                        return Expression.Lambda(expression).Compile().DynamicInvoke();
                    }
                    break;
            }

            return VisitExpression(expression.Operand);
        }

        /// <summary>
        /// 将具有指定值的参数添加到此“SQL”.
        /// </summary>
        /// <param name="value">参数值.</param>
        /// <param name="paramName">参数名.</param>
        protected virtual void AddParameter(object value, out string paramName)
        {
            _parameterIndex++;
            paramName = $"@p{_parameterIndex}";
            this._parameters.Add(paramName, value: value);
        }

        /// <summary>
        /// 处理方法调用表达式.
        /// </summary>
        /// <param name="expression">表达式.</param>
        /// <returns>处理结果.</returns>
        protected virtual object VisitCallExpression(MethodCallExpression expression)
        {
            var method = expression.Method.Name.ToLower();
            switch (method)
            {
                case "contains":
                    // Is this a string-contains or array-contains expression?
                    if (expression.Object != null && expression.Object.Type == typeof(string))
                        //Like
                        return VisitContainsExpression(expression, TextSearch.Contains);
                    else
                        // IN
                        return VisitInExpression(expression);
                case "startswith":
                    return VisitContainsExpression(expression, TextSearch.StartsWith);
                case "endswith":
                    return VisitContainsExpression(expression, TextSearch.EndsWith);

                case "isnullorempty":
                case "isnullorwhitespace":
                    return VisitIsNullExpression(expression);

                case "tostring":
                    return VisitTostingExpression(expression, TextSearch.ToString);
                case "trim":
                    return VisitTostingExpression(expression, TextSearch.Trim);

                case "todatetime":
                    return VisitTostingExpression(expression, TextSearch.ToDateTime);   //暂未实现
                default:
                    break;
            }

            return expression;
        }

        /// <summary>
        /// 表达式 “In”
        /// </summary>
        /// <param name="expression">表达式.</param>
        /// <returns>处理结果.</returns>
        protected virtual object VisitInExpression(MethodCallExpression expression)
        {
            Expression collection;
            Expression property;
            if (expression.Object == null && expression.Arguments.Count == 2)
            {
                collection = expression.Arguments[0];
                property = expression.Arguments[1];
            }
            else if (expression.Object != null && expression.Arguments.Count == 1)
            {
                collection = expression.Object;
                property = expression.Arguments[0];
            }
            else
            {
                throw new Exception("Unsupported method call: " + expression.Method.Name);
            }

            var inClause = new StringBuilder("(");
            foreach (var value in (IEnumerable)GetExpressionValue(collection))
            {
                string paramName = string.Empty;
                AddParameter(value, out paramName);
                inClause.Append($"{paramName},");
            }
            if (inClause.Length == 1)
            {
                inClause.Append("null,");
            }
            inClause[inClause.Length - 1] = ')';

            return $"{VisitExpression(property)} in {inClause}";
        }

        /// <summary>
        /// 获取表达式的结果.
        /// </summary>
        /// <param name="expression">表达式.</param>
        /// <returns>获取的结果.</returns>
        protected object GetExpressionValue(Expression expression)
        {
            var member = Expression.Convert(expression, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            return getter();
        }

        /// <summary>
        /// 处理成员访问表达式.
        /// </summary>
        /// <param name="expression">表达式.</param>
        /// <returns>处理结果.</returns>
        protected override object VisitBinary(BinaryExpression expression)
        {
            object left, right;
            var operand = this.BindOperant(expression.NodeType);
            if (operand == "and" || operand == "or")
            {
                // Process left and right side of the "and/or" expression, e.g.:
                // Foo == 42    or      Bar == 42
                //   left    operand     right
                MemberExpression leftMemberExpression = expression.Left as MemberExpression;
                if (((leftMemberExpression != null) && (leftMemberExpression?.Expression != null)) && (leftMemberExpression?.Expression?.NodeType == ExpressionType.Parameter))
                {
                    left = $"{VisitMemberAccess(leftMemberExpression)} = '1'";
                }
                else
                {
                    left = VisitExpression(expression.Left);
                }

                leftMemberExpression = expression.Right as MemberExpression;
                if (((left != null) && (leftMemberExpression?.Expression != null)) && (leftMemberExpression?.Expression?.NodeType == ExpressionType.Parameter))
                {
                    right = $"{VisitMemberAccess(leftMemberExpression)} = '1'";
                }
                else
                {
                    right = VisitExpression(expression.Right);
                }
            }
            else
            {
                // It's a single expression, e.g. Foo == 42
                left = VisitExpression(expression.Left);
                right = VisitExpression(expression.Right);

                if (right == null)
                {
                    // Special case 'is (not) null' syntax
                    if (expression.NodeType == ExpressionType.Equal)
                    {
                        return $"{left} is null";
                    }
                    else
                    {
                        return $"{left} is not null";
                    }
                }
                if (SqlCheck.Check<TEntity>(expression.Right.ToString()))
                    return $"{left} {operand} {right}";

                string paramName = string.Empty;
                AddParameter(right, out paramName);
                return $"{left} {operand} {paramName}";
            }

            return $"{left} {operand} {right}";
        }

        /// <summary>
        /// 包含字符串的表达式 “like Contains|StartsWith|EndsWith”.
        /// </summary>
        /// <param name="expression">表达式.</param>
        /// <param name="textSearch">类型.</param>
        /// <returns>结果.</returns>
        protected virtual object VisitContainsExpression(MethodCallExpression expression, TextSearch textSearch)
        {
            var column = VisitExpression(expression.Object);
            if (expression.Arguments.Count == 0 || expression.Arguments.Count > 1)
            {
                throw new ArgumentException("Contains-expression should contain exactly one argument.", nameof(expression));
            }
            var value = VisitExpression(expression.Arguments[0]);
            var textLike = string.Empty;
            switch (textSearch)
            {
                case TextSearch.Contains:
                    textLike = $"%{value}%";
                    break;
                case TextSearch.StartsWith:
                    textLike = $"{value}%";
                    break;
                case TextSearch.EndsWith:
                    textLike = $"%{value}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Invalid TextSearch value '{textSearch}'.", nameof(textSearch));
            }
            string paramName = string.Empty;
            AddParameter(textLike, out paramName);
            return $"lower({column}) like lower({paramName})";
        }

        /// <summary>
        /// 包含字符串的表达式 “IsNullOrEmpty | IsNullOrWhiteSpace”.
        /// </summary>
        /// <param name="expression">表达式.</param>
        /// <returns>结果.</returns>
        protected virtual object VisitIsNullExpression(MethodCallExpression expression)
        {
            object column = null;
            if (expression.Object != null)
                column = VisitExpression(expression.Object);
            else
                column = new BaseExpressionVisitor().Visit(expression.Arguments[0]);

            object value = null;
            if (expression.Arguments.Count > 0)
                value = VisitExpression(expression.Arguments[0]);
            return $"({value} IS NULL OR {value} = '' OR LEN({value}) <= 0)";
        }

        /// <summary>
        /// 包含字符串的表达式 “ToSting ”.
        /// </summary>
        /// <param name="expression">表达式.</param>
        /// <param name="textSearch">类型.</param>
        /// <returns>结果.</returns>
        protected virtual object VisitTostingExpression(MethodCallExpression expression, TextSearch textSearch)
        {
            //column()
            object column = null;
            if (expression.Object != null)
                column = VisitExpression(expression.Object);
            else
                column = new BaseExpressionVisitor().Visit(expression.Arguments[0]);

            //(value)
            object value = null;
            if (expression.Arguments.Count > 0)
                value = VisitExpression(expression.Arguments[0]);
            var textSQL = string.Empty;
            switch (textSearch)
            {
                case TextSearch.ToString:
                    if (value != null)
                    {
                        //.ToString("yyyy") | .ToString("yyyyMMdd") ...
                        if (typeof(TEntity).GetField(value.ToString()) == null && typeof(TEntity).GetProperty(value.ToString()) == null)
                        {
                            switch (value.ToString())
                            {
                                case "yyyy":
                                    textSQL = $"CONVERT(VARCHAR(4), {column} , 23)"; //2020
                                    break;

                                case "MM-dd":
                                    textSQL = $"CONVERT(VARCHAR(5), {column} , 10)"; //05-08
                                    break;

                                case "yyyMMdd":
                                    textSQL = $"CONVERT(VARCHAR(100), {column} , 112)"; //20200508
                                    break;

                                case "yyyy-MM-dd":
                                    textSQL = $"CONVERT(VARCHAR(100), {column} , 23)"; //2020-05-08
                                    break;

                                case "yyyy-MM-dd hh:mm:ss":
                                    textSQL = $"CONVERT(VARCHAR(100), {column} , 20)"; //2020-05-08 12:20:22
                                    break;

                                case "MMdd":
                                    textSQL = $"SUBSTRING(CONVERT(VARCHAR(100), {column} , 112),5,10 - 5)"; //0508
                                    break;
                                default:
                                    throw new Exception("时间格式错误或暂不支持。");
                            }

                        }
                    }
                    else if (column != null)
                    {
                        //.ToString()
                        if (typeof(TEntity).GetField(column.ToString()) != null || typeof(TEntity).GetProperty(column.ToString()) != null)
                        {
                            //列名
                            textSQL = $"CAST({column} AS VARCHAR(MAX))";
                        }
                        else if (typeof(TEntity).GetField(column.ToString()) == null && typeof(TEntity).GetProperty(column.ToString()) == null)
                        {
                            //非列名
                            textSQL = $"CAST('{column}' AS VARCHAR(MAX))";
                        }
                    }
                    else
                    {
                        throw new Exception("“ToString”解析失败。");
                    }
                    return textSQL;

                case TextSearch.Trim:
                    if (typeof(TEntity).GetField(column.ToString()) != null || typeof(TEntity).GetProperty(column.ToString()) != null)
                    {
                        textSQL = $"LTRIM(RTRIM({column}))";
                    }
                    else
                    {
                        textSQL = $"LTRIM(RTRIM('{column}'))";
                    }

                    return textSQL;

                case TextSearch.ToDateTime:
                    if (value != null)
                    {
                        if (typeof(TEntity).GetField(value.ToString()) != null
                           || typeof(TEntity).GetProperty(value.ToString()) != null)
                        {
                            textSQL = $"CAST({value} AS DATETIME)";
                        }
                    }

                    return textSQL;
                default:
                    throw new ArgumentOutOfRangeException($"Invalid TextSearch value '{textSearch}'.", nameof(textSearch));
            }
        }

        #region > private methods

        /// <summary>
        /// 追加查询条件
        /// </summary>
        /// <param name="conditionOperator">如果不是第一个条件，追加“AND”</param>
        /// <param name="expression">表达式</param>
        private void AppendToWhere(string conditionOperator, Expression expression)
        {
            var sqlExpression = VisitExpression(expression).ToString();
            if (_whereBuilder.Length == 0)
            {
                _whereBuilder.Append(" where ");
            }
            else
            {
                _whereBuilder.AppendFormat(" {0} ", conditionOperator);
            }

            _whereBuilder.Append("(" + sqlExpression + ")");
        }

        #endregion

        /// <summary>
        /// 文本搜索的类型
        /// </summary>
        protected enum TextSearch
        {
            /// <summary>
            /// 匹配字符串中的任何位置 “%n%”。
            /// </summary>
            Contains,

            /// <summary>
            /// 匹配字符串的开头 “n%”.
            /// </summary>
            StartsWith,

            /// <summary>
            /// 匹配字符串的结尾 “%n”.
            /// </summary>
            EndsWith,

            /// <summary>
            /// Tostring
            /// </summary>
            ToString,

            /// <summary>
            /// ToDateTime
            /// </summary>
            ToDateTime,

            /// <summary>
            /// Trim
            /// </summary>
            Trim
        }

    }
}


