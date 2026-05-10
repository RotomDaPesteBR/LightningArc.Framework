namespace LightningArc.Results
{
    public partial class Error
    {
        /// <summary>
        /// Represents the base class for all error modules in the application.
        /// </summary>
        /// <remarks>
        /// Each error category (e.g., Application, Database, Business) should inherit from this class.
        /// It defines the basic structure for the composition of the error code through a prefix.
        /// </remarks>
        public abstract class ErrorModule
        {
            /// <summary>
            /// Gets the error category code prefix.
            /// </summary>
            /// <remarks>
            /// This value is used to categorize the error and is combined with a suffix to form the complete error code.
            /// For example, the 'Application' category might have a prefix of 1.
            /// </remarks>
            public const int CodePrefix = 0;

            /// <summary>
            /// Initializes a new instance of the <see cref="ErrorModule"/> class.
            /// </summary>
            /// <remarks>
            /// This constructor is protected to ensure that the class can only be instantiated through inheritance.
            /// </remarks>
            protected ErrorModule() { }
        }

        /// <summary>
        /// Represents a factory for creating errors for a specific module.
        /// </summary>
        /// <typeparam name="TModule">The type of the error module, which must inherit from <see cref="ErrorModule"/>.</typeparam>
        /// <remarks>
        /// This class is used to allow for the creation of extension methods that build
        /// specific errors for a module in a fluent manner (e.g., <c>Error.Custom.OrderRejected(...)</c>).
        /// </remarks>
        public class ErrorModule<TModule>
            where TModule : ErrorModule
        {
            internal ErrorModule() { }
        }

        /// <summary>
        /// Provides an entry point for creating custom module errors.
        /// </summary>
        /// <typeparam name="TModule">The type of the error module, which must inherit from <see cref="ErrorModule"/>.</typeparam>
        /// <returns>A new instance of <see cref="ErrorModule{TModule}"/>, which serves as an error factory for the specified module.</returns>
        /// <remarks>
        /// Use this method in conjunction with extension methods to create specific errors for a module,
        /// for example: <c>Error.Of&lt;Business&gt;().OrderRejected(...)</c>.
        /// </remarks>
        public static ErrorModule<TModule> Of<TModule>()
            where TModule : ErrorModule => new();
    }
}
