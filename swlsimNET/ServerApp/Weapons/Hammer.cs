﻿using System;
using System.Collections.Generic;
using swlsimNET.ServerApp.Combat;
using swlsimNET.ServerApp.Models;
using swlsimNET.ServerApp.Spells;
using swlsimNET.ServerApp.Spells.Hammer;
using System.Linq;

namespace swlsimNET.ServerApp.Weapons
{
    public class Hammer : Weapon
    {
        private Passive _fastAndFurious;

        private bool _hasLetLoose;
        private bool _hasAnnihilate;
        private bool _enraged50;
        private bool _enraged100;

        private bool _init;

        private double _enragedLockTimeStamp;
        private double _timeSinceEnraged;

        private bool _pneumaticMaul = true;
        private bool _pneumaticAvailable = false;
        private bool _fumingDespoiler = false;
        //private bool _theTenderiser = false;
        private double _pneumaticStamp = -1;

        public bool FastAndFuriousBonus { get; private set; }
        public bool LetLooseBonus { get; private set; }

        public Hammer(WeaponType wtype, WeaponAffix waffix) : base(wtype, waffix)
        {
            _maxGimickResource = 100;
        }
        private readonly List<string> _hammerConsumers = new List<string>
        {
            "Demolish", "DemolishRage", "Eruption"
        };

        public override void PreAttack(IPlayer player, RoundResult rr)
        {
            if (_pneumaticStamp >= player.CurrentTimeSec)
            {
                var demolish = player.Spells.Where(s => s.GetType() == typeof(DemolishRage));
                var eruption = player.Spells.Where(s => s.GetType() == typeof(EruptionRage));

                foreach (var d in demolish)
                {
                    d.PrimaryGimmickCost = 0;
                }
                foreach (var e in eruption)
                {
                    e.PrimaryGimmickCost = 0;
                }

            }
            else
            {
                var demolish = player.Spells.Where(s => s.GetType() == typeof(DemolishRage));
                var eruption = player.Spells.Where(s => s.GetType() == typeof(EruptionRage));

                foreach (var d in demolish)
                {
                    d.PrimaryGimmickCost = 50;
                }
                foreach (var e in eruption)
                {
                    e.PrimaryGimmickCost = 50;
                }
            }
        }

        public override void AfterAttack(IPlayer player, ISpell spell, RoundResult rr)
        {
            // Only on first activation
            if (!_init)
            {
                _init = true;

                _fastAndFurious = player.GetPassive(nameof(FastAndFurious));
                _hasLetLoose = player.HasPassive(nameof(LetLoose));
                _hasAnnihilate = player.HasPassive(nameof(Annihilate));
            }

            var enraged50 = GimmickResource >= 50 && GimmickResource < 100;
            var enraged100 = GimmickResource >= 100;

            // Has any enraged treshold passed since last time
            if (!_enraged50 && enraged50)
            {
                // Passed 50 treshold
                _enragedLockTimeStamp = player.CurrentTimeSec;
            }
            else if(!_enraged100 && _enraged50 && enraged100)
            {
                // Reached 100 treshold
                _enragedLockTimeStamp = player.CurrentTimeSec;
            }

            _enraged50 = enraged50;
            _enraged100 = enraged100;

            if (_fastAndFurious == null)
            {
                _timeSinceEnraged = player.CurrentTimeSec - _enragedLockTimeStamp;
                FastAndFuriousBonus = _timeSinceEnraged < 3.5;
            }
            //Whenever you critically hit with a Hammer ability, you gain a benefical effect which allows you to gain the benefits of the Enrage bonus effects on your abilities without spending 
            //any Rage and without being Enraged.This effect can only occur once every 9 seconds.
            // _pneumaticStamp == player.CurrentTime =+ 9;
            if (_pneumaticMaul)
            {
                var attack = rr.Attacks.FirstOrDefault();
                if (attack != null && attack.IsCrit && !_pneumaticAvailable)
                {
                    _pneumaticAvailable = true;
                    _pneumaticStamp = player.CurrentTimeSec + 9; 
                }

                if (_pneumaticStamp < player.CurrentTimeSec + 9 &&_pneumaticAvailable && spell.GetType() == typeof(DemolishRage) || spell.GetType() == typeof(EruptionRage))
                {
                    var demolish = player.Spells.Where(s => s.GetType() == typeof(DemolishRage));
                    var eruption = player.Spells.Where(s => s.GetType() == typeof(EruptionRage));

                    foreach (var d in demolish)
                    {
                        d.PrimaryGimmickCost = 50;
                    }
                    foreach (var e in eruption)
                    {
                        e.PrimaryGimmickCost = 50;
                    }

                    _pneumaticAvailable = false;
                }
            }
            var spellName = spell.Name;
            if (_fumingDespoiler && spellName != null && !_hammerConsumers.Contains(spellName, StringComparer.CurrentCultureIgnoreCase))
            {
                player.AddBonusAttack(rr, new FumingDespoiler(player));
            }
        }

        public override double GetBonusBaseDamageMultiplier(IPlayer player, ISpell spell, double rageBeforeCast)
        {
            double bonusBaseDamageMultiplier = 0;

            if (LetLooseBonus)
            {
                // if using "Rampage" causes you to become Enraged increase base damage,

                if (spell.GetType() == typeof(Demolish) || spell.GetType() == typeof(DemolishRage))
                {
                    // Demolish: 30 %
                    bonusBaseDamageMultiplier += 0.3;
                    LetLooseBonus = false; // Buff consumed
                }
                else if (_hasAnnihilate &&
                         (spell.GetType() == typeof(Eruption) || spell.GetType() == typeof(EruptionRage)))
                {
                    // Eruption with Annihilate Passive: 20%
                    bonusBaseDamageMultiplier += 0.2;
                    LetLooseBonus = false; // Buff consumed
                }  
            }

            if (FastAndFuriousBonus)
            {
                bonusBaseDamageMultiplier += _fastAndFurious.BaseDamageModifier;
            }

            return bonusBaseDamageMultiplier;
        }

        public override void OnHit(IPlayer player, ISpell spell, double rageBeforeCast)
        {
            // TODO: Save enrage states between rounds instead

            if (_hasLetLoose && spell.GetType() == typeof(Rampage))
            {
                // Enraged status before attack
                var enraged50ba = rageBeforeCast >= 50 && rageBeforeCast < 100;
                var enraged100ba = rageBeforeCast >= 100 && rageBeforeCast > 50;

                // Enraged status after attack
                var enraged50aa = GimmickResource >= 50 && GimmickResource < 100;
                var enraged100aa = GimmickResource >= 100;

                var rampageMadeUsEnraged = false;

                // Has any enraged treshold passed since since last time
                if (!enraged50ba && enraged50aa)
                {
                    // Passed 50 treshold
                    rampageMadeUsEnraged = true;
                }
                else if (!enraged100ba && enraged50ba && enraged100aa)
                {
                    // Reached 100 treshold
                    rampageMadeUsEnraged = true;
                }

                // If Rampage attack made us enraged
                if (rampageMadeUsEnraged)
                {
                    // Consumable buff to Hammer that are used on next Demolish or Eruption
                    LetLooseBonus = true;
                }
            }
        }
        public class FumingDespoiler : Spell
        {
            public FumingDespoiler(IPlayer player)
            {
                WeaponType = WeaponType.Hammer;
                SpellType = SpellType.Gimmick;
                BaseDamage = 0.825;
            }
        }
    }
}

