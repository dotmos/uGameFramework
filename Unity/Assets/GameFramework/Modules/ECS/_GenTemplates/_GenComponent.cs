
using System;
using System.Collections;
using System.Collections.Generic;
using ECS;
/*name:using*/ /*endname*/

/// <summary>
/*name:componentComment*/ /*endname*/
                          /// </summary>
[System.Serializable]
public partial class /*name:ComponentName*/GenTemplateComponent/*endname*/ : ECS.Component {
/*block:enum*/
/// <summary>
/*name:comment*//*endname*/
/// </summary>
    public enum /*name:enumName*/State/*endname*/ : int
    {
        /*block:entry*//*block:comment*/
/// <summary>
/*name:comment*//*endname*/
/// </summary>/*endblock:comment*/
        /*name:entryName*/
        state1/*endname*/ /*name:entryNumber*//*endname*/,
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

    /// <summary>
    /// Copy values from other component. Shallow copy.
    /// </summary>
    /// <param name="target"></param>
    public override void CopyValues(ECS.IComponent otherComponent) {
        /*name:ComponentName*/GenTemplateComponent/*endname*/ component = (/*name:ComponentName*/GenTemplateComponent/*endname*/)otherComponent;
/*block:copyField*/        this./*name:name*/state/*endname*/ = component./*name:name*/state/*endname*/;
/*endblock:copyField*/
    }

    public override IComponent Clone() {
        var clone = new  /*name:ComponentName*/GenTemplateComponent/*endname*/();
        clone.CopyValues(this);
        return clone;
    }
}


/*block:rip*/
[System.Serializable]
public class GenTemplateComponent2 : ECS.Component
{
    public float time;

    public override IComponent Clone() {
        throw new NotImplementedException();
    }

    public override void CopyValues(IComponent target) {
        throw new NotImplementedException();
    }
}
/*endblock:rip*/