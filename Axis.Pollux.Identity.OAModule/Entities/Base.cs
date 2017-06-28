﻿
using Axis.Luna.Extensions;
using Axis.Luna.Operation;
using System;

namespace Axis.Pollux.Identity.OAModule.Entities
{
    public abstract class PolluxEntity<Key> : IEquatable<PolluxEntity<Key>>
    {
        public Key UniqueId { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public virtual IOperation Validate() => ResolvedOp.Try(() => { });

        public virtual bool Equals(PolluxEntity<Key> other) => other?.UniqueId.Equals(UniqueId) ?? false;

        public override bool Equals(object obj) => Equals(obj.Cast<PolluxEntity<Key>>());
        public override int GetHashCode() => ResolvedOp.Try(() => UniqueId.GetHashCode()).Result;


        public PolluxEntity()
        {
            CreatedOn = DateTime.Now;
        }
    }
}
