namespace Service.CodeCompiler {
    public static class ReferencedAssemblies{

        //THis is a mess, most stuff is not working for some reason. Fix this, then fix ICodeCompilerService implementations to NOT use full set of assemblies anymore
        public static string[] assemblies = {
            //Scripts
            "Assembly-CSharp",

            //UnityEngine
            "UnityEngine",
            "UnityEngine.Networking",
            "UnityEngine.UI",
            //Not working
//            "UnityEngine.Audio",
//            "UnityEngine.Events",
//            "UnityEngine.EventSystems",
//            "UnityEngine.Rendering",
//            "UnityEngine.SceneManagement",
//            "UnityEngine.Scripting",
//            "UnityEngine.Serialization",
//            "UnityEngine.Sprites",

            //Zenject
            "Zenject",
            "Zenject.Commands"

            //UniRx
//            "UniRx",
//            "UniRx.Operators",
//            "UniRx.Triggers",

            //System
//            "System",
//            "System.Core",
//            "System.Data",
//            "System.Data.DataSetExtensions"
//            "System.Collections",
//            "System.Collections.Generic"

        };
    }
}
