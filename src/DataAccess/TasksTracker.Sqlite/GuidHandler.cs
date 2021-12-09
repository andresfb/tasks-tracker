using Dapper;

namespace TasksTracker.Sqlite;

public class GuidHandler : SqlMapper.TypeHandler<Guid>
{
    public override Guid Parse(object value)
        => new((string)value);

    public override void SetValue(System.Data.IDbDataParameter parameter, Guid value)
        => parameter.Value = value.ToString();
}