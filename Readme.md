Jump-Location: A cd that learns
=====================

If you spend any time in a console you know that `cd` is by far the most
common command that you issue. I'll open up a console to it's default location
in `C:\Users\tkellogg` or `C:\Windows\system32` and then issue a `cd C:\work\MyProject`. 
`Jump-Location` is a cmdlet lets you issue a `j my` to jump 
directly to `C:\work\MyProject`. 

It learns your behavior by keeping track of how long you spend in certain
directories and favoring them over the ones you don't care about.  You don't 
have to use `Jump-Location` as a replacement for `cd`. Use `cd`	to go local, and 
use `Jump-Location` to jump further away.

`Jump-Location` is a powershell implementation of [autojump][1].


How it knows where you want to go
---------------------------------

It keeps track of [how long you stay in a directory][2] and builds a database.
When you use the `Jump-Location` or `j` command, it looks through the database
to find the most likely directory and jumps there. You should only need to
give it a 2-3 character hint. For instance, on mine I can do:

* `j de` -> `C:\Users\tkellogg\code\Jump-Location\bin\Debug`
* `j doc` -> `C:\Users\tkellogg\Documents`

What if you have several projects and you want to get to the Debug directory
of one that you don't use very often? If you're `jumpstat` looks like this:

    255    C:\Users\tkellogg\code\Jump-Location\bin\Debug
		50     C:\Users\tkellogg\code\MongoDB.FSharp\bin\Debug

Using `j de` will jump to `Jump-Location\bin\Debug`. But use something like
`j mo d` if you really want to go to `MongoDB.FSharp\bin\Debug`. You can 
issue a `j mo d`. `mo` matches `MongoDB.FSharp` and `d` matches `Debug`.


Quick Primer on `jumpstat`
--------------------------

You can use `jumpstat` to see what's in the database. In tradition with `autojump`,
the database is saved to `~\jump-location.txt`. You can open up that file and
make changes. The file will auto-load into any open powershell sessions.

Since we're in Powershell (and not legacy Bash) `jumpstat` returns _objects_. 
This means that you don't ever have to know anything about `~\jump-location.txt`.
You can manipulate the objects it returns. For instance, this is valid:

```
PS> $stats = jumpstat
PS> $stats[10].Weight = -1
PS> jumpstat -Save
```

Setting a weight to a negative number like that will cause it to be blacklisted
from all jump results. Use the `jumpstat -All` option to see negative weights
included in the jumpstat result set.


Why `Jump-Location`?
--------------------

In PowerShell, cmdlet names must have a `<Verb>-<Noun>` format. You can
alias them (like I did with the `j` command).
PowerShell also has [a list of approved verbs][3]. 
`Jump` is not on that list, but I chose it anyway because it's most 
descriptive of what you're actually trying to do to `Location`. It's also
most similar to "autojump".


Installation
------------
1. Download [latest release][5].
2. Open properties for zip file and click "Unblock" button if you have one.
3. Unzip 
4. Open a PowerShell console
5. Run `.\Install.ps1`. You may need to allow remote scripts by running 
`Set-ExecutionPolicy -RemoteSigned`. You may also have to right-click `Install.ps1`
and Unblock it from the properties window. 
**Alternative:**
Add line `Import-Module $modules\Jump-Location\Jump.Location.psd1` to your `$PROFILE`,
where `$modules\Jump-Location` is a path to folder with module.

Next time you open a PowerShell console Jump-Location will start learning 
your habits. You'll also have access to the `j` and `jumpstat` aliases.

If you find any bugs, please report them so I can fix them quickly!

Build from source
-----------------
From root directory:

1. Run `msbuild` .
2. Run `.\build.ps1`

In directory `Build` you will have local build of module.

TODO
----------
1. Local search. `j . blah` will only match dirs under cwd. Using `.` will also search outside the DB.
2. Better PS documentation

References
----------
1. [old releases][4].

 [1]: https://github.com/joelthelion/autojump
 [2]: http://stackoverflow.com/a/11813545/503826
 [3]: http://blogs.msdn.com/b/powershell/archive/2009/07/15/final-approved-verb-list-for-windows-powershell-2-0.aspx
 [4]: https://github.com/tkellogg/Jump-Location/downloads
 [5]: https://sourceforge.net/projects/jumplocation/files/latest/download

