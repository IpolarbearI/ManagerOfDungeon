// GameFonts.cs - central font provider: loads the pack's pixel font from Resources, falls back to built-in.
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>UI font provider. Put a TTF at Resources/Fonts/KenneyPixel to theme all pack UI.</summary>
    public static class GameFonts
    {
        static Font _ui;
        public static Font UI
        {
            get
            {
                if (_ui == null)
                {
                    _ui = Resources.Load<Font>("Fonts/KenneyPixel");
                    if (_ui == null) _ui = Resources.Load<Font>("Fonts/KenneyMini");
                    if (_ui == null) _ui = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                return _ui;
            }
        }
    }
}
