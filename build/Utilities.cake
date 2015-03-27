public void Archive(FilePath archive, DirectoryPath toCompress)
{
    FilePath toolPath = "tools\\7-Zip.CommandLine\\tools\\7za.exe";

    var builder = new ProcessArgumentBuilder();	
    builder.Append("a");
    builder.Append("\"" + archive +"\"");
    builder.Append("\"" + toCompress +"\"");
    
    var settings = new ProcessSettings
    {
        Arguments = builder
    };
    
    if (StartProcess(toolPath.MakeAbsolute(Context.Environment), settings) != 0)
    {
        throw new Exception("Failed to transform file");
    }
}

public string JoinPath(string parent, string target)
{
    if(string.IsNullOrWhiteSpace(parent))
    {
        throw new ArgumentException("Parent path must contain a valid path.", "parent");
    }
    return System.IO.Path.Combine(parent, target);
}

public string JoinPath(string root, params string[] rest)
{
    var current = root;
    foreach(var path in rest)
        current =  System.IO.Path.Combine(current, path);
    return current;
}

// Takes a list of path options an takes the last one that isn't null, whitespace, and that exists.
public DirectoryPath GetPathFromOptions(params string[] options)
{
    string final = null;
    foreach(var path in options)
    {
        if(string.IsNullOrWhiteSpace(path))
            continue;
        final = path;
    }
    
    return string.IsNullOrWhiteSpace(final) ? (DirectoryPath) null : (DirectoryPath) final;
}

// Takes a list of path options an takes the last one that isn't null, whitespace, and that exists.
public DirectoryPath GetPathFromOptions(DirectoryPath root, params string[] options)
{
    var result = GetPathFromOptions(options);
    if(result == null)
        return null;
        
    if(result.IsRelative)
    {
        result = result.MakeAbsolute(root);
    }
    
    return Context.FileSystem.Exist(result) ? result : (DirectoryPath) null;
}

public void ResilientCleanDir(string directory)
{
    TryRepeat(3, () => CleanDirectory(directory));
}

public void ResilientCleanDirs(DirectoryPath[] directories)
{
    TryRepeat(3, () => CleanDirectories(directories));
}

public void ResilientDeleteDir(string directory)
{
    TryRepeat(3, () => DeleteDirectory(directory, true));
}

public void ResilientDeleteDir(DirectoryPath directory)
{
    TryRepeat(3, () => DeleteDirectory(directory, true));
}

public void TryRepeat(int timesToTry, Action toTry)
{
    for(int i = 0;;)
    {
        try {
            toTry();
            return;
        } catch(Exception) {
            i++;
            if(i >= timesToTry)
                throw;
        }
    }
}

public IEnumerable<FileInfo> FindInPath(string name, string directory = null)
{
    var searchPattern = name.Contains(".") ? name : name + ".*";

    var path = Environment.GetEnvironmentVariable("PATH");
    var directoryPaths = path.Split(';').ToList();
    if(directory != null)
    {
        directoryPaths.Add(directory);
    }

    foreach (var directoryPath in directoryPaths)
    {
        var dirInfo = new System.IO.DirectoryInfo(directoryPath);
        if(!dirInfo.Exists)
            continue;
        
        var files = dirInfo.EnumerateFiles(searchPattern, System.IO.SearchOption.AllDirectories);
        var enumerator = files.GetEnumerator();
        while(true)
        {
            try {
                if(!enumerator.MoveNext())
                {
                    break;
                }
            } catch(UnauthorizedAccessException ex) {
                goto stop;
            }
            
            var file = enumerator.Current;
            yield return file;
        }
    stop:
        continue;
    }
}

public bool ExistsInPath(string name)
{
    return FindInPath(name).Any();
}

public float GetTypeScriptVersion() 
{
    var regex = new System.Text.RegularExpressions.Regex(@"TypeScript\\(?<version>[0-9]\.[0-9])");
    var paths = FindInPath("tsc", "C:\\Program Files (x86)\\Microsoft SDKs\\TypeScript\\");
    float highestVersion = 0.0f;
    foreach(var path in paths)
    {
        if(!regex.IsMatch(path.FullName))
            continue;
        var matches = regex.Match(path.FullName);
        var versionString = matches.Groups["version"].ToString();
        var version = float.Parse(versionString);
        highestVersion = version > highestVersion ? version : highestVersion;
    }
    return highestVersion;
}