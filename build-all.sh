#!bin/sh
common_build_options=/target:Build /verbosity:quiet
xbuild "./GNU.Gettext/GNU.Gettext.sln" $common_build_options /property:configuration=Release 
xbuild "./GNU.Gettext/GNU.Gettext.sln" $common_build_options /property:configuration=Debug
