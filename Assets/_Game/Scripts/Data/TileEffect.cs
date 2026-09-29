using UnityEngine;

namespace ManagerOfDungeon
{
    public abstract class TileEffect : ScriptableObject
    {
        public virtual void OnRaidEnter(in TileContext context) { }

        public virtual void OnRaidStay(in TileContext context) { }

        public virtual void OnRaidExit(in TileContext context) { }
    }
}
