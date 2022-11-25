using System.IO;

namespace FileSystemNS
{
    public static class FileStreamExt
    {
        public static int ReadByteAt(this FileStream fileStream, long position)
        {
            fileStream.Position = position;
            return fileStream.ReadByte();
        }

        public static void ReadAt(this FileStream fileStream, long position, byte[] bytes, int offset, int count)
        {
            fileStream.Position = position;
            fileStream.Read(bytes, offset, count);
        }

        public static void WriteByteAt(this FileStream fileStream, long position, byte b)
        {
            fileStream.Position = position;
            fileStream.WriteByte(b);
        }

        public static void WriteAt(this FileStream fileStream, long position, byte[] bytes, int offset, int count)
        {
            fileStream.Position = position;
            fileStream.Write(bytes, offset, count);
        }
    }
}
