using System;

namespace YamlDotNet.Serialization
{
    /// <summary>
    /// Creates objects using a Func{Type,object}"/>.
    /// </summary>
    public sealed class LambdaObjectFactory : ChainedObjectFactory
    {
        private readonly Func<Type, object> factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaObjectFactory"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public LambdaObjectFactory(Func<Type, object> factory) : this(factory, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaObjectFactory" /> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="nextFactory">The next factory.</param>
        /// <exception cref="System.ArgumentNullException">factory</exception>
        public LambdaObjectFactory(Func<Type, object> factory, IObjectFactory nextFactory) : base(nextFactory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            this.factory = factory;
        }

        public override object Create(Type type)
        {
            return factory(type) ?? base.Create(type);
        }
    }
}