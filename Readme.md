Jump-Location: A cd that learns
=====================

If you spend any time in a console you know that `cd` is by far the most
common command that you issue. You'll open up a console to `C:\Users\me`
or `C:\Windows\system32` and then issue a `cd C:\work\Website`. 
`Jump-Location` is a cmdlet lets you issue a `j we` to jump 
directly to `C:\work\Website`. 

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

You can use `jumpstat` to see what's in the database. Right now, `jumpstat` 
just returns a wall of text, but in the future I'd like to follow the 
PowerShell paradigm and return a list of objects that you can edit.


Why `Jump-Location`?
--------------------

In PowerShell, cmdlet names must have a `<Verb>-<Noun>` format. You can
alias them (like I did with the `j` command).
PowerShell also has [a list of approved verbs][3]. 
`Jump` is not on that list, but I chose it anyway because it's most 
descriptive of what you're actually trying to do to `Location`. It's also
most similar to "autojump".


What still needs to be done
---------------------------

I'm trying to make this as close to [autojump][1] as possible. While its
still lacking a lot of the features that Autojump has, it still has enough
to be very useful. Of the things left to implement, most notable are:

* Tab completion
* Multiple arguments
* Fuzzy matching
* _[bug]_ Multiple consoles will interfere with each other


Installation
------------

1. Download [the zip file][4]
2. Unzip to `~\Desktop\Jump-Location`
3. Open a PowerShell console
4. Edit your profile: `notepad $PROFILE.CurrentUserAllHosts`
5. Add a line to execute the install script: `~\Desktop\Jump-Location\Install.ps1`

Next time you open a PowerShell console Jump-Location will start learning 
your habits. You'll also have access to the `j` and `jumpstat` aliases.

If you find any bugs, please report them so I can fix them quickly!


 [1]: https://github.com/joelthelion/autojump
 [2]: http://stackoverflow.com/a/11813545/503826
 [3]: http://blogs.msdn.com/b/powershell/archive/2009/07/15/final-approved-verb-list-for-windows-powershell-2-0.aspx
 [4]: https://github.com/tkellogg/Jump-Location/downloads


