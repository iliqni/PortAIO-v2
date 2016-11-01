using EloBuddy; 
 using LeagueSharp.Common; 
 namespace Flowers_ADC_Series.Pluging
{
    using Common;
    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using Color = System.Drawing.Color;
    using Orbwalking = Orbwalking;
    using static Common.Common;

    internal class KogMaw : Program
    {
        private new readonly Menu Menu = Championmenu;

        public KogMaw()
        {
            Q = new Spell(SpellSlot.Q, 980f);
            W = new Spell(SpellSlot.W, Me.AttackRange);
            E = new Spell(SpellSlot.E, 1200f);
            R = new Spell(SpellSlot.R, 1800f);

            Q.SetSkillshot(0.25f, 50f, 2000f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1.2f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            var ComboMenu = Menu.AddSubMenu(new Menu("Combo", "Combo"));
            {
                ComboMenu.AddItem(new MenuItem("ComboQ", "Use Q", true).SetValue(true));
                ComboMenu.AddItem(new MenuItem("ComboW", "Use W", true).SetValue(true));
                ComboMenu.AddItem(new MenuItem("ComboE", "Use E", true).SetValue(true));
                ComboMenu.AddItem(new MenuItem("ComboR", "Use R", true).SetValue(true));
                ComboMenu.AddItem(
                    new MenuItem("ComboRLimit", "Use R|Limit Stack >= x", true).SetValue(new Slider(3, 0, 10)));
            }

            var HarassMenu = Menu.AddSubMenu(new Menu("Harass", "Harass"));
            {
                HarassMenu.AddItem(new MenuItem("HarassQ", "Use Q", true).SetValue(true));
                HarassMenu.AddItem(new MenuItem("HarassE", "Use E", true).SetValue(true));
                HarassMenu.AddItem(new MenuItem("HarassR", "Use R", true).SetValue(true));
                HarassMenu.AddItem(
                    new MenuItem("HarassRLimit", "Use R|Limit Stack >= x", true).SetValue(new Slider(5, 0, 10)));
                HarassMenu.AddItem(
                    new MenuItem("HarassMana", "When Player ManaPercent >= x%", true).SetValue(new Slider(60)));
            }

            var LaneClearMenu = Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            {
                LaneClearMenu.AddItem(new MenuItem("LaneClearQ", "Use Q", true).SetValue(true));
                LaneClearMenu.AddItem(new MenuItem("LaneClearE", "Use E", true).SetValue(true));
                LaneClearMenu.AddItem(
                    new MenuItem("LaneClearECount", "If E CanHit Counts >= x", true).SetValue(new Slider(3, 1, 5)));
                LaneClearMenu.AddItem(new MenuItem("LaneClearR", "Use R", true).SetValue(true));
                LaneClearMenu.AddItem(
                    new MenuItem("LaneClearRLimit", "Use R|Limit Stack >= x", true).SetValue(new Slider(4, 0, 10)));
                LaneClearMenu.AddItem(
                    new MenuItem("LaneClearMana", "If Player ManaPercent >= %", true).SetValue(new Slider(60)));
            }

            var JungleClearMenu = Menu.AddSubMenu(new Menu("JungleClear", "JungleClear"));
            {
                JungleClearMenu.AddItem(new MenuItem("JungleClearQ", "Use Q", true).SetValue(true));
                JungleClearMenu.AddItem(new MenuItem("JungleClearW", "Use W", true).SetValue(true));
                JungleClearMenu.AddItem(new MenuItem("JungleClearE", "Use E", true).SetValue(true));
                JungleClearMenu.AddItem(new MenuItem("JungleClearR", "Use R", true).SetValue(true));
                JungleClearMenu.AddItem(
                    new MenuItem("JungleClearRLimit", "Use R|Limit Stack >= x", true).SetValue(new Slider(5, 0, 10)));
                JungleClearMenu.AddItem(
                    new MenuItem("JungleClearMana", "When Player ManaPercent >= x%", true).SetValue(new Slider(30)));
            }

            var KillStealMenu = Menu.AddSubMenu(new Menu("KillSteal", "KillSteal"));
            {
                KillStealMenu.AddItem(new MenuItem("KillStealQ", "Use Q", true).SetValue(true));
                KillStealMenu.AddItem(new MenuItem("KillStealE", "Use E", true).SetValue(true));
                KillStealMenu.AddItem(new MenuItem("KillStealR", "Use R", true).SetValue(true));
            }

            var MiscMenu = Menu.AddSubMenu(new Menu("Misc", "Misc"));
            {
                MiscMenu.AddItem(new MenuItem("GapE", "Anti GapCloser E", true).SetValue(true));
                MiscMenu.AddItem(
                    new MenuItem("SemiR", "Semi-manual R Key", true).SetValue(new KeyBind('T', KeyBindType.Press)));
            }

            var DrawMenu = Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            {
                DrawMenu.AddItem(new MenuItem("DrawQ", "Draw Q Range", true).SetValue(false));
                DrawMenu.AddItem(new MenuItem("DrawW", "Draw W Range", true).SetValue(false));
                DrawMenu.AddItem(new MenuItem("DrawE", "Draw E Range", true).SetValue(false));
                DrawMenu.AddItem(new MenuItem("DrawR", "Draw R Range", true).SetValue(false));
                DrawMenu.AddItem(new MenuItem("DrawDamage", "Draw ComboDamage", true).SetValue(true));
            }

            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Obj_AI_Base.OnSpellCast += OnSpellCast;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private void OnEnemyGapcloser(ActiveGapcloser Args)
        {
            if (Menu.Item("GapE", true).GetValue<bool>() && E.IsReady() && Args.Sender.IsValidTarget(E.Range))
            {
                E.CastTo(Args.Sender);
            }
        }

        private void OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs Args)
        {
            if (!sender.IsMe || !Orbwalking.IsAutoAttack(Args.SData.Name))
            {
                return;
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                var target = (AIHeroClient)Args.Target;

                if (target != null && !target.IsDead && !target.IsZombie)
                {
                    if (Menu.Item("ComboW", true).GetValue<bool>() && W.IsReady() && target.IsValidTarget(W.Range))
                    {
                        W.Cast();
                    }

                    if (Menu.Item("ComboR", true).GetValue<bool>() && R.IsReady() &&
                        Menu.Item("ComboRLimit", true).GetValue<Slider>().Value >= GetRCount &&
                        target.IsValidTarget(R.Range))
                    {
                        R.CastTo(target);
                    }

                    if (Menu.Item("ComboQ", true).GetValue<bool>() && Q.IsReady() && target.IsValidTarget(Q.Range))
                    {
                        Q.CastTo(target);
                    }

                    if (Menu.Item("ComboE", true).GetValue<bool>() && E.IsReady() && target.IsValidTarget(E.Range))
                    {
                        E.CastTo(target);
                    }
                }
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                if (Me.ManaPercent >= Menu.Item("JungleClearMana", true).GetValue<Slider>().Value)
                {
                    var mobs = MinionManager.GetMinions(Me.Position, R.Range, MinionTypes.All, MinionTeam.Neutral,
                        MinionOrderTypes.MaxHealth);

                    if (mobs.Any())
                    {
                        var mob = mobs.FirstOrDefault();
                        var bigmob = mobs.FirstOrDefault(x => !x.Name.ToLower().Contains("mini"));

                        if (Menu.Item("JungleClearW", true).GetValue<bool>() && W.IsReady() && bigmob != null &&
                            bigmob.IsValidTarget(W.Range))
                        {
                            W.Cast();
                        }

                        if (Menu.Item("JungleClearR", true).GetValue<bool>() && R.IsReady() &&
                            Menu.Item("JungleClearRLimit", true).GetValue<Slider>().Value >= GetRCount && bigmob != null)
                        {
                            R.Cast(bigmob);
                        }

                        if (Menu.Item("JungleClearE", true).GetValue<bool>() && E.IsReady())
                        {
                            if (bigmob != null && bigmob.IsValidTarget(E.Range))
                            {
                                E.Cast(bigmob);
                            }
                            else
                            {
                                var eMobs = MinionManager.GetMinions(Me.Position, E.Range, MinionTypes.All, MinionTeam.Neutral,
                                    MinionOrderTypes.MaxHealth);
                                var eFarm = E.GetLineFarmLocation(eMobs, E.Width);

                                if (eFarm.MinionsHit >= 2)
                                {
                                    E.Cast(eFarm.Position);
                                }
                            }
                        }

                        if (Menu.Item("JungleClearQ", true).GetValue<bool>() && Q.IsReady() && mob != null &&
                            mob.IsValidTarget(Q.Range))
                        {
                            Q.Cast(mob);
                        }
                    }
                }
            }
        }

        private void OnUpdate(EventArgs Args)
        {
            if (Me.IsDead)
            {
                return;
            }

            if (W.Level > 0)
            {
                W.Range = Me.AttackRange + new[] { 130, 150, 170, 190, 210 }[W.Level - 1];
            }

            if (R.Level > 0)
            {
                R.Range = 1200 + 300*R.Level - 1;
            }

            SemiRLogic();
            KillSteal();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
            }
        }

