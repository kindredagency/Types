using System.Collections.Generic;

namespace Framework.Sample.Mapping.Samples.BaseModels
{
    public class BaseModelA
    {
        public BaseModelC[] ModelCArray { get; set; }

        public List<BaseModelC> ModelCList { get; set; }

        public IEnumerable<BaseModelC> ModelCIEnumerable { get; set; }

        public ICollection<BaseModelC> ModelCCollection { get; set; }

        public IList<BaseModelC> ModelCIList { get; set; }
        
    }
}