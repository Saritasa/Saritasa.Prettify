CodePrettify
============

The tool to prettify code using Roslyn. Following rules are implemented:

- SA1633, file must have header
- SA1200, SA1208, SA1201, SA1209, usings
- SA1027, tabs must not be used
- SA1028, remove trailing whitespaces

Note
----

1. Do not delete folder "SCAnalyzersLibraries".
1. Do not update any packages because we use old Roslyn libraries for compatibility with StyleCop Analyzers.

Install
-------

- https://s3-us-west-2.amazonaws.com/saritasa-code-prettify/release/publish.htm

Todo
----

1. Need to remove redunant libraries for Microsoft Analyzers.
