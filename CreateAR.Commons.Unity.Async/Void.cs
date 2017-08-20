namespace CreateAR.Commons.Unity.Async
{
    /// <summary>
    /// Void type for use within the Async package.
    /// </summary>
    public sealed class Void
    {
        /// <summary>
        /// Static instance.
        /// </summary>
        public static Void Instance = new Void();

        /// <summary>
        /// Keep it private.
        /// </summary>
        private  Void()
        {
            
        }
    }
}