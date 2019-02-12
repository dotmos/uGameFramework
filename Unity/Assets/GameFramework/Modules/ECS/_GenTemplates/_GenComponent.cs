
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
    public enum /*name:enumName*/State/*endname*/ : int {
        /*block:entry*//*block:comment*/
                       /// <summary>
        /*name:comment*//*endname*/
                        /// </summary>/*endblock:comment*/
        /*name:entryName*/
        state1/*endname*/ /*name:entryNumber*//*endname*/,
        /*endblock:entry*/
        /*block:rip*/
        state2 = 1,
        state3 = 2,
        /*endblock:rip*/
    }
    /*endblock:enum*/

    /*block:modelClass*/
    [System.Serializable]
    public partial class /*name:className*/SomeModel/*endname*//*name:inheritance*//*endname*/ {
        /*block:field*/
        /*name:scope*/public/*endname*/ /*name:type*/string/*endname*/ /*name:name*/name/*endname*/ /*block:valueBlock*/= /*name:value*/"value"/*endname*//*endblock:valueBlock*/;
        /*endblock:field*/
        /*name:classSerialization*/
        public void Deserialize(object incoming) {
            throw new System.NotImplementedException();
        }

        public void Deserialize(ByteBuffer buf) {
            throw new System.NotImplementedException();
        }

        public int Serialize(FlatBufferBuilder builder) {
            throw new System.NotImplementedException();
        }
        /*endname*/
    }

    /*endblock:modelClass*/

    /*block:field*/
    /// <summary>
    /*name:comment*//*endname*/
                    /// </summary>
    /*name:attributes*//*endname*/
    public /*name:type*/State/*endname*/ /*name:name*/state/*endname*/ /*name:value*/= State.state1/*endname*/;
    /*endblock:field*/
    /*block:rip*/
    public string testName = "f95";
    public UID? testUID = new UID(1895);
    public float testNumber = 18.95f;

    public class FreeTypeTestObject : IPocoBase<FreeTypeTestObject> {
        private string data;
        public FreeTypeTestObject() {
            data = "bla";
        }
        public void ShallowCopy(FreeTypeTestObject from) {
            this.data = from.data;
        }
    }

    public FreeTypeTestObject freeTypeTest = null;

    public System.Collections.Generic.List<UID> testListUID = new System.Collections.Generic.List<UID>();
    public System.Collections.Generic.List<int> testListPrimitive = new System.Collections.Generic.List<int>();
    public System.Collections.Generic.Dictionary<string, int> testDict = new System.Collections.Generic.Dictionary<string, int>();
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
/*block:shallowCopyList*/        if (this./*name:name*/testListUID/*endname*/== null) {
                this./*name:name*/testListUID/*endname*/ = new /*name:type*/System.Collections.Generic.List<UID>/*endname*/(component./*name:name*/testListUID/*endname*/);
            } else {
                this./*name:name*/testListUID/*endname*/.Clear();
                this./*name:name*/testListUID/*endname*/.AddRange(component./*name:name*/testListUID/*endname*/);
            }
/*endblock:shallowCopy*/
/*block:shallowCopyDict*/       if (this./*name:name*/testDict/*endname*/== null) {
                this./*name:name*/testDict/*endname*/ = new /*name:type*/System.Collections.Generic.Dictionary<string, int>/*endname*/(component./*name:name*/testDict/*endname*/);
            } else {
                this./*name:name*/testDict/*endname*/.Clear();
                foreach (var pair in component./*name:name*/testDict/*endname*/) {
                    this./*name:name*/testDict/*endname*/[pair.Key] = pair.Value;
                }
            }
/*endblock:shallowCopyDict*/
/*block:shallowCopyFreeType*/       if (this./*name:name*/freeTypeTest/*endname*/== null) {
                this./*name:name*/freeTypeTest/*endname*/ = new /*name:type*/FreeTypeTestObject/*endname*/();
            }
            this./*name:name*/freeTypeTest/*endname*/.ShallowCopy(component./*name:name*/freeTypeTest/*endname*/);
/*endblock:shallowCopyFreeType*/
    }

    public override IComponent Clone() {
        var clone = new  /*name:ComponentName*/GenTemplateComponent/*endname*/();
        clone.CopyValues(this);
        return clone;
    }
