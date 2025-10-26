namespace Shared
{
    public enum TransactionPhase
    {
        Prepare,
        Commit,
        Abort
    }

    public class TransactionRequest
    {
        public string TransactionId { get; set; } = "";
        public TransactionPhase Phase { get; set; }
        public string Operation { get; set; } = "";
        public double Number1 { get; set; }
        public double Number2 { get; set; }
        public Dictionary<string, int> VectorClock { get; set; } = new();
    }

    public class TransactionResponse
    {
        public string TransactionId { get; set; } = "";
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public double Result { get; set; }
        public Dictionary<string, int> VectorClock { get; set; } = new();
    }

    public class TwoPhaseCommitCoordinator
    {
        private readonly List<string> _participants;
        private readonly Dictionary<string, TransactionResponse> _prepareResponses = new();

        public TwoPhaseCommitCoordinator(List<string> participants)
        {
            _participants = participants;
        }

        public async Task<double> ExecuteDistributedTransaction(string operation, double num1, double num2 = 0)
        {
            var transactionId = Guid.NewGuid().ToString("N")[..8];
            Console.WriteLine($"🔁 Starting 2PC transaction: {transactionId}");

            try
            {
                // Phase 1: Prepare
                Console.WriteLine($"📋 Phase 1: PREPARE for transaction {transactionId}");
                var prepareSuccess = await PreparePhase(transactionId, operation, num1, num2);

                if (prepareSuccess)
                {
                    // Phase 2: Commit
                    Console.WriteLine($"✅ Phase 2: COMMIT for transaction {transactionId}");
                    return await CommitPhase(transactionId);
                }
                else
                {
                    // Phase 2: Abort
                    Console.WriteLine($"❌ Phase 2: ABORT for transaction {transactionId}");
                    await AbortPhase(transactionId);
                    throw new Exception("Transaction aborted due to prepare phase failure");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Transaction {transactionId} failed: {ex.Message}");
                await AbortPhase(transactionId);
                throw;
            }
        }

        private async Task<bool> PreparePhase(string transactionId, string operation, double num1, double num2)
        {
            _prepareResponses.Clear();
            var prepareTasks = new List<Task>();

            foreach (var participant in _participants)
            {
                prepareTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var response = await SendTransactionRequest(participant, new TransactionRequest
                        {
                            TransactionId = transactionId,
                            Phase = TransactionPhase.Prepare,
                            Operation = operation,
                            Number1 = num1,
                            Number2 = num2
                        });
                        
                        lock (_prepareResponses)
                        {
                            _prepareResponses[participant] = response;
                        }
                        
                        Console.WriteLine($"📤 {participant} PREPARE response: {(response.Success ? "YES" : "NO")}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ {participant} PREPARE failed: {ex.Message}");
                        lock (_prepareResponses)
                        {
                            _prepareResponses[participant] = new TransactionResponse { Success = false, Message = ex.Message };
                        }
                    }
                }));
            }

            await Task.WhenAll(prepareTasks);
            
            // Check if all participants voted YES
            var allSuccess = _prepareResponses.Values.All(r => r.Success);
            Console.WriteLine($"📊 Prepare phase result: {(allSuccess ? "ALL YES - CAN COMMIT" : "SOME NO - MUST ABORT")}");
            
            return allSuccess;
        }

        private async Task<double> CommitPhase(string transactionId)
        {
            var commitTasks = new List<Task>();
            var results = new Dictionary<string, double>();

            foreach (var participant in _participants)
            {
                commitTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var response = await SendTransactionRequest(participant, new TransactionRequest
                        {
                            TransactionId = transactionId,
                            Phase = TransactionPhase.Commit
                        });
                        
                        lock (results)
                        {
                            results[participant] = response.Result;
                        }
                        
                        Console.WriteLine($"✅ {participant} COMMITTED with result: {response.Result}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ {participant} COMMIT failed: {ex.Message}");
                    }
                }));
            }

            await Task.WhenAll(commitTasks);
            
            // Return combined result (for this example, we'll return the first result)
            return results.Values.FirstOrDefault();
        }

        private async Task AbortPhase(string transactionId)
        {
            var abortTasks = new List<Task>();

            foreach (var participant in _participants)
            {
                abortTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await SendTransactionRequest(participant, new TransactionRequest
                        {
                            TransactionId = transactionId,
                            Phase = TransactionPhase.Abort
                        });
                        
                        Console.WriteLine($"🔄 {participant} ABORTED transaction");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ {participant} ABORT failed: {ex.Message}");
                    }
                }));
            }

            await Task.WhenAll(abortTasks);
        }

        private async Task<TransactionResponse> SendTransactionRequest(string serverUrl, TransactionRequest request)
        {
            // This would normally use gRPC, but for simplicity we'll simulate
            await Task.Delay(100); // Simulate network delay
            
            // Simulate different responses based on server
            var random = new Random(serverUrl.GetHashCode());
            
            switch (request.Phase)
            {
                case TransactionPhase.Prepare:
                    // Simulate some failures in prepare phase
                    var success = random.NextDouble() > 0.2; // 80% success rate
                    return new TransactionResponse
                    {
                        TransactionId = request.TransactionId,
                        Success = success,
                        Message = success ? "Ready to commit" : "Cannot commit"
                    };
                
                case TransactionPhase.Commit:
                    // Calculate the actual result
                    double result = request.Operation.ToLower() switch
                    {
                        "square" => request.Number1 * request.Number1,
                        "cube" => request.Number1 * request.Number1 * request.Number1,
                        "multiply" => request.Number1 * request.Number2,
                        _ => 0
                    };
                    
                    return new TransactionResponse
                    {
                        TransactionId = request.TransactionId,
                        Success = true,
                        Result = result,
                        Message = "Committed successfully"
                    };
                
                case TransactionPhase.Abort:
                    return new TransactionResponse
                    {
                        TransactionId = request.TransactionId,
                        Success = true,
                        Message = "Aborted successfully"
                    };
                
                default:
                    throw new ArgumentException("Unknown phase");
            }
        }
    }
}