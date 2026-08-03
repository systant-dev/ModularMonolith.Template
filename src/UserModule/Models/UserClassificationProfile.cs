using ModularMonolith.Template.SharedKernel.Compliance;

namespace UserModule.Models;

[ClassificationProfile]
internal sealed partial class UserClassificationProfile : ClassificationProfile<User>
{
    #region Constructor Declarations

    public UserClassificationProfile()
    {
        Classify(user => user.FullName, DataClassification.Personal);
        Classify(user => user.Email, DataClassification.Sensitive);
        Classify(user => user.PhoneNumber, DataClassification.Personal);
    }

    #endregion
}
