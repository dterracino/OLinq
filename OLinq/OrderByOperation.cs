﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace OLinq
{

    static class OrderByOperation
    {

        public static IOperation CreateOperation(OperationContext context, MethodCallExpression expression)
        {
            return Operation.CreateMethodCallOperation(typeof(OrderByOperation<,>), context, expression, 0, 1);
        }

    }

    class OrderByOperation<TSource, TKey> : EnumerableSourceWithFuncOperation<TSource, TKey, IEnumerable<TSource>>, IOrderedEnumerable<TSource>, INotifyCollectionChanged, IEnumerable<TSource>
    {

        SortedSet<FuncOperation<TKey>> sort = new SortedSet<FuncOperation<TKey>>(new FuncResultComparer<TKey>());

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="expression"></param>
        public OrderByOperation(OperationContext context, MethodCallExpression expression)
            : base(context, expression, expression.Arguments[0], expression.Arguments[1].UnpackLambda<TSource, TKey>())
        {
            SetValue(this);
        }

        protected override void OnLambdaCollectionReset()
        {
            Reset();
        }

        protected override void OnLambdaCollectionItemsAdded(IEnumerable<FuncOperation<TKey>> newItems, int startingIndex)
        {
            foreach (var item in newItems)
                sort.Add(item);

            NotifyCollectionChangedUtil.RaiseAddEvent<TSource>(RaiseCollectionChanged, newItems.Select(i => Funcs[i]));
        }

        protected override void OnLambdaCollectionItemsRemoved(IEnumerable<FuncOperation<TKey>> oldItems, int startingIndex)
        {
            foreach (var item in oldItems)
                sort.Remove(item);

            NotifyCollectionChangedUtil.RaiseRemoveEvent<TSource>(RaiseCollectionChanged, oldItems.Select(i => Funcs[i]));
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            return sort.Select(i => Funcs[i]).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IOrderedEnumerable<TSource> CreateOrderedEnumerable<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resets the collection based on the underlying lambdas.
        /// </summary>
        void Reset()
        {
            // remove obsolete items
            var oldItems = sort.Except(Funcs).ToList();
            foreach (var item in oldItems)
                sort.Remove(item);

            // add missing items
            var newItems = Funcs.Except(sort).ToList();
            foreach (var item in newItems)
                sort.Add(item);

            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises the CollectionChanged event.
        /// </summary>
        /// <param name="args"></param>
        void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }
    }

}
