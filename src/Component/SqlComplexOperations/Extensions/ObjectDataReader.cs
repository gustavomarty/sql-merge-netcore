using System.Collections;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace SqlComplexOperations.Extensions
{
    [ExcludeFromCodeCoverage]
    internal class ObjectDataReader<T> : DbDataReader
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly IDictionary<string, int> _propToOrdinalTable = new Dictionary<string, int>();
        private readonly IDictionary<int, string> _ordinalToPropTable = new Dictionary<int, string>();
        private Func<T, object>[] _getPropValueFunc;

        internal ObjectDataReader(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
            Initialize();
        }

        private void Initialize()
        {
            var properties = typeof(T).GetProperties();
            properties = properties.Where(x => !x.GetGetMethod()?.IsVirtual ?? false).ToArray();

            _getPropValueFunc = new Func<T, object>[properties.Length];

            var ordinal = 0;
            foreach (var name in properties.Select(x => x.Name))
            {
                var propertyName = name;
                _propToOrdinalTable.Add(propertyName, ordinal);
                _ordinalToPropTable.Add(ordinal, propertyName);

                var parameterExpression = Expression.Parameter(typeof(T), "name");
                var func = (Func<T, object>)Expression.Lambda(
                    Expression.Convert(
                        Expression.Property(parameterExpression, propertyName),
                        typeof(object)),
                        parameterExpression)
                    .Compile();

                _getPropValueFunc[ordinal] = func;

                ordinal++;
            }
        }

        public override bool Read()
        {
            return _enumerator.MoveNext();
        }

        public override int GetOrdinal(string name)
        {
            return _propToOrdinalTable.TryGetValue(name, out var ordinal) ? ordinal : -1;
        }

        public override bool IsDBNull(int ordinal)
        {
            return GetValue(ordinal) == null;
        }

        public override object GetValue(int ordinal)
        {
            var func = _getPropValueFunc[ordinal];
            return func(_enumerator.Current);
        }

        public override int GetValues(object[] values)
        {
            var index = 0;
            try
            {
                var max = Math.Min(values.Length, FieldCount);
                for (; index < max; index++)
                {
                    values[index] = IsDBNull(index) ? DBNull.Value : GetValue(index);
                }

                return max;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error in {typeof(T).Name}.GetValues, column index {index}, property {_ordinalToPropTable[index]}  ", e);
            }
        }

        public override object this[int ordinal] => GetValue(ordinal);

        public override object this[string name] => GetValue(GetOrdinal(name));

        public override int Depth => 0;

        public override int FieldCount => _ordinalToPropTable.Count;

        public override bool HasRows => true;

        public override bool IsClosed => _enumerator != null;

        public override bool GetBoolean(int ordinal)
        {
            return (bool)GetValue(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            return (byte)GetValue(ordinal);
        }

        public override char GetChar(int ordinal)
        {
            return (char)GetValue(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return (DateTime)GetValue(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return (decimal)GetValue(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return (double)GetValue(ordinal);
        }

        public override Type GetFieldType(int ordinal)
        {
            var value = GetValue(ordinal);
            return value == null ? typeof(object) : value.GetType();
        }

        public override float GetFloat(int ordinal)
        {
            return (float)GetValue(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            return (Guid)GetValue(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            return (short)GetValue(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            return (int)GetValue(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            return (long)GetValue(ordinal);
        }

        public override string GetName(int ordinal)
        {
            return _ordinalToPropTable.TryGetValue(ordinal, out var name) ? name : null;
        }

        public override string GetString(int ordinal)
        {
            return (string)GetValue(ordinal);
        }

        public override bool NextResult()
        {
            return false;
        }

        #region NotImplemented

        public override int RecordsAffected => -1;

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion NotImplemented
    }
}
