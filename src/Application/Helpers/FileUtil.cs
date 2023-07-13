namespace Application.Helpers;

public static class FileUtil
{
    public static long ToByteFromMb(this long sizeInMb)
        => sizeInMb * 1024 * 1024;
}