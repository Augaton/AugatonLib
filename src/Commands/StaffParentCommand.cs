using System;
using System.Text;
using CommandSystem;
using NorthwoodLib.Pools;

namespace AugatonLib.Commands
{
    public abstract class StaffParentCommand : ParentCommand
    {
        public abstract string Permission { get; }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder builder = StringBuilderPool.Shared.Rent();

            try
            {
                builder.AppendLine($"{Command} - {Description}");
                builder.AppendLine($"Permission : {Permission}");
                builder.AppendLine("Sous-commandes :");

                foreach (ICommand command in AllCommands)
                {
                    string aliases = command.Aliases is null || command.Aliases.Length == 0
                        ? string.Empty
                        : $" ({string.Join(", ", command.Aliases)})";

                    builder.AppendLine($"  {command.Command}{aliases} - {command.Description}");
                }

                response = builder.ToString();
                return false;
            }
            finally
            {
                StringBuilderPool.Shared.Return(builder);
            }
        }
    }
}
