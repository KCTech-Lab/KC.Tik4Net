namespace KC.Tik4Net.Api;

internal static class TagSequence
{
    private static volatile int _tagCounter;

    internal static int Next()
    {
        var tag = Interlocked.Increment(ref _tagCounter);

        return tag;
    }
}