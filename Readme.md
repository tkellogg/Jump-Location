Jump-Location: A cd that learns
---------------------

If you spend any time in a console you know that `cd` is by far the most
common command that you issue. You'll open up a console to `C:\Users\me`
or `C:\Windows\system32` and then issue a `cd C:\work\Website`. 
`Jump-Location` is a cmdlet lets you issue a `Jump-Location we` to jump 
directly to `C:\work\Website`. 

It learns your behavior by keeping track of how long you spend in certain
directories and favoring them over the ones you don't care about. You don't 
have to 
use `Jump-Location` as a replacement for `cd`. Use `cd`	to go local, and 
use `Jump-Location` to jump further away.

`Jump-Location` is a powershell implementation of 
[autojump](https://github.com/joelthelion/autojump)

Known Issues
------------------

This is still an immature tool. I still haven't figured out some things:

* Start tracking from when the console is first started. Right now we don't
start tracking directories until you first call `Jump-Location`. We could
probably insert some script into the profile, but I'd rather avoid using an
installer if we can manage.
* Use as normal user. Right now you need to be running as admin in order
for the database to be persisted to the filesystem. I'm not real sure about
all the permissions settings and corner cases yet.
