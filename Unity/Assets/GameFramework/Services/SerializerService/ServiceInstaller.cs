using UnityEngine;
using System.Collections;
using Zenject;

namespace Service.Serializer{
    public class ServiceInstaller : Installer<ServiceInstaller>{
        
    	public override void InstallBindings()
        {
            //Container.Bind<ISerializerService>().ToSingle<LitJsonSerializerService>();
            Container.Bind<ISerializerService>().To<JsonNetSerializerService>().AsSingle();

            // Due to zenject update 6 this usage was not supported anymore.
          /*  Container.Bind<ISerializerService>().WithId(SerializerID.LitJson).To<LitJsonSerializerService>().AsSingle();
            Container.Bind<ISerializerService>().WithId(SerializerID.UnityJson).To<UnityJsonSerializerService>().AsSingle();
            Container.Bind<ISerializerService>().WithId(SerializerID.DataContract).To<DataContractSerializerService>().AsSingle();
            Container.Bind<ISerializerService>().WithId(SerializerID.JsonDataContract).To<JsonDataContractSerializerService>().AsSingle();
            Container.Bind<ISerializerService>().WithId(SerializerID.Jil).To<JilSerializerService>().AsSingle();
            Container.Bind<ISerializerService>().WithId(SerializerID.JsonNet).To<JsonNetSerializerService>().AsSingle();*/
        //    Container.BindAllInterfaces<ISerializerService>().AsSingle();
           // Container.BindInterfacesTo<ISerializerService>().AsSingle();

        }
    }
}