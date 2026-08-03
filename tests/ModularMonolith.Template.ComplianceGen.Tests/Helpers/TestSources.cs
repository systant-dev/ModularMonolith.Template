namespace ModularMonolith.Template.ComplianceGen.Tests.Helpers;

internal static class TestSources
{
    #region Public Field Declarations

    /// <summary>
    /// The SharedKernel compliance abstractions, provided as source so generator tests compile
    /// deterministically without depending on the referenced assembly being loaded into the AppDomain.
    /// </summary>
    public const string Abstractions = """
        namespace ModularMonolith.Template.SharedKernel.Compliance
        {
            public enum DataClassification { None = 0, Public = 1, Internal = 2, Personal = 3, Sensitive = 4 }

            public interface IStringRedactor { string Redact(string? value); }

            public interface IRedactorProvider { IStringRedactor GetRedactor(DataClassification classification); }

            public interface IModelRedactor<T> { T Redact(T model, IRedactorProvider redactorProvider); }

            public abstract class ClassificationProfile<T>
            {
                protected void Classify<TProperty>(
                    global::System.Linq.Expressions.Expression<global::System.Func<T, TProperty>> selector,
                    DataClassification classification)
                {
                }
            }
        }
        """;

    /// <summary>A model with two string members classified via a fluent profile.</summary>
    public const string SimpleProfile = """
        namespace Contracts
        {
            public sealed class Customer
            {
                public string Name { get; set; } = string.Empty;
                public string Email { get; set; } = string.Empty;
                public int Age { get; set; }
            }

            public sealed partial class CustomerClassification : ModularMonolith.Template.SharedKernel.Compliance.ClassificationProfile<Customer>
            {
                public CustomerClassification()
                {
                    Classify(x => x.Name, ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Personal);
                    Classify(x => x.Email, ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Sensitive);
                }
            }
        }
        """;

    /// <summary>A profile classifying a nested member path.</summary>
    public const string NestedProfile = """
        namespace Contracts
        {
            public sealed class Address
            {
                public string PostCode { get; set; } = string.Empty;
            }

            public sealed class Order
            {
                public string Reference { get; set; } = string.Empty;
                public Address ShipTo { get; set; } = new();
            }

            public sealed partial class OrderClassification : ModularMonolith.Template.SharedKernel.Compliance.ClassificationProfile<Order>
            {
                public OrderClassification()
                {
                    Classify(x => x.Reference, ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Internal);
                    Classify(x => x.ShipTo.PostCode, ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Personal);
                }
            }
        }
        """;

    /// <summary>A profile classifying a non-string member, which is diagnosed (CMP002).</summary>
    public const string NonStringMember = """
        namespace Contracts
        {
            public sealed class Account
            {
                public string Owner { get; set; } = string.Empty;
                public int Balance { get; set; }
            }

            public sealed partial class AccountClassification : ModularMonolith.Template.SharedKernel.Compliance.ClassificationProfile<Account>
            {
                public AccountClassification()
                {
                    Classify(x => x.Owner, ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Personal);
                    Classify(x => x.Balance, ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Sensitive);
                }
            }
        }
        """;

    /// <summary>A profile whose selector is not a member access, which is diagnosed (CMP001).</summary>
    public const string UnsupportedSelector = """
        namespace Contracts
        {
            public sealed class Note
            {
                public string Text { get; set; } = string.Empty;
            }

            public sealed partial class NoteClassification : ModularMonolith.Template.SharedKernel.Compliance.ClassificationProfile<Note>
            {
                public NoteClassification()
                {
                    Classify(x => x.Text.ToUpper(), ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Personal);
                }
            }
        }
        """;

    /// <summary>Source that declares no classification profiles.</summary>
    public const string NoProfiles = """
        namespace Contracts
        {
            public sealed class Empty
            {
                public string Value { get; set; } = string.Empty;
            }
        }
        """;

    /// <summary>Two profiles declared in the same compilation, each producing its own redactor.</summary>
    public const string MultipleProfiles = """
        namespace Contracts
        {
            public sealed class Customer
            {
                public string Name { get; set; } = string.Empty;
            }

            public sealed class Supplier
            {
                public string ContactEmail { get; set; } = string.Empty;
            }

            public sealed partial class CustomerClassification : ModularMonolith.Template.SharedKernel.Compliance.ClassificationProfile<Customer>
            {
                public CustomerClassification()
                {
                    Classify(x => x.Name, ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Personal);
                }
            }

            public sealed partial class SupplierClassification : ModularMonolith.Template.SharedKernel.Compliance.ClassificationProfile<Supplier>
            {
                public SupplierClassification()
                {
                    Classify(x => x.ContactEmail, ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Sensitive);
                }
            }
        }
        """;

    /// <summary>A profile declared in the global namespace, exercising the namespace-less key path.</summary>
    public const string GlobalNamespaceProfile = """
        public sealed class Account
        {
            public string Owner { get; set; } = string.Empty;
        }

        public sealed partial class AccountClassification : ModularMonolith.Template.SharedKernel.Compliance.ClassificationProfile<Account>
        {
            public AccountClassification()
            {
                Classify(x => x.Owner, ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Personal);
            }
        }
        """;

    /// <summary>A profile classifying a member three levels deep, exercising nested null guards.</summary>
    public const string DeeplyNestedProfile = """
        namespace Contracts
        {
            public sealed class Country
            {
                public string IsoCode { get; set; } = string.Empty;
            }

            public sealed class Address
            {
                public Country Country { get; set; } = new();
            }

            public sealed class Customer
            {
                public Address HomeAddress { get; set; } = new();
            }

            public sealed partial class CustomerClassification : ModularMonolith.Template.SharedKernel.Compliance.ClassificationProfile<Customer>
            {
                public CustomerClassification()
                {
                    Classify(x => x.HomeAddress.Country.IsoCode, ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Personal);
                }
            }
        }
        """;

    /// <summary>An internal, non-sealed profile, exercising accessibility and type-keyword emission.</summary>
    public const string InternalProfile = """
        namespace Contracts
        {
            public sealed class Ticket
            {
                public string Subject { get; set; } = string.Empty;
            }

            internal partial class TicketClassification : ModularMonolith.Template.SharedKernel.Compliance.ClassificationProfile<Ticket>
            {
                public TicketClassification()
                {
                    Classify(x => x.Subject, ModularMonolith.Template.SharedKernel.Compliance.DataClassification.Internal);
                }
            }
        }
        """;

    #endregion
}
