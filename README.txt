CHUTOKI FREE  -  version 1.0.0
==============================
The fan that goes shhh.
Made by Radjouh Slim  -  slim.radjouh@gmail.com
Free software, no ads, MIT license (see LICENSE.txt).


WHY CHUTOKI?
------------
I made Chutoki because no app could make my PC truly quiet: the fan noise kept
waking up my son. So I built it myself, with the help of artificial intelligence.
If I could do it, so can you: go for it and build your own tools!
                                                                  - Slim


WHAT IT DOES
------------
Chutoki automatically adjusts your motherboard fan speeds:
quiet when your PC rests, stronger when it works.
 - Quiet  : shhh, baby's asleep (the quietest, default)
 - Auto   : handles everything (Quiet at rest, Normal while gaming)
 - Normal : good balance
 - Strong : maximum cooling

The baby tells you how things are:
 asleep (green) = all good  -  awake (orange) = getting warm
 crying (red) = very hot

Languages: English, French, Italian, Spanish, Chinese, Japanese
(picked automatically from Windows, can be changed in the ⚙ settings).


REQUIREMENTS
------------
 - Windows 10 or 11, 64-bit.
 - A desktop PC (laptop fans are managed by the manufacturer: the app then only
   shows temperatures).
 - The small free PawnIO driver, installed once from its official page:
   https://github.com/namazso/PawnIO.Setup/releases
   (the app offers the link if needed).


INSTALL
-------
1. Unzip the folder anywhere (for example C:\Program Files\Chutoki).
2. Run Chutoki.exe and accept the Windows prompt (administrator rights are
   needed to talk to the fans).
3. On first launch, tell the app how your CPU is cooled.
4. Tick "Start with Windows" if you want it to start automatically.

Keep all files together (the .exe, the .config and the .dll files).


SAFEGUARDS AND SAFETY
---------------------
 - Fast heating: if the temperature jumps, fans react immediately.
 - Warning zone: above about 82 °C (CPU) or 92 °C (graphics card), fans follow
   at least the Normal mode.
 - Quiet boost: if the PC stays warm for 2 minutes in Quiet mode, Chutoki
   switches to Normal until it cools down, then goes back to Quiet.
 - Emergency: if the CPU or graphics card gets too hot, all fans go to full
   speed at once.
 - If a sensor stops responding, fans go to full speed.
 - If a fan stops spinning, its minimum is raised and you get an alert.
 - Water-cooling pumps are never slowed down by default.
 - When you quit the app, your PC goes back to its usual (BIOS) settings.


UNINSTALL
---------
1. Untick "Start with Windows".
2. Right-click the icon near the clock > Quit.
3. Delete the app folder and the %APPDATA%\Chutoki folder.
(PawnIO is uninstalled separately from Windows "Installed apps".)


PRIVACY
-------
No personal data is collected. No ads. The app sends nothing over the Internet.
Your settings and reports stay on your PC, in %APPDATA%\Chutoki.


DISCLAIMER
----------
This software is provided "as is", without any warranty. It changes fan speeds:
you use it at your own risk. If in doubt, pause it (advanced settings) or quit
it: the BIOS takes over again.


CONTACT
-------
Question, bug, unsupported motherboard?
Write to slim.radjouh@gmail.com with your CPU, graphics card and motherboard
(shown in the advanced settings ⚙).
