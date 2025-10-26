using Grpc.Core;
using Shared;

namespace CalculatorServer.Services;

public class CalculatorService : Calculator.CalculatorBase
{
    private readonly VectorClock _vectorClock;
    private readonly Random _random;
    private readonly string _serverId;

    public CalculatorService()
    {
        // _serverId = $"Server-{Environment.MachineName}-{DateTime.Now:HHmmss}";
        // _vectorClock = new VectorClock(_serverId);
        // _random = new Random(); 
        // Console.WriteLine($"Calculator service started with ID: {_serverId}");
 _serverId = $"Server-{Environment.MachineName}-{DateTime.Now:HHmmss}";
    _vectorClock = new VectorClock(_serverId);
    _random = new Random();

    //  Try to become leader if no active leader exists
    _ = Task.Run(async () =>
    {
        var currentLeader = await LeaderElection.GetCurrentLeader();
        if (currentLeader == null || !currentLeader.IsActive)
        {
            await LeaderElection.ElectLeader(_serverId, $"http://localhost:{GetPort()}");
        }
    });

    Console.WriteLine($"Calculator service started with ID: {_serverId}");

    }

    public override async Task<CalculationResponse> Square(CalculationRequest request, ServerCallContext context)
    {
        Console.WriteLine($"\n🔢 Square operation requested for: {request.Number}");
        
         if (IsServerPartitioned())
    {
        Console.WriteLine($"🚫 Server {_serverId} is partitioned - operation rejected");
        throw new RpcException(new Status(StatusCode.Unavailable, "Server is partitioned"));
    }

        // Save state for potential rollback
        _vectorClock.SaveState();
        
        // Merge incoming vector clock
        _vectorClock.Merge(request.VectorClock.ToDictionary(x => x.Key, x => x.Value));
        
        // Increment server clock
        _vectorClock.Increment();
        
        // Simulate processing delay
        var delay = _random.Next(2000, 5000);
        Console.WriteLine($"⏱️  Processing delay: {delay}ms");
        await Task.Delay(delay);


        // Check for simulated error
        if (SimulateError(request.Number))
        {
            _vectorClock.Rollback();
            return new CalculationResponse
            {
                IsSuccess = false,
                ErrorMessage = "Simulated server error occurred",
                VectorClock = { _vectorClock.GetClock() }
            };
        }

        var result = request.Number * request.Number;
        Console.WriteLine($"✅ Square result: {result}");

        return new CalculationResponse
        {
            Result = result,
            IsSuccess = true,
            VectorClock = { _vectorClock.GetClock() }
        };
    }

    public override async Task<CalculationResponse> Cube(CalculationRequest request, ServerCallContext context)
    {
        Console.WriteLine($"\n🔢 Cube operation requested for: {request.Number}");
        

 if (IsServerPartitioned())
    {
        Console.WriteLine($"🚫 Server {_serverId} is partitioned - operation rejected");
        throw new RpcException(new Status(StatusCode.Unavailable, "Server is partitioned"));
    }


        _vectorClock.SaveState();
        _vectorClock.Merge(request.VectorClock.ToDictionary(x => x.Key, x => x.Value));
        _vectorClock.Increment();
        
        var delay = _random.Next(2000, 5000);
        Console.WriteLine($"⏱️  Processing delay: {delay}ms");
        await Task.Delay(delay);

        if (SimulateError(request.Number))
        {
            _vectorClock.Rollback();
            return new CalculationResponse
            {
                IsSuccess = false,
                ErrorMessage = "Simulated server error occurred",
                VectorClock = { _vectorClock.GetClock() }
            };
        }

        var result = request.Number * request.Number * request.Number;
        Console.WriteLine($"✅ Cube result: {result}");

        return new CalculationResponse
        {
            Result = result,
            IsSuccess = true,
            VectorClock = { _vectorClock.GetClock() }
        };
    }

    public override async Task<CalculationResponse> SlowMultiply(MultiplyRequest request, ServerCallContext context)
    {
        Console.WriteLine($"\n🔢 SlowMultiply operation: {request.Number1} × {request.Number2}");
        
         if (IsServerPartitioned())
    {
        Console.WriteLine($"🚫 Server {_serverId} is partitioned - operation rejected");
        throw new RpcException(new Status(StatusCode.Unavailable, "Server is partitioned"));
    }


        _vectorClock.SaveState();
        _vectorClock.Merge(request.VectorClock.ToDictionary(x => x.Key, x => x.Value));
        _vectorClock.Increment();
        
        Console.WriteLine("⏱️  SlowMultiply processing: 5000ms");
        await Task.Delay(5000);

        if (SimulateError((request.Number1 + request.Number2) / 2))
        {
            _vectorClock.Rollback();
            return new CalculationResponse
            {
                IsSuccess = false,
                ErrorMessage = "Simulated server error occurred",
                VectorClock = { _vectorClock.GetClock() }
            };
        }

        var result = request.Number1 * request.Number2;
        Console.WriteLine($"✅ SlowMultiply result: {result}");

        return new CalculationResponse
        {
            Result = result,
            IsSuccess = true,
            VectorClock = { _vectorClock.GetClock() }
        };
    }

    private bool SimulateError(double number)
    {
        // Fail if number is negative
        if (number < 0)
        {
            Console.WriteLine("❌ Error: Negative number not supported");
            return true;
        }

        // Random failure (1 in 4 times)
        if (_random.Next(1, 5) == 1)
        {
            Console.WriteLine("❌ Error: Random failure simulated");
            return true;
        }

        return false;
    }

   
private bool IsServerPartitioned()
{
    return NetworkPartition.IsPartitioned(_serverId);
}


private int GetPort()
{
    // Extract port from server configuration
    return 5000;
}


}