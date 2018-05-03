# WildMouse.Unearth.PDFSniffer
A command line tool to analyse (lots of) PDF files. Build it yourself or just download and run PDFSniffer.exe. 

Wild Mouse's [Unearth](https://unearth.ai) cognitive search tool has to ingest lots (millions!) of PDF files.

This is an open source/iTextSharp version of a little tool we use to check the files our customers are giving us.

PDFSniffer -h will give a list of commands, but how it is normally used is like:

pdfsniffer -dir:c:\folder -csv:c:\results\folder.csv -i

This will scan all PDFs in c:\folder and it's subdirectories and output the results to c:\results\folder.csv (one line per .pdf).

The -i means perform a deep scan. This will look at every page and every image on every page and produce 2 extra csv files:
c:\results\folder_pageinf.csv and c:\results\folder_imginf.csv.

We then usually upload these files to a database or excel (for small pdf collections) so we can get a statistical view of the
'problem domain'.
