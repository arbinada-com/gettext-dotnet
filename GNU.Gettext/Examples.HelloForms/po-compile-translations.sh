#!bin/sh

# Compile PO files to satellite assemblies
mono ./../../Bin/Debug/GNU.Gettext.Msgfmt.exe -i./po/fr.po -lfr-FR -d./bin/Debug -bExamples.HelloForms.Messages -cmcs -L./../../Bin/Debug -v
mono ./../../Bin/Debug/GNU.Gettext.Msgfmt.exe -i./po/ru.po -lru-RU -d./bin/Debug -bExamples.HelloForms.Messages -cmcs -L./../../Bin/Debug -v --debug
