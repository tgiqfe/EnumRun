namespace EnumRun.Lib
{
    internal class ParentDirectory
    {
        public static void Create(string targetPath)
        {
            if (targetPath.Contains(Path.DirectorySeparatorChar))
            {
                string parent = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(parent))
                {
                    Directory.CreateDirectory(parent);
                }
            }
        }
    }
}
