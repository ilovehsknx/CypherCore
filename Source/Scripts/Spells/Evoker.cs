﻿// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using Framework.Constants;
using Game.Entities;
using Game.Scripting;
using Game.Spells;
using System;
using System.Collections.Generic;

namespace Scripts.Spells.Evoker
{
    struct SpellIds
    {
        public const uint EnergizingFlame = 400006;
        public const uint GlideKnockback = 358736;
        public const uint Hover = 358267;
        public const uint LivingFlame = 361469;
        public const uint LivingFlameDamage = 361500;
        public const uint LivingFlameHeal = 361509;
        public const uint PyreDamage = 357212;
        public const uint SoarRacial = 369536;
    }

    [Script] // 362969 - Azure Strike (blue)
    class spell_evo_azure_strike : SpellScript
    {
        void FilterTargets(List<WorldObject> targets)
        {
            targets.Remove(GetExplTargetUnit());
            targets.RandomResize((uint)GetEffectInfo(0).CalcValue(GetCaster()) - 1);
            targets.Add(GetExplTargetUnit());
        }

        public override void Register()
        {
            OnObjectAreaTargetSelect.Add(new ObjectAreaTargetSelectHandler(FilterTargets, 1, Targets.UnitDestAreaEnemy));
        }
    }

    [Script] // 358733 - Glide (Racial)
    class spell_evo_glide : SpellScript
    {
        public override bool Validate(SpellInfo spellInfo)
        {
            return ValidateSpellInfo(SpellIds.GlideKnockback, SpellIds.Hover, SpellIds.SoarRacial);
        }

        SpellCastResult CheckCast()
        {
            Unit caster = GetCaster();

            if (!caster.IsFalling())
                return SpellCastResult.NotOnGround;

            return SpellCastResult.SpellCastOk;
        }

        void HandleCast()
        {
            Player caster = GetCaster().ToPlayer();
            if (caster == null)
                return;

            caster.CastSpell(caster, SpellIds.GlideKnockback, true);

            caster.GetSpellHistory().StartCooldown(Global.SpellMgr.GetSpellInfo(SpellIds.Hover, GetCastDifficulty()), 0, null, false, TimeSpan.FromMilliseconds(250));
            caster.GetSpellHistory().StartCooldown(Global.SpellMgr.GetSpellInfo(SpellIds.SoarRacial, GetCastDifficulty()), 0, null, false, TimeSpan.FromMilliseconds(250));
        }

        public override void Register()
        {
            OnCheckCast.Add(new CheckCastHandler(CheckCast));
            OnCast.Add(new CastHandler(HandleCast));
        }
    }

    [Script] // 361469 - Living Flame (Red)
    class spell_evo_living_flame : SpellScript
    {
        public override bool Validate(SpellInfo spellInfo)
        {
            return ValidateSpellInfo(SpellIds.LivingFlameDamage, SpellIds.LivingFlameHeal, SpellIds.EnergizingFlame);
        }

        void HandleHitTarget(uint effIndex)
        {
            Unit caster = GetCaster();
            Unit hitUnit = GetHitUnit();
            if (caster.IsFriendlyTo(hitUnit))
                caster.CastSpell(hitUnit, SpellIds.LivingFlameHeal, true);
            else
                caster.CastSpell(hitUnit, SpellIds.LivingFlameDamage, true);
        }

        void HandleLaunchTarget(uint effIndex)
        {
            Unit caster = GetCaster();
            if (caster.IsFriendlyTo(GetHitUnit()))
                return;

            AuraEffect auraEffect = caster.GetAuraEffect(SpellIds.EnergizingFlame, 0);
            if (auraEffect != null)
            {
                int manaCost = GetSpell().GetPowerTypeCostAmount(PowerType.Mana).GetValueOrDefault(0);
                if (manaCost != 0)
                    GetCaster().ModifyPower(PowerType.Mana, MathFunctions.CalculatePct(manaCost, auraEffect.GetAmount()));
            }
        }

        public override void Register()
        {
            OnEffectHitTarget.Add(new EffectHandler(HandleHitTarget, 0, SpellEffectName.Dummy));
            OnEffectLaunchTarget.Add(new EffectHandler(HandleLaunchTarget, 0, SpellEffectName.Dummy));
        }
    }

    [Script] // 393568 - Pyre
    class spell_evo_pyre : SpellScript
    {
        public override bool Validate(SpellInfo spellInfo)
        {
            return ValidateSpellInfo(SpellIds.PyreDamage);
        }

        void HandleDamage(uint effIndex)
        {
            GetCaster().CastSpell(GetHitUnit().GetPosition(), SpellIds.PyreDamage, new CastSpellExtraArgs(true));
        }

        public override void Register()
        {
            OnEffectHitTarget.Add(new EffectHandler(HandleDamage, 0, SpellEffectName.Dummy));
        }
    }
}
