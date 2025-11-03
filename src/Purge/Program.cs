// See https://aka.ms/new-console-template for more information

if( args.Length != 2)
{
    Console.Error.WriteLine("Usage: Purge {VMS directory} {destination directory}");
    return 0;
}


return (Purge.Purge.PurgeDirectoryTree(args[0], args[1]) ? 1 : 0);




