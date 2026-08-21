using System;
using AugatonLib.Runtime;
using CommandSystem;

namespace AugatonLib.Commands
{
    public sealed class CollectionCommand : StaffCommand
    {
        public CollectionCommand(string permission) => Permission = permission;

        public override string Command => "collection";

        public override string[] Aliases => new[] { "augaton", "lib" };

        public override string Description => "Etat de la collection Zone-Shilari, des integrations et des arbitres partages.";

        public override string Permission { get; }

        protected override bool OnExecute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count > 0 && string.Equals(arguments.At(0), "console", StringComparison.OrdinalIgnoreCase))
            {
                PluginDirectory.PrintBanner();
                response = "Banniere reaffichee dans la console serveur.";
                return true;
            }

            response = CollectionReport.Text();
            return true;
        }
    }
}
