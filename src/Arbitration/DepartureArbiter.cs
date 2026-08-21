using System;
using System.Collections.Generic;
using Exiled.API.Features;

namespace AugatonLib.Arbitration
{
    public static class DepartureArbiter
    {
        private sealed class Declaration
        {
            public Declaration(string owner, int priority, Func<Player, bool> scope)
            {
                Owner = owner;
                Priority = priority;
                Scope = scope;
            }

            public string Owner { get; }

            public int Priority { get; }

            public Func<Player, bool> Scope { get; }
        }

        private static readonly List<Declaration> Declarations = new List<Declaration>(4);

        public static int Count => Declarations.Count;

        public static void Declare(string owner, int priority, Func<Player, bool> scope)
        {
            if (string.IsNullOrEmpty(owner) || scope is null)
                return;

            Withdraw(owner);
            Declarations.Add(new Declaration(owner, priority, scope));
            Declarations.Sort((left, right) => right.Priority.CompareTo(left.Priority));
        }

        public static void Withdraw(string owner)
        {
            if (string.IsNullOrEmpty(owner))
                return;

            Declarations.RemoveAll(declaration => string.Equals(declaration.Owner, owner, StringComparison.Ordinal));
        }

        public static string Resolve(Player player)
        {
            if (player is null)
                return null;

            foreach (Declaration declaration in Declarations)
            {
                if (InScope(declaration, player))
                    return declaration.Owner;
            }

            return null;
        }

        public static bool IsOwner(Player player, string owner)
        {
            if (string.IsNullOrEmpty(owner) || player is null)
                return false;

            if (Declarations.Count == 0)
                return true;

            string resolved = Resolve(player);

            return resolved is null
                ? false
                : string.Equals(resolved, owner, StringComparison.Ordinal);
        }

        public static void Clear() => Declarations.Clear();

        public static string Describe()
        {
            if (Declarations.Count == 0)
                return "aucune declaration";

            string[] entries = new string[Declarations.Count];

            for (int i = 0; i < Declarations.Count; i++)
                entries[i] = $"{Declarations[i].Owner} ({Declarations[i].Priority})";

            return string.Join(" > ", entries);
        }

        private static bool InScope(Declaration declaration, Player player)
        {
            try
            {
                return declaration.Scope(player);
            }
            catch (Exception e)
            {
                Log.Error($"DepartureArbiter: portee de {declaration.Owner} en echec : {e}");
                return false;
            }
        }
    }
}
