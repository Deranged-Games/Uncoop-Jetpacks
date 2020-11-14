using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.Entities;
using Sandbox.Game.Gui;
using Sandbox.Game;
using VRage.Common.Utils;
using VRageMath;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using VRage.Utils;
using VRage.Library.Utils;

using System.Text.RegularExpressions;
using Ingame = Sandbox.ModAPI.Ingame;

namespace Dondelium.Dampeners{
  [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
  public class DampenerCharacterDisable : MySessionComponentBase{
    private static HashSet<IMyEntity> ents = new HashSet<IMyEntity>();
    public static Dictionary<long, charController> ccDictionary = new Dictionary<long, charController>();

    private bool init = false;
    public void Init(){}
    internal static Action UpdateHook;
    
    public override void UpdateAfterSimulation(){
      if(!init){
        init = true;
        MyAPIGateway.Entities.GetEntities(ents, delegate (IMyEntity e) {
          Entities_OnEntityAdd(e);
          return false;
        });
        MyAPIGateway.Entities.OnEntityAdd += Entities_OnEntityAdd;
        MyAPIGateway.Entities.OnEntityRemove += Entities_OnEntityRemove;
      }
      if(UpdateHook != null) UpdateHook();
    }

    private void Entities_OnEntityAdd(IMyEntity obj){
      if(obj == null) return;
      if(obj is IMyCharacter){
        charController cc;
        if (ccDictionary.TryGetValue(obj.EntityId, out cc)){
          cc.Close();
          ccDictionary.Remove(obj.EntityId);
        }
        cc = new charController(obj);
        ccDictionary.Add(obj.EntityId, cc);
      }
    }

    private void Entities_OnEntityRemove(IMyEntity obj){
      if(obj == null) return;
      if(obj is IMyCharacter){
        charController cc;
        if(ccDictionary.TryGetValue(obj.EntityId, out cc)){
          cc.Close();
          ccDictionary.Remove(obj.EntityId);
        }
      }
    }

    protected override void UnloadData(){
      MyAPIGateway.Entities.OnEntityAdd -= Entities_OnEntityAdd;
      MyAPIGateway.Entities.OnEntityRemove -= Entities_OnEntityRemove;
      ents.Clear();
    }
  }

  public class charController{
    private IMyCharacter character;
    
    public charController(IMyEntity obj){
      character = obj as IMyCharacter;
      DampenerCharacterDisable.UpdateHook += Update;
    }
    
    internal void Close(){
      if(DampenerCharacterDisable.UpdateHook != null)
        DampenerCharacterDisable.UpdateHook -= Update;
    }

    public void Update(){
      var control = character as Sandbox.Game.Entities.IMyControllableEntity;
      if (control.RelativeDampeningEntity != null) return;

      if (character.EnabledDamping) character.SwitchDamping();
    }
  }
}
