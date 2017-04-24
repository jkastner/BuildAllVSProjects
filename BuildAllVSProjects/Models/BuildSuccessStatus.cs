namespace BuildAllVSProjects.Models
{
    public enum BuildSuccessStatus
    {
        Undefined,
        NotAttempted,
        IsBuilding,
        FailedOnLatest,
        SucceededOnLatest,
        FailedOnPrevious,
        SucceededOnPrevious,
        Exception
    }
}