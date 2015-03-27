# Cake.Streaming
A Gulp-like stream addin for Cake.

# Using It
Cake.Streaming functions very similair to gulp. It accepts a file group, and then "pipes" the files through the .Pipe methods. `Destination` also functions similar to to `.pipe(dest('dist'))`, in that it writes all the piped files out to a destination directory using `Contents` as the source stream and `Name` as the destination file name.
```csharp
Task("A")
  .Does(() =>
{
    Source("./src/**/*.js")
        .Pipe(MyJsProcessor())
        .Destination("dist");
});
```

# Creating a Processor
Processors are simply `Action`s or `Func`s that accept a `PipeFile` and, optionally, return a `PipeFile`.
Example:
```csharp
public static Func<PipeFile, PipeFile> AppendText(string text)
{
    return file => {
        var reader = file.ToReader();
        var memoryStream = new MemoryStream();
        var writer = new StreamWriter(memoryStream, new UTF8Encoding(true, false), 1024, false);
        writer.Write(text);
        writer.Write(reader.ReadToEnd());
        file.Contents = memoryStream;
        return file;
    };
}
```

Usage:
```csharp
Task("A")
  .Does(() =>
{
    Source("files/*.text")
        .Pipe(AppendText("Lorem ipsum"))
        .Destination("files");
});
```

If you want to create a Cake Addin using streaming, simply reference `Cake.Streaming.Core`. Both `PipeFile` and `ICakePipe` live in this assembly.

Note: Pipe methods make also be asynchronous.

# What's a PipeFile?
Inspired vaguely by [Vinyl](https://github.com/wearefractal/vinyl), a PipeFile is simple a wrapper over a stream that stores some basic metadata about the files being passed through the pipes. It has a `Contents` property which returns a `Stream`. PipeFile's contents can either be a `MemoryStream`, analogous roughly to buffers, or a `FileStream`. You can directly manipulate or replace a PipeFile's contents. Manipluting a files contents that `IsFileStream == true` with manipulate the file on disk. You can convert a PipeFile with a backing FileStream to a MemoryStream by calling `ToBuffer`.
