using SqlComplexOperations.Models.Enumerators;

namespace SqlComplexOperations.Models.Output
{
    public class OutputModelComplete<T> : OutputModelSimple
        where T : class
    {
        public List<OutputDataComplete<T>> Data { get; set; } = new();
    }

    public class OutputDataComplete<T>
        where T : class
    {
        public OutputAction Action { get; set; }
        public T? InsertedData { get; set; }
        public T? DeletedData { get; set; }
    }
}
