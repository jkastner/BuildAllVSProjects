
using System.ComponentModel;

namespace BuildAllVSProjects.Models
{
    public enum BuildSuccessStatus
    {
        Undefined,
        [Description("Not attempted")]
        NotAttempted,
        [Description("Building...")]
        IsBuilding,
        [Description("Build failed")]
        FailedOnLatest,
        [Description("Build succeeded")]
        SucceededOnLatest,
        [Description("Build previously failed")]
        FailedOnPrevious,
        [Description("Build previously succeeded")]
        SucceededOnPrevious,
        [Description("Build exception")]
        Exception

    }
}