#!bin/sh
## Extract messages to PO template file
mono ./../../Bin/Debug/GNU.Gettext.Xgettext.exe -d -j "./" --recursive -o "./po/Messages.pot"