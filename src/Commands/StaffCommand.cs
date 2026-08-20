using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace AugatonLib.Commands
{
    public abstract class StaffCommand : ICommand
    {
        public abstract string Command { get; }

        public virtual string[] Aliases => Array.Empty<string>();

        public abstract string Description { get; }

        public abstract string Permission { get; }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            try
            {
                if (!sender.CheckPermission(Permission))
                {
                    response = $"Permission refusee. Noeud requis : {Permission}";
                    return false;
                }

                foreach (string argument in arguments)
                {
                    if (argument is not null && argument.Length > 96)
                    {
                        response = "Argument trop long.";
                        return false;
                    }
                }

                return OnExecute(arguments, sender, out response);
            }
            catch (Exception e)
            {
                Log.Error($"{Command}: {e}");
                response = "Erreur interne, voir la console serveur.";
                return false;
            }
        }

        protected abstract bool OnExecute(ArraySegment<string> arguments, ICommandSender sender, out string response);

        protected static string Author(ICommandSender sender)
        {
            return Player.Get(sender) is Player player
                ? $"{player.Nickname} ({player.UserId})"
                : sender.LogName;
        }

        protected static void Audit(ICommandSender sender, string plugin, string action)
        {
            Log.Info($"[{plugin}] {Author(sender)} {action}.");
        }

        protected static bool TryFindPlayer(string query, out Player player, out string error)
        {
            player = null;
            error = null;

            if (string.IsNullOrEmpty(query))
            {
                error = "Joueur non specifie.";
                return false;
            }

            player = Player.Get(query);

            if (player is null)
            {
                error = $"Joueur \"{query}\" introuvable.";
                return false;
            }

            return true;
        }
    }
}
