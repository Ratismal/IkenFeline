#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameEngine;

namespace LittleWitch
{
    public class patch_AbilityIgnite : AbilityIgnite
    {
        public override bool InRange(BattleGrid grid, BattleUnit unit, VectorI target)
        {
            return true;
        }

        public override bool InHitArea(BattleGrid grid, BattleUnit unit, VectorI target, VectorI tile)
        {
            return true;
        }
    }
}
