# GAMESS-Interface
Very simple but yet effective GUI to use in combination with the GAMESS (The General Atomic and Molecular Electronic Structure System), general ab initio quantum chemistry package.

 ![](https://raw.githubusercontent.com/Cipulot/GAMESS-Interface/master/gamess_img.png)

## Getting started
This GUI is meant for those who don't wanna spend time with long commands inputs to start their GAMESS jobs.
It is used to generate analysis files like bond geometry, lone pair electron dislocation and orbital energy distribution that can be displayed is softwares like Avogadro.

## Functionality
This software has 2 main functions based on the original GAMESS job process:
* *.inp file to *.log file
* *.dat file to *.wfn file

Other than that you can chose to run this application for example on a remote location machine and have the output files directly sent to you via email (file size limit of 25 MB).

 **New!**
Now GAMESS-INTERFACE will automatically detect if a valid GAMESS installation is present in your system. This eliminates the need to manually enter the version and install path.
## Solution structure
This application is developed using VS2019 and C#. There are 2 main forms, one for settings input and the main one that will be used by the user when running jobs.

## Known issues and notes
NOTE: in order to run this app you NEED to install the GAMESS package. The *.bat file that will run thre jobs have to be modified due to cmd stuff but you don't have to worry about that, the application will check for thew need to do this operation.

## References
Here are the main software used:
* [GAMESS Homepage](https://www.msg.chem.iastate.edu/gamess/)
* [GAMESS Download](https://www.msg.chem.iastate.edu/gamess/download.html)
* [Avogadro](https://avogadro.cc/)
