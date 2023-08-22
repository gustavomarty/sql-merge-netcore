using SqlComplexOperations.Models.Enumerators;

namespace SqlComplexOperations.Models
{
    public class OutputModelWithData<T> : OutputModel
        where T : class
    {
        public List<OutputData<T>> Data { get; set; } = new();
    }

    public class OutputData<T>
        where T : class
    {
        public OutputAction Action { get; set; }
        public T? InsertedData { get; set; }
        public T? DeletedData { get; set; }
    }
}
