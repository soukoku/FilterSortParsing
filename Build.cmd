@echo off
cls
dotnet clean Soukoku.FilterSortParsing -c Release
dotnet pack Soukoku.FilterSortParsing -c Release -o publish
pause