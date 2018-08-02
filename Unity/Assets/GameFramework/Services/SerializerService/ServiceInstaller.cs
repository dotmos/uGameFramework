using UnityEngine;
using System.Collections;
using Zenject;

namespace Service.Serializer{
    public class ServiceInstaller : Installer<ServiceInstaller>{
        
    	public override void InstallBindings()
        {
            Container.Bind<JsonNetSerializerService>().AsSingle();

            //Container.Bind<ISerializerService>().ToSingle<LitJsonSerializerService>();
            Container.Bind<ISerializerService>().To<JsonNetSerializerService>().FromResolve();

            // Due to zenject update 6 this usage was not supported anymore.
            Container.Bind<LitJsonSerializerService>().AsSingle();
            Container.Bind<UnityJsonSerializerService>().AsSingle();
            Container.Bind<DataContractSerializerService>().AsSingle();
            Container.Bind<JsonDataContractSerializerService>().AsSingle();
            Container.Bind<JilSerializerService>().AsSingle();

            Container.Bind<ISerializerService>().WithId(SerializerID.LitJson).To<LitJsonSerializerService>().FromResolve();
            Container.Bind<ISerializerService>().WithId(SerializerID.UnityJson).To<UnityJsonSerializerService>().FromResolve();
            Container.Bind<ISerializerService>().WithId(SerializerID.DataContract).To<DataContractSerializerService>().FromResolve();
            Container.Bind<ISerializerService>().WithId(SerializerID.JsonDataContract).To<JsonDataContractSerializerService>().FromResolve();
            Container.Bind<ISerializerService>().WithId(SerializerID.Jil).To<JilSerializerService>().FromResolve();
            Container.Bind<ISerializerService>().WithId(SerializerID.JsonNet).To<JsonNetSerializerService>().FromResolve();
        //    Container.BindAllInterfaces<ISerializerService>().AsSingle();
           // Container.BindInterfacesTo<ISerializerService>().AsSingle();

        }
    }
}