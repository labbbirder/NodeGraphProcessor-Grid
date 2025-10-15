using System;

namespace BBBirder.Instructions
{
    internal struct RuntimeArgument
    {
        public object reference;
        public int subkey;
        public Type parameterType;

        public T GetValue<T>()
        {
            if (parameterType.IsValueType)
            {
                if (reference is ColumnBuffer<T> buffer)
                {
                    return buffer.GetValueOrDefaultByToken(subkey);
                }
                else
                {
                    return (reference as IColumnBuffer).GetValueOrDefaultByToken<T>(subkey);
                }
            }
            else
            {
                if (reference is null)
                {
                    return default;
                }
                else if (reference is T t)
                {
                    return t;
                }
                else
                {
                    RuntimeConverter.TryConvert<T>(reference, out var targetValue);
                    return targetValue;
                }
            }
        }
    }
}
