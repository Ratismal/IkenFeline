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
    }
}
