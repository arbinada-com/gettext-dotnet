#!bin/sh
## Extract messages to PO template file
mono ./../../Bin/Debug/GNU.Gettext.Xgettext.exe -j -d"./" --recursive -o"./po/Messages.pot"