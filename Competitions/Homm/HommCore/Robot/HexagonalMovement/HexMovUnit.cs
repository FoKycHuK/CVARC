﻿using CVARC.V2;
using HoMM.Robot;
using HoMM.World;

namespace HoMM.Robot.HexagonalMovement
{
    class HexMovUnit : IUnit
    {
        private IHommRobot actor;

        public HexMovUnit(IHommRobot actor)
        {
            this.actor = actor;
        }

        public UnitResponse ProcessCommand(object command)
        {
            var movement = Compatibility.Check<IHexMovCommand>(this, command).Movement;
            if (movement == null) return UnitResponse.Denied();

            var movementDuration = movement.Apply(actor);

            return UnitResponse.Accepted(movementDuration + 0.001);
        }
    }
}