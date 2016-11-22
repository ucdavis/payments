using System;

namespace Payments.Core
{
    public class DomainObject : DomainObject<int> { }

    public class DomainObject<T>
    {
        public T Id { get; set; }
    }
}
