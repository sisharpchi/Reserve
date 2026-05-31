using System.Reflection;

namespace ReserveFlow.Modules.Integrations.Application;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
