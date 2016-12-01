using System;
using System.Threading;
using System.Threading.Tasks;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Serilog;

namespace Mofichan.Backend
{
    /// <summary>
    /// A type of <see cref="IMofichanBackend"/> that allows interaction with Mofichan
    /// via a console.
    /// <para></para>
    /// This is mainly intended for testing purposes.
    /// </summary>
    public class ConsoleBackend : BaseBackend
    {
        private readonly CancellationTokenSource cancellationSource;

        private Task readFromConsoleTask;
        private IUser consoleUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleBackend"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public ConsoleBackend(ILogger logger) : base(logger.ForContext<ConsoleBackend>())
        {
            this.cancellationSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Initialises the backend.
        /// </summary>
        public override void Start()
        {
            Console.CancelKeyPress += (s, e) =>
            {
                this.Dispose();
                Environment.Exit(0);
            };

            Console.Write("\nConsole backend started. Please enter your name: ");

            this.consoleUser = new ConsoleUser(Console.ReadLine());

            Console.WriteLine("Hello {0}. Please enter messages for Mofichan below.\n",
                this.consoleUser.Name);

            Console.Write("> ");

            this.readFromConsoleTask = this.ReadFromConsoleAsync(this.cancellationSource.Token);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            this.cancellationSource.Cancel();

            try
            {
                this.readFromConsoleTask?.Wait();
            }
            catch (AggregateException e)
            {
                if (e.InnerException is TaskCanceledException)
                {
                    Logger.Verbose(e, "Expected task cancellation exception thrown");
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                this.cancellationSource.Dispose();
            }
        }

        /// <summary>
        /// Tries to retrieve a room by its ID.
        /// </summary>
        /// <param name="roomId">The room identifier.</param>
        /// <returns>
        /// The room corresponding to the ID, if it exists.
        /// </returns>
        protected override IRoom GetRoomById(string roomId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tries to retrieve a user by its ID.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        /// The user corresponding to the ID, if it exists.
        /// </returns>
        protected override IUser GetUserById(string userId)
        {
            throw new NotImplementedException();
        }

        private async Task ReadFromConsoleAsync(CancellationToken token)
        {
            while (true)
            {
                var readLineTask = Console.In.ReadLineAsync();

                while (!readLineTask.IsCompleted)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(100);
                }

                var message = await readLineTask;

                if (string.IsNullOrWhiteSpace(message))
                {
                    Console.Write("> ");
                    continue;
                }

                var context = new MessageContext(
                    from: this.consoleUser,
                    to: null,
                    body: message);

                this.OnReceiveMessage(new IncomingMessage(context));
            }
        }

        private class ConsoleUser : User
        {
            public ConsoleUser(string name)
            {
                this.Name = name;
            }

            public override string Name { get; }

            public override UserType Type
            {
                get
                {
                    return UserType.Adminstrator;
                }
            }

            public override string UserId
            {
                get
                {
                    return "ConsoleUser-" + this.Name;
                }
            }

            public override void ReceiveMessage(string message)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("< {1}", this.Name, message);
                Console.ResetColor();
                Console.Write("> ");
            }
        }
    }
}
