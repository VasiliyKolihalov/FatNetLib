namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public enum StepType
    {
        MoveStepAfter,
        MoveStepBefore,
        PutController,
        PutDependency,
        PutModule,
        PutScript,
        PutSendingMiddleware,
        PutReceivingMiddleware,
        SortMiddlewares,
        RemoveModule,
        RemoveStep,
        ReplaceOldStep
    }
}
