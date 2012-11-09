﻿using System;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace OLinq
{

    abstract class EnumerableSourceWithLambdaOperation<TSource, TLambdaResult, TResult> : EnumerableSourceOperation<TSource, TResult>
    {

        LambdaContainer<TSource, TLambdaResult> lambdas;

        public EnumerableSourceWithLambdaOperation(OperationContext context, MethodCallExpression expression, Expression sourceExpression, Expression<Func<TSource, TLambdaResult>> lambdaExpression)
            : base(context, expression, sourceExpression)
        {
            // generate lambda collection
            lambdas = new LambdaContainer<TSource, TLambdaResult>(lambdaExpression, CreateLambdaContext);
            lambdas.CollectionChanged += lambdas_CollectionChanged;
            lambdas.ValueChanged += lambdas_ValueChanged;
            lambdas.Items = Source;
        }

        /// <summary>
        /// Gets the lambda collection which wraps the source.
        /// </summary>
        protected LambdaContainer<TSource, TLambdaResult> Lambdas
        {
            get { return lambdas; }
        }

        /// <summary>
        /// Creates a context for a new lambda expression.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        OperationContext CreateLambdaContext(TSource item, params ParameterExpression[] parameters)
        {
            // generate new parameter
            var ctx = new OperationContext(Context);
            var var = OperationFactory.FromValue(item);
            ctx.Variables[parameters[0].Name] = var;
            return ctx;
        }

        /// <summary>
        /// Invoked when the source collection has been reset.
        /// </summary>
        protected override void OnSourceCollectionReset()
        {
            if (lambdas != null)
                lambdas.Items = Source;
        }

        /// <summary>
        /// Invoked when then lambda collection is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void lambdas_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            OnLambdaCollectionChanged(args);
        }

        /// <summary>
        /// Invoked when then lambda collection is changed.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnLambdaCollectionChanged(NotifyCollectionChangedEventArgs args)
        {

        }

        /// <summary>
        /// Invoked when one of the lambda's value is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void lambdas_ValueChanged(object sender, LambdaValueChangedEventArgs<TSource, TLambdaResult> args)
        {
            OnLambdaValueChanged(args);
        }

        /// <summary>
        /// Invoked when one of the lambda's value is changed.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnLambdaValueChanged(LambdaValueChangedEventArgs<TSource, TLambdaResult> args)
        {

        }

    }

}
