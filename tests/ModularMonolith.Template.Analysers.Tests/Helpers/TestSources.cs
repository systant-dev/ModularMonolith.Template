namespace ModularMonolith.Template.Analysers.Tests.Helpers;

internal static class TestSources
{
    #region Field Declarations

    public const string MarkerTypes = """
        namespace ModularMonolith.Template.SharedKernel.Modular
        {
            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
            public sealed class ModuleAttribute : System.Attribute { }

            public interface IModule { }
        }
        """;

    public const string ModuleImplementingInterface = """
        namespace Sample
        {
            using ModularMonolith.Template.SharedKernel.Modular;

            [Module]
            public sealed class GoodModule : IModule { }
        }
        """;

    public const string ModuleMissingInterface = """
        namespace Sample
        {
            using ModularMonolith.Template.SharedKernel.Modular;

            [Module]
            public sealed class BadModule { }
        }
        """;

    public const string TypeWithoutModuleAttribute = """
        namespace Sample
        {
            using ModularMonolith.Template.SharedKernel.Modular;

            public sealed class PlainType { }
        }
        """;

    public const string ModuleInheritingInterfaceFromBase = """
        namespace Sample
        {
            using ModularMonolith.Template.SharedKernel.Modular;

            public abstract class ModuleBase : IModule { }

            [Module]
            public sealed class DerivedModule : ModuleBase { }
        }
        """;

    public const string CrossModuleTypes = """
        namespace UserModule
        {
            public sealed class UserStore
            {
                public int Count() => 0;
            }
        }

        namespace UserModule.Contracts
        {
            public sealed class UserResponse
            {
                public int Id { get; set; }
            }
        }
        """;

    public const string ModuleReferencingOtherModuleInternal = """
        namespace OrderModule
        {
            using UserModule;

            public sealed class OrderService
            {
                public int Count()
                {
                    UserStore store = new();
                    return store.Count();
                }
            }
        }
        """;

    public const string ModuleReferencingOtherModuleContracts = """
        namespace OrderModule
        {
            using UserModule.Contracts;

            public sealed class OrderService
            {
                public int UserId()
                {
                    UserResponse response = new();
                    return response.Id;
                }
            }
        }
        """;

    public const string ModuleReferencingOwnModuleInternal = """
        namespace UserModule
        {
            public sealed class UserService
            {
                public int Count()
                {
                    UserStore store = new();
                    return store.Count();
                }
            }
        }
        """;

    public const string NonModuleReferencingModuleInternal = """
        namespace Api
        {
            using UserModule;

            public sealed class Startup
            {
                public int Count()
                {
                    UserStore store = new();
                    return store.Count();
                }
            }
        }
        """;

    public const string ResultMarkerTypes = """
        namespace ModularMonolith.Template.SharedKernel.Results
        {
            public class Result { }

            public sealed class Result<TValue> : Result { }
        }

        namespace Sample
        {
            using ModularMonolith.Template.SharedKernel.Results;
            using System.Threading.Tasks;

            public sealed class Service
            {
                public Result DoWork() => new();

                public Result<int> Produce() => new();

                public Task<Result> DoWorkAsync() => Task.FromResult(new Result());

                public int Number() => 0;
            }
        }
        """;

    public const string ResultDiscarded = """
        namespace Sample
        {
            public sealed class Consumer
            {
                public void Run()
                {
                    Service service = new();
                    service.DoWork();
                }
            }
        }
        """;

    public const string ResultDiscardedAsync = """
        namespace Sample
        {
            using System.Threading.Tasks;

            public sealed class Consumer
            {
                public async Task RunAsync()
                {
                    Service service = new();
                    await service.DoWorkAsync();
                }
            }
        }
        """;

    public const string ResultConsumedByAssignment = """
        namespace Sample
        {
            using ModularMonolith.Template.SharedKernel.Results;

            public sealed class Consumer
            {
                public void Run()
                {
                    Service service = new();
                    Result result = service.DoWork();
                    _ = result;
                }
            }
        }
        """;

    public const string NonResultExpressionStatement = """
        namespace Sample
        {
            public sealed class Consumer
            {
                public void Run()
                {
                    Service service = new();
                    service.Number();
                }
            }
        }
        """;

    #endregion
}
