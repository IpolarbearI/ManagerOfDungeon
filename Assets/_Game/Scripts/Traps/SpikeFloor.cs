using System.Collections.Generic;
using UnityEngine;

namespace ManagerOfDungeon
{
    public sealed class SpikeFloor : MonoBehaviour
    {
        readonly Dictionary<CombatUnit, float> _cooldownByHero = new Dictionary<CombatUnit, float>();
        float _radius = CombatRanges.SpikeRadius;
        SpriteRenderer _body;
        Sprite _idle;
        Sprite _active;
        float _flash;

        public void Setup(Vector3 worldPosition)
        {
            transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
            transform.localScale = Vector3.one;
            name = ResourceIds.ObjSpikeFloor;

            _idle = PackAssets.LoadSprite(PackAssets.SpikeIdle);
            _active = PackAssets.LoadSprite(PackAssets.SpikeActive);

            _body = gameObject.GetComponent<SpriteRenderer>();
            if (_body == null)
                _body = gameObject.AddComponent<SpriteRenderer>();
            _body.sprite = _idle;
            PackAssets.ApplySpriteMaterial(_body);
            _body.sortingOrder = 2;
            transform.localScale = Vector3.one * 1.6f;
        }

        public void Tick(IReadOnlyList<CombatUnit> units, float deltaTime)
        {
            if (units == null)
                return;

            if (_flash > 0f)
            {
                _flash -= deltaTime;
                if (_flash <= 0f && _body != null)
                    _body.sprite = _idle != null ? _idle : _body.sprite;
            }

            var keys = new List<CombatUnit>(_cooldownByHero.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                var hero = keys[i];
                if (hero == null)
                {
                    _cooldownByHero.Remove(hero);
                    continue;
                }

                _cooldownByHero[hero] -= deltaTime;
                if (_cooldownByHero[hero] <= 0f)
                    _cooldownByHero.Remove(hero);
            }

            Vector2 center = transform.position;
            for (int i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                if (unit == null || !unit.IsAlive || unit.Team != CombatTeam.Attacker)
                    continue;

                if (Vector2.Distance(center, unit.transform.position) > _radius)
                    continue;

                if (_cooldownByHero.ContainsKey(unit))
                    continue;

                float damage = unit.MaxHealth * CombatRanges.SpikeHeroMaxHealthPercent;
                unit.TakeHit(damage);
                _cooldownByHero[unit] = CombatRanges.SpikeCooldown;
                FlashActive();
            }
        }

        void FlashActive()
        {
            _flash = 0.25f;
            if (_body != null && _active != null)
                _body.sprite = _active;
        }
    }
}
