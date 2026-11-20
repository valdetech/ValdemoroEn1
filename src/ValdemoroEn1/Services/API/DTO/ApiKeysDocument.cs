using Plugin.Firebase.Firestore;

namespace ValdemoroEn1.Services.API.DTO;

public sealed class ApiKeysDocument : IFirestoreObject
{
    [FirestoreProperty("IQAir")]
    public string IQAir { get; private set; }
}
