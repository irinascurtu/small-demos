// See https://aka.ms/new-console-template for more information
using Polly;
using Polly.CircuitBreaker;
using System.Net;

Console.WriteLine("Hello, World!");
// Circuit breaker with default options.
// See https://www.pollydocs.org/strategies/circuit-breaker#defaults for defaults.
//var optionsDefaults = new CircuitBreakerStrategyOptions();

//// Circuit breaker with customized options:
//// The circuit will break if more than 50% of actions result in handled exceptions,
//// within any 10-second sampling duration, and at least 8 actions are processed.
//var optionsComplex = new CircuitBreakerStrategyOptions
//{
//    FailureRatio = 0.5,
//    SamplingDuration = TimeSpan.FromSeconds(1),
//    MinimumThroughput = 8,
//    BreakDuration = TimeSpan.FromSeconds(30),
//    ShouldHandle = new PredicateBuilder().Handle<ArgumentNullException>()
//};

//// Circuit breaker using BreakDurationGenerator:
//// The break duration is dynamically determined based on the properties of BreakDurationGeneratorArguments.
//var optionsBreakDurationGenerator = new CircuitBreakerStrategyOptions
//{
//    FailureRatio = 0.5,
//    SamplingDuration = TimeSpan.FromSeconds(10),
//    MinimumThroughput = 8,
//    BreakDurationGenerator = static args => new ValueTask<TimeSpan>(TimeSpan.FromMinutes(args.FailureCount)),
//};

//// Handle specific failed results for HttpResponseMessage:
//var optionsShouldHandle = new CircuitBreakerStrategyOptions<HttpResponseMessage>
//{
//    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
//        .Handle<ArgumentNullException>()
//        .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError)
//};


//var optionsStateProvider = new CircuitBreakerStrategyOptions<HttpResponseMessage>
//{
//    StateProvider = stateProvider
//};

//var circuitState = stateProvider.CircuitState;
//Console.WriteLine(stateProvider.CircuitState);
///*
//CircuitState.Closed - Normal operation; actions are executed.
//CircuitState.Open - Circuit is open; actions are blocked.
//CircuitState.HalfOpen - Recovery state after break duration expires; actions are permitted.
//CircuitState.Isolated - Circuit is manually held open; actions are blocked.
//*/

// Manually control the Circuit Breaker state:
//var manualControl = new CircuitBreakerManualControl();
//var optionsManualControl = new CircuitBreakerStrategyOptions
//{
//    ManualControl = manualControl
//};

//// Manually isolate a circuit, e.g., to isolate a downstream service.
//await manualControl.IsolateAsync();

//// Manually close the circuit to allow actions to be executed again.
//await manualControl.CloseAsync();

//// Monitor the circuit state, useful for health reporting:
///

var stateProvider = new CircuitBreakerStateProvider();
var pipeline = new ResiliencePipelineBuilder()
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.1,
        SamplingDuration = TimeSpan.FromSeconds(10),
        MinimumThroughput = 3,
        BreakDuration = TimeSpan.FromSeconds(30),
        ShouldHandle = new PredicateBuilder().Handle<ArgumentNullException>(),
        StateProvider = stateProvider,
        //ManualControl = manualControl
        OnOpened = static args =>
        {
            Console.WriteLine("Circuit is opened");
            //args.Context.Properties.Set(SleepDurationKey, args.BreakDuration);
            return ValueTask.CompletedTask;
        },
    })
    .Build();

for (int i = 0; i < 10; i++)
{
    try
    {
        pipeline.Execute(() => throw new ArgumentNullException());
        Console.WriteLine(stateProvider.CircuitState);
        
    }
    catch (ArgumentNullException)
    {
        Console.WriteLine("Operation failed please try again.");
        Console.WriteLine(stateProvider.CircuitState);
    }
    catch (BrokenCircuitException)
    {
        Console.WriteLine(stateProvider.CircuitState);
        Console.WriteLine("Operation failed too many times please try again later.");
    }
}
