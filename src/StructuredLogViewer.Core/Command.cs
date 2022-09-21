using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Microsoft.Build.Logging.StructuredLogger
{
    public class Command : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public Command(Action execute)
            : this(execute, () => true)
        {
        }

        public Command(Action execute, Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object parameter) => canExecute();
        public void Execute(object parameter) => execute();
    }

    public class Command<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Func<T, bool> canExecute;

        public event EventHandler CanExecuteChanged;

        public Command(Action<T> execute)
        {
            this.execute = execute;
        }

        public Command(Action<T> execute, Func<T, bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <inheritdoc/>
        public bool CanExecute(object parameter)
        {
            if (parameter is null && default(T) is not null)
            {
                return false;
            }

            if (!TryGetCommandArgument(parameter, out T result))
            {
                ThrowArgumentExceptionForInvalidCommandArgument(parameter);
            }

            return CanExecute(result);
        }

        public bool CanExecute(T parameter)
        {
            return canExecute?.Invoke(parameter) != false;
        }


        public void Execute(T parameter)
        {
            this.execute(parameter);
        }

        public void Execute(object parameter)
        {
            if (!TryGetCommandArgument(parameter, out T result))
            {
                ThrowArgumentExceptionForInvalidCommandArgument(parameter);
            }

            Execute(result);
        }

        public void NotifyCanExecuteChanged() {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        internal static bool TryGetCommandArgument(object? parameter, out T? result)
        {
            // If the argument is null and the default value of T is also null, then the
            // argument is valid. T might be a reference type or a nullable value type.
            if (parameter is null && default(T) is null)
            {
                result = default;

                return true;
            }

            // Check if the argument is a T value, so either an instance of a type or a derived
            // type of T is a reference type, an interface implementation if T is an interface,
            // or a boxed value type in case T was a value type.
            if (parameter is T argument)
            {
                result = argument;

                return true;
            }

            result = default;

            return false;
        }

        internal static void ThrowArgumentExceptionForInvalidCommandArgument(object? parameter)
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            static Exception GetException(object? parameter)
            {
                if (parameter is null)
                {
                    return new ArgumentException($"Parameter \"{nameof(parameter)}\" (object) must not be null, as the command type requires an argument of type {typeof(T)}.", nameof(parameter));
                }

                return new ArgumentException($"Parameter \"{nameof(parameter)}\" (object) cannot be of type {parameter.GetType()}, as the command type requires an argument of type {typeof(T)}.", nameof(parameter));
            }

            throw GetException(parameter);
        }
    }
}
