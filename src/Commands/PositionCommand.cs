using System;
using System.Globalization;
using System.Text;
using CommandSystem;
using Exiled.API.Features;
using NorthwoodLib.Pools;
using UnityEngine;

namespace AugatonLib.Commands
{
    public class PositionCommand : StaffCommand
    {
        public PositionCommand(string permission) => Permission = permission;

        public override string Command => "pos";

        public override string[] Aliases => new[] { "position", "coords" };

        public override string Description => "Releve la position monde courante au format UncomplicatedCustomTeams.";

        public override string Permission { get; }

        protected override bool OnExecute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (player is null)
            {
                response = "Commande disponible uniquement depuis la console en jeu.";
                return false;
            }

            if (!player.IsAlive)
            {
                response = "Vous devez etre en vie pour relever une position.";
                return false;
            }

            Vector3 position = player.Position;
            float yaw = Mathf.Repeat(player.Rotation.eulerAngles.y, 360f);
            Room room = player.CurrentRoom;

            StringBuilder builder = StringBuilderPool.Shared.Rent();

            try
            {
                builder.AppendLine(room is null
                    ? "Position relevee hors d'une salle identifiee."
                    : $"Position relevee dans {room.Type} ({room.Zone}).");

                builder.AppendLine("settings:");
                builder.AppendLine($"  pos_x: {Format(position.x)}");
                builder.AppendLine($"  pos_y: {Format(position.y)}");
                builder.AppendLine($"  pos_z: {Format(position.z)}");
                builder.AppendLine($"  rot_y: {Format(yaw)}");

                response = builder.ToString();
                return true;
            }
            finally
            {
                StringBuilderPool.Shared.Return(builder);
            }
        }

        private static string Format(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
