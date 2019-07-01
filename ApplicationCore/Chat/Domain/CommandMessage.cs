using ApplicationCore.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationCore.Chat.Domain
{
    public class CommandMessage
    {
        public MessageCommandType Type { get; private set; }
        public string Command { get; private set; }

        public CommandMessage(CommandMessageDto dto)
        {
            this.Type = dto.Type;
            this.Command = dto.Command;
        }

        public CommandMessage(string rawCommand)
        {
            var commandParts = rawCommand.Substring(1).Split('=');
            var strType = commandParts.FirstOrDefault();
            var command = commandParts.Skip(1).FirstOrDefault();
            if (string.IsNullOrEmpty(command))
            {
                throw new BadRequestException(
                    new List<(string, string)>
                    {
                        ("CommandArgumentsMissing", $"Arguments are missing for the command")
                    });
            }

            switch (strType)
            {
                case "stock":
                    this.Type = MessageCommandType.Stock;
                    break;
                default:
                    throw new BadRequestException(
                        new List<(string, string)>
                        {
                            ("CommandNotRecognized", $"The command {strType} is not valid")
                        });
            };

            this.Command = command;
        }

        public CommandMessageDto ToDto()
        {
            return new CommandMessageDto
            {
                Type = this.Type,
                Command = this.Command
            };
        }
    }

    public class CommandMessageDto
    {
        public MessageCommandType Type { get; set; }
        public string Command { get; set; }
    }

    public enum MessageCommandType
    {
        Stock
    }
}
