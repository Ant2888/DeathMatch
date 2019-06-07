# RoR2 - DeathMatch

This is the source code for the RoR2 mod: DeathMatch. 

To contribute you can reach me on discord (ant2888#2639) or just send a pull request!

## Building

To build this follow the beginners tutorial found [here](https://github.com/risk-of-thunder/R2Wiki/wiki/Baby's-First-Mod). 
I will not be including the libs/ folder in this repository so simply grab them from the link above.

Note: This mod requires BepInEx and the R2API (as a placeholder for now). I recommend setting up postbuild/prebuild scripts in VS to build those projects first and place the binaries in libs/

I also kept in my postbuild to copy and paste the binary after it is built into my BepInEx/plugins/DeathMatch folder, feel free to modify it to do the same.
