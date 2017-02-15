//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace YamlDotNet.Serialization
{
    internal sealed class LazyComponentRegistrationList<TArgument, TComponent> : IEnumerable<Func<TArgument, TComponent>>
    {
        private readonly List<LazyComponentRegistration> entries = new List<LazyComponentRegistration>();

        public LazyComponentRegistrationList<TArgument, TComponent> Clone()
        {
            var clone = new LazyComponentRegistrationList<TArgument, TComponent>();
            foreach (var entry in entries)
            {
                clone.entries.Add(entry);
            }
            return clone;
        }

        public sealed class LazyComponentRegistration
        {
            public readonly Type ComponentType;
            public readonly Func<TArgument, TComponent> Factory;

            public LazyComponentRegistration(Type componentType, Func<TArgument, TComponent> factory)
            {
                ComponentType = componentType;
                Factory = factory;
            }
        }

        public sealed class TrackingLazyComponentRegistration
        {
            public readonly Type ComponentType;
            public readonly Func<TComponent, TArgument, TComponent> Factory;

            public TrackingLazyComponentRegistration(Type componentType, Func<TComponent, TArgument, TComponent> factory)
            {
                ComponentType = componentType;
                Factory = factory;
            }
        }

        public void Add(Type componentType, Func<TArgument, TComponent> factory)
        {
            entries.Add(new LazyComponentRegistration(componentType, factory));
        }

        public void Remove(Type componentType)
        {
            for (int i = 0; i < entries.Count; ++i)
            {
                if(entries[i].ComponentType == componentType)
                {
                    entries.RemoveAt(i);
                    return;
                }
            }

            throw new KeyNotFoundException(string.Format("A component registration of type '{0}' was not found.", componentType.FullName));
        }

        public int Count {  get { return entries.Count; } }

        public IEnumerable<Func<TArgument, TComponent>> InReverseOrder
        {
            get
            {
                for (int i = entries.Count - 1; i >= 0; --i)
                {
                    yield return entries[i].Factory;
                }
            }
        }

        public IRegistrationLocationSelectionSyntax<TComponent> CreateRegistrationLocationSelector(
            Type componentType,
            Func<TArgument, TComponent> factory
        )
        {
            return new RegistrationLocationSelector(
                this,
                new LazyComponentRegistration(componentType, factory)
            );
        }

        public ITrackingRegistrationLocationSelectionSyntax<TComponent> CreateTrackingRegistrationLocationSelector(
            Type componentType,
            Func<TComponent, TArgument, TComponent> factory
        )
        {
            return new TrackingRegistrationLocationSelector(
                this,
                new TrackingLazyComponentRegistration(componentType, factory)
            );
        }

        public IEnumerator<Func<TArgument, TComponent>> GetEnumerator()
        {
            return entries.Select(e => e.Factory).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int IndexOfRegistration(Type registrationType)
        {
            for (int i = 0; i < entries.Count; ++i)
            {
                if (registrationType == entries[i].ComponentType)
                {
                    return i;
                }
            }
            return -1;
        }

        private void EnsureNoDuplicateRegistrationType(Type componentType)
        {
            if (IndexOfRegistration(componentType) != -1)
            {
                throw new InvalidOperationException(string.Format("A component of type '{0}' has already been registered.", componentType.FullName));
            }
        }

        private int EnsureRegistrationExists<TRegistrationType>()
        {
            var registrationIndex = IndexOfRegistration(typeof(TRegistrationType));
            if (registrationIndex == -1)
            {
                throw new InvalidOperationException(string.Format("A component of type '{0}' has not been registered.", typeof(TRegistrationType).FullName));
            }
            return registrationIndex;
        }

        private class RegistrationLocationSelector : IRegistrationLocationSelectionSyntax<TComponent>
        {
            private readonly LazyComponentRegistrationList<TArgument, TComponent> registrations;
            private readonly LazyComponentRegistration newRegistration;

            public RegistrationLocationSelector(LazyComponentRegistrationList<TArgument, TComponent> registrations, LazyComponentRegistration newRegistration)
            {
                this.registrations = registrations;
                this.newRegistration = newRegistration;
            }

            void IRegistrationLocationSelectionSyntax<TComponent>.InsteadOf<TRegistrationType>()
            {
                if (newRegistration.ComponentType != typeof(TRegistrationType))
                {
                    registrations.EnsureNoDuplicateRegistrationType(newRegistration.ComponentType);
                }

                var registrationIndex = registrations.EnsureRegistrationExists<TRegistrationType>();
                registrations.entries[registrationIndex] = newRegistration;
            }

            void IRegistrationLocationSelectionSyntax<TComponent>.After<TRegistrationType>()
            {
                registrations.EnsureNoDuplicateRegistrationType(newRegistration.ComponentType);
                var registrationIndex = registrations.EnsureRegistrationExists<TRegistrationType>();
                registrations.entries.Insert(registrationIndex + 1, newRegistration);
            }

            void IRegistrationLocationSelectionSyntax<TComponent>.Before<TRegistrationType>()
            {
                registrations.EnsureNoDuplicateRegistrationType(newRegistration.ComponentType);
                var registrationIndex = registrations.EnsureRegistrationExists<TRegistrationType>();
                registrations.entries.Insert(registrationIndex, newRegistration);
            }

            void IRegistrationLocationSelectionSyntax<TComponent>.OnBottom()
            {
                registrations.EnsureNoDuplicateRegistrationType(newRegistration.ComponentType);
                registrations.entries.Add(newRegistration);
            }

            void IRegistrationLocationSelectionSyntax<TComponent>.OnTop()
            {
                registrations.EnsureNoDuplicateRegistrationType(newRegistration.ComponentType);
                registrations.entries.Insert(0, newRegistration);
            }
        }

        private class TrackingRegistrationLocationSelector : ITrackingRegistrationLocationSelectionSyntax<TComponent>
        {
            private readonly LazyComponentRegistrationList<TArgument, TComponent> registrations;
            private readonly TrackingLazyComponentRegistration newRegistration;

            public TrackingRegistrationLocationSelector(LazyComponentRegistrationList<TArgument, TComponent> registrations, TrackingLazyComponentRegistration newRegistration)
            {
                this.registrations = registrations;
                this.newRegistration = newRegistration;
            }

            void ITrackingRegistrationLocationSelectionSyntax<TComponent>.InsteadOf<TRegistrationType>()
            {
                if (newRegistration.ComponentType != typeof(TRegistrationType))
                {
                    registrations.EnsureNoDuplicateRegistrationType(newRegistration.ComponentType);
                }

                var registrationIndex = registrations.EnsureRegistrationExists<TRegistrationType>();
                var innerComponentFactory = registrations.entries[registrationIndex].Factory;
                registrations.entries[registrationIndex] = new LazyComponentRegistration(
                    newRegistration.ComponentType,
                    arg => newRegistration.Factory(innerComponentFactory(arg), arg)
                );
            }
        }
    }
}