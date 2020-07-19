using DShop.Common.RabbitMq;
using DShop.Common.Messages;
using System.Threading.Tasks;

namespace DShop.Common.Handlers
{
    //IMPORTANT: 'in' parameter of type TCommand passes the ICommand as input parameter to ICommandHandler
    //Same ICommand is registered and observed in CommandDispatcher.cs also
    //This way any command of type ICommand passed as message from one class to another(this covers Command part of CQRS)
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Task HandleAsync(TCommand command, ICorrelationContext context);
    }
}