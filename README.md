# Kefka - Cross-platform file transform cli tool

Requires .NET Core 6 runtime.

For help, run:
<pre>
kefka -h
</pre>

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
  --no-remove-bom
        Do not remove byte-order-mark if one exists.
        Default is to remove it.

  TODO: If both output-path and output-file are omitted,
    input must be a single source
    and output is sent to STDOUT.

Examples:
  kefka --eol=lf path/to/file1.js path/to/file2.js -op output/path
  kefka --eol=lf path/to/file.js -of output/file.js
</pre>


## Concat files
* Supports large files.
* Can use line-ending delimiters between files.
<pre>
Usage:
  kefka --concat [input-files] [-of output-file]
        [-d=DELIMITER_TYPE] [-dn=DELIMITER_NUMBER]
        [-e=EOF_EOL_TYPE]

Options:
  --concat
        Indicates file concatination.
  [input-files]
        Space-delimited list of input files.
  [-of output-file]
        Output file.
  [-d=DELIMITER_TYPE]
        Optional delimiter.
        If omitted, will not use a delimiter.
        DELIMITER_TYPE values:
            lf    line-feed
            crlf  carriage-return/line-feed
            cr    carriage-return
  [-dn=DELIMITER_NUMBER]
        Optional delimiter number.
        The number of times the delimiter will repeat.
        If omitted, will use 1.
  [-e=EOF_EOL_TYPE]
        Optional end-line at end of file.
        If omitted, no end-line at end of file.
        EOF_EOL_TYPE values:
            lf    line-feed
            crlf  carriage-return/line-feed
            cr    carriage-return

Example:
  kefka --concat path/to/file1.js path/to/file2.js
        -of output/file.js -d=lf -dn=2 -e=lf
</pre>

## Test args
<pre>
--eol=lf ../../../test-data/test.txt -op ../../../test-data/output/
--concat ../../../test-data/test1.txt ../../../test-data/test2.txt -of ../../../test-data/output/concat.txt -d=lf -dn=2 -e=lf
</pre>
