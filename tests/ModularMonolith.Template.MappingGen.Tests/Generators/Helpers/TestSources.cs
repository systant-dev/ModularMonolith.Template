namespace ModularMonolith.Template.MappingGen.Tests.Generators.Helpers;

internal static class TestSources
{
    #region Public Field Declarations

    /// <summary>A source type, a matching target type and a mapper with a single call site.</summary>
    public const string MatchingProperties = """
        namespace Contracts
        {
            public sealed class Person
            {
                public string FirstName { get; set; } = string.Empty;
                public string LastName { get; set; } = string.Empty;
                public int Age { get; set; }
            }

            public sealed class PersonDto
            {
                public string FirstName { get; set; } = string.Empty;
                public string LastName { get; set; } = string.Empty;
                public int Age { get; set; }
            }

            public static partial class PersonMapper
            {
                [ModularMonolith.Template.MappingGen.Map]
                public static partial TTarget To<TTarget>(this Person source) where TTarget : new();
            }

            public static class Consumer
            {
                public static PersonDto Map(Person person) => person.To<PersonDto>();
            }
        }
        """;

    /// <summary>Property names differ only by casing and a widening numeric conversion is required.</summary>
    public const string CaseInsensitiveAndConvertible = """
        namespace Contracts
        {
            public sealed class Source
            {
                public string Name { get; set; } = string.Empty;
                public int Count { get; set; }
                public string Ignored { get; set; } = string.Empty;
            }

            public sealed class Target
            {
                public string NAME { get; set; } = string.Empty;
                public long Count { get; set; }
                public string Missing { get; set; } = string.Empty;
            }

            public static partial class SourceMapper
            {
                [ModularMonolith.Template.MappingGen.Map]
                public static partial TTarget To<TTarget>(this Source source) where TTarget : new();
            }

            public static class Consumer
            {
                public static Target Map(Source source) => source.To<Target>();
            }
        }
        """;

    /// <summary>A single mapper used with two different concrete target types.</summary>
    public const string MultipleTargets = """
        namespace Contracts
        {
            public sealed class Order
            {
                public int Id { get; set; }
                public string Customer { get; set; } = string.Empty;
            }

            public sealed class OrderSummary
            {
                public int Id { get; set; }
            }

            public sealed class OrderDetails
            {
                public int Id { get; set; }
                public string Customer { get; set; } = string.Empty;
            }

            public static partial class OrderMapper
            {
                [ModularMonolith.Template.MappingGen.Map]
                public static partial TTarget To<TTarget>(this Order source) where TTarget : new();
            }

            public static class Consumer
            {
                public static OrderSummary Summary(Order order) => order.To<OrderSummary>();
                public static OrderDetails Details(Order order) => order.To<OrderDetails>();
            }
        }
        """;

    /// <summary>A mapper that is declared but never invoked with a concrete target.</summary>
    public const string NoInvocations = """
        namespace Contracts
        {
            public sealed class Widget
            {
                public string Label { get; set; } = string.Empty;
            }

            public static partial class WidgetMapper
            {
                [ModularMonolith.Template.MappingGen.Map]
                public static partial TTarget To<TTarget>(this Widget source) where TTarget : new();
            }
        }
        """;

    /// <summary>A target with a required member that has no matching source property.</summary>
    public const string UnmappableRequiredMember = """
        namespace Contracts
        {
            public sealed class Account
            {
                public int Id { get; set; }
            }

            public sealed class AccountDto
            {
                public required int Id { get; init; }
                public required string Name { get; init; }
            }

            public static partial class AccountMapper
            {
                [ModularMonolith.Template.MappingGen.Map]
                public static partial TTarget To<TTarget>(this Account source);
            }

            public static class Consumer
            {
                public static AccountDto Map(Account account) => account.To<AccountDto>();
            }
        }
        """;

    #endregion
}
