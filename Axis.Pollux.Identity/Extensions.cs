﻿using Axis.Luna.Extensions;
using Axis.Luna.Operation;
using Axis.Luna.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Pollux.Identity
{
    public static class Extensions
    {
        public static IOperation ValidateModels(this IEnumerable<IValidatable> models) => ValidateModels(models.ToArray());

        public static IOperation ValidateModels(params IValidatable[] models)
        => LazyOp.Try(() =>
        {
            models
                .Select(_m => _m?.Validate() ?? ResolvedOp.Fail(new NullReferenceException())) // validate the model, or create a failed operation if the model is null
                .Where(_op => _op.Succeeded != true)
                .Select(_op => _op.GetException())
                .Aggregate(new List<Exception>(), (agg, next) => agg.UsingValue(_l => _l.Add(next)))
                .ThrowIf(_l => _l.Count > 0, _l => new AggregateException(_l.ToArray()));
        });
    }
}