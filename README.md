# Kefka - Cross-platform file transform cli tool

Requires .NET Core 3.1 runtime.

## Convert line endings
* Supports large files.
* Supports UTF-8 input.
* Anything that starts with "TODO" below, has not been implemented yet.
<pre>
Usage:
  kefka --eol=lf [input-files] [-op output-path]
  kefka --eol=lf [input-file] [-of output-file]

Options:
  --eol=TYPE
        Set to line ending type that you want to convert to.
        Only supports UTF-8 input/output.
        TYPE values:
            lf    line-feed
            TODO: crlf  carriage-return/line-feed
            TODO: cr    carriage-return
  [input-files]
        Optional space-delimited list of input files.
        TODO: Can use simple wildcard.
        TODO: If omitted, reads from STDIN.
  [-op output-path]
        Output path. Same input filename will be used.
  [-of output-file]
        Output file. Must have single input source.

  TODO: If both output-path and output-file are omitted,
    input must be a single source
    and output is sent to STDOUT.

Examples:
  kefka --eol=lf path/to/file1.js path/to/file2.js -op output/path
  kefka --eol=lf path/to/file.js -of output/file.js
</pre>
