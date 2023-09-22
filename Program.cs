using System.Collections.Concurrent;
using System.Diagnostics;

if (args.Length == 0)
{
    Console.Error.WriteLine("No args provided. Terminating...");
    return;
}

if (!uint.TryParse(args[0], out uint fibIndex))
{
    Console.Error.WriteLine($"Failed to parse '{args[0]}' into a uint. Terminating...");
    return;
}

if (fibIndex > 93)
{
    Console.Error.WriteLine(
        "This program can't calculate any fibonacci numbers larger than the"
            + " 93rd, because a ulong isn't big enough. Sorry! Try a smaller one."
    );
    return;
}

Console.WriteLine($"Getting fibonacci number for an input of: {fibIndex}");

var fibOne = new Fibonacci();
var fibTwo = new Fibonacci();

var runOneTask = RunFib(fibOne, fibIndex);
var runTwoTask = RunFib(fibTwo, fibIndex);

var finishedRuns = await Task.WhenAll(runOneTask, runTwoTask);

(ulong value, TimeSpan runTime) fibOneResult = finishedRuns[0];
(ulong value, TimeSpan runTime) fibTwoResult = finishedRuns[1];

Console.WriteLine($"Run one got {fibOneResult.value:n0}, and completed in {fibOneResult.runTime}.");
Console.WriteLine($"Run two got {fibTwoResult.value:n0}, and completed in {fibTwoResult.runTime}.");

if (fibOneResult.runTime == fibTwoResult.runTime)
{
    Console.WriteLine("Both runs somehow managed to complete at exactly the same time. Wild.");
}
else if (fibOneResult.runTime < fibTwoResult.runTime)
{
    Console.WriteLine("Run one completed more quickly.");
}
else if (fibOneResult.runTime > fibTwoResult.runTime)
{
    Console.WriteLine("Run two completed more quickly.");
}

// Little wrapper so we can accurately get run time and determine which task finished first.
static async Task<(ulong value, TimeSpan runTime)> RunFib(Fibonacci fib, uint fibIndex)
{
    var stopwatch = new Stopwatch();
    stopwatch.Start();

    ulong fibValue = await fib.Fib(fibIndex);

    stopwatch.Stop();
    return (fibValue, stopwatch.Elapsed);
}

public class Fibonacci
{
    // Maintain a shared list of all discovered fibonacci numbers so far, we don't redo calculations we've already done
    // AKA the "dynamic programming" solution
    private static readonly ConcurrentDictionary<int, ulong> _discoveredNumbers =
        new(new[] { KeyValuePair.Create(0, 0ul), KeyValuePair.Create(1, 1ul), });

    private static readonly Random _random = Random.Shared;

    public async Task<ulong> Fib(uint fibIndex)
    {
        int msToWait = _random.Next(1000);
        await Task.Delay(msToWait);

        if (fibIndex == 0)
        {
            return 0;
        }
        if (fibIndex == 1)
        {
            return 1;
        }

        if (!_discoveredNumbers.TryGetValue((int)fibIndex - 2, out ulong nMinusTwo))
        {
            // Not found in the dictionary, need to calculate it
            nMinusTwo = await Fib(fibIndex - 2);
            _discoveredNumbers.TryAdd((int)fibIndex - 2, nMinusTwo);
        }

        if (!_discoveredNumbers.TryGetValue((int)fibIndex - 1, out ulong nMinusOne))
        {
            // Not found in the dictionary, need to calculate it
            nMinusOne = await Fib(fibIndex - 1);
            _discoveredNumbers.TryAdd((int)fibIndex - 1, nMinusOne);
        }

        ulong fibSum = nMinusOne + nMinusTwo;
        _discoveredNumbers.TryAdd((int)fibIndex, fibSum);

        return fibSum;
    }
}
