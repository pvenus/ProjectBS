using NUnit.Framework;
using Stage;

public sealed class PortfolioOutcomeRouterTests
{
    [Test]
    public void DefaultRouterDispatchesTypedPayload()
    {
        int calls = 0;
        ChoiceExecutionContext context = new(applyPortfolioOutcome:
            (PortfolioOutcomeExecutionData _, out string error) => { calls++; error=""; return true; });
        Assert.That(Router().TryExecute("id", Config(), context, out _),
            Is.EqualTo(ChoiceExecutionResult.Success));
        Assert.That(calls, Is.EqualTo(1));
    }

    [Test]
    public void ReplayDoesNotExecuteAgain()
    {
        int calls=0; var router=Router();
        ChoiceExecutionContext context = new(applyPortfolioOutcome:
            (PortfolioOutcomeExecutionData _, out string error) => { calls++; error=""; return true; });
        router.TryExecute("id",Config(),context,out _);
        Assert.That(router.TryExecute("id",Config(),context,out _),Is.EqualTo(ChoiceExecutionResult.AlreadyExecuted));
        Assert.That(calls,Is.EqualTo(1));
    }

    [Test]
    public void FailedCallbackDoesNotEnterHistory()
    {
        int calls=0; var router=Router();
        ChoiceExecutionContext context = new(applyPortfolioOutcome:
            (PortfolioOutcomeExecutionData _, out string error) => { calls++; error="fault"; return calls>1; });
        Assert.That(router.TryExecute("id",Config(),context,out _),Is.EqualTo(ChoiceExecutionResult.ExecutionFailed));
        Assert.That(router.TryExecute("id",Config(),context,out _),Is.EqualTo(ChoiceExecutionResult.Success));
    }

    [Test]
    public void MissingRuntimeCallbackFailsClosed()
    {
        Assert.That(Router().TryExecute("id",Config(),new ChoiceExecutionContext(),out string error),
            Is.EqualTo(ChoiceExecutionResult.ExecutionFailed));
        Assert.That(error,Is.EqualTo("PORTFOLIO_OUTCOME_CONTEXT_MISSING"));
    }

    [Test]
    public void ExistingCompleteEventBehaviorIsUnchanged()
    {
        int calls=0; ChoiceExecutionContext context=new(completeEvent:()=>{calls++; return true;});
        ChoiceExecutionConfig config=ChoiceExecutionDataFactory.CreateConfig(ChoiceExecutionType.CompleteEvent);
        Assert.That(Router().TryExecute("legacy",config,context,out _),Is.EqualTo(ChoiceExecutionResult.Success));
        Assert.That(calls,Is.EqualTo(1));
    }

    [Test]
    public void InvalidIdentityNeverInvokesCallback()
    {
        int calls=0; ChoiceExecutionConfig config=Config();
        ((PortfolioOutcomeExecutionData)config.data).sourcePopupId += ".wrong";
        ChoiceExecutionContext context=new(applyPortfolioOutcome:
            (PortfolioOutcomeExecutionData _, out string error)=>{calls++;error="";return true;});
        Assert.That(Router().TryExecute("bad",config,context,out _),Is.EqualTo(ChoiceExecutionResult.InvalidConfig));
        Assert.That(calls,Is.Zero);
    }

    private static ChoiceExecutionRouter Router()=>ChoiceExecutionRouter.CreateDefault();
    private static ChoiceExecutionConfig Config()=>new()
    { executionType=ChoiceExecutionType.PortfolioOutcome, data=PortfolioOutcomeContractTests.ValidData() };
}
