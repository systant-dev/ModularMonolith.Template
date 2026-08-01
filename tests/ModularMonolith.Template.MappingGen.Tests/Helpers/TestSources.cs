namespace ModularMonolith.Template.MappingGen.Tests.Helpers;

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

    /// <summary>Requires ToString(), explicit numeric narrowing and enum-to-int conversions.</summary>
    public const string TypeConversions = """
        namespace Contracts
        {
            public enum Colour { Red, Green, Blue }

            public sealed class Source
            {
                public int Code { get; set; }
                public long Big { get; set; }
                public Colour Shade { get; set; }
            }

            public sealed class Target
            {
                public string Code { get; set; } = string.Empty;
                public int Big { get; set; }
                public int Shade { get; set; }
            }

            public static partial class ConversionMapper
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

    /// <summary>A nested reference-typed property that requires recursive mapping.</summary>
    public const string NestedMapping = """
        namespace Contracts
        {
            public sealed class Address
            {
                public string City { get; set; } = string.Empty;
                public string Zip { get; set; } = string.Empty;
            }

            public sealed class AddressDto
            {
                public string City { get; set; } = string.Empty;
                public string Zip { get; set; } = string.Empty;
            }

            public sealed class Person
            {
                public string Name { get; set; } = string.Empty;
                public Address HomeAddress { get; set; } = new();
            }

            public sealed class PersonDto
            {
                public string Name { get; set; } = string.Empty;
                public AddressDto HomeAddress { get; set; } = new();
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

    /// <summary>A collection property whose element type must be mapped and materialised to an array.</summary>
    public const string CollectionMapping = """
        using System.Collections.Generic;

        namespace Contracts
        {
            public sealed class Item
            {
                public int Id { get; set; }
            }

            public sealed class ItemDto
            {
                public int Id { get; set; }
            }

            public sealed class Basket
            {
                public List<Item> Items { get; set; } = new();
            }

            public sealed class BasketDto
            {
                public ItemDto[] Items { get; set; } = System.Array.Empty<ItemDto>();
            }

            public static partial class BasketMapper
            {
                [ModularMonolith.Template.MappingGen.Map]
                public static partial TTarget To<TTarget>(this Basket source) where TTarget : new();
            }

            public static class Consumer
            {
                public static BasketDto Map(Basket basket) => basket.To<BasketDto>();
            }
        }
        """;

    /// <summary>A self-referential type that must trigger cycle detection during nested mapping.</summary>
    public const string RecursiveCycle = """
        namespace Contracts
        {
            public sealed class Node
            {
                public int Value { get; set; }
                public Node Next { get; set; } = new();
            }

            public sealed class NodeDto
            {
                public int Value { get; set; }
                public NodeDto Next { get; set; } = new();
            }

            public static partial class NodeMapper
            {
                [ModularMonolith.Template.MappingGen.Map]
                public static partial TTarget To<TTarget>(this Node source) where TTarget : new();
            }

            public static class Consumer
            {
                public static NodeDto Map(Node node) => node.To<NodeDto>();
            }
        }
        """;

    /// <summary>An optional member that matches by name but has an unconvertible type, triggering MAP002.</summary>
    public const string IncompatibleOptionalMember = """
        namespace Contracts
        {
            public sealed class Source
            {
                public int Id { get; set; }
                public string Amount { get; set; } = string.Empty;
            }

            public sealed class Target
            {
                public int Id { get; set; }
                public decimal Amount { get; set; }
            }

            public static partial class ValueMapper
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

    /// <summary>A collection of complex elements materialised to a List, nested inside another mapped object.</summary>
    public const string NestedCollectionMapping = """
        using System.Collections.Generic;

        namespace Contracts
        {
            public sealed class Line
            {
                public int Quantity { get; set; }
            }

            public sealed class LineDto
            {
                public int Quantity { get; set; }
            }

            public sealed class Order
            {
                public List<Line> Lines { get; set; } = new();
            }

            public sealed class OrderDto
            {
                public List<LineDto> Lines { get; set; } = new();
            }

            public sealed class Invoice
            {
                public Order Order { get; set; } = new();
            }

            public sealed class InvoiceDto
            {
                public OrderDto Order { get; set; } = new();
            }

            public static partial class InvoiceMapper
            {
                [ModularMonolith.Template.MappingGen.Map]
                public static partial TTarget To<TTarget>(this Invoice source) where TTarget : new();
            }

            public static class Consumer
            {
                public static InvoiceDto Map(Invoice invoice) => invoice.To<InvoiceDto>();
            }
        }
        """;

    /// <summary>Source and target that both inherit mappable properties from a base class.</summary>
    public const string InheritedProperties = """
        namespace Contracts
        {
            public abstract class EntityBase
            {
                public int Id { get; set; }
                public string Name { get; set; } = string.Empty;
            }

            public sealed class Employee : EntityBase
            {
                public string Department { get; set; } = string.Empty;
            }

            public abstract class DtoBase
            {
                public int Id { get; set; }
                public string Name { get; set; } = string.Empty;
            }

            public sealed class EmployeeDto : DtoBase
            {
                public string Department { get; set; } = string.Empty;
            }

            public static partial class EmployeeMapper
            {
                [ModularMonolith.Template.MappingGen.Map]
                public static partial TTarget To<TTarget>(this Employee source) where TTarget : new();
            }

            public static class Consumer
            {
                public static EmployeeDto Map(Employee employee) => employee.To<EmployeeDto>();
            }
        }
        """;

    #endregion
}