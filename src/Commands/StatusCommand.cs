using System;
using System.Text;
using CommandSystem;
using NorthwoodLib.Pools;
using AugatonLib.Hints;

namespace AugatonLib.Commands
{
    public sealed class StatusCommand : StaffCommand
    {
        private readonly string pluginName;
        private readonly string version;
        private readonly Func<StringBuilder, bool> detail;

        public StatusCommand(string pluginName, string version, string permission, Func<StringBuilder, bool> detail = null)
        {
            this.pluginName = pluginName;
            this.version = version;
            this.detail = detail;
            Permission = permission;
        }

        public override string Command => "status";

        public override string[] Aliases => new[] { "s", "info" };

        public override string Description => "Affiche l'etat courant du plugin.";

        public override string Permission { get; }

        protected override bool OnExecute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder builder = StringBuilderPool.Shared.Rent();

            try
            {
                builder.AppendLine($"{pluginName} v{version}");
                builder.AppendLine($"  round : {(Exiled.API.Features.Round.IsStarted ? "en cours" : "hors round")}, " +
                                   $"{Exiled.API.Features.Player.List.Count} joueur(s)");
                builder.AppendLine($"  hints : {(HintChannel.ServiceAvailable ? "HintServiceMeow" : "natifs (repli)")}");

                detail?.Invoke(builder);

                response = builder.ToString();
                return true;
            }
            finally
            {
                StringBuilderPool.Shared.Return(builder);
            }
        }
    }
}
