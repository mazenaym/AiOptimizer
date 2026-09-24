//using PromptOptimizer.Application.Common.Interfaces;

//namespace PromptOptimizer.Infrastructure.Tokenization;

//public class TokenCounter : ITokenCounter
//{
//    public int CountTokens(string text, string? modelName = null)
//    {
//        if (string.IsNullOrWhiteSpace(text)) return 0;
        
//        // Approximate token count based on average word/subword length
//        var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
//        return (int)Math.Ceiling(words.Length * 1.3);
//    }
//}
