using Framework.AssetLibrary.Types.Mapping;
using Framework.Sample.Mapping.Samples.BaseModels;
using Framework.Sample.Mapping.Samples.RepositoryModels;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Framework.AssetLibrary.Types;

namespace Framework.Sample.Mapping.Samples.Repositories
{
    public class SampleRepository
    {
        private IMapperContext mapper;
        private BaseModelA baseModelA;

        public SampleRepository(int numberOfObjects)
        {
            //Make the mock objects and fill them with data.
            baseModelA = new BaseModelA();

            List<BaseModelC> baseModelCList = new List<BaseModelC>();

            for (int count = 0; count < numberOfObjects; count++)
            {
                BaseModelC baseModelC = new BaseModelC();
                baseModelC.ModelCProprtyA = Guid.NewGuid().ToString();
                baseModelC.ModelA = baseModelA;

                baseModelC.ModelB = new BaseModelB();
                baseModelC.ModelB.ModelBPropertyA = "PropertyAValue";
                baseModelC.ModelB.ModelC = baseModelC;

                baseModelCList.Add(baseModelC);
            }

            var list = baseModelCList.ToList();

            //Assign the data to different types of lists for testing purposes.
            baseModelA.ModelCArray = baseModelCList.ToArray();
            baseModelA.ModelCList = list;
            baseModelA.ModelCIEnumerable = baseModelCList.AsEnumerable();
            baseModelA.ModelCCollection = list;
            baseModelA.ModelCIList = list;
          

            //Now define the mapping rules
            mapper = new MapperFactory().GetContext();

            mapper.AddMap<BaseModelA, RepositoryModelA>();
            mapper.AddMap<BaseModelB, RepositoryModelB>();
            mapper.AddMap<BaseModelC, RepositoryModelC>();

            //Define the conversion hierarchy
            mapper.AddHierarchy<BaseModelA>().Include<BaseModelC>();
            mapper.AddHierarchy<BaseModelC>().Include<BaseModelA>();
            mapper.AddHierarchy<BaseModelC>().Include<BaseModelB>();
            mapper.AddHierarchy<BaseModelB>().Include<BaseModelC>();
        }

        public void ImplicitMapping()
        {
            //Now to translate it to repository model structure.
            RepositoryModelA repositoryModelResult = mapper.Map<BaseModelA, RepositoryModelA>(baseModelA);
        }
      
    }
}