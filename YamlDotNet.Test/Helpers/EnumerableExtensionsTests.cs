using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using YamlDotNet.Helpers;

namespace YamlDotNet.Test.Helpers
{
    public class EnumerableExtensionsTests
    {
        private class RestrictedUseEnumerable : IEnumerable<int>
        {
            private readonly int maxUses;
            public int TotalUses { get; private set; }
            public bool Disposed { get; private set; }

            public RestrictedUseEnumerable(int maxUses)
            {
                this.maxUses = maxUses;
            }

            public IEnumerator<int> GetEnumerator()
            {
                if (TotalUses == maxUses)
                {
                    throw new NotSupportedException("Please do not enumerate this object multiple times");
                }

                ++TotalUses;
                return new RestrictedUseEnumerator(this);
            }

            private class RestrictedUseEnumerator : IEnumerator<int>
            {
                private readonly RestrictedUseEnumerable owner;

                public RestrictedUseEnumerator(RestrictedUseEnumerable owner)
                {
                    this.owner = owner;
                }

                public int Current { get; private set; }

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                    owner.Disposed = true;
                }

                public bool MoveNext()
                {
                    return ++Current <= 5;
                }

                public void Reset()
                {
                    Current = -1;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Fact]
        public void Calling_Buffer_does_not_enumerate()
        {
            var sequence = new RestrictedUseEnumerable(0);
            sequence.Buffer();
            Assert.Equal(0, sequence.TotalUses);
        }

        [Fact]
        public void Buffer_enumerates_only_once()
        {
            var sequence = new RestrictedUseEnumerable(1);

            var sut = sequence.Buffer();

            Assert.Equal(0, sequence.TotalUses);
            Assert.Equal(new[] { 1, 2, 3 }, sut.Take(3));
            Assert.Equal(1, sequence.TotalUses);
            Assert.Equal(new[] { 1, 2 }, sut.Take(2));
            Assert.Equal(1, sequence.TotalUses);
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, sut);
            Assert.Equal(1, sequence.TotalUses);
        }

        [Fact]
        public void Buffer_does_not_dispose_the_enumerator_before_reaching_the_end()
        {
            var sequence = new RestrictedUseEnumerable(1);

            var sut = sequence.Buffer();

            sut.Skip(3).Any();
            Assert.False(sequence.Disposed);
        }

        [Fact]
        public void Buffer_disposes_the_enumerator_when_reaching_the_end()
        {
            var sequence = new RestrictedUseEnumerable(1);

            var sut = sequence.Buffer();

            sut.Skip(5).Any();
            Assert.True(sequence.Disposed);
        }

        [Fact]
        public void Exceptions_are_buffered()
        {
            static IEnumerable<int> ThrowAfterFirstItem()
            {
                yield return 1;
                throw new ApplicationException();
            }

            var sut = ThrowAfterFirstItem().Buffer();

            Assert.Equal(1, sut.First());
            Assert.Throws<ApplicationException>(() => sut.ToList());
            Assert.Throws<ApplicationException>(() => sut.ToList());
        }
    }
}