        private void SemiRLogic()
        {
            if (Menu.Item("SemiR", true).GetValue<KeyBind>().Active && R.IsReady())
            {
                var target = TargetSelector.GetSelectedTarget() ??
                             TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

                if (CheckTarget(target, R.Range))
                {
                    R.CastTo(target);
                }
            }
        }

        private void KillSteal()
        {
            if (Menu.Item("KillStealQ", true).GetValue<bool>() && Q.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && x.Health < Q.GetDamage(x)))
                {
                    Q.CastTo(target);
                    return;
                }
            }

            if (Menu.Item("KillStealE", true).GetValue<bool>() && E.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(E.Range) && x.Health < E.GetDamage(x)))
                {
                    E.CastTo(target);
                    return;
                }
            }

            if (Menu.Item("KillStealR", true).GetValue<bool>() && R.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range) && x.Health < R.GetDamage(x)))
                {
                    R.CastTo(target);
                    return;
                }
            }
        }

        private void Combo()
        {
            var target = TargetSelector.GetSelectedTarget() ??
                         TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            if (CheckTarget(target, R.Range))
            {
                if (Menu.Item("ComboR", true).GetValue<bool>() && R.IsReady() &&
                    Menu.Item("ComboRLimit", true).GetValue<Slider>().Value >= GetRCount &&
                    target.IsValidTarget(R.Range))
                {
                    R.CastTo(target);
                }

                if (Menu.Item("ComboQ", true).GetValue<bool>() && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.CastTo(target);
                }

                if (Menu.Item("ComboE", true).GetValue<bool>() && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.CastTo(target);
                }

                if (Menu.Item("ComboW", true).GetValue<bool>() && W.IsReady() && target.IsValidTarget(W.Range) &&
                    target.DistanceToPlayer() > Orbwalking.GetRealAutoAttackRange(Me) && Me.CanAttack)
                {
                    W.Cast();
                }
            }
        }

        private void Harass()
        {
            if (Me.UnderTurret(true))
            {
                return;
            }

            if (Me.ManaPercent >= Menu.Item("HarassMana", true).GetValue<Slider>().Value)
            {
                var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

                if (CheckTarget(target, R.Range))
                {
                    if (Menu.Item("HarassR", true).GetValue<bool>() && R.IsReady() &&
                        Menu.Item("HarassRLimit", true).GetValue<Slider>().Value >= GetRCount &&
                        target.IsValidTarget(R.Range))
                    {
                        R.CastTo(target);
                    }

                    if (Menu.Item("HarassQ", true).GetValue<bool>() && Q.IsReady() && target.IsValidTarget(Q.Range))
                    {
                        Q.CastTo(target);
                    }

                    if (Menu.Item("HarassE", true).GetValue<bool>() && E.IsReady() && target.IsValidTarget(E.Range))
                    {
                        E.CastTo(target);
                    }
                }
            }
        }

        private void LaneClear()
        {
            if (Me.ManaPercent >= Menu.Item("LaneClearMana", true).GetValue<Slider>().Value)
            {
                var minions = MinionManager.GetMinions(Me.Position, R.Range);

                if (minions.Any())
                {
                    if (Menu.Item("LaneClearR", true).GetValue<bool>() && R.IsReady() &&
                        Menu.Item("LaneClearRLimit", true).GetValue<Slider>().Value >= GetRCount)
                    {
                        var rMinion =
                            minions.FirstOrDefault(x => x.DistanceToPlayer() > Orbwalking.GetRealAutoAttackRange(Me));

                        if (rMinion != null && HealthPrediction.GetHealthPrediction(rMinion, 250) > 0)
                        {
                            R.Cast(rMinion);
                        }
                    }

                    if (Menu.Item("LaneClearE", true).GetValue<bool>() && E.IsReady())
                    {
                        var eMinions = MinionManager.GetMinions(Me.Position, E.Range);
                        var eFarm = E.GetLineFarmLocation(eMinions, E.Width);

                        if (eFarm.MinionsHit >= Menu.Item("LaneClearECount", true).GetValue<Slider>().Value)
                        {
                            E.Cast(eFarm.Position);
                        }
                    }

                    if (Menu.Item("LaneClearQ", true).GetValue<bool>() && Q.IsReady())
                    {
                        var qMinion =
                            MinionManager
                                .GetMinions(
                                    Me.Position, Q.Range)
                                .FirstOrDefault(
                                    x =>
                                        x.Health < Q.GetDamage(x) &&
                                        HealthPrediction.GetHealthPrediction(x, 250) > 0 &&
                                        x.Health > Me.GetAutoAttackDamage(x));

                        if (qMinion != null)
                        {
                            Q.Cast(qMinion);
                        }
                    }
                }
            }
        }

        private void JungleClear()
        {
            if (Me.ManaPercent >= Menu.Item("JungleClearMana", true).GetValue<Slider>().Value)
            {
                var mobs = MinionManager.GetMinions(Me.Position, R.Range, MinionTypes.All, MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth);

                if (mobs.Any())
                {
                    var mob = mobs.FirstOrDefault();
                    var bigmob = mobs.FirstOrDefault(x => !x.Name.ToLower().Contains("mini"));

                    if (Menu.Item("JungleClearW", true).GetValue<bool>() && W.IsReady() && bigmob != null &&
                        bigmob.IsValidTarget(W.Range))
                    {
                        W.Cast();
                    }

                    if (Menu.Item("JungleClearR", true).GetValue<bool>() && R.IsReady() &&
                        Menu.Item("JungleClearRLimit", true).GetValue<Slider>().Value >= GetRCount && bigmob != null)
                    {
                        R.Cast(bigmob);
                    }

                    if (Menu.Item("JungleClearE", true).GetValue<bool>() && E.IsReady())
                    {
                        if (bigmob != null && bigmob.IsValidTarget(E.Range))
                        {
                            E.Cast(bigmob);
                        }
                        else
                        {
                            var eMobs = MinionManager.GetMinions(Me.Position, E.Range, MinionTypes.All, MinionTeam.Neutral,
                                MinionOrderTypes.MaxHealth);
                            var eFarm = E.GetLineFarmLocation(eMobs, E.Width);

                            if (eFarm.MinionsHit >= 2)
                            {
                                E.Cast(eFarm.Position);
                            }
                        }
                    }

                    if (Menu.Item("JungleClearQ", true).GetValue<bool>() && Q.IsReady() && mob != null &&
                        mob.IsValidTarget(Q.Range))
                    {
                        Q.Cast(mob);
                    }
                }
            }
        }

        private void OnDraw(EventArgs Args)
        {
            if (!Me.IsDead && !Shop.IsOpen && !MenuGUI.IsChatOpen )
            {
                if (Menu.Item("DrawQ", true).GetValue<bool>() && Q.IsReady())
                {
                    Render.Circle.DrawCircle(Me.Position, Q.Range, Color.Green, 1);
                }

                if (Menu.Item("DrawW", true).GetValue<bool>() && W.IsReady())
                {
                    Render.Circle.DrawCircle(Me.Position, W.Range, Color.FromArgb(9, 253, 242), 1);
                }

                if (Menu.Item("DrawE", true).GetValue<bool>() && E.IsReady())
                {
                    Render.Circle.DrawCircle(Me.Position, E.Range, Color.FromArgb(188, 6, 248), 1);
                }

                if (Menu.Item("DrawR", true).GetValue<bool>() && R.IsReady())
                {
                    Render.Circle.DrawCircle(Me.Position, R.Range, Color.FromArgb(19, 130, 234), 1);
                }

                if (Menu.Item("DrawDamage", true).GetValue<bool>())
                {
                    foreach (
                        var x in HeroManager.Enemies.Where(e => e.IsValidTarget() && !e.IsDead && !e.IsZombie))
                    {
                        HpBarDraw.Unit = x;
                        HpBarDraw.DrawDmg((float)ComboDamage(x), new ColorBGRA(255, 204, 0, 170));
                    }
                }
            }
        }

        private int GetRCount
            => Me.HasBuff("kogmawlivingartillerycost") ? Me.GetBuffCount("kogmawlivingartillerycost") : 0;
    }
}
