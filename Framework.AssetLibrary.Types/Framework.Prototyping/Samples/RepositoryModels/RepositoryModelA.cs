using System.Collections.Generic;

namespace Framework.Sample.Mapping.Samples.RepositoryModels
{
    public class RepositoryModelA
    {  
        public RepositoryModelC[] ModelCArray { get; set; }

        public List<RepositoryModelC> ModelCList { get; set; }

        public IEnumerable<RepositoryModelC> ModelCIEnumerable { get; set; }

        public ICollection<RepositoryModelC> ModelCCollection { get; set; }

        public IList<RepositoryModelC> ModelCIList { get; set; }        
        
    }
}