/*block:serialization*/
    #region serialization
    public override int Serialize(FlatBuffers.FlatBufferBuilder builder) {
/*block:inheritanceSer*/        var baseData = base.Serialize(builder);
/*endblock:inheritanceSer*/
/*block:s_enum*/        var /*name|fu,pre#s:name*/sState/*endname*/ = (byte)/*name:name*/state/*endname*/;
/*endblock:s_enum*/
/*block:s_string*/      var /*name|fu,pre#s:name*/sTestName/*endname*/ = (StringOffset)FlatbufferSerializer.GetOrCreateSerialize(builder,/*name:name*/testName/*endname*/) ;
/*endblock:s_string*/
/*block:s_nonprim*/        var /*name|fu,pre#s:name*/sTestUID/*endname*/ = new Offset<Serial./*name|pre#FB:type*/FBUID/*endname*/>((int)FlatbufferSerializer.GetOrCreateSerialize(builder,/*name:name*/testUID/*endname*/)) ;
/*endblock:s_nonprim*/
/*block:s_list_primitive*/        var /*name|fu,pre#s:name*/sTestListPrimitive/*endname*/ = FlatbufferSerializer.CreateList</*name:innertype*/int/*endname*/>(builder,/*name:name*/testListPrimitive/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestListPrimitiveVector/*endname*/) ;
/*endblock:s_list_primitive*/
/*block:s_list_nonprim*/        var /*name|fu,pre#s:name*/sTestListUID/*endname*/ = FlatbufferSerializer.CreateList</*name:innertype*/UID/*endname*/,Serial./*name|pre#FB:innertype*/FBUID/*endname*/>(builder,/*name:name*/testListUID/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestListUIDVector/*endname*/) ;
/*endblock:s_list_nonprim*/
        Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|pre#StartFB:ComponentName*/StartFBGenTemplateComponent/*endname*/(builder);
/*block:s2_default*/        Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Add:name*/AddState/*endname*/(builder,/*name|fu,pre#s:name*/sState/*endname*/);
/*endblock:s2_default*/        
/*block:s2_primitive*/        Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Add:name*/AddState/*endname*/(builder,/*name:name*/sState/*endname*/);
/*endblock:s2_primitive*/
/*block:s2_list*/        if (/*name:name*/testListPrimitive/*endname*/!= null) Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Add:name*/AddTestListPrimitive/*endname*/(builder,(VectorOffset)/*name|fu,pre#s:name*/sTestListPrimitive/*endname*/);
/*endblock:s2_list*/
/*block:inheritanceSer2*/        Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/.AddBaseData(builder,new Offset<Serial./*name:basetype*/FBComponent/*endname*/>(baseData) );
/*endblock:inheritanceSer2*/
return Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|pre#EndFB:ComponentName*/EndFBGenTemplateComponent/*endname*/(builder).Value;
    }
     
    public override void Deserialize(object data) {
        var input = (Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/)data;
/*block:inheritance_deser*/        base.Deserialize(input.BaseData);
/*endblock:inheritance_deser*/
/*block:d_default*/        /*name:name*/state/*endname*/ = (/*name:type*/State/*endname*/)input./*name|fu:name*/State/*endname*/; // string
/*endblock:d_default*/
/*block:d_nonprim*/     /*name:name*/testUID/*endname*/ = FlatbufferSerializer.GetOrCreateDeserialize</*name:type*/UID/*endname*/>(input./*name|fu:name*/TestUID/*endname*/);
/*endblock:d_nonprim*/
/*block:d_prim_list*/        /*name:name*/testListPrimitive/*endname*/ = FlatbufferSerializer.DeserializeList</*name:innertype:*/int/*endname*/>(input./*name|fu,post#BufferPosition:name*/TestListPrimitiveBufferPosition/*endname*/,input./*name|fu,pre#Get,post#Array:name*/GetTestListPrimitiveArray/*endname*/);
/*endblock:d_prim_list*/
/*block:d_nonprim_list*/     {
            var tempList = new System.Collections.Generic.List<object>(); // first create List<object> of all results and then pass this to the Create-method. Didn't find a better way,yet Generics with T? do not work for interfaces
            for (int i=0;i<input./*name|fu,post#Length:name*/TestListUIDLength/*endname*/; i++) tempList.Add(input./*name|fu:name*/TestListUID/*endname*/(i));
            /*name:name*/testListUID/*endname*/ = FlatbufferSerializer.DeserializeList</*name:innertype*/UID/*endname*/,Serial./*name|pre#FB:innertype*/FBUID/*endname*/>(input./*name|fu,post#BufferPosition:name*/TestListUIDBufferPosition/*endname*/, input./*name|fu,post#Length:name*/TestListUIDLength/*endname*/,tempList);
        }
/*endblock:d_nonprim_list*/ 
    } 

    public override void Deserialize(FlatBuffers.ByteBuffer buf) {
        var fbSettlerDataComponent = Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|pre#GetRootAsFB:ComponentName*/GetRootAsFBGenTemplateComponent/*endname*/(buf);
        Deserialize(fbSettlerDataComponent);
    }
    #endregion
/*endblock:serialization*/
}


/*block:rip*/
[System.Serializable]
public class GenTemplateComponent2 : ECS.Component
{
    public float time;

    public override IComponent Clone() {
        throw new System.NotImplementedException();
    }

    public override void CopyValues(IComponent target) {
        throw new System.NotImplementedException();
    }
}
/*endblock:rip*/