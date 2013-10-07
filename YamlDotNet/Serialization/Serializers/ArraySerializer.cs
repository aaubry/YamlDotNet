using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization.Descriptors;

namespace YamlDotNet.Serialization.Serializers
{
    internal class ArraySerializer : ObjectSerializerBase
    {
        public ArraySerializer(SerializerSettings settings) : base(settings)
        {
        }

        protected override bool CheckIsSequence(ITypeDescriptor typeDescriptor)
        {
            // An array is necessary a sequence.
            return true;
        }

        protected override object ReadItems<TStart, TEnd>(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
        {
            var arrayDescriptor = (ArrayDescriptor) typeDescriptor;

            var listType = typeof (List<>).MakeGenericType(arrayDescriptor.ElementType);
            var list = Activator.CreateInstance(listType);
            base.ReadItems<TStart, TEnd>(context, list, typeDescriptor);

            return listType.GetMethod("ToArray").Invoke(list, null);
        }

        protected override void ReadItem(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
        {
            var arrayDescriptor = (ArrayDescriptor) typeDescriptor;
            ((IList) thisObject).Add(context.ReadYaml(null, arrayDescriptor.ElementType));
        }

        public override void WriteItems(SerializerContext context, object thisObject, ITypeDescriptor typeDescriptor)
        {
            throw new NotImplementedException();
        }
    }
}