namespace ReserveFlow.Common.Application.Messaging;

public interface ICommand;

public interface ICommand<out TResponse>;
