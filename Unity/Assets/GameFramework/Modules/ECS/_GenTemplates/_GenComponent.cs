
using System;
using System.Collections;
using System.Collections.Generic;
using ECS;
using FlatBuffers;
using Service.Serializer;
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
    /*block:rip*/
    public string testName="f95";
    public UID testUID = new UID(1895);
    public float testNumber = 18.95f;
    public List<UID> testListUID = new List<UID>();
    public List<int> testListPrimitive = new List<int>();
    public Dictionary<string, int> testDict = new Dictionary<string, int>();
    /*endblock:rip*/

    protected override void OnConstruct() {
        base.OnConstruct();

/*block:newInstance*/        this./*name:name*/state/*endname*/ = new /*name:type*/State()/*endname*/;
/*endblock:newInstance*/

    }

    /// <summary>
    /// Copy values from other component. Shallow copy.
    /// </summary>
    /// <param name="target"></param>
    public override void CopyValues(IComponent target) {
        /*name:ComponentName*/GenTemplateComponent/*endname*/ component = (/*name:ComponentName*/GenTemplateComponent/*endname*/)target;
/*block:copyField*/        this./*name:name*/state/*endname*/ = component./*name:name*/state/*endname*/;
/*endblock:copyField*/
/*block:shallowCopy*/        this./*name:name*/state/*endname*/ = new /*name:type*/State()/*endname*/;
/*endblock:shallowCopy*/

    }

    public override IComponent Clone() {
        var clone = new  /*name:ComponentName*/GenTemplateComponent/*endname*/();
        clone.CopyValues(this);
        return clone;
    }

    #region serialization
    public new int Serialize(FlatBuffers.FlatBufferBuilder builder) {
        var componentBase = base.Serialize(builder);
/*block:s_enum*/        var /*name|fu,pre#s:name*/sState/*endname*/ = (byte)/*name:name*/state/*endname*/;
/*endblock:s_enum*/
/*block:s_list_primitive*/        var /*name|fu,pre#s:name*/sTestListPrimitive/*endname*/ = FlatbufferSerializer.CreateList</*name:innertype*/int/*endname*/>(builder,/*name:name*/testListPrimitive/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestListPrimitiveVector/*endname*/) ;
/*endblock:s_list_primitive*/

        Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|pre#StartFB:ComponentName*/StartFBGenTemplateComponent/*endname*/(builder);
/*block:s2_default*/        Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Add:name*/AddState/*endname*/(builder,/*name|fu,pre#s:name*/sState/*endname*/);
/*endblock:s2_default*/        
/*block:s2_primitive*/        Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Add:name*/AddState/*endname*/(builder,/*name:name*/sState/*endname*/);
/*endblock:s2_primitive*/
/*block:s2_list*/        if (/*name:name*/sTestListPrimitive/*endname*/!= null) Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Add:name*/AddTestListPrimitive/*endname*/(builder,(VectorOffset)/*name|fu,pre#s:name*/sTestListPrimitive/*endname*/);
/*endblock:s2_list*/
return Serial.FBGenTemplateComponent.EndFBGenTemplateComponent(builder).Value;
    }

    public void Deserialize(Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/ input) {
        base.Deserialize(input.BaseComponent);
/*block:d_enum*/        /*name:name*/state/*endname*/ = (/*name:type*/State/*endname*/)input./*name:name*/State/*endname*/; // string
/*endblock:d_enum*/
    }

    public new void Deserialize(FlatBuffers.ByteBuffer buf) {
        var fbSettlerDataComponent = Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|pre#GetRootAsFB:ComponentName*/GetRootAsFBGenTemplateComponent/*endname*/(buf);
        Deserialize(fbSettlerDataComponent);
    }
    #endregion

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