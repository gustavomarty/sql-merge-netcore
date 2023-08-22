namespace SqlComplexOperations.Models
{
    public class OutputModel
    {
        public int Inserted { get; set; } = 0;
        public int Updated { get; set; } = 0;
        public int Deleted { get; set; } = 0;

        public int Total { 
            get {
                return Inserted + Updated + Deleted;    
            } 
        }

    }
}
