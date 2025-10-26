// using Grpc.Core;
// using Shared;

// namespace CalculatorServer.Services
// {
//     public class LamportCalculatorService : Calculator.CalculatorBase
//     {
//         private readonly LamportClock _lamportClock;
//         private readonly Random _random;
//         private readonly string _serverId;

//         public LamportCalculatorService()
//         {
//             _serverId = $"LamportServer-{Environment.MachineName}-{DateTime.Now:HHmmss}";
//             _lamportClock = new LamportClock(_serverId);
//             _random = new Random();
//             Console.WriteLine($"Lamport Calculator service started with ID: {_serverId}");
//         }

//         public override async Task<CalculationResponse> Square(CalculationRequest request, ServerCallContext context)
//         {
//             Console.WriteLine($"\n🔢 Lamport Square operation for: {request.Number}");
            
//             // Extract received Lamport time (we'll use first clock value)
//             var receivedTime = request.VectorClock.Values.FirstOrDefault();
            
//             // Update our Lamport clock
//             _lamportClock.Update(receivedTime);
            
//             // Simulate processing delay
//             var delay = _random.Next(2000, 5000);
//             await Task.Delay(delay);

//             // Simulate errors
//             if (request.Number < 0 || _random.Next(1, 5) == 1)
//             {
//                 return new CalculationResponse
//                 {
//                     IsSuccess = false,
//                     ErrorMessage = "Lamport simulation error",
//                     VectorClock = { { _serverId, _lamportClock.GetTime() } }
//                 };
//             }

//             var result = request.Number * request.Number;
//             Console.WriteLine($"✅ Lamport Square result: {result}, Clock: {_lamportClock}");

//             return new CalculationResponse
//             {
//                 Result = result,
//                 IsSuccess = true,
//                 VectorClock = { { _serverId, _lamportClock.GetTime() } }
//             };
//         }

//         // Similar implementation for Cube method...
//     }
// }

//         public override async Task<CalculationResponse> Cube(CalculationRequest request, ServerCallContext context)
//         {
//             Console.WriteLine($"\n🔢 Lamport Cube operation for: {request.Number}");
            
//             // Extract received Lamport time (we'll use first clock value)
//             var receivedTime = request.VectorClock.Values.FirstOrDefault();
            
//             // Update our Lamport clock
//             _lamportClock.Update(receivedTime);
            
//             // Simulate processing delay
//             var delay = _random.Next(2000, 5000);
//             await Task.Delay(delay);

//             // Simulate errors
//             if (request.Number < 0 || _random.Next(1, 5) == 1)
//             {
//                 return new CalculationResponse
//                 {
//                     IsSuccess = false,
//                     ErrorMessage = "Lamport simulation error",
//                     VectorClock = { { _serverId, _lamportClock.GetTime() } }
//                 };
//             }

//             var result = request.Number * request.Number * request.Number;
//             Console.WriteLine($"✅ Lamport Cube result: {result}, Clock: {_lamportClock}");

//             return new CalculationResponse
//             {
//                 Result = result,
//                 IsSuccess = true,
//                 VectorClock = { { _serverId, _lamportClock.GetTime() } }
//             };
//         }


using Grpc.Core;
using Shared;

namespace CalculatorServer.Services
{
    public class LamportCalculatorService : Calculator.CalculatorBase
    {
        private readonly LamportClock _lamportClock;
        private readonly Random _random;
        private readonly string _serverId;

        public LamportCalculatorService()
        {
            _serverId = $"LamportServer-{Environment.MachineName}-{DateTime.Now:HHmmss}";
            _lamportClock = new LamportClock(_serverId);
            _random = new Random();
            Console.WriteLine($"Lamport Calculator service started with ID: {_serverId}");
        }

        public override async Task<CalculationResponse> Square(CalculationRequest request, ServerCallContext context)
        {
            Console.WriteLine($"\n🔢 Lamport Square operation for: {request.Number}");
            
            // Extract received Lamport time (we'll use first clock value)
            var receivedTime = request.VectorClock.Values.FirstOrDefault();
            
            // Update our Lamport clock
            _lamportClock.Update(receivedTime);
            
            // Simulate processing delay
            var delay = _random.Next(2000, 5000);
            Console.WriteLine($"⏱️  Processing delay: {delay}ms");
            await Task.Delay(delay);

            // Simulate errors
            if (request.Number < 0 || _random.Next(1, 5) == 1)
            {
                Console.WriteLine("❌ Error: Lamport simulation error");
                return new CalculationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Lamport simulation error",
                    VectorClock = { { _serverId, _lamportClock.GetTime() } }
                };
            }

            var result = request.Number * request.Number;
            Console.WriteLine($"✅ Lamport Square result: {result}, Clock: {_lamportClock}");

            return new CalculationResponse
            {
                Result = result,
                IsSuccess = true,
                VectorClock = { { _serverId, _lamportClock.GetTime() } }
            };
        }

        public override async Task<CalculationResponse> Cube(CalculationRequest request, ServerCallContext context)
        {
            Console.WriteLine($"\n🔢 Lamport Cube operation for: {request.Number}");
            
            // Extract received Lamport time
            var receivedTime = request.VectorClock.Values.FirstOrDefault();
            
            // Update our Lamport clock
            _lamportClock.Update(receivedTime);
            
            // Simulate processing delay
            var delay = _random.Next(2000, 5000);
            Console.WriteLine($"⏱️  Processing delay: {delay}ms");
            await Task.Delay(delay);

            // Simulate errors
            if (request.Number < 0 || _random.Next(1, 5) == 1)
            {
                Console.WriteLine("❌ Error: Lamport simulation error");
                return new CalculationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Lamport simulation error",
                    VectorClock = { { _serverId, _lamportClock.GetTime() } }
                };
            }

            var result = request.Number * request.Number * request.Number;
            Console.WriteLine($"✅ Lamport Cube result: {result}, Clock: {_lamportClock}");

            return new CalculationResponse
            {
                Result = result,
                IsSuccess = true,
                VectorClock = { { _serverId, _lamportClock.GetTime() } }
            };
        }

        public override async Task<CalculationResponse> SlowMultiply(MultiplyRequest request, ServerCallContext context)
        {
            Console.WriteLine($"\n🔢 Lamport SlowMultiply operation: {request.Number1} × {request.Number2}");
            
            // Extract received Lamport time
            var receivedTime = request.VectorClock.Values.FirstOrDefault();
            
            // Update our Lamport clock
            _lamportClock.Update(receivedTime);
            
            // Simulate 5 second processing
            Console.WriteLine("⏱️  SlowMultiply processing: 5000ms");
            await Task.Delay(5000);

            // Simulate errors
            if (request.Number1 < 0 || request.Number2 < 0 || _random.Next(1, 5) == 1)
            {
                Console.WriteLine("❌ Error: Lamport simulation error");
                return new CalculationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Lamport simulation error",
                    VectorClock = { { _serverId, _lamportClock.GetTime() } }
                };
            }

            var result = request.Number1 * request.Number2;
            Console.WriteLine($"✅ Lamport SlowMultiply result: {result}, Clock: {_lamportClock}");

            return new CalculationResponse
            {
                Result = result,
                IsSuccess = true,
                VectorClock = { { _serverId, _lamportClock.GetTime() } }
            };
        }
    }
}