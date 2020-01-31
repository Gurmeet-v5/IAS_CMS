using System.Linq.Expressions;
using System.Web.Mvc;
using MvcContrib.Pagination;
using MvcContrib.UI.Grid;
using System.Linq;
using System;
using KISD.Common;
using System.Reflection;
using System.Collections.Generic;

namespace KISD.Areas.Admin.Models
{
    public class PagedViewModel<T>
    {
        public ViewDataDictionary ViewData { get; set; }
        public IQueryable<T> Query { get; set; }
        public GridSortOptions GridSortOptions { get; set; }
        public string DefaultSortColumn { get; set; }
        public IPagination<T> PagedList { get; private set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public long EmailID { get; set; }
        public string EmailTxt { get; set; }
        public long EmailTypeID { get; set; }
        public string Title { get; set; }
        public string Title2 { get; set; }
        public string strFromDate { get; set; }
        public string strDate { get; set; }
        public string strToDate { get; set; }
        public string strCreateDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime dtDate { get; set; }
        public DateTime dtCreateDate { get; set; }
        public string status { get; set; }
        public string UserType { get; set; }
        public string ModuleType { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<long> CreateByID { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public Nullable<long> LastModifyByID { get; set; }
        public Nullable<bool> IsDeletedInd { get; set; }
        public List<SelectListItem> CommonSelectItemList { get; set; }
        public List<SelectListItem> CommonSelectItemList2 { get; set; }
        public PagedViewModel<T> AddFilter(Expression<Func<T, bool>> predicate)
        {
            Query = Query.Where(predicate);
            return this;
        }

        public PagedViewModel<T> AddFilter<TValue>(string key, TValue value, Expression<Func<T, bool>> predicate)
        {
            ProcessQuery(value, predicate);
            ViewData[key] = value;
            return this;
        }

        public PagedViewModel<T> AddFilter<TValue>(string keyField, object value, Expression<Func<T, bool>> predicate,
            IQueryable<TValue> query, string textField)
        {
            ProcessQuery(value, predicate);
            var selectList = query.ToSelectList(keyField, textField, value);
            ViewData[keyField] = selectList;
            return this;
        }

        public PagedViewModel<T> Setup()
        {
            if (string.IsNullOrWhiteSpace(GridSortOptions.Column))
            {
                GridSortOptions.Column = DefaultSortColumn;
            }
            int? pag = 1;
            var count = Query.Count();

            if (Page > 1)
                pag = count > ((Page - 1) * PageSize.Value) ? Page.Value : (Page.Value) - 1;
            if (PageSize.Value == 0)
            {
                PageSize = count;
                pag = 1;
            }
            PagedList =
            //Query.OrderBy(GridSortOptions.Column, GridSortOptions.Direction)
            //.AsPagination(Page ?? 1, PageSize ?? 10);
            RelationObjectsOrder.OrderBy(Query, this.GridSortOptions).AsPagination(pag ?? 1, PageSize ?? 10);
            return this;
        }

        private void ProcessQuery<TValue>(TValue value, Expression<Func<T, bool>> predicate)
        {
            if (value == null) return;
            if (typeof(TValue) == typeof(string))
            {
                if (string.IsNullOrWhiteSpace(value as string)) return;
            }
            Query = Query.Where(predicate);
        }
    }

    public static class RelationObjectsOrder
    {
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> collection, GridSortOptions sortOptions)
        {
            if (string.IsNullOrEmpty(sortOptions.Column))
            {
                return collection;
            }

            Type collectionType = typeof(T);

            ParameterExpression parameterExpression = Expression.Parameter(collectionType, "p");

            Expression seedExpression = parameterExpression;

            Expression aggregateExpression = sortOptions.Column.Split('.').Aggregate(seedExpression, Expression.Property);

            MemberExpression memberExpression = aggregateExpression as MemberExpression;

            if (memberExpression == null)
            {
                throw new NullReferenceException(string.Format("Unable to cast Member Expression for given path: {0}.", sortOptions.Column));
            }

            LambdaExpression orderByExp = Expression.Lambda(memberExpression, parameterExpression);

            const string orderBy = "OrderBy";

            const string orderByDesc = "OrderByDescending";

            Type childPropertyType = ((PropertyInfo)(memberExpression.Member)).PropertyType;

            string methodToInvoke = sortOptions.Direction == MvcContrib.Sorting.SortDirection.Ascending ? orderBy : orderByDesc;

            var orderByCall = Expression.Call(typeof(Queryable), methodToInvoke, new[] { collectionType, childPropertyType }, collection.Expression, Expression.Quote(orderByExp));

            return collection.Provider.CreateQuery<T>(orderByCall);
        }
    }
}
