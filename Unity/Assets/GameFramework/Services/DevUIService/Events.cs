///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using UniRx;
using System.Runtime.Serialization;

namespace Service.DevUIService {
    
    public class Events {
        
        [DataContract(IsReference = true)]
        public class NewUIElement {
           [DataMember] public DevUIElement elem;
           [DataMember] public DevUIView view;
           [DataMember] public bool inEditMode;

        }

        [DataContract(IsReference = true)]
        public class WriteToScriptingConsole {
           [DataMember] public string text;

        }

        [DataContract(IsReference = true)]
        public class ScriptingConsoleOpened {

        }

        [DataContract(IsReference = true)]
        public class ScriptingConsoleClosed {

        }

        [DataContract(IsReference = true)]
        public class UIViewRenamed {
           [DataMember] public string from;
           [DataMember] public string to;
           [DataMember] public DevUIView view;

        }

        [DataContract(IsReference = true)]
        public class PickedEntity {
           [DataMember] public ECS.UID entity;

        }
        
    }
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
