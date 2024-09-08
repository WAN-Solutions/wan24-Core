using System.Linq.Expressions;

namespace wan24.Core
{
    /// <summary>
    /// Expression helper
    /// </summary>
    public static class ExpressionHelper
    {
        /// <summary>
        /// Expression compiler
        /// </summary>
        public static Func<LambdaExpression, bool, Delegate> Compiler { get; set; } = (expression, preferInterpretation) => expression.Compile(preferInterpretation);

        /// <summary>
        /// Compile an expression
        /// </summary>
        /// <typeparam name="T">Resulting delegate type</typeparam>
        /// <param name="expression">Expression</param>
        /// <param name="preferInterpretation">If to prefer an interpreted result</param>
        /// <returns>Delegate</returns>
        public static T CompileExt<T>(this Expression<T> expression, in bool preferInterpretation = false) where T : Delegate => (T)Compiler(expression, preferInterpretation);
    }
}
