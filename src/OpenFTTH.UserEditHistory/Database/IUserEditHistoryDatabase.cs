namespace OpenFTTH.UserEditHistory.Database;

internal interface IUserEditHistoryDatabase
{
    void InitSchema();
    void Upsert(IReadOnlyCollection<UserEditHistory> userEditHistories);
    void BulkUpsert(IReadOnlyCollection<UserEditHistory> userEditHistories);
}
