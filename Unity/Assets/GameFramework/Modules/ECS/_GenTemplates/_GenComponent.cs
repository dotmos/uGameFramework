
using System ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*name:using*/ /*endname*/

/// <summary>
/*name:componentComment*/ /*endname*/
/// </summary>
[System.Serializable]
public partial class /*name:ComponentName*/GenTemplateComponent/*endname*/ : ECS.Component {
/*block:enum*/
    public enum /*name:enumName*/State/*endname*/ : int
    {
 /*block:entry*/        /*name:entryName*/state1/*endname*/ /*name:entryNumber*//*endname*/,
 /*endblock:entry*/
 /*block:rip*/        state2 = 1,
        state3 = 2,
 /*endblock:rip*/
    }
/*endblock:enum*/
 
/*block:field*/
/// <summary>
/*name:comment*//*endname*/    
/// </summary>
    /*name:attributes*//*endname*/
    public /*name:type*/State/*endname*/ /*name:name*/state/*endname*/ /*name:value*/= State.state1/*endname*/;
/*endblock:field*/
}


/*block:rip*/
[System.Serializable]
public class GenTemplateComponent2 : ECS.Component
{
    public float time;
}
/*endblock:rip*/