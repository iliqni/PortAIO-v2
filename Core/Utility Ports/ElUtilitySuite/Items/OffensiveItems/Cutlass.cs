using EloBuddy;
namespace ElUtilitySuite.Items.OffensiveItems
{
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal class Cutlass : Item
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        public override ItemId Id => ItemId.Bilgewater_Cutlass;

        /// <summary>
        ///     Gets or sets the name of the item.
        /// </summary>
        /// <value>
        ///     The name of the item.
        /// </value>
        public override string Name => "Bilgewater Cutlass";

        public static EloBuddy.SDK.Item Bilgewater_Cutlass;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        public override void CreateMenu()
        {
            Bilgewater_Cutlass = new EloBuddy.SDK.Item(ItemId.Bilgewater_Cutlass);
            this.Menu.AddItem(new MenuItem("UseCutlassCombo", "Use on Combo").SetValue(true));
            this.Menu.AddItem(new MenuItem("CutlassMyHp", "Use on My Hp %").SetValue(new Slider(100)));
        }

        /// <summary>
        ///     Shoulds the use item.
        /// </summary>
        /// <returns></returns>
        public override bool ShouldUseItem()
        {
            return this.Menu.Item("UseCutlassCombo").IsActive() && this.ComboModeActive && this.Player.HealthPercent < this.Menu.Item("CutlassMyHp").GetValue<Slider>().Value && Bilgewater_Cutlass.IsOwned() && Bilgewater_Cutlass.IsReady();
        }

        /// <summary>
        ///     Uses the item.
        /// </summary>
        public override void UseItem()
        {
            var targ = TargetSelector.GetTarget(550, TargetSelector.DamageType.Physical);
            if (targ != null && targ.IsHPBarRendered && targ.IsVisible)
                Bilgewater_Cutlass.Cast(targ);
        }

        #endregion
    }
}