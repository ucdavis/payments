using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Payments.Tests.Helpers
{
    public static class MockExtensions
    {
        public static Mock<DbSet<T>> MockDbSet<T>(this IQueryable<T> data) where T : class, new()
        {
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }
}
