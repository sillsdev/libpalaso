The original source for the files in this directory and subdirectory
were taken from https://github.com/markedup-mobi/file-lock.
They are available under the Apache V2 license: https://github.com/markedup-mobi/file-lock/blob/master/LICENSE

As of the time of adding them (Nov 2014), the only modifications were
-change the namespaces
-check if the process is still running (so crashes don't get us stuck)
-make timeout optional


FileLockContent.cs
IFileLock.cs
SimpleFileLock.cs
FileSys/LockIO.cs