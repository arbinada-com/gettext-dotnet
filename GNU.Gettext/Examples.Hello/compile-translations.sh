#!bin/sh

# Compile PO files to satellite assemblies
mono ./../../Bin/GNU.Gettext.Msgfmt.exe -i./po/fr.po -lfr-FR -d./bin/Debug -bExamples.Hello.Messages -cmcs -L./../../Bin -v
mono ./../../Bin/GNU.Gettext.Msgfmt.exe -i./po/ru.po -lru-RU -d./bin/Debug -bExamples.Hello.Messages -cmcs -L./../../Bin -v
