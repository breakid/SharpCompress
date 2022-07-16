# SharpCompress
C# application for compressing files and directories

Unfortunately, due to the SharpZipLib dependency, I wasn't able to get this to compile directly with CSC, so Visual Studio is required. If anyone figures out how to compile with CSC, please let me know!

### Usage

    SharpCompress [/F] [/R] [/V] <input_filepath> [<input_filepath>] /O <output_filepath>

    /O  Used to specify path to output file; required
    /F  Forcibly overwrite output file, if it exists
    /R  Specifies that directories should be compressed recursively
    /V  Verbose; print the name of each file as it's compressed

One or more input filepaths can be provided; paths can be individual files or directories. By default, if a directory is provided, all files immediately in the specified directory will be compressed. Use the /r flag to process directories recursively.

As with my other utilities, flags are case insensitive and can be specified using '/' or '-' (e.g., '-v', '-F'), depending on your preference.

Currently, the entire directory structure will be replicated in the zip file. I'm considering adding a flag to "flatten" paths, but it isn't a priority for me at this time.