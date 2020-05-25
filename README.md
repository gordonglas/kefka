# Kefka - Cross-platform file transform cli tool

Requires .NET Core 3.1 runtime.

## Convert line endings
* Supports large files.
* Supports UTF-8 input.
* Anything that starts with "TODO" below, has not been implemented yet.
<pre>
kefka --eol=lf [input-files] [-o output]

Options:
  --eol=TYPE
        Set to line ending type that you want to convert to.
        TYPE values:
            lf    line-feed
            TODO: crlf  carriage-return/line-feed
            TODO: cr    carriage-return
  [input-files]
        Optional space-delimited list of input files.
        TODO: Can use simple wildcard.
        TODO: If omitted, reads from STDIN.
  TODO: [--in-place]
        Modify the input-files directly.
        Ignores [-o output].
  [-o output]
        Output path or file.
        Path must end with slash.
        TODO: If omitted, [input-files] must be single file
        and sends output to STDOUT.
        TODO: If [input-files] is omitted,
        and this is specified, this is the output file.

Examples:
  kefka --eol=lf path/to/file.txt -o output/path/
</pre>